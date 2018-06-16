using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DynamicAdjustmentGridSizeExample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public double MainGridHeight;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void MainGrid_Loaded(object sender, RoutedEventArgs e)
        {
            MainGridHeight = GridRow0.ActualHeight + GridRow1.ActualHeight;
        }

        private void MainGrid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            Point p = e.GetCurrentPoint(fe).Position;
            PointerPoint ptrPt = e.GetCurrentPoint(fe);
            
            if (p.Y < GridRow0.Height + 10 && p.Y > GridRow0.Height - 10 && ptrPt.Properties.IsLeftButtonPressed)
            {
                GridRow0.Height = p.Y;
                GridRow1.Height = MainGridHeight - GridRow0.Height;
            }
        }
    }
}
