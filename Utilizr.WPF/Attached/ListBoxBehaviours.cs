using System.Windows;
using System.Windows.Controls;

namespace Utilizr.WPF.Attached
{
    public static class ListBoxBehaviours
    {
        public static readonly DependencyProperty SlideOnSelectedIndexChangedProperty =
            DependencyProperty.RegisterAttached(
                "SlideOnSelectedIndexChanged",
                typeof(bool),
                typeof(ListBox),
                new PropertyMetadata(false, OnSlideOnSelectedIndexChangedChanged)
            );

        static void OnSlideOnSelectedIndexChangedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ListBox listBox)
                return;

            if ((bool)e.NewValue)
            {
                listBox.SelectionChanged += ListBox_SelectionChanged;
            }
            else
            {
                listBox.SelectionChanged -= ListBox_SelectionChanged;
            }
        }

        public static bool GetSlideOnSelectedIndexChanged(DependencyObject obj)
        {
            return (bool)obj.GetValue(SlideOnSelectedIndexChangedProperty);
        }

        public static void SetSlideOnSelectedIndexChanged(DependencyObject obj, bool value)
        {
            obj.SetValue(SlideOnSelectedIndexChangedProperty, value);
        }

        static void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListBox listBox)
                return;

            if (!listBox.IsLoaded)
                return;

            object? oldUiModel = null;
            if (e.RemovedItems.Count > 0)
                oldUiModel = e.RemovedItems[0];

            object? newUiModel = null;
            if (e.AddedItems.Count > 0)
                newUiModel = e.AddedItems[0];

            if (oldUiModel == null || newUiModel == null)
                return;


            // todo: some nice slide animation for attached ListBox property 'SlideOnSelectedIndexChanged'.

            //var oldContent = (UIElement)listBox.ItemContainerGenerator.ContainerFromItem(oldUiModel);
            //var oldIndex = listBox.ItemContainerGenerator.IndexFromContainer(oldContent);
            //var newContent = (UIElement)listBox.ItemContainerGenerator.ContainerFromItem(newUiModel);
            //var newIndex = listBox.ItemContainerGenerator.IndexFromContainer(newContent);

            //if (newIndex == 0)
            //{
            //    // edge case to scroll back to start
            //}

            //var marginCoefficient = newIndex - oldIndex;
            // todo: storyboard to slide content in ListBox

            listBox.ScrollIntoView(newUiModel);
        }
    }
}
