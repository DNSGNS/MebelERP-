using MyApp1.Resources.ContentViews.Workshop;

namespace MyApp1;

public partial class ObjectWorkshopEditorPage : ContentPage
{
    private readonly ApiService _apiService = new ApiService();
    // Наш класс хранения данных, который мы создали ранее
    public WorkshopData WorkshopViewModel { get; set; } = new WorkshopData();

    private string _currentTab = "Cutting";
    private string _workerName = App.CurrentUser.Name; // В реальности берите из настроек профиля или логина

    public ObjectWorkshopEditorPage()
    {
        InitializeComponent();
        BindingContext = WorkshopViewModel;
    }

    // Загружаем данные при открытии страницы
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RefreshData();
    }

    private async Task RefreshData()
    {
        // 1. Получаем "сырые" данные из БД
        WorkshopViewModel.IsBusy = true;
        try
        {
            // 1. Получаем данные
            var apiTasks = await _apiService.GetProductionTasksForWorkerAsync(_workerName);

            // 2. Распределяем их по спискам внутри WorkshopData (метод FillFromApi мы писали выше)
            WorkshopViewModel.FillFromApi(apiTasks);

            // 3. Обновляем текущую вкладку, чтобы она увидела новые данные
            SwitchToTab(_currentTab);
        }
        catch (Exception ex)
        {
            // Обработка ошибок
            await DisplayAlert("Ошибка", $"Не удалось загрузить данные: {ex.Message}", "OK");
        }
        finally
        {
            // Выключаем индикатор в любом случае
            WorkshopViewModel.IsBusy = false;
        }
    }

    private void OnTabClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is string tabName)
        {
            _currentTab = tabName;
            SwitchToTab(tabName);
        }
    }

    private void SwitchToTab(string tabName)
    {
        UpdateTabButtonsUI(tabName);

        ContentView newContent = null;

        switch (tabName)
        {
            case "Cutting":
                var cuttingView = new CuttingWorkshopContentView();
                // Подписываемся на кнопку обновления внутри вьюшки
                cuttingView.RefreshRequested += async (s, e) => await RefreshData();
                newContent = cuttingView;
                break;

            case "Layout":
                var MDFView = new MDFWorkshopContentView();
                // Подписываемся на кнопку обновления внутри вьюшки
                MDFView.RefreshRequested += async (s, e) => await RefreshData();
                newContent = MDFView;
                break;
            case "Doors":
                var DoorView = new DoorWorkshopContentView();
                // Подписываемся на кнопку обновления внутри вьюшки
                DoorView.RefreshRequested += async (s, e) => await RefreshData();
                newContent = DoorView;
                break;
            case "Edge":
                var edgeView = new EdgeWorkshopContentView();
                edgeView.RefreshRequested += async (s, e) => await RefreshData();
                newContent = edgeView;
                break;
        }

        if (newContent != null)
        {
            // Передаем ViewModel
            newContent.BindingContext = WorkshopViewModel;
            ContentContainer.Content = newContent;
        }
    }
    private void UpdateTabButtonsUI(string tabName)
    {
        btnCutting.BackgroundColor = tabName == "Cutting" ? Color.FromArgb("#6750A4") : Colors.Transparent;
        btnLayout.BackgroundColor = tabName == "Layout" ? Color.FromArgb("#6750A4") : Colors.Transparent;
        btnDoors.BackgroundColor = tabName == "Doors" ? Color.FromArgb("#6750A4") : Colors.Transparent;
        btnEdge.BackgroundColor = tabName == "Edge" ? Color.FromArgb("#6750A4") : Colors.Transparent;
    }

    // Добавим кнопку "Обновить" в XAML или обработчик для Pull-to-Refresh
    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await RefreshData();
    }
}