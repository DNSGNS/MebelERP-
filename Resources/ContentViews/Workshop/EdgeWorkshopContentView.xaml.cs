using MyApp1;

namespace MyApp1.Resources.ContentViews.Workshop;

public partial class EdgeWorkshopContentView : ContentView
{
    public event EventHandler? RefreshRequested;

    public EdgeWorkshopContentView()
    {
        InitializeComponent();
    }

    private void OnRefreshClicked(object sender, EventArgs e)
    {
        RefreshRequested?.Invoke(this, EventArgs.Empty);
    }

    private async void OnTaskTakenChecked(object sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox cb && cb.BindingContext is CuttingWorkshopItem item)
        {
            // 1. Подтверждение действия (только если мастер БЕРЕТ задачу)
            if (e.Value)
            {
                bool confirm = await App.Current.MainPage.DisplayAlert(
                    "Бронирование",
                    "Взять эту задачу в работу?",
                    "Да", "Нет");

                if (!confirm)
                {
                    // Откатываем визуально без вызова события
                    cb.CheckedChanged -= OnTaskTakenChecked;
                    cb.IsChecked = false;
                    cb.CheckedChanged += OnTaskTakenChecked;
                    return;
                }
            }

            // 2. Подготовка данных
            string currentWorker = App.CurrentUser?.Name ?? "Работник";
            var apiService = new ApiService();

            // 3. Запрос к API
            var (isSuccess, errorMessage) = await apiService.UpdateTaskTakeStatusWithResultAsync(item.Id, e.Value, currentWorker);

            if (!isSuccess)
            {
                // 4. Откат при ошибке сервера (например, если кто-то другой успел занять)
                cb.CheckedChanged -= OnTaskTakenChecked;
                cb.IsChecked = !e.Value;
                cb.CheckedChanged += OnTaskTakenChecked;

                string title = errorMessage.Contains("занята") ? "Задача занята" : "Ошибка";
                await App.Current.MainPage.DisplayAlert(title, errorMessage, "OK");

                // Опционально: вызвать обновление списка, чтобы увидеть реального владельца
                // RefreshRequested?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private async void OnTaskDoneChecked(object sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox cb && cb.BindingContext is CuttingWorkshopItem item)
        {
            var apiService = new ApiService();
            string WorkerName = App.CurrentUser.Name;
            // В БД статус выполнения обычно числовой или булевый, адаптируйте под ваш API
            bool success = await apiService.UpdateTaskStatusAsync(item.Id, ProductionTaskStatus.Ready, WorkerName);

            if (!success)
            {
                cb.CheckedChanged -= OnTaskDoneChecked;
                cb.IsChecked = !e.Value;
                cb.CheckedChanged += OnTaskDoneChecked;
                await App.Current.MainPage.DisplayAlert("Ошибка", "Ошибка сохранения результата", "ОК");
            }
        }
    }

    private async void OnOpenReportClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is CuttingWorkshopItem item)
        {
            if (item.Data == null)
            {
                await App.Current.MainPage.DisplayAlert("Ошибка", "Данные чертежа отсутствуют", "ОК");
                return;
            }

            var tempViewModel = new CuttingData { SavedReport = item.Data };
            var reportPage = new CuttingSavedReportPage { BindingContext = tempViewModel };

            await Navigation.PushAsync(reportPage);
        }
    }
}