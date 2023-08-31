using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Utilizr.WPF.Attached
{
    public static class ListViewBehaviour
    {

        #region AutoScrollSelectedItem Attached Property
        public static readonly DependencyProperty AutoScrollSelectedItemProperty =
            DependencyProperty.RegisterAttached(
                "AutoScrollSelectedItem",
                typeof(bool),
                typeof(ListViewBehaviour),
                new PropertyMetadata(
                    false,
                    (d, e) =>
                    {
                        if (d is not ListView listView)
                            return;

                        void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
                        {
                            if (e.AddedItems.Count == 0)
                                return;

                            var last = e.AddedItems[e.AddedItems.Count - 1];
                            listView.ScrollIntoView(last);
                        };

                        if (e.NewValue != null && (bool)e.NewValue)
                        {
                            listView.SelectionChanged += listView_SelectionChanged;
                        }
                        
                        if (e.OldValue != null && (bool)e.OldValue)
                        {
                            listView.SelectionChanged -= listView_SelectionChanged;
                        }
                    }
                )
            );

        public static bool GetAutoScrollSelectedItem(ListView obj)
        {
            return (bool)obj.GetValue(AutoScrollSelectedItemProperty);
        }

        public static void SetAutoScrollSelectedItem(ListView obj, bool val)
        {
            obj.SetValue(AutoScrollSelectedItemProperty, val);
        }
        #endregion



        // Whether this listview has sorting support
        #region IsSortable Attached Property
        public static readonly DependencyProperty IsSortableProperty =
            DependencyProperty.RegisterAttached(
                "IsSortable",
                typeof(bool),
                typeof(ListViewBehaviour),
                new UIPropertyMetadata(false)
            );

        public static bool GetIsSortable(ListView obj)
        {
            return (bool)obj.GetValue(IsSortableProperty);
        }

        public static void SetIsSortable(ListView obj, bool val)
        {
            obj.SetValue(IsSortableProperty, val);
        }
        #endregion


        // Property name within model object which will be used for sorting
        #region SortPropertyName Attached Property
        public static readonly DependencyProperty SortPropertyNameProperty =
            DependencyProperty.RegisterAttached(
                "SortPropertyName",
                typeof(string),
                typeof(ListViewBehaviour),
                new UIPropertyMetadata(null)
            );

        public static string GetSortPropertyName(GridViewColumn obj)
        {
            return (string)obj.GetValue(SortPropertyNameProperty);
        }

        public static void SetSortPropertyName(GridViewColumn obj, string val)
        {
            obj.SetValue(SortPropertyNameProperty, val);
        }
        #endregion


        // Here for GUI state
        #region SortDirection Attached Property
        public static readonly DependencyProperty SortDirectionProperty =
            DependencyProperty.RegisterAttached(
                "SortDirection",
                typeof(ListSortDirection),
                typeof(ListViewBehaviour),
                new UIPropertyMetadata(ListSortDirection.Ascending)
            );

        public static ListSortDirection GetSortDirection(GridViewColumn obj)
        {
            return (ListSortDirection)obj.GetValue(SortDirectionProperty);
        }

        public static void SetSortDirection(GridViewColumn obj, ListSortDirection val)
        {
            obj.SetValue(SortDirectionProperty, val);
        }
        #endregion


        // Here for GUI state
        #region IsSortingByThisColumn Attached Property
        public static readonly DependencyProperty IsSortingByThisColumnProperty =
            DependencyProperty.RegisterAttached(
                "IsSortingByThisColumn",
                typeof(bool),
                typeof(ListViewBehaviour),
                new UIPropertyMetadata(false)
            );

        public static bool GetIsSortingByThisColumn(GridViewColumn obj)
        {
            return (bool)obj.GetValue(IsSortingByThisColumnProperty);
        }

        public static void SetIsSortingByThisColumn(GridViewColumn obj, bool val)
        {
            obj.SetValue(IsSortingByThisColumnProperty, val);
        }
        #endregion


        // Will automatically sort based on the columns that have set a SortPropertyName
        #region AutoSort Attached Property
        public static readonly DependencyProperty AutoSortProperty =
            DependencyProperty.RegisterAttached(
                "AutoSort",
                typeof(bool),
                typeof(ListViewBehaviour),
                new UIPropertyMetadata(
                    false,
                    (d, e) =>
                    {
                        if (d is not ListView listView)
                            return;

                        //if (GetHeaderClickedCommand(listView) != null)
                        //    return; // Don't change click handler if a command is set

                        bool oldValue = (bool)e.OldValue;
                        bool newValue = (bool)e.NewValue;
                        if (oldValue && !newValue)
                        {
                            listView.RemoveHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                        }
                        if (!oldValue && newValue)
                        {
                            listView.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                        }
                    }
                )
            );

        public static bool GetAutoSort(ListView obj)
        {
            return (bool)obj.GetValue(AutoSortProperty);
        }

        public static void SetAutoSort(ListView obj, bool val)
        {
            obj.SetValue(AutoSortProperty, val);
        }
        #endregion


        // Custom sort logic rather than the default
        #region HeaderClickedCommand Attached Property
        public static readonly DependencyProperty HeaderClickedCommandProperty =
            DependencyProperty.RegisterAttached(
                "HeaderClickedCommand",
                typeof(ICommand),
                typeof(ListViewBehaviour),
                new UIPropertyMetadata(
                    null,
                    (d, e) =>
                    {
                        if (d is not ListView listView)
                            return;

                        if (e.OldValue != null && e.NewValue == null)
                        {
                            listView.RemoveHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                        }

                        if (e.OldValue == null && e.NewValue != null)
                        {
                            listView.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                        }
                    }
                )
            );

        public static ICommand GetHeaderClickedCommand(ListView obj)
        {
            return (ICommand)obj.GetValue(HeaderClickedCommandProperty);
        }

        public static void SetHeaderClickedCommand(ListView obj, ICommand val)
        {
            obj.SetValue(HeaderClickedCommandProperty, val);
        }
        #endregion



        static void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (e?.OriginalSource is not GridViewColumnHeader clickedHeader)
                return;

            //var sortInfo = GetSortInfo(clickedHeader.Column);
            var propertyName = GetSortPropertyName(clickedHeader.Column);
            if (string.IsNullOrEmpty(propertyName))
                return;

            var listView = GetVisualAncestor<ListView>(clickedHeader);
            if (listView == null)
                return;

            var direction = GetSortDirection(clickedHeader.Column);

            var cmd = GetHeaderClickedCommand(listView);
            if (cmd != null)
            {
                if (cmd.CanExecute(propertyName) == true)
                    cmd.Execute(propertyName);
            }
            else
            {
                ApplySort(listView.Items, propertyName, ref direction);
            }


            // update other GridViewColumns for the given ListView
            if (listView.View is not GridView gridView)
                return;

            foreach (var gridViewColumn in gridView.Columns)
            {
                bool sortingBy = gridViewColumn == clickedHeader.Column;
                SetIsSortingByThisColumn(gridViewColumn, sortingBy);

                if (!sortingBy)
                    continue;

                SetSortDirection(gridViewColumn, direction);
            }
        }

        static T? GetVisualAncestor<T>(DependencyObject reference) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(reference);
            while (parent is not T)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            if (parent != null)
                return (T)parent;

            return null;
        }

        static void ApplySort(ICollectionView view, string propertyName, ref ListSortDirection direction)
        {
            view.SortDescriptions.Clear();

            direction = direction == ListSortDirection.Ascending
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;

            if (!string.IsNullOrEmpty(propertyName))
            {
                view.SortDescriptions.Add(new SortDescription(propertyName, direction));
            }
        }
    }
}