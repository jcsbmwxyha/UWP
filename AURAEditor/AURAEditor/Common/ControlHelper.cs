using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using static AuraEditor.Common.Definitions;

namespace AuraEditor.Common
{
    class ControlHelper
    {
        public static T FindControl<T>(UIElement parent, Type targetType, string ControlName) where T : FrameworkElement
        {

            if (parent == null) return null;

            if (parent.GetType() == targetType && ((T)parent).Name == ControlName)
            {
                return (T)parent;
            }
            T result = null;
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                UIElement child = (UIElement)VisualTreeHelper.GetChild(parent, i);

                if (FindControl<T>(child, targetType, ControlName) != null)
                {
                    result = FindControl<T>(child, targetType, ControlName);
                    break;
                }
            }
            return result;
        }
        public static List<T> FindAllControl<T>(UIElement parent, Type targetType) where T : FrameworkElement
        {
            List<T> ResultCollection = new List<T>();

            if (parent == null) return null;

            if (parent.GetType() == targetType)
            {
                ResultCollection.Add((T)parent);
            }

            List<T> result = new List<T>();

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                UIElement child = (UIElement)VisualTreeHelper.GetChild(parent, i);
                result = FindAllControl<T>(child, targetType);
                if (result != null)
                {
                    ResultCollection.AddRange(result);
                }
            }

            return ResultCollection;
        }
        public static async void ShowMess(string res)
        {
            var messDialog = new MessageDialog(res);
            await messDialog.ShowAsync();
        }
        public static bool IsOverlapping(double x1, double w1, double x2, double w2)
        {
            if ((x2 + w2 > x1) && (x1 + w1 > x2))
                return true;
            else
                return false;
        }
        public static double RoundToTens(double number)
        {
            return Math.Round(number / 10d, 0) * 10;
        }
        public static double RoundToGrid(double number)
        {
            return Math.Round(number / GridWidthPixels, 0) * GridWidthPixels;
        }
    }
}
