using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace MyApp1;

// Базовый класс с общей логикой ID и Статусов
public abstract class WorkshopTaskBase : INotifyPropertyChanged
{
    public Guid Id { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string ObjectName { get; set; } = string.Empty; // "Шкаф в спальню"

    private bool _isTaken;
    public bool IsTaken
    {
        get => _isTaken;
        set { _isTaken = value; OnPropertyChanged(); }
    }

    private DateTime _creationDate;
    public DateTime CreationDate
    {
        get => _creationDate;
        set { _creationDate = value; OnPropertyChanged(); }
    }

    private ProductionTaskStatus _status;
    public ProductionTaskStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsReady));
        }
    }

    // Свойство для Чекбокса "Готово"
    public bool IsReady
    {
        get => Status == ProductionTaskStatus.Ready;
        set => Status = value ? ProductionTaskStatus.Ready : ProductionTaskStatus.Pending;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

// Обертка для задач Раскроя и Кромки (использует ваш CuttingSaveForm)
public class CuttingWorkshopItem : WorkshopTaskBase
{
    public CuttingSaveForm? Data { get; set; }
    private CuttingType _materialType;
    public CuttingType MaterialType
    {
        get => _materialType;
        set { _materialType = value; OnPropertyChanged(); }
    }

}

// Модель для задач по дверям (купе или межкомнатным)
public class DoorWorkshopItem : WorkshopTaskBase
{
    private DoorSlimLineForm? _data;

    /// <summary>
    /// Данные самой двери (наполнение, количество, вставки)
    /// </summary>
    public DoorSlimLineForm? Data
    {
        get => _data;
        set
        {
            _data = value;
            OnPropertyChanged();
            // Уведомляем UI, что краткое описание тоже могло измениться
            OnPropertyChanged(nameof(DoorSummary));
        }
    }

    /// <summary>
    /// Вспомогательное свойство для отображения в списке задач
    /// </summary>
    public string DoorSummary => Data != null
        ? $"Кол-во: {Data.DoorsCount} шт."
        : "Нет данных о дверях";

    /// <summary>
    /// Можно добавить тип профиля или системы, если это важно для заголовка задачи
    /// </summary>
    private string _profileSystem = string.Empty;
    public string ProfileSystem
    {
        get => _profileSystem;
        set { _profileSystem = value; OnPropertyChanged(); }
    }
}


// Модель для деталей фасада
public class FasadWorkshopItem : WorkshopTaskBase
{
    public double Length { get; set; }
    public double Width { get; set; }
    public int Count { get; set; }
    public string Color { get; set; } = string.Empty;
    public string MillingText { get; set; } = string.Empty;
    public FasadEdgeType SelectedEdgeType { get; set; }

    [JsonIgnore]
    public string SizeText => $"{Length} x {Width}";
}