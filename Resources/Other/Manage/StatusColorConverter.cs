using System.Globalization;

namespace MyApp1;

public class StatusColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ProjectStatus status)
        {
            return status switch
            {
                ProjectStatus.New => Colors.Gray,
                ProjectStatus.InProgress => Colors.Gold, // Желтый
                ProjectStatus.Completed => Colors.Green,
                _ => Colors.Gray
            };
        }
        return Colors.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DateDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime date)
        {
            // Если дата минимальна (не назначена), возвращаем прочерк
            if (date == DateTime.MinValue)
                return "—";

            // Иначе возвращаем форматированную дату
            return date.ToString("dd.MM.yyyy");
        }
        return "—";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class StatusToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString(); // Из Enum в String для отображения в Picker
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string statusStr)
        {
            // Важно: здесь должен быть ваш enum ProjectStatus
            if (Enum.TryParse(typeof(ProjectStatus), statusStr, out var result))
            {
                return result;
            }
        }
        return ProjectStatus.New;
    }
}
public class DateToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime date)
        {
            if (date == DateTime.MinValue || date.Year <= 2000)
                return "выберите дату";

            return date.ToString("dd.MM.yyyy");
        }
        return "выберите дату";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
}


public class MaterialNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is MaterialType type)
        {
            return type switch
            {
                MaterialType.LDSP => "ЛДСП",
                MaterialType.HDF => "ХДФ",
                MaterialType.MDF => "МДФ",
                MaterialType.AGT => "АГТ",
                _ => value.ToString()
            };
        }
        return value;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
}
