using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AuraEditor.Common
{
    public static class StateHelper
    {
        public static string GetState(DependencyObject obj)
        {
            return (string)obj.GetValue(StateProperty);
        }

        public static void SetState(DependencyObject obj, string value)
        {
            obj.SetValue(StateProperty, value);
        }

        public static readonly DependencyProperty StateProperty = DependencyProperty.RegisterAttached(
                                    "State", typeof(String), typeof(StateHelper), new PropertyMetadata(null, StateChanged));

        static private void StateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                VisualStateManager.GoToState(d as Control, e.NewValue as string, true);
        }
    }
}
