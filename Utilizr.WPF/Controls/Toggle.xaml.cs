using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Utilizr.WPF.Extension;

namespace Utilizr.WPF
{
    public partial class Toggle : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public delegate Task<bool> PreToggledCheckDelegate(bool newStateToAllow);

        public event EventHandler SwitchedOn;
        public event EventHandler SwitchedOff;

        public static readonly DependencyProperty BackgroundOnProperty =
            DependencyProperty.Register(
                nameof(BackgroundOn),
                typeof(SolidColorBrush),
                typeof(Toggle),
                new PropertyMetadata(new SolidColorBrush(Colors.Green))
            );

        public SolidColorBrush BackgroundOn
        {
            get { return (SolidColorBrush)GetValue(BackgroundOnProperty); }
            set { SetValue(BackgroundOnProperty, value.Clone()); }
        }


        public static readonly DependencyProperty BackgroundOffProperty =
            DependencyProperty.Register(
                nameof(BackgroundOff),
                typeof(SolidColorBrush),
                typeof(Toggle),
                new PropertyMetadata(new SolidColorBrush(Colors.Red))
            );

        public SolidColorBrush BackgroundOff
        {
            get { return (SolidColorBrush)GetValue(BackgroundOffProperty); }
            set { SetValue(BackgroundOffProperty, value.Clone()); }
        }


        public static readonly DependencyProperty BackgroundHandleProperty =
            DependencyProperty.Register(
                nameof(BackgroundHandle),
                typeof(Brush),
                typeof(Toggle),
                new PropertyMetadata(new SolidColorBrush(Colors.White))
            );

        public Brush BackgroundHandle
        {
            get { return (Brush)GetValue(BackgroundHandleProperty); }
            set { SetValue(BackgroundHandleProperty, value); }
        }


        public static readonly DependencyProperty IsToggledProperty =
            DependencyProperty.Register(
                nameof(IsToggled),
                typeof(bool),
                typeof(Toggle),
                new PropertyMetadata(default(bool), OnIsToggledChanged)
            );

        private static void OnIsToggledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Toggle toggle)
                return;

            bool newIsToggledValue = (bool)e.NewValue;
            toggle.SetToggleMargin(newIsToggledValue);
            toggle.StartStoryboard(newIsToggledValue);
        }

        public bool IsToggled
        {
            get { return (bool)GetValue(IsToggledProperty); }
            set
            {
                SetToggleMargin(value);
                SetValue(IsToggledProperty, value);
                OnPropertyChanged(nameof(IsToggled));
            }
        }

        public static readonly DependencyProperty ToggleMarginProperty =
            DependencyProperty.Register(
                nameof(ToggleMargin),
                typeof(Thickness),
                typeof(Toggle),
                new PropertyMetadata(
                    default(Thickness),
                    OnToggleMarginChanged
                )
            );

        private static void OnToggleMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as Toggle)?.AnimateToggle();
        }

        public Thickness ToggleMargin
        {
            get { return (Thickness)GetValue(ToggleMarginProperty); }
            set
            {
                SetValue(ToggleMarginProperty, value);
                OnPropertyChanged(nameof(ToggleMargin));
                AnimateToggle();
            }
        }


        public static readonly DependencyProperty PreToggledProperty =
            DependencyProperty.Register(
                nameof(PreToggled),
                typeof(PreToggledCheckDelegate),
                typeof(Toggle),
                new PropertyMetadata(null)
            );

        /// <summary>
        /// Logic which will be invoked just before the toggle is switched.
        /// Return true to allow toggle position to be updated, false to deny.
        /// If null, no check is preformed, and toggle will always updated.
        /// </summary>
        public PreToggledCheckDelegate PreToggled
        {
            get { return (PreToggledCheckDelegate)GetValue(PreToggledProperty); }
            set { SetValue(PreToggledProperty, value); }
        }


        readonly Storyboard? _onStoryBoard;
        readonly Storyboard? _offStoryBoard;
        private Storyboard _currentBackgroundStoryboard;

        public Toggle()
        {
            InitializeComponent();

#if DEBUG
            if (DesignerProperties.GetIsInDesignMode(this))
                return;
#endif

            _onStoryBoard = LayoutRoot.TryFindResource("OnStoryBoard") as Storyboard;
            _offStoryBoard = LayoutRoot.TryFindResource("OffStoryBoard") as Storyboard;
            LayoutRoot.DataContext = this;
            SizeChanged += (sender, args) =>
            {
                SetToggleMargin(IsToggled);
                ToggleEllipse.Margin = ToggleMargin;
            };
            Loaded += (s, e) =>
            {
                Border.Background = IsToggled
                    ? BackgroundOn.Clone()
                    : BackgroundOff.Clone();
            };
        }

        void SetToggleMargin(bool toggled)
        {
            ToggleMargin = new Thickness(
                toggled
                    ? ActualWidth - ToggleEllipse.ActualWidth - 4
                    : 4,
                0,
                0,
                0
            );
        }

        void AnimateToggle()
        {
            if (!IsLoaded)
            {
                // don't animate for inital gui setup
                ToggleEllipse.Margin = ToggleMargin;
            }
            else
            {
                var ta = new ThicknessAnimation()
                {
                    To = ToggleMargin,
                    Duration = TimeSpan.FromMilliseconds(200),
                    DecelerationRatio = 0.8,
                    FillBehavior = FillBehavior.Stop,
                };

                // Use FillBehaviour.Stop so we can manually update ToggleEllipse.Margin to
                // another value, e.g. when unloaded the assignment above will not work. But
                // this means the property won't be under the animation anymore and will revert
                // to the previous value, set corectly in the complete event.
                // https://learn.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/how-to-set-a-property-after-animating-it-with-a-storyboard
                void complete(object sender, EventArgs args)
                {
                    ToggleEllipse.Margin = ToggleMargin;
                    ta.Completed -= complete;
                }
                ta.Completed += complete;

                ToggleEllipse.BeginAnimation(Ellipse.MarginProperty, ta);
            }
        }

        protected async override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            var newToggleValue = !IsToggled;
            if (PreToggled != null)
            {
                bool canToggle = await PreToggled.Invoke(newToggleValue).ConfigureAwait(true);
                if (canToggle)
                {
                    ToggleState(newToggleValue);
                }
                return; // Not allowed to update toggle position
            }
            else
            {
                ToggleState(newToggleValue);
            }
        }

        void ToggleState(bool newToggleValue)
        {
            SetCurrentValue(IsToggledProperty, newToggleValue);

            StartStoryboard(newToggleValue);

            if (newToggleValue)
                OnSwitchedOn();
            else
                OnSwitchedOff();
        }

        private void StartStoryboard(bool newToggledState)
        {
            try
            {
                var startBoard = newToggledState ? _onStoryBoard : _offStoryBoard;
                startBoard.Begin(this, HandoffBehavior.SnapshotAndReplace, true);
            }
            catch (Exception)
            {
                Border.Background = newToggledState
                    ? BackgroundOn.Clone()
                    : BackgroundOff.Clone();
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnSwitchedOff()
        {
            SwitchedOff?.Invoke(this, new EventArgs());
        }

        protected virtual void OnSwitchedOn()
        {
            SwitchedOn?.Invoke(this, new EventArgs());
        }
    }
}
