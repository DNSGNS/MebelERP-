using System.Collections.ObjectModel;

namespace MyApp1;

public partial class UsersListPage : ContentPage
{
    private readonly ApiService _apiService;
    private bool _isBusy;

    public ObservableCollection<WorkMans> Users { get; set; } = new();

    // Свойство для индикатора загрузки
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
        }
    }

    public UsersListPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Загружаем данные каждый раз при появлении страницы
        // (чтобы увидеть нового пользователя после возврата со страницы добавления)
        await LoadUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        Users.Clear();

        var usersList = await _apiService.GetUsersAsync();

        foreach (var user in usersList)
        {
            Users.Add(user);
        }

        IsBusy = false;
    }

    private async void OnAddUserClicked(object sender, EventArgs e)
    {
        // Переход на вашу страницу добавления
        await Navigation.PushAsync(new AddUserPage());
    }

    private async void OnEditUserTapped(object sender, TappedEventArgs e)
    {
        var user = e.Parameter as WorkMans;
        if (user == null) return;

        // Переходим на страницу, передавая выбранного пользователя
        await Navigation.PushAsync(new AddUserPage(user));
    }
    private async void OnDeleteUserTapped(object sender, TappedEventArgs e)
    {
        var user = e.Parameter as WorkMans;
        if (user == null) return;

        bool confirm = await DisplayAlert("Удаление", $"Вы уверены, что хотите удалить сотрудника {user.Name}?", "Да", "Нет");
        if (confirm)
        {
            IsBusy = true;
            bool success = await _apiService.DeleteUserAsync(user.Id);

            if (success)
            {
                Users.Remove(user);
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось удалить пользователя", "OK");
            }
            IsBusy = false;
        }
    }
}