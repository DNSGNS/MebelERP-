using System;

namespace MyApp1;

public partial class AuthorizationPage : ContentPage
{
    private readonly ApiService _apiService = new ApiService();


    public AuthorizationPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;
        string username = UsernameEntry.Text?.Trim();
        string password = PasswordEntry.Text?.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowError("Введите логин и пароль");
            return;
        }

        try
        {
            SetLoading(true);
            var worker = await _apiService.LoginAsync(username, password);
            if (worker != null)
            {
                App.CurrentUser = worker;
                UserSessionService.SaveUser(worker);
                var window = Application.Current.Windows[0];
                ((App)Application.Current).LoadApp(window);
            }
            else
            {
                ShowError("Неверный логин или пароль");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Ошибка сети: {ex.Message}");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    private void SetLoading(bool isBusy)
    {
        LoadingOverlay.IsVisible = isBusy;
        LoginBtn.IsEnabled = !isBusy;
        UsernameEntry.IsEnabled = !isBusy;
        PasswordEntry.IsEnabled = !isBusy;
    }
}