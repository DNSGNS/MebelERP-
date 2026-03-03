using MyApp1.Resources.Other;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace MyApp1;

public class ProjectData : INotifyPropertyChanged
{

    private Guid _id = Guid.NewGuid();
    public Guid Id
    {
        get => _id;
        set { _id = value; OnPropertyChanged(); }
    }

    // 1. Список объектов (комнат/шкафов)
    private ObservableCollection<ObjectData> _objects = new();
    public ObservableCollection<ObjectData> Objects
    {
        get => _objects;
        set
        {
            _objects = value;
            OnPropertyChanged();
        }
    }


    private bool _isUploading;
    [JsonIgnore]
    public bool IsUploading
    {
        get => _isUploading;
        set { _isUploading = value; OnPropertyChanged(); }
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


    // 2. Суммарная фурнитура по всему проекту
    private FurnitureForm _totalFurniture = new();
    public FurnitureForm TotalFurniture
    {
        get => _totalFurniture;
        set { _totalFurniture = value; OnPropertyChanged(); }
    }

    private ObservableCollection<FurnitureProfileForm> _allProfiles = new();
    public ObservableCollection<FurnitureProfileForm> AllProfiles
    {
        get => _allProfiles;
        set { _allProfiles = value; OnPropertyChanged(); }
    }

    // 3. Количественные показатели
    private int _totalLdspCount;
    public int TotalLdspCount
    {
        get => _totalLdspCount;
        set { _totalLdspCount = value; OnPropertyChanged(); }
    }

    private int _totalEdgeCount;
    public int TotalEdgeCount
    {
        get => _totalEdgeCount;
        set { _totalEdgeCount = value; OnPropertyChanged(); }
    }

    // 4. Ценовые показатели (услуги)
    private decimal _workshopCost; public decimal WorkshopCost { get => _workshopCost; set { _workshopCost = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalProjectPrice)); } }
    private decimal _packagingCost; public decimal PackagingCost { get => _packagingCost; set { _packagingCost = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalProjectPrice)); } }
    private decimal _installationCost; public decimal InstallationCost { get => _installationCost; set { _installationCost = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalProjectPrice)); } }
    private decimal _deliveryCost; public decimal DeliveryCost { get => _deliveryCost; set { _deliveryCost = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalProjectPrice)); } }
    private decimal _liftingCost; public decimal LiftingCost { get => _liftingCost; set { _liftingCost = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalProjectPrice)); } }
    private decimal _garbageRemovalCost; public decimal GarbageRemovalCost { get => _garbageRemovalCost; set { _garbageRemovalCost = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalProjectPrice)); } }

    private decimal _totalProjectPrice;     public decimal TotalProjectPrice { get => _totalProjectPrice; set { _totalProjectPrice = value; OnPropertyChanged(); } }

    private bool _isMaterialReady = false;
    public bool IsMaterialReady
    {
        get => _isMaterialReady;
        set
        {
            _isMaterialReady = value;
            OnPropertyChanged();
        }
    }

    private bool _isFurnitureReady = false;
    public bool IsFurnitureReady
    {
        get => _isFurnitureReady;
        set
        {
            _isFurnitureReady = value;
            OnPropertyChanged();
        }
    }


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


    private ProjectStatus _status = ProjectStatus.New;
    public ProjectStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged();
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

    [JsonIgnore]
    public decimal RemainingBalance => TotalProjectPrice - Prepayment;
    public ProjectData()
    {
        Objects.CollectionChanged += (s, e) => RecalculateTotals();
    }

    /// <summary>
    /// Суммирует все поля Furniture из каждого ObjectData, Пересчитывает все стоимостные показатели проекта.
    /// </summary>
    public void RecalculateTotals()
    {
        if (Objects.Count == 0)
            return;

        var sumFurniture = new FurnitureForm();

        // Локальные переменные для промежуточных расчетов
        decimal tempWorkshop = 0;
        decimal tempPackaging = 0;
        decimal tempInstallation = 0;
        decimal tempProjectPrice = 0;
        int doorCount = 0;

        AllProfiles.Clear();

        foreach (var obj in Objects)
        {
            // 1. Суммируем фурнитуру (используем ваш метод, но чуть компактнее)
            if (obj.Furniture != null)
            {
                var f = obj.Furniture;
                sumFurniture.Hdf = (sumFurniture.Hdf ?? 0) + (f.Hdf ?? 0);
                sumFurniture.NaprBez = (sumFurniture.NaprBez ?? 0) + (f.NaprBez ?? 0);
                sumFurniture.NaprS = (sumFurniture.NaprS ?? 0) + (f.NaprS ?? 0);
                sumFurniture.PetliBez = (sumFurniture.PetliBez ?? 0) + (f.PetliBez ?? 0);
                sumFurniture.PetliS = (sumFurniture.PetliS ?? 0) + (f.PetliS ?? 0);
                sumFurniture.Skoba = (sumFurniture.Skoba ?? 0) + (f.Skoba ?? 0);
                sumFurniture.Nakladnaya = (sumFurniture.Nakladnaya ?? 0) + (f.Nakladnaya ?? 0);
                sumFurniture.Knopka = (sumFurniture.Knopka ?? 0) + (f.Knopka ?? 0);
                sumFurniture.Gola = (sumFurniture.Gola ?? 0) + (f.Gola ?? 0);
                sumFurniture.Truba = (sumFurniture.Truba ?? 0) + (f.Truba ?? 0);
                sumFurniture.GazLift = (sumFurniture.GazLift ?? 0) + (f.GazLift ?? 0);
                sumFurniture.Kruchki = (sumFurniture.Kruchki ?? 0) + (f.Kruchki ?? 0);
                sumFurniture.Podsvetka = (sumFurniture.Podsvetka ?? 0) + (f.Podsvetka ?? 0);
            }

            if (obj.Furniture?.Profiles != null && obj.Furniture.Profiles.Count > 0)
            {
                var profile = obj.Furniture.Profiles[0]; // Берем первый (и единственный) профиль

                var profileCopy = new FurnitureProfileForm
                {
                    TopGuideName = profile.TopGuideName,
                    TopGuideSize = profile.TopGuideSize,
                    TopGuideCount = profile.TopGuideCount,
                    BottomGuideName = profile.BottomGuideName,
                    BottomGuideSize = profile.BottomGuideSize,
                    BottomGuideCount = profile.BottomGuideCount,
                    VerticalSlimName = profile.VerticalSlimName,
                    VerticalSlimSize = profile.VerticalSlimSize,
                    VerticalSlimCount = profile.VerticalSlimCount,
                    NarrowFrameName = profile.NarrowFrameName,
                    NarrowFrameSize = profile.NarrowFrameSize,
                    NarrowFrameCount = profile.NarrowFrameCount,
                    WideFrameName = profile.WideFrameName,
                    WideFrameSize = profile.WideFrameSize,
                    WideFrameCount = profile.WideFrameCount,
                    MiddleFrameName = profile.MiddleFrameName,
                    MiddleFrameSize = profile.MiddleFrameSize,
                    MiddleFrameCount = profile.MiddleFrameCount
                };
                AllProfiles.Add(profileCopy);
            }

            // 2. Суммируем стоимости объектов
            if (obj.Summary != null)
            {
                var cost = obj.Summary;
                tempWorkshop += (decimal)cost.WorkshopCost;
                tempPackaging += (decimal)cost.PackagingCost;
                tempInstallation += (decimal)cost.InstallationServiceCost; // Исправлено с *= на +=

                // Суммируем только внутренние затраты объекта
                //tempProjectPrice += (decimal)(cost.TotalFurnitureCost + cost.TotalSoftlyCost +cost.TotalDoorCost + cost.TotalFasadCost + cost.EdgingCost + cost.LdspCost);
                tempProjectPrice += (decimal)cost.TotalCost;
            }

            // 3. Считаем двери
            if (obj.DoorData != null)
            {
                doorCount += obj.DoorData.DoorCount;
            }
        }

        // 4. Внешние расходы (PriceList)
        var price = new PriceList();
        var tempDelivery = (decimal)price.Delivery;
        var tempGarbage = (decimal)price.GarbageRemoval;

        // Расчет подъема по вашей формуле
        var tempLifting = tempInstallation * 0.28m + doorCount * 333;
        if (tempLifting < 1500) tempLifting = 1500;

        // 5. Итоговое присвоение свойствам (вызывает UI Update один раз)
        TotalFurniture = sumFurniture;
        WorkshopCost = (int)tempWorkshop;
        PackagingCost = (int)tempPackaging;
        InstallationCost = (int)tempInstallation;
        DeliveryCost = (int)tempDelivery;
        GarbageRemovalCost = (int)tempGarbage;
        LiftingCost = (int)tempLifting;

        // Финальная сумма проекта
        TotalProjectPrice = tempProjectPrice +1.05m*( tempDelivery + tempGarbage + tempLifting);

        // Дополнительные счетчики
        TotalLdspCount = (int)Objects.Sum(x => x.Summary?.TotalLdspCount ?? 0);
        TotalEdgeCount = (int)Objects.Sum(x => x.Summary?.TotalEdgeCount ?? 0);

        OnPropertyChanged(nameof(TotalProjectPrice));
    }

    [JsonIgnore]
    private bool _isExpanded;
    public bool IsExpanded
    {
        get => _isExpanded;
        set { _isExpanded = value; OnPropertyChanged(); }
    }

    // Реализация интерфейса без базового класса
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}