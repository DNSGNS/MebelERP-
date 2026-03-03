using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MyApp1;

    // Конвертер для выбора месяца (из 1-12 в 0-11 для Picker и обратно)
    public class MonthIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int month)
                return month - 1; // Picker работает с индексами от 0
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index)
                return index + 1; // Превращаем индекс 0 в Январь (1)
            return 1;
        }
    }

    // Делает пустые ячейки прозрачными
    public class EmptyDayColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isEmpty && isEmpty)
                return Colors.Transparent;
            return Colors.White; // Цвет обычной ячейки
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    // Убирает рамку у пустых ячеек
    public class EmptyDayStrokeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isEmpty && isEmpty)
                return 0;
            return 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    // Инвертирует bool (нужен для IsVisible="{Binding IsEmpty, Converter={StaticResource InvertedBoolConverter}}")
    public class InvertedBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolean)
                return !boolean;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
