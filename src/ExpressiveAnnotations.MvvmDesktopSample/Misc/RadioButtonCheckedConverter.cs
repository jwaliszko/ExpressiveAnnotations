using System;
using System.Globalization;
using System.Windows.Data;

namespace ExpressiveAnnotations.MvvmDesktopSample.Misc
{
    public class RadioButtonCheckedConverter : IValueConverter //http://stackoverflow.com/q/397556/270315
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }
}
