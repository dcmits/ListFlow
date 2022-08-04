using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace ListFlow.Helpers
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value
                ? parameter != null && string.CompareOrdinal(parameter.ToString(), "Inverse") == 0 ? Visibility.Collapsed : (object)Visibility.Visible
                : parameter != null && string.CompareOrdinal(parameter.ToString(), "Inverse") == 0 ? Visibility.Visible : (object)Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((Visibility)value)
            {
                case Visibility.Collapsed:
                case Visibility.Hidden:
                    return false;
                case Visibility.Visible:
                    return true;
                default:
                    return false;
            }
        }
    }
}
