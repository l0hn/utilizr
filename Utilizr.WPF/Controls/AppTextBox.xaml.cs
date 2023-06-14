using Utilizr.Globalisation;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Utilizr.Logging;
using Utilizr.Validation;
//using Utilizr.WPF.Model;
using Utilizr.WPF.Util;
using ValidationResult = Utilizr.Validation.ValidationResult;
using Utilizr.Model;

namespace Utilizr.WPF
{
    public partial class AppTextBox : UserControl, INotifyPropertyChanged
    {
        private const double DEFAULT_IMAGE_SIZE = 20;

        #region INotifyPropertyChanged Impl
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(params string[] properties)
        {
            foreach (var propName in properties)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }
        #endregion

        public event TextChangedEventHandler TextChanged;

        #region properties

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register(
                nameof(Image),
                typeof(ImageSource),
                typeof(AppTextBox),
                new PropertyMetadata(default(ImageSource))
            );

        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        private Thickness _imageMargin = new(0);
        public Thickness ImageMargin
        {
            get { return _imageMargin; }
            set
            {
                _imageMargin = value;
                OnPropertyChanged(nameof(ImageMargin));
            }
        }

        private double _imageHeight = DEFAULT_IMAGE_SIZE;
        public double ImageHeight
        {
            get { return _imageHeight; }
            set
            {
                _imageHeight = value;
                OnPropertyChanged(nameof(ImageHeight));
            }
        }

        public static readonly DependencyProperty ImageWidthProperty =
            DependencyProperty.Register(
                nameof(ImageWidth),
                typeof(double),
                typeof(AppTextBox),
                new PropertyMetadata(DEFAULT_IMAGE_SIZE)
            );

        public double ImageWidth
        {
            get { return (double)GetValue(ImageWidthProperty); }
            set
            {
                SetValue(ImageWidthProperty, value);
                OnPropertyChanged(nameof(ImageWidth));
            }
        }        

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(AppTextBox),
                new PropertyMetadata(default(string))
            );

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set
            {
                SetValue(TextProperty, value);
                OnPropertyChanged(nameof(Text));
            }
        }
        
        public Brush PasswordForeground => IsPassword ? Foreground : new SolidColorBrush(Colors.Transparent);
        public Brush PlainTextForeground => IsPassword ? new SolidColorBrush(Colors.Transparent) : Foreground;

        public static readonly DependencyProperty IsPasswordProperty =
            DependencyProperty.Register(nameof(IsPassword),
                typeof(bool),
                typeof(AppTextBox),
                new PropertyMetadata(default(bool), IsPasswordChanged)
            );

        private static void IsPasswordChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var self = dependencyObject as AppTextBox;

            if ((bool)dependencyPropertyChangedEventArgs.NewValue && self != null)
            {
                var pwd = self.PasswordBox;
                if (pwd != null)
                {
                    pwd.Password = self.Text;
                }
            }
            
            self?.OnPropertyChanged(
                nameof(PasswordForeground),
                nameof(PlainTextForeground));
        }

        public bool IsPassword
        {
            get { return (bool)GetValue(IsPasswordProperty); }
            set
            {
                SetValue(IsPasswordProperty, value);
                OnPropertyChanged(nameof(IsPassword));
                OnPropertyChanged(nameof(PasswordForeground));
                OnPropertyChanged(nameof(PlainTextForeground));
            }
        }


        public static readonly DependencyProperty WatermarkTextProperty =
            DependencyProperty.Register(
                nameof(WatermarkText),
                typeof(string),
                typeof(AppTextBox),
                new PropertyMetadata(default(string))
            );

        public string WatermarkText
        {
            get { return (string)GetValue(WatermarkTextProperty); }
            set { SetValue(WatermarkTextProperty, value); }
        }


        public static readonly DependencyProperty WatermarkForegroundProperty =
            DependencyProperty.Register(
                nameof(WatermarkForeground),
                typeof(Brush),
                typeof(AppTextBox),
                new PropertyMetadata(null)
            );

        /// <summary>
        /// Optionally provide an exact watermark text colour. Will default to opacity of 0.7 if null.
        /// </summary>
        public Brush WatermarkForeground
        {
            get { return (Brush)GetValue(WatermarkForegroundProperty); }
            set { SetValue(WatermarkForegroundProperty, value); }
        }


        private FontWeight _fontWeight;
        public new FontWeight FontWeight
        {
            get { return _fontWeight; }
            set
            {
                _fontWeight = value;
                OnPropertyChanged(nameof(FontWeight));
            }
        }

        private double _fontSize;
        public new double FontSize
        {
            get { return _fontSize; }
            set
            {
                _fontSize = value;
                OnPropertyChanged(nameof(FontSize));
            }
        }

        public static readonly DependencyProperty ImageLeftAlignedDependencyProperty =
            DependencyProperty.Register(
                nameof(ImageLeftAligned),
                typeof(bool),
                typeof(AppTextBox),
                new PropertyMetadata(default(bool))
            );

        public bool ImageLeftAligned
        {
            get { return (bool)GetValue(ImageLeftAlignedDependencyProperty); }
            set
            {
                SetValue(ImageLeftAlignedDependencyProperty, value);
                OnPropertyChanged(nameof(ImageLeftAligned));
            }
        }

        public static readonly new DependencyProperty BorderBrushProperty =
            DependencyProperty.Register(
                nameof(BorderBrush),
                typeof(Brush),
                typeof(AppTextBox),
                new PropertyMetadata(
                    ResourceHelper.GetDictionaryDefined<Brush>(nameof(AppTextBox), nameof(BorderBrush))
                )
            );

        public new Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set
            {
                SetValue(BorderBrushProperty, value);
                OnPropertyChanged(nameof(BorderBrush));
            }
        }



        public static readonly DependencyProperty TextBoxBackgroundDependencyProperty =
            DependencyProperty.Register(
                nameof(TextBoxBackground),
                typeof(Brush),
                typeof(AppTextBox), 
                new PropertyMetadata(
                    ResourceHelper.GetDictionaryDefined<Brush>(nameof(AppTextBox), nameof(TextBoxBackground))
                )
            );

        public Brush TextBoxBackground
        {
            get { return (Brush)GetValue(TextBoxBackgroundDependencyProperty); }
            set
            {
                SetValue(TextBoxBackgroundDependencyProperty, value);
                OnPropertyChanged(nameof(TextBoxBackground));
            }
        }

        public static readonly DependencyProperty WatermarkAlignmentDependencyProperty =
            DependencyProperty.Register(
                nameof(WatermarkAlignment),
                typeof(TextAlignment),
                typeof(AppTextBox),
                new PropertyMetadata(default(TextAlignment))
            );

        public TextAlignment WatermarkAlignment
        {
            get { return (TextAlignment)GetValue(WatermarkAlignmentDependencyProperty); }
            set
            {
                SetValue(WatermarkAlignmentDependencyProperty, value);
                OnPropertyChanged(nameof(WatermarkAlignment));
            }
        }

        public static readonly DependencyProperty ShowValidationErrorIconProperty = 
            DependencyProperty.Register(
                nameof(ShowValidationErrorIcon),
                typeof(bool),
                typeof(AppTextBox),
                new PropertyMetadata(default(bool))
            );

        public bool ShowValidationErrorIcon
        {
            get { return (bool) GetValue(ShowValidationErrorIconProperty); }
            set { SetValue(ShowValidationErrorIconProperty, value); }
        }

        public static readonly DependencyProperty ValidationMessageProperty = 
            DependencyProperty.Register(
                nameof(ValidationMessage),
                typeof(string),
                typeof(AppTextBox),
                new PropertyMetadata(default(string))
            );

        public string ValidationMessage
        {
            get { return (string) GetValue(ValidationMessageProperty); }
            set { SetValue(ValidationMessageProperty, value); }
        }

        public static readonly DependencyProperty ValidaterProperty = 
            DependencyProperty.Register(
                nameof(InputValidater),
                typeof (Validater),
                typeof (AppTextBox),
                new PropertyMetadata(
                    default(Validater),
                    OnValidaterPropertyChanged
                )
            );

        private static void OnValidaterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is AppTextBox appTextBox))
                return;

            if (e.OldValue is Validater oldValidater)
            {
                oldValidater.ClearErrorRequest -= appTextBox.ClearErrorRequest;
                oldValidater.Validated -= appTextBox.ValidationRequested;
            }

            if (e.NewValue is Validater newValidater)
            {
                newValidater.ClearErrorRequest += appTextBox.ClearErrorRequest;
                newValidater.Validated += appTextBox.ValidationRequested;
            }
        }

        public Validater InputValidater
        {
            get { return (Validater) GetValue(ValidaterProperty); }
            set { SetValue(ValidaterProperty, value); }
        }

        private void ValidationRequested(object sender, ValidationResult eventArgs)
        {
            AllowValidationToShow = true;
            ProcessValidation(eventArgs);
        }

        private void ClearErrorRequest(object sender, EventArgs eventArgs)
        {
            ShowValidationErrorIcon = false;
            AllowValidationToShow = false;
        }

        public static readonly DependencyProperty AllowValidationToShowProperty =
            DependencyProperty.Register(
                nameof(AllowValidationToShow),
                typeof(bool),
                typeof(AppTextBox),
                new PropertyMetadata(default(bool))
            );

        public bool AllowValidationToShow
        {
            get { return (bool) GetValue(AllowValidationToShowProperty); }
            set
            {
                SetValue(AllowValidationToShowProperty, value);
                OnPropertyChanged(nameof(ShowValidationErrorIcon));
            }
        }


        public static readonly DependencyProperty InputValidaterModeProperty =
            DependencyProperty.Register(
                nameof(InputValidaterMode),
                typeof(InputValidationMode),
                typeof(AppTextBox),
                new PropertyMetadata(InputValidationMode.Default)
            );

        /// <summary>
        /// Determines how the validation will be invoked
        /// </summary>
        public InputValidationMode InputValidaterMode
        {
            get { return (InputValidationMode)GetValue(InputValidaterModeProperty); }
            set { SetValue(InputValidaterModeProperty, value); }
        }


        public static readonly DependencyProperty ClearPasswordNowProperty =
            DependencyProperty.Register(
                nameof(ClearPasswordNow),
                typeof(bool),
                typeof(AppTextBox),
                new PropertyMetadata(false, OnClearPasswordNowChanged)
            );

        private static void OnClearPasswordNowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                (d as AppTextBox)?.ClearPasswordContent();
            }
        }

        /// <summary>
        /// When true, invokes <see cref="ClearPasswordContent"/>. Provides a method to clear a
        /// password from binding alone.
        /// </summary>
        public bool ClearPasswordNow
        {
            get { return (bool)GetValue(ClearPasswordNowProperty); }
            set { SetValue(ClearPasswordNowProperty, value); }
        }


        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(AppTextBox),
                new PropertyMetadata(null)
            );

        /// <summary>
        /// Executed when the enter key has been pressed.
        /// </summary>
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }


        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(
                nameof(CommandParameter),
                typeof(object),
                typeof(AppTextBox),
                new PropertyMetadata(null)
            );

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }


        public static readonly DependencyProperty BorderStyleProperty = 
            DependencyProperty.Register(
                nameof(BorderStyle),
                typeof(Style),
                typeof(AppTextBox),
                new PropertyMetadata(default(Style))
            );

        public Style BorderStyle
        {
            get { return (Style) GetValue(BorderStyleProperty); }
            set { SetValue(BorderStyleProperty, value); }
        }


        public static readonly DependencyProperty CapsLockBubbleBackgroundProperty =
            DependencyProperty.Register(
                nameof(CapsLockBubbleBackground),
                typeof(Brush),
                typeof(AppTextBox),
                new PropertyMetadata(
                    ResourceHelper.GetDictionaryDefined<Brush>(nameof(AppTextBox), nameof(CapsLockBubbleBackground))
                )
            );

        public Brush CapsLockBubbleBackground
        {
            get { return (Brush)GetValue(CapsLockBubbleBackgroundProperty); }
            set { SetValue(CapsLockBubbleBackgroundProperty, value); }
        }


        public static readonly DependencyProperty CapsLockBubbleBorderProperty =
            DependencyProperty.Register(
                nameof(CapsLockBubbleBorder),
                typeof(Brush),
                typeof(AppTextBox),
                new PropertyMetadata(
                    ResourceHelper.GetDictionaryDefined<Brush>(nameof(AppTextBox), nameof(CapsLockBubbleBorder))
                )
            );

        public Brush CapsLockBubbleBorder
        {
            get { return (Brush)GetValue(CapsLockBubbleBorderProperty); }
            set { SetValue(CapsLockBubbleBorderProperty, value); }
        }



        public static readonly DependencyProperty ShowCapsLockWarningProperty =
            DependencyProperty.Register(
                nameof(ShowCapsLockWarning),
                typeof(bool),
                typeof(AppTextBox),
                new PropertyMetadata(false)
            );

        /// <summary>
        /// <see cref="CapsLockHelper.Initialise(Window)"/> must be called when window loaded before this can be used./>
        /// </summary>
        public bool ShowCapsLockWarning
        {
            get { return (bool)GetValue(ShowCapsLockWarningProperty); }
            set { SetValue(ShowCapsLockWarningProperty, value); }
        }

        public bool IsCapsLockOnAndShouldShow
        {
            get
            {
                if (!ShowCapsLockWarning)
                    return false;

                return CapsLockHelper.CapsLock;
            }
        }

        public static readonly DependencyProperty CapsLockMessageProperty =
            DependencyProperty.Register(
                nameof(CapsLockMessage),
                typeof(ITranslatable),
                typeof(AppTextBox),
                new PropertyMetadata(L._I("Caps Lock is turned on"))
            );

        public ITranslatable CapsLockMessage
        {
            get { return (ITranslatable)GetValue(CapsLockMessageProperty); }
            set { SetValue(CapsLockMessageProperty, value); }
        }


        public static new readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register(
                nameof(Padding),
                typeof(Thickness),
                typeof(AppTextBox),
                new PropertyMetadata(new Thickness(6,2,0,3))
            );

        public new Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        private bool _showWatermark = true;
        public bool ShowWatermark
        {
            get { return _showWatermark; }
            private set
            {
                _showWatermark = value;
                OnPropertyChanged(nameof(ShowWatermark));
            }
        }

        /// <summary>
        /// If true, will call Focus() when Loaded fires.
        /// </summary>
        public bool FocusOnLoaded { get; set; }

        #endregion

        private PasswordBox PasswordBox => (PasswordBox)TextBox.Template.FindName("PasswordTextBox", TextBox);

        /// <summary>
        /// Last value used to revert when escape key pressed.
        /// </summary>
        string _lastValue;

        public AppTextBox()
        {
            InitializeComponent();
            TextBox.DataContext = this;

            //Handle all focus events
            GotFocus += AppTextBox_GotGenericFocus;
            LostFocus += AppTextBox_LostGenricFocus;
            GotKeyboardFocus += AppTextBox_GotGenericFocus;
            LostKeyboardFocus += AppTextBox_LostGenricFocus;
            CapsLockHelper.CapsLockChanged += (capsLock) => OnPropertyChanged(nameof(IsCapsLockOnAndShouldShow));
            L.LocaleChanged += (s, e) => OnPropertyChanged(nameof(CapsLockMessage));

            TextBox.KeyUp += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    Text = _lastValue;
                    if (IsPassword)
                    {
                        SetPasswordContent(_lastValue);
                    }

                    //_lastValue set when loosing focus, do this after setting Text
                    //var tr = new TraversalRequest(FocusNavigationDirection.Previous);
                    //MoveFocus(tr);

                    // Remove focus to parent which can accept focus
                    try
                    {
                        var parent = (FrameworkElement)TextBox.Parent;
                        while (parent != null && (parent as IInputElement)?.Focusable == false)
                        {
                            parent = (FrameworkElement)parent.Parent;
                        }

                        DependencyObject scope = FocusManager.GetFocusScope(TextBox);
                        FocusManager.SetFocusedElement(scope, parent as IInputElement);
                    }
                    catch (Exception ex)
                    {
                        Log.Exception(nameof(AppTextBox), ex);
                    }
                }
                else if (e.Key == Key.Enter)
                {
                    SetLastValue();
                    try
                    {
                        if (Command?.CanExecute(CommandParameter) == true)
                        {
                            Command?.Execute(CommandParameter);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Exception(ex);
                    }
                }
            };

            TextBox.PreviewTextInput += (s, e) =>
            {
                // Set here on preview event to avoid watermark being removed
                // after the user's text is displaying on the UI. Will still need to
                // be set in other places, e.g., lost focus.
                SetShowWatermark(e.Text);
            };
            TextBox.TextChanged += (s, e) => OnTextChanged(e);

            Loaded += (sender, args) =>
            {
                if (IsPassword)
                {
                    var pwd = PasswordBox;
                    if (pwd != null && pwd.Password != Text)
                    {
                        pwd.Password = Text;

                        if (FocusOnLoaded)
                            pwd.Focus();
                    }
                }
                else if (FocusOnLoaded)
                {
                    TextBox.Focus();
                }
                SetLastValue();
                OnPropertyChanged(nameof(IsCapsLockOnAndShouldShow));
            };
        }

        private void Validate()
        {
            if (!AllowValidationToShow)
            {
                return;
            }
            if (InputValidater == null)
            {
                return;
            }
            var validationResult = InputValidater.Validate(Text);
            ProcessValidation(validationResult);
        }

        void ProcessValidation(ValidationResult validationResult)
        {
            ShowValidationErrorIcon = !validationResult.IsValid;
            ValidationMessage = validationResult.ToString();
        }

        private void AppTextBox_GotGenericFocus(object sender, EventArgs e)
        {
            if (IsPassword)
            {
                var pwd = PasswordBox;
                if (pwd != null)
                {
                    pwd.Focus();
                    pwd.SelectAll();
                }
            }
            else
            {
                TextBox.Focus();
                if (TextBox.SelectedText != Text)
                {
                    TextBox.SelectAll();
                }
            }
        }

        private void AppTextBox_LostGenricFocus(object sender, EventArgs e)
        {
            // Still need to call HandleLostFocus, even if not in Default mode
            if (!IsPassword && InputValidaterMode == InputValidationMode.Default)
                AllowValidationToShow = true;

            HandleLostFocus();
        }

        protected virtual void OnTextChanged(TextChangedEventArgs e)
        {
            SetShowWatermark(TextBox.Text);

            if (IsPassword)
            {
                var pwd = PasswordBox;
                if (pwd != null && pwd.Password != Text)
                    pwd.Password = Text;
            }

            TextChanged?.Invoke(this, e);

            if(InputValidaterMode == InputValidationMode.Default)
                Validate();
        }

        private void PasswordBox_KeyUp(object sender, KeyEventArgs e)
        {
            Text = (sender as PasswordBox)?.Password;
            SetShowWatermark(Text);
        }

        private void PasswordTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Still need to call HandleLostFocus, even if not in Default mode
            if (IsPassword && InputValidaterMode == InputValidationMode.Default)
                AllowValidationToShow = true;

            HandleLostFocus();
        }

        public void ClearPasswordContent()
        {
            SetPasswordContent(null);
        }

        void SetPasswordContent(string pwdValue)
        {
            if (IsPassword)
            {
                try
                {
                    var pwd = PasswordBox;
                    if (pwd != null)
                    {
                        pwd.Password = pwdValue;
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        void HandleLostFocus()
        {
            SetShowWatermark(Text);

            if (InputValidaterMode != InputValidationMode.Default)
                return;

            SetLastValue();
            Validate();
        }

        void SetLastValue()
        {
            _lastValue = IsPassword
                ? PasswordBox?.Password ?? null
                : TextBox.Text;
        }

        void SetShowWatermark(string text)
        {
            ShowWatermark = string.IsNullOrEmpty(text);
        }
    }
}