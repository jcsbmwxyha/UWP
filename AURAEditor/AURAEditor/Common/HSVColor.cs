using Windows.UI;

namespace AuraEditor.Common
{
    public class HSVColor
    {
        public double H;
        public double S;
        public double V;

        public HSVColor() { H = S = V = 0; }
        public HSVColor(double h, double s, double v)
        {
            H = h;
            S = s;
            V = v;
        }

        internal Color GetRGB()
        {
            return Math2.HSVToRGB(H, S, V);
        }
    }
}