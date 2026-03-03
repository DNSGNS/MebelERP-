namespace MyApp1;

public partial class LayoutDetailContentView : ContentView
{
    private CuttingData ViewModel => BindingContext as CuttingData;
    private bool _isDisplayingAlert = false;
    private bool _shouldFocusNewRow = false;
    public LayoutDetailContentView()
    {
        InitializeComponent();
    }

    private void OnAddDetailClicked(object sender, EventArgs e)
    {
        ViewModel?.DetailsForm.AddDetail();
        _shouldFocusNewRow = true;
    }

    private void OnRemoveDetailClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is CuttingDetails detail)
        {
            ViewModel?.DetailsForm.RemoveDetail(detail);
        }
    }

    private void OnDetailSizeChanged(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry && entry.BindingContext is CuttingDetails detail)
        {
            var settings = ViewModel?.Settings;
            if (settings == null) return;

            double sheetL = settings.SheetLength ?? 0;
            double sheetW = settings.SheetWidth ?? 0;

            if (sheetL <= 0 || sheetW <= 0) return;

            // Проверка размеров
            bool fitsNormally = detail.Length <= sheetL && detail.Width <= sheetW;
            bool fitsRotated = detail.CanRotate && (detail.Length <= sheetW && detail.Width <= sheetL);

            if (!fitsNormally && !fitsRotated)
            {
                // Вместо DisplayAlert просто красим текст в красный
                entry.TextColor = Colors.Red;

                // Можно добавить легкую вибрацию или короткое уведомление (Toast), 
                // которое не блокирует фокус, если у вас подключен CommunityToolkit
            }
            else
            {
                entry.TextColor = Color.FromArgb("#1C1B1F");
            }

        }
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

            // Центрируем строку в списке, чтобы клавиатура её не закрыла
            var detail = entry.BindingContext as CuttingDetails;
            if (detail != null)
            {
                DetailsCollectionView.ScrollTo(detail, position: ScrollToPosition.Center, animate: true);
            }
        }
    }

    // 2. Навигация: Длина -> Ширина (Колонки 1 -> 3)
    private void OnLengthCompleted(object sender, EventArgs e)
    {
        var parentGrid = (sender as Entry)?.Parent as Grid;
        var nextEntry = parentGrid?.Children.FirstOrDefault(c => Grid.GetColumn((View)c) == 3) as Entry;
        nextEntry?.Focus();
    }

    // 3. Навигация: Ширина -> Кол-во (Колонки 3 -> 4)
    private void OnWidthCompleted(object sender, EventArgs e)
    {
        var parentGrid = (sender as Entry)?.Parent as Grid;
        var nextEntry = parentGrid?.Children.FirstOrDefault(c => Grid.GetColumn((View)c) == 4) as Entry;
        nextEntry?.Focus();
    }

    // 4. Навигация: Кол-во -> Новая строка
    private void OnCountCompleted(object sender, EventArgs e)
    {
        var detail = (sender as Entry)?.BindingContext as CuttingDetails;
        if (detail != null && detail == ViewModel?.DetailsForm.Details.LastOrDefault())
        {
            OnAddDetailClicked(sender, e);
        }
    }

    // 5. Фокус новой строки при загрузке Entry
    private async void OnLengthEntryLoaded(object sender, EventArgs e)
    {
        if (!_shouldFocusNewRow || sender is not Entry entry) return;

        var detail = entry.BindingContext as CuttingDetails;
        if (detail != ViewModel?.DetailsForm.Details.LastOrDefault()) return;

        _shouldFocusNewRow = false;

        // Прокрутка вниз
        DetailsCollectionView.ScrollTo(detail, position: ScrollToPosition.End, animate: false);

        await Task.Delay(100);
        entry.Focus();
    }
}