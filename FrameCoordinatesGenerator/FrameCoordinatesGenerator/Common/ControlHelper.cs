﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace FrameCoordinatesGenerator.Common
{
    static class ControlHelper
    {
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
        public static async void ShowMess(string res)
        {
            var messDialog = new MessageDialog(res);
            await messDialog.ShowAsync();
        }
    }
}
