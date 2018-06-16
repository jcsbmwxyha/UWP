using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace AuraEditor.Common
{
    public static class AuraEditorColorHelper
    {
        public static Brush GetSmartBrush()
        {
            LinearGradientBrush lgb = new LinearGradientBrush();
            GradientStopCollection gradientStops = new GradientStopCollection();
            GradientStop stop1 = new GradientStop();
            GradientStop stop2 = new GradientStop();
            GradientStop stop3 = new GradientStop();
            stop1.Color = Windows.UI.Colors.Red;
            stop2.Color = Windows.UI.Colors.Yellow;
            stop3.Color = Windows.UI.Colors.LightGreen;
            stop1.Offset = 0.1;
            stop2.Offset = 0.4;
            stop3.Offset = 0.75;
            gradientStops.Add(stop1);
            gradientStops.Add(stop2);
            gradientStops.Add(stop3);
            lgb.GradientStops = gradientStops;
            lgb.StartPoint = new Point(0, 1);
            lgb.EndPoint = new Point(0, 0);
            return lgb;
        }

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

        public static Brush GetTimeLineBackgroundColor(bool odd)
        {
            Color color;
            if (odd)
                color = new Color { A = 127, R = 123, G = 211, B = 123 };
            else
                color = new Color { A = 127, R = 93, G = 181, B = 93 };
            return new SolidColorBrush(color);
        }

        public static Color HslToRgb(double a, double h, double s, double l)
        {
            double r = 0, g = 0, b = 0;
            if (l != 0)
            {
                if (s == 0)
                    r = g = b = l;
                else
                {
                    double temp2;
                    if (l < 0.5)
                        temp2 = l * (1.0 + s);
                    else
                        temp2 = l + s - (l * s);

                    double temp1 = 2.0 * l - temp2;

                    r = GetColorComponent(temp1, temp2, h + 1.0 / 3.0);
                    g = GetColorComponent(temp1, temp2, h);
                    b = GetColorComponent(temp1, temp2, h - 1.0 / 3.0);
                }
            }
            return new Color { A = (byte)(255 * a), R = (byte)(255 * r), G = (byte)(255 * g), B = (byte)(255 * b) };
        }

        static private double GetColorComponent(double temp1, double temp2, double temp3)
        {
            if (temp3 < 0.0)
                temp3 += 1.0;
            else if (temp3 > 1.0)
                temp3 -= 1.0;

            if (temp3 < 1.0 / 6.0)
                return temp1 + (temp2 - temp1) * 6.0 * temp3;
            else if (temp3 < 0.5)
                return temp2;
            else if (temp3 < 2.0 / 3.0)
                return temp1 + ((temp2 - temp1) * ((2.0 / 3.0) - temp3) * 6.0);
            else
                return temp1;
        }

        public static double[] RgbTOHsl(Color RGB)
        {
            double _R = ((int)RGB.R) / 255f;
            double _G = ((int)RGB.G) / 255f;
            double _B = ((int)RGB.B) / 255f;

            double _Min = Math.Min(Math.Min(_R, _G), _B);
            double _Max = Math.Max(Math.Max(_R, _G), _B);
            double _Delta = _Max - _Min;

            double H = 0;
            double S = 0;
            double L = (double)((_Max + _Min) / 2.0f);

            if (_Delta != 0)
            {
                if (L < 0.5f)
                {
                    S = (double)(_Delta / (_Max + _Min));
                }
                else
                {
                    S = (double)(_Delta / (2.0f - _Max - _Min));
                }


                if (_R == _Max)
                {
                    H = (_G - _B) / _Delta;
                }
                else if (_G == _Max)
                {
                    H = 2f + (_B - _R) / _Delta;
                }
                else if (_B == _Max)
                {
                    H = 4f + (_R - _G) / _Delta;
                }
            }

            H = H / 6.0;
            H = Math.Round(H, 1);
            S = Math.Round(S, 1);
            L = Math.Round(L, 1);

            double[] HSL = new double[3];
            HSL[0] = H;
            HSL[1] = S;
            HSL[2] = L;

            return HSL;
        }
    }
}