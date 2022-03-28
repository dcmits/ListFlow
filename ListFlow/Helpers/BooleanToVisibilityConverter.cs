using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace ListFlow.Helpers
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : (object)Visibility.Hidden;
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
                    break;
            }
            return null;
        }
    }
}
