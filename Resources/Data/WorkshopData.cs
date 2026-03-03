using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyApp1;

public class WorkshopData : INotifyPropertyChanged
{
    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    // 1. Вкладка Раскрой
    public ObservableCollection<CuttingWorkshopItem> CuttingTasks { get; set; } = new();

    // 2. Вкладка Кромка
    public ObservableCollection<CuttingWorkshopItem> EdgeTasks { get; set; } = new();

    // 3. Вкладка МДФ/Фасады
    public ObservableCollection<FasadWorkshopItem> FasadTasks { get; set; } = new();

     //4. Вкладка Двери(Заглушка)
    public ObservableCollection<DoorWorkshopItem> DoorTasks { get; set; } = new();

    // Метод для заполнения данных из API
    public void FillFromApi(List<ProductionTask> tasks)
    {
        CuttingTasks.Clear();
        EdgeTasks.Clear();
        FasadTasks.Clear();
        DoorTasks.Clear();

        foreach (var t in tasks)
        {
            if (t.Type == TaskType.Cutting)
            {
                if (t.Status == ProductionTaskStatus.Pending)
                {
                    var item = MapCutting(t);
                    CuttingTasks.Add(item);
                }
                else if (t.Status == ProductionTaskStatus.CutCompleted)
                {
                    var item = MapCutting(t);
                    EdgeTasks.Add(item);
                }

            }
            else if (t.Type == TaskType.Milling && t.FasadDetail != null)
            {
                FasadTasks.Add(new FasadWorkshopItem
                {
                    Id = t.Id,
                    ProjectName = t.Project?.ProjectName ?? "Без названия",
                    CreationDate = t.Project.CreateTime,
                    IsTaken = t.IsTaken,
                    Status = t.Status,
                    Length = t.FasadDetail.Length,
                    Width = t.FasadDetail.Width,
                    Count = t.Quantity,
                    Color = t.FasadDetail.Color ?? "",
                    MillingText = t.FasadDetail.Frez
                    // SelectedEdgeType = t.FasadDetail.SelectedEdgeType
                });
            }
            else if (t.Type == TaskType.Doors) // Проверьте имя типа в вашем Enum TaskType
            {
                var doorItem = MapDoor(t);
                DoorTasks.Add(doorItem);
            }
        }
    }


    private DoorWorkshopItem MapDoor(ProductionTask t)
    {
        var item = new DoorWorkshopItem
        {
            Id = t.Id,
            ProjectName = t.Project?.ProjectName ?? "Без названия",
            ObjectName = t.Project?.ProjectObjects
                            ?.FirstOrDefault(po => po.Id == t.ProjectObjectId)
                            ?.OrderName
                            ?? "Двери купе",
            IsTaken = t.IsTaken,
            Status = t.Status,
            CreationDate = t.Project?.CreateTime ?? DateTime.Now
        };

        // === SlimLine ===
        var door = t.DoorDetail;
        if (door == null)
            return item;

        var form = new DoorSlimLineForm
        {
            OpeningHeight = door.OpeningHeight,
            OpeningWidth = door.OpeningWidth,

            SelectedInstallationTypeIndex = door.InstallationTypeIndex,
            SelectedArrangementIndex = door.ArrangementIndex,
            SelectedColorIndex = door.ColorIndex,

            // ВАЖНО: установка этого индекса создаёт Inserts
            MiddleFramesCountIndex = door.MiddleFramesCountIndex
        };

        // === Восстановление вставок ===
        if (door.Inserts != null && form.Inserts != null)
        {
            foreach (var dbInsert in door.Inserts)
            {
                var formInsert = form.Inserts.FirstOrDefault(x => x.Index == dbInsert.Index);
                if (formInsert == null)
                    continue;

                formInsert.MaterialIndex = dbInsert.MaterialIndex;

                // Первая высота расчётная — НЕ трогаем
                if (dbInsert.Index > 0)
                {
                    formInsert.Height = dbInsert.Height;
                }
            }
        }

        // Присваиваем данные двери
        item.Data = form;

        return item;
    }

    private CuttingWorkshopItem MapCutting(ProductionTask t)
    {
        var item = new CuttingWorkshopItem
        {
            Id = t.Id,
            ProjectName = t.Project?.ProjectName ?? "Без названия",
            ObjectName = t.Project?.ProjectObjects
                        ?.FirstOrDefault(po => po.Id == t.ProjectObjectId)
                        ?.OrderName
                        ?? "Раскрой",
            IsTaken = t.IsTaken,
            Status = t.Status,
            CreationDate = t.Project.CreateTime,
            // ЗАПИСЫВАЕМ ТИП: Берем его из задачи (предполагаем, что сервер его присылает)
            // Если поле на сервере называется иначе, замените t.CuttingType на нужное
            MaterialType = (CuttingType)(t.CuttingRep?.Type ?? 0)
        };

        if (t.CuttingRep == null) return item;

        // Инициализация формы данных (ваша реализация)
        item.Data = new CuttingSaveForm
        {
            SheetLength = t.CuttingRep.SheetLength,
            SheetWidth = t.CuttingRep.SheetWidth,
            SheetArea = t.CuttingRep.SheetArea,
            MaterialColor = t.CuttingRep.MaterialColor,
            TotalSheets = t.CuttingRep.TotalSheets,
            TotalSheetArea = t.CuttingRep.TotalSheetArea,
            TotalPartsCount = t.CuttingRep.TotalPartsCount,
            TotalPartsArea = t.CuttingRep.TotalPartsArea,
            Edge1Name = t.CuttingRep.Edge1Name,
            Edge1Thickness = t.CuttingRep.Edge1Thickness,
            TotalEdge1 = t.CuttingRep.TotalEdge1,
            Edge2Name = t.CuttingRep.Edge2Name,
            Edge2Thickness = t.CuttingRep.Edge2Thickness,
            TotalEdge2 = t.CuttingRep.TotalEdge2
        };

        if (t.CuttingRep.Details != null)
        {
            foreach (var dbDetail in t.CuttingRep.Details)
            {
                var uiDetail = new CuttingDetails
                {
                    // Основные параметры
                    Length = dbDetail.Length,
                    Width = dbDetail.Width,
                    Count = dbDetail.Count,
                    Color = dbDetail.Color,
                    CanRotate = dbDetail.CanRotate,

                    // Кромка 1
                    E1L1 = dbDetail.E1L1,
                    E1L2 = dbDetail.E1L2,
                    E1W1 = dbDetail.E1W1,
                    E1W2 = dbDetail.E1W2,

                    // Кромка 2
                    E2L1 = dbDetail.E2L1,
                    E2L2 = dbDetail.E2L2,
                    E2W1 = dbDetail.E2W1,
                    E2W2 = dbDetail.E2W2
                };
                item.Data.Details.Add(uiDetail);
            }
        }

        // Восстановление чертежей из JSON
        if (t.CuttingRep.Sheets != null)
        {
            foreach (var sheetDetail in t.CuttingRep.Sheets)
            {
                if (!string.IsNullOrEmpty(sheetDetail.LayoutDataJson))
                {
                    try
                    {
                        var sheetLayout = System.Text.Json.JsonSerializer.Deserialize<SheetLayout>(sheetDetail.LayoutDataJson);
                        if (sheetLayout != null)
                        {
                            sheetLayout.ColorName = sheetDetail.ColorName;
                            sheetLayout.SheetIndex = sheetDetail.SheetNumber;
                            item.Data.Sheets.Add(sheetLayout);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка десериализации: {ex.Message}");
                    }
                }
            }
        }

        return item;
    }


    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}