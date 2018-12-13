using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace FrameCoordinatesGenerator.Common
{
    class BindingHelper
    {
        #region -- Canvas.Left --
        public static readonly DependencyProperty CanvasLeftBindingPathProperty =
            DependencyProperty.RegisterAttached(
                "CanvasLeftBindingPath", typeof(string), typeof(BindingHelper),
                new PropertyMetadata(null, GridBindingPathPropertyChanged));
        public static string GetCanvasLeftBindingPath(DependencyObject obj)
        {
            return (string)obj.GetValue(CanvasLeftBindingPathProperty);
        }
        public static void SetCanvasLeftBindingPath(DependencyObject obj, string value)
        {
            obj.SetValue(CanvasLeftBindingPathProperty, value);
        }
        #endregion

        #region -- Canvas.Top --
        public static readonly DependencyProperty CanvasTopBindingPathProperty =
            DependencyProperty.RegisterAttached(
                "CanvasTopBindingPath", typeof(string), typeof(BindingHelper),
                new PropertyMetadata(null, GridBindingPathPropertyChanged));
        public static string GetCanvasTopBindingPath(DependencyObject obj)
        {
            return (string)obj.GetValue(CanvasTopBindingPathProperty);
        }
        public static void SetCanvasTopBindingPath(DependencyObject obj, string value)
        {
            obj.SetValue(CanvasTopBindingPathProperty, value);
        }
        #endregion

        #region -- Width --
        public static readonly DependencyProperty WidthBindingPathProperty =
           DependencyProperty.RegisterAttached(
               "WidthBindingPath", typeof(string), typeof(BindingHelper),
               new PropertyMetadata(null, GridBindingPathPropertyChanged));
        public static string GetWidthBindingPath(DependencyObject obj)
        {
            return (string)obj.GetValue(WidthBindingPathProperty);
        }
        public static void SetWidthBindingPath(DependencyObject obj, string value)
        {
            obj.SetValue(WidthBindingPathProperty, value);
        }
        #endregion

        #region -- Height --
        public static readonly DependencyProperty HeightBindingPathProperty =
            DependencyProperty.RegisterAttached(
                "HeightBindingPath", typeof(string), typeof(BindingHelper),
                new PropertyMetadata(null, GridBindingPathPropertyChanged));
        public static string GetHeightBindingPath(DependencyObject obj)
        {
            return (string)obj.GetValue(HeightBindingPathProperty);
        }
        public static void SetHeightBindingPath(DependencyObject obj, string value)
        {
            obj.SetValue(HeightBindingPathProperty, value);
        }
        #endregion

        private static void GridBindingPathPropertyChanged(
            DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var propertyPath = e.NewValue as string;

            if (propertyPath != null)
            {
                DependencyProperty property = null;

                if (e.Property == CanvasLeftBindingPathProperty)
                    property = Canvas.LeftProperty;
                else if (e.Property == CanvasTopBindingPathProperty)
                    property = Canvas.TopProperty;
                else if (e.Property == WidthBindingPathProperty)
                    property = FrameworkElement.WidthProperty;
                else if (e.Property == HeightBindingPathProperty)
                    property = FrameworkElement.HeightProperty;

                BindingOperations.SetBinding(obj, property,
                    new Binding
                    {
                        Path = new PropertyPath(propertyPath),
                        Mode = BindingMode.TwoWay
                    });
            }
        }
    }
}
