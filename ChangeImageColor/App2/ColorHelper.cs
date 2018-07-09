using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace App2
{
    public static class MyColorHelper
    {
        public static Brush GetRainbowBrush()
        {
            LinearGradientBrush lgb = new LinearGradientBrush();
            GradientStopCollection gradientStops = new GradientStopCollection();
            GradientStop stop1 = new GradientStop();
            GradientStop stop2 = new GradientStop();
            GradientStop stop3 = new GradientStop();
            GradientStop stop4 = new GradientStop();
            GradientStop stop5 = new GradientStop();
            GradientStop stop6 = new GradientStop();

            stop1.Color = Windows.UI.Colors.Red;
            stop2.Color = Windows.UI.Colors.Yellow;
            stop3.Color = Windows.UI.Colors.LightGreen;
            stop4.Color = Windows.UI.Colors.Aqua;
            stop5.Color = Windows.UI.Colors.Blue;
            stop6.Color = Windows.UI.Colors.Purple;
            stop1.Offset = 0.1;
            stop2.Offset = 0.25;
            stop3.Offset = 0.4;
            stop4.Offset = 0.6;
            stop5.Offset = 0.75;
            stop6.Offset = 0.9;
            gradientStops.Add(stop1);
            gradientStops.Add(stop2);
            gradientStops.Add(stop3);
            gradientStops.Add(stop4);
            gradientStops.Add(stop5);
            gradientStops.Add(stop6);
            lgb.GradientStops = gradientStops;
            lgb.StartPoint = new Point(0, 0);
            lgb.EndPoint = new Point(1, 0);
            return lgb;
        }
    }
}