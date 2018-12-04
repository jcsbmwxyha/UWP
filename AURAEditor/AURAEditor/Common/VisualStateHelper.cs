using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AuraEditor.Common
{
    public static class VisualStateHelper
    {
        public static string GetVisualState(DependencyObject obj)
        {
            return (string)obj.GetValue(StateProperty);
        }

        public static void SetVisualState(DependencyObject obj, string value)
        {
            obj.SetValue(StateProperty, value);
        }

        public static readonly DependencyProperty StateProperty = DependencyProperty.RegisterAttached(
                                    "VisualState", typeof(String), typeof(VisualStateHelper), new PropertyMetadata(null, VisualStateChanged));

        static private void VisualStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                VisualStateManager.GoToState(d as Control, e.NewValue as string, true);
        }
    }
}
