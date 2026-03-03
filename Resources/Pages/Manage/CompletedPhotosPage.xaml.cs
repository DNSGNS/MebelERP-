using System.Collections.ObjectModel;

namespace MyApp1;

public partial class CompletedPhotosPage : ContentPage
{
    public string ProjectName { get; set; }
    public ObservableCollection<ProjectImage> Images { get; set; }

    public CompletedPhotosPage(CompletedProject project)
    {
        InitializeComponent();
        ProjectName = project.ProjectName;
        Images = new ObservableCollection<ProjectImage>(project.Images ?? new List<ProjectImage>());
        BindingContext = this;
    }

    private async void OnImageTapped(object sender, TappedEventArgs e)
    {
        // Получаем массив байтов из CommandParameter
        if (e.Parameter is byte[] imageData && imageData.Length > 0)
        {
            // Открываем страницу просмотра, передавая байты
            await Navigation.PushModalAsync(new ImagePreviewPage(imageData));
        }
    }
}