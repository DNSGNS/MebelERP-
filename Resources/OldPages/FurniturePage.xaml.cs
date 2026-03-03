namespace MyApp1;

public partial class FurniturePage : ContentPage
{
    public ObjectData CurrentOrder { get; set; }
    private readonly ProjectData _parentProject;

    // Ссылка на фурнитуру берется из общего заказа
    public FurnitureForm Furniture => CurrentOrder.Furniture;

    // 2. Конструктор принимает ObjectData
    public FurniturePage(ObjectData order, ProjectData project)
    {
        _parentProject = project;

        InitializeComponent();

        CurrentOrder = order;

        BindingContext = this;
    }

    private void OnEntryFocused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(50);
                entry.CursorPosition = 0;
                entry.SelectionLength = entry.Text?.Length ?? 0;
            });
        }
    }

    //private async void OnNavigateToFasadClicked(object sender, EventArgs e)
    //{
    //    // Переход на страницу фасадов
    //    await Navigation.PushAsync(new FasadPage());
    //}

    private async void OnFinishClicked(object sender, EventArgs e)
    {
        
          await Navigation.PushAsync(new ReportPage(CurrentOrder, _parentProject));
    }
}