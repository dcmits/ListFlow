using System;
using System.Globalization;
using System.Windows.Data;

namespace ListFlow.Helpers
{
    [ValueConversion(typeof(string), typeof(bool))]
    public class NullStringToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!string.IsNullOrEmpty((string)value))
            {
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
