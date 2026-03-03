namespace MyApp1;

public partial class CuttingSettingContentView : ContentView
{
    public CuttingSettingContentView()
    {
        InitializeComponent();
    }

    // Метод для удобного выделения всего текста при фокусе (как в ваших предыдущих страницах)
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