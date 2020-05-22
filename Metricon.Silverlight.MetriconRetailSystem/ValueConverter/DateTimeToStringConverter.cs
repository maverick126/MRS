using System;
using System.Globalization;
using System.Windows.Data;

namespace Metricon.Silverlight.MetriconRetailSystem.ValueConverter
{
    public class DateTimeToStringConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime)
            {
                DateTime dateTime = (DateTime)value;
                if (dateTime == new DateTime(1, 1, 1))
                    return string.Empty;
                else
                {
                    if (dateTime.Hour == 0 && dateTime.Minute == 0)
                        return dateTime.ToString("dd/MM/yyyy");
                    else
                        return dateTime.ToString("dd/MM/yyyy h:mm tt"); 
                }
            }
            else
                return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        #endregion
    }
}
