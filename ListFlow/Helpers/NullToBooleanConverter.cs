using System;
using System.Globalization;
using System.Windows.Data;

namespace ListFlow.Helpers
{
    [ValueConversion(typeof(object), typeof(bool))]
    class NullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter != null && string.CompareOrdinal(parameter.ToString(), "Inverse") == 0 ? value != null : value == null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
