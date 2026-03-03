using MyApp1;

namespace MyApp1.Resources.ContentViews.Workshop;

public partial class DoorWorkshopContentView : ContentView
{
    // Событие для обновления родительской страницы
    public event EventHandler? RefreshRequested;

    public DoorWorkshopContentView()
    {
        InitializeComponent();
    }

    private void OnRefreshClicked(object sender, EventArgs e)
    {
        RefreshRequested?.Invoke(this, EventArgs.Empty);
    }

    // Обработка чекбокса "Взять в работу"
    private async void OnTaskTakenChecked(object sender, CheckedChangedEventArgs e)
    {
        // Проверяем, что инициатор - это CheckBox и контекст - это DoorWorkshopItem (или ваш тип данных)
        if (sender is CheckBox cb && cb.BindingContext is DoorWorkshopItem item)
        {
            // 1. Получаем имя текущего работника
            string currentWorker = App.CurrentUser?.Name ?? "Работник";

            var apiService = new ApiService();

            // 2. Вызываем метод с расширенным результатом (успех + сообщение)
            // Предполагается, что вы обновили ApiService методом UpdateTaskTakeStatusWithResultAsync
            var (isSuccess, errorMessage) = await apiService.UpdateTaskTakeStatusWithResultAsync(item.Id, e.Value, currentWorker);

            if (!isSuccess)
            {
                // 3. Откат визуального состояния при ошибке
                // Отписываемся от события, чтобы изменение IsChecked не вызвало этот метод повторно
                cb.CheckedChanged -= OnTaskTakenChecked;
                cb.IsChecked = !e.Value;
                cb.CheckedChanged += OnTaskTakenChecked;

                // 4. Показываем конкретную ошибку от сервера (например, "Задача уже занята работником: Иван")
                string title = errorMessage.Contains("занята") ? "Задача занята" : "Ошибка";

                await App.Current.MainPage.DisplayAlert(
                    title,
                    string.IsNullOrEmpty(errorMessage) ? "Не удалось обновить статус 'В работе'" : errorMessage,
                    "ОК");

                // 5. Опционально: Обновляем данные всей страницы, чтобы увидеть актуальные имена в списке
                // RefreshRequested?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    // Обработка чекбокса "Готово"
    private async void OnTaskDoneChecked(object sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox cb && cb.BindingContext is DoorWorkshopItem item)
        {
            // 1. Проверка: Действительно ли пользователь хочет завершить задачу?
            // Запрашиваем подтверждение только при установке галочки (завершении)
            if (e.Value)
            {
                bool confirm = await App.Current.MainPage.DisplayAlert(
                    "Завершение",
                    "Вы действительно хотите пометить задачу как выполненную?",
                    "Да", "Нет");

                if (!confirm)
                {
                    // Откатываем визуальное состояние без вызова события
                    cb.CheckedChanged -= OnTaskDoneChecked;
                    cb.IsChecked = false;
                    cb.CheckedChanged += OnTaskDoneChecked;
                    return;
                }
            }

            // 2. Логика API
            var newStatus = e.Value ? ProductionTaskStatus.Ready : ProductionTaskStatus.Pending;
            string workerName = App.CurrentUser?.Name ?? "Работник";

            var apiService = new ApiService();
            bool success = await apiService.UpdateTaskStatusAsync(item.Id, newStatus, workerName);

            if (success)
            {
                item.Status = newStatus;

                // Если задачу пометили как готовую, можно показать уведомление
                if (e.Value)
                {
                    await App.Current.MainPage.DisplayAlert("Успех", "Статус обновлен: Готово", "ОК");
                    // Опционально: вызвать обновление списка
                    // RefreshRequested?.Invoke(this, EventArgs.Empty);
                }
            }
            else
            {
                // 3. Откат при ошибке сервера
                cb.CheckedChanged -= OnTaskDoneChecked;
                cb.IsChecked = !e.Value;
                cb.CheckedChanged += OnTaskDoneChecked;
                await App.Current.MainPage.DisplayAlert("Ошибка", "Не удалось сохранить статус на сервере", "ОК");
            }
        }
    }

    // Кнопка перехода к деталям
    private async void OnOpenDetailsClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is DoorWorkshopItem item)
        {
            if (item.Data == null)
            {
                await App.Current.MainPage.DisplayAlert("Инфо", "Детальные данные отсутствуют", "OK");
                return;
            }

            // Открываем страницу деталей
            await Navigation.PushAsync(new DoorDetailsPage(item));
        }
    }
}