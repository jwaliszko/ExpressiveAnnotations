using System;
using System.Globalization;
using System.Windows.Data;

namespace ExpressiveAnnotations.MvvmDesktopSample.Misc
{
    public class FormatStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stringValue = (value as DateTime?)?.ToShortDateString() ?? value.ToString();

            if (parameter == null)
                return stringValue;

            var formatterString = parameter.ToString();
            return string.IsNullOrEmpty(formatterString)
                ? stringValue
                : string.Format(culture, formatterString, stringValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
