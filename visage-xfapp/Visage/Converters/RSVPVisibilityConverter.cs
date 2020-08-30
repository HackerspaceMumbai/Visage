using System;
using System.Globalization;
using Xamarin.Forms;

namespace Visage.Converters
{
    // used to set visibility of rsvp label in the list of events
    public class RSVPVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(value as string))
                return false;

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
