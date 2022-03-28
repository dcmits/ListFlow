using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace ListFlow.Helpers
{
    [ValueConversion(typeof(Models.EventDetails.Usage), typeof(Visibility))]
    public class EventDetailsUsageToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null)
            {
                if (bool.TryParse(parameter.ToString(), out bool includeOptional))
                {
                    return (Models.EventDetails.Usage)value == Models.EventDetails.Usage.Mandatory || (Models.EventDetails.Usage)value == Models.EventDetails.Usage.Optional ? Visibility.Visible : Visibility.Hidden;
                }
            }

            return (Models.EventDetails.Usage)value == Models.EventDetails.Usage.Mandatory ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
