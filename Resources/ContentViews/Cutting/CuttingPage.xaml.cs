namespace MyApp1;

public partial class CuttingPage : ContentPage
{
    // Храним ссылку на родительский объект, если понадобится доступ к его имени и т.д.
    public ObjectData ParentObject { get; }

    // Удаляем жесткую привязку к CuttingLdsp
    public CuttingData CurrentCut { get; private set; }

    // Обновляем конструктор: теперь он принимает и весь объект, и конкретный раскрой
    public CuttingPage(ObjectData objectData, CuttingData specificCutting)
    {
        InitializeComponent();

        ParentObject = objectData;
        CurrentCut = specificCutting; // Устанавливаем тот раскрой, который выбрали в меню

        Title = $"Раскрой: {ParentObject.ObjectName}";
        BindingContext = CurrentCut;

        SwitchToTab("Setting");
    }
    private void OnTabClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is string tabName)
        {
            SwitchToTab(tabName);
        }
    }

    private void SwitchToTab(string tabName)
    {

        btnSetting.BackgroundColor = Colors.Transparent;
        btnDetail.BackgroundColor = Colors.Transparent;
        btnEdge.BackgroundColor = Colors.Transparent;
        btnCutting.BackgroundColor = Colors.Transparent;
        btnEdit.BackgroundColor = Colors.Transparent;
        btnOtchet.BackgroundColor = Colors.Transparent;


        // �������� ������
        switch (tabName)
        {
            case "Setting": btnSetting.BackgroundColor = Color.FromArgb("#6750A4"); break;
            case "Detail": btnDetail.BackgroundColor = Color.FromArgb("#6750A4"); break;
            case "Edge": btnEdge.BackgroundColor = Color.FromArgb("#6750A4"); break;
            case "Cutting": btnCutting.BackgroundColor = Color.FromArgb("#6750A4"); break;
            case "Edit": btnEdit.BackgroundColor = Color.FromArgb("#6750A4"); break;
            case "Otchet": btnOtchet.BackgroundColor = Color.FromArgb("#6750A4"); break;
        }

        // Ěĺí˙ĺě ńîäĺđćčěîĺ
        ContentView newContent = tabName switch
        {
            "Setting" => new CuttingSettingContentView { BindingContext = CurrentCut },
            "Detail" => new CuttingDetailContentView { BindingContext = CurrentCut },
            "Edge" => new CuttingEdgeContentView { BindingContext = CurrentCut },
            "Cutting" => new CuttingProcessContentView { BindingContext = CurrentCut },
            "Edit" => new CuttingEditorContentView { BindingContext = CurrentCut },
            "Otchet" => new CuttingReportContentView { BindingContext = CurrentCut },
            _ => new ContentView { Content = new Label { Text = "Ошибка" } }
        };

        ContentContainer.Content = newContent;
    }

    private async void OnSaveReport(object sender, EventArgs e)
    {
        //        ParentProject?.RecalculateTotals();

        // 2. Уведомляем пользователя
        await DisplayAlert("Успех", "Данные проекта сохранены", "OK");

        // 3. Возвращаемся назад к списку объектов
        await Navigation.PopAsync();
    }

    private async void OnCancel(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Отмена", "Изменения не сохранятся?", "Да", "Нет");
        if (confirm)
            await Navigation.PopAsync();
    }
}