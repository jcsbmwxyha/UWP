using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace AuraEditor.Common
{
    public static class ZoneHelper
    {
        public static string GetZoneState(DependencyObject obj)
        {
            return (string)obj.GetValue(ZoneStateProperty);
        }

        public static void SetZoneState(DependencyObject obj, string value)
        {
            obj.SetValue(ZoneStateProperty, value);
        }

        public static readonly DependencyProperty ZoneStateProperty = DependencyProperty.RegisterAttached(
                                    "ZoneState", typeof(String), typeof(VisualStateHelper), new PropertyMetadata(null, ZoneStateChanged));

        static private void ZoneStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is Rectangle)
            {
                Rectangle r = d as Rectangle;
                if (e.NewValue == "Normal")
                    r.Fill = new SolidColorBrush(Colors.Transparent);
                else if (e.NewValue == "Hover")
                    r.Fill = new SolidColorBrush(Colors.Blue);
                else if (e.NewValue == "Selected")
                    r.Fill = new SolidColorBrush(Colors.Red);
            }
            else if(d is Image)
            {

            }
            //if (e.NewValue != null)
            //    VisualStateManager.GoToState(d as Control, e.NewValue as string, true);
        }
    }
}
