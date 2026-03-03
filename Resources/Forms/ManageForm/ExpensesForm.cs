using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace MyApp1;



public class ExpensesForm : INotifyPropertyChanged
{
    // Полный список всех записей
    public List<Expense> RawExpenses { get; set; } = new();

    // Отфильтрованный список для отображения в CollectionView
    public ObservableCollection<Expense> AllExpenses { get; set; } = new();

    public ObservableCollection<ExpenseCategory> Categories { get; set; } = new();

    // Списки для выбора в Picker
    public List<string> Months { get; } = new List<string>
    { "Все время", "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };

    public List<string> Years { get; set; } = new();

    private int _selectedMonthIndex = 0; // "Все время" по умолчанию
    public int SelectedMonthIndex
    {
        get => _selectedMonthIndex;
        set { _selectedMonthIndex = value; OnPropertyChanged(); ApplyFilter(); }
    }

    private string _selectedYear = "Все";
    public string SelectedYear
    {
        get => _selectedYear;
        set { _selectedYear = value; OnPropertyChanged(); ApplyFilter(); }
    }

    private decimal _totalAmount;
    public decimal TotalAmount
    {
        get => _totalAmount;
        set { _totalAmount = value; OnPropertyChanged(); }
    }

    public void ApplyFilter()
    {
        IEnumerable<Expense> filtered = RawExpenses;

        // 1. Фильтр по году
        // Проверяем, что SelectedYear не null, не пустой и не равен "Все"
        if (!string.IsNullOrEmpty(SelectedYear) && SelectedYear != "Все")
        {
            // Используем TryParse вместо Parse, чтобы избежать краша при ошибке формата
            if (int.TryParse(SelectedYear, out int year))
            {
                filtered = filtered.Where(x => x.Date.Year == year);
            }
        }

        // 2. Фильтр по месяцу
        if (SelectedMonthIndex > 0)
        {
            filtered = filtered.Where(x => x.Date.Month == SelectedMonthIndex);
        }

        // Обновляем отображаемую коллекцию
        AllExpenses.Clear();
        foreach (var item in filtered.OrderByDescending(x => x.Date))
        {
            AllExpenses.Add(item);
        }

        UpdateTotal();
    }

    public void UpdateTotal()
    {
        TotalAmount = AllExpenses.Sum(x => x.Amount);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

