using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace AuraEditor.Common
{
    static class ControlHelper
    {
        public static DependencyObject FindParentDependencyObject(DependencyObject child)
        {
            return VisualTreeHelper.GetParent(child);
        }
        public static T FindParentControl<T>(UIElement child, Type targetType) where T : FrameworkElement
        {
            if (child == null)
                return null;

            if (child.GetType() == targetType)
            {
                return (T)child;
            }

            UIElement parent = (UIElement)VisualTreeHelper.GetParent(child);

            return FindParentControl<T>(parent, targetType);
        }
        public static T FindControl<T>(UIElement parent, Type targetType, string ControlName) where T : FrameworkElement
        {
            if (parent == null)
                return null;

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
        public static T FindFirstControl<T>(UIElement parent, Type targetType) where T : FrameworkElement
        {
            if (parent == null)
                return null;

            if (parent.GetType() == targetType)
            {
                return (T)parent;
            }

            T result = null;
            int count = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < count; i++)
            {
                UIElement child = (UIElement)VisualTreeHelper.GetChild(parent, i);

                if (FindFirstControl<T>(child, targetType) != null)
                {
                    result = FindFirstControl<T>(child, targetType);
                    break;
                }
            }

            return result;
        }
        public static ContentDialog GetCurrentContentDialog()
        {
            var popups = VisualTreeHelper.GetOpenPopups(Window.Current);

            if (popups.Count == 0)
                return null;

            Popup popup = VisualTreeHelper.GetOpenPopups(Window.Current)[0];
            if (popup.Child is ContentDialog dialog)
                return dialog;
            else
                return null;
        }
        public static async void ShowMess(string res)
        {
            var messDialog = new MessageDialog(res);
            await messDialog.ShowAsync();
        }
        public static bool IsPiling(double x1, double w1, double x2, double w2)
        {
            if ((x2 + w2 > x1) && (x1 + w1 > x2))
                return true;
            else
                return false;
        }

        static public Storyboard AnimationStart(DependencyObject o, string target, double runTime, double from, double to)
        {
            var storyboard = new Storyboard();
            var animation = new DoubleAnimation();

            animation.Duration = TimeSpan.FromMilliseconds(runTime);
            animation.EnableDependentAnimation = true;
            animation.From = from;
            animation.To = to;

            Storyboard.SetTargetProperty(animation, target);
            Storyboard.SetTarget(animation, o);
            storyboard.Children.Add(animation);
            storyboard.Begin();

            return storyboard;
        }
        static public async Task AnimationStartAsync(DependencyObject o, string target, double runTime, double from, double to)
        {
            var storyboard = new Storyboard();
            var animation = new DoubleAnimation();

            animation.Duration = TimeSpan.FromMilliseconds(runTime);
            animation.EnableDependentAnimation = true;
            animation.From = from;
            animation.To = to;

            Storyboard.SetTargetProperty(animation, target);
            Storyboard.SetTarget(animation, o);
            storyboard.Children.Add(animation);
            storyboard.Begin();

            while (storyboard.GetCurrentState() == ClockState.Active)
            {
                await Task.Delay(50);
            }
        }
    }
}
