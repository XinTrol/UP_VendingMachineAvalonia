using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace UP_4.Converters
{
    public class InvertedBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // Если параметр "Invert" – возвращаем противоположное значение
                if (parameter is string param && param == "Invert")
                    return !boolValue;

                // Иначе возвращаем само значение (используется для кнопки "По количеству")
                return boolValue;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}