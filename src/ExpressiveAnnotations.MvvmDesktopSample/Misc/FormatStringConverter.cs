using System;
using System.Windows.Data;

namespace ExpressiveAnnotations.MvvmDesktopSample.Misc
{
    public class FormatStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter == null)
                return value.ToString();

            var formatterString = parameter.ToString();
            return string.IsNullOrEmpty(formatterString) ? value.ToString() : string.Format(culture, formatterString, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
