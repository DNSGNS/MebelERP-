using CommunityToolkit.Maui.Storage;
using System.Collections.ObjectModel;
using System.Globalization;

namespace MyApp1;

public partial class CuttingReportContentView : ContentView
{
    // Удобный доступ к данным
    private CuttingData ViewModel => BindingContext as CuttingData;

    public CuttingReportContentView()
    {
        InitializeComponent();
    }

    // Этот метод срабатывает автоматически, когда страница получает данные
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        RefreshSheetsUI();
    }

    private void RefreshSheetsUI()
    {
        if (ViewModel?.LastResult?.Sheets == null) return;

        // Очищаем старое, если было
        ReportSheetsContainer.Children.Clear();

        // Отрисовываем каждый лист
        foreach (var sheet in ViewModel.LastResult.Sheets)
        {
            var sheetFrame = CreateSheetUI(sheet, ViewModel.Settings.EdgeOffset);
            ReportSheetsContainer.Children.Add(sheetFrame);
        }
    }

    // Копируем метод создания UI листа из CuttingProcess
    private Frame CreateSheetUI(SheetLayout sheet, double edgeOffset)
    {
        var frame = new Frame
        {
            BackgroundColor = Colors.White,
            BorderColor = Colors.LightGray,
            CornerRadius = 10,
            Padding = 0,
            HasShadow = true,
            Margin = new Thickness(0, 0, 0, 10)
        };

        var mainStack = new VerticalStackLayout();

        // Инфо-панель сверху
        var infoLayout = new VerticalStackLayout { Spacing = 2, Padding = 15 };

        infoLayout.Children.Add(new Label
        {
            Text = $"ЛИСТ {sheet.SheetIndex}",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#6750A4")
        });

        // Добавляем цвет, если он есть
        if (!string.IsNullOrEmpty(sheet.ColorName))
        {
            infoLayout.Children.Add(new Label
            {
                Text = $"Цвет материала: {sheet.ColorName}",
                FontSize = 14,
                TextColor = Colors.DimGray,
                FontAttributes = FontAttributes.Italic
            });
        }

        infoLayout.Children.Add(new Label { Text = $"{sheet.SheetW} x {sheet.SheetH} мм", FontSize = 14 });

        mainStack.Children.Add(infoLayout);

        // Основной чертеж листа
        var graphicsView = new GraphicsView
        {
            HeightRequest = 400, // Можно чуть уменьшить для отчета
            BackgroundColor = Colors.White,
            Drawable = new CuttingDiagramDrawable(sheet, edgeOffset)
        };
        mainStack.Children.Add(graphicsView);

        double sheetPartsArea = sheet.Parts.Sum(p => p.Length * p.Width);
        mainStack.Children.Add(new Label
        {
            Text = $"Деталей: {sheet.Parts.Count} | Заполнение: {Math.Round(sheetPartsArea)} кв.мм.",
            FontSize = 12,
            TextColor = Colors.DimGray,
            Padding = new Thickness(15, 5, 15, 15)
        });

        frame.Content = mainStack;
        return frame;
    }
    private async void OnSaveReportClicked(object sender, EventArgs e)
    {
        try
        {
            if (BindingContext is not CuttingData data) return;

            // 1. Формируем "снимок" данных для сохранения
            var report = new CuttingSaveForm
            {
                SheetLength = data.Settings.SheetLength ?? 0,
                SheetWidth = data.Settings.SheetWidth ?? 0,
                SheetArea = (data.Settings.SheetLength ?? 0) * (data.Settings.SheetWidth ?? 0),
                MaterialColor = data.MaterialColor, 

                TotalSheets = data.LastResult.TotalSheets,
                TotalSheetArea = data.LastResult.TotalSheetArea,
                TotalPartsCount = data.LastResult.TotalPartsCount,
                TotalPartsArea = data.LastResult.TotalPartsArea,

                Edge1Name = data.Settings.Edge1Name,
                Edge1Thickness = data.Settings.Edge1Thickness,
                TotalEdge1 = data.TotalEdge1,

                Edge2Name = data.Settings.Edge2Name,
                Edge2Thickness = data.Settings.Edge2Thickness,
                TotalEdge2 = data.TotalEdge2,

                // Копируем листы в новую коллекцию
                Sheets = new ObservableCollection<SheetLayout>(data.LastResult.Sheets),
                Details = new ObservableCollection<CuttingDetails>(data.DetailsForm.Details)
            };

            // Записываем отчет в модель данных
            data.SavedReport = report;




            string objname = ViewModel.ObjectName ?? "Изделие";
            string projname = ViewModel.ProjectName ?? "Адрес";
            // 1. Генерируем PDF во временную папку (как и раньше)
            string fileName = $"{projname}_{objname}.pdf";
            string tempFilePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            var service = new PdfReportService();
            service.GenerateReport(tempFilePath, data);

            // 2. Используем FileSaver для сохранения в конкретную папку
            using var stream = File.OpenRead(tempFilePath);

            // Это откроет системное окно сохранения файла (сразу в папку Downloads)
            var fileSaverResult = await FileSaver.Default.SaveAsync(fileName, stream, default);

            if (fileSaverResult.IsSuccessful)
            {
                // Можно просто закрыть страницу или показать успех
                await Shell.Current.DisplayAlert("Готово", $"Файл сохранен: {fileSaverResult.FilePath}", "OK");
                await Navigation.PopAsync();
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
                await Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }
}

////// Конвертер для перевода мм² в м²
////// Также умеет считать площадь одного листа, если передать ему объект CuttingSettingForm
////public class AreaConverter : IValueConverter
////{
////    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
////    {
////        if (value is double areaInMm2)
////        {
////            // Перевод мм² -> м² (делим на 1,000,000)
////            return areaInMm2 / 1_000_000.0;
////        }

////        // Если передали настройки (для расчета площади 1 листа)
////        if (value is CuttingSettingForm settings)
////        {
////            double l = settings.SheetLength ?? 0;
////            double w = settings.SheetWidth ?? 0;
////            return (l * w) / 1_000_000.0;
////        }

////        return 0.0;
////    }

////    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
////    {
////        throw new NotImplementedException();
////    }
//}