using MyApp1.Resources.Other;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace MyApp1;

public class ProjectManageData : INotifyPropertyChanged
{
    private bool _isProcessing;
    [JsonIgnore]
    public bool IsProcessing
    {
        get => _isProcessing;
        set
        {
            _isProcessing = value;
            OnPropertyChanged(nameof(IsProcessing));
        }
    }

    private Guid _id = Guid.NewGuid();
    public Guid Id
    {
        get => _id;
        set { _id = value; OnPropertyChanged(); }
    }

    private string _projectName = "Новый проект";
    public string ProjectName
    {
        get => _projectName;
        set
        {
            _projectName = value;
            OnPropertyChanged();
        }
    }

    private string _creatorName = "Администратор";
    public string CreatorName
    {
        get => _creatorName;
        set
        {
            _creatorName = value;
            OnPropertyChanged();
        }
    }

    // 4. Ценовые показатели (услуги)
    private decimal _workshopCost; public decimal WorkshopCost { get => _workshopCost; set { _workshopCost = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalProjectPrice)); } }
    private decimal _packagingCost; public decimal PackagingCost { get => _packagingCost; set { _packagingCost = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalProjectPrice)); } }
    private decimal _installationCost; public decimal InstallationCost { get => _installationCost; set { _installationCost = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalProjectPrice)); } }
    private decimal _deliveryCost; public decimal DeliveryCost { get => _deliveryCost; set { _deliveryCost = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalProjectPrice)); } }
    private decimal _liftingCost; public decimal LiftingCost { get => _liftingCost; set { _liftingCost = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalProjectPrice)); } }
    private decimal _garbageRemovalCost; public decimal GarbageRemovalCost { get => _garbageRemovalCost; set { _garbageRemovalCost = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalProjectPrice)); } }

    private decimal _totalProjectPrice; public decimal TotalProjectPrice { get => _totalProjectPrice; set { _totalProjectPrice = value; OnPropertyChanged(); } }


    // Добавьте это внутрь класса ProjectManageData
    private ObservableCollection<WorkMan> _assignedInstallers = new();
    public ObservableCollection<WorkMan> AssignedInstallers
    {
        get => _assignedInstallers;
        set { _assignedInstallers = value; OnPropertyChanged(); OnPropertyChanged(nameof(InstallersDisplay)); }
    }

    [JsonIgnore]
    public string InstallersDisplay => AssignedInstallers.Count > 0
        ? string.Join(", ", AssignedInstallers.Select(a => a.Name))
        : "Выбрать...";

    private DateTime _creationDate = DateTime.Now;
    public DateTime CreationDate
    {
        get => _creationDate;
        set
        {
            _creationDate = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<DateTime> _installDates = new();
    public ObservableCollection<DateTime> InstallDates
    {
        get => _installDates;
        set { _installDates = value; OnPropertyChanged(); }
    }


    private ProjectStatus _status = ProjectStatus.New;
    public ProjectStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Status));
        }
    }


    private decimal _prepayment;
    public decimal Prepayment
    {
        get => _prepayment;
        set
        {
            _prepayment = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(RemainingBalance));
        }
    }

    public string DatesDisplay => InstallDates.Any()
        ? string.Join(", ", InstallDates.Select(d => d.ToString("dd.MM.yyyy")))
        : "Дата не назначена";

    [JsonIgnore]
    public decimal RemainingBalance => TotalProjectPrice - Prepayment;


    [JsonIgnore]
    private bool _isExpanded;
    public bool IsExpanded
    {
        get => _isExpanded;
        set { _isExpanded = value; OnPropertyChanged(); }
    }

    // Реализация интерфейса без базового класса
    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}