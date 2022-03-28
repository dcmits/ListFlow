using System;
using System.Globalization;
using System.Windows.Data;

namespace ListFlow.Helpers
{
    [ValueConversion(typeof(Models.EventDetails.Usage), typeof(bool))]
    public class EventDetailsUsageToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int selectedValue = int.Parse(parameter.ToString());

            return (Models.EventDetails.Usage)value == (Models.EventDetails.Usage)selectedValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int selectedValue = int.Parse(parameter.ToString());

            return (Models.EventDetails.Usage)selectedValue;
        }
    }
}
