using System.Windows;
using System.Windows.Automation;

namespace Utilizr.WPF.Attached
{
    public static class UIElementNarratorBehaviour
    {
        public static readonly DependencyProperty NarratorDescriptionProperty =
            DependencyProperty.RegisterAttached(
                "NarratorDescription",
                typeof(string),
                typeof(UIElementNarratorBehaviour),
                new PropertyMetadata(string.Empty, OnNarratorDescriptionChanged)
            );

        public static string GetNarratorDescription(DependencyObject obj)
        {
            return (string)obj.GetValue(NarratorDescriptionProperty);
        }

        public static void SetNarratorDescription(DependencyObject obj, string value)
        {
            obj.SetValue(NarratorDescriptionProperty, value);
        }

        private static void OnNarratorDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element && e.NewValue is string narratorDescription)
            {
                AutomationProperties.SetName(element, narratorDescription);
                AutomationProperties.SetHelpText(element, narratorDescription);
            }
        }
    }
}
