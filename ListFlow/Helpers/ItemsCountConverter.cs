using System;
using System.Globalization;
using System.Windows.Data;

namespace ListFlow.Helpers
{
    [ValueConversion(typeof(int?), typeof(string))]
    public class ItemsCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string textResult = string.Empty;
            int textType = 0;

            if(parameter != null)
            {
                int.TryParse(parameter.ToString(), out textType);
            }

            if (value != null)
            {
                switch ((int)value)
                {
                    case 0:
                        if (textType == 1)
                        {
                            textResult = Properties.Resources.Undo_NoAction;
                        }
                        else
                        {
                            textResult = Properties.Resources.ListItemsCountNo;
                        }
                        break;
                    case 1:
                        if (textType == 1)
                        {
                            textResult = Properties.Resources.Undo_OneAction;
                        }
                        else
                        {
                            textResult = Properties.Resources.ListItemsCountOne;
                        }
                        break;
                    default:
                        if (textType == 1)
                        {
                            textResult = string.Format(Properties.Resources.Undo_Actions, (int)value);
                        }
                        else
                        {
                            textResult = string.Format(Properties.Resources.ListItemsCount, (int)value);
                        }
                        break;
                }
            }
            
            return textResult;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }
}
