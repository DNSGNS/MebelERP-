using System.Collections.ObjectModel;

namespace MyApp1;

public partial class FasadPage : ContentPage
{
    public ObjectData CurrentOrder { get; set; }
    private readonly ProjectData _parentProject;

    // Перенаправляем коллекции на списки внутри заказа
    public ObservableCollection<FasadForm> StandardFasads => CurrentOrder.StandardFasads;
    public ObservableCollection<FasadForm> SpecialFasads => CurrentOrder.SpecialFasads;
    public ObservableCollection<FasadForm> NonStandardFasads => CurrentOrder.NonStandardFasads;

    // 2. Конструктор принимает ObjectData
    public FasadPage(ObjectData order, ProjectData project)
    {
        _parentProject = project;

        InitializeComponent();
        CurrentOrder = order;
        BindingContext = this;

        // Инициализируем пустые списки начальными элементами только если они пустые
        if (StandardFasads.Count == 0)
            StandardFasads.Add(new FasadForm { SelectedType = FasadType.Standard });

        if (SpecialFasads.Count == 0)
            SpecialFasads.Add(new FasadForm { SelectedType = FasadType.AGT });

        if (NonStandardFasads.Count == 0)
            NonStandardFasads.Add(new FasadForm { SelectedType = FasadType.NonStandard });
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

    // Добавление
    private void OnAddStandardClicked(object sender, EventArgs e) =>
        AddForm(StandardFasads, FasadType.Standard);

    private void OnAddSpecialClicked(object sender, EventArgs e) =>
        AddForm(SpecialFasads, FasadType.AGT);

    private void OnAddNonStandardClicked(object sender, EventArgs e) =>
        AddForm(NonStandardFasads, FasadType.NonStandard);

    private void AddForm(ObservableCollection<FasadForm> list, FasadType type)
    {
        if (Validate(list.LastOrDefault()))
            list.Add(new FasadForm { SelectedType = type });
    }

    // Удаление
    private void OnRemoveStandardClicked(object sender, EventArgs e) => RemoveForm(StandardFasads, sender);
    private void OnRemoveSpecialClicked(object sender, EventArgs e) => RemoveForm(SpecialFasads, sender);
    private void OnRemoveNonStandardClicked(object sender, EventArgs e) => RemoveForm(NonStandardFasads, sender);

    private void RemoveForm(ObservableCollection<FasadForm> list, object sender)
    {
        var form = (sender as Button)?.CommandParameter as FasadForm;
        if (form != null && list.Count > 1) list.Remove(form);
    }

    private async void OnNavigateToDoorClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new DoorPage(CurrentOrder, _parentProject));

    private bool Validate(FasadForm form)
    {
        if (form == null) return true;
        if ((form.Length ?? 0) <= 0 || (form.Width ?? 0) <= 0)
        {
            DisplayAlert("Внимание", "Заполните размеры текущей детали", "OK");
            return false;
        }
        return true;
    }
}