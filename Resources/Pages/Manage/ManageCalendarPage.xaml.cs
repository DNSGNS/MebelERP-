namespace MyApp1;

public partial class ManageCalendarPage : ContentPage
{
	public ManageCalendarPage(List<ProjectManageData> projects)
    {
        InitializeComponent();
        BindingContext = new CalendarForm(projects);
    }
    // Пустой конструктор для автоматической загрузки
    public ManageCalendarPage()
    {
        InitializeComponent();
        // Мы не вызываем LoadDataAsync прямо в конструкторе (т.к. он async), 
        // а переносим вызов в OnAppearing
    }

    // Метод, который срабатывает при открытии страницы
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Если BindingContext еще не задан (значит вызван пустой конструктор), загружаем данные
        if (BindingContext == null)
        {
            await LoadDataAsync();
        }
    }

    private async Task LoadDataAsync()
    {
        try
        {
            // 1. Показываем индикатор загрузки (если он есть в XAML, можно добавить Overlay)
            // Здесь используем логику аналогичную ManageMainPage
            var apiService = new ApiService();

            // 2. Запрашиваем данные от API
            var response = await apiService.GetInstallationPlanningDataAsync();

            if (response != null)
            {
                // 3. Маппим данные в нашу форму (используем тот же маппер, что и в основной странице)
                var form = ProjectMapper.MapToInstallerForm(response);

                // 4. Устанавливаем BindingContext для календаря
                // Передаем список проектов (преобразуем ObservableCollection в List)
                BindingContext = new CalendarForm(form.Projects.ToList());
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", "Не удалось загрузить данные для календаря: " + ex.Message, "OK");
        }
    }

    private async void OnDayTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is CalendarDay day && day.Projects.Any())
        {
            // Собираем список названий проектов для отображения
            var actionSheetOptions = day.Projects
                .Select(p => $"{p.ProjectName} ({p.InstallersDisplay})")
                .ToArray();

            // Показываем стандартное меню выбора
            // Можно заменить на переход на страницу деталей, если нужно
            string action = await DisplayActionSheet(
                $"Заказы на {day.DayNumber}.{day.Date:MM}:",
                "Закрыть",
                null,
                actionSheetOptions);

            // Опционально: если нажали на конкретный проект в списке
            if (action != "Закрыть" && action != null)
            {
                // Логика перехода к проекту, если нужно
                // Например: найти проект по имени и открыть его редактирование
            }
        }
        else if (e.Parameter is CalendarDay emptyDay && !emptyDay.IsEmpty)
        {
            // Если день пустой, можно предложить создать проект (опционально)
            // await DisplayAlert("Инфо", "На этот день нет установок", "OK");
        }
    }
}