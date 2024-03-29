﻿// -- FILE ------------------------------------------------------------------
// name       : ListViewLayoutManager.cs
// created    : Jani Giannoudis - 2008.03.27
// language   : c#
// environment: .NET 3.0
// copyright  : (c) 2008-2012 by Itenso GmbH, Switzerland
// --------------------------------------------------------------------------
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.ComponentModel;

namespace Itenso.Windows.Controls.ListViewLayout
{
    public class ListViewLayoutManager
    {
        public static readonly DependencyProperty EnabledProperty = 
            DependencyProperty.RegisterAttached(
                "Enabled",
                typeof(bool),
                typeof(ListViewLayoutManager),
                new FrameworkPropertyMetadata(
                    new PropertyChangedCallback(OnLayoutManagerEnabledChanged)
                )
            );

        public ListView ListView { get; private set; }
        public ScrollBarVisibility VerticalScrollBarVisibility { get; set; } = ScrollBarVisibility.Auto;

        private ScrollViewer _scrollViewer;
        private bool _loaded;
        private bool _resizing;
        private Cursor _resizeCursor;
        private GridViewColumn _autoSizedColumn;

        private const double zeroWidthRange = 0.1;

        public ListViewLayoutManager(ListView listView)
        {
            if (listView == null)
                throw new ArgumentNullException(nameof(listView));

            ListView = listView;
            ListView.Loaded += new RoutedEventHandler(ListViewLoaded);
            ListView.Unloaded += new RoutedEventHandler(ListViewUnloaded);
        }

        public static void SetEnabled(DependencyObject dependencyObject, bool enabled)
        {
            dependencyObject.SetValue(EnabledProperty, enabled);
        }

        public void Refresh()
        {
            InitColumns();
            DoResizeColumns();
        }

        private void RegisterEvents(DependencyObject start)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(start); i++)
            {
                var childVisual = VisualTreeHelper.GetChild(start, i) as Visual;
                if (childVisual is Thumb)
                {
                    GridViewColumn gridViewColumn = FindParentColumn(childVisual);
                    if (gridViewColumn != null)
                    {
                        var thumb = childVisual as Thumb;
                        if (ProportionalColumn.IsProportionalColumn(gridViewColumn) ||
                            FixedColumn.IsFixedColumn(gridViewColumn) || IsFillColumn(gridViewColumn))
                        {
                            thumb.IsHitTestVisible = false;
                        }
                        else
                        {
                            thumb.PreviewMouseMove += new MouseEventHandler(ThumbPreviewMouseMove);
                            thumb.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(ThumbPreviewMouseLeftButtonDown);
                            DependencyPropertyDescriptor.FromProperty(GridViewColumn.WidthProperty,typeof(GridViewColumn))
                                .AddValueChanged(gridViewColumn, GridColumnWidthChanged);
                        }
                    }
                }
                else if (childVisual is GridViewColumnHeader)
                {
                    GridViewColumnHeader columnHeader = childVisual as GridViewColumnHeader;
                    columnHeader.SizeChanged += new SizeChangedEventHandler(GridColumnHeaderSizeChanged);
                }
                else if (_scrollViewer == null && childVisual is ScrollViewer)
                {
                    _scrollViewer = childVisual as ScrollViewer;
                    _scrollViewer.ScrollChanged += new ScrollChangedEventHandler(ScrollViewerScrollChanged);
                    // assume we do the regulation of the horizontal scrollbar
                    _scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    _scrollViewer.VerticalScrollBarVisibility = VerticalScrollBarVisibility;
                }

                RegisterEvents(childVisual);  // recursive
            }
        }

        private void UnregisterEvents(DependencyObject start)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(start); i++)
            {
                var childVisual = VisualTreeHelper.GetChild(start, i) as Visual;
                if (childVisual is Thumb)
                {
                    GridViewColumn gridViewColumn = FindParentColumn(childVisual);
                    if (gridViewColumn != null)
                    {
                        var thumb = childVisual as Thumb;
                        if (ProportionalColumn.IsProportionalColumn(gridViewColumn) ||
                            FixedColumn.IsFixedColumn(gridViewColumn) || IsFillColumn(gridViewColumn))
                        {
                            thumb.IsHitTestVisible = true;
                        }
                        else
                        {
                            thumb.PreviewMouseMove -= new MouseEventHandler(ThumbPreviewMouseMove);
                            thumb.PreviewMouseLeftButtonDown -= new MouseButtonEventHandler(ThumbPreviewMouseLeftButtonDown);
                            DependencyPropertyDescriptor.FromProperty(GridViewColumn.WidthProperty, typeof(GridViewColumn))
                                .RemoveValueChanged(gridViewColumn, GridColumnWidthChanged);
                        }
                    }
                }
                else if (childVisual is GridViewColumnHeader)
                {
                    GridViewColumnHeader columnHeader = childVisual as GridViewColumnHeader;
                    columnHeader.SizeChanged -= new SizeChangedEventHandler(GridColumnHeaderSizeChanged);
                }
                else if (_scrollViewer == null && childVisual is ScrollViewer)
                {
                    _scrollViewer = childVisual as ScrollViewer;
                    _scrollViewer.ScrollChanged -= new ScrollChangedEventHandler(ScrollViewerScrollChanged);
                }

                UnregisterEvents(childVisual);  // recursive
            }
        }

        private GridViewColumn FindParentColumn(DependencyObject element)
        {
            if (element == null)
                return null;

            while (element != null)
            {
                GridViewColumnHeader gridViewColumnHeader = element as GridViewColumnHeader;
                if (gridViewColumnHeader != null)
                {
                    return (gridViewColumnHeader).Column;
                }
                element = VisualTreeHelper.GetParent(element);
            }

            return null;
        }

        private GridViewColumnHeader FindColumnHeader(DependencyObject start, GridViewColumn gridViewColumn)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(start); i++)
            {
                Visual childVisual = VisualTreeHelper.GetChild(start, i) as Visual;
                if (childVisual is GridViewColumnHeader)
                {
                    GridViewColumnHeader gridViewHeader = childVisual as GridViewColumnHeader;
                    if (gridViewHeader.Column == gridViewColumn)
                    {
                        return gridViewHeader;
                    }
                }
                GridViewColumnHeader childGridViewHeader = FindColumnHeader(childVisual, gridViewColumn);  // recursive
                if (childGridViewHeader != null)
                {
                    return childGridViewHeader;
                }
            }
            return null;
        }

        private void InitColumns()
        {
            GridView view = ListView.View as GridView;
            if (view == null)
            {
                return;
            }

            foreach (GridViewColumn gridViewColumn in view.Columns)
            {
                if (!RangeColumn.IsRangeColumn(gridViewColumn))
                {
                    continue;
                }

                double? minWidth = RangeColumn.GetRangeMinWidth(gridViewColumn);
                double? maxWidth = RangeColumn.GetRangeMaxWidth(gridViewColumn);
                if (!minWidth.HasValue && !maxWidth.HasValue)
                {
                    continue;
                }

                GridViewColumnHeader columnHeader = FindColumnHeader(ListView, gridViewColumn);
                if (columnHeader == null)
                {
                    continue;
                }

                double actualWidth = columnHeader.ActualWidth;
                if (minWidth.HasValue)
                {
                    columnHeader.MinWidth = minWidth.Value;
                    if (!double.IsInfinity(actualWidth) && actualWidth < columnHeader.MinWidth)
                    {
                        gridViewColumn.Width = columnHeader.MinWidth;
                    }
                }
                if (maxWidth.HasValue)
                {
                    columnHeader.MaxWidth = maxWidth.Value;
                    if (!double.IsInfinity(actualWidth) && actualWidth > columnHeader.MaxWidth)
                    {
                        gridViewColumn.Width = columnHeader.MaxWidth;
                    }
                }
            }
        }

        protected virtual void ResizeColumns()
        {
            GridView view = ListView.View as GridView;
            if (view == null || view.Columns.Count == 0)
            {
                return;
            }

            // listview width
            double actualWidth = double.PositiveInfinity;
            if (_scrollViewer != null)
            {
                actualWidth = _scrollViewer.ViewportWidth;
            }
            if (double.IsInfinity(actualWidth))
            {
                actualWidth = ListView.ActualWidth;
            }
            if (double.IsInfinity(actualWidth) || actualWidth <= 0)
            {
                return;
            }

            double resizeableRegionCount = 0;
            double otherColumnsWidth = 0;
            // determine column sizes
            foreach (GridViewColumn gridViewColumn in view.Columns)
            {
                if (ProportionalColumn.IsProportionalColumn(gridViewColumn))
                {
                    double? proportionalWidth = ProportionalColumn.GetProportionalWidth(gridViewColumn);
                    if (proportionalWidth != null)
                    {
                        resizeableRegionCount += proportionalWidth.Value;
                    }
                }
                else
                {
                    otherColumnsWidth += gridViewColumn.ActualWidth;
                }
            }

            if (resizeableRegionCount <= 0)
            {
                // no proportional columns present: commit the regulation to the scroll viewer
                if (_scrollViewer != null)
                {
                    _scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                }

                // search the first fill column
                GridViewColumn fillColumn = null;
                for (int i = 0; i < view.Columns.Count; i++)
                {
                    GridViewColumn gridViewColumn = view.Columns[i];
                    if (IsFillColumn(gridViewColumn))
                    {
                        fillColumn = gridViewColumn;
                        break;
                    }
                }

                if (fillColumn != null)
                {
                    double otherColumnsWithoutFillWidth = otherColumnsWidth - fillColumn.ActualWidth;
                    double fillWidth = actualWidth - otherColumnsWithoutFillWidth;
                    if (fillWidth > 0)
                    {
                        double? minWidth = RangeColumn.GetRangeMinWidth(fillColumn);
                        double? maxWidth = RangeColumn.GetRangeMaxWidth(fillColumn);

                        bool setWidth = !(minWidth.HasValue && fillWidth < minWidth.Value);
                        if (maxWidth.HasValue && fillWidth > maxWidth.Value)
                        {
                            setWidth = false;
                        }
                        if (setWidth)
                        {
                            if (_scrollViewer != null)
                            {
                                _scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                            }
                            fillColumn.Width = fillWidth;
                        }
                    }
                }
                return;
            }

            double resizeableColumnsWidth = actualWidth - otherColumnsWidth;
            if (resizeableColumnsWidth <= 0)
            {
                return; // missing space
            }

            // resize columns
            double resizeableRegionWidth = resizeableColumnsWidth / resizeableRegionCount;
            foreach (GridViewColumn gridViewColumn in view.Columns)
            {
                if (ProportionalColumn.IsProportionalColumn(gridViewColumn))
                {
                    double? proportionalWidth = ProportionalColumn.GetProportionalWidth(gridViewColumn);
                    if (proportionalWidth != null)
                    {
                        gridViewColumn.Width = proportionalWidth.Value * resizeableRegionWidth;
                    }
                }
            }
        }

        // returns the delta
        private double SetRangeColumnToBounds(GridViewColumn gridViewColumn)
        {
            double startWidth = gridViewColumn.Width;

            double? minWidth = RangeColumn.GetRangeMinWidth(gridViewColumn);
            double? maxWidth = RangeColumn.GetRangeMaxWidth(gridViewColumn);

            if ((minWidth.HasValue && maxWidth.HasValue) && (minWidth > maxWidth))
            {
                return 0; // invalid case
            }

            if (minWidth.HasValue && gridViewColumn.Width < minWidth.Value)
            {
                gridViewColumn.Width = minWidth.Value;
            }
            else if (maxWidth.HasValue && gridViewColumn.Width > maxWidth.Value)
            {
                gridViewColumn.Width = maxWidth.Value;
            }

            return gridViewColumn.Width - startWidth;
        }

        private bool IsFillColumn(GridViewColumn gridViewColumn)
        {
            if (gridViewColumn == null)
            {
                return false;
            }

            GridView view = ListView.View as GridView;
            if (view == null || view.Columns.Count == 0)
            {
                return false;
            }

            bool? isFillColumn = RangeColumn.GetRangeIsFillColumn(gridViewColumn);
            return isFillColumn.HasValue && isFillColumn.Value;
        }

        private void DoResizeColumns()
        {
            if (_resizing)
                return;

            _resizing = true;
            try
            {
                ResizeColumns();
            }
            finally
            {
                _resizing = false;
            }
        }

        private void ListViewLoaded(object sender, RoutedEventArgs e)
        {
            RegisterEvents(ListView);
            InitColumns();
            DoResizeColumns();
            _loaded = true;
        }

        private void ListViewUnloaded(object sender, RoutedEventArgs e)
        {
            if (!_loaded)
                return;

            UnregisterEvents(ListView);
            _loaded = false;
        }

        private void ThumbPreviewMouseMove(object sender, MouseEventArgs e)
        {
            Thumb thumb = sender as Thumb;
            if (thumb == null)
                return;

            GridViewColumn gridViewColumn = FindParentColumn(thumb);
            if (gridViewColumn == null)
                return;

            // suppress column resizing for proportional, fixed and range fill columns
            if (ProportionalColumn.IsProportionalColumn(gridViewColumn) ||
                FixedColumn.IsFixedColumn(gridViewColumn) ||
                IsFillColumn(gridViewColumn))
            {
                thumb.Cursor = null;
                return;
            }

            // check range column bounds
            if (thumb.IsMouseCaptured && RangeColumn.IsRangeColumn(gridViewColumn))
            {
                double? minWidth = RangeColumn.GetRangeMinWidth(gridViewColumn);
                double? maxWidth = RangeColumn.GetRangeMaxWidth(gridViewColumn);

                if ((minWidth.HasValue && maxWidth.HasValue) && (minWidth > maxWidth))
                    return; // invalid case

                if (_resizeCursor == null)
                    _resizeCursor = thumb.Cursor; // save the resize cursor

                if (minWidth.HasValue && gridViewColumn.Width <= minWidth.Value)
                {
                    thumb.Cursor = Cursors.No;
                }
                else if (maxWidth.HasValue && gridViewColumn.Width >= maxWidth.Value)
                {
                    thumb.Cursor = Cursors.No;
                }
                else
                {
                    thumb.Cursor = _resizeCursor; // between valid min/max
                }
            }
        }

        private void ThumbPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Thumb thumb = sender as Thumb;
            GridViewColumn gridViewColumn = FindParentColumn(thumb);

            // suppress column resizing for proportional, fixed and range fill columns
            if (ProportionalColumn.IsProportionalColumn(gridViewColumn) ||
                FixedColumn.IsFixedColumn(gridViewColumn) ||
                IsFillColumn(gridViewColumn))
            {
                e.Handled = true;
            }
        }

        private void GridColumnWidthChanged(object sender, EventArgs e)
        {
            if (!_loaded)
                return;

            GridViewColumn gridViewColumn = sender as GridViewColumn;

            // suppress column resizing for proportional and fixed columns
            if (ProportionalColumn.IsProportionalColumn(gridViewColumn) || FixedColumn.IsFixedColumn(gridViewColumn))
                return;

            // ensure range column within the bounds
            if (RangeColumn.IsRangeColumn(gridViewColumn))
            {
                // special case: auto column width - maybe conflicts with min/max range
                if (gridViewColumn != null && gridViewColumn.Width.Equals(double.NaN))
                {
                    _autoSizedColumn = gridViewColumn;
                    return; // handled by the change header size event
                }

                // ensure column bounds
                if (Math.Abs(SetRangeColumnToBounds(gridViewColumn) - 0) > zeroWidthRange)
                    return;
            }

            DoResizeColumns();
        }

        // handle autosized column
        private void GridColumnHeaderSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_autoSizedColumn == null)
                return;

            GridViewColumnHeader gridViewColumnHeader = sender as GridViewColumnHeader;
            if (gridViewColumnHeader != null && gridViewColumnHeader.Column == _autoSizedColumn)
            {
                if (gridViewColumnHeader.Width.Equals(double.NaN))
                {
                    // sync column with 
                    gridViewColumnHeader.Column.Width = gridViewColumnHeader.ActualWidth;
                    DoResizeColumns();
                }

                _autoSizedColumn = null;
            }
        }

        private void ScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_loaded && Math.Abs(e.ViewportWidthChange - 0) > zeroWidthRange)
                DoResizeColumns();
        }

        private static void OnLayoutManagerEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ListView listView = dependencyObject as ListView;
            if (listView != null)
            {
                bool enabled = (bool)e.NewValue;
                if (enabled)
                {
                    new ListViewLayoutManager(listView);
                }
            }
        }
    }
}