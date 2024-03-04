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

        public static readonly DependencyProperty TextBrushProperty =
            DependencyProperty.Register(
                nameof(TextBrush),
                typeof(Brush),
                typeof(SecureTextBlock),
                new PropertyMetadata(System.Windows.Media.Brushes.Transparent)
            );

        public Brush TextBrush
        {
            get { return (Brush)GetValue(TextBrushProperty); }
            set { SetValue(TextBrushProperty, value); }
        }


        public static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register(
                nameof(TextAlignment),
                typeof(TextAlignment),
                typeof(SecureTextBlock),
                new PropertyMetadata(TextAlignment.Left)
            );

        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }


        public SecureTextBlock()
        {
            InitializeComponent();
            SecureTextShape.DataContext = this;
            SecureTextShape.Typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
        }
    }
}
