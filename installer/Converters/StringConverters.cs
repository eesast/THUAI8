using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace installer.Converters
{
    public class StringNotNullOrEmptyBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;

            string strValue = value.ToString();
            return !string.IsNullOrEmpty(strValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}