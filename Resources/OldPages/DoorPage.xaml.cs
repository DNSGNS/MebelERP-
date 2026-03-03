namespace MyApp1;

public partial class DoorPage : ContentPage
{
    public ObjectData CurrentOrder { get; set; }
    private readonly ProjectData _parentProject;

    // —сылка на данные дверей внутри общего заказа
    public DoorForm DoorData => CurrentOrder.DoorData;

    // 2.  онструктор принимает ObjectData
    public DoorPage(ObjectData order, ProjectData project)
    {
        _parentProject = project;

        InitializeComponent();

        CurrentOrder = order;

        // ѕрив€зываем контекст напр€мую к данным дверей.
        // Ёто позволит в XAML писать Path=Mirror.Length вместо Path=DoorData.Mirror.Length
        BindingContext = DoorData;
    }

    private async void OnNavigateToSoftlyClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SoftlyPage(CurrentOrder, _parentProject));
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
}