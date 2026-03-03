using System.Collections.ObjectModel;

namespace MyApp1;

public partial class MainCalculationPage : ContentPage
{
    // Свойство для индикатора загрузки
    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    // Привязываем свойство к глобальной коллекции в App
    public ObservableCollection<ProjectData> ProjectsList
    {
        get => App.AllProjects;
        set => App.AllProjects = value;
    }

    public MainCalculationPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    // --- КНОПКА ЗАГРУЗКИ ---
    private async void OnLoadAllClicked(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet("Выберите источник загрузки:", "Отмена", null, "Внутренняя память", "База данных (API)");
        if (action == "Отмена" || string.IsNullOrEmpty(action)) return;

        IsBusy = true;

        try
        {
            List<ProjectData> projectsToLoad = new();

            if (action == "Внутренняя память")
            {
                // --- ЛОГИКА ЗАГРУЗКИ ИЗ ФАЙЛА ---
                var loaded = await ProjectStorageService.LoadProjectsAsync();
                if (loaded == null)
                {
                    await DisplayAlert("Ошибка", "Не удалось прочитать локальный файл.", "OK");
                    return;
                }
                projectsToLoad = loaded.ToList();
            }
            else if (action == "База данных (API)")
            {
                // --- ЛОГИКА ЗАГРУЗКИ ИЗ БД ---
                var apiService = new ApiService();
                var apiProjects = await apiService.GetProjectsByCreatorAsync(App.CurrentUser.Name);

                if (apiProjects == null || apiProjects.Count == 0)
                {
                    await DisplayAlert("Инфо", "В БД проектов не найдено.", "OK");
                    return;
                }

                foreach (var apiProj in apiProjects)
                {
                    projectsToLoad.Add(ProjectMapper.MapToMaui(apiProj));
                }
            }

            // --- ОБЩАЯ ЧАСТЬ ОБНОВЛЕНИЯ ИНТЕРФЕЙСА ---
            if (projectsToLoad.Any())
            {
                App.AllProjects.Clear();

                foreach (var p in projectsToLoad)
                {
                    // Настраиваем автопересчет при изменении состава проекта
                    p.Objects.CollectionChanged += (s, ev) => p.RecalculateTotals();
                    p.RecalculateTotals();
                    App.AllProjects.Add(p);
                }
                IsBusy = false;
                await DisplayAlert("Успех", $"Загружено проектов: {projectsToLoad.Count} (Источник: {action})", "OK");
            }
            else
            {
                IsBusy = false;
                await DisplayAlert("Инфо", "Проектов не найдено.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка загрузки", $"Произошел сбой: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"LOAD ERROR: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // --- КНОПКА СОХРАНЕНИЯ ---
    private async void OnSaveAllClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Сохранение", "Сохранить проекты?", "Да", "Нет");
        if (!confirm) return;

        try
        {
            await ProjectStorageService.SaveProjectsAsync(App.AllProjects);
            await DisplayAlert("Успешно", "Все проекты сохранены в файл!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Ошибка при сохранении: {ex.Message}", "OK");
        }
    }

    // Добавление проекта
    private async void OnAddProjectClicked(object sender, EventArgs e)
    {
        string name = await DisplayPromptAsync("Новый проект", "Введите название проекта:", "ОК", "Отмена", "Моя Кухня");
        if (!string.IsNullOrWhiteSpace(name))
        {
            var newProject = new ProjectData
            {
                ProjectName = name,
                CreationDate = DateTime.Now // Сохраняем текущую дату
            };

            newProject.Objects.CollectionChanged += (s, ev) => newProject.RecalculateTotals();
            ProjectsList.Add(newProject);
        }
    }

    private async void OnProjectOptionsClicked(object sender, EventArgs e)
    {
        var project = (sender as Button)?.CommandParameter as ProjectData;
        if (project == null) return;

        // Формируем список доступных действий
        string action = await DisplayActionSheet($"Меню: {project.ProjectName}", "Отмена", null,
            "Переименовать", "Изменить дату", "Изменить цену");

        if (action == "Переименовать")
        {
            string newName = await DisplayPromptAsync("Переименовать", "Новое название проекта:", "ОК", "Отмена", project.ProjectName);
            if (!string.IsNullOrWhiteSpace(newName))
                project.ProjectName = newName;
        }
        else if (action == "Изменить дату")
        {
            // Упрощенный ввод даты через текст (ДД.ММ.ГГГГ)
            string currentDateStr = project.CreationDate.ToString("dd.MM.yyyy");
            string newDateStr = await DisplayPromptAsync("Дата проекта", "Введите дату (ДД.ММ.ГГГГ):", "ОК", "Отмена", currentDateStr, keyboard: Keyboard.Numeric);

            if (DateTime.TryParse(newDateStr, out DateTime parsedDate))
            {
                project.CreationDate = parsedDate;
            }
            else if (newDateStr != null) // Если не нажали Отмена, но ввели ерунду
            {
                await DisplayAlert("Ошибка", "Неверный формат даты", "ОК");
            }
        }
        else if (action == "Изменить цену")
        {
            // Проверка условия: менять цену можно только если нет объектов
            if (project.Objects.Count > 0)
            {
                await DisplayAlert("Ограничение", "Нельзя вручную менять цену, если в проекте есть объекты. Цена рассчитывается автоматически.", "ОК");
            }
            else
            {
                string priceStr = await DisplayPromptAsync("Цена проекта", "Введите итоговую стоимость:", "ОК", "Отмена", project.TotalProjectPrice.ToString("0"), keyboard: Keyboard.Numeric);
                if (decimal.TryParse(priceStr, out decimal newPrice))
                {
                    project.TotalProjectPrice = newPrice;
                }
            }
        }
    }

    // Удаление проекта
    private async void OnRemoveProjectClicked(object sender, EventArgs e)
    {
        var project = (sender as Button)?.CommandParameter as ProjectData;
        if (project != null)
        {
            bool confirm = await DisplayAlert("Удаление", $"Удалить проект \"{project.ProjectName}\"?", "Да", "Нет");
            if (confirm) ProjectsList.Remove(project);
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

    // MainCalculationPage.xaml.cs

    private void OnAddObjectClicked(object sender, EventArgs e)
    {
        // Получаем проект, в который добавляем объект
        var project = (sender as Button)?.CommandParameter as ProjectData;

        if (project != null)
        {
            // Создаем новый объект и СРАЗУ записываем в него имя проекта
            var newObject = new ObjectData
            {
                ObjectName = "Новый объект",
                ProjectName = project.ProjectName // <-- Записываем название проекта
            };

            // Добавляем объект в список проекта
            project.Objects.Add(newObject);

            // (Опционально) Если нужно сразу пересчитать итоги проекта
            project.RecalculateTotals();
        }
    }

    private void OnRemoveObjectClicked(object sender, EventArgs e)
    {
        var objectData = (sender as Button)?.CommandParameter as ObjectData;
        if (objectData == null) return;

        foreach (var project in ProjectsList)
        {
            if (project.Objects.Contains(objectData))
            {
                project.Objects.Remove(objectData);
                project.RecalculateTotals();
                break;
            }
        }
    }

    private async void OnObjectOptionsClicked(object sender, EventArgs e)
    {
        var obj = (sender as Button)?.CommandParameter as ObjectData;
        if (obj == null) return;

        // Показываем меню действий
        string action = await DisplayActionSheet($"Объект: {obj.ObjectName}", "Отмена", null,
            "Переименовать", "Загрузить фото", "Удалить фото");

        if (action == "Переименовать")
        {
            string newName = await DisplayPromptAsync("Переименовать", "Название объекта:", "ОК", "Отмена", obj.ObjectName);
            if (!string.IsNullOrWhiteSpace(newName))
                obj.ObjectName = newName;
        }
        else if (action == "Загрузить фото")
        {
            await PickAndSaveImageAsync(obj);
        }
        else if (action == "Удалить фото")
        {
            // Если нужно удалить картинку
            obj.ImagePath = null;
        }
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

    // Вспомогательный метод для выбора и сохранения фото
    private async Task PickAndSaveImageAsync(ObjectData obj)
    {
        try
        {
            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Выберите фото объекта"
            });

            if (result == null) return;

            // Генерируем новое имя файла
            var newFileName = $"{Guid.NewGuid()}{Path.GetExtension(result.FileName)}";
            var localPath = Path.Combine(FileSystem.AppDataDirectory, newFileName);

            // Копируем
            using (var stream = await result.OpenReadAsync())
            using (var newStream = File.OpenWrite(localPath))
            {
                await stream.CopyToAsync(newStream);
            }

            // Удаляем старое фото, если было
            if (!string.IsNullOrEmpty(obj.ImagePath) && File.Exists(obj.ImagePath))
            {
                try { File.Delete(obj.ImagePath); } catch { }
            }

            // ВАЖНО: Сначала сбрасываем путь, чтобы UI "дернулся", если вдруг файл тот же (маловероятно с GUID, но надежно)
            obj.ImagePath = null;

            // Присваиваем новый путь
            obj.ImagePath = localPath;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось загрузить фото: {ex.Message}", "ОК");
        }
    }
    private async void OnEditObjectClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var project = button?.CommandParameter as ProjectData;
        var objectData = button?.BindingContext as ObjectData;

        if (objectData != null && project != null)
            await Navigation.PushAsync(new ObjectEditorPage(objectData, project));
        //await Navigation.PushModalAsync(new ObjectEditorPage(objectData, project));
        //await Navigation.PushAsync(new LDSPPage(objectData, project));
    }

    private async void OnRecalculateClicked(object sender, EventArgs e)
    {
        //var project = (sender as Button)?.CommandParameter as ProjectData;
        //project?.RecalculateTotals();

        // 1. Извлекаем проект из параметра команды кнопки
        // Поскольку кнопка находится внутри шаблона списка, её CommandParameter привязан к конкретному проекту {Binding .}
        var currentProject = (sender as Button)?.CommandParameter as ProjectData;

        if (currentProject == null)
        {
            await DisplayAlert("Ошибка", "Не удалось определить данные проекта для отправки.", "OK");
            return;
        }

        bool confirm = await DisplayAlert("Отправка", $"Отправить проект \"{currentProject.ProjectName}\" в базу данных?", "Да", "Нет");
        if (!confirm) return;

        currentProject.IsUploading = true;

        try
        {
            // Показываем индикатор активности (визуально для пользователя)
            // Если у вас есть ActivityIndicator, можно включить его здесь

            // 2. Маппим данные
            var apiProject = ProjectMapper.MapToApi(currentProject);
            if (apiProject.ProjectObjects.Count <= 0)
            {
                apiProject.Status = ProjectStatus.Completed;
            }    
            // 3. Отправляем на сервер
            var apiService = new ApiService();
            bool isSuccess = await apiService.SaveProjectToDbAsync(apiProject);


            currentProject.IsUploading = false;
            if (isSuccess)
            {
                await DisplayAlert("Успех", "Проект успешно сохранен в облачной базе!", "OK");
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось соединиться с сервером или данные некорректны.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Критическая ошибка", ex.Message, "OK");
        }
        finally
        {
            // Выключаем загрузку для этой карточки
            currentProject.IsUploading = false;
        }
    }

    private async void OnCuttingClicked(object sender, EventArgs e)
    {
        var obj = (sender as Button)?.CommandParameter as ObjectData;
        if (obj == null) return;

        // 1. Показываем меню выбора типа материала
        string action = await DisplayActionSheet("Выберите страницу:", "Отмена", null,
            "Расчет Slim Line",
            "Раскрой корпус (ЛДСП)",
            "Раскрой фасады (AGT)",
            "Раскрой фасады (ЛДСП)");

        if (action == "Отмена" || string.IsNullOrEmpty(action)) return;

        if (action == "Расчет Slim Line")
            await Navigation.PushAsync(new DoorSlimLinePage(obj));

        CuttingData selectedCutting = null;

        // 2. В зависимости от выбора запускаем нужную синхронизацию и выбираем поле
        switch (action)
        {
            case "Раскрой корпус (ЛДСП)":
                obj.SyncLdspToCutting();
                selectedCutting = obj.CuttingLdsp;
                break;

            case "Раскрой фасады (AGT)":
                obj.SyncAgtToCutting();
                selectedCutting = obj.CuttingFAgt;
                break;

            case "Раскрой фасады (ЛДСП)":
                obj.SyncFLdspToCutting();
                selectedCutting = obj.CuttingFLdsp;
                break;
        }

        // 3. Переход на страницу
        if (selectedCutting != null)
        {
            selectedCutting.ProjectName = obj.ProjectName;
            selectedCutting.ObjectName = obj.ObjectName;

            if (selectedCutting.SavedReport != null)
            {
                // Если у CuttingSavedReportPage такой же конструктор, исправьте аналогично
                await Navigation.PushAsync(new CuttingSavedReportPage
                {
                    BindingContext = selectedCutting
                });
            }
            else
            {
                // ПЕРЕДАЕМ: (весь объект, выбранный тип раскроя)
                await Navigation.PushAsync(new CuttingPage(obj, selectedCutting));
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
    //            await DisplayAlert("Ошибка", "Отчета еще нет", "OK");
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
}