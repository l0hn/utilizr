using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Utilizr.Async;
using Utilizr.Network;
using Utilizr.WPF.Extension;

namespace Utilizr.WPF.Controls
{
    public partial class FavIconImage : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public static readonly DependencyProperty DomainProperty =
            DependencyProperty.Register(
                nameof(Domain),
                typeof(string),
                typeof(FavIconImage),
                new PropertyMetadata("google.com", DomainPropertyChanged)
               );

        public string Domain
        {
            get { return (string)GetValue(DomainProperty); }
            set { SetValue(DomainProperty, value); }
        }

        private static void DomainPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (!(dependencyObject is FavIconImage self))
                return;

            self._domain = dependencyPropertyChangedEventArgs.NewValue as string;
            if (dependencyPropertyChangedEventArgs.OldValue == dependencyPropertyChangedEventArgs.NewValue)
                return;

            _ = self.FetchIconImage();
        }


        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register(
                nameof(Size),
                typeof(int),
                typeof(FavIconImage),
                new PropertyMetadata(32, SizePropertyChanged)
            );

        public int Size
        {
            get { return (int)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        private static void SizePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (!(dependencyObject is FavIconImage self))
                return;

            self._size = (int)dependencyPropertyChangedEventArgs.NewValue;
            if (dependencyPropertyChangedEventArgs.OldValue == dependencyPropertyChangedEventArgs.NewValue)
                return;

            self._favIco = null;
            _ = self.FetchIconImage();
        }


        public static readonly DependencyProperty OverlayBrushProperty =
            DependencyProperty.Register(
                nameof(OverlayBrush),
                typeof(Brush),
                typeof(FavIconImage),
                new PropertyMetadata(null)
                );

        public Brush OverlayBrush
        {
            get { return (Brush)GetValue(OverlayBrushProperty); }
            set { SetValue(OverlayBrushProperty, value); }
        }


        public static readonly DependencyProperty DefaultIconImageSourceProperty =
            DependencyProperty.Register(
                nameof(DefaultIconImageSource),
                typeof(ImageSource),
                typeof(FavIconImage),
                new PropertyMetadata(null, OnDefaultIconImageSourceChanged)
            );

        private static void OnDefaultIconImageSourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (!(dependencyObject is FavIconImage self))
                return;

            self._defaultImageSource = dependencyPropertyChangedEventArgs.NewValue as ImageSource;
        }

        public ImageSource DefaultIconImageSource
        {
            get { return (ImageSource)GetValue(DefaultIconImageSourceProperty); }
            set { SetValue(DefaultIconImageSourceProperty, value); }
        }


        public static readonly DependencyProperty AlwaysUseDefaultImageProperty =
            DependencyProperty.Register(
                nameof(AlwaysUseDefaultImage),
                typeof(bool),
                typeof(FavIconImage),
                new PropertyMetadata(
                    // Note: this intentionally needs to be the default and opt-in to avoid the dotnet core 
                    // regression bug in windows 8.1 https://github.com/dotnet/wpf/issues/3762#issuecomment-781570208
                    true,
                    (d, e) =>
                    {
                        if (!(d is FavIconImage favIconImage))
                            return;

                        _ = favIconImage?.FetchIconImage();
                    }
                )
            );
        /// <summary>
        /// If true, will only display the image within <see cref="DefaultIconImageSource"/>.
        /// </summary>
        public bool AlwaysUseDefaultImage
        {
            get { return (bool)GetValue(AlwaysUseDefaultImageProperty); }
            set { SetValue(AlwaysUseDefaultImageProperty, value); }
        }


        private ImageSource? _iconImageSource;
        public ImageSource? IconImageSource => _iconImageSource ?? DefaultIconImageSource; 

        private FavIco? _favIco;
        private FavIco? FavIco
        {
            get { return _favIco; }
            set
            {
                    _favIco = value;
                    if (_favIco != null && !string.IsNullOrEmpty(_favIco.FilePath))
                    {
                        try
                        {
                            Application.Current.Dispatcher.SafeInvoke(() =>
                            {
                                _iconImageSource = BitmapFrame.Create(
                                    new Uri(_favIco.FilePath),
                                    BitmapCreateOptions.None,
                                    BitmapCacheOption.Default);
                            });
                        
                            UpdateDecorationForImage(_iconImageSource);
                        }
                        catch
                        {
                            _iconImageSource = null;
                            UpdateDecorationForImage(_defaultImageSource);
                        }
                    }
                    else
                    {
                        _iconImageSource = null;
                        UpdateDecorationForImage(_defaultImageSource);
                    }

                    Application.Current.Dispatcher.SafeInvoke(() => OnPropertyChanged(nameof(IconImageSource)));
            }
        }

        private bool _drawUnscaled;
        public bool DrawUnscaled
        {
            get => _drawUnscaled;
            private set
            {
                if (_drawUnscaled == value)
                    return;

                _drawUnscaled = value;
                OnPropertyChanged(nameof(DrawUnscaled));
            }
        }

        private bool _showBorder;
        public bool ShowBorder
        {
            get => _showBorder;
            private set
            {
                if (_showBorder == value)
                    return;

                _showBorder = value;
                OnPropertyChanged(nameof(ShowBorder));
            }
        }

        private Stretch _rawImageStretch;
        public Stretch RawImageStretch
        {
            get { return _rawImageStretch; }
            set
            {
                if (_rawImageStretch == value)
                    return;

                _rawImageStretch = value; 
                OnPropertyChanged(nameof(RawImageStretch));
            }
        }

        private string? _domain;
        private int _size = (int)SizeProperty.DefaultMetadata.DefaultValue;
        private ImageSource? _defaultImageSource = null;

        public FavIconImage()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
        }

        void UpdateDecorationForImage(ImageSource? source)
        {
            if (source == null)
            {
                ShowBorder = false;
                return;
            }

            var exactSize = (int)source.Width == _size && (int)source.Height == _size;
            var larger = source.Width > _size && source.Height > _size;
            var smaller = !larger && !exactSize;

            ShowBorder = smaller;
            DrawUnscaled = exactSize || smaller;
        }

        async Task FetchIconImage()
        {
            if (AlwaysUseDefaultImage)
            {
                FavIco = null;
                return;
            }

            if (_favIco?.Domain == Domain)
                return;

            if (_favIco != null)
                if (FavIcon.SanitizeUrl(_favIco.Domain) == FavIcon.SanitizeUrl(Domain))
                    return;

            Debug.WriteLine("setting to null");
            FavIco = null;

            await Task.Run(() =>
            {
                var domain = _domain;
                Sleeper.Sleep(200);
                if (domain != _domain)
                    return;

                var ico = FavIcon.GetFavIcon(domain, GetPreferredSizeOrder());
                if (domain == _domain)
                {
                    Debug.WriteLine("setting to ico");
                    FavIco = ico;
                }
            });
        }

        FavIcoSize[] GetPreferredSizeOrder()
        {
            List<FavIcoSize> sizeOrder = new List<FavIcoSize>()
            {
                _size <= 16 ? FavIcoSize.Small : _size <= 32 ? FavIcoSize.Medium : FavIcoSize.Large,
                _size <= 16 ? FavIcoSize.Medium : _size <= 32 ? FavIcoSize.Large : FavIcoSize.Medium,
                _size <= 16 ? FavIcoSize.Large : FavIcoSize.Small,
            };

            return sizeOrder.ToArray();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
