using MyApp1.Resources.Other;

namespace MyApp1;

public partial class ObjectWarehouseEditorPage : ContentPage
{
    public ObjectData CurrentObject { get; }
    public ProjectData ParentProject { get; }

    public ObjectWarehouseEditorPage(ObjectData obj, ProjectData project)
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
            case "LDSP": btnLDSP.BackgroundColor = Color.FromArgb("#6750A4"); break;
            case "Fasady": btnFasady.BackgroundColor = Color.FromArgb("#6750A4"); break;
            case "Dveri": btnDveri.BackgroundColor = Color.FromArgb("#6750A4"); break;
            case "Myagkaya": btnMyagkaya.BackgroundColor = Color.FromArgb("#6750A4"); break;
            case "Furnitura": btnFurnitura.BackgroundColor = Color.FromArgb("#6750A4"); break;
            case "Otchet": btnOtchet.BackgroundColor = Color.FromArgb("#6750A4"); break;
        }

        // Меняем содержимое (используем Warehouse-версии)
        ContentView newContent = tabName switch
        {
            "LDSP" => new LDSPWarehouseContentView { BindingContext = CurrentObject },
            "Fasady" => new FasadWarehouseContentView { BindingContext = CurrentObject },
            "Dveri" => new DoorWarehouseContentView { BindingContext = CurrentObject },
            "Myagkaya" => new SoftlyWarehouseContentView { BindingContext = CurrentObject },
            "Furnitura" => new FurnitureWarehouseContentView { BindingContext = CurrentObject },
            "Otchet" => new ReportContentView { BindingContext = CurrentObject, ParentProject = ParentProject }, // оставляю старый отчет, т.к. страница идентична
            _ => new ContentView { Content = new Label { Text = "Неизвестная вкладка", VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center } }
        };

        ContentContainer.Content = newContent;
    }

    private async void OnCancel(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Отмена", "Изменения не сохранятся?", "Да", "Нет");
        if (confirm)
            await Navigation.PopAsync();
    }
}