using MyApp1.Resources.Other;

namespace MyApp1;

public partial class ReportContentView : ContentView
{
    public ObjectData CurrentOrder { get; set; }
    public ProjectData ParentProject { get; set; }  // Добавляем, чтобы можно было пересчитывать проект

    public ReportContentView()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (BindingContext is ObjectData data)
        {
            CurrentOrder = data;

            // Пересчитываем данные для отчёта (как в оригинале)
            PriceList prices = new PriceList();
            data.UpdateCalculation(prices);

            // Привязка уже на data (ObjectData)
        }
    }

    //private async void OnSaveReportClicked(object sender, EventArgs e)
    //{
    //    // 1. Пересчитываем весь проект
    //    ParentProject?.RecalculateTotals();

    //    // 2. Уведомляем пользователя
    //    var page = FindParentPage();
    //    if (page != null)
    //    {
    //        await page.DisplayAlert("Успех", "Данные проекта сохранены", "OK");
    //    }
    //    else
    //    {
    //        // Если не нашли страницу — просто ничего не делаем или fallback
    //    }

    //    // 3. Можно добавить возврат на главную вкладку, но пока оставляем как есть
    //    // await Shell.Current.GoToAsync("//MainCalculationPage"); или подобное
    //}

    //// Вспомогательный метод для DisplayAlert
    //private ContentPage? FindParentPage()
    //{
    //    var parent = this.Parent;
    //    while (parent != null && parent is not ContentPage)
    //    {
    //        parent = parent.Parent;
    //    }
    //    return parent as ContentPage;
    //}
}