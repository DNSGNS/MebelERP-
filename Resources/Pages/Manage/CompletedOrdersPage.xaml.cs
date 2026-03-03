using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MyApp1;

public partial class CompletedOrdersPage : ContentPage, INotifyPropertyChanged
{
    // Используем ObservableCollection для автоматического обновления UI
    public ObservableCollection<CompletedProject> CompletedProjects { get; set; } = new();

    // Храним все данные здесь для локальной фильтрации
    private List<CompletedProject> _allProjects = new();
    public List<string> Years { get; set; } = new();
    private string _selectedYear = "Все";
    public string SelectedYear
    {
        get => _selectedYear;
        set
        {
            _selectedYear = value;
            OnPropertyChanged();
        }
    }

    private readonly ApiService _apiService;

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged(); // Уведомляем UI об изменении
        }
    }

    public CompletedOrdersPage()
    {
        InitializeComponent();
        _apiService = new ApiService();

        GenerateYearList();

        // Устанавливаем BindingContext на саму страницу (или на ViewModel, если она есть)
        BindingContext = this;
    }

    private void GenerateYearList()
    {
        int currentYear = DateTime.Now.Year;
        Years.Clear();
        Years.Add("Все");
        // Текущий, -1, -2, -3
        for (int i = 0; i <= 3; i++)
        {
            Years.Add((currentYear - i).ToString());
        }
        SelectedYear = "Все";
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadCompletedData();
    }

    private async Task LoadCompletedData()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true; // Показываем индикатор
            var data = await _apiService.GetCompletedProjectsAsync();

            if (data != null)
            {
                _allProjects = data; // Сохраняем полный список
                ApplyFilter();       // Применяем текущий фильтр
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось загрузить архив: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false; // Скрываем индикатор в любом случае
        }
    }

    private void OnYearSelected(object sender, EventArgs e)
    {
        // Сбрасываем выделение при смене года
        foreach (var p in _allProjects) p.IsSelected = false;
        ApplyFilter();
    }

    private void OnSelectAllClicked(object sender, EventArgs e)
    {
        // Выделяем только те, что сейчас видны (в отфильтрованной коллекции)
        foreach (var project in CompletedProjects)
        {
            project.IsSelected = true;
        }
    }

    private async void OnDeleteSelectedClicked(object sender, EventArgs e)
    {
        var selectedIds = CompletedProjects.Where(p => p.IsSelected).Select(p => p.Id).ToList();

        if (!selectedIds.Any())
        {
            await DisplayAlert("Внимание", "Ни один проект не выбран", "OK");
            return;
        }

        bool confirm = await DisplayAlert("Подтверждение", $"Удалить выбранные проекты ({selectedIds.Count} шт.)? Это действие необратимо.", "Удалить", "Отмена");
        if (!confirm) return;

        try
        {
            IsBusy = true;
            bool success = await _apiService.DeleteCompletedProjectsAsync(selectedIds);

            if (success)
            {
                // Удаляем локально из общего списка
                _allProjects.RemoveAll(p => selectedIds.Contains(p.Id));
                ApplyFilter();
                await DisplayAlert("Успех", "Проекты удалены", "OK");
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось удалить проекты на сервере", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFilter()
    {
        if (_allProjects == null) return;

        IEnumerable<CompletedProject> filtered;

        if (SelectedYear == "Все")
        {
            filtered = _allProjects;
        }
        else if (int.TryParse(SelectedYear, out int year))
        {
            filtered = _allProjects.Where(p => p.CreateTime.Year == year);
        }
        else
        {
            filtered = _allProjects;
        }

        // Обновляем коллекцию для UI
        CompletedProjects.Clear();
        foreach (var item in filtered)
        {
            CompletedProjects.Add(item);
        }
    }

    private async void OnFurnitureClicked(object sender, EventArgs e)
    {
        // Заглушка для перехода к фурнитуре
        var button = sender as Button;
        if (button?.CommandParameter is CompletedProject project)
        {
            await Navigation.PushAsync(new CompletedFurniturePage(project));
        }
    }

    private async void OnPhotosClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button?.CommandParameter is CompletedProject project)
        {
            //if (project.Images == null || !project.Images.Any())
            //{
            //    await DisplayAlert("Пусто", "В этом проекте нет сохраненных фотографий.", "OK");
            //    return;
            //}
            await Navigation.PushAsync(new CompletedPhotosPage(project));
        }
    }

    private async void OnObjectsClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button?.CommandParameter is CompletedProject project)
        {
            if (project.ProjectObjects == null || !project.ProjectObjects.Any())
            {
                await DisplayAlert("Внимание", "В этом проекте нет сохраненных объектов.", "OK");
                return;
            }
            // Переход на новую страницу объектов
            await Navigation.PushAsync(new CompletedObjectsPage(project));
        }
    }
}