using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Utilizr.WPF.Controls
{
    public partial class IconButton : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(params string[] properties)
        {
            foreach (var propName in properties)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }
        #endregion

        public event EventHandler<IconButtonClickedEventArgs> Clicked;

        public static readonly DependencyProperty IconOnProperty = 
            DependencyProperty.Register(
                nameof(IconOn),
                typeof(ImageSource),
                typeof(IconButton),
                new PropertyMetadata(default(ImageSource))
            );
        /// <summary>
        /// Image 'On' state. If states not required, only one of IconOn or IconOff needs to be set.
        /// </summary>
        public ImageSource IconOn
        {
            get { return (ImageSource)GetValue(IconOnProperty); }
            set { SetValue(IconOnProperty, value); }
        }


        public static readonly DependencyProperty IconOffProperty =
            DependencyProperty.Register(
                nameof(IconOff),
                typeof(ImageSource),
                typeof(IconButton),
                new PropertyMetadata(default(ImageSource))
            );
        /// <summary>
        /// Image 'Off' state. If states not required, only one of IconOn or IconOff needs to be set.
        /// </summary>
        public ImageSource IconOff
        {
            get { return (ImageSource)GetValue(IconOffProperty); }
            set { SetValue(IconOffProperty, value); }
        }


        public static readonly DependencyProperty IsOnProperty =
            DependencyProperty.Register(
                nameof(IsOn),
                typeof(bool),
                typeof(IconButton),
                new PropertyMetadata(default(bool), IsOnChanged)
            );

        private static void IsOnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is IconButton btnObj))
                return;

            btnObj.FakeCurrentIconChanged();
        }

        public bool IsOn
        {
            get { return (bool)GetValue(IsOnProperty); }
            set { SetValue(IsOnProperty, value); }
        }

        public ImageSource CurrentIcon
        {
            get
            {
                //If only one image set, always use that.
                if(IsOn)
                    return IconOn ?? IconOff;

                return IconOff ?? IconOn;
            }
        }


        public static readonly DependencyProperty DrawIconUnscaledProperty = 
            DependencyProperty.Register(
                nameof(DrawIconUnscaled),
                typeof(bool),
                typeof(IconButton),
                new PropertyMetadata(true)
            );

        public bool DrawIconUnscaled
        {
            get { return (bool) GetValue(DrawIconUnscaledProperty); }
            set { SetValue(DrawIconUnscaledProperty, value); }
        }


        public static readonly DependencyProperty CommandProperty = 
            DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(IconButton),
                new PropertyMetadata(default(ICommand))
            );

        public ICommand Command
        {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }


        public static readonly DependencyProperty CommandParameterProperty = 
            DependencyProperty.Register(
                nameof(CommandParameter),
                typeof(object),
                typeof(IconButton),
                new PropertyMetadata(null)
            );

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }


        public static readonly DependencyProperty UnscaledStretchDirectionProperty =
            DependencyProperty.Register(
                nameof(UnscaledStretchDirection),
                typeof(StretchDirection),
                typeof(IconButton),
                new PropertyMetadata(StretchDirection.DownOnly)
            );

        public StretchDirection UnscaledStretchDirection
        {
            get { return (StretchDirection)GetValue(UnscaledStretchDirectionProperty); }
            set { SetValue(UnscaledStretchDirectionProperty, value); }
        }



        public static readonly DependencyProperty UnscaledImageStyleProperty =
            DependencyProperty.Register(
                nameof(UnscaledImageStyle),
                typeof(Style),
                typeof(IconButton),
                new PropertyMetadata(default(Style))
            );

        public Style UnscaledImageStyle
        {
            get { return (Style)GetValue(UnscaledImageStyleProperty); }
            set { SetValue(UnscaledImageStyleProperty, value); }
        }

        public IconButton()
        {
            InitializeComponent();
            Button.DataContext = this;
            Button.Click += (s, e) =>
            {
                bool newState = !IsOn;
                IsOn = newState;
                OnButtonIconClicked(newState);
            };
            FakeCurrentIconChanged();            
        }

        private void FakeCurrentIconChanged()
        {
            OnPropertyChanged(nameof(CurrentIcon));
        }

        protected virtual void OnButtonIconClicked(bool isOn)
        {
            Clicked?.Invoke(this, new IconButtonClickedEventArgs(isOn));
        }
    }

    public class IconButtonClickedEventArgs : EventArgs
    {
        public bool IsOn { get; set; }

        public IconButtonClickedEventArgs(bool isOn)
        {
            IsOn = isOn;
        }
    }
}