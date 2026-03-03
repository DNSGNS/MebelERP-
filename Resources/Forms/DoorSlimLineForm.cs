using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
namespace MyApp1;


public class DoorSlimLineForm : INotifyPropertyChanged
{
    public DoorSlimLineForm()
    {
        Inserts.CollectionChanged += Inserts_CollectionChanged;
        CheckInsertsHeights();
        Recalculate();
    }

    /// <summary>
    /// Конструктор для инициализации формы из модели данных (режим просмотра/редактирования)
    /// </summary>
    /// <param name="detail">Объект данных из БД</param>
    public DoorSlimLineForm(DoorDetail detail) : this() // Сначала вызываем конструктор по умолчанию для подписки на события
    {
        if (detail == null) return;

        // Заполняем основные свойства напрямую (используем поля, чтобы избежать лишних промежуточных пересчетов)
        _openingHeight = detail.OpeningHeight;
        _openingWidth = detail.OpeningWidth;
        _selectedInstallationTypeIndex = detail.InstallationTypeIndex;
        _selectedArrangementIndex = detail.ArrangementIndex;
        _selectedColorIndex = detail.ColorIndex;
        _middleFramesCountIndex = detail.MiddleFramesCountIndex;

        // Очищаем стандартный список вставок и наполняем его данными из модели
        if (detail.Inserts != null && detail.Inserts.Count > 0)
        {
            // Временно отписываемся от события, чтобы массовое добавление не вызывало Recalculate на каждый чих
            Inserts.CollectionChanged -= Inserts_CollectionChanged;

            Inserts.Clear();
            foreach (var insertDetail in detail.Inserts.OrderBy(i => i.Index))
            {
                Inserts.Add(new SlimInsertRow
                {
                    Index = insertDetail.Index,
                    Name = insertDetail.Name,
                    MaterialIndex = insertDetail.MaterialIndex,
                    Height = insertDetail.Height,
                    Width = insertDetail.Width,
                    Count = insertDetail.Count,
                    IsHeightEditable = insertDetail.Index > 0 // Первая вставка обычно вычисляемая
                });
            }

            // Возвращаем подписку обратно
            Inserts.CollectionChanged += Inserts_CollectionChanged;
            foreach (var item in Inserts)
            {
                item.PropertyChanged += OnRowPropertyChanged;
            }
        }

        // Вызываем итоговый пересчет, чтобы заполнилась таблица Profiles и вычислились веса
        Recalculate();
    }

    [System.Text.Json.Serialization.JsonIgnore]
    public ObjectData Parent { get; set; }


    public Action OnRequestSync { get; set; }
    private int _selectedInstallationTypeIndex = 0;

    /// <summary>
    /// Вариант установки дверей
    /// </summary>
    public int SelectedInstallationTypeIndex
    {
        get => _selectedInstallationTypeIndex;
        set { _selectedInstallationTypeIndex = value; Recalculate(); }
    }
    public List<string> InstallationTypes { get; } = new()
    {
        "Видимый верхний трек",
        "Скрытый верхний трек, корпус",
        "Скрытый верхний трек, проем"
    };

    private double _openingHeight;
    /// <summary>
    /// Высота проёма
    /// </summary>
    public double OpeningHeight
    {
        get => _openingHeight;
        set { _openingHeight = value; Recalculate(); }
    }

    private double _openingWidth;
    /// <summary>
    /// Ширина проема
    /// </summary>
    public double OpeningWidth
    {
        get => _openingWidth;
        set { _openingWidth = value; Recalculate(); }
    }

    private int _selectedArrangementIndex = 0;

    /// <summary>
    /// Расположение дверей
    /// </summary>
    public int SelectedArrangementIndex
    {
        get => _selectedArrangementIndex;
        set
        {
            _selectedArrangementIndex = value;
            Recalculate();
        }
    }
    public List<string> DoorArrangements { get; } = new()
    {
        "|----____|",
        "|----____----|",
        "|----____----____|",
        "|----____ ____----|",
        "|----____----____----|"
    };

    private int _selectedColorIndex = 0;

    /// <summary>
    /// Цвет профиля
    /// </summary>
    public int SelectedColorIndex
    {
        get => _selectedColorIndex;
        set
        {
            _selectedColorIndex = value;
            CalculateAndUpdateProfilesTable(); // Обновляем название цвета в таблице
            OnPropertyChanged();
        }
    }
    public List<string> ProfileColors { get; } = new()
    {
        "Золотой", "Хром-матовый", "Белый", "Чёрный-матовый"
    };

    private int _middleFramesCountIndex = 0;

    /// <summary>
    /// Количество средних рамок (от 0 до 4)
    /// </summary>
    public int MiddleFramesCountIndex
    {
        get => _middleFramesCountIndex;
        set
        {
            _middleFramesCountIndex = value;
            UpdateInsertsList();
            Recalculate();
            CheckInsertsHeights();
        }
    }
    public List<string> MiddleFramesOptions { get; } = new() { "0 шт.", "1 шт.", "2 шт.", "3 шт.", "4 шт." };

    // --- ВЫЧИСЛЯЕМЫЕ ПОЛЯ (Read Only) ---

    private int _doorsCount;
    /// <summary>
    /// Количество дверей
    /// </summary>
    public int DoorsCount { get => _doorsCount; private set { _doorsCount = value; OnPropertyChanged(); } }

    private int _overlapsCount;
    /// <summary>
    /// Количество перекрытий
    /// </summary>
    public int OverlapsCount { get => _overlapsCount; private set { _overlapsCount = value; OnPropertyChanged(); } }

    // Размеры двери
    private double _doorHeight;
    public double DoorHeight { get => _doorHeight; private set { _doorHeight = value; OnPropertyChanged(); } }

    private double _doorWidth;
    public double DoorWidth { get => _doorWidth; private set { _doorWidth = value; OnPropertyChanged(); } }

    private double _doorWeight;
    public double DoorWeight { get => _doorWeight; private set { _doorWeight = value; OnPropertyChanged(); } }

    private string _errorMessage;
    public string ErrorMessage
    {
        get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); }
    }

    // --- КОЛЛЕКЦИИ ---

    // Динамическая таблица вставок
    private ObservableCollection<SlimInsertRow> _inserts = new();
    public ObservableCollection<SlimInsertRow> Inserts
    {
        get => _inserts;
        set
        {
            if (_inserts != null)
                _inserts.CollectionChanged -= Inserts_CollectionChanged;

            _inserts = value ?? new ObservableCollection<SlimInsertRow>();
            _inserts.CollectionChanged += Inserts_CollectionChanged;

            // Если JSON загрузил готовый список, привязываем события к каждой строке
            foreach (var item in _inserts)
            {
                item.PropertyChanged -= OnRowPropertyChanged;
                item.PropertyChanged += OnRowPropertyChanged;
            }

            OnPropertyChanged();
        }
    }

    // Слушает добавление/удаление строк в списке
    private void Inserts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (SlimInsertRow item in e.NewItems)
            {
                item.PropertyChanged -= OnRowPropertyChanged; // Защита от двойной подписки
                item.PropertyChanged += OnRowPropertyChanged;
            }
        }
        if (e.OldItems != null)
        {
            foreach (SlimInsertRow item in e.OldItems)
            {
                item.PropertyChanged -= OnRowPropertyChanged;
            }
        }
    }

    // Слушает изменения внутри конкретной строки
    private void OnRowPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (sender is SlimInsertRow row)
        {
            if (e.PropertyName == nameof(SlimInsertRow.Height) && row.Index > 0)
            {
                CalculateFirstInsertHeight();
                RecalculateWeight();
                ValidateErrorMessage();
            }
            if (e.PropertyName == nameof(SlimInsertRow.MaterialIndex))
            {
                row.Width = CalculateInsertWidth(row.MaterialIndex);
                CalculateFirstInsertHeight();
                RecalculateWeight();
                ValidateErrorMessage();
            }
        }
    }

    // Таблица профилей
    public ObservableCollection<ProfileRow> Profiles { get; set; } = new();






    private void ValidateErrorMessage()
    {
        if (CheckInsertsHeights())
        {
            ErrorMessage = "Неверно внесены высоты вставок";
        }
        else
        {
            ErrorMessage = string.Empty; // Очищаем ошибку, если всё ок
        }
    }
    private void UpdateInsertsList()
    {
        int frames = MiddleFramesCountIndex;
        int insertsCount = frames + 1;

        if (Inserts.Count == insertsCount)
        {
            foreach (var row in Inserts)
            {
                row.Width = CalculateInsertWidth(row.MaterialIndex);
                row.Count = DoorsCount;
            }
            return;
        }
        Inserts.Clear();

        for (int i = 0; i < insertsCount; i++)
        {
            string displayName = $"Вставка {i + 1}";
            if (insertsCount > 1 && i == insertsCount - 1) displayName += " (низ двери)";

            var row = new SlimInsertRow
            {
                Index = i,
                Name = displayName,
                IsHeightEditable = i > 0,
                MaterialIndex = 0,
                Count = DoorsCount
            };

            row.Width = CalculateInsertWidth(row.MaterialIndex);

            Inserts.Add(row);
        }
    }
    /// <summary>
    /// Рассчитывает количество дверей на основе выбранной схемы расположения
    /// Формула: =ЕСЛИ(E12=U4;2;ЕСЛИ(E12=U5;3;ЕСЛИ(E12=U6;4;ЕСЛИ(E12=U7;4;ЕСЛИ(E12=U8;5)))))
    /// </summary>
    private int CalculateDoorsCount()
    {
        return SelectedArrangementIndex switch
        {
            0 => 2, // "I-----____I" (2 двери)
            1 => 3, // "I-----____-----I" (3 двери)
            2 => 4, // "I-----____-----____I" (4 двери)
            3 => 4, // "I-----____  ____-----I" (4 двери, но с зазором)
            4 => 5, // "I-----____-----____-----I" (5 дверей)
            _ => -1  // По умолчанию (быть не может)
        };
    }

    /// <summary>
    /// Рассчитывает количество перекрытий на основе выбранной схемы расположения
    /// Формула: =ЕСЛИ(E12=U4;1;ЕСЛИ(E12=U5;2;ЕСЛИ(E12=U6;3;ЕСЛИ(E12=U7;2;ЕСЛИ(E12=U8;4)))))
    /// </summary>
    private int CalculateOverlapsCount()
    {
        return SelectedArrangementIndex switch
        {
            0 => 1, // "I-----____I" (1 перекрытие)
            1 => 2, // "I-----____-----I" (2 перекрытия)
            2 => 3, // "I-----____-----____I" (3 перекрытия)
            3 => 2, // "I-----____  ____-----I" (2 перекрытия, т.к. есть зазор)
            4 => 4, // "I-----____-----____-----I" (4 перекрытия)
            _ => -1  // По умолчанию (быть не может)
        };
    }

    /// <summary>
    /// Рассчитывает высоту двери на основе типа установки
    /// Формула (упрощенная, без учета врезной направляющей):
    /// Видимый верхний трек: OpeningHeight - 39
    /// Скрытый верхний трек, корпус: OpeningHeight + 12  
    /// Скрытый верхний трек, проем: OpeningHeight - 28
    /// </summary>
    private double CalculateDoorHeight()
    {
        if (OpeningHeight <= 0) return 0;

        return SelectedInstallationTypeIndex switch
        {
            0 => OpeningHeight - 45, // Видимый верхний трек
            1 => OpeningHeight + 6, // Скрытый верхний трек, корпус
            2 => OpeningHeight - 34, // Скрытый верхний трек, проем
            _ => OpeningHeight       // По умолчанию (не должно случаться)
        };
    }

    private void RecalculateWeight()
    {

        double
            q2 = 0.298 * Profiles[2].Size * Profiles[2].Count / 1000,
            q3 = 0.283 * Profiles[3].Size * Profiles[3].Count / 1000,
            q4 = 0.56 * Profiles[4].Size * Profiles[4].Count / 1000,
            q5 = 0.312 * Profiles[5].Size * Profiles[5].Count / 1000;


        double q2345 = (q2 + q3 + q4 + q5) / DoorsCount;


        double totalInsertWeight = 0;

        for (int i = 0; i < Inserts.Count; i++)
        {
            var insert = Inserts[i];

            double insertWeight = insert.MaterialIndex switch
            {
                0 => insert.Height * insert.Width / 1000000 * 8,
                1 => insert.Height * insert.Width / 1000000 * 11,
                _ => insert.Height * insert.Width / 1000000 * 8
            };

            totalInsertWeight += insertWeight;
        }

        DoorWeight = Math.Ceiling((q2345 + totalInsertWeight) * 1.05);
    }

    /// <summary>
    /// Рассчитывает ширину двери
    /// Формула из AC34 (упрощенная, с учетом что элемент всегда Шлегель)
    /// </summary>
    private double CalculateDoorWidth()
    {
        double baseValue = 0;
        // Если выбрана особая схема "|----____ ____----|" (индекс 3)
        if (SelectedArrangementIndex == 3)
        {
            // Для схемы с зазором посередине: (ширина_проема - 20 + перекрытия*10) / дверей
            baseValue = (OpeningWidth - 20 + OverlapsCount * 10) / DoorsCount;
            return Math.Ceiling(baseValue);
        }
        else
        {
            // Для всех остальных схем: (ширина_проема - 10 + перекрытия*10) / дверей
            baseValue = (OpeningWidth - 10 + OverlapsCount * 10) / DoorsCount;
            return Math.Ceiling(baseValue);
        }
    }

    private void CalculateAndUpdateProfilesTable()
    {
        Profiles.Clear();
        string colorName = ProfileColors[SelectedColorIndex];

        double topRailSize = SelectedInstallationTypeIndex == 1 ? OpeningWidth - 2 - 32 : OpeningWidth - 2;
        double bottomRailSize = OpeningWidth - 2;

        //1
        Profiles.Add(new ProfileRow
        {
            Name = $"Направляющая верхняя ({colorName})",
            Size = topRailSize,
            Count = 1
        });

        //2
        Profiles.Add(new ProfileRow
        {
            Name = $"Направляющая нижняя ({colorName})",
            Size = bottomRailSize,
            Count = 1
        });

        // 3
        Profiles.Add(new ProfileRow
        {
            Name = $"Профиль вертикальный Slim ({colorName})",
            Size = DoorHeight,
            Count = DoorsCount * 2
        });

        // 4
        Profiles.Add(new ProfileRow
        {
            Name = $"Рамка узкая ({colorName})",
            Size = SelectedInstallationTypeIndex == 0 ? (DoorWidth - 2 * 9.7) : 0,
            Count = SelectedInstallationTypeIndex == 0 ? DoorsCount : 0
        });

        double Size5Profile = DoorWidth - 2 * 9.7;
        // 5
        Profiles.Add(new ProfileRow
        {
            Name = $"Рамка широкая ({colorName})",
            Size = Size5Profile,
            Count = SelectedInstallationTypeIndex == 0 ? DoorsCount : DoorsCount * 2
        });

        // 6
        Profiles.Add(new ProfileRow
        {
            Name = $"Рамка средняя ({colorName})",
            Size = MiddleFramesCountIndex == 0 ? 0 : Size5Profile,
            Count = DoorsCount * MiddleFramesCountIndex
        });

        Parent?.SyncSlimLineToFurniture();
    }


    /// <summary>
    /// Рассчитывает ширину вставки на основе материала
    /// Формула: =ЕСЛИ(C36=$U$12;$L$8-ЕСЛИ(D8=AB34;AF34;AF35);
    ///            ЕСЛИ(C36=$U$13;$L$8-ЕСЛИ(D8=AB34;AF34;AF35)-2;
    ///            $L$8-ЕСЛИ(D8=AB34;AF34;AF35)-3))
    /// </summary>
    /// <param name="materialIndex">Индекс материала (0 - ЛДСП10, 1 - Стекло)</param>
    private double CalculateInsertWidth(int materialIndex)
    {
        if (materialIndex == 0)
        {
            return DoorWidth - 4;
        }
        else
        {
            return DoorWidth - 4 - 3;
        }
    }

    /// <summary>
    /// Рассчитывает высоту первой вставки автоматически
    /// </summary>
    private void CalculateFirstInsertHeight()
    {
        if (Inserts.Count == 0) return;

        double result = DoorHeight;

        for (int i = 1; i < Inserts.Count; i++)
        {
            var insert = Inserts[i];

            // Если высота 0, вычитаем 0 (пропускаем)
            if (insert.Height == 0) continue;

            double deduction = insert.Height;
            if (insert.MaterialIndex == 1)
            {
                deduction += 3;
            }

            result -= deduction;
        }

        result -= 3;

        var firstMatIndex = Inserts[0].MaterialIndex;
        if (firstMatIndex == 1)
        {
            result -= 3;
        }
        result -= MiddleFramesCountIndex * 1.3;

        Inserts[0].Height = Math.Floor(result);
    }

    private bool CheckInsertsHeights()
    {
        if (Inserts.Count == 0) return false;

        for (int i = 0; i < Inserts.Count; i++)
            if (Inserts[i].Height <= 0)
                return true;
        return false;
    }
    public void Recalculate()
    {
        DoorsCount = CalculateDoorsCount();
        OverlapsCount = CalculateOverlapsCount();

        DoorHeight = CalculateDoorHeight();
        DoorWidth = CalculateDoorWidth();

        CalculateAndUpdateProfilesTable();
        UpdateInsertsList();

        CalculateFirstInsertHeight();
        RecalculateWeight();

        ValidateErrorMessage();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

// Класс строки вставки
public class SlimInsertRow : INotifyPropertyChanged
{
    public List<string> MaterialOptions { get; } = new() { "ЛДСП, 10мм", "Стекло, зеркало 4мм" };

    public int Index { get; set; }
    public string Name { get; set; }

    private int _materialIndex;
    public int MaterialIndex
    {
        get => _materialIndex;
        set
        {
            // ЗАЩИТА: Если Picker пытается сбросить индекс в -1 при закрытии страницы,
            // мы просто игнорируем это действие, сохраняя старое значение.
            if (value == -1 && _materialIndex != -1)
                return;

            if (_materialIndex != value)
            {
                _materialIndex = value;
                OnPropertyChanged();
            }
        }
    }

    private double _height;
    public double Height
    {
        get => _height;
        set { _height = value; OnPropertyChanged(); }
    }

    private double _width;
    public double Width
    {
        get => _width;
        set { _width = value; OnPropertyChanged(); }
    }

    private int _count;
    public int Count
    {
        get => _count;
        set { _count = value; OnPropertyChanged(); }
    }

    public bool IsHeightEditable { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

// Класс строки профиля
public class ProfileRow
{
    public string Name { get; set; }
    public double Size { get; set; }
    public int Count { get; set; }
}