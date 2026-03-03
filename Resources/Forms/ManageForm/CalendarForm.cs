using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MyApp1;

public class CalendarDay
{
    public int DayNumber { get; set; }
    public DateTime Date { get; set; }
    public bool IsEmpty { get; set; } // Для пустых ячеек в начале/конце месяца
    public List<ProjectManageData> Projects { get; set; } = new();
}

public class CalendarForm : BindableObject
{
    private DateTime _currentDate = DateTime.Now;
    private ObservableCollection<CalendarDay> _days = new();
    private List<ProjectManageData> _allProjects;

    public ObservableCollection<CalendarDay> Days
    {
        get => _days;
        set { _days = value; OnPropertyChanged(); }
    }

    public int SelectedMonth
    {
        get => _currentDate.Month;
        set { _currentDate = new DateTime(_currentDate.Year, value, 1); RefreshCalendar(); }
    }

    public int SelectedYear
    {
        get => _currentDate.Year;
        set { _currentDate = new DateTime(value, _currentDate.Month, 1); RefreshCalendar(); }
    }

    public List<int> Years { get; } = Enumerable.Range(2024, 10).ToList();
    public List<string> Months { get; } = new()
    { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };

    public CalendarForm(List<ProjectManageData> projects)
    {
        _allProjects = projects;
        RefreshCalendar();
    }

    private void RefreshCalendar()
    {
        var daysList = new List<CalendarDay>();
        DateTime firstDayOfMonth = new DateTime(_currentDate.Year, _currentDate.Month, 1);
        int daysInMonth = DateTime.DaysInMonth(_currentDate.Year, _currentDate.Month);

        // Определяем смещение (день недели первого числа)
        int offset = ((int)firstDayOfMonth.DayOfWeek + 6) % 7;

        // Пустые ячейки в начале
        for (int i = 0; i < offset; i++)
            daysList.Add(new CalendarDay { IsEmpty = true });

        // Заполняем дни
        for (int i = 1; i <= daysInMonth; i++)
        {
            var date = new DateTime(_currentDate.Year, _currentDate.Month, i);

            // ИСПРАВЛЕННАЯ ЛОГИКА:
            // Теперь мы ищем проекты, у которых ХОТЯ БЫ ОДНА дата из списка InstallDates 
            // совпадает с текущим числом в календаре
            var dayProjects = _allProjects
                .Where(p => p.InstallDates != null &&
                            p.InstallDates.Any(d => d.Date == date.Date))
                .ToList();

            daysList.Add(new CalendarDay
            {
                DayNumber = i,
                Date = date,
                Projects = dayProjects
            });
        }

        Days = new ObservableCollection<CalendarDay>(daysList);
    }
}
