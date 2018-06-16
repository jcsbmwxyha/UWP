using System;
using Windows.Foundation;
using Windows.UI;

namespace ColorPickerTest
{
    public static class Math2
    {
        static double RadiusToAngle = 180 / Math.PI;
        static double AngleToRadius = Math.PI / 180;
        static double Degree30ToRadius = Math.PI / 6;
        static double Degree60ToRadius = Math.PI / 3;
        static double Degree120ToRadius = Math.PI * 2 / 3;

        static public Color HSVToRGB(double h, double s, double v)
        {
            h = h % 360;
            double c = v * s;
            double x = c * (1 - Math.Abs((h / 60 % 2) - 1));
            double m = v - c;
            double red = 0, green = 0, blue = 0;

            if (h >= 0 && h < 60)
            {
                red = c;
                green = x;
                blue = 0;
            }
            else if (h >= 60 && h < 120)
            {
                red = x;
                green = c;
                blue = 0;
            }
            else if (h >= 120 && h < 180)
            {
                red = 0;
                green = c;
                blue = x;
            }
            else if (h >= 180 && h < 240)
            {
                red = 0;
                green = x;
                blue = c;
            }
            else if (h >= 240 && h < 300)
            {
                red = x;
                green = 0;
                blue = c;
            }
            else if (h >= 300 && h < 360)
            {
                red = c;
                green = 0;
                blue = x;
            }
            //r = (red + m) * 255;
            //g = (green + m) * 255;
            //b = (blue + m) * 255;
            return new Color
            {
                A = (byte)(255),
                R = (byte)Math.Round(Math.Round((red + m) * 255, 5)),
                G = (byte)Math.Round(Math.Round((green + m) * 255, 5)),
                B = (byte)Math.Round(Math.Round((blue + m) * 255, 5)),
            };
        }
        static public HSVColor RGBToHSV(Color color)
        {
            HSVColor hsv = new HSVColor();

            double red = color.R / 255.0;
            double green = color.G / 255.0;
            double blue = color.B / 255.0;

            double max = Math.Max(Math.Max(red, green), blue);
            double min = Math.Min(Math.Min(red, green), blue);
            double delta = max - min;
            hsv.V = max;

            if (delta == 0)
            {
                hsv.H = hsv.S = 0;
            }
            else if (max == red)
            {
                hsv.H = 60 * (((green - blue) / delta) % 6);
            }
            else if (max == green)
            {
                hsv.H = 60 * ((blue - red) / delta + 2);
            }
            else if (max == blue)
            {
                hsv.H = 60 * ((red - green) / delta + 4);
            }

            hsv.S = (max == 0) ? 0 : (delta / max);
            hsv.H = (hsv.H + 360) % 360;

            return hsv;
        }
        static public Point SVToPoint(double s, double v, double sideLength)
        {
            double theta = 60 * s;
            double d = sideLength * v;
            double d60 = d / Math.Sin(DegreeToRadian(120 - theta)) * Math.Sin(Degree60ToRadius);
            return new Point(d60 * Math.Cos(DegreeToRadian(theta)),
                             d60 * Math.Sin(DegreeToRadian(theta)));
        }
        static public double ComputeH(double x, double y)
        {
            double rad = Math.Atan(y / x);
            double theta = Math.Abs(RadianToDegree(rad));
            double angle = 0;

            if (x >= 0 && y >= 0)
            {
                angle = 90 + theta;
            }
            else if (x >= 0 && y < 0)
            {
                angle = 90 - theta;
            }
            else if (x < 0 && y >= 0)
            {
                angle = 270 - theta;
            }
            else // (dx < 0 && dy < 0)
            {
                angle = 270 + theta;
            }

            return angle;
        }
        static public void ComputeSV(Point p, double triangleSide, out double s, out double v)
        {
            double r = Math.Atan2(p.Y, p.X);
            double d = Distance(new Point(0, 0), p);
            s = r / DegreeToRadian(60);
            v = (d / Math.Sin(Degree60ToRadius) * Math.Sin(Degree120ToRadius - r)) / triangleSide;
        }
        static public Point RotatePoint(Point p, Point center, double angle)
        {
            double dx = p.X - center.X;
            double dy = p.Y - center.Y;
            double rad = Math.Atan2(dy, dx);
            double d = Distance(p, center);

            return new Point(
                Math.Cos(rad - DegreeToRadian(angle)) * d + center.X,
                Math.Sin(rad - DegreeToRadian(angle)) * d + center.Y
            );
        }
        static public double DegreeToRadian(double degree)
        {
            return degree * AngleToRadius;
        }
        static public double RadianToDegree(double radian)
        {
            return radian * RadiusToAngle;
        }
        static public double Distance(Point pt0, Point pt1)
        {
            double dx = pt0.X - pt1.X;
            double dy = pt0.Y - pt1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}