using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Utilizr.WPF.Attached;

namespace Utilizr.WPF.Model
{
    public class LightBoxInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<LightBoxInfoShowChangedEventArgs>? ShowChanged;

        private bool _show;
        /// <summary>
        /// Whether the lightbox is currently being shown
        /// </summary>
        public bool Show
        {
            get { return _show; }
            set
            {
                var oldShow = _show;
                _show = value;
                OnPropertyChanged(nameof(Show));
                OnShowChanged(value);

                // Only fire if changing state, not subsequent close calls
                if (!value && oldShow)
                {
                    ClosedAction?.Invoke();
                }
            }
        }

        private void View_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool isVisible && isVisible)
            {
                Keyboard.Focus(View!);
                View!.Focus();
            }
        }

        private UIElement? _view;
        /// <summary>
        /// The view which the lightbox will host
        /// </summary>
        public UIElement? View
        {
            get { return _view; }
            set
            {
                if (_view != null && _view != value)
                {
                    // unregister if new and not the same view
                    _view.IsVisibleChanged -= View_IsVisibleChanged;
                    EscapeKeyBehaviour.SetEscapeKeyAction(_view, null);
                    EscapeKeyBehaviour.SetEnableEscapeKey(_view, false);
                }

                _view = value;

                if (value != null)
                {
                    EscapeKeyBehaviour.SetEscapeKeyAction(value, (uie) =>
                    {
                        Show = false;
                    });

                    EscapeKeyBehaviour.SetEnableEscapeKey(value, true);

                    value.Focusable = true;
                    value.IsVisibleChanged += View_IsVisibleChanged;
                }

                OnPropertyChanged(nameof(View));
            }
        }

        private bool _hasClose;
        /// <summary>
        /// Wether the lightbox will show a close button
        /// </summary>
        public bool HasClose
        {
            get { return _hasClose; }
            set
            {
                _hasClose = value;
                OnPropertyChanged(nameof(HasClose));
            }
        }

        private Action? _closedAction;
        /// <summary>
        /// A custom action to be preformed when the lightbox is closed, if set.
        /// </summary>
        public Action? ClosedAction
        {
            get { return _closedAction; }
            set
            {
                _closedAction = value;
                OnPropertyChanged(nameof(ClosedAction));
            }
        }

        /// <summary>
        /// Used to indicate whether the app should attempt to bring itself to the foreground
        /// </summary>
        private bool _bringAppIntoFocus;
        public bool BringAppIntoFocus
        {
            get { return _bringAppIntoFocus; }
            set
            {
                _bringAppIntoFocus = value;
                OnPropertyChanged(nameof(BringAppIntoFocus));
            }
        }

        private Brush _lightBoxBackground = new SolidColorBrush(Colors.White);
        public Brush LightBoxBackground
        {
            get { return _lightBoxBackground; }
            set
            {
                _lightBoxBackground = value;
                OnPropertyChanged(nameof(LightBoxBackground));
            }
        }


        public static LightBoxInfo Empty()
        {
            return new LightBoxInfo(null, true, false, false, null);
        }

        public LightBoxInfo(
            UIElement? view,
            bool hasClose = true,
            bool show = true,
            bool bringAppIntoFocus = false,
            Action? closedAction = null,
            Brush? backgroundBrush = null)
        {
            View = view;
            HasClose = hasClose;
            Show = show;
            BringAppIntoFocus = bringAppIntoFocus;
            ClosedAction = closedAction;

            if (backgroundBrush != null)
                LightBoxBackground = backgroundBrush;
        }

        public void SetContent(UIElement view, bool hasClose = true, bool show = true)
        {
            View = view;
            HasClose = hasClose;
            Show = show;
        }

        protected virtual void OnPropertyChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected virtual void OnShowChanged(bool showing)
        {
            ShowChanged?.Invoke(this, new LightBoxInfoShowChangedEventArgs(showing));
        }
    }

    public class LightBoxInfoShowChangedEventArgs : EventArgs
    {
        public bool Showing { get; }

        public LightBoxInfoShowChangedEventArgs(bool showing)
        {
            Showing = showing;
        }
    }
}