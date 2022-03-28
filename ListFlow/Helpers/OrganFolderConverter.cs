using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ListFlow.Helpers
{
    public class OrganFolderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string FileLocationPath = string.Empty;
            try
            {
                if (value != null)
                {
                    FileLocationPath = string.Format(@"file://{0}", value);
                }
            }
            catch { }

            return FileLocationPath;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
