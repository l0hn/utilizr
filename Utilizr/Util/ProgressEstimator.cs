using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using System.Text;
using System.Threading.Tasks;

namespace Utilizr.Util
{
    public class ProgressEstimator : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? PercentChanged;
        public event EventHandler? PauseResumeChanged;

        private double _percent;
        /// <summary>
        /// Current progress percentage between 0 and 1.
        /// </summary>
        public double Percent
        {
            get { return _percent; }
            set
            {
                _percent = value;
                OnPropertyChanged(nameof(Percent));
                OnPercentChanged();
            }
        }

        public double PercentCalculationFrequencyMs
        {
            get { return _timer.Interval; }
            set { _timer.Interval = value; }
        }

        private bool _isPaused;
        public virtual bool IsPaused
        {
            get { return _isPaused; }
            protected set
            {
                _isPaused = value;
                OnPropertyChanged(nameof(IsPaused));
                OnPauseResumeChanged();
            }
        }

        protected int CurrentSegmentIndex { get; private set; }
        protected ReadOnlyCollection<ProgressEstimatorSegment> Segments => _segments.AsReadOnly();
        protected double TotalEstimatedTime => _segments.Sum(p => p.ExpectedMs);

        private DateTime _currentSegmentStarted;
        private DateTime _pauseStarted;
        private readonly List<ProgressEstimatorSegment> _segments;
        private readonly object LOCK = new();
        private readonly Timer _timer;

        public ProgressEstimator()
        {
            _segments = new List<ProgressEstimatorSegment>();
            _timer = new Timer();
            _timer.Interval = 500;
            _timer.Elapsed += (s, e) => Tick();
        }

        public ProgressEstimator(ProgressEstimatorSegment segment)
            : this()
        {
            AddSegments(segment);
        }

        public ProgressEstimator(IEnumerable<ProgressEstimatorSegment> segments)
            : this()
        {
            AddSegments(segments.ToArray());
        }

        public ProgressEstimator(params ProgressEstimatorSegment[] segments)
            : this()
        {
            AddSegments(segments);
        }

        public void ClearSegments()
        {
            lock (LOCK)
            {
                if (_timer.Enabled)
                    throw new InvalidOperationException("Cannot clear segment collection whilst estimation is in progress");

                _segments.Clear();
            }
        }

        public void AddSegments(params ProgressEstimatorSegment[] segments)
        {
            lock (LOCK)
            {
                if (_timer.Enabled)
                    throw new InvalidOperationException("Cannot add segments to collection whilst estimation is in progress");

                _segments.AddRange(segments);
            }
        }

        public void AddSegments(IEnumerable<ProgressEstimatorSegment> segments)
        {
            lock (LOCK)
            {
                if (_timer.Enabled)
                    throw new InvalidOperationException("Cannot add segments to collection whilst estimation is in progress");

                _segments.AddRange(segments);
            }
        }

        public void Start()
        {
            lock (LOCK)
            {
                if (_timer.Enabled)
                    return;

                CurrentSegmentIndex = 0;
                _currentSegmentStarted = DateTime.UtcNow;
                Percent = 0;
                _timer.Start();
            }
        }

        public void Pause()
        {
            // Since percentage is calculated from time elapsed, take note of
            // how long estimator is paused. We can then offset the start time
            // so we have the same percentage when resuming the estimator.
            lock (LOCK)
            {
                if (!_timer.Enabled)
                    return;

                _pauseStarted = DateTime.UtcNow;
                _timer.Stop();
                IsPaused = true;
            }
        }

        public void Resume()
        {
            lock (LOCK)
            {
                if (_timer.Enabled || !IsPaused)
                    return;

                var pausedTime = DateTime.UtcNow - _pauseStarted;
                _currentSegmentStarted = _currentSegmentStarted.Add(pausedTime);
                _timer.Start();
                IsPaused = false;
            }
        }

        public void SegmentDone()
        {
            CurrentSegmentIndex++;
            _currentSegmentStarted = DateTime.UtcNow;
        }

        /// <summary>
        /// Whether <see cref="Percent"/> should be set to '1.0'.
        /// </summary>
        /// <param name="updatePercentValue"></param>
        public virtual void Finished(bool updatePercentValue = true)
        {
            lock (LOCK)
            {
                _timer.Stop();
                IsPaused = false;

                if (updatePercentValue)
                    Percent = 1;
            }
        }

        public bool IsRunning => _timer.Enabled || IsPaused;

        void Tick()
        {
            double totalTime = TotalEstimatedTime; // cache
            double completedPercent = 0;

            // Calculate percentage for work already done.
            if (CurrentSegmentIndex > 0)
            {
                double doneSegmentsTime = _segments.Take(CurrentSegmentIndex)
                                                   .Sum(p => p.ExpectedMs);

                completedPercent = doneSegmentsTime / totalTime;
            }


            // Calculate the percentage for the current segment
            if (completedPercent < 1 && CurrentSegmentIndex < _segments.Count)
            {
                double takenMs = (DateTime.UtcNow - _currentSegmentStarted).TotalMilliseconds;
                double expectedMs = _segments[CurrentSegmentIndex].ExpectedMs;
                double percentWhenComplete = expectedMs / totalTime;

                // Use a function that increases from 0 to 1 where x increases from 0 to infinity.
                // This value can be used against the 'work complete' percentage of the segment.
                // Using y = x / (x + a) http://math.stackexchange.com/a/869157/78626
                // x = time taken
                // a = expected time
                // When x is equal to a, y = 0.5. Thus when reaching the estimated time (x == a),
                // only 50% of the segments total percentage will be included.
                // If a = expected / 4, when expected time = taken time, y = approx 0.8
                double y = takenMs / (takenMs + (expectedMs / 4));
                double segmentPercent = percentWhenComplete * y;
                completedPercent += segmentPercent;
            }

            double roundedDown = Math.Floor(completedPercent * 100);

            // Possible that Stop() called but Elapsed event raised afterwards or already running on invocation.
            // Only set if the timer is still enabled.
            lock (LOCK)
            {
                if (_timer.Enabled)
                    Percent = roundedDown / 100;
            }
        }

        protected virtual void OnPercentChanged()
        {
            PercentChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnPauseResumeChanged()
        {
            PauseResumeChanged?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ProgressEstimatorSegment
    {
        public long ExpectedMs { get; set; }

        public ProgressEstimatorSegment()
        {

        }

        public ProgressEstimatorSegment(long expectedMs)
        {
            ExpectedMs = expectedMs;
        }
    }
}
