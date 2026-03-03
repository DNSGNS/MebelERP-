using CommunityToolkit.Maui.Core;
using MyApp1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;

namespace MyApp1;

public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ProjectName { get; set; } = string.Empty;
    public string CreatorName { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; } = DateTime.Now;

    //public DateTime InstallTime { get; set; }


    public ProjectStatus Status { get; set; } = ProjectStatus.New;


    [Column(TypeName = "decimal(18,2)")]
    public decimal WorkshopCost { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PackagingCost { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal InstallationCost { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DeliveryCost { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal LiftingCost { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal GarbageRemovalCost { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalProjectPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Prepayment { get; set; }

    // Один проект может содержать несколько заказов (объектов)
    public List<ProjectObject> ProjectObjects { get; set; } = new();

    public List<ProjectWork> ProjectWorks { get; set; } = new();

    public ProjectWarehouse? Warehouse { get; set; }

    public List<ProjectInstallDate> InstallDates { get; set; } = new();
}

public class CompletedProject : INotifyPropertyChanged
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreateTime { get; set; } // Дата созд.

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalProjectPrice { get; set; } // Стоимость

    [Column(TypeName = "decimal(18,2)")]
    public decimal Prepayment { get; set; } // Предоплата

    public decimal RemainingBalance => TotalProjectPrice - Prepayment; // Остаток (вычисляемый)

    public string ProjectName { get; set; } = string.Empty; // Адрес (название проекта)
    public string CreatorName { get; set; } = string.Empty; // Создатель

    public DateTime InstallTime { get; set; } // Дата уст.

    // Храним имена установщиков строкой для истории (например: "Алексей, Дмитрий")
    public string InstallersSnapshot { get; set; } = string.Empty;


    public List<CompletedProjectObject> ProjectObjects { get; set; } = new();
    // 1. Фурнитура
    public List<FurnitureCompletedDetail> FurnitureCompletedDetails { get; set; } = new();

    // 2. Фотографии (ссылка на новую таблицу)
    public List<ProjectImage> Images { get; set; } = new();

    private bool _isSelected;
    [NotMapped] // Не создавать колонку в БД
    [JsonIgnore] // Не отправлять/получать по API
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class ProjectImage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string ImageName { get; set; } = string.Empty; // Имя фотографии

    public byte[] ImageData { get; set; } // Сама фотография (BLOB)

    // Внешний ключ на завершенный проект
    public Guid CompletedProjectId { get; set; }
    [JsonIgnore]
    public CompletedProject? CompletedProject { get; set; }
}

public class ProjectWarehouse
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    // Связь 1-к-1 с проектом
    public Guid ProjectId { get; set; }
    [JsonIgnore]
    public Project? Project { get; set; }
    public bool IsMaterialReady { get; set; }
    public bool IsFurnitureReady { get; set; }
}

public class ProjectObject // Переименовано из ProjectObject
{
    public int Id { get; set; }
    public Guid ProjectId { get; set; }
    public string OrderName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")] // Для денег лучше использовать decimal
    public decimal TotalCost { get; set; }

    public byte[]? ImageData { get; set; }

    // Связи: 
    [JsonIgnore]
    public Project? Project { get; set; }

    public List<LdspDetail> LdspDetails { get; set; } = new();
    public List<FasadDetail> FasadDetails { get; set; } = new();
    public List<DoorDetail> DoorDetails { get; set; } = new();
    public List<SoftlyDetail> SoftlyDetails { get; set; } = new();
    public List<FurnitureDetail> FurnitureDetails { get; set; } = new();
    public List<Report> Reports { get; set; } = new();
    public List<CuttingRep> CuttingRep { get; set; } = new();
}

public class LdspDetail
{
    public int Id { get; set; }
    public int ProjectObjectId { get; set; }
    public double Length { get; set; }
    public double Width { get; set; }
    public int Count { get; set; }
    public double Area { get; set; }

    [JsonIgnore]
    public ProjectObject? ProjectObject { get; set; } // Ссылка на родителя
}

public class FasadDetail
{
    public int Id { get; set; }
    public int ProjectObjectId { get; set; }
    public double Length { get; set; }
    public double Width { get; set; }
    public int Count { get; set; }
    public double Area { get; set; }
    public FasadTypeBd Type { get; set; } // Types enum
    public string Color { get; set; } = string.Empty;
    public string Frez { get; set; } = string.Empty;
    public FrezType FrezType { get; set; } // TypesFrez enum

    [JsonIgnore]
    public ProjectObject? ProjectObject { get; set; }
}

public class DoorDetail
{
    public int Id { get; set; }
    public int? ProjectObjectId { get; set; }
    public int? CompletedProjectObjectId { get; set; }
    public int InstallationTypeIndex { get; set; }
    public double OpeningHeight { get; set; }
    public double OpeningWidth { get; set; }
    public int ArrangementIndex { get; set; }
    public int ColorIndex { get; set; }
    public int MiddleFramesCountIndex { get; set; }

    public List<SlimInsertDetail> Inserts { get; set; } = new();

    [JsonIgnore]
    public ProjectObject? ProjectObject { get; set; }
    [JsonIgnore]
    public CompletedProjectObject? CompletedProjectObject { get; set; }
}

public class SlimInsertDetail
{
    public int Id { get; set; }
    public int DoorDetailId { get; set; }

    public int Index { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MaterialIndex { get; set; }
    public double Height { get; set; }
    public double Width { get; set; }
    public int Count { get; set; }

    [JsonIgnore]
    public DoorDetail? DoorDetail { get; set; }
}

// Таблица "Мягкие элементы"
public class SoftlyDetail
{
    public int Id { get; set; }
    public int ProjectObjectId { get; set; }
    public double Length { get; set; }
    public double Width { get; set; }
    public int Count { get; set; }
    public double Area { get; set; }
    public SelectedCategory SelectedCategory { get; set; }
    public ShieldType ShieldType { get; set; }
    public int Tabletop { get; set; }
    public int Apron { get; set; }
    public int Additional { get; set; }
    public bool HasBase { get; set; }
    public bool HasDryer { get; set; }


    [JsonIgnore]
    public ProjectObject? ProjectObject { get; set; }
}

// Таблица "Фурнитура"
public class FurnitureDetail
{
    public int Id { get; set; }
    public int ProjectObjectId { get; set; }
    public double Hdf { get; set; }
    public double NaprBez { get; set; }
    public double NaprS { get; set; }
    public double PetliBez { get; set; }
    public double PetliS { get; set; }
    public double Skoba { get; set; }
    public double Nakladnaya { get; set; }
    public double Knopka { get; set; }
    public double Gola { get; set; }
    public double Truba { get; set; }
    public double GazLift { get; set; }
    public double Kruchki { get; set; }
    public double Podsvetka { get; set; }
    public string? Comment { get; set; }

    public List<FurnitureProfileDetail> Profiles { get; set; } = new();
    [JsonIgnore]
    public ProjectObject? ProjectObject { get; set; }
}

public class FurnitureProfileDetail
{
    public int Id { get; set; }
    public int? FurnitureDetailId { get; set; }

    public int? FurnitureCompletedDetailId { get; set; }
    // 1. Направляющая верхняя
    public string TopGuideName { get; set; } = string.Empty;
    public double TopGuideSize { get; set; }
    public int TopGuideCount { get; set; }

    // 2. Направляющая нижняя
    public string BottomGuideName { get; set; } = string.Empty;
    public double BottomGuideSize { get; set; }
    public int BottomGuideCount { get; set; }

    // 3. Профиль вертикальный Slim
    public string VerticalSlimName { get; set; } = string.Empty;
    public double VerticalSlimSize { get; set; }
    public int VerticalSlimCount { get; set; }

    // 4. Рамка узкая
    public string NarrowFrameName { get; set; } = string.Empty;
    public double NarrowFrameSize { get; set; }
    public int NarrowFrameCount { get; set; }

    // 5. Рамка широкая
    public string WideFrameName { get; set; } = string.Empty;
    public double WideFrameSize { get; set; }
    public int WideFrameCount { get; set; }

    // 6. Рамка средняя
    public string MiddleFrameName { get; set; } = string.Empty;
    public double MiddleFrameSize { get; set; }
    public int MiddleFrameCount { get; set; }

    [JsonIgnore]
    public FurnitureDetail? FurnitureDetail { get; set; }
    [JsonIgnore]
    public FurnitureCompletedDetail? FurnitureCompletedDetail { get; set; }
}

// Таблица "Фурнитура (завершенная)"
public class FurnitureCompletedDetail
{
    public int Id { get; set; }
    public double Hdf { get; set; }
    public double NaprBez { get; set; }
    public double NaprS { get; set; }
    public double PetliBez { get; set; }
    public double PetliS { get; set; }
    public double Skoba { get; set; }
    public double Nakladnaya { get; set; }
    public double Knopka { get; set; }
    public double Gola { get; set; }
    public double Truba { get; set; }
    public double GazLift { get; set; }
    public double Kruchki { get; set; }
    public double Podsvetka { get; set; }
    //public string? Comment { get; set; }

    public List<FurnitureProfileDetail> Profiles { get; set; } = new();
    public Guid CompletedProjectId { get; set; }
    [JsonIgnore]
    public CompletedProject? CompletedProject { get; set; }
}

// Таблица "Отчет"
public class Report
{
    public int Id { get; set; }
    public int ProjectObjectId { get; set; }

    public double TotalAreaLdsp { get; set; }
    public int TotalLdspCount { get; set; }
    public int TotalEdgeCount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal LdspCost { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal EdgingCost { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalFasadCost { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalDoorCost { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalSoftlyCost { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal WorkshopCost { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal PackagingCost { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal InstallationServiceCost { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCost { get; set; }

    [JsonIgnore]
    public ProjectObject? ProjectObject { get; set; }

}

//таблица раскроя
public class CuttingRep
{
    public int Id { get; set; }
    public int? ProjectObjectId { get; set; }
    public int? CompletedProjectObjectId { get; set; }
    public CuttingType Type { get; set; }

    // Параметры листа
    public double SheetLength { get; set; }
    public double SheetWidth { get; set; }
    public double SheetArea { get; set; }

    public string? MaterialColor { get; set; }

    // Итоги раскроя
    public int TotalSheets { get; set; }
    public double TotalSheetArea { get; set; }
    public int TotalPartsCount { get; set; }
    public double TotalPartsArea { get; set; }

    // Кромка 1
    public string Edge1Name { get; set; } = string.Empty;
    public double Edge1Thickness { get; set; }
    public double TotalEdge1 { get; set; }

    // Кромка 2
    public string Edge2Name { get; set; } = string.Empty;
    public double Edge2Thickness { get; set; }
    public double TotalEdge2 { get; set; }

    // Связь с родителем
    public ProjectObject? ProjectObject { get; set; }
    [JsonIgnore]
    public CompletedProjectObject? CompletedProjectObject { get; set; }
    public List<CuttingPartItem> Details { get; set; } = new();

    // Связь с листами раскроя (чертежами)
    public List<SheetLayoutDetail> Sheets { get; set; } = new();
}

public class CompletedProjectObject
{
    public int Id { get; set; }

    public string OrderName { get; set; } = string.Empty;

    // Связь с родительским завершенным проектом
    public Guid CompletedProjectId { get; set; }
    [JsonIgnore]
    public CompletedProject? CompletedProject { get; set; }

    // 1-к-1
    public DoorDetail? DoorDetail { get; set; }

    public List<CuttingRep> CuttingReports { get; set; } = new();
}


// Таблица для хранения исходного списка деталей раскроя
public class CuttingPartItem
{
    public int Id { get; set; }
    public int CuttingRepId { get; set; } // Внешний ключ

    public string? Color { get; set; }

    public int Length { get; set; }
    public int Width { get; set; }
    public int Count { get; set; }
    public bool CanRotate { get; set; }

    // Кромка Тип 1 (флаги сторон)
    public bool E1L1 { get; set; }
    public bool E1L2 { get; set; }
    public bool E1W1 { get; set; }
    public bool E1W2 { get; set; }

    // Кромка Тип 2 (флаги сторон)
    public bool E2L1 { get; set; }
    public bool E2L2 { get; set; }
    public bool E2W1 { get; set; }
    public bool E2W2 { get; set; }

    // Навигационное свойство обратно к отчету
    public CuttingRep? CuttingRep { get; set; }
}


public class SheetLayoutDetail
{
    public int Id { get; set; }
    public int CuttingDetailId { get; set; }

    public string? ColorName { get; set; }

    public int SheetNumber { get; set; }

    // Если нужно хранить специфические данные раскроя в JSON
    public string? LayoutDataJson { get; set; }

    public CuttingRep? CuttingDetail { get; set; }
}

// Таблица работников
public class WorkMans
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; } = string.Empty;

    public WorkPosition Position { get; set; }

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    // Связь с таблицей работ
    public List<ProjectWork> ProjectWorks { get; set; } = new();

    public WorkManAccess? Access { get; set; }  // сделать nullable
}

public class WorkManAccess
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    // Ссылка на рабочего
    [Required]
    public Guid WorkManId { get; set; }

    [JsonIgnore]
    [ForeignKey("WorkManId")]
    public WorkMans? WorkMan { get; set; }  // сделать nullable

    // --- Группа: Оперативная работа ---
    public bool CanAccessSurveyor { get; set; }         // Замеры
    public bool CanAccessSalaries { get; set; }         // Зарплаты (общие)
    public bool CanAccessWarehouse { get; set; }        // Склад
    public bool CanAccessInstaller { get; set; }        // Монтаж
    public bool CanAccessWorkshop { get; set; }         // Цех
    public bool CanAccessCalendar { get; set; }          // Календарь

    // --- Группа: Управление проектами ---
    public bool CanAccessActiveProjects { get; set; }    // Менеджер 1 (Активные)
    public bool CanAccessArchiveProjects { get; set; }   // Менеджер 2 (Архив)

    // --- Группа: Журналы ---
    public bool CanAccessPersonalJournal { get; set; }   // Мой журнал (свои работы)
    public bool CanAccessAllJournals { get; set; }       // Все журналы (просмотр чужих)
    public bool CanAccessExpenses { get; set; }          // Журнал расходов (траты)

    // --- Группа: Отчеты и Производство ---
    public bool CanAccessMonthlyReport { get; set; }     // Итоговый отчет
    public bool CanAccessLayout { get; set; }            // Раскладка
    public bool CanAccessCutting { get; set; }           // Раскрой

    // --- Группа: Система ---
    public bool CanAccessUsersList { get; set; }         // Управление пользователями
    public bool CanAccessProfile { get; set; } = true;   // Профиль (обычно доступен всем)
}

public class ProjectWork
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    // Связь с проектом
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    // Связь с работником
    public Guid WorkManId { get; set; }
    public WorkMans WorkMan { get; set; } = null!;

    // Поля для расчета будущих зарплат (задачи)
    // Можно использовать bool (делал/не делал) или decimal (сумма к выплате)

    [Column(TypeName = "decimal(18,2)")]
    public decimal DidWorkshop { get; set; }       // Цех

    [Column(TypeName = "decimal(18,2)")]
    public decimal DidPackaging { get; set; }      // Упаковка

    [Column(TypeName = "decimal(18,2)")]
    public decimal DidSaw { get; set; }             // Пила

    [Column(TypeName = "decimal(18,2)")]
    public decimal DidEdging { get; set; }          // Кромление

    [Column(TypeName = "decimal(18,2)")]
    public decimal DidAdditive { get; set; }        // Присадка


    public int DidDoorCanvas { get; set; }       //Двери-купе полотно


    public int DidDoorSectional { get; set; } //Двери-купе секционные


    [Column(TypeName = "decimal(18,2)")]
    public decimal DidMeasurement { get; set; }    // Замер

    public decimal DidInstallation { get; set; }   // Установка

    [Column(TypeName = "decimal(18,2)")]
    public decimal DidGrindingSoap { get; set; } //Шлифовка мыло

    [Column(TypeName = "decimal(18,2)")]
    public decimal DidGrindingFrez { get; set; } //Шлифовка фреза

    [Column(TypeName = "decimal(18,2)")]
    public decimal DidMilling { get; set; } //Фрезеровка


    [Column(TypeName = "decimal(18,2)")]
    public decimal Additionally { get; set; } //Доп



    public MaterialType? UsedMaterialType { get; set; } // Использованный материал

    // Поле для итоговой выплаты за этот конкретный выезд/работу
    [Column(TypeName = "decimal(18,2)")]
    public decimal CalculatedSalary { get; set; }

    public string? Comment { get; set; }

    public DateTime DatePerformed { get; set; } = DateTime.Now;
}


// Таблица Справочник типов затрат
public class ExpenseCategory : INotifyPropertyChanged
{
    private string _name = string.Empty;

    public int Id { get; set; }

    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged(); // Уведомляем UI об изменении
            }
        }
    }

    // Навигационное свойство (если есть)
    [JsonIgnore]
    public List<Expense>? Expenses { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

// Таблица Сами транзакции (Затраты)
public class Expense
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public decimal Amount { get; set; }

    // Внешний ключ на категорию
    public int CategoryId { get; set; }

    // Навигационное свойство
    public ExpenseCategory? Category { get; set; }

}

public class ProductionTask
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    // Ссылки для удобства (чтобы быстро найти задачи проекта)
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }

    public int ProjectObjectId { get; set; } // Ссылка на "Шкаф в спальню"

    public string WorkerName { get; set; } = string.Empty; // Имя работника, который выполняет задачу

    public bool IsTaken { get; set; } = false; // Взял ли работник задачу в работу

    // Тип задачи (чтобы ты знал, на какой вкладке это рисовать)
    public TaskType Type { get; set; } // (Cutting / Milling / Doors) - возьми из прошлого ответа

    // Твой статус (Pending -> CutCompleted -> Ready)
    public ProductionTaskStatus Status { get; set; } = ProductionTaskStatus.Pending;

    // Время завершения (для истории)
    public DateTime? FinishedAt { get; set; }

    // Сколько штук нужно сделать в рамках этой конкретной задачи
    public int Quantity { get; set; }

    // Сколько из них в итоге оказались браком (заполняется при завершении)
    public int ScrapCount { get; set; } = 0;

    // Если эта задача — переделка брака, здесь будет ID оригинальной задачи
    public Guid? ParentTaskId { get; set; }

    // Пометка, что это переделка
    public bool IsRemake { get; set; } = false;


    // === ВТОРИЧНЫЕ КЛЮЧИ (Связи) ===

    // 1. РАСКРОЙ: Ссылаемся ТОЛЬКО на карту раскроя. Детали достанем через Include.
    public int? CuttingRepId { get; set; }
    public CuttingRep? CuttingRep { get; set; }

    // 2. МДФ: Ссылаемся на конкретную деталь (чтобы отмечать поштучно)
    public int? FasadDetailId { get; set; }
    public FasadDetail? FasadDetail { get; set; }

    // 3. ДВЕРИ: Ссылаемся на конкретную дверь
    public int? DoorDetailId { get; set; }
    public DoorDetail? DoorDetail { get; set; }
}
public class MonthlyFinanceSummary
{
    [Key]
    public int Id { get; set; }

    public int Year { get; set; }
    public int Month { get; set; }

    // Мы храним название для удобства, хотя его можно вычислить
    public string MonthName { get; set; } = string.Empty;

    public int OrderCount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalSales { get; set; }


    [Column(TypeName = "decimal(18,2)")]
    public decimal Realization { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalExpenses { get; set; }

    // Поле для контроля актуальности данных
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Вычисляемые свойства (не хранятся в БД, но полезны в коде)
    [NotMapped]
    public decimal AverageOrderValue => OrderCount > 0 ? Math.Round(TotalSales / OrderCount, 2) : 0;
    [NotMapped]
    public decimal NetProfit => Realization - TotalExpenses;

    [NotMapped]
    public decimal Margin => TotalSales > 0
        ? Math.Round((NetProfit / TotalSales) * 100, 2)
        : 0;
}


public class ProjectInstallDate
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public DateTime Date { get; set; }
}