﻿using System;
using System.Globalization;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace AuraEditor.Common
{
    public static class AuraEditorColorHelper
    {
        public class RecentColor
        {
            public string HexColor { get; set; }
            public RecentColor()
            {
                HexColor = "#00000000";
            }
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