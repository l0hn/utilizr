using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Utilizr.WPF
{
    public partial class BusyCircle : UserControl
    {
        public static readonly DependencyProperty ForegroundBrushProperty =
            DependencyProperty.Register(
                nameof(ForegroundBrush),
                typeof(Brush),
                typeof(BusyCircle),
                new PropertyMetadata(
                    new SolidColorBrush(Colors.Gray)
                )
            );

        public Brush ForegroundBrush
        {
            get { return (Brush)GetValue(ForegroundBrushProperty); }
            set { SetValue(ForegroundBrushProperty, value); }
        }


        public static readonly DependencyProperty IsBusyProperty =
            DependencyProperty.Register(
                nameof(IsBusy),
                typeof(bool),
                typeof(BusyCircle),
                new PropertyMetadata(false)
            );

        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }

        public BusyCircle()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
        }
    }
}