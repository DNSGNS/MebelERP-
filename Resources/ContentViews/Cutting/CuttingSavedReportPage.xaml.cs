using CommunityToolkit.Maui.Storage;
using System.Linq;
using System.Text.Json;

namespace MyApp1;

public partial class CuttingSavedReportPage : ContentPage
{
    public CuttingData ViewModel { get; set; }

    // Свойство для управления видимостью колонки "Цвет"
    private bool _isSavedColorVisible;
    public bool IsSavedColorVisible
    {
        get => _isSavedColorVisible;
        set
        {
            _isSavedColorVisible = value;
            OnPropertyChanged();
        }
    }

    public CuttingSavedReportPage()
    {
        InitializeComponent();
    }

    // 2. НОВЫЙ КОНСТРУКТОР для работы с архивом (CompletedProject)
    public CuttingSavedReportPage(CuttingRep archiveReport, string projectName, string objectName)
    {
        InitializeComponent();

        // Преобразуем архивную модель (CuttingRep) в рабочую модель MAUI (CuttingData)
        ViewModel = new CuttingData
        {
            ProjectName = projectName,
            ObjectName = objectName,
            SavedReport = new CuttingSaveForm
            {
                SheetLength = archiveReport.SheetLength,
                SheetWidth = archiveReport.SheetWidth,
                SheetArea = archiveReport.SheetArea,
                MaterialColor = archiveReport.MaterialColor,
                TotalSheets = archiveReport.TotalSheets,
                TotalSheetArea = archiveReport.TotalSheetArea,
                TotalPartsCount = archiveReport.TotalPartsCount,
                TotalPartsArea = archiveReport.TotalPartsArea,
                Edge1Name = archiveReport.Edge1Name,
                Edge1Thickness = archiveReport.Edge1Thickness,
                TotalEdge1 = archiveReport.TotalEdge1,
                Edge2Name = archiveReport.Edge2Name,
                Edge2Thickness = archiveReport.Edge2Thickness,
                TotalEdge2 = archiveReport.TotalEdge2
            }
        };

        // Восстанавливаем список деталей
        if (archiveReport.Details != null)
        {
            foreach (var detail in archiveReport.Details)
            {
                ViewModel.SavedReport.Details.Add(new CuttingDetails
                {
                    Id = detail.Id,
                    Color = detail.Color,
                    Length = detail.Length,
                    Width = detail.Width,
                    Count = detail.Count,
                    CanRotate = detail.CanRotate,
                    E1L1 = detail.E1L1,
                    E1L2 = detail.E1L2,
                    E1W1 = detail.E1W1,
                    E1W2 = detail.E1W2,
                    E2L1 = detail.E2L1,
                    E2L2 = detail.E2L2,
                    E2W1 = detail.E2W1,
                    E2W2 = detail.E2W2
                });
            }
        }

        // Восстанавливаем чертежи для отрисовки графики
        if (archiveReport.Sheets != null)
        {
            foreach (var sheet in archiveReport.Sheets)
            {
                if (!string.IsNullOrEmpty(sheet.LayoutDataJson))
                {
                    try
                    {
                        var layout = JsonSerializer.Deserialize<SheetLayout>(sheet.LayoutDataJson);
                        if (layout != null)
                        {
                            layout.ColorName = sheet.ColorName;
                            ViewModel.SavedReport.Sheets.Add(layout);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка загрузки чертежа из архива: {ex.Message}");
                    }
                }
            }
        }

        // Устанавливаем BindingContext на созданную вручную ViewModel
        BindingContext = ViewModel;
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (BindingContext is CuttingData data)
        {
            ViewModel = data;
        }

        // Проверяем, есть ли цвета в сохраненных деталях
        if (ViewModel?.SavedReport?.Details != null)
        {
            IsSavedColorVisible = ViewModel.SavedReport.Details.Any(d => !string.IsNullOrEmpty(d.Color));
        }

        RefreshSheetsUI();
    }

    private void RefreshSheetsUI()
    {
        // Теперь берем данные ТОЛЬКО из SavedReport
        if (ViewModel?.SavedReport?.Sheets == null) return;

        ReportSheetsContainer.Children.Clear();

        foreach (var sheet in ViewModel.SavedReport.Sheets)
        {
            var sheetFrame = CreateSheetUI(sheet, 0); // Offset 0 для просмотра
            ReportSheetsContainer.Children.Add(sheetFrame);
        }
    }

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

        var stack = new VerticalStackLayout { BackgroundColor = Colors.White };

        // --- ИНФО О ЛИСТЕ ---
        var infoStack = new VerticalStackLayout { Padding = new Thickness(15, 10, 15, 5), Spacing = 2 };

        infoStack.Children.Add(new Label
        {
            Text = $"Лист {sheet.SheetIndex}",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Black
        });

        // Добавляем вывод цвета листа, если он есть
        if (!string.IsNullOrEmpty(sheet.ColorName))
        {
            infoStack.Children.Add(new Label
            {
                Text = $"Цвет материала: {sheet.ColorName}",
                FontSize = 14,
                TextColor = Colors.DimGray,
                FontAttributes = FontAttributes.Italic
            });
        }

        infoStack.Children.Add(new Label
        {
            Text = $"{sheet.SheetW} x {sheet.SheetH} мм",
            FontSize = 14,
            TextColor = Colors.Gray
        });

        stack.Children.Add(infoStack);
        // --------------------

        var graphicsView = new GraphicsView
        {
            HeightRequest = 400,
            BackgroundColor = Colors.White,
            Drawable = new CuttingDiagramDrawable(sheet, edgeOffset)
        };
        stack.Children.Add(graphicsView);

        double sheetPartsArea = sheet.Parts.Sum(p => p.Length * p.Width);
        stack.Children.Add(new Label
        {
            Text = $"Деталей: {sheet.Parts.Count} | Площадь деталей: {Math.Round(sheetPartsArea / 1000000.0, 3)} м²",
            FontSize = 12,
            TextColor = Colors.DimGray,
            Padding = new Thickness(15, 5, 15, 15)
        });

        frame.Content = stack;
        return frame;
    }

    private async void OnSavePdfClicked(object sender, EventArgs e)
    {
        try
        {
            if (ViewModel == null) return;

            string objname = ViewModel.ObjectName ?? "Изделие";
            string projname = ViewModel.ProjectName ?? "Адрес";

            // Имя файла
            string fileName = $"{projname}_{objname}_Архив.pdf";
            string tempFilePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            // Генерируем PDF используя сервис
            // Важно: PdfReportService должен уметь работать с данными из SavedReport, 
            // если ViewModel передан как аргумент.
            var service = new PdfReportService();
            service.GenerateReport(tempFilePath, ViewModel);

            // Сохраняем через FileSaver
            using var stream = File.OpenRead(tempFilePath);
            var fileSaverResult = await FileSaver.Default.SaveAsync(fileName, stream, default);

            if (fileSaverResult.IsSuccessful)
            {
                await DisplayAlert("Готово", $"Файл сохранен:\n{fileSaverResult.FilePath}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось сохранить PDF: {ex.Message}", "OK");
        }
    }
}

// Специальный конвертер для страницы архива
public class SavedAreaConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is double areaInMm2)
        {
            return areaInMm2 / 1_000_000.0;
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}