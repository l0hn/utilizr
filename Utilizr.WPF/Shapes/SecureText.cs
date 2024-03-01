using System;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using Utilizr.Logging;
using System.Security;
using Utilizr.Util;

namespace Utilizr.WPF.Shapes
{
    public class SecureText : Shape
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(SecureString),
                typeof(SecureText),
                new PropertyMetadata(default, (d, e) => InvalidateShape(d, e, true))
            );

        public SecureString Text
        {
            get { return (SecureString)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }


        public static readonly DependencyProperty TextFontSizeProperty =
            DependencyProperty.Register(
                nameof(TextFontSize),
                typeof(double),
                typeof(SecureText),
                new PropertyMetadata(14D, (d, e) => InvalidateShape(d, e, true))
            );

        public double TextFontSize
        {
            get { return (double)GetValue(TextFontSizeProperty); }
            set { SetValue(TextFontSizeProperty, value); }
        }


        public static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register(
                nameof(TextAlignment),
                typeof(TextAlignment),
                typeof(SecureText),
                new PropertyMetadata(TextAlignment.Left, (d, e) => InvalidateShape(d, e, true))
            );

        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        public static readonly DependencyProperty TextFillProperty =
            DependencyProperty.Register(
                nameof(TextFill),
                typeof(Brush),
                typeof(SecureText),
                new PropertyMetadata(System.Windows.Media.Brushes.Transparent, (d, e) => InvalidateShape(d, e))
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
                typeof(SecureText),
                new PropertyMetadata(System.Windows.Media.Brushes.Black, (d, e) => InvalidateShape(d, e))
            );

        public Brush TextStroke
        {
            get { return (Brush)GetValue(TextStrokeProperty); }
            set { SetValue(TextStrokeProperty, value); }
        }


        public static readonly DependencyProperty TextStrokeThicknessProperty =
            DependencyProperty.Register(
                nameof(TextStrokeThickness),
                typeof(double),
                typeof(SecureText),
                new PropertyMetadata(1D, (d, e) => InvalidateShape(d, e, true))
            );

        public double TextStrokeThickness
        {
            get { return (double)GetValue(TextStrokeThicknessProperty); }
            set { SetValue(TextStrokeThicknessProperty, value); }
        }


        public static readonly DependencyProperty TextStrokeOffsetProperty =
            DependencyProperty.Register(
                nameof(TextStrokeOffset),
                typeof(Point),
                typeof(SecureText),
                new PropertyMetadata(new Point(0, 0), (d, e) => InvalidateShape(d, e, true))
            );

        public Point TextStrokeOffset
        {
            get { return (Point)GetValue(TextStrokeOffsetProperty); }
            set { SetValue(TextStrokeOffsetProperty, value); }
        }



        public static readonly DependencyProperty TypefaceProperty =
            DependencyProperty.Register(
                nameof(Typeface),
                typeof(Typeface),
                typeof(SecureText),
                new PropertyMetadata(default, (d, e) => InvalidateShape(d, e, true))
            );

        public Typeface Typeface
        {
            get { return (Typeface)GetValue(TypefaceProperty); }
            set { SetValue(TypefaceProperty, value); }
        }




        static void InvalidateShape(DependencyObject d, DependencyPropertyChangedEventArgs e, bool invalidateSize = false)
        {
            if (d is not SecureText shape)
                return;

            if (invalidateSize)
                shape.InvalidateMeasure();

            shape.InvalidateVisual();
        }


        protected override Geometry DefiningGeometry
        {
            get
            {
                // todo: possible cache and invalidate on text change
                var height = 0D;
                var width = 0D;

                if (Text != null && Text.Length > 0)
                {
                    using var pinned = new PinnedString(Text);
                    var formattedText = GenerateForamattedText(pinned.String);
                    height = formattedText.Height;
                    width = formattedText.Width;
                }

                return new RectangleGeometry(new Rect(0, 0, width, height));
            }
        }

        static readonly Point _zeroPoint = new Point(0, 0);

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (Text == null || Text.Length < 1)
                return;

            try
            {
                using var pinnedText = new PinnedString(Text);
                var formattedText = GenerateForamattedText(pinnedText.String);

                var textLocation = _zeroPoint;
                if (TextAlignment == TextAlignment.Center)
                {
                    textLocation.X = (ActualWidth - formattedText.Width) / 2;
                }

                var outlinePoint = new Point(
                    textLocation.X + TextStrokeOffset.X,
                    textLocation.Y + TextStrokeOffset.Y
                );

                var textGeometry = formattedText.BuildGeometry(outlinePoint);
                drawingContext.DrawText(formattedText, textLocation);
                drawingContext.DrawGeometry(TextFill, new Pen(TextStroke, TextStrokeThickness), textGeometry);
            }
            catch (Exception ex)
            {
                Log.Exception(nameof(SecureText), ex);
            }
        }

        FormattedText GenerateForamattedText(string? text)
        {
            var ftText = new FormattedText(
                text ?? string.Empty,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                Typeface,
                TextFontSize,
                TextFill,
                1.0
            );

            ftText.TextAlignment = TextAlignment;

            if (!double.IsNaN(Width))
                ftText.MaxTextWidth = Width; // One has been set rather than filling parent

            return ftText;
        }
    }
}
