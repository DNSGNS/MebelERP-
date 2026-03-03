using System.Reflection.Metadata;

namespace MyApp1;

public partial class CompletedObjectsPage : ContentPage
{
    public CompletedProject Project { get; set; }
    public List<CompletedProjectObject> ProjectObjects => Project?.ProjectObjects;

    public CompletedObjectsPage(CompletedProject project)
    {
        InitializeComponent();
        Project = project;
        Title = $"Объекты: {project.ProjectName}";
        BindingContext = this;
    }

    private async void OnReportDoorSlimLineClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button?.CommandParameter is CompletedProjectObject completedObject)
        {
            if (completedObject.DoorDetail == null)
            {
                await DisplayAlert("Инфо", "Данные Slim Line отсутствуют для этого объекта", "OK");
                return;
            }
            // Переходим на страницу просмотра, передавая данные
            await Navigation.PushAsync(new DoorSlimLineReadPage(completedObject.DoorDetail));
        }
    }


    private async void OnCuttingReportCorpusLDSPClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button?.CommandParameter is CompletedProjectObject obj)
        {
            var report = obj.CuttingReports?.FirstOrDefault(r => r.Type == CuttingType.Ldsp);
            if (report != null)
            {
                await Navigation.PushAsync(new CuttingSavedReportPage(report, Project.ProjectName, obj.OrderName));
            }
            else
            {
                await DisplayAlert("Пусто", "Отчет 'Корпус ЛДСП' не найден в архиве.", "OK");
            }
        }
    }

    private async void OnCuttingReportFasadsAGTClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button?.CommandParameter is CompletedProjectObject obj)
        {
            var report = obj.CuttingReports?.FirstOrDefault(r => r.Type == CuttingType.FAgt);
            if (report != null)
            {
                await Navigation.PushAsync(new CuttingSavedReportPage(report, Project.ProjectName, obj.OrderName));
            }
            else
            {
                await DisplayAlert("Пусто", "Отчет 'Фасады AGT' не найден в архиве.", "OK");
            }
        }
    }

    private async void OnCuttingReportFasadsLDSPClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button?.CommandParameter is CompletedProjectObject obj)
        {
            var report = obj.CuttingReports?.FirstOrDefault(r => r.Type == CuttingType.FLdsp);
            if (report != null)
            {
                await Navigation.PushAsync(new CuttingSavedReportPage(report, Project.ProjectName, obj.OrderName));
            }
            else
            {
                await DisplayAlert("Пусто", "Отчет 'Фасады ЛДСП' не найден в архиве.", "OK");
            }
        }
    }
}