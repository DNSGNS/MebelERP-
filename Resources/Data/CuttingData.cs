using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Collections.ObjectModel;

namespace MyApp1;

public class CuttingData : INotifyPropertyChanged
{
    [JsonIgnore]
        public CuttingSettingForm Settings { get; set; } = new CuttingSettingForm();

    [JsonIgnore]
        public CuttingDetailsForm DetailsForm { get; set; } = new CuttingDetailsForm();

    [JsonIgnore]
    private CuttingProcessForm _lastResult;

    [JsonIgnore]
    public CuttingProcessForm LastResult
    {
        get => _lastResult;
        set
        {
            _lastResult = value;
            OnPropertyChanged();
        }
    }

    [JsonIgnore]
    private CuttingEditorForm _lastEdit;

    [JsonIgnore] // Чтобы не сохранять состояние редактора в JSON (только данные)
    public CuttingEditorForm LastEdit
    {
        get => _lastEdit;
        set
        {
            if (_lastEdit != value)
            {
                _lastEdit = value;
                OnPropertyChanged();
            }
        }
    }


    [JsonIgnore]
    public bool IsColorVisible => DetailsForm.Details.Any(d => !string.IsNullOrEmpty(d.Color));
    [JsonIgnore]
    public bool IsMillingText => DetailsForm.Details.Any(d => !string.IsNullOrEmpty(d.MillingText));




    [JsonIgnore]
    public double TotalEdge1 => CalculateEdgeLength(1);
    [JsonIgnore]
    public double TotalEdge2 => CalculateEdgeLength(2);

    private CuttingSaveForm _savedReport;
    public CuttingSaveForm SavedReport
    {
        get => _savedReport;
        set
        {
            _savedReport = value;
            OnPropertyChanged();
        }
    }

    public CuttingData()
    {
        // 1. Инициализируем объект результата
        _lastResult = new CuttingProcessForm();

        // 2. Инициализируем редактор, передавая ему ССЫЛКУ на список листов из результата.
        // Теперь, когда редактор добавляет лист в Sheets, он автоматически 
        // добавляется и в LastResult.Sheets, так как это один и тот же объект в памяти.
        _lastEdit = new CuttingEditorForm(_lastResult.Sheets, Settings);

        // Подписываемся на изменения деталей для пересчета итогов
        DetailsForm.Details.CollectionChanged += OnDetailsCollectionChanged;
    }






    // Метод для пересчета общей длины кромки
    public void RefreshTotals()
    {
        OnPropertyChanged(nameof(TotalEdge1));
        OnPropertyChanged(nameof(TotalEdge2));
    }

    private double CalculateEdgeLength(int type)

    {
        double totalMm = 0;
        foreach (var detail in DetailsForm.Details)
        {
            int sideCount = 0;
            if (type == 1)
            {
                if (detail.E1L1) sideCount += detail.Length;
                if (detail.E1L2) sideCount += detail.Length;
                if (detail.E1W1) sideCount += detail.Width;
                if (detail.E1W2) sideCount += detail.Width;
            }

            else if (type == 2)
            {
                if (detail.E2L1) sideCount += detail.Length;
                if (detail.E2L2) sideCount += detail.Length;
                if (detail.E2W1) sideCount += detail.Width;
                if (detail.E2W2) sideCount += detail.Width;
            }
            totalMm += sideCount * detail.Count;
        }

        return totalMm / 1000.0; // Перевод в метры

    }


    private string _projectName;
    public string ProjectName
    {
        get => _projectName;
        set { _projectName = value; OnPropertyChanged(); }
    }

    private string _objectName;
    public string ObjectName
    {
        get => _objectName;
        set { _objectName = value; OnPropertyChanged(); }
    }

    private string? _materialColor;
    public string? MaterialColor
    {
        get => _materialColor;
        set { _materialColor = value; OnPropertyChanged(); }
    }


    private void OnDetailsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (CuttingDetails item in e.NewItems)
                item.PropertyChanged += OnDetailPropertyChanged;
        }

        if (e.OldItems != null)
        {
            foreach (CuttingDetails item in e.OldItems)
                item.PropertyChanged -= OnDetailPropertyChanged;
        }
        RefreshTotals();
        OnPropertyChanged(nameof(IsColorVisible));
        OnPropertyChanged(nameof(IsMillingText));

    }

    private void OnDetailPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // Список свойств, изменение которых требует пересчета метров кромки
        var affectingProperties = new List<string>
    {
        "Length", "Width", "Count", "Color",
        "E1L1", "E1L2", "E1W1", "E1W2",
        "E2L1", "E2L2", "E2W1", "E2W2", "IsActive"
    };

        if (affectingProperties.Contains(e.PropertyName))
        {
            // 1. Пересчитываем общие метры кромки
            RefreshTotals();

            // 2. Если изменился именно цвет, уведомляем UI, 
            // чтобы он проверил, нужно ли показать/скрыть колонку
            if (e.PropertyName == "Color")
            {
                OnPropertyChanged(nameof(IsColorVisible));

            }
            if (e.PropertyName == "MillingText")
            {
                OnPropertyChanged(nameof(IsMillingText));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}