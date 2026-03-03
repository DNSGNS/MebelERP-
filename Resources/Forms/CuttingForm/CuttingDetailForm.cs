using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace MyApp1;

public class CuttingDetailsForm
{
    // Используем ObservableCollection, чтобы UI видел добавление/удаление строк
    public ObservableCollection<CuttingDetails> Details { get; set; } = new();

    public void AddDetail()
    {
        var newDetail = new CuttingDetails
        {
            Id = Details.Count + 1
        };
        Details.Add(newDetail);
    }

    public void RemoveDetail(CuttingDetails detail)
    {
        if (Details.Contains(detail))
        {
            Details.Remove(detail);
            RebuildIds(); // Пересчитываем номера после удаления
        }
    }

    private void RebuildIds()
    {
        for (int i = 0; i < Details.Count; i++)
        {
            Details[i].Id = i + 1;
        }
    }
}



public class CuttingDetails : INotifyPropertyChanged
{
    private int _id;
    private int _length;
    private int _width;
    private int _count = 1;
    private bool _canRotate = false;

    public int Id
    {
        get => _id;
        set { _id = value; OnPropertyChanged(); }
    }

    public int Length
    {
        get => _length;
        set { _length = value; OnPropertyChanged(); }
    }

    public int Width
    {
        get => _width;
        set { _width = value; OnPropertyChanged(); }
    }

    public int Count
    {
        get => _count;
        set { _count = value; OnPropertyChanged(); }
    }


    private string _color;
    public string Color
    {
        get => _color;
        set
        {
            if (_color != value)
            {
                _color = value;
                OnPropertyChanged();
            }
        }
    }

    private string _millingText;
    public string MillingText
    {
        get => _millingText;
        set
        {
            if (_millingText != value)
            {
                _millingText = value;
                OnPropertyChanged();
            }
        }
    }

    // Делаем Nullable, чтобы скрывать столбец, если ни у одной детали тип не выбран
    private FasadEdgeType? _selectedEdgeType;
    public FasadEdgeType? SelectedEdgeType
    {
        get => _selectedEdgeType;
        set
        {
            if (_selectedEdgeType != value)
            {
                _selectedEdgeType = value;
                OnPropertyChanged();
            }
        }
    }

    public bool CanRotate
    {
        get => _canRotate;
        set { _canRotate = value; OnPropertyChanged(); }
    }

    // Кромка Тип 1
    private bool _e1L1, _e1L2, _e1W1, _e1W2;

    public bool E1L1
    {
        get => _e1L1;
        set { if (_e1L1 != value) { _e1L1 = value; if (_e1L1) E2L1 = false; OnPropertyChanged(); } }
    }
    public bool E1L2
    {
        get => _e1L2;
        set { if (_e1L2 != value) { _e1L2 = value; if (_e1L2) E2L2 = false; OnPropertyChanged(); } }
    }
    public bool E1W1
    {
        get => _e1W1;
        set { if (_e1W1 != value) { _e1W1 = value; if (_e1W1) E2W1 = false; OnPropertyChanged(); } }
    }
    public bool E1W2
    {
        get => _e1W2;
        set { if (_e1W2 != value) { _e1W2 = value; if (_e1W2) E2W2 = false; OnPropertyChanged(); } }
    }
    // Кромка Тип 2
    private bool _e2L1, _e2L2, _e2W1, _e2W2;
    public bool E2L1
    {
        get => _e2L1;
        set { if (_e2L1 != value) { _e2L1 = value; if (_e2L1) E1L1 = false; OnPropertyChanged(); } }
    }
    public bool E2L2
    {
        get => _e2L2;
        set { if (_e2L2 != value) { _e2L2 = value; if (_e2L2) E1L2 = false; OnPropertyChanged(); } }
    }
    public bool E2W1
    {
        get => _e2W1;
        set { if (_e2W1 != value) { _e2W1 = value; if (_e2W1) E1W1 = false; OnPropertyChanged(); } }
    }
    public bool E2W2
    {
        get => _e2W2;
        set { if (_e2W2 != value) { _e2W2 = value; if (_e2W2) E1W2 = false; OnPropertyChanged(); } }
    }
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}