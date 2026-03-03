using MyApp1;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace MyApp1;

public partial class AddUserPage : ContentPage, INotifyPropertyChanged
{
    private WorkMans? _editingUser;

    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public ICommand ToggleCommand { get; }
    public List<WorkPosition> WorkPositions { get; set; }
    public WorkPosition SelectedPosition { get; set; }

    // Новое свойство для привязки чекбоксов
    public WorkManAccess Access { get; set; }

    public string ButtonText { get; set; } = "Сохранить сотрудника";

    public AddUserPage(WorkMans? userToEdit = null)
    {
        InitializeComponent();
        WorkPositions = Enum.GetValues(typeof(WorkPosition)).Cast<WorkPosition>().ToList();

        if (userToEdit != null)
        {
            _editingUser = userToEdit;
            Name = userToEdit.Name;
            Username = userToEdit.Username;
            Password = userToEdit.Password;
            SelectedPosition = userToEdit.Position;

            // Если у пользователя уже есть права, используем их, иначе создаем новые
            Access = userToEdit.Access ?? new WorkManAccess { WorkManId = userToEdit.Id };

            ButtonText = "Сохранить изменения";
            Title = "Редактирование";
        }
        else
        {
            SelectedPosition = WorkPosition.Installer;
            // Для нового пользователя создаем пустой объект прав
            Access = new WorkManAccess();
            Title = "Новый сотрудник";
        }

        ToggleCommand = new Command<string>(prop =>
        {
            var pi = typeof(WorkManAccess).GetProperty(prop);
            if (pi == null) return;
            var current = (bool)(pi.GetValue(Access) ?? false);
            pi.SetValue(Access, !current);
            // Уведомляем UI
            OnPropertyChanged(nameof(Access));
        });

        BindingContext = this;
    }

    private async void OnSaveUserClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            await DisplayAlert("Ошибка", "Заполните все поля", "OK");
            return;
        }

        var apiService = new ApiService();
        bool success;

        if (_editingUser == null)
        {
            var newUser = new WorkMans
            {
                Id = Guid.NewGuid(),
                Name = this.Name,
                Position = this.SelectedPosition,
                Username = this.Username,
                Password = this.Password,
                // Прикрепляем права к новому пользователю
                Access = this.Access
            };
            newUser.Access.WorkManId = newUser.Id; // Важно проставить ID связи
            success = await apiService.CreateUserAsync(newUser);
        }
        else
        {
            _editingUser.Name = this.Name;
            _editingUser.Position = this.SelectedPosition;
            _editingUser.Username = this.Username;
            _editingUser.Password = this.Password;

            // Обновляем права в редактируемом пользователе
            _editingUser.Access = this.Access;

            success = await apiService.UpdateUserAsync(_editingUser);
        }

        if (success)
        {
            await DisplayAlert("Успех", "Данные сохранены!", "OK");
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Ошибка", "Не удалось сохранить. Возможно, логин занят или нет связи.", "OK");
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}