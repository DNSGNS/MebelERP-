using Microsoft.Maui.Graphics;

namespace MyApp1;

public partial class CuttingEditorContentView : ContentView
{
    private CuttingData ViewModel => BindingContext as CuttingData;

    // Переменные для хранения состояния
    private double _originalPartX;
    private double _originalPartY;
    private float _currentVisualScale = 1.0f;


    // Новые переменные для панорамирования
    private bool _isPanningMode = false;
    private double _lastPanX;
    private double _lastPanY;

    public CuttingEditorContentView()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (ViewModel?.LastEdit != null)
        {
            // Нам больше не нужно создавать new CuttingEditorForm(sheets.ToList())
            // Просто подписываемся на обновление отрисовки
            ViewModel.LastEdit.PropertyChanged -= OnEditorPropertyChanged;
            ViewModel.LastEdit.PropertyChanged += OnEditorPropertyChanged;

            EditorCanvas.Invalidate();
        }
    }

    private void OnEditorPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CuttingEditorForm.SelectedSheet) ||
            e.PropertyName == nameof(CuttingEditorForm.Scale) ||
            e.PropertyName == nameof(CuttingEditorForm.EditorDrawable))
        {
            EditorCanvas.Invalidate();

        }
    }
    private void OnAddSheetClicked(object sender, EventArgs e)
    {
        if (ViewModel?.LastEdit == null) return;
        ViewModel.LastEdit.AddSheet();
        EditorCanvas.Invalidate();
    }

    private async void OnRemoveSheetClicked(object sender, EventArgs e)
    {
        if (ViewModel?.LastEdit == null) return;

        bool answer = await Application.Current.MainPage.DisplayAlert(
            "Удаление листа",
            "Вы уверены? Детали с этого листа переместятся на склад.",
            "Удалить",
            "Отмена");

        if (answer)
        {
            ViewModel.LastEdit.RemoveCurrentSheet();
            EditorCanvas.Invalidate();
        }
    }




    private void OnRotateClicked(object sender, EventArgs e)
    {
        if (ViewModel?.LastEdit == null) return;
        if (ViewModel.LastEdit.RotateSelectedPart())
            EditorCanvas.Invalidate();
    }

    private void OnZoomInClicked(object sender, EventArgs e)
    {
        if (ViewModel?.LastEdit == null) return;
        ViewModel.LastEdit.ZoomIn();
        EditorCanvas.Invalidate(); // Принудительная перерисовка
    }

    private void OnZoomOutClicked(object sender, EventArgs e)
    {
        if (ViewModel?.LastEdit == null) return;
        ViewModel.LastEdit.ZoomOut();
        EditorCanvas.Invalidate(); // Принудительная перерисовка
    }

    // --- ВЗАИМОДЕЙСТВИЕ С КАНВАСОМ ---

    private async void OnCanvasStartInteraction(object sender, TouchEventArgs e)
    {
        var editor = ViewModel?.LastEdit;
        if (editor?.SelectedSheet == null || e.Touches.Length == 0) return;

        var touch = e.Touches[0];

        // Пересчет координат (как было)
        var sheet = editor.SelectedSheet;
        float padding = CuttingEditorForm.CanvasPadding;
        float baseScale = Math.Min((float)(EditorCanvas.Width - padding * 2) / (float)sheet.SheetW,
                                   (float)(EditorCanvas.Height - padding * 2) / (float)sheet.SheetH);
        float finalScale = baseScale * (float)editor.Scale;

        if (editor.IsMergingMode)
        {
            // 1. Ищем зону
            var waste = editor.FindWasteAt(touch.X, touch.Y, finalScale, ViewModel.Settings.EdgeOffset);

            if (!waste.IsEmpty)
            {
                // Переключаем выделение
                editor.ToggleWasteSelection(waste);

                // Если выбрано 2 зоны - пробуем объединить
                if (editor.SelectedWasteRects.Count == 2)
                {
                    // Вызываем слияние и получаем результат (текст ошибки или null если успех)
                    string errorMsg = editor.TryMergeSelectedWaste();

                    if (errorMsg == null)
                    {
                        // УСПЕХ!
                        // Опционально: выключить режим после успеха
                        // editor.IsMergingMode = false; 
                    }
                    else
                    {
                        // ОШИБКА: Показываем почему
                        await Application.Current.MainPage.DisplayAlert("Нельзя объединить", errorMsg, "OK");

                        // Сбрасываем выделение, чтобы попробовать снова
                        editor.SelectedWasteRects.Clear();
                        EditorCanvas.Invalidate();
                    }
                }
            }
            else
            {
                // Для диагностики: если кликнули мимо
                // System.Diagnostics.Debug.WriteLine($"Missed click at {touch.X}:{touch.Y}");
            }
        }
        else
        {
            // ... старый код для перемещения деталей ...
            var part = editor.FindPartAt(touch.X, touch.Y, finalScale, ViewModel.Settings.EdgeOffset);
            if (part != null)
            {
                _isPanningMode = false;
                editor.SelectedPart = part;
                _originalPartX = part.X;
                _originalPartY = part.Y;
            }
            else
            {
                _isPanningMode = false; // Или включить Pan
                editor.SelectedPart = null;
            }
        }

        EditorCanvas.Invalidate();
    }

    private void OnCanvasDragInteraction(object sender, TouchEventArgs e)
    {
        var editor = ViewModel?.LastEdit;
        if (editor == null || e.Touches.Length == 0) return;

        var touch = e.Touches[0];

        if (_isPanningMode)
        {
            // Вычисляем, на сколько реально сдвинулся палец с прошлого кадра
            double deltaX = touch.X - _lastPanX;
            double deltaY = touch.Y - _lastPanY;

            // Применяем дельту к текущему положению "камеры"
            editor.PanX += deltaX;
            editor.PanY += deltaY;

            // ОБЯЗАТЕЛЬНО обновляем последнюю точку, чтобы на следующем шаге дельта не росла
            _lastPanX = touch.X;
            _lastPanY = touch.Y;
        }
        else if (editor.SelectedPart != null)
        {
            // --- ЛОГИКА ПЕРЕМЕЩЕНИЯ ДЕТАЛИ ---
            var sheet = editor.SelectedSheet;
            float padding = CuttingEditorForm.CanvasPadding;
            float baseScale = Math.Min((float)(EditorCanvas.Width - padding * 2) / (float)sheet.SheetW,
                                       (float)(EditorCanvas.Height - padding * 2) / (float)sheet.SheetH);
            float finalScale = baseScale * (float)editor.Scale;

            // Обратная формула: (Экран - Сдвиг) / Масштаб - Обпил
            double newX = (touch.X - editor.PanX) / finalScale - ViewModel.Settings.EdgeOffset;
            double newY = (touch.Y - editor.PanY) / finalScale - ViewModel.Settings.EdgeOffset;

            // Центрируем деталь
            editor.SelectedPart.X = newX - (editor.SelectedPart.Length / 2);
            editor.SelectedPart.Y = newY - (editor.SelectedPart.Width / 2);

            EditorCanvas.Invalidate();
        }
    }

    private void OnCanvasEndInteraction(object sender, TouchEventArgs e)
    {
        // Сброс режима
        _isPanningMode = false;

        if (ViewModel?.LastEdit?.SelectedPart == null) return;

        var touch = e.Touches[0];
        var editor = ViewModel.LastEdit;
        var sheet = editor.SelectedSheet;

        float padding = CuttingEditorForm.CanvasPadding;
        float baseScale = Math.Min((float)(EditorCanvas.Width - padding * 2) / (float)sheet.SheetW,
                                   (float)(EditorCanvas.Height - padding * 2) / (float)sheet.SheetH);
        float finalScale = baseScale * (float)editor.Scale;

        // Расчет координат отпускания с учетом PanX/PanY
        double sheetX = (touch.X - editor.PanX) / finalScale;
        double sheetY = (touch.Y - editor.PanY) / finalScale;

        bool isOutsideSheet = sheetX < 0 || sheetX > sheet.SheetW ||
                              sheetY < 0 || sheetY > sheet.SheetH;

        if (isOutsideSheet)
        {
            editor.MoveToStorage(editor.SelectedPart);
        }
        else
        {
            bool success = editor.TryPlacePart(editor.SelectedPart, editor.SelectedPart.X, editor.SelectedPart.Y);
            if (!success)
            {
                editor.SelectedPart.X = _originalPartX;
                editor.SelectedPart.Y = _originalPartY;
            }
        }
        EditorCanvas.Invalidate();
    }

    // 1. Начало перетаскивания (из склада)
    // Вставьте это рядом с другими методами OnCanvas...
    private void OnPartDragStarting(object sender, DragStartingEventArgs e)
    {
        // Получаем элемент, который потянули
        var element = sender as Element;
        // Берем его данные (StorageItem)
        var storageItem = element?.BindingContext as StorageItem;

        if (storageItem != null && storageItem.Count > 0)
        {
            // Кладем деталь в "корзину" перетаскивания под ключом "StorageItem"
            e.Data.Properties["StorageItem"] = storageItem;

            // Опционально: можно добавить текст или визуальную метку
            e.Data.Text = storageItem.DetailId.ToString();
        }
        else
        {
            // Если деталей 0, отменяем перетаскивание
            e.Cancel = true;
        }
    }



    private void OnMergeModeClicked(object sender, EventArgs e)
    {
        if (ViewModel?.LastEdit == null) return;

        // Переключаем режим
        ViewModel.LastEdit.IsMergingMode = !ViewModel.LastEdit.IsMergingMode;

        // Визуальная индикация кнопки (по желанию)
        var btn = sender as Button;
        if (ViewModel.LastEdit.IsMergingMode)
        {
            btn.BackgroundColor = Colors.Orange;
            btn.Text = "✅ Выберите 2 зоны";
        }
        else
        {
            btn.BackgroundColor = Color.FromArgb("#6750A4");
            btn.Text = "🔗 Объединить остатки";
        }
    }


    private void OnStoragePartTapped(object sender, TappedEventArgs e)
    {
        // Получаем элемент, по которому кликнули
        var element = sender as Element;

        // Получаем привязанный к нему объект StorageItem
        var storageItem = element?.BindingContext as StorageItem;

        if (storageItem != null)
        {
            // Выполняем поворот (меняем размеры и обновляем превью)
            storageItem.Rotate();

            // Опционально: Вибрация для тактильного отклика
            try { HapticFeedback.Perform(HapticFeedbackType.Click); } catch { }
        }
    }

    private async void OnRotateZoneDrop(object sender, DropEventArgs e)
    {
        if (e.Data.Properties.TryGetValue("StorageItem", out var itemObj) && itemObj is StorageItem storageItem)
        {
            // 1. Запоминаем старые размеры для лога
            double oldW = storageItem.Width;
            double oldL = storageItem.Length;

            // 2. Вызываем поворот
            storageItem.Rotate();

            // 3. Выводим отладочное окно с данными
            await Shell.Current.DisplayAlert("Debug",
                $"Было: {oldL}x{oldW}\n" +
                $"Стало: {storageItem.Length}x{storageItem.Width}\n" +
                $"CanRotate: {storageItem.CanRotate}", "OK");

            // 4. ХАК для обновления UI (если данные изменились, а картинка нет)
            // Иногда CollectionView нужно "пнуть", обновив ссылку в источнике
            var index = ViewModel.LastEdit.StorageItems.IndexOf(storageItem);
            if (index != -1)
            {
                ViewModel.LastEdit.StorageItems[index] = null;
                ViewModel.LastEdit.StorageItems[index] = storageItem;
            }
        }
    }

    private void OnStorageItemSelected(object sender, SelectionChangedEventArgs e)
    {
        // Берем выбранный элемент
        var selectedItem = e.CurrentSelection.FirstOrDefault() as StorageItem;

        if (selectedItem != null)
        {
            // 1. Поворачиваем
            selectedItem.Rotate();

            // 2. Сбрасываем выделение, чтобы можно было кликнуть еще раз
            if (sender is CollectionView cv)
            {
                cv.SelectedItem = null;
            }

            // Опционально: вибрация
            try { HapticFeedback.Perform(HapticFeedbackType.Click); } catch { }
        }
    }

    // 2. Бросание детали (на лист)
    private async void OnCanvasDrop(object sender, DropEventArgs e)
    {
        // 1. Получаем данные
        if (!e.Data.Properties.TryGetValue("StorageItem", out var val) || val is not StorageItem storageItem)
            return;

        var editor = ViewModel.LastEdit;
        var sheet = editor.SelectedSheet;
        if (sheet == null) return;

        // --- ИСПРАВЛЕНИЕ ЛОГИКИ ЦВЕТА ---
        // Проверяем цвет только если на листе УЖЕ есть детали.
        // Если лист пустой (Parts.Count == 0), проверку пропускаем, позволяя любому цвету "занять" лист.
        if (sheet.Parts.Count > 0)
        {
            string sheetColor = sheet.ColorName ?? "";
            string partColor = storageItem.Color ?? "";
            if ((sheetColor == "Без цвета") && (partColor == ""))
            {
            }

            else if(sheetColor != partColor)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Нельзя поместить деталь цвета '{partColor}' на лист с цветом '{sheetColor}'.", "OK");
                return;
            }
        }
        // --------------------------------

        // 2. Рассчитываем координаты (Ваша рабочая логика без изменений)
        var position = e.GetPosition(EditorCanvas);
        if (!position.HasValue) return;

        float padding = CuttingEditorForm.CanvasPadding;

        float currentVisualScale = Math.Min((float)(EditorCanvas.Width - padding * 2) / (float)sheet.SheetW,
                                            (float)(EditorCanvas.Height - padding * 2) / (float)sheet.SheetH);
        float finalScale = currentVisualScale * (float)editor.Scale;

        double dropX = (position.Value.X - padding) / finalScale - ViewModel.Settings.EdgeOffset;
        double dropY = (position.Value.Y - padding) / finalScale - ViewModel.Settings.EdgeOffset;

        dropX -= storageItem.Length / 2.0;
        dropY -= storageItem.Width / 2.0;

        // 3. Создаем новую деталь
        var newPart = new PlacedPart
        {
            DetailId = storageItem.DetailId,
            Color = storageItem.Color,
            Length = storageItem.Length,
            Width = storageItem.Width,
            X = dropX,
            Y = dropY,
            IsRotated = false
        };

        // 4. Пытаемся разместить
        // Здесь сработает логика из CuttingEditorForm, которая присвоит листу цвет первой детали
        if (editor.TryPlacePart(newPart, dropX, dropY))
        {
            storageItem.Count--;

            if (storageItem.Count <= 0)
            {
                editor.StorageItems.Remove(storageItem);
            }

            editor.SelectedPart = newPart;
            EditorCanvas.Invalidate();
        }
    }


}