
namespace MyApp1;

public enum FasadTypeBd
{
    Standard,
    AGT,
    Mirror,
    Aluminium,
    LDSP,
    NonStandard
}

public enum FrezType
{
    Freza,
    Mylo
}

public enum OtherType // Для таблицы Door, так как там есть Type_Other
{
    Type1,
    Type2
}

public enum SelectedCategory
{
    None,
    Standard,
    Peretyazhka,
    Karetka
}

public enum ShieldType
{
    Peretyazhka,
    Karetka
}

public enum ProjectStatus
{
    New = 0,        // Новый
    InProgress = 1, // В работе
    Completed = 2   // Завершен
}
public enum WorkPosition
{

    Measurer = 0,       // Замерщик
    Storekeeper = 1,    // Складовщик
    WorkshopWorker = 2, // Цеховик
    Manager = 3,        // Менеджер
    Installer = 4       // Установщик
}

public enum MaterialType
{
    LDSP,
    MDF,
    HDF,
    AGT
}

// Перечисление типов раскроя
public enum CuttingType
{
    Ldsp = 0,   // Корпус (ЛДСП)
    FAgt = 1,   // Фасады AGT
    FLdsp = 2   // Фасады ЛДСП
}

public enum TaskType
{
    Cutting = 1,   // Это карта раскроя (ЛДСП)
    Milling = 2,   // Это фрезеровка/МДФ (отдельный список)
    Doors = 3      // Двери
}

public enum ProductionTaskStatus
{
    // Общий статус "Не готово"
    Pending = 0,

    // Специфично для Раскроя (ЛДСП)
    CutCompleted = 10,  // Распилили (но еще не кромили)

    // Финальный статус для ВСЕХ (МДФ, Двери, Кромка)
    Ready = 20          // Готово полностью
}