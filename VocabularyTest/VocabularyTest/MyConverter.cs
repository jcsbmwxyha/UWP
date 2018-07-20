using System;
using Windows.UI.Xaml.Data;

namespace VocabularyTest
{
    // Custom class implements the IValueConverter interface.
    public class BoolToStringConverter : IValueConverter
    {
        #region IValueConverter Members

        // Define the Convert method to change a DateTime object to 
        // a month string.
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string content = "";

            if ((bool)value) 
            {
                content = "\uE249";
            }
            else
            {
                content = "\uE24A";
            }

            return content;
        }

        // ConvertBack is not implemented for a OneWay binding.
        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class BoolToStringConverterForEar : IValueConverter
    {
        #region IValueConverter Members

        // Define the Convert method to change a DateTime object to 
        // a month string.
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string content = "";

            if ((bool)value)
            {
                content = "\uF270";
            }
            else
            {
                content = "";
            }

            return content;
        }

        // ConvertBack is not implemented for a OneWay binding.
        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}