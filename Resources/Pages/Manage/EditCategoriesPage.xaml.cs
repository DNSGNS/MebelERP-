using System.Collections.ObjectModel;

namespace MyApp1;

public partial class EditCategoriesPage : ContentPage
{
    private readonly ApiService _apiService;
    // Ссылка на тот же список, что и в ExpensesPage
    public ObservableCollection<ExpenseCategory> Categories { get; set; }

    public EditCategoriesPage(ObservableCollection<ExpenseCategory> categories)
    {
        InitializeComponent();
        _apiService = new ApiService();

        // Присваиваем переданную коллекцию
        Categories = categories;
        CategoriesList.ItemsSource = Categories;
    }

    // Добавление
    private async void OnAddClicked(object sender, EventArgs e)
    {
        string result = await DisplayPromptAsync("Новый тип", "Название затраты:", "Добавить", "Отмена");
        if (!string.IsNullOrWhiteSpace(result))
        {
            var newCat = await _apiService.CreateCategoryAsync(result);
            if (newCat != null)
                Categories.Add(newCat); // Обновится и здесь, и в выпадающем списке ExpensesPage
        }
    }

    // Редактирование
    private async void OnEditClicked(object sender, EventArgs e)
    {
        var cat = (sender as Button)?.CommandParameter as ExpenseCategory;
        if (cat == null) return;

        string result = await DisplayPromptAsync("Правка", "Новое название:", "Сохранить", "Отмена", initialValue: cat.Name);
        if (!string.IsNullOrWhiteSpace(result) && result != cat.Name)
        {
            string oldName = cat.Name;
            cat.Name = result;

            if (!await _apiService.UpdateCategoryAsync(cat))
            {
                cat.Name = oldName; // Откат если ошибка
                await DisplayAlert("Ошибка", "Не удалось сохранить", "OK");
            }
        }
    }

    // Удаление
    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        var cat = (sender as Button)?.CommandParameter as ExpenseCategory;
        if (cat == null) return;

        if (await DisplayAlert("Удаление", $"Удалить '{cat.Name}'?", "Да", "Нет"))
        {
            if (await _apiService.DeleteCategoryAsync(cat.Id))
                Categories.Remove(cat);
            else
                await DisplayAlert("Ошибка", "Тип используется в расходах и не может быть удален", "OK");
        }
    }
}