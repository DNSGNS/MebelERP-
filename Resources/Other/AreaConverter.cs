using System.Globalization;

namespace MyApp1; // Убедитесь, что namespace совпадает с тем, что в XAML (xmlns:local)

public class AreaConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double areaInMm2)
        {
            return areaInMm2 / 1_000_000.0;
        }

        if (value is CuttingSettingForm settings)
        {
            double l = settings.SheetLength ?? 0;
            double w = settings.SheetWidth ?? 0;
            return (l * w) / 1_000_000.0;
        }

        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


public class SheetDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Проверяем, что пришел объект листа
        if (value is SheetLayout sheet)
        {
            // Вариант 1: Цвет не указан -> "Лист 1"
            if (string.IsNullOrEmpty(sheet.ColorName))
            {
                return $"Лист {sheet.SheetIndex}";
            }

            // Вариант 2: Цвет есть -> "Лист 1 [Белый]"
            return $"Лист {sheet.SheetIndex} [{sheet.ColorName}]";
        }

        return "Лист ?";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}