using Microsoft.Maui.Controls;

namespace MyApp1;

public partial class LayoutPage : ContentPage
{
    // Ссылка на родителя и на данные раскладки
    public ObjectData ParentObject { get; }
    public CuttingData CurrentLayout { get; private set; }

    public LayoutPage(ObjectData objectData, CuttingData layoutData)
    {
        InitializeComponent();

        ParentObject = objectData;
        CurrentLayout = layoutData;

        Title = $"Раскладка: {ParentObject.ObjectName}";
        BindingContext = CurrentLayout;


        CurrentLayout.Settings.SheetLength = 2960;
        CurrentLayout.Settings.SheetWidth = 1280;
        CurrentLayout.Settings.CutWidth = 60;
        CurrentLayout.Settings.EdgeOffset = 60;
        // По умолчанию открываем детали
        SwitchToTab("Detail");
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
        // Сброс стилей кнопок
        btnDetail.BackgroundColor = Colors.Transparent;
        btnProcess.BackgroundColor = Colors.Transparent;

        // Подсветка активной
        switch (tabName)
        {
            case "Detail": btnDetail.BackgroundColor = Color.FromArgb("#6750A4"); break;
            case "Process": btnProcess.BackgroundColor = Color.FromArgb("#6750A4"); break;
            case "Edit": btnEdit.BackgroundColor = Color.FromArgb("#6750A4"); break;
            case "Report": btnReport.BackgroundColor = Color.FromArgb("#6750A4"); break;
        }

        // Смена контента
        ContentView newContent = tabName switch
        {
            // Передаем BindingContext (CurrentLayout)
            "Detail" => new LayoutDetailContentView { BindingContext = CurrentLayout },
            "Process" => new LayoutProcessContentView { BindingContext = CurrentLayout },
            "Edit" => new LayoutEditorContentView { BindingContext = CurrentLayout },
            "Report" => new LayoutReportContentView { BindingContext = CurrentLayout },

            // Если нужна страница настроек стола (TableLength, Spacing), можно создать LayoutSettingsContentView
            // "Setting" => new LayoutSettingsContentView { BindingContext = CurrentLayout },

            _ => new ContentView { Content = new Label { Text = "Ошибка" } }
        };

        ContentContainer.Content = newContent;
    }
}