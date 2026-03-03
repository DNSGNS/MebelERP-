using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MyApp1;

public class PriceMarkupConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double dbl) return dbl * 1.5;
        if (value is decimal dec) return dec * 1.5m;
        if (value is int i) return i * 1.5;
        if (value is float f) return f * 1.5f;

        return value;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}