using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Utilizr.Logging;

namespace Utilizr.WPF.Attached
{
    public static class ListBoxBehaviour
    {
        public interface IListBoxCheckBoxToggleModel
        {
            void ToggleCheckState();
        }

        #region EnableCheckBoxToggle
        public static readonly DependencyProperty EnableCheckBoxToggleProperty =
            DependencyProperty.RegisterAttached(
                "EnableCheckBoxToggle",
                typeof(bool),
                typeof(ListBoxBehaviour),
                new PropertyMetadata(false, OnEnableCheckBoxToggleChanged)
            );

        public static bool GetEnableCheckBoxToggle(DependencyObject element)
        {
            return (bool)element.GetValue(EnableCheckBoxToggleProperty);
        }

        public static void SetEnableCheckBoxToggle(DependencyObject element, bool value)
        {
            element.SetValue(EnableCheckBoxToggleProperty, value);
        }

        private static void OnEnableCheckBoxToggleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListBox listBox)
            {
                if ((bool)e.NewValue)
                    listBox.PreviewKeyDown += ListBox_PreviewKeyDown;
                else
                    listBox.PreviewKeyDown -= ListBox_PreviewKeyDown;
            }
        }

        private static void ListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(sender is ListBox listBox))
                return;

            if (!(e.Key == Key.Space))
                return;

            if (!(listBox.SelectedItem is IListBoxCheckBoxToggleModel checkBoxToggleModel))
                return;

            if (checkBoxToggleModel != null)
            {
                checkBoxToggleModel.ToggleCheckState();
                e.Handled = true;
            }
        }
        #endregion



        #region SyncSelectedItemInView Attached Property
        public static readonly DependencyProperty SyncSelectedItemInViewProperty =
            DependencyProperty.RegisterAttached(
                "SyncSelectedItemInView",
                typeof(bool),
                typeof(ListBoxBehaviour),
                new PropertyMetadata(false, OnSyncSelectedItemInViewChanged)
            );

        private static void OnSyncSelectedItemInViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ListBox listBox))
                return;

            if ((bool)e.NewValue)
                listBox.SelectionChanged += ListBox_SyncSelectionChanged;
            else
                listBox.SelectionChanged -= ListBox_SyncSelectionChanged;
        }

        private static void ListBox_SyncSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ListBox listBox))
                return;

            try
            {
                var newSelection = listBox.SelectedItem;
                listBox.ScrollIntoView(newSelection);
                var container = listBox.ItemContainerGenerator.ContainerFromItem(listBox.SelectedItem) as ListBoxItem;
                container?.Focus();
            }
            catch (Exception ex)
            {
                Log.Exception(nameof(ListBoxBehaviour), ex);
            }
        }

        public static bool GetSyncSelectedItemInView(ListBox obj)
        {
            return (bool)obj.GetValue(SyncSelectedItemInViewProperty);
        }

        public static void SetSyncSelectedItemInView(ListBox obj, bool val)
        {
            obj.SetValue(SyncSelectedItemInViewProperty, val);
        }
        #endregion



        #region SlideOnSelectedIndexChanged
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
        #endregion
    }
}