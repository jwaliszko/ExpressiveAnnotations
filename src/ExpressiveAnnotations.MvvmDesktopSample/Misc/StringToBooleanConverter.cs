using System;
using System.Globalization;
using System.Windows.Data;

namespace ExpressiveAnnotations.MvvmDesktopSample.Misc
{
    public class StringToBooleanConverter : IValueConverter // http://stackoverflow.com/q/18449890/270315
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool) value == bool.Parse((string) parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return bool.Parse((string) parameter);
        }
    }
}