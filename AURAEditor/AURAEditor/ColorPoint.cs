using AuraEditor.UserControls;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using static AuraEditor.Common.ControlHelper;
using AuraEditor.Dialogs;

namespace AuraEditor
{
    public class ColorPoint
    {
        public ColorPointBt UI { get; }
        public double Offset
        {
            get
            {
                return UI.X / 180;
            }
            set
            {
                UI.X = value * 180;
            }
        }
        public Color Color
        {
            get
            {
                List<RadioButton> items = FindAllControl<RadioButton>(UI, typeof(RadioButton));
                return (items[0].Background as SolidColorBrush).Color;
            }
            set
            {
                List<RadioButton> items = FindAllControl<RadioButton>(UI, typeof(RadioButton));
                items[0].Background = new SolidColorBrush(value);
            }
        }
        public ColorPoint()
        {
            UI = new ColorPointBt();
            UI.DataContext = this;
            UI.X = 0;
            UI.LeftBorder = 0;
            UI.RightBorder = 180;
        }

        public ColorPoint(TriggerDialog td)
        {
            UI = new ColorPointBt(td);
            UI.DataContext = this;
            UI.X = 0;
            UI.LeftBorder = 0;
            UI.RightBorder = 180;
        }

        public ColorPoint(ColorPoint cp)
        {
            UI = new ColorPointBt();
            UI.DataContext = this;
            UI.X = cp.UI.X;
            UI.LeftBorder = cp.UI.LeftBorder;
            UI.RightBorder = cp.UI.RightBorder;
            Color = cp.Color;
        }

        public ColorPoint(ColorPoint cp, TriggerDialog td)
        {
            UI = new ColorPointBt(td);
            UI.DataContext = this;
            UI.X = cp.UI.X;
            UI.LeftBorder = cp.UI.LeftBorder;
            UI.RightBorder = cp.UI.RightBorder;
            Color = cp.Color;
        }
    }
}
