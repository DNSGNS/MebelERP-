using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MyApp1;

public class EdgeStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ProductionTaskStatus status)
        {
            // Чекбокс нажат ТОЛЬКО если статус уже "Ready"
            return status == ProductionTaskStatus.Ready;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Если нажали (True) -> возвращаем Ready, если отжали (False) -> возвращаем CutCompleted (исходное состояние для кромки)
        if (value is bool isChecked && isChecked)
        {
            return ProductionTaskStatus.Ready;
        }
        return ProductionTaskStatus.CutCompleted;
    }
}

public class IntToBoolConverter : IMarkupExtension, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Если это число (int или double), возвращаем true, если оно больше 0
        if (value is int i) return i > 0;
        if (value is double d) return d > 0;

        // Для остальных типов просто проверяем на null
        return value != null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public object ProvideValue(IServiceProvider serviceProvider) => this;
}
