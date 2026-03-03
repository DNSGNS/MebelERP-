using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace MyApp1;

public class StorageItem : INotifyPropertyChanged
{
    public int DetailId { get; set; }
    public string? Color { get; set; }
    public double Length { get; set; }
    public double Width { get; set; }
    public bool CanRotate { get; set; } = true;


    public IDrawable Preview => new StoragePartDrawable(this);

    private int _count;
    public int Count
    {
        get => _count;
        set { _count = value; OnPropertyChanged(nameof(Count)); }
    }
    public void Rotate()
    {
        // Проверяем, разрешено ли вращение для этой детали
        if (!CanRotate) return;

        // Меняем местами Длину и Ширину
        (Length, Width) = (Width, Length);

        // Уведомляем интерфейс, что данные изменились
        OnPropertyChanged(nameof(Length));
        OnPropertyChanged(nameof(Width));

        // ВАЖНО: Уведомляем, что Preview изменилось, чтобы перерисовалась картинка
        OnPropertyChanged(nameof(Preview));
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}



public class CuttingEditorForm : INotifyPropertyChanged
{
    public ObservableCollection<SheetLayout> Sheets { get; }
    public ObservableCollection<StorageItem> StorageItems { get; set; } = new();

    // [FIX] Храним ссылку на настройки, они нужны для логики и рисования
    private readonly CuttingSettingForm _settings;


    private double _panX = 40;
    public double PanX
    {
        get => _panX;
        set { _panX = value; OnPropertyChanged(nameof(EditorDiagramDrawable)); } // Перерисовка при сдвиге
    }

    private double _panY = 40;
    public double PanY
    {
        get => _panY;
        set { _panY = value; OnPropertyChanged(nameof(EditorDiagramDrawable)); }
    }


    public const float CanvasPadding = 40f;



    private bool _isMergingMode;
    public bool IsMergingMode
    {
        get => _isMergingMode;
        set
        {
            if (_isMergingMode != value)
            {
                _isMergingMode = value;
                SelectedWasteRects.Clear(); // Сброс выделения при смене режима
                SelectedPart = null;        // Сброс выделения детали
                OnPropertyChanged();
                OnPropertyChanged(nameof(EditorDrawable));
            }
        }
    }

    // Список выбранных для объединения зон (максимум 2)
    public List<RectF> SelectedWasteRects { get; set; } = new List<RectF>();



    // --- УПРАВЛЕНИЕ МАСШТАБОМ ---
    private double _scale = 1.0;
    public double Scale
    {
        get => _scale;
        set
        {
            double newScale = Math.Clamp(value, 0.2, 5.0);
            if (_scale != newScale)
            {
                _scale = newScale;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EditorDrawable));
            }
        }
    }

    public void ZoomIn() => Scale += 0.1;
    public void ZoomOut() => Scale -= 0.1;

    // --- ОТРИСОВКА ---
    // [FIX] Передаем настройки (_settings) в конструктор отрисовщика
    public IDrawable EditorDrawable => new EditorDiagramDrawable(this, _settings);

    private SheetLayout _selectedSheet;
    public SheetLayout SelectedSheet
    {
        get => _selectedSheet;
        set
        {
            if (_selectedSheet != value)
            {
                _selectedSheet = value;
                SelectedPart = null;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EditorDrawable));
            }
        }
    }

    private PlacedPart _selectedPart;
    public PlacedPart SelectedPart
    {
        get => _selectedPart;
        set
        {
            if (_selectedPart != value)
            {
                _selectedPart = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPartSelected));
                OnPropertyChanged(nameof(EditorDrawable));
            }
        }
    }



    public bool IsPartSelected => SelectedPart != null;

    // [FIX] Конструктор теперь требует настройки
    public CuttingEditorForm(ObservableCollection<SheetLayout> sheets, CuttingSettingForm settings)
    {
        Sheets = sheets; // Теперь это ссылка на тот же список, что и в LastResult
        _settings = settings;
        if (Sheets.Any())
            SelectedSheet = Sheets.First();
    }

    // Поворот выбранной детали
    public bool RotateSelectedPart()
    {
        if (SelectedPart == null || SelectedSheet == null) return false;

        double oldL = SelectedPart.Length;
        double oldW = SelectedPart.Width;

        // Меняем размеры местами
        SelectedPart.Length = oldW;
        SelectedPart.Width = oldL;
        SelectedPart.IsRotated = !SelectedPart.IsRotated;

        if (IsOutOfBounds(SelectedPart, SelectedPart.X, SelectedPart.Y, SelectedSheet) ||
            HasCollision(SelectedPart, SelectedPart.X, SelectedPart.Y, SelectedSheet))
        {
            SelectedPart.Length = oldL;
            SelectedPart.Width = oldW;
            SelectedPart.IsRotated = !SelectedPart.IsRotated;
            return false;
        }

        // [FIX] Передаем реальные настройки в RebuildWaste
        bool isCutByLength = _settings.CuttingMethod == "По длине";
        GuillotinePacker.RebuildWaste(SelectedSheet, _settings.CutWidth, _settings.EdgeOffset, isCutByLength);

        OnPropertyChanged(nameof(EditorDrawable));
        return true;
    }

    // Перемещение детали на склад
    public void MoveToStorage(PlacedPart part)
    {
        if (SelectedSheet == null || part == null) return;

        if (SelectedSheet.Parts.Contains(part))
        {
            SelectedSheet.Parts.Remove(part);

            if (SelectedSheet.Parts.Count == 0)
            {
                SelectedSheet.ColorName = "";
            }
        }

        // Ищем на складе такую же деталь (Ваш код без изменений)
        var existing = StorageItems.FirstOrDefault(i =>
            i.DetailId == part.DetailId &&
            i.Color == part.Color &&
            Math.Abs(i.Length - part.Length) < 0.1 &&
            Math.Abs(i.Width - part.Width) < 0.1);

        if (existing != null)
        {
            existing.Count++;
        }
        else
        {
            StorageItems.Add(new StorageItem
            {
                DetailId = part.DetailId,
                Color = part.Color,
                Length = part.Length,
                Width = part.Width,
                Count = 1
            });
        }

        // Пересчет отходов (Ваш код без изменений)
        bool isCutByLength = _settings.CuttingMethod == "По длине";
        GuillotinePacker.RebuildWaste(SelectedSheet, _settings.CutWidth, _settings.EdgeOffset, isCutByLength);

        SelectedPart = null;
        OnPropertyChanged(nameof(EditorDrawable));
    }

    // Попытка поставить деталь в новые координаты
    public bool TryPlacePart(PlacedPart part, double dropX, double dropY)
    {
        if (SelectedSheet == null) return false;

        // --- [НОВАЯ ЛОГИКА] ПРОВЕРКА ЦВЕТА ---

        // Если на листе УЖЕ есть детали, проверяем совпадение цветов строго.
        // Если лист пустой (Parts.Count == 0), эту проверку пропускаем.
        if (SelectedSheet.Parts.Count > 0)
        {
            if (!string.IsNullOrEmpty(SelectedSheet.ColorName) &&
                !string.IsNullOrEmpty(part.Color) &&
                SelectedSheet.ColorName != part.Color)
            {
                // Цвет детали не совпадает с цветом, который уже есть на листе!
                return false;
            }
        }
        // --- КОНЕЦ НОВОЙ ЛОГИКИ ---

        // 1. ПОИСК МЕСТА (МАГНЕТИЗМ) - (Ваш код без изменений)
        var targetWaste = SelectedSheet.WasteRects.FirstOrDefault(w =>
            dropX >= w.X && dropX <= (w.X + w.Width) &&
            dropY >= w.Y && dropY <= (w.Y + w.Height) &&
            w.Width >= part.Length && w.Height >= part.Width);

        // ... (копируем координаты - без изменений)
        double finalX = targetWaste != null ? targetWaste.X : dropX;
        double finalY = targetWaste != null ? targetWaste.Y : dropY;

        // Создаем временную копию координат для проверки коллизий
        double originalX = part.X;
        double originalY = part.Y;
        part.X = finalX;
        part.Y = finalY;

        // 2. ПРОВЕРКА КОЛЛИЗИЙ (Ваш код без изменений)
        if (IsOutOfBounds(part, part.X, part.Y, SelectedSheet) ||
            HasCollision(part, part.X, part.Y, SelectedSheet))
        {
            // Возвращаем деталь на старое место (если она двигалась)
            part.X = originalX;
            part.Y = originalY;
            return false;
        }

        // 3. ДОБАВЛЕНИЕ
        if (!SelectedSheet.Parts.Contains(part))
        {
            SelectedSheet.Parts.Add(part);

            // Если это первая деталь на листе, задаем цвет листу
            if (SelectedSheet.Parts.Count == 1)
            {
                SelectedSheet.ColorName = part.Color ?? "";
            }
        }

        // 4. ПЕРЕСЧЕТ (Ваш код без изменений)
        bool isCutByLength = _settings.CuttingMethod == "По длине";
        GuillotinePacker.RebuildWaste(SelectedSheet, _settings.CutWidth, _settings.EdgeOffset, isCutByLength);

        OnPropertyChanged(nameof(EditorDrawable));

        return true;
    }
    public PlacedPart FindPartAt(double touchX, double touchY, float finalScale, double edgeOffsetFromView)
    {
        if (SelectedSheet == null) return null;

        // Примечание: тут можно использовать переданный edgeOffsetFromView 
        // или взять _settings.EdgeOffset. Лучше использовать то, что совпадает с отрисовкой.
        // Сейчас оставим параметр аргумента, так как он приходит из View
        double xOnSheet = (touchX - CanvasPadding) / finalScale - edgeOffsetFromView;
        double yOnSheet = (touchY - CanvasPadding) / finalScale - edgeOffsetFromView;

        return SelectedSheet.Parts.LastOrDefault(p =>
            xOnSheet >= p.X && xOnSheet <= (p.X + p.Length) &&
            yOnSheet >= p.Y && yOnSheet <= (p.Y + p.Width));
    }

    private bool IsOutOfBounds(PlacedPart part, double x, double y, SheetLayout sheet)
    {
        // ВАЖНО: Размеры листа для деталей теперь меньше на величину обпила.
        // Но алгоритм хранит координаты внутри "чистой" зоны.
        // Если sheet.SheetW - это ПОЛНЫЙ размер листа, то:
        double safeW = sheet.SheetW - (_settings.EdgeOffset * 2);
        double safeH = sheet.SheetH - (_settings.EdgeOffset * 2);

        return x < 0 || y < 0 ||
               (x + part.Length) > safeW ||
               (y + part.Width) > safeH;
    }

    private bool HasCollision(PlacedPart part, double x, double y, SheetLayout sheet)
    {
        foreach (var other in sheet.Parts)
        {
            if (other == part) continue;
            if (x < other.X + other.Length && x + part.Length > other.X &&
                y < other.Y + other.Width && y + part.Width > other.Y)
                return true;
        }
        return false;
    }




    public RectF FindWasteAt(double touchX, double touchY, float finalScale, double edgeOffsetFromView)
    {
        if (SelectedSheet == null) return RectF.Zero;

        double xOnSheet = (touchX - CanvasPadding) / finalScale - edgeOffsetFromView;
        double yOnSheet = (touchY - CanvasPadding) / finalScale - edgeOffsetFromView;

        // Ищем прямоугольник, в который попал клик
        // Используем RectF.Contains или простую математику
        var found = SelectedSheet.WasteRects.FirstOrDefault(w =>
            xOnSheet >= w.X && xOnSheet <= (w.X + w.Width) &&
            yOnSheet >= w.Y && yOnSheet <= (w.Y + w.Height));

        return found; // Вернет RectF.Zero (пустую структуру), если не найдено, так как RectF - struct
    }

    // Логика выбора зоны
    public void ToggleWasteSelection(RectF waste)
    {
        if (waste.IsEmpty) return;

        if (SelectedWasteRects.Contains(waste))
        {
            SelectedWasteRects.Remove(waste);
        }
        else
        {
            if (SelectedWasteRects.Count >= 2) SelectedWasteRects.Clear();
            SelectedWasteRects.Add(waste);
        }
        OnPropertyChanged(nameof(EditorDrawable));
    }

    // ГЛАВНЫЙ МЕТОД: Попытка объединения
    // Возвращает null при успехе, иначе текст ошибки
    public string TryMergeSelectedWaste()
    {
        if (SelectedWasteRects.Count != 2 || SelectedSheet == null)
            return "Выберите ровно 2 зоны.";

        var r1 = SelectedWasteRects[0];
        var r2 = SelectedWasteRects[1];

        // Используем значение из настроек + небольшой запас в 1мм для надежности
        float eps = (float)_settings.CutWidth + 1.0f;

        // 1. Проверяем горизонтальное соседство (один слева, другой справа)
        // Расстояние между правой границей левого и левой границей правого
        bool closeHorizontal = (Math.Abs(r1.Right - r2.Left) < eps) || (Math.Abs(r2.Right - r1.Left) < eps);

        // Проверка вертикального нахлеста (общая "стенка" по высоте)
        float overlapY = Math.Min(r1.Bottom, r2.Bottom) - Math.Max(r1.Top, r2.Top);

        // 2. Проверка вертикального соседства (один сверху, другой снизу)
        bool closeVertical = (Math.Abs(r1.Bottom - r2.Top) < eps) || (Math.Abs(r2.Bottom - r1.Top) < eps);

        // Проверка горизонтального нахлеста (общая "стенка" по ширине)
        float overlapX = Math.Min(r1.Right, r2.Right) - Math.Max(r1.Left, r2.Left);

        List<RectF> newRects = new List<RectF>();

        // ЛОГИКА СЛИЯНИЯ
        if (closeHorizontal && overlapY > 0.1f)
        {
            // Зоны стоят бок о бок. Ширина пропила (CutWidth) физически разделяет их.
            // При объединении мы создаем общий прямоугольник, который "поглощает" этот пропил.

            if (Math.Abs(r1.Y - r2.Y) < eps && Math.Abs(r1.Height - r2.Height) < eps)
            {
                // Идеальное слияние (одинаковая высота)
                float newX = Math.Min(r1.X, r2.X);
                // Новая ширина = сумма ширин + ширина пропила между ними
                float newW = r1.Width + r2.Width + (float)_settings.CutWidth;
                newRects.Add(new RectF(newX, r1.Y, newW, r1.Height));
            }
            else
            {
                // Сложное слияние (L-образное)
                float commonTop = Math.Max(r1.Top, r2.Top);
                float commonBottom = Math.Min(r1.Bottom, r2.Bottom);
                float commonH = commonBottom - commonTop;

                float newX = Math.Min(r1.X, r2.X);
                float newW = r1.Width + r2.Width + (float)_settings.CutWidth;

                RectF bigRect = new RectF(newX, commonTop, newW, commonH);
                newRects.Add(bigRect);
                AddResidue(newRects, r1, bigRect);
                AddResidue(newRects, r2, bigRect);
            }
        }
        else if (closeVertical && overlapX > 0.1f)
        {
            // Зоны стоят друг над другом
            if (Math.Abs(r1.X - r2.X) < eps && Math.Abs(r1.Width - r2.Width) < eps)
            {
                // Идеальное слияние (одинаковая ширина)
                float newY = Math.Min(r1.Y, r2.Y);
                // Новая высота включает пропил
                float newH = r1.Height + r2.Height + (float)_settings.CutWidth;
                newRects.Add(new RectF(r1.X, newY, r1.Width, newH));
            }
            else
            {
                // Сложное слияние
                float commonLeft = Math.Max(r1.Left, r2.Left);
                float commonRight = Math.Min(r1.Right, r2.Right);
                float commonW = commonRight - commonLeft;

                float newY = Math.Min(r1.Y, r2.Y);
                float newH = r1.Height + r2.Height + (float)_settings.CutWidth;

                RectF bigRect = new RectF(commonLeft, newY, commonW, newH);
                newRects.Add(bigRect);
                AddResidue(newRects, r1, bigRect);
                AddResidue(newRects, r2, bigRect);
            }
        }
        else
        {
            return $"Зоны не касаются. Зазор по факту: {Math.Min(Math.Abs(r1.Right - r2.Left), Math.Abs(r2.Right - r1.Left)):F2}. Допуск: {eps:F2}";
        }

        // Удаление старых и добавление новых (как было раньше)
        if (newRects.Count > 0)
        {
            SelectedSheet.WasteRects.Remove(r1);
            SelectedSheet.WasteRects.Remove(r2);
            foreach (var nr in newRects) SelectedSheet.WasteRects.Add(nr);

            SelectedWasteRects.Clear();
            OnPropertyChanged(nameof(EditorDrawable));
            return null;
        }

        return "Ошибка геометрии.";
    }

    // Вспомогательный метод для вычисления остатков при неполном слиянии
    private void AddResidue(List<RectF> list, RectF original, RectF subtracted)
    {
        // Если original больше subtracted, добавляем разницу.
        // Простой случай: original разрезается subtracted.

        // Верхний кусок
        if (original.Top < subtracted.Top && original.Right > subtracted.Left && original.Left < subtracted.Right)
            list.Add(new RectF(original.X, original.Y, original.Width, subtracted.Top - original.Top));

        // Нижний кусок
        if (original.Bottom > subtracted.Bottom && original.Right > subtracted.Left && original.Left < subtracted.Right)
            list.Add(new RectF(original.X, subtracted.Bottom, original.Width, original.Bottom - subtracted.Bottom));

        // Левый кусок
        if (original.Left < subtracted.Left && original.Bottom > subtracted.Top && original.Top < subtracted.Bottom)
            list.Add(new RectF(original.X, original.Y, subtracted.Left - original.Left, original.Height));

        // Правый кусок
        if (original.Right > subtracted.Right && original.Bottom > subtracted.Top && original.Top < subtracted.Bottom)
            list.Add(new RectF(subtracted.Right, original.Y, original.Right - subtracted.Right, original.Height));
    }




    public void AddSheet()
    {
        // Берем размеры из настроек или копируем с первого листа
        double w = Sheets.Any() ? Sheets.First().SheetW : 2800;
        double h = Sheets.Any() ? Sheets.First().SheetH : 2070;

        var newSheet = new SheetLayout
        {
            SheetIndex = Sheets.Count + 1,
            SheetW = w,
            SheetH = h,
            Parts = new List<PlacedPart>(),
            WasteRects = new List<RectF>(),
            // [ВАЖНО] Новый лист создается "чистым", без цвета
            ColorName = null
        };

        // Инициализируем "пустоту" для нового листа
        bool isCutByLength = _settings.CuttingMethod == "По длине";
        GuillotinePacker.RebuildWaste(newSheet, _settings.CutWidth, _settings.EdgeOffset, isCutByLength);

        Sheets.Add(newSheet);
        SelectedSheet = newSheet;
    }

    // Метод удаления текущего листа
    public void RemoveCurrentSheet()
    {
        if (SelectedSheet == null || Sheets.Count == 0) return;

        var sheetToRemove = SelectedSheet;

        // 1. Перемещаем ВСЕ детали на склад
        // Используем ToList(), чтобы создать копию списка, так как MoveToStorage удаляет из коллекции
        var partsToMove = sheetToRemove.Parts.ToList();
        foreach (var part in partsToMove)
        {
            MoveToStorage(part);
        }

        // 2. Удаляем лист из коллекции
        Sheets.Remove(sheetToRemove);

        // 3. Переключаем выбор
        if (Sheets.Any())
        {
            SelectedSheet = Sheets.Last();
        }
        else
        {
            SelectedSheet = null;
            // Опционально: можно запретить удалять последний лист или создавать новый пустой
            AddSheet();
        }

        // Перенумерация листов (если нужно для красоты в UI)
        for (int i = 0; i < Sheets.Count; i++)
        {
            Sheets[i].SheetIndex = i + 1;
        }

        OnPropertyChanged(nameof(EditorDrawable));
    }



    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}