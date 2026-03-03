using System.Globalization;

namespace MyApp1;

public class FasadTypeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is FasadType type)
        {
            return type switch
            {
                FasadType.Standard => "Стандарт",
                FasadType.AGT => "АГТ",
                FasadType.Mirror => "Зеркало",
                FasadType.Aluminium => "Алюминий",
                FasadType.LDSP => "ЛДСП",
                FasadType.NonStandard => "Нестандартная фрезеровка",
                _ => value.ToString()
            };
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
}

public class FasadEdgeTypeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is FasadEdgeType edgeType)
        {
            return edgeType switch
            {
                FasadEdgeType.Freza => "Фреза",
                FasadEdgeType.Mylo => "Мыло",
                _ => edgeType.ToString()
            };
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
}

public class SoftCategoryConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SoftCategory category)
        {
            return category switch
            {
                SoftCategory.None => "Нет",
                SoftCategory.Standard => "Обычное",
                SoftCategory.Peretyazhka => "Перетяжка",
                SoftCategory.Karetka => "Каретка",
                _ => value.ToString()
            };
        }
        return value;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
}

public class SoftShieldTypeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SoftShieldType type)
        {
            return type switch
            {
                SoftShieldType.Peretyazhka => "Перетяжка",
                SoftShieldType.Karetka => "Каретка",
                _ => value.ToString()
            };
        }
        return value;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
}

public class OtherInsertTypeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is OtherInsertType type)
        {
            return type switch
            {
                OtherInsertType.MDF => "МДФ",
                OtherInsertType.Lacobel => "Лакобель",
                OtherInsertType.GraphiteMirror => "Графитовое зеркало",
                OtherInsertType.MatteGlass => "Матовое стекло",
                OtherInsertType.GraphiteGlass => "Графит стекло",
                _ => value.ToString()
            };
        }
        return value;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
}

public class WorkPositionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is WorkPosition position)
        {
            return position switch
            {
                WorkPosition.Measurer => "Замерщик",
                WorkPosition.Storekeeper => "Складовщик",
                WorkPosition.WorkshopWorker => "Цеховик",
                WorkPosition.Manager => "Менеджер",
                WorkPosition.Installer => "Установщик",
                _ => position.ToString()
            };
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
}

public class ProjectStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ProjectStatus status)
        {
            return status switch
            {
                ProjectStatus.New => "Новый",
                ProjectStatus.InProgress => "В процессе",
                ProjectStatus.Completed => "Выполнен",
                _ => status.ToString()
            };
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "Новый" => ProjectStatus.New,
            "В процессе" => ProjectStatus.InProgress,
            "Выполнен" => ProjectStatus.Completed,
            _ => ProjectStatus.New
        };
    }
}


public class StringBoolConverter : IValueConverter
{
    // Если строка не пустая — возвращает true (видимый), если пустая — false (скрытый)
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !string.IsNullOrWhiteSpace(value as string);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ByteArrayToImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is byte[] bytes && bytes.Length > 0)
        {
            return ImageSource.FromStream(() => new MemoryStream(bytes));
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
}