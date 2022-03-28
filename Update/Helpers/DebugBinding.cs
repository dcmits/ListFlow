using System;
using System.Windows;
using System.Windows.Data;
using System.Diagnostics;
using System.Windows.Markup;

namespace SeatFlow.Helpers
{
    /// <summary>
    /// Usage : Property="{Binding Value, Converter={namespace:DebugExtension}}"
    /// </summary>
    public class DebugConverter : IValueConverter
    {
        public static DebugConverter Instance = new DebugConverter();
        private DebugConverter() { }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Debugger.Break();
            return value;   //Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Debugger.Break();
            return value;   //Binding.DoNothing;
        }
    }

    public class DebugExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return DebugConverter.Instance;
        }
    }
}
