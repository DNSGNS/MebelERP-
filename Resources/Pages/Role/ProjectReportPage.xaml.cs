namespace MyApp1;

public partial class ProjectReportPage : ContentPage
{
    private ProjectData _project;

    public ProjectReportPage(ProjectData project)
    {
        InitializeComponent();

        _project = project;

        // 1. Принудительный пересчет перед отображением, 
        // чтобы все суммы (TotalFurniture, Services) обновились
        _project.RecalculateTotals();

        // 2. Привязка данных
        BindingContext = _project;
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        // Просто возвращаемся назад
        await Navigation.PopAsync();
    }
}