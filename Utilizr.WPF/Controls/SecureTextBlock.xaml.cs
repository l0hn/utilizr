using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Utilizr.WPF.Controls
{

    public partial class SecureTextBlock : UserControl
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
            nameof(Text),
            typeof(SecureString),
            typeof(SecureTextBlock),
            new PropertyMetadata(default(SecureString)));

        public SecureString Text
        {
            get { return (SecureString)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }


        public static readonly DependencyProperty TextFontSizeProperty =
            DependencyProperty.Register(
                nameof(TextFontSize),
                typeof(double),
                typeof(SecureTextBlock),
                new PropertyMetadata(14D)
            );

        public double TextFontSize
        {
            get { return (double)GetValue(TextFontSizeProperty); }
            set { SetValue(TextFontSizeProperty, value); }
        }

        public static readonly DependencyProperty ShadowOffsetProperty =
            DependencyProperty.Register(
                nameof(ShadowOffset),
                typeof(Point),
                typeof(SecureTextBlock),
                new PropertyMetadata(new Point(-10, -10))
            );

        public Point ShadowOffset
        {
            get { return (Point)GetValue(ShadowOffsetProperty); }
            set { SetValue(ShadowOffsetProperty, value); }
        }

        public static readonly DependencyProperty TextStrokeThicknessProperty =
            DependencyProperty.Register(
                nameof(TextStrokeThickness),
                typeof(double),
                typeof(SecureTextBlock),
                new PropertyMetadata(1D)
            );

        public double TextStrokeThickness
        {
            get { return (double)GetValue(TextStrokeThicknessProperty); }
            set { SetValue(TextStrokeThicknessProperty, value); }
        }


        public static readonly DependencyProperty TextFillProperty =
            DependencyProperty.Register(
                nameof(TextFill),
                typeof(Brush),
                typeof(SecureTextBlock),
                new PropertyMetadata(System.Windows.Media.Brushes.Transparent)
            );

        public Brush TextFill
        {
            get { return (Brush)GetValue(TextFillProperty); }
            set { SetValue(TextFillProperty, value); }
        }


        public static readonly DependencyProperty TextStrokeProperty =
            DependencyProperty.Register(
                nameof(TextStroke),
                typeof(Brush),
                typeof(SecureTextBlock),
                new PropertyMetadata(System.Windows.Media.Brushes.Black)
            );

        public Brush TextStroke
        {
            get { return (Brush)GetValue(TextStrokeProperty); }
            set { SetValue(TextStrokeProperty, value); }
        }

        public SecureTextBlock()
        {
            InitializeComponent();
            SecureTextShape.DataContext = this;
            SecureTextShape.Typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
        }
    }
}
