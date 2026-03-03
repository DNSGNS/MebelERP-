using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyApp1;

public partial class MainCalculationWarehousePage : ContentPage, INotifyPropertyChanged
{
    private readonly ApiService _apiService = new ApiService();
    private bool _isLoaded = false;
    private bool _isUpdating = false;
    public bool IsUpdating
    {
        get => _isUpdating;
        set { _isUpdating = value; OnPropertyChanged(); }
    }
    // Привязываем свойство к глобальной коллекции в App
    public ObservableCollection<ProjectData> ProjectsList
    {
        get => App.AllProjects;
        set => App.AllProjects = value;
    }

    public MainCalculationWarehousePage()
    {
        InitializeComponent();
        BindingContext = this;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Загружаем данные при первом открытии
        if (!_isLoaded)
        {
            await LoadProjectsFromApi();
            _isLoaded = true;
        }
    }

    private async void OnLoadAllClicked(object sender, EventArgs e)
    {
        await LoadProjectsFromApi();
    }

    // --- КНОПКА ЗАГРУЗКИ ---
    private async Task LoadProjectsFromApi()
    {
        try
        {
            IsUpdating = true; // Используем свойство!

            var apiProjects = await _apiService.GetNewProjectsAsync();
            if (apiProjects != null)
            {
                ProjectsList.Clear();
                foreach (var apiProj in apiProjects)
                {
                    var mauiProj = ProjectMapper.MapToMaui(apiProj);
                    ProjectsList.Add(mauiProj);
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
        finally
        {
            IsUpdating = false; // Выключаем индикатор
        }
    }

    private async void OnRenameProjectClicked(object sender, EventArgs e)
    {
        var project = (sender as Button)?.CommandParameter as ProjectData;
        if (project == null) return;

        string newName = await DisplayPromptAsync("Переименовать", "Новое название проекта:", "ОК", "Отмена", project.ProjectName);
        if (!string.IsNullOrWhiteSpace(newName)) project.ProjectName = newName;
    }

    private void OnProjectTapped(object sender, TappedEventArgs e)
    {
        var project = (sender as Frame)?.BindingContext as ProjectData;
        if (project != null) project.IsExpanded = !project.IsExpanded;
    }

    private async void OnImageTapped(object sender, TappedEventArgs e)
    {
        // В MAUI CommandParameter передается через e.Parameter
        var imagePath = e.Parameter as string;

        if (!string.IsNullOrEmpty(imagePath))
        {
            // Открываем модальное окно с картинкой
            await Navigation.PushModalAsync(new ImagePreviewPage(imagePath));
        }
    }

    private async void OnEditObjectClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var project = button?.CommandParameter as ProjectData;
        var objectData = button?.BindingContext as ObjectData;

        if (objectData != null && project != null)
            await Navigation.PushAsync(new ObjectWarehouseEditorPage(objectData, project));
    }

    //private async void OnRecalculateClicked(object sender, EventArgs e)
    //{
    //    var button = sender as Button;
    //    var project = button?.BindingContext as ProjectData;

    //    if (project == null) return;

    //    // Проверка галочек
    //    if (!project.IsMaterialReady || !project.IsFurnitureReady)
    //    {
    //        await DisplayAlert("Внимание", "Необходимо отметить готовность материалов и фурнитуры!", "OK");
    //        return;
    //    }

    //    try
    //    {
    //        var apiService = new ApiService();

    //        // 1. Обновляем основной статус проекта в БД (перевод в InProgress)
    //        bool statusSuccess = await apiService.UpdateProjectStatusAsync(project.Id, (int)project.Status);

    //        if (statusSuccess)
    //        {
    //            // 2. Если статус обновился, вызываем создание заданий для цеха
    //            bool tasksSuccess = await apiService.GenerateProductionTasksAsync(project.Id);

    //            if (tasksSuccess)
    //            {
    //                await DisplayAlert("Успех", 
    //                    $"Проект '{project.ProjectName}' запущен в работу. Задания для цеха (раскрой, МДФ, двери) успешно созданы.", 
    //                    "OK");
    //            }
    //            else
    //            {
    //                // Ошибка на этапе создания задач
    //                await DisplayAlert("Частичный успех", 
    //                    "Статус проекта изменен, но не удалось создать производственные задания. Проверьте соединение или обратитесь к администратору.", 
    //                    "OK");
    //            }
    //        }
    //        else
    //        {
    //            await DisplayAlert("Ошибка", "Не удалось обновить статус проекта в базе данных", "OK");
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        await DisplayAlert("Ошибка сети", ex.Message, "OK");
    //    }
    //}

    // Обработчик чекбоксов (Срабатывает сразу при нажатии)

    private async void OnWarehouseStatusChanged(object sender, CheckedChangedEventArgs e)
    {
        if (IsUpdating) return;

        var checkbox = sender as CheckBox;
        var project = checkbox?.BindingContext as ProjectData;

        if (project != null)
        {
            try
            {
                IsUpdating = true; // Показываем загрузку при смене статуса
                var newStatusInt = await _apiService.UpdateWarehouseStatusAsync(
                    project.Id, project.IsMaterialReady, project.IsFurnitureReady);

                if (newStatusInt.HasValue)
                {
                    project.Status = (ProjectStatus)newStatusInt.Value;
                    if (project.Status == ProjectStatus.InProgress)
                    {
                        bool tasksSuccess = await _apiService.GenerateProductionTasksAsync(project.Id);
                        if (tasksSuccess)
                        {
                            await DisplayAlert("Успех", $"Проект '{project.ProjectName}' запущен в работу.", "OK");
                        }
                    }
                }
            }
            finally
            {
                IsUpdating = false;
            }
        }
    }

    //Метод открывает страницу CuttingSavedReportPage
    private async void OnCuttingClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var objectData = button?.CommandParameter as ObjectData;

        if (objectData == null) return;

        // 1. Показываем меню выбора типа материала
        string action = await DisplayActionSheet("Выберите страницу:", "Отмена", null,
            "Расчет Slim Line",
            "Раскрой корпус (ЛДСП)",
            "Раскрой фасады (AGT)",
            "Раскрой фасады (ЛДСП)");

        if (action == "Отмена" || string.IsNullOrEmpty(action)) return;

        if (action == "Расчет Slim Line")
            await Navigation.PushAsync(new DoorSlimLineReadPage(objectData));

        CuttingData selectedCutting = null;

        // 2. В зависимости от выбора запускаем синхронизацию и выбираем нужный объект CuttingData
        switch (action)
        {
            case "Раскрой корпус (ЛДСП)":
                objectData.SyncLdspToCutting();
                selectedCutting = objectData.CuttingLdsp;
                break;

            case "Раскрой фасады (AGT)":
                objectData.SyncAgtToCutting();
                selectedCutting = objectData.CuttingFAgt;
                break;

            case "Раскрой фасады (ЛДСП)":
                objectData.SyncFLdspToCutting();
                selectedCutting = objectData.CuttingFLdsp;
                break;
        }

        // 3. Логика проверки и перехода (как в старом методе)
        if (selectedCutting != null)
        {
            // Прокидываем названия для заголовков отчета
            selectedCutting.ProjectName = objectData.ProjectName;
            selectedCutting.ObjectName = objectData.ObjectName;

            // Если отчет уже был сохранен ранее, открываем просмотр архива
            if (selectedCutting.SavedReport != null)
            {
                await Navigation.PushAsync(new CuttingSavedReportPage
                {
                    BindingContext = selectedCutting
                });
            }
            else
            {
                // Если отчета нет, выводим предупреждение (логика старого метода)
                await DisplayAlert("Внимание", $"Отчет для типа '{action}' еще не сформирован", "OK");
            }
        }
    }

    ////Метод открывает страницу CuttingSavedReportPage
    //private async void OnCuttingClicked(object sender, EventArgs e)
    //{
    //    var button = sender as Button;
    //    var objectData = button?.CommandParameter as ObjectData;

    //    if (objectData != null)
    //    {
    //        // 1. Записываем названия проекта и объекта в данные раскроя
    //        objectData.Cutting.ProjectName = objectData.ProjectName;
    //        objectData.Cutting.ObjectName = objectData.ObjectName;

    //        // 2. Синхронизируем данные из ЛДСП в Раскрой
    //        objectData.SyncLdspToCutting();

    //        // 3. ЛОГИКА ПЕРЕХОДА:
    //        // Если отчет уже был сохранен ранее (есть данные в SavedReport), 
    //        // открываем страницу просмотра архива.
    //        if (objectData.Cutting.SavedReport != null)
    //        {
    //            await Navigation.PushAsync(new CuttingSavedReportPage
    //            {
    //                BindingContext = objectData.Cutting
    //            });
    //        }
    //        else
    //        {
    //            // Если отчета еще нет, открываем стандартную страницу редактора/расчета
    //            await Navigation.PushAsync(new CuttingPage(objectData));
    //        }
    //    }
    //}

    private void OnObjectTapped(object sender, TappedEventArgs e)
    {
        var obj = (sender as Frame)?.BindingContext as ObjectData;
        if (obj != null)
        {
            obj.IsExpanded = !obj.IsExpanded;
        }
    }

    private async void OnReportClicked(object sender, EventArgs e)
    {
        var project = (sender as Button)?.CommandParameter as ProjectData;
        if (project != null)
            await Navigation.PushAsync(new ProjectReportPage(project));
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}