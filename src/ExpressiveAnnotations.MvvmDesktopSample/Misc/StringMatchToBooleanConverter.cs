using System;
using System.Globalization;
using System.Windows.Data;

namespace ExpressiveAnnotations.MvvmDesktopSample.Misc
{
    public class StringMatchToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            var checkValue = value.ToString();
            var targetValue = parameter.ToString();
            return checkValue.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return null;
            if (targetType == typeof (bool))
                return bool.Parse(parameter.ToString());
            return parameter as string;
        }
    }
}
