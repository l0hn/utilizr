using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Utilizr.WPF.Shapes
{
    public class PercentageArc: Shape
    {
        private double _percent = 0;
        private double _angleDegrees = 0;
        private double _angleRads = 0;
        private double _cosAngle = 0;
        private double _sinAngle = 0;
        private double _startAngleDegrees = 0;
        private double _startAngleRads = 0;
        private double _startSinAngle = 0;
        private double _startCosAngle = 0;
        const double PI_180 = Math.PI/180;


        public static readonly DependencyProperty PercentProperty =
            DependencyProperty.Register(
                nameof(Percent),
                typeof(double),
                typeof(PercentageArc),
                new FrameworkPropertyMetadata(
                    default(double),
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    PercentChanged
                )
            );

        static void PercentChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is not PercentageArc self)
                return;

            self._percent = (double) e.NewValue;

            if (self._percent > 1)
                return;

            // don't bother recalculating if the same to 2 decimal places
            // but make sure we paint for 100%, fixes issue where small gap sometimes left
            var newRounded = Math.Round((double)e.NewValue, 2);
            if (newRounded == Math.Round((double)e.OldValue, 2) && newRounded < 1)
                return;

            self.UpdateAngles();
        }

        void UpdateAngles()
        {
            if (_percent == 1)
                _angleDegrees = 359.999;
            else
                _angleDegrees = _percent * _degreesOfMotion;

            _angleRads = _angleDegrees * PI_180;
            _sinAngle = Math.Sin(_angleRads);
            _cosAngle = Math.Cos(_angleRads);
        }

        public double Percent
        {
            get { return (double) GetValue(PercentProperty); }
            set { SetValue(PercentProperty, value); }
        }


        public static readonly DependencyProperty StartAngleDegreesProperty = 
            DependencyProperty.Register(
                nameof(StartAngleDegrees),
                typeof(double),
                typeof(PercentageArc),
                new FrameworkPropertyMetadata(
                    default(double),
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    StartAngleChanged
                )
            );

        static void StartAngleChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is not PercentageArc self)
                return;

            //don't bother recalculating if the same to 3 decimal places
            if (Math.Round((double)e.NewValue, 0) == Math.Round((double)e.OldValue, 0))
                return;

            self._startAngleDegrees = self.StartAngleDegrees;
            self._startAngleRads = self._startAngleDegrees * PI_180;
            self._startSinAngle = Math.Sin(self._startAngleRads);
            self._startCosAngle = Math.Cos(self._startAngleRads);
        }

        public double StartAngleDegrees
        {
            get { return (double) GetValue(StartAngleDegreesProperty); }
            set { SetValue(StartAngleDegreesProperty, value); }
        }

        public static readonly DependencyProperty DegreesOfMotionProperty = 
            DependencyProperty.Register(
                nameof(DegreesOfMotion),
                typeof(double),
                typeof(PercentageArc),
                new FrameworkPropertyMetadata(
                    360.0,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    DegreesOfMotionChanged
                )
            );

        static void DegreesOfMotionChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is not PercentageArc self)
                return;

            self._degreesOfMotion = self.DegreesOfMotion;
            self.UpdateAngles();
        }

        public double DegreesOfMotion
        {
            get { return (double) GetValue(DegreesOfMotionProperty); }
            set { SetValue(DegreesOfMotionProperty, value); }
        }

        public static readonly DependencyProperty DirectionProperty =
            DependencyProperty.Register(
                nameof(Direction),
                typeof(SweepDirection),
                typeof(PercentageArc),
                new FrameworkPropertyMetadata(
                    SweepDirection.Clockwise,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnDirectionChanged
                )
            );

        private static void OnDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PercentageArc)?.InvalidateVisual();
        }

        public SweepDirection Direction
        {
            get { return (SweepDirection)GetValue(DirectionProperty); }
            set { SetValue(DirectionProperty, value); }
        }


        public PercentageArc()
        {
            Loaded += (sender, args) =>
            {
                UpdateAngles();
                InvalidateVisual();
            };
        }

        static PercentageArc()
        {
            StrokeThicknessProperty.OverrideMetadata(
                typeof(PercentageArc),
                new FrameworkPropertyMetadata(
                    (double)StrokeThicknessProperty.DefaultMetadata.DefaultValue,
                    StrokeThicknessPropertyChanged
                )
            );
        }

        static void StrokeThicknessPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (dependencyObject is not PercentageArc self)
                return;

            self._strokeThickness = self.StrokeThickness;
        }

        /// <summary>
        /// The RenderedGeometry property returns the final rendered geometry
        /// </summary>
        public override Geometry RenderedGeometry
        {
            get
            {
                // RenderedGeometry = defining geometry
                return DefiningGeometry;
            }
        }

        /// <summary>
        /// Return the transformation applied to the geometry before rendering
        /// </summary>
        public override Transform GeometryTransform
        {
            get
            {
                return Transform.Identity;
            }
        }

        private StreamGeometry _geometry = new();
        private readonly StreamGeometry _blankGeometry = new();
        private StreamGeometryContext? _geometryContext;
        private double _actualWidth, _actualHeight, _strokeThickness;
        private double _degreesOfMotion = 360.0;
        private Size _size = new(0, 0);
        private Point _startPoint = new(0,0);
        private Point _endPoint = new(0,0);

        protected override Geometry DefiningGeometry
        {
            get
            {
                //don't bother if no size
                if (_actualWidth == 0 || _percent <= 0)
                {
                    return _blankGeometry;
                }

                //calculate half sizes
                var halfWidth = (_actualWidth) / 2;
                var halfHeight = (_actualHeight) / 2;
                var halfStroke = (_strokeThickness / 2);

                //start point always the same but factor stroke thickness
                if (_startAngleDegrees > 0)
                {
                    //transform to origin
                    var startTransX = halfStroke - halfWidth;
                    var startTransY = halfWidth - halfHeight;

                    //calculate rotation and translate back from origin
                    _startPoint.X = (startTransX * _startCosAngle) - (startTransY * _startSinAngle) + halfWidth;
                    _startPoint.Y = (startTransY * _startCosAngle) + (startTransX * _startSinAngle) + halfHeight;
                }
                else
                {
                    _startPoint.X = halfStroke;
                    _startPoint.Y = halfWidth;
                }

                //calculate endpoint
                //translate rotation point to origin
                var transX = _startPoint.X - halfWidth;
                var transY = _startPoint.Y - halfHeight;

                //calculate rotation and translate back from origin
                _endPoint.X = (transX * _cosAngle) - (transY * _sinAngle) + halfWidth;
                _endPoint.Y = Direction == SweepDirection.Clockwise
                    ? (transY * _cosAngle) + (transX * _sinAngle) + halfHeight
                    : (transY * _cosAngle) - (transX * _sinAngle) + halfHeight;

                //factor stroke thickness into final arc size
                _size.Width = halfWidth - halfStroke;
                _size.Height = halfHeight - halfStroke;

                //create the geometry
                _geometry = new StreamGeometry();
                using (_geometryContext = _geometry.Open())
                {
                    _geometryContext.BeginFigure(_startPoint, false, false);
                    _geometryContext.ArcTo(_endPoint, _size, 1, _angleDegrees > 180, Direction, true, true);
                    //Debug.WriteLine($"[{Tag}] Start: ({_startPoint}), End: ({_endPoint.X:N3},{_endPoint.Y:N3}), Size: ({_actualWidth},{_actualHeight})");
                }

                return _geometry;
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {

            if (double.IsInfinity(constraint.Width) || double.IsInfinity(constraint.Height))
            {
                _actualHeight = 100;
                _actualWidth = 100;
            }
            else
            {
                _actualWidth = constraint.Width;
                _actualHeight = constraint.Height;
            }

            return new Size(_actualWidth, _actualHeight);
        }
    }
}