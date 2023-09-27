using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Utilizr.WPF.Extension;

namespace Utilizr.WPF.Controls
{
    public partial class TextBlockFilePath : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(TextBlockFilePath),
                new PropertyMetadata(null, InvalidateTrimmedText)
            );

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private string _textLeft;
        public string TextLeft
        {
            get { return _textLeft; }
            private set
            {
                _textLeft = value;
                OnPropertyChanged(nameof(TextLeft), nameof(TextFilePath));
            }
        }

        private string _textRight;
        public string TextRight
        {
            get { return _textRight; }
            private set
            {
                _textRight = value;
                OnPropertyChanged(nameof(TextRight), nameof(TextFilePath));
            }
        }

        public string TextFilePath => $"{TextLeft}{TrimmingString}{TextRight}";

        private bool _isTrimmed;
        public bool IsTrimmed
        {
            get { return _isTrimmed; }
            private set
            {
                _isTrimmed = value;
                OnPropertyChanged(nameof(IsTrimmed));
            }
        }

        private bool _isMultiline;
        public bool IsMultiline
        {
            get { return _isMultiline; }
            private set
            {
                _isMultiline = value;
                OnPropertyChanged(nameof(IsMultiline));
            }
        }

        public static readonly DependencyProperty TrimmingStringProperty =
            DependencyProperty.Register(
                nameof(TrimmingString),
                typeof(string),
                typeof(TextBlockFilePath),
                new PropertyMetadata("...", InvalidateTrimmedText)
            );

        /// <summary>
        /// The string shown in the middle when trimming has occurred.
        /// </summary>
        public string TrimmingString
        {
            get { return (string)GetValue(TrimmingStringProperty); }
            set { SetValue(TrimmingStringProperty, value); }
        }

        static void InvalidateTrimmedText(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TextBlockFilePath)?.UpdateTrimmedText();
        }
     

        public static readonly DependencyProperty FudgeFactorProperty =
            DependencyProperty.Register(
                nameof(FudgeFactor),
                typeof(double),
                typeof(TextBlockFilePath),
                new PropertyMetadata(0.05, InvalidateTrimmedText)
            );

        /// <summary>
        /// Most likely not using a monospace font but using character count to attempt removing
        /// middle of the file path. This is the percentage of extra characters removed from the
        /// file path to ensure each side is smaller than the available space.
        /// </summary>
        public double FudgeFactor
        {
            get { return (double)GetValue(FudgeFactorProperty); }
            set { SetValue(FudgeFactorProperty, value); }
        }



        public static readonly DependencyProperty MinimumCharsToKeepProperty =
            DependencyProperty.Register(
                nameof(MinimumCharsToKeep),
                typeof(int),
                typeof(TextBlockFilePath),
                new PropertyMetadata(50, InvalidateTrimmedText)
            );

        /// <summary>
        /// Never produce a string less than the specified value.
        /// Note: Will also solve issues with very large strings not trimming correctly.
        /// </summary>
        public int MinimumCharsToKeep
        {
            get { return (int)GetValue(MinimumCharsToKeepProperty); }
            set { SetValue(MinimumCharsToKeepProperty, value); }
        }



        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register(
                nameof(Rows),
                typeof(int),
                typeof(TextBlockFilePath),
                new PropertyMetadata(1, InvalidateTrimmedText)
            );

        /// <summary>
        /// The number of rows which the Text is allowed to wrap over before being clipped
        /// </summary>
        public int Rows
        {
            get { return (int)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }


        public static readonly DependencyProperty TextHorizontalAlignmentProperty =
            DependencyProperty.Register(
                nameof(TextHorizontalAlignment),
                typeof(HorizontalAlignment),
                typeof(TextBlockFilePath),
                new PropertyMetadata(HorizontalAlignment.Center)
            );

        public HorizontalAlignment TextHorizontalAlignment
        {
            get { return (HorizontalAlignment)GetValue(TextHorizontalAlignmentProperty); }
            set { SetValue(TextHorizontalAlignmentProperty, value); }
        }


        public TextBlockFilePath()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;

            Loaded += (s, e) => UpdateTrimmedText();
            SizeChanged += (s, e) => UpdateTrimmedText();
            this.RegisterDpiChanged(UpdateTrimmedText);
        }

        void UpdateTrimmedText()
        {
            string? localText = Text?.ToString(); // copy, not same reference

            var widthAdjusted = ActualWidth;
            var heightAdjusted = ActualHeight;
            if (widthAdjusted < 1 || 
                double.IsInfinity(widthAdjusted) ||
                double.IsNaN(widthAdjusted) ||
                string.IsNullOrEmpty(localText) ||
                double.IsInfinity(heightAdjusted) ||
                double.IsNaN(heightAdjusted))
            {
                // not yet loaded
                IsTrimmed = false;
                return;
            }

            var formattedText = GenerateFormattedText(localText);
            if (formattedText.Width < widthAdjusted)
            {
                IsTrimmed = false;
                return;
            }

            if (formattedText.Width < widthAdjusted * Rows)
            {
                IsTrimmed = false;
                return;
            }


            IsTrimmed = true;
            IsMultiline = Rows > 1;
            // TODO left and right split is done on char count, rather than size. 
            // Ideally needs to be size since 00000 is longer than lllll in proportional font.
            var charCount = localText.Length + TrimmingString.Length;
            var trimPercent = ((Rows * widthAdjusted) / formattedText.Width);

            var adjustedPercent = trimPercent - FudgeFactor > 0
                ? trimPercent - FudgeFactor
                : trimPercent;

            var charsToKeep = Math.Max((int)(adjustedPercent * charCount), MinimumCharsToKeep);
            var charsToRemove = Math.Max(charCount - charsToKeep, 0);

            if (charsToKeep < 1)
            {
                // possible early layout call?
                TextLeft = string.Empty;
                TextRight = string.Empty;
                return;
            }

            var middleChar = charCount / 2;
            var startTrim = middleChar - (charsToRemove / 2);
            var endTrim = middleChar + (charsToRemove / 2);

            FormattedText? left = null;
            FormattedText? right = null;
            try
            {
                left = GenerateFormattedText(localText.Substring(0, startTrim));
                right = GenerateFormattedText(localText.Substring(endTrim));

                // In extreme cases one side can be much longer if not using a monospace font.
                // Measure again and remove a few extra chars if out by fudge factor percent.
                double percentageOff = left.Width > right.Width
                    ? (left.Width / right.Width)
                    : (right.Width / left.Width);

                if (FudgeFactor + 1 > percentageOff)
                {
                    TextLeft = left.Text;
                    TextRight = right.Text;
                    return;
                }

                if (left.Width > right.Width)
                {
                    var leftEndTrim = (int)(left.Text.Length / percentageOff);
                    TextLeft = leftEndTrim >= left.Text.Length - 1
                        ? left.Text // fail safe
                        : left.Text.Substring(0, leftEndTrim);
                    TextRight = right.Text;
                }
                else
                {
                    var rightRemoveChars = (int)(right.Text.Length / percentageOff);
                    var rightStartTrim = right.Text.Length - rightRemoveChars;
                    TextLeft = left.Text;
                    TextRight = rightStartTrim >= right.Text.Length - 1
                        ? right.Text // fail safe
                        : right.Text.Substring(rightStartTrim);
                }
            }
            catch (Exception ex)
            {
                ex.Data["WidthAdjusted"] = $"{widthAdjusted:N0}";
                ex.Data["HeightAdjusted"] = $"{heightAdjusted:N0}";
                ex.Data["Rows"] = $"{Rows:N0}";
                ex.Data["FormattedWidth"] = $"{formattedText.Width:N0}";
                ex.Data["trimPercent"] = $"{trimPercent:N4}";
                ex.Data["adjustedPercent"] = $"{adjustedPercent:N4}";
                ex.Data["charCount"] = $"{charCount:N0}";
                ex.Data["charsToKeep"] = $"{charsToKeep:N0}";
                ex.Data["charsToRemove"] = $"{charsToRemove:N0}";
                ex.Data["middleChar"] = $"{middleChar:N0}";
                ex.Data["startTrim"] = $"{startTrim:N0}";
                ex.Data["endTrim"] = $"{endTrim:N0}";
                ex.Data["left"] = left?.Text;
                ex.Data["right"] = right?.Text;
                ex.Data["localText"] = localText;
                throw;
            }

            //System.Diagnostics.Debug.WriteLine(
            //    $"WidthAdjusted={widthAdjusted:N0}, " +
            //    $"HeightAdjusted={heightAdjusted:N0}, " +
            //    $"Rows={rows:N0}, " +
            //    $"FormattedWidth={formattedText.Width:N0}, " +
            //    $"trimPercent={trimPercent:N4}, " +
            //    $"adjustedPercent={adjustedPercent:N4}, " +
            //    $"charCount={charCount:N0}, " +
            //    $"charsToKeep={charsToKeep:N0}, " +
            //    $"charsToRemove={charsToRemove:N0}, " +
            //    $"middleChar={middleChar:N0}, " +
            //    $"startTrim={startTrim:N0}, " +
            //    $"endTrim={endTrim:N0}, " +
            //    $"{Text} => {TextFilePath}"
            //);
        }

        private FormattedText GenerateFormattedText(string text)
        {
            return new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                FontSize,
                Foreground,
                new NumberSubstitution(),
                this.GetDpi()
            );
        }

        protected virtual void OnPropertyChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}