using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;

namespace MyApp1;

public partial class ManageMainPage : ContentPage
{
    private InstallerForm _viewModel;
    private bool _isDataChanged = false;
    private ProjectManageData _currentProjectForDate;

    public ICommand BackCommand => new Command(async () => await CheckAndGoBack());

    private async Task CheckAndGoBack()
    {
        if (!_isDataChanged)
        {
            await Navigation.PopAsync();
            return;
        }

        try
        {
            _viewModel.IsBusy = true; // Блокируем экран при сохранении
            var apiService = new ApiService();
            int successCount = 0;

            foreach (var project in _viewModel.Projects)
            {
                bool isSaved = await apiService.SaveInstallationPlanAsync(project);
                if (isSaved) successCount++;
            }

            _isDataChanged = false;
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", "Не удалось сохранить данные при выходе", "OK");
        }
        finally
        {
            _viewModel.IsBusy = false;
        }
    }

    protected override bool OnBackButtonPressed()
    {
        Dispatcher.Dispatch(async () => await CheckAndGoBack());
        return true;
    }

    public ManageMainPage()
    {
        InitializeComponent();
        _viewModel = new InstallerForm();
        BindingContext = _viewModel;

        LoadDataAsync();
    }

    private async void LoadDataAsync()
    {
        try
        {
            _viewModel.IsBusy = true; // Показываем загрузку

            var apiService = new ApiService();
            var apiData = await apiService.GetInstallationPlanningDataAsync();

            if (apiData != null)
            {
                var loadedForm = ProjectMapper.MapToInstallerForm(apiData);
                _viewModel.Projects = loadedForm.Projects;
                _viewModel.AllAvailableInstallers = loadedForm.AllAvailableInstallers;
                _viewModel.DistributedInstallers = loadedForm.DistributedInstallers;

                foreach (var project in _viewModel.Projects)
                    project.PropertyChanged += OnProjectPropertyChanged;

                _isDataChanged = false;
            }
        }
        finally
        {
            _viewModel.IsBusy = false; // Скрываем загрузку в любом случае
        }
    }

    private async void OnOpenCalendarClicked(object sender, EventArgs e)
    {
        // Проверяем, есть ли данные в списке
        if (_viewModel.Projects == null || _viewModel.Projects.Count == 0)
        {
            await DisplayAlert("Внимание", "Нет данных для отображения в календаре", "OK");
            return;
        }

        // Переходим на страницу календаря, передавая список проектов
        // Используем .ToList(), так как конструктор CalendarPage ожидает List<ProjectManageData>
        await Navigation.PushAsync(new ManageCalendarPage(_viewModel.Projects.ToList()));
    }

    // Для даты: программно открываем календарь при нажатии на текст
    private void OnDateLabelTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is DatePicker picker)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DateTime? checkDate = picker.Date;
                if (checkDate?.Year < 2020) picker.Date = DateTime.Now;
                picker.Focus();
                // Изменение даты через Binding вызовет PropertyChanged -> _isDataChanged = true
            });
        }
    }

    // Для установщиков: меняем Clicked на Tapped
    private void OnSelectInstallersTapped(object sender, TappedEventArgs e)
    {
        // Так как параметр теперь передается через TappedEventArgs
        var project = (ProjectManageData)e.Parameter;

        // Вызываем ваш существующий метод логики выбора (при необходимости передаем sender и проект)
        OpenInstallerMenu(project, sender);
    }

    // Вынесите логику из старого OnSelectInstallersClicked в этот метод
    private async void OpenInstallerMenu(ProjectManageData project, object sender)
    {
        var options = _viewModel.AllAvailableInstallers.Select(w =>
        {
            bool isAssigned = project.AssignedInstallers.Any(a => a.Id == w.Id);
            return isAssigned ? $"[ВЫБРАН] {w.Name}" : w.Name;
        }).ToList();

        string selected = await DisplayActionSheet("Управление установщиками", "Готово", null, options.ToArray());

        if (selected != "Готово" && !string.IsNullOrEmpty(selected))
        {
            string cleanName = selected.Replace("[ВЫБРАН] ", "");
            var worker = _viewModel.AllAvailableInstallers.First(w => w.Name == cleanName);

            if (project.AssignedInstallers.Any(w => w.Id == worker.Id))
            {
                var toRemove = project.AssignedInstallers.First(w => w.Id == worker.Id);
                project.AssignedInstallers.Remove(toRemove);
            }
            else
            {
                project.AssignedInstallers.Add(worker);
                if (!_viewModel.DistributedInstallers.Any(w => w.Id == worker.Id))
                    _viewModel.DistributedInstallers.Add(worker);
            }

            project.OnPropertyChanged(nameof(project.InstallersDisplay));

            // Повторный вызов для мультивыбора
            OpenInstallerMenu(project, sender);
        }
    }
    private async void OnSaveAllClicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Подтверждение", "Сохранить изменения?", "Да", "Нет");
        if (!answer) return;

        try
        {
            _viewModel.IsBusy = true;
            var apiService = new ApiService();
            int successCount = 0;

            foreach (var project in _viewModel.Projects)
            {
                bool isSaved = await apiService.SaveInstallationPlanAsync(project);
                if (isSaved) successCount++;
            }

            _isDataChanged = false;
            await DisplayAlert("Готово", "Данные сохранены", "OK");
            LoadDataAsync();
        }
        finally
        {
            _viewModel.IsBusy = false;
        }
    }

    private async void OnTransferToCompletedClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var project = button?.CommandParameter as ProjectManageData;
        if (project == null) return;

        // 1. Проверка наличия дат установки
        // Если список дат пуст или коллекция не инициализирована
        if (project.InstallDates == null || project.InstallDates.Count == 0)
        {
            await DisplayAlert("Внимание", "Невозможно завершить проект: не указаны даты установки.", "OK");
            return;
        }

        bool confirm = await DisplayAlert("Завершение", $"Архивировать {project.ProjectName}?", "Да", "Отмена");
        if (!confirm) return;

        project.IsProcessing = true;
        try
        {
            var apiService = new ApiService();

            // 1. Извлекаем список имен назначенных установщиков
            var installers = project.AssignedInstallers?.Select(x => x.Name).ToList() ?? new List<string>();

            // 2. ИСПРАВЛЕНИЕ: Используем InstallDates.ToList() вместо несуществующего InstalTime
            // Если список дат пуст, передаем текущую дату как дефолтную
            var dates = project.InstallDates.ToList();

            // Вызываем обновленный метод API (который принимает List<DateTime>)
            bool isSuccess = await apiService.CompleteProjectAsync(project.Id, installers, dates);
            if (isSuccess)
            {
                _isDataChanged = false;

                project.IsProcessing = false;
                await DisplayAlert("Готово", "Проект архивирован", "OK");
                // Обновляем список на странице
                LoadDataAsync();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Произошла ошибка при отправке данных: {ex.Message}", "OK");
        }
        finally
        {
            // 3. Выключаем анимацию загрузки в любом случае
            project.IsProcessing = false;
        }
    }
    private async void OnDeleteProjectClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var project = (ProjectManageData)button.CommandParameter;

        // Очень важное предупреждение, так как это удаление из базы данных
        bool confirm = await DisplayAlert("УДАЛЕНИЕ",
            $"Вы уверены, что хотите ПОЛНОСТЬЮ УДАЛИТЬ проект '{project.ProjectName}' и все связанные с ним расчеты? Это действие необратимо.",
            "УДАЛИТЬ", "Отмена");

        if (!confirm) return;

        try
        {
            _viewModel.IsBusy = true;
            var apiService = new ApiService();

            bool isSuccess = await apiService.DeleteProjectAsync(project.Id);

            if (isSuccess)
            {
                _isDataChanged = false; // Сбрасываем флаг, так как данные обновим сейчас
                await DisplayAlert("Готово", "Проект успешно удален", "OK");
                LoadDataAsync(); // Перезагружаем список
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось удалить проект. Возможно, есть связи, препятствующие удалению.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
        }
        finally
        {
            _viewModel.IsBusy = false;
        }
    }

// 1. Метод при нажатии на текст дат в строке
private async void OnDatesMenuTapped(object sender, TappedEventArgs e)
{
    if (sender is Label label && label.BindingContext is ProjectManageData project)
    {
        await ShowDatesMenu(project);
    }
}

    // 2. Показ меню управления
    private async Task ShowDatesMenu(ProjectManageData project)
    {
        var options = new List<string> { "➕ Добавить дату" };
        if (project.InstallDates != null && project.InstallDates.Any())
        {
            // Сортируем для красоты в меню
            options.AddRange(project.InstallDates.OrderBy(d => d).Select(d => $"❌ Удалить {d:dd.MM.yyyy}"));
        }

        string selected = await DisplayActionSheet("Управление датами", "Отмена", null, options.ToArray());

        if (selected == "➕ Добавить дату")
        {
            _currentProjectForDate = project;
            GlobalDatePicker.DateSelected -= OnGlobalDateSelected;
            GlobalDatePicker.Date = DateTime.Today;
            GlobalDatePicker.DateSelected += OnGlobalDateSelected;

            DatePickerOverlay.IsVisible = true;
            await Task.Delay(150);
            MainThread.BeginInvokeOnMainThread(() => GlobalDatePicker.Focus());
        }
        else if (!string.IsNullOrEmpty(selected) && selected.StartsWith("❌ Удалить "))
        {
            string dateStr = selected.Replace("❌ Удалить ", "");

            if (DateTime.TryParseExact(dateStr, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime clickedDate))
            {
                // ИСПРАВЛЕНИЕ ТУТ:
                // Ищем в коллекции дату, которая совпадает по календарному дню
                var dateInList = project.InstallDates.FirstOrDefault(d =>
                    d.Year == clickedDate.Year &&
                    d.Month == clickedDate.Month &&
                    d.Day == clickedDate.Day);

                if (dateInList != default)
                {
                    project.InstallDates.Remove(dateInList); // Удаляем найденный объект
                    project.OnPropertyChanged(nameof(project.DatesDisplay));
                    _isDataChanged = true;
                }
            }
        }
    }



    private void OnDatePickerCancelClicked(object sender, EventArgs e)
    {
        DatePickerOverlay.IsVisible = false;
        _currentProjectForDate = null;
    }










    // 3. Обработка выбора в ГЛОБАЛЬНОМ пикере
    private void OnGlobalDateSelected(object sender, DateChangedEventArgs e)
    {
        if (e.NewDate.HasValue && e.NewDate.Value.Year < 2010) return;

        if (_currentProjectForDate != null && e.NewDate.HasValue)
        {
            DateTime selectedDate = e.NewDate.Value.Date;

            if (_currentProjectForDate.InstallDates == null)
                _currentProjectForDate.InstallDates = new ObservableCollection<DateTime>();

            if (!_currentProjectForDate.InstallDates.Contains(selectedDate))
            {
                _currentProjectForDate.InstallDates.Add(selectedDate);
                _currentProjectForDate.OnPropertyChanged(nameof(ProjectManageData.DatesDisplay));
                _isDataChanged = true;
            }
        }

        // Скрываем оверлей
        DatePickerOverlay.IsVisible = false;

        GlobalDatePicker.DateSelected -= OnGlobalDateSelected;
        GlobalDatePicker.Date = new DateTime(1900, 1, 1);
        GlobalDatePicker.DateSelected += OnGlobalDateSelected;

        _currentProjectForDate = null;
    }

    private void OnProjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Игнорируем технические свойства, если нужно, или просто ловим всё
        _isDataChanged = true;
    }

    //private async void OnSelectInstallersClicked(object sender, EventArgs e)
    //{
    //    var button = (Button)sender;
    //    var project = (ProjectManageData)button.CommandParameter;

    //    // Формируем список имен. 
    //    // Если человек уже выбран, добавим пометку [V] или "УДАЛИТЬ"
    //    var options = _viewModel.AllAvailableInstallers.Select(w =>
    //    {
    //        bool isAssigned = project.AssignedInstallers.Any(a => a.Id == w.Id);
    //        return isAssigned ? $"[ВЫБРАН] {w.Name}" : w.Name;
    //    }).ToList();

    //    string selected = await DisplayActionSheet("Управление установщиками", "Готово", null, options.ToArray());

    //    if (selected != "Готово" && !string.IsNullOrEmpty(selected))
    //    {
    //        // Очищаем имя от пометки, чтобы найти рабочего
    //        string cleanName = selected.Replace("[ВЫБРАН] ", "");
    //        var worker = _viewModel.AllAvailableInstallers.First(w => w.Name == cleanName);

    //        bool alreadyExists = project.AssignedInstallers.Any(w => w.Id == worker.Id);

    //        if (alreadyExists)
    //        {
    //            // Если нажали на уже выбранного — удаляем
    //            var toRemove = project.AssignedInstallers.First(w => w.Id == worker.Id);
    //            project.AssignedInstallers.Remove(toRemove);
    //        }
    //        else
    //        {
    //            // Если новый — добавляем
    //            project.AssignedInstallers.Add(worker);

    //            // Также добавляем в общий список распределенных, если его там нет
    //            if (!_viewModel.DistributedInstallers.Any(w => w.Id == worker.Id))
    //                _viewModel.DistributedInstallers.Add(worker);
    //        }

    //        // Обновляем текст на кнопке
    //        project.OnPropertyChanged(nameof(project.InstallersDisplay));

    //        // РЕКУРСИЯ: Снова вызываем это же меню, чтобы пользователь мог выбрать еще кого-то
    //        // Меню не закроется "навсегда", а просто переоткроется сразу
    //        OnSelectInstallersClicked(sender, e);
    //    }
    //}
}