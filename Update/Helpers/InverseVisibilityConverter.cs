using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace SeatFlow.Helpers
{
    [ValueConversion(typeof(Visibility), typeof(Visibility))]
    public class InverseVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is Visibility && (Visibility)value == Visibility.Visible) ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is Visibility && ((Visibility)value == Visibility.Hidden || (Visibility)value == Visibility.Collapsed)) ? Visibility.Hidden : Visibility.Visible;
        }
    }
}
