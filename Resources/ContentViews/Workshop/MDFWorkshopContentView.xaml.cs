using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MyApp1;

public partial class MDFWorkshopContentView : ContentView
{
    public event EventHandler? RefreshRequested;

    private List<(FasadWorkshopItem Item, Entry Input)> _scrapEntries = new();
    public ObservableCollection<FasadWorkshopItem> FilteredFasadTasks { get; set; } = new();
    private WorkshopData? _viewModel;

    private readonly ApiService _apiService = new ApiService();

    public MDFWorkshopContentView()
    {
        InitializeComponent();
        TasksList.ItemsSource = FilteredFasadTasks;
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (BindingContext is WorkshopData vm)
        {
            _viewModel = vm;
            UpdateColorPicker();
            ReloadDataAndFilters();
        }
    }

    private void UpdateColorPicker()
    {
        if (_viewModel == null) return;

        var uniqueColors = _viewModel.FasadTasks
            .Select(x => x.Color)
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        ColorFilterPicker.Items.Clear();
        ColorFilterPicker.Items.Add("Все цвета");

        foreach (var color in uniqueColors)
        {
            ColorFilterPicker.Items.Add(color);
        }

        // Устанавливаем индекс по умолчанию, чтобы текст появился сразу
        if (ColorFilterPicker.SelectedIndex == -1)
            ColorFilterPicker.SelectedIndex = 0;
    }
    private void ReloadDataAndFilters()
    {
        if (_viewModel == null) return;

        string selectedColor = ColorFilterPicker.SelectedItem as string ?? "Все цвета";

        var filtered = _viewModel.FasadTasks.AsEnumerable();

        if (selectedColor != "Все цвета")
            filtered = filtered.Where(x => x.Color == selectedColor);

        FilteredFasadTasks.Clear();
        foreach (var item in filtered)
            FilteredFasadTasks.Add(item);
    }

    private void OnColorFilterChanged(object sender, EventArgs e) => ReloadDataAndFilters();

    private void OnRefreshClicked(object sender, EventArgs e) => RefreshRequested?.Invoke(this, EventArgs.Empty);

    private async void OnBulkCuttingClicked(object sender, EventArgs e)
    {
        var selected = FilteredFasadTasks.Where(x => x.IsTaken).ToList();

        if (!selected.Any())
        {
            await Shell.Current.DisplayAlert("Внимание", "Выберите детали (чекбоксами) для раскроя", "ОК");
            return;
        }

        string workerName = App.CurrentUser?.Name ?? "Работник";
        var apiService = new ApiService();

        // Список для успешно забронированных деталей
        var successfullyTakenItems = new List<FasadWorkshopItem>();
        var errors = new List<string>();

        // 1. Пытаемся забронировать каждую деталь на сервере
        foreach (var task in selected)
        {
            var (isSuccess, errorMessage) = await apiService.UpdateTaskTakeStatusWithResultAsync(task.Id, true, workerName);

            if (isSuccess)
            {
                successfullyTakenItems.Add(task);
            }
            else
            {
                // Сохраняем ошибку (например: "Задача занята работником Иван")
                errors.Add($"- {task.Color}: {errorMessage}");
            }
        }

        // 2. Если были ошибки (кто-то успел занять детали раньше)
        if (errors.Any())
        {
            string message = "Некоторые детали не удалось забронировать:\n" + string.Join("\n", errors.Take(5));
            if (errors.Count > 5) message += $"\n...и еще {errors.Count - 5}";

            await Shell.Current.DisplayAlert("Конфликт бронирования", message, "ОК");
        }

        // 3. Если ни одной детали не удалось взять — прерываем процесс
        if (!successfullyTakenItems.Any())
        {
            RefreshRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        // 4. Подготавливаем данные для страницы раскроя ТОЛЬКО из успешно взятых деталей
        var cuttingData = new CuttingData();
        foreach (var item in successfullyTakenItems)
        {
            cuttingData.DetailsForm.Details.Add(new CuttingDetails
            {
                Id = cuttingData.DetailsForm.Details.Count + 1,
                Length = (int)item.Length,
                Width = (int)item.Width,
                Count = item.Count,
                Color = item.Color,
                MillingText = item.MillingText,
                SelectedEdgeType = item.SelectedEdgeType
            });
        }

        // 5. Открываем страницу раскроя
        await Shell.Current.Navigation.PushAsync(new LayoutPage(new ObjectData(), cuttingData));

        // 6. Обновляем список, чтобы убрать или перекрасить занятые задачи
        RefreshRequested?.Invoke(this, EventArgs.Empty);
    }

    // МЕТОД: Отменить бронирование
    private async void OnCancelBookingClicked(object sender, EventArgs e)
    {
        var selected = FilteredFasadTasks.Where(x => x.IsTaken).ToList();
        if (!selected.Any()) return;

        bool confirm = await Shell.Current.DisplayAlert("Отмена", "Снять бронь с выбранных деталей?", "Да", "Нет");
        if (!confirm) return;

        var apiService = new ApiService();
        int errorCount = 0;

        foreach (var task in selected)
        {
            // Передаем false и пустую строку (сбрасываем работника)
            var (isSuccess, _) = await apiService.UpdateTaskTakeStatusWithResultAsync(task.Id, false, string.Empty);
            if (!isSuccess) errorCount++;
        }

        if (errorCount > 0)
        {
            await Shell.Current.DisplayAlert("Внимание", $"Не удалось отменить бронь для {errorCount} деталей. Возможно, они уже обновлены.", "ОК");
        }

        // Обновляем список через родителя
        RefreshRequested?.Invoke(this, EventArgs.Empty);
    }

    private async void OnCompleteTasksClicked(object sender, EventArgs e)
    {
        var selected = FilteredFasadTasks.Where(x => x.IsTaken).ToList();
        if (!selected.Any()) return;

        bool confirm = await Shell.Current.DisplayAlert("Завершение", "Пометить выбранные детали как готовые?", "Да", "Нет");
        if (!confirm) return;

        string workerName = App.CurrentUser.Name;

        foreach (var task in selected)
        {
            // Устанавливаем статус Ready
            await _apiService.UpdateTaskStatusAsync(task.Id, ProductionTaskStatus.Ready, workerName);
        }

        // Обновляем данные (после этого Ready задачи обычно исчезают из текущего списка)
        RefreshRequested?.Invoke(this, EventArgs.Empty);
    }


    private void OnScrapButtonClicked(object sender, EventArgs e)
    {
        // 1. Берем детали, которые отмечены чекбоксом "В работе" (IsTaken)
        var selected = FilteredFasadTasks.Where(x => x.IsTaken).ToList();

        if (!selected.Any())
        {
            Shell.Current.DisplayAlert("Внимание", "Сначала выберите детали, отметив их галочкой 'В работе'", "OK");
            return;
        }

        // 2. Подготовка контейнера
        ScrapItemsContainer.Children.Clear();
        _scrapEntries.Clear();

        // 3. Создаем строки ввода для каждой детали
        foreach (var task in selected)
        {
            var row = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(100)
            },
                Margin = new Thickness(0, 5)
            };

            // Используем ваши свойства: MillingText, SizeText и Count
            var infoLabel = new VerticalStackLayout { Spacing = 2 };
            infoLabel.Children.Add(new Label
            {
                Text = task.MillingText,
                FontAttributes = FontAttributes.Bold,
                FontSize = 13
            });
            infoLabel.Children.Add(new Label
            {
                Text = $"{task.SizeText} | Всего: {task.Count} шт.",
                FontSize = 11,
                TextColor = Colors.Gray
            });

            var scrapInput = new Entry
            {
                Placeholder = "Брак шт.",
                Text = "0",
                Keyboard = Keyboard.Numeric,
                HorizontalTextAlignment = TextAlignment.Center,
                MaxLength = 3,
                TextColor = Colors.Black
            };

            row.Children.Add(infoLabel);
            Grid.SetColumn(scrapInput, 1);
            row.Children.Add(scrapInput);

            ScrapItemsContainer.Children.Add(row);
            ScrapItemsContainer.Children.Add(new BoxView { HeightRequest = 1, Color = Color.FromArgb("#F0F0F0") });

            _scrapEntries.Add((task, scrapInput));
        }

        ScrapOverlay.IsVisible = true;
    }

    private void OnCancelScrapClicked(object sender, EventArgs e) => ScrapOverlay.IsVisible = false;

    private async void OnConfirmScrapClicked(object sender, EventArgs e)
    {
        var tasksToReport = new List<(FasadWorkshopItem Task, int ScrapCount)>();
        var tasksToComplete = new List<(FasadWorkshopItem Task, int ScrapCount)>(); // Исправил опечатку

        // Защита от вылета (NullReferenceException), о которой ты говорил ранее
        string workerName = App.CurrentUser?.Name ?? "Работник";

        foreach (var entry in _scrapEntries)
        {
            // Проверяем, что введено число и оно не отрицательное
            if (int.TryParse(entry.Input.Text, out int val) && val >= 0)
            {
                // 1. Проверка на превышение общего кол-ва
                if (val > entry.Item.Count)
                {
                    await Shell.Current.DisplayAlert("Ошибка",
                        $"Брак ({val}) не может быть больше общего кол-ва ({entry.Item.Count}) для {entry.Item.MillingText}", "OK");
                    return;
                }

                // 2. Распределяем: если 0 — в готовые, если > 0 — в брак
                if (val == 0)
                {
                    tasksToComplete.Add((entry.Item, val));
                }
                else
                {
                    tasksToReport.Add((entry.Item, val));
                }
            }
        }

        ScrapOverlay.IsVisible = false;

        int successCount = 0;

        // Отправляем брак
        foreach (var report in tasksToReport)
        {
            var (isSuccess, _) = await _apiService.ReportScrapAsync(report.Task.Id, report.ScrapCount);
            if (isSuccess) successCount++;
        }

        // Отправляем готовые (где ввели 0)
        foreach (var report in tasksToComplete)
        {
            var isSuccess = await _apiService.UpdateTaskStatusAsync(report.Task.Id, ProductionTaskStatus.Ready, workerName);
            if (isSuccess) successCount++;
        }

        if (successCount > 0)
        {
            await Shell.Current.DisplayAlert("Успех", $"Обработано позиций: {successCount}.", "OK");
            RefreshRequested?.Invoke(this, EventArgs.Empty);
        }
    }


}