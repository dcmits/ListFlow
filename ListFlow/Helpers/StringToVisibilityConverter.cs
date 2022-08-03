using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace ListFlow.Helpers
{
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value.ToString()) ? Visibility.Collapsed : Visibility.Visible ;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
