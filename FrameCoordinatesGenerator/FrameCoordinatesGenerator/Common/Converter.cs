﻿using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace FrameCoordinatesGenerator.Common
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool)
            {
                if ((bool)value == true)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is bool)
                return (bool)value;
            return false;
        }
    }

    public class ReverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool)
            {
                if ((bool)value == true)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is bool)
                return !(bool)value;
            return false;
        }
    }

    public class BooleanToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool)
            {
                if ((bool)value == true)
                    return new SolidColorBrush(Colors.Red);
                else
                    return new SolidColorBrush(Colors.Green);
            }
            return new SolidColorBrush(Colors.Green);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is bool)
                return !(bool)value;
            return false;
        }
    }
}