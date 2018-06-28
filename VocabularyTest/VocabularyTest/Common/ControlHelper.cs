﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace VocabularyTest.Common
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
    }
}
