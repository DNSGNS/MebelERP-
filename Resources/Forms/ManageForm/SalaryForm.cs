using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace MyApp1;

public class SalaryReportItem : INotifyPropertyChanged
{
    public Guid Id { get; set; }
    public DateTime DatePerformed { get; set; }
    public string WorkerName { get; set; } = string.Empty;

    public Guid ProjectId { get; set; }

    public Guid WorkerId { get; set; }

    public string ProjectName { get; set; } = string.Empty;
    public MaterialType? Material { get; set; }

    // Работы
    public decimal Saw { get; set; }
    public decimal Edging { get; set; }
    public decimal Additive { get; set; }
    public int DoorCanvas { get; set; }
    public int DoorSectional { get; set; }
    public decimal Packaging { get; set; }
    public decimal Installation { get; set; }
    public decimal GrindingSoap { get; set; }
    public decimal GrindingFrez { get; set; }
    public decimal Milling { get; set; }
    public decimal Additionally { get; set; }

    public decimal Measurement { get; set; }

    public string? Comment { get; set; }
    public decimal TotalSalary { get; set; }

    // Геттер для красивого вывода даты
    public string DateDisplay => DatePerformed.ToString("dd.MM.yyyy");

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}


public class SalaryForm : INotifyPropertyChanged
{
    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    // Исходный список всех записей (храним здесь данные, полученные из API)
    public List<SalaryReportItem> _allSourceItems = new();

    // Список для отображения на экране (после фильтрации)
    public ObservableCollection<SalaryReportItem> DisplayItems { get; set; } = new();


    public List<ProjectSimpleDto> AllProjects { get; set; } = new();

    public List<WorkerSimpleDto> AllWorkers { get; set; } = new();

    // ID и Имя текущего работника
    public Guid? CurrentWorkerId { get; set; }
    public string? CurrentWorkerName { get; set; }

    // Сумма
    private decimal _totalSum;
    public decimal TotalSum
    {
        get => _totalSum;
        set { _totalSum = value; OnPropertyChanged(); }
    }

    public void ApplyFilters(int month, int year)
    {
        var query = _allSourceItems.AsQueryable();

        if (!string.IsNullOrEmpty(CurrentWorkerName))
        {
            query = query.Where(x => x.WorkerName == CurrentWorkerName);
        }

        query = query.Where(x => x.DatePerformed.Month == month && x.DatePerformed.Year == year);

        DisplayItems.Clear();
        foreach (var item in query)
        {
            DisplayItems.Add(item);
        }

        TotalSum = DisplayItems.Sum(x => x.TotalSalary);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}