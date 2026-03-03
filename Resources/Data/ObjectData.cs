using MyApp1.Resources.Other;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace MyApp1;

public class ObjectData : INotifyPropertyChanged
{
    // 1. ЛДСП
    public ObservableCollection<LDSPForm> LdspForms { get; set; } = new();



    // 2. Фурнитура
    public FurnitureForm Furniture { get; set; } = new();

    // 3. Фасады
    public ObservableCollection<FasadForm> StandardFasads { get; set; } = new();
    public ObservableCollection<FasadForm> SpecialFasads { get; set; } = new();
    public ObservableCollection<FasadForm> NonStandardFasads { get; set; } = new();



    // 4. Двери
    public DoorForm DoorData { get; set; } = new();
    public DoorSlimLineForm DoorSlimLineData { get; set; } = new();

    // 5. Мягкие элементы
    public SoftlyForm SoftlyData { get; set; } = new();



    public CuttingData CuttingLdsp { get; set; } = new();
    public CuttingData CuttingFAgt { get; set; } = new();    // Для фасадов AGT
    public CuttingData CuttingFLdsp { get; set; } = new();   // Для фасадов Лдсп




    public void SyncAgtToCutting()
    {
        // 1. Отбираем только валидные фасады AGT
        var validSources = SpecialFasads
            .Where(f => f.SelectedType == FasadType.AGT && f.Length.HasValue && f.Width.HasValue)
            .ToList();

        var cuttingDetails = CuttingFAgt.DetailsForm.Details;

        // 2. Синхронизируем по индексу
        for (int i = 0; i < validSources.Count; i++)
        {
            var source = validSources[i];
            int newLength = (int)Math.Round(source.Length.Value);
            int newWidth = (int)Math.Round(source.Width.Value);
            int newCount = source.Count ?? 1;
            string newColor = source.Color;

            if (i < cuttingDetails.Count)
            {
                // Обновляем существующую деталь (сохраняя настройки кромки)
                var existing = cuttingDetails[i];
                existing.Id = i + 1; // Устанавливаем порядковый номер
                existing.Length = newLength;
                existing.Width = newWidth;
                existing.Count = newCount;
                existing.Color = newColor;
            }
            else
            {
                // Создаем новую, если список в раскрое короче
                cuttingDetails.Add(new CuttingDetails
                {
                    Id = i + 1,
                    Length = newLength,
                    Width = newWidth,
                    Count = newCount,
                    Color = newColor,
                    CanRotate = true
                });
            }
        }

        // 3. Удаляем лишние строки, если в фасадах их стало меньше
        while (cuttingDetails.Count > validSources.Count)
        {
            cuttingDetails.RemoveAt(cuttingDetails.Count - 1);
        }

        CuttingFAgt.RefreshTotals();
    }

    public void SyncFLdspToCutting()
    {
        // 1. Отбираем только валидные фасады ЛДСП
        var validSources = SpecialFasads
            .Where(f => f.SelectedType == FasadType.LDSP && f.Length.HasValue && f.Width.HasValue)
            .ToList();

        var cuttingDetails = CuttingFLdsp.DetailsForm.Details;

        for (int i = 0; i < validSources.Count; i++)
        {
            var source = validSources[i];
            int newLength = (int)Math.Round(source.Length.Value);
            int newWidth = (int)Math.Round(source.Width.Value);
            int newCount = source.Count ?? 1;
            string newColor = source.Color;

            if (i < cuttingDetails.Count)
            {
                var existing = cuttingDetails[i];
                existing.Id = i + 1;
                existing.Length = newLength;
                existing.Width = newWidth;
                existing.Count = newCount;
                existing.Color = newColor;
            }
            else
            {
                cuttingDetails.Add(new CuttingDetails
                {
                    Id = i + 1,
                    Length = newLength,
                    Width = newWidth,
                    Count = newCount,
                    Color = newColor,
                    CanRotate = true
                });
            }
        }

        while (cuttingDetails.Count > validSources.Count)
        {
            cuttingDetails.RemoveAt(cuttingDetails.Count - 1);
        }

        CuttingFLdsp.RefreshTotals();
    }

    public void SyncSlimLineToFurniture()
    {
        var slim = this.DoorSlimLineData;

        // Базовая проверка: если данных в SlimLine еще нет, ничего не делаем
        if (slim.Profiles.Count < 6) return;

        if (this.Furniture.Profiles.Count == 0)
        {
            this.Furniture.Profiles.Add(new FurnitureProfileForm());
        }

        var profileToUpdate = this.Furniture.Profiles[0];

        profileToUpdate.TopGuideName = slim.Profiles[0].Name;
        profileToUpdate.TopGuideSize = slim.Profiles[0].Size;
        profileToUpdate.TopGuideCount = slim.Profiles[0].Count;

        profileToUpdate.BottomGuideName = slim.Profiles[1].Name;
        profileToUpdate.BottomGuideSize = slim.Profiles[1].Size;
        profileToUpdate.BottomGuideCount = slim.Profiles[1].Count;

        profileToUpdate.VerticalSlimName = slim.Profiles[2].Name;
        profileToUpdate.VerticalSlimSize = slim.Profiles[2].Size;
        profileToUpdate.VerticalSlimCount = slim.Profiles[2].Count;

        profileToUpdate.NarrowFrameName = slim.Profiles[3].Name;
        profileToUpdate.NarrowFrameSize = slim.Profiles[3].Size;
        profileToUpdate.NarrowFrameCount = slim.Profiles[3].Count;

        profileToUpdate.WideFrameName = slim.Profiles[4].Name;
        profileToUpdate.WideFrameSize = slim.Profiles[4].Size;
        profileToUpdate.WideFrameCount = slim.Profiles[4].Count;

        profileToUpdate.MiddleFrameName = slim.Profiles[5].Name;
        profileToUpdate.MiddleFrameSize = slim.Profiles[5].Size;
        profileToUpdate.MiddleFrameCount = slim.Profiles[5].Count;
    }

    // Метод для синхронизации размеров из ЛДСП в Раскрой
    public void SyncLdspToCutting()
    {
        // 1. Сначала отбираем только валидные строки из ЛДСП (где есть размеры и кол-во)
        var validSources = LdspForms
            .Where(x => x.Length.HasValue && x.Width.HasValue && x.Count.HasValue)
            .ToList();

        var cuttingDetails = CuttingLdsp.DetailsForm.Details;

        // 2. Проходим по списку и синхронизируем данные
        for (int i = 0; i < validSources.Count; i++)
        {
            var source = validSources[i];

            // Получаем актуальные размеры из ЛДСП
            int newLength = (int)Math.Round(source.Length.Value);
            int newWidth = (int)Math.Round(source.Width.Value);
            int newCount = source.Count.Value;

            if (i < cuttingDetails.Count)
            {
                // СЦЕНАРИЙ А: Деталь под этим индексом уже была.
                // Мы обновляем только Размеры и Количество.
                // Настройки кромки (E1L1...) и вращения (CanRotate) НЕ трогаем — они сохраняются.

                var existingDetail = cuttingDetails[i];

                // Обновляем свойства (NotifyPropertyChanged сработает автоматически внутри сеттеров)
                existingDetail.Length = newLength;
                existingDetail.Width = newWidth;
                existingDetail.Count = newCount;
                existingDetail.Id = i + 1; // На всякий случай актуализируем ID
            }
            else
            {
                // СЦЕНАРИЙ Б: Это новая деталь (в ЛДСП добавили строк).
                // Создаем её с нуля.
                var newDetail = new CuttingDetails
                {
                    Id = i + 1,
                    Length = newLength,
                    Width = newWidth,
                    Count = newCount,
                    CanRotate = true // Значение по умолчанию
                };
                cuttingDetails.Add(newDetail);
            }
        }

        // 3. Если в ЛДСП удалили строки (стало меньше, чем было в раскрое),
        // удаляем лишние детали с конца списка раскроя.
        while (cuttingDetails.Count > validSources.Count)
        {
            cuttingDetails.RemoveAt(cuttingDetails.Count - 1);
        }

        // Обновляем расчеты в CuttingData (длины кромок и т.д.)
        CuttingLdsp.RefreshTotals();
    }


    private string _imagePath;
    public string ImagePath
    {
        get => _imagePath;
        set
        {
            _imagePath = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasImage)); // Уведомляем интерфейс, что надо показать/скрыть картинку
        }
    }

    // Вспомогательное свойство для скрытия элемента Image, если пути нет
    [JsonIgnore]
    public bool HasImage => !string.IsNullOrEmpty(ImagePath);

    private string _projectName;
    public string ProjectName
    {
        get => _projectName;
        set
        {
            _projectName = value;
            OnPropertyChanged();
        }
    }


    [JsonIgnore]
    private bool _isExpanded;
    public bool IsExpanded
    {
        get => _isExpanded;
        set { _isExpanded = value; OnPropertyChanged(); }
    }


    // Результат расчета
    private OrderSummary _summary;
    public OrderSummary Summary
    {
        get => _summary;
        set { _summary = value; OnPropertyChanged(); }
    }

    // Метод для запуска расчета и сохранения результата внутрь этого объекта
    public void UpdateCalculation(PriceList prices)
    {
        // Вызываем созданный ранее статический калькулятор
        Summary = CalculatorService.Calculate(this, prices);
    }

    private string _objectName = "Новый объект";
    public string ObjectName
    {
        get => _objectName;
        set { _objectName = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}