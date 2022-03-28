using System;
using System.Globalization;
using System.Windows.Data;

namespace SeatFlow.Helpers
{
   [ValueConversion(typeof(object), typeof(string))]
    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string format = parameter as string;

            if (!string.IsNullOrEmpty(format))
            {
                if (format.EndsWith("%"))
                {
                    return string.Format(culture, format, (double)value * 100);
                }
                else
                { 
                    return string.Format(culture, format, value);
                }
            }
            else
            {
                return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }
}
