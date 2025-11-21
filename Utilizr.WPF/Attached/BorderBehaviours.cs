using System.Windows;
using System.Windows.Controls;

namespace Utilizr.WPF.Attached
{
    public static class BorderBehaviours
    {
        public static readonly DependencyProperty AutoRoundCornersProperty =
            DependencyProperty.RegisterAttached(
                "AutoRoundCorners",
                typeof(bool),
                typeof(BorderBehaviours),
                new PropertyMetadata(false, OnAutoRoundCornersChanged));

        public static void SetAutoRoundCorners(DependencyObject element, bool value)
        {
            element.SetValue(AutoRoundCornersProperty, value);
        }

        public static bool GetAutoRoundCorners(DependencyObject element)
        {
            return (bool)element.GetValue(AutoRoundCornersProperty);
        }

        private static void OnAutoRoundCornersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Border border)
            {
                if ((bool)e.NewValue)
                {
                    border.SizeChanged += Border_SizeChanged;
                    UpdateCornerRadius(border);
                }
                else
                {
                    border.SizeChanged -= Border_SizeChanged;
                }
            }
        }

        private static void Border_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is Border border)
            {
                UpdateCornerRadius(border);
            }
        }

        private static void UpdateCornerRadius(Border border)
        {
            double radius = border.ActualHeight / 2;
            border.CornerRadius = new CornerRadius(radius);
        }
    }
}
