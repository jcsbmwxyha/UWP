using System;
using System.Globalization;
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
            stop1.Color = Colors.Red;
            stop2.Color = Colors.Yellow;
            stop3.Color = Colors.LightGreen;
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

            stop1.Color = Colors.Red;
            stop2.Color = Colors.Yellow;
            stop3.Color = Colors.LightGreen;
            stop4.Color = Colors.Aqua;
            stop5.Color = Colors.Blue;
            stop6.Color = Colors.Purple;
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

        public static Color HSLToRGB(double a, double h, double s, double l)
        {
            byte r = 0;
            byte g = 0;
            byte b = 0;

            if (s == 0)
            {
                r = g = b = (byte)(l * 255);
            }
            else
            {
                double v1, v2;
                double hue = (double)h;

                v2 = (l < 0.5) ? (l * (1 + s)) : ((l + s) - (l * s));
                v1 = 2 * l - v2;

                r = (byte)(255 * HueToRGB(v1, v2, hue + (1.0f / 3)));
                g = (byte)(255 * HueToRGB(v1, v2, hue));
                b = (byte)(255 * HueToRGB(v1, v2, hue - (1.0f / 3)));
            }

            return Color.FromArgb(255, r, g, b);
        }
        private static double HueToRGB(double v1, double v2, double vH)
        {
            if (vH < 0)
                vH += 1;

            if (vH > 1)
                vH -= 1;

            if ((6 * vH) < 1)
                return (v1 + (v2 - v1) * 6 * vH);

            if ((2 * vH) < 1)
                return v2;

            if ((3 * vH) < 2)
                return (v1 + (v2 - v1) * ((2.0f / 3) - vH) * 6);

            return v1;
        }
        public static double[] RgbTOHsl(Color RGB)
        {
            double _R = ((int)RGB.R) / 255f;
            double _G = ((int)RGB.G) / 255f;
            double _B = ((int)RGB.B) / 255f;

            double _Max = Math.Max(Math.Max(_R, _G), _B);
            double _Min = Math.Min(Math.Min(_R, _G), _B);
            double _Delta = _Max - _Min;

            double H = 0;
            double S = 0;
            double L = (_Max + _Min) / 2.0;

            if (_Delta != 0)
            {
                S = _Delta / (1 - Math.Abs(2 * L - 1));

                if (_R == _Max)
                {
                    H = 60 * ((_G - _B) / _Delta % 6);
                }
                else if (_G == _Max)
                {
                    H = 60 * ((_B - _R) / _Delta + 2);
                }
                else if (_B == _Max)
                {
                    H = 60 * ((_R - _G) / _Delta + 4);
                }
            }
            if (H < 0)
                H += 360;

            double[] HSL = new double[3];
            HSL[0] = H / 360;
            HSL[1] = S;
            HSL[2] = L;

            return HSL;
        }

        public static Color HexToColor(string hexColor)
        {
            //Remove # if present
            if (hexColor.IndexOf('#') != -1)
                hexColor = hexColor.Replace("#", "");
            byte alpha = 0;
            byte red = 0;
            byte green = 0;
            byte blue = 0;

            if (hexColor.Length == 8)
            {
                //#AARRGGBB
                alpha = byte.Parse(hexColor.Substring(0, 2), NumberStyles.AllowHexSpecifier);
                red = byte.Parse(hexColor.Substring(2, 2), NumberStyles.AllowHexSpecifier);
                green = byte.Parse(hexColor.Substring(4, 2), NumberStyles.AllowHexSpecifier);
                blue = byte.Parse(hexColor.Substring(6, 2), NumberStyles.AllowHexSpecifier);
            }
            return Color.FromArgb(alpha, red, green, blue);
        }

        public static string ColorToHex(byte a, byte r, byte g, byte b)
        {
            string hex = "#" + a.ToString("X2") + r.ToString("X2") + g.ToString("X2") + b.ToString("X2");
            return hex;
        }
    }
}