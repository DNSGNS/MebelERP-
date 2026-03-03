using System.Collections.ObjectModel;

namespace MyApp1;

public partial class FasadContentView : ContentView
{
    private bool _shouldFocusNewRow = false;
    public ObjectData CurrentOrder { get; set; }

    public ObservableCollection<FasadForm> StandardFasads => CurrentOrder?.StandardFasads;
    public ObservableCollection<FasadForm> SpecialFasads => CurrentOrder?.SpecialFasads;
    public ObservableCollection<FasadForm> NonStandardFasads => CurrentOrder?.NonStandardFasads;

    public List<FasadEdgeType> BulkEdgeTypes { get; } = new() { FasadEdgeType.Freza, FasadEdgeType.Mylo };

    public FasadContentView()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (BindingContext is ObjectData order)
        {
            CurrentOrder = order;
            // Инициализация первых строк, если пусто
            if (StandardFasads?.Count == 0) StandardFasads.Add(new FasadForm { SelectedType = FasadType.Standard });
            if (SpecialFasads?.Count == 0) SpecialFasads.Add(new FasadForm { SelectedType = FasadType.AGT });
            if (NonStandardFasads?.Count == 0) NonStandardFasads.Add(new FasadForm { SelectedType = FasadType.NonStandard });
        }
    }

    #region Навигация по полям

    private void OnLengthCompleted(object sender, EventArgs e)
    {
        FocusNextEntry(sender as Entry, 1); // К Ширине (Column 1)
    }

    private void OnWidthCompleted(object sender, EventArgs e)
    {
        FocusNextEntry(sender as Entry, 2); // К Кол-ву (Column 2)
    }

    private void OnCountCompleted(object sender, EventArgs e)
    {
        var entry = sender as Entry;
        var form = entry?.BindingContext as FasadForm;
        if (form == null) return;

        // Проверяем, в каком списке находится текущая форма, и если она последняя - добавляем новую
        if (form == StandardFasads?.LastOrDefault()) OnAddStandardClicked(null, null);
        else if (form == SpecialFasads?.LastOrDefault()) OnAddSpecialClicked(null, null);
        else if (form == NonStandardFasads?.LastOrDefault()) OnAddNonStandardClicked(null, null);
    }

    private void FocusNextEntry(Entry current, int targetColumn)
    {
        // Иерархия: Entry -> VerticalStackLayout -> Grid
        var parentGrid = current?.Parent?.Parent as Grid;
        var nextLayout = parentGrid?.Children
            .FirstOrDefault(c => Grid.GetColumn((View)c) == targetColumn && Grid.GetRow((View)c) == Grid.GetRow(current.Parent as View)) as VerticalStackLayout;

        var nextEntry = nextLayout?.Children.FirstOrDefault(c => c is Entry) as Entry;
        nextEntry?.Focus();
    }

    private async void OnEntryLoaded(object sender, EventArgs e)
    {
        if (!_shouldFocusNewRow || sender is not Entry entry) return;

        var form = entry.BindingContext as FasadForm;
        // Проверяем, является ли эта форма последней в любом из списков
        bool isLast = form == StandardFasads?.LastOrDefault() ||
                      form == SpecialFasads?.LastOrDefault() ||
                      form == NonStandardFasads?.LastOrDefault();

        if (isLast)
        {
            _shouldFocusNewRow = false;
            await Task.Delay(100);
            entry.Focus();
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
        }
    }
    #endregion

    private void OnAddStandardClicked(object sender, EventArgs e) => AddForm(StandardFasads, FasadType.Standard);
    private void OnAddSpecialClicked(object sender, EventArgs e) => AddForm(SpecialFasads, FasadType.AGT);
    private void OnAddNonStandardClicked(object sender, EventArgs e) => AddForm(NonStandardFasads, FasadType.NonStandard);

    private void AddForm(ObservableCollection<FasadForm>? list, FasadType type)
    {
        if (list == null) return;
        if (Validate(list.LastOrDefault()))
        {
            _shouldFocusNewRow = true;
            list.Add(new FasadForm { SelectedType = type });
        }
    }

    // Остальные методы (Remove, ApplyBulk, Validate) остаются без изменений
    private void OnRemoveStandardClicked(object sender, EventArgs e) => RemoveForm(StandardFasads, sender);
    private void OnRemoveSpecialClicked(object sender, EventArgs e) => RemoveForm(SpecialFasads, sender);
    private void OnRemoveNonStandardClicked(object sender, EventArgs e) => RemoveForm(NonStandardFasads, sender);

    private void RemoveForm(ObservableCollection<FasadForm>? list, object sender)
    {
        if (list == null) return;
        var form = (sender as Button)?.CommandParameter as FasadForm;
        if (form != null && list.Count > 1) list.Remove(form);
    }

    private bool Validate(FasadForm? form)
    {
        if (form == null) return true;
        if ((form.Length ?? 0) <= 0 || (form.Width ?? 0) <= 0)
        {
            FindParentPage()?.DisplayAlert("Внимание", "Заполните размеры текущей детали", "OK");
            return false;
        }
        return true;
    }

    private void OnApplyBulkUpdateClicked(object sender, EventArgs e)
    {
        var selectedEdge = BulkEdgePicker.SelectedItem as FasadEdgeType?;
        string newColor = BulkColorEntry.Text;
        string newMilling = BulkMillingEntry.Text;
        var allLists = new[] { StandardFasads, SpecialFasads, NonStandardFasads };

        foreach (var list in allLists)
        {
            if (list == null) continue;
            foreach (var item in list.Where(x => x.IsSelected))
            {
                if (selectedEdge.HasValue) item.SelectedEdgeType = selectedEdge.Value;
                if (!string.IsNullOrWhiteSpace(newColor)) item.Color = newColor;
                if (!string.IsNullOrWhiteSpace(newMilling)) item.MillingText = newMilling;
                item.IsSelected = false;
            }
        }
        BulkColorEntry.Text = BulkMillingEntry.Text = string.Empty;
        BulkEdgePicker.SelectedItem = null;
    }

    private ContentPage? FindParentPage()
    {
        var parent = this.Parent;
        while (parent != null && parent is not ContentPage) parent = parent.Parent;
        return parent as ContentPage;
    }
}