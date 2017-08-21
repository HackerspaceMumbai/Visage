using System;
using System.Globalization;
using Xamarin.Forms;

namespace Visage.Converters
{
    public class CheckInVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value != null)
            {
                if(string.IsNullOrEmpty(value as string))
                {
                    var eventStartDate = value as string;

                    var today = DateTime.Now.ToString("d");

                    return eventStartDate.Equals(today);
                }

                return false;
            }

            return false;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
