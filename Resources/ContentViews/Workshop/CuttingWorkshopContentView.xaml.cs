using MyApp1; 

namespace MyApp1.Resources.ContentViews.Workshop;

public partial class CuttingWorkshopContentView : ContentView
{
    // Событие, на которое может подписаться родительская страница
    public event EventHandler? RefreshRequested;

    public CuttingWorkshopContentView()
    {
        InitializeComponent();
    }

    // 1. Обработчик кнопки обновления
    private void OnRefreshClicked(object sender, EventArgs e)
    {
        // Вызываем событие, чтобы родительская страница (ObjectWorkshopEditorPage) обновила данные
        RefreshRequested?.Invoke(this, EventArgs.Empty);
    }

    // 2. Затычка для чекбокса "Взял в работу"
    // 2. Обработчик чекбокса "Взял в работу" с расширенной обработкой ошибок
    private async void OnTaskTakenChecked(object sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox cb && cb.BindingContext is CuttingWorkshopItem item)
        {
            // 1. Получаем имя текущего работника
            string currentWorker = App.CurrentUser?.Name ?? "Неизвестный работник";

            var apiService = new ApiService();

            // 2. Вызываем метод API, который возвращает (Успех, Сообщение)
            // Примечание: Убедитесь, что вы обновили метод в ApiService, как мы обсуждали ранее
            var (isSuccess, errorMessage) = await apiService.UpdateTaskTakeStatusWithResultAsync(item.Id, e.Value, currentWorker);

            if (!isSuccess)
            {
                // 3. Откат визуального состояния БЕЗ повторного срабатывания события
                cb.CheckedChanged -= OnTaskTakenChecked;
                cb.IsChecked = !e.Value;
                cb.CheckedChanged += OnTaskTakenChecked;

                // 4. Показываем пользователю конкретную причину (например, "Задача уже занята работником: Иван")
                // Если сервер не прислал текст, используем стандартную заглушку
                string displayMessage = string.IsNullOrEmpty(errorMessage)
                    ? "Не удалось обновить статус на сервере"
                    : errorMessage;

                await App.Current.MainPage.DisplayAlert("Внимание", displayMessage, "ОК");

                // 5. Опционально: можно вызвать обновление данных, чтобы актуализировать список
                // OnRefreshClicked(this, EventArgs.Empty);
            }
        }
    }

    // 3.  "Готово"
    private async void OnTaskDoneChecked(object sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox cb && cb.BindingContext is CuttingWorkshopItem item)
        {
            // 1. Сначала спрашиваем подтверждение
            string action = e.Value ? "завершить" : "вернуть в работу";
            bool confirm = await App.Current.MainPage.DisplayAlert(
                "Подтверждение",
                $"Вы действительно хотите {action} эту задачу?",
                "Да", "Нет");

            if (!confirm)
            {
                // 2. Если пользователь нажал "Нет" — откатываем состояние чекбокса
                cb.CheckedChanged -= OnTaskDoneChecked; // Отписываемся, чтобы не зациклить
                cb.IsChecked = !e.Value;
                cb.CheckedChanged += OnTaskDoneChecked;
                return; // Выходим из метода
            }

            // 3. Если подтвердил — продолжаем логику API
            var newStatus = e.Value ? ProductionTaskStatus.CutCompleted : ProductionTaskStatus.Pending;
            string workerName = App.CurrentUser?.Name ?? "Работник";

            var apiService = new ApiService();
            bool success = await apiService.UpdateTaskStatusAsync(item.Id, newStatus, workerName);

            if (success)
            {
                if (e.Value)
                {
                    await App.Current.MainPage.DisplayAlert("Готово", "Задача выполнена!", "ОК");
                    // RefreshRequested?.Invoke(this, EventArgs.Empty); // Опционально: обновить список
                }
            }
            else
            {
                // Откат при ошибке сервера
                cb.CheckedChanged -= OnTaskDoneChecked;
                cb.IsChecked = !e.Value;
                cb.CheckedChanged += OnTaskDoneChecked;

                await App.Current.MainPage.DisplayAlert("Ошибка", "Не удалось сохранить результат на сервере", "ОК");
            }
        }
    }

    // 4. Открытие отчета
    private async void OnOpenReportClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is CuttingWorkshopItem item)
        {
            if (item.Data == null)
            {
                await App.Current.MainPage.DisplayAlert("Ошибка", "Данные раскроя отсутствуют", "ОК");
                return;
            }

            // ВАЖНО: CuttingSavedReportPage ожидает CuttingData в качестве BindingContext.
            // Мы создаем временный объект CuttingData и кладем в него наш SavedReport.
            var tempViewModel = new CuttingData
            {
                SavedReport = item.Data
            };

            // Создаем страницу отчета
            var reportPage = new CuttingSavedReportPage();
            reportPage.BindingContext = tempViewModel;

            // Навигация
            await Navigation.PushAsync(reportPage);
        }
    }
}