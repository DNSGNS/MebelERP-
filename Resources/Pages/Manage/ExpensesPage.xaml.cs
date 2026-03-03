using MyApp1;
namespace MyApp1;

public partial class ExpensesPage : ContentPage
{
    private ExpensesForm _viewModel;
    private readonly ApiService _apiService;

    // Флаг несохранённых изменений
    private bool _isDirty = false;
    private bool _isPageReady = false;

    public ExpensesPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
        _viewModel = new ExpensesForm();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDataAsync();
    }

    protected override bool OnBackButtonPressed()
    {
        Dispatcher.Dispatch(async () => await CheckAndGoBack());
        return true;
    }

    private async Task CheckAndGoBack()
    {
        await AutoSaveAndNavigateAsync(async () => await Navigation.PopAsync());
    }

    private async Task LoadDataAsync()
    {
        _isPageReady = false; // Блокируем фиксацию изменений
        _isDirty = false;
        LoadingOverlay.IsVisible = true;

        var dto = await _apiService.GetExpensesInitialDataAsync();

        if (dto != null)
        {
            _viewModel = ProjectMapper.MapToExpensesForm(dto);

            // --- НАСТРОЙКА ДИНАМИЧЕСКИХ ГОДОВ ---
            var currentYear = DateTime.Now.Year;
            _viewModel.Years.Clear();
            _viewModel.Years.Add("Все");

            // Добавляем года: от (текущий + 1) вниз до 2023 (или любого года начала учета)
            // Это позволит всегда видеть следующий год в списке заранее
            for (int y = currentYear + 1; y >= 2025; y--)
            {
                _viewModel.Years.Add(y.ToString());
            }

            // Сохраняем все данные в RawExpenses и применяем фильтр "Все время"
            _viewModel.RawExpenses = _viewModel.AllExpenses.ToList();
            _viewModel.ApplyFilter();

            BindingContext = _viewModel;
        }
        LoadingOverlay.IsVisible = false;

        await Task.Delay(100);

        _isPageReady = true;
        _isDirty = false;
        // dto = await _apiService.GetExpensesInitialDataAsync();
        //if (dto != null)
        //{
        //    _viewModel = ProjectMapper.MapToExpensesForm(dto);
        //    BindingContext = _viewModel;
        //}
        //else
        //{
        //    await DisplayAlert("Ошибка", "Не удалось загрузить данные", "OK");
        //}

        //LoadingOverlay.IsVisible = false;
    }

    // Срабатывает при изменении любого свойства записи (сумма, дата, категория)
    private void OnExpensePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        _isDirty = true;
    }

    private void OnAddRowClicked(object sender, EventArgs e)
    {
        var newExpense = new Expense { Date = DateTime.Now, Amount = 0 };
        _viewModel.RawExpenses.Insert(0, newExpense);
        _viewModel.ApplyFilter(); // Обновит AllExpenses с учетом текущих фильтров

        if (_isPageReady) _isDirty = true;
    }

    private void OnAmountTextChanged(object sender, TextChangedEventArgs e)
    {
        // Вызываем метод пересчета суммы во ViewModel
        _viewModel?.UpdateTotal();

        if (_isPageReady && sender is Entry entry && entry.IsFocused)
        {
            _isDirty = true;
        }
    }

    private void OnManualChanged(object sender, EventArgs e)
    {
        if (_isPageReady)
        {
            _isDirty = true;
        }
    }

    private void OnEntryFocused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(50);
                entry.CursorPosition = 0;
                entry.SelectionLength = entry.Text?.Length ?? 0;
            });
        }
    }

    private async void OnDeleteRowClicked(object sender, EventArgs e)
    {
        // Получаем кнопку, которая вызвала событие
        if (sender is Button button && button.CommandParameter is Expense expenseToDelete)
        {
            bool confirm = await DisplayAlert("Подтверждение",
                $"Удалить запись на сумму {expenseToDelete.Amount:N0} ₽?", "Да", "Нет");

            if (!confirm) return;

            LoadingOverlay.IsVisible = true;

            bool success = true;

            // Если у записи есть ID > 0, значит она есть в БД и её надо удалить через API
            if (expenseToDelete.Id > 0)
            {
                success = await _apiService.DeleteExpenseAsync(expenseToDelete.Id);
            }

            LoadingOverlay.IsVisible = false;

            if (success)
            {
                // Удаляем из локальной коллекции (UI обновится автоматически)
                _viewModel.AllExpenses.Remove(expenseToDelete);
                // Пересчитываем общую сумму
                _viewModel.UpdateTotal();
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось удалить запись", "OK");
            }
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (!_isDirty)
        {
            await DisplayAlert("Инфо", "Изменений нет", "OK");
            return;
        }
        await AutoSaveAndNavigateAsync(() => Task.CompletedTask);
    }

    private async void OnEditCategoriesClicked(object sender, EventArgs e)
    {
        await AutoSaveAndNavigateAsync(async () =>
            await Navigation.PushAsync(new EditCategoriesPage(_viewModel.Categories)));
    }

    private async void OnChartsClicked(object sender, EventArgs e)
    {
        if (_viewModel == null || !_viewModel.AllExpenses.Any())
        {
            await DisplayAlert("Инфо", "Нет данных для построения графика", "OK");
            return;
        }

        await AutoSaveAndNavigateAsync(async () =>
            await Navigation.PushAsync(new ChartsPage(_viewModel)));
    }

    private async Task AutoSaveAndNavigateAsync(Func<Task> navigateAction)
    {
        if (!_isDirty || !_isPageReady)
        {
            await navigateAction();
            return;
        }

        try
        {
            LoadingOverlay.IsVisible = true;

            var expensesToSave = _viewModel.AllExpenses.ToList();
            int successCount = 0;

            foreach (var expense in expensesToSave)
            {
                if (expense.Amount <= 0 || expense.Category == null)
                    continue;

                var apiModel = ProjectMapper.MapToApiExpense(expense);
                var savedExpense = await _apiService.SaveExpenseAsync(apiModel);

                if (savedExpense != null)
                {
                    if (expense.Id == 0)
                        expense.Id = savedExpense.Id;

                    successCount++;
                }
            }
            _isDirty = false;
            _viewModel.UpdateTotal();

            //if (successCount > 0)
            //    await DisplayAlert("Автосохранение", $"Сохранено записей: {successCount}", "OK");
        }
        catch (Exception)
        {
            await DisplayAlert("Ошибка", "Не удалось сохранить данные перед переходом", "OK");
        }
        finally
        {
            LoadingOverlay.IsVisible = false;
        }

        await navigateAction();
    }

    private Task AutoSaveAndNavigateAsync(Func<Task> navigateAction, bool dummy = false)
=> AutoSaveAndNavigateAsync(navigateAction);



}