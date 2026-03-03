namespace MyApp1;

public partial class SlimLineInstructionsPage : ContentPage
{
	public SlimLineInstructionsPage()
	{
		InitializeComponent();
	}

    private async void OnImageTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not string imageName)
            return;

        // Открываем страницу просмотра изображения
        await Navigation.PushModalAsync(new ImagePreviewPage(imageName));
    }
}