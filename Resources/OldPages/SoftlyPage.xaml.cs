namespace MyApp1;

public partial class SoftlyPage : ContentPage
{
    public ObjectData CurrentOrder { get; set; }
    private readonly ProjectData _parentProject;

    // Ссылка на данные мягкой зоны внутри общего заказа
    public SoftlyForm SoftlyData => CurrentOrder.SoftlyData;

    // 2. Конструктор принимает ObjectData
    public SoftlyPage(ObjectData order, ProjectData project)
    {
        InitializeComponent();
        _parentProject = project;

        CurrentOrder = order;

        BindingContext = SoftlyData;
    }


    // Авто-выделение текста при нажатии на поле
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
    //private async void OnFinishClicked(object sender, EventArgs e)
    //{
    //    // Здесь можно добавить сохранение или переход на страницу итогов
    //    await DisplayAlert("Готово", "Расчет сохранен", "OK");
    //}
    private async void OnNavigateToFurnitureClicked(object sender, EventArgs e)
    {
        // Переход на страницу фасадов
        await Navigation.PushAsync(new FurniturePage(CurrentOrder, _parentProject));
    }
}