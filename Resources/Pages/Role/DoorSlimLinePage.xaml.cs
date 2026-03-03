namespace MyApp1;

public partial class DoorSlimLinePage : ContentPage
{
    public ObjectData ParentObject { get; }
    public DoorSlimLinePage(ObjectData objectData)
    {
        InitializeComponent();
        ParentObject = objectData;

        if (ParentObject.DoorSlimLineData != null)
        {
            ParentObject.DoorSlimLineData.Parent = ParentObject;
        }

        Title = $"Slim Line: {ParentObject.ObjectName}";
        BindingContext = ParentObject.DoorSlimLineData;
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

    private async void OnShowInstructionsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SlimLineInstructionsPage());
    }
}

