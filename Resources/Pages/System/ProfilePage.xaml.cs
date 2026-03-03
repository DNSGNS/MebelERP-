namespace MyApp1;

public partial class ProfilePage : ContentPage
{
    private readonly ApiService _apiService = new ApiService();
    public ProfilePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Привязываем данные текущего пользователя к странице
        BindingContext = App.CurrentUser;
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Выход",
                                          "Вы уверены, что хотите выйти?",
                                          "Да",
                                          "Нет");

        if (!confirm)
            return;

        // 1. Очищаем текущего пользователя
        App.CurrentUser = null;

        // 2. Удаляем сохранённую сессию
        UserSessionService.ClearSession();

        // 3. Получаем текущее окно
        var window = Application.Current.Windows[0];

        // 4. Перезагружаем приложение
        ((App)Application.Current).LoadApp(window);
    }
    private void SetLoading(bool isBusy)
    {
        RefreshIndicator.IsVisible = isBusy;
        RefreshIndicator.IsRunning = isBusy;
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        try
        {
            SetLoading(true);

            // Берём текущий логин (или id если есть)
            var username = App.CurrentUser?.Username;
           

            if (string.IsNullOrEmpty(username))
            {
                await DisplayAlert("Ошибка", "Пользователь не найден", "OK");
                return;
            }

            // 🔹 ВАЖНО:
            // Лучше иметь отдельный метод типа GetUserByUsernameAsync
            // Но если нет — можно использовать LoginAsync повторно
            var updatedUser = await _apiService.GetUserByUsernameAsync(username);

            if (updatedUser != null)
            {
                // Обновляем глобального пользователя
                App.CurrentUser = updatedUser;

                // Перезаписываем файл сессии
                UserSessionService.SaveUser(updatedUser);

                // Обновляем BindingContext
                BindingContext = null;
                BindingContext = updatedUser;

                await DisplayAlert("Готово", "Данные обновлены", "OK");
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось обновить данные", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
        finally
        {
            SetLoading(false);
        }
    }
}