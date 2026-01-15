using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Utilizr.WPF.Extension;

namespace Utilizr.WPF.Controls
{
    public class ToggleAutomationPeer : FrameworkElementAutomationPeer, IToggleProvider
    {
        public ToggleAutomationPeer(Toggle owner)
            : base(owner)
        {
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
            => AutomationControlType.Button;

        protected override string GetClassNameCore()
            => "Toggle";

        public ToggleState ToggleState
            => ((Toggle)Owner).IsToggled ? ToggleState.On : ToggleState.Off;

        public void Toggle()
        { }
    }


    public partial class Toggle : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public delegate Task<bool> PreToggledCheckDelegate(bool newStateToAllow);

        public event EventHandler SwitchedOn;
        public event EventHandler SwitchedOff;


        public static readonly DependencyProperty HighlightBorderBrushProperty =
            DependencyProperty.Register(
                nameof(HighlightBorderBrush),
                typeof(Brush),
                typeof(Toggle),
                new PropertyMetadata(new SolidColorBrush("#FF2D7EFF".ARGBToColor()))
            );

        public Brush HighlightBorderBrush
        {
            get { return (Brush)GetValue(HighlightBorderBrushProperty); }
            set { SetValue(HighlightBorderBrushProperty, value); }
        }


        public static readonly DependencyProperty BackgroundOnProperty =
            DependencyProperty.Register(
                nameof(BackgroundOn),
                typeof(SolidColorBrush),
                typeof(Toggle),
                new PropertyMetadata(new SolidColorBrush("#FF0BC86D".ARGBToColor()), OnBackgroundOnChanged)
            );

        private static void OnBackgroundOnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Toggle toggle)
                return;

            toggle.SetupOnStoryboard();
            toggle.UpdateBorderBackgroundWithoutAnimation(toggle.IsToggled);
        }

        public SolidColorBrush BackgroundOn
        {
            get { return (SolidColorBrush)GetValue(BackgroundOnProperty); }
            set { SetValue(BackgroundOnProperty, value); }
        }


        public static readonly DependencyProperty BackgroundOffProperty =
            DependencyProperty.Register(
                nameof(BackgroundOff),
                typeof(SolidColorBrush),
                typeof(Toggle),
                new PropertyMetadata(new SolidColorBrush("#FFDEDEDE".ARGBToColor()), OnBackgroundOffChanged)
            );

        private static void OnBackgroundOffChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Toggle toggle)
                return;

            toggle.SetupOffStoryboard();
            toggle.UpdateBorderBackgroundWithoutAnimation(toggle.IsToggled);
        }

        public SolidColorBrush BackgroundOff
        {
            get { return (SolidColorBrush)GetValue(BackgroundOffProperty); }
            set { SetValue(BackgroundOffProperty, value); }
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

        public override string ToString()
        {
            return string.Empty;
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ToggleAutomationPeer(this);
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

        Storyboard? _onStoryboard;
        Storyboard? _offStoryboard;

        public Toggle()
        {
            InitializeComponent();

#if DEBUG
            if (DesignerProperties.GetIsInDesignMode(this))
                return;
#endif

            LayoutRoot.DataContext = this;
            SetupOnStoryboard();
            SetupOffStoryboard();
            SizeChanged += (sender, args) =>
            {
                UpdateBorderCornerRadius();
                UpdateHandleSize();
                SetToggleMargin(IsToggled);
                ToggleEllipse.Margin = ToggleMargin;
            };
            Loaded += (s, e) =>
            {
                UpdateBorderCornerRadius();
                UpdateHandleSize();
                UpdateBorderBackgroundWithoutAnimation(IsToggled);

                SetToggleMargin(IsToggled);
                ToggleEllipse.Margin = ToggleMargin;
            };

            PreviewKeyDown += (s, e) =>
            {
                if (IsKeyboardFocused)
                {
                    if (e.Key == Key.Space || e.Key == Key.Enter)
                    {
                        _ = UpdateForMouseClickOrKeyboardAction();
                        e.Handled = true;
                    }
                }
            };
        }

        void SetupOnStoryboard()
        {
            if (_onStoryboard != null)
                _onStoryboard.Completed -= Storyboard_Completed;

            if (_onStoryboard == null)
            {
                var colourOnAnim = new ColorAnimation()
                {
                    To = BackgroundOn.Color,
                    Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                    FillBehavior = FillBehavior.Stop,
                };

                Storyboard.SetTargetName(Border, Border.Name);
                Storyboard.SetTargetProperty(colourOnAnim, new PropertyPath(Border.BackgroundProperty));

                _onStoryboard = new Storyboard() { FillBehavior = FillBehavior.Stop };
                _onStoryboard.Children.Add(colourOnAnim);
            }

            _onStoryboard.Completed += Storyboard_Completed;
        }

        void SetupOffStoryboard()
        {
            if (_offStoryboard != null)
                _offStoryboard.Completed -= Storyboard_Completed;

            if (_offStoryboard == null)
            {
                var colourOnAnim = new ColorAnimation()
                {
                    To = BackgroundOff.Color,
                    Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                    FillBehavior = FillBehavior.Stop,
                };

                Storyboard.SetTargetName(Border, Border.Name);
                Storyboard.SetTargetProperty(colourOnAnim, new PropertyPath(Border.BackgroundProperty));

                _offStoryboard = new Storyboard() { FillBehavior = FillBehavior.Stop };
                _offStoryboard.Children.Add(colourOnAnim);
            }

            _offStoryboard.Completed += Storyboard_Completed;
        }

        void Storyboard_Completed(object? sender, EventArgs e)
        {
            // Storyboard now not holding end value
            UpdateBorderBackgroundWithoutAnimation(IsToggled);
        }

        void UpdateBorderBackgroundWithoutAnimation(bool isToggled)
        {
            Border.Background = isToggled
                ? BackgroundOn.Clone()
                : BackgroundOff.Clone();
        }

        void UpdateHandleSize()
        {
            var handlePadding = 6;
            ToggleEllipse.Height = Math.Max(Border.ActualHeight - handlePadding, 10);
            ToggleEllipse.Width = Math.Max(Border.ActualHeight - handlePadding, 10);
        }

        void UpdateBorderCornerRadius()
        {
            var uniformBackgroundRadius = Border.ActualHeight < double.PositiveInfinity
                ? (Border.ActualHeight - /*(Border.Margin.Top + Border.Margin.Bottom)*/0) / 2
                : 0;
            Border.CornerRadius = new CornerRadius(uniformBackgroundRadius);

            // these are different sizes, will look a little off if not the same
            var uniformHighlightRadius = HighlightBorder.ActualHeight < double.PositiveInfinity
                ? (HighlightBorder.ActualHeight - /*(HighlightBorder.Margin.Top + HighlightBorder.Margin.Bottom)*/0) / 2
                : 0;
            HighlightBorder.CornerRadius = new CornerRadius(uniformHighlightRadius);
        }

        void SetToggleMargin(bool toggled)
        {
            ToggleMargin = new Thickness(
                toggled
                    ? Border.ActualWidth + Border.Margin.Left - ToggleEllipse.ActualWidth - 4
                    : Border.Margin.Right + 4,
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
                void complete(object? sender, EventArgs args)
                {
                    ToggleEllipse.Margin = ToggleMargin;
                    ta.Completed -= complete;
                }
                ta.Completed += complete;

                ToggleEllipse.BeginAnimation(Ellipse.MarginProperty, ta);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            _ = UpdateForMouseClickOrKeyboardAction();
        }

        async Task UpdateForMouseClickOrKeyboardAction()
        {
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
                var storyboard = newToggledState ? _onStoryboard : _offStoryboard;
                storyboard.Begin(this, HandoffBehavior.SnapshotAndReplace, true);

            }
            catch (Exception)
            {
                UpdateBorderBackgroundWithoutAnimation(newToggledState);
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
