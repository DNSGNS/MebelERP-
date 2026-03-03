using System.Collections.ObjectModel;

namespace MyApp1;

public partial class MonthlyReportPage : ContentPage
{
    private readonly ApiService _apiService = new ApiService();

    // Храним полный список данных, полученных от API
    private List<MonthlyReportItem> _allReports = new();

    // Список для отображения (привязан к UI)
    public ObservableCollection<MonthlyReportItem> ReportItems { get; set; } = new();

    public MonthlyReportPage()
    {
        InitializeComponent();
        BindingContext = this;

        // Настройка шаблона
        BindableLayout.SetItemTemplate(ItemsList, CreateItemTemplate());
        BindableLayout.SetItemsSource(ItemsList, ReportItems);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAllDataAsync();
    }

    private async Task LoadAllDataAsync()
    {
        // 1. Загружаем ВЕСТЬ отчет из API
        // Используем новый метод, который мы создали ранее
        var data = await _apiService.GetFinancialReportAsync();

        if (data == null || data.Count == 0) return;

        _allReports = data;

        // 2. Извлекаем уникальные года из полученных данных
        var availableYears = _allReports
            .Select(r => r.Year.ToString())
            .Distinct()
            .OrderByDescending(y => y) // Свежие года сверху
            .ToList();

        // 3. Заполняем Пикер
        YearPicker.ItemsSource = availableYears;

        // 4. Выбираем первый (самый свежий) год по умолчанию
        if (availableYears.Any())
        {
            // Это автоматически вызовет OnYearChanged, если он подписан в XAML
            YearPicker.SelectedItem = availableYears.First();
        }
    }

    // Обработчик смены года в Пикере
    private void OnYearChanged(object sender, EventArgs e)
    {
        if (YearPicker.SelectedItem == null) return;

        string selectedYearStr = YearPicker.SelectedItem.ToString();
        if (int.TryParse(selectedYearStr, out int year))
        {
            FilterDataByYear(year);
        }
    }

    private void FilterDataByYear(int year)
    {
        ReportItems.Clear();

        // Фильтруем уже загруженные данные локально (без запроса к API)
        var filtered = _allReports
            .Where(r => r.Year == year)
            .OrderByDescending(r => r.Month)
            .ToList();

        foreach (var item in filtered)
        {
            ReportItems.Add(item);
        }
    }
    private async void OnRefreshClicked(object sender, EventArgs e) => await LoadAllDataAsync();

    private async void OnChartDummyClicked(object sender, EventArgs e)
    {
        // Передаем ВСЕ данные (_allReports), а не только отфильтрованные (ReportItems)
        if (_allReports == null || !_allReports.Any())
        {
            await DisplayAlert("Внимание", "Данные еще не загружены", "OK");
            return;
        }

        // Теперь страница графика получит полную историю
        await Navigation.PushAsync(new SalesChartPage(_allReports.ToList()));
    }

    private DataTemplate CreateItemTemplate()
    {
        return new DataTemplate(() =>
        {
            var border = new Border
            {
                StrokeThickness = 0,
                BackgroundColor = Colors.White,
                Padding = new Thickness(0, 8),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 }
            };

            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new(100), new(70), new(90), new(100), new(100), new(100), new(100), new(80)
                },
                ColumnSpacing = 5,
                WidthRequest = 740
            };

            // Стили берем из XAML ресурсов
            var style = (Style)Resources["DataLabelStyle"];

            var monthLbl = new Label { Style = style };
            monthLbl.SetBinding(Label.TextProperty, "MonthName");
            Grid.SetColumn(monthLbl, 0);

            var countLbl = new Label { Style = style };
            countLbl.SetBinding(Label.TextProperty, "OrderCount");
            Grid.SetColumn(countLbl, 1);

            var avgLbl = new Label { Style = style };
            avgLbl.SetBinding(Label.TextProperty, new Binding("AverageOrderValue", stringFormat: "{0:N0}"));
            Grid.SetColumn(avgLbl, 2);

            var salesLbl = new Label { Style = style, FontAttributes = FontAttributes.Bold };
            salesLbl.SetBinding(Label.TextProperty, new Binding("TotalSales", stringFormat: "{0:N0}"));
            Grid.SetColumn(salesLbl, 3);

            var realLbl = new Label { Style = style };
            realLbl.SetBinding(Label.TextProperty, new Binding("Realization", stringFormat: "{0:N0}"));
            Grid.SetColumn(realLbl, 4);

            var expLbl = new Label { Style = style, TextColor = Color.FromArgb("#C62828") };
            expLbl.SetBinding(Label.TextProperty, new Binding("TotalExpenses", stringFormat: "{0:N0}"));
            Grid.SetColumn(expLbl, 5);

            var profitLbl = new Label { Style = style, FontAttributes = FontAttributes.Bold };
            profitLbl.SetBinding(Label.TextProperty, new Binding("NetProfit", stringFormat: "{0:N0}"));
            // Предполагается, что в модели есть свойство ProfitColor типа Color или string
            profitLbl.SetBinding(Label.TextColorProperty, "ProfitColor");
            Grid.SetColumn(profitLbl, 6);

            var marginLbl = new Label { Style = style };
            marginLbl.SetBinding(Label.TextProperty, new Binding("Margin", stringFormat: "{0:F2}%"));
            Grid.SetColumn(marginLbl, 7);

            grid.Add(monthLbl); grid.Add(countLbl); grid.Add(avgLbl); grid.Add(salesLbl);
            grid.Add(realLbl); grid.Add(expLbl); grid.Add(profitLbl); grid.Add(marginLbl);

            border.Content = grid;
            return border;
        });
    }
}



  