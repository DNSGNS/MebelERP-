using MyApp1.Resources.Other;

namespace MyApp1;

public partial class ObjectEditorPage : ContentPage
{
    public ObjectData CurrentObject { get; }
    public ProjectData ParentProject { get; }

    public ObjectEditorPage(ObjectData obj, ProjectData project)
    {
        InitializeComponent();
        CurrentObject = obj;
        ParentProject = project;

        // Показываем первую вкладку по умолчанию
        SwitchToTab("LDSP");
    }

    private void OnTabClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is string tabName)
        {
            SwitchToTab(tabName);
        }
    }

    private void SwitchToTab(string tabName)
    {
        // Снимаем выделение со всех кнопок
        btnLDSP.BackgroundColor = Colors.Transparent;
        btnFasady.BackgroundColor = Colors.Transparent;
        btnDveri.BackgroundColor = Colors.Transparent;
        btnMyagkaya.BackgroundColor = Colors.Transparent;
        btnFurnitura.BackgroundColor = Colors.Transparent;
        btnOtchet.BackgroundColor = Colors.Transparent;


        // Выделяем активную
        switch (tabName)
        {
            case "LDSP":     btnLDSP.BackgroundColor = Color.FromArgb("#6750A4"); break;
            case "Fasady":   btnFasady.BackgroundColor = Color.FromArgb("#6750A4"); break;
            case "Dveri":    btnDveri.BackgroundColor = Color.FromArgb("#6750A4"); break;
            case "Myagkaya": btnMyagkaya.BackgroundColor = Color.FromArgb("#6750A4"); break;
            case "Furnitura":btnFurnitura.BackgroundColor = Color.FromArgb("#6750A4"); break;
            case "Otchet":   btnOtchet.BackgroundColor = Color.FromArgb("#6750A4"); break;
        }

        // Меняем содержимое
        ContentView newContent = tabName switch
        {
            "LDSP"     => new LDSPContentView     { BindingContext = CurrentObject },
            "Fasady"   => new FasadContentView    { BindingContext = CurrentObject },
            "Dveri"    => new DoorContentView     { BindingContext = CurrentObject },
            "Myagkaya" => new SoftlyContentView   { BindingContext = CurrentObject },
            "Furnitura"=> new FurnitureContentView{ BindingContext = CurrentObject },
            "Otchet"   => new ReportContentView   { BindingContext = CurrentObject, ParentProject = ParentProject },
            _ => new ContentView { Content = new Label { Text = "Неизвестная вкладка", VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center } }
        };

        ContentContainer.Content = newContent;
    }

    private async void OnSaveReport(object sender, EventArgs e)
    {
        ParentProject?.RecalculateTotals();

        // 2. Уведомляем пользователя
        await DisplayAlert("Успех", "Данные проекта сохранены", "OK");

        // 3. Возвращаемся назад к списку объектов
        await Navigation.PopAsync();
    }

    private async void OnCancel(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Отмена", "Изменения не сохранятся?", "Да", "Нет");
        if (confirm)
            await Navigation.PopAsync();
    }
}