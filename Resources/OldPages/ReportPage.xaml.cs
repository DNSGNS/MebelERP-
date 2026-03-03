using MyApp1.Resources.Other;

namespace MyApp1;

public partial class ReportPage : ContentPage
{
    private readonly ProjectData _parentProject;

    // Добавляем второй аргумент - проект
    public ReportPage(ObjectData data, ProjectData project)
    {
        InitializeComponent();

        _parentProject = project;

        // 1. Считаем данные для конкретного объекта
        PriceList prices = new PriceList();
        data.UpdateCalculation(prices);

        // 2. Привязываем данные объекта к разметке
        BindingContext = data;
    }

    private async void OnSaveReportClicked(object sender, EventArgs e)
    {

            // 1. Вызываем пересчет всего проекта (суммируем все объекты проекта)
            _parentProject?.RecalculateTotals();

            // 3. Уведомляем пользователя
            await DisplayAlert("Успех", "Данные проекта сохранены", "OK");

            // 4. Возвращаемся назад к списку объектов
           // await Navigation.PushAsync(new MainCalculationPage());
        

    }
}