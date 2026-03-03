namespace MyApp1;

public partial class ImagePreviewPage : ContentPage
{
    public ImagePreviewPage(string imagePath)
    {
        InitializeComponent();
        FullImage.Source = imagePath;
    }

    public ImagePreviewPage(byte[] imageData)
    {
        InitializeComponent();
        FullImage.Source = ImageSource.FromStream(() => new MemoryStream(imageData));
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync(); // Закрываем окно
    }
}