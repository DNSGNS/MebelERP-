namespace MyApp1;

public partial class SalaryReportPage : ContentPage
{
    private SalaryForm _viewModel;
    private readonly ApiService _apiService;
    private bool _isLoaded = false;


    // Конструктор принимает необязательное имя работника
    public SalaryReportPage(string workerName = null)
    {
        InitializeComponent();

        _apiService = new ApiService();
        _viewModel = new SalaryForm();

        // Настраиваем режим работы (один сотрудник или все)
        if (!string.IsNullOrEmpty(workerName))
        {
            _viewModel.CurrentWorkerName = workerName;
            BtnAddRecord.IsVisible = true; // Показываем кнопку добавления только для конкретного работника
            this.Title = $"Зарплата: {workerName}";
        }
        else
        {
            _viewModel.CurrentWorkerName = null;
            BtnAddRecord.IsVisible = false; // Скрываем кнопку
            this.Title = "Общий отчет по зарплатам";
        }

        int currentYear = DateTime.Now.Year;
        for (int year = 2023; year <= currentYear + 1; year++)
        {
            YearPicker.Items.Add(year.ToString());
        }


        BindingContext = _viewModel;

        // Устанавливаем текущий месяц и год в фильтры по умолчанию
        MonthPicker.SelectedIndex = DateTime.Now.Month - 1; // Индекс с 0
        YearPicker.SelectedItem = DateTime.Now.Year.ToString();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Убираем условие if (!_isLoaded)
        // Теперь LoadDataAsync будет вызываться каждый раз при возврате на эту страницу
        await LoadDataAsync();
    }
    private async Task LoadDataAsync()
    {
        _viewModel.IsBusy = true;

        var response = await _apiService.GetSalaryReportAsync();

        if (response != null)
        {
            // 1. Прямое копирование данных из DTO в элементы списка
            _viewModel._allSourceItems = response.Reports
                .Select(x => new SalaryReportItem
                {
                    Id = x.Id,
                    DatePerformed = x.DatePerformed,
                    WorkerId = x.WorkerId,
                    WorkerName = x.WorkerName,
                    ProjectId = x.ProjectId,
                    ProjectName = x.ProjectName,

                    // Теперь типы совпадают (MaterialType? = MaterialType?)
                    Material = x.Material,

                    Saw = x.Saw,
                    Edging = x.Edging,
                    Additive = x.Additive,
                    DoorCanvas = x.DoorCanvas,
                    DoorSectional = x.DoorSectional,
                    Packaging = x.Packaging,
                    Installation = x.Installation,
                    GrindingSoap = x.GrindingSoap,
                    GrindingFrez = x.GrindingFrez,
                    Milling = x.Milling,
                    Additionally = x.Additionally,
                    Measurement = x.Measurement,
                    Comment = x.Comment,
                    TotalSalary = x.TotalSalary
                })
                .ToList();

            // 2. Сохраняем список проектов
            _viewModel.AllProjects = response.Projects.Select(p => new ProjectSimpleDto
            {
                Id = p.Id,
                Name = p.Name,
                TotalProjectPrice = p.TotalProjectPrice
            }).ToList();

            _viewModel.AllWorkers = response.Workers.Select(w => new WorkerSimpleDto
            {
                Id = w.Id,
                Name = w.Name
            }).ToList();

            // 3. Логика поиска ID работника
            if (!string.IsNullOrEmpty(_viewModel.CurrentWorkerName))
            {
                var workerRecord = response.Workers
                    .FirstOrDefault(x => x.Name == _viewModel.CurrentWorkerName);

                if (workerRecord != null)
                {
                    _viewModel.CurrentWorkerId = workerRecord.Id;
                }
            }

            ApplyCurrentFilters();
        }
        else
        {
            await DisplayAlert("Ошибка", "Не удалось загрузить отчет", "OK");
        }

        _viewModel.IsBusy = false;
    }

    private void OnFilterChanged(object sender, EventArgs e)
    {
        if (_viewModel._allSourceItems.Any())
        {
            ApplyCurrentFilters();
        }
    }

    private void ApplyCurrentFilters()
    {
        if (MonthPicker.SelectedIndex == -1 || YearPicker.SelectedItem == null) return;

        int selectedMonth = MonthPicker.SelectedIndex + 1;
        int selectedYear = int.Parse(YearPicker.SelectedItem.ToString());

        _viewModel.ApplyFilters(selectedMonth, selectedYear);
    }

    // --- ЗАГЛУШКИ ДЛЯ КНОПОК ---

    private async void OnEditRecordTapped(object sender, TappedEventArgs e)
    {
        // Получаем объект записи из параметра нажатия
        var item = e.Parameter as SalaryReportItem;
        if (item == null) return;

        // Переходим на страницу редактирования, используя новый конструктор
        // Передаем саму запись и список всех проектов для выпадающего списка
        await Navigation.PushAsync(new AddSalaryRecordPage(item, _viewModel.AllProjects));
    }

    private async void OnDeleteRecordTapped(object sender, TappedEventArgs e)
    {
        var item = e.Parameter as SalaryReportItem;
        if (item == null) return;

        bool confirm = await DisplayAlert("Удаление", $"Удалить запись по проекту {item.ProjectName}?", "Да", "Нет");

        if (confirm)
        {
            _viewModel.IsBusy = true;

            // Вызываем API по реальному Id
            bool success = await _apiService.DeleteSalaryRecordAsync(item.Id);

            if (success)
            {
                // Удаляем из локальных списков, чтобы не перегружать всё с сервера
                _viewModel._allSourceItems.Remove(item);
                _viewModel.DisplayItems.Remove(item);
                _viewModel.TotalSum = _viewModel.DisplayItems.Sum(x => x.TotalSalary);
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось удалить запись на сервере", "OK");
            }

            _viewModel.IsBusy = false;
        }
    }

    private async void OnAddRecordClicked(object sender, EventArgs e)
    {
        if (_viewModel.CurrentWorkerId == null || string.IsNullOrEmpty(_viewModel.CurrentWorkerName))
        {
            await DisplayAlert("Внимание", "Данные работника не определены", "OK");
            return;
        }

        // Переходим на страницу создания, передавая нужные данные
        await Navigation.PushAsync(new AddSalaryRecordPage(
            _viewModel.CurrentWorkerName,
            _viewModel.CurrentWorkerId.Value,
            _viewModel.AllProjects));
    }
}