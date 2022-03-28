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
            if ((bool)value)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
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
            }
            return null;
        }
    }
}
