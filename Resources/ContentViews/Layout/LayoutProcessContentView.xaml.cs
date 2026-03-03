using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace MyApp1;

public partial class LayoutProcessContentView : ContentView
{
    private CuttingData ViewModel => BindingContext as CuttingData;

    public LayoutProcessContentView()
    {
        InitializeComponent();
    }

    // Этот метод вызывается, когда ContentView привязывается к данным (ViewModel)
    // или когда мы возвращаемся на вкладку
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (ViewModel?.LastResult?.Sheets != null)
        {
            // Отписываемся от старого (для безопасности) и подписываемся на новый список
            ViewModel.LastResult.Sheets.CollectionChanged -= (s, e) => RefreshUI();
            ViewModel.LastResult.Sheets.CollectionChanged += (s, e) => RefreshUI();

            RefreshUI(); // Рисуем текущее состояние
        }
    }

    private async void OnCalculateClicked(object sender, EventArgs e)
    {
        if (ViewModel == null) return;

        var settings = ViewModel.Settings;
        var details = ViewModel.DetailsForm.Details.ToList();

        // 1. Валидация
        double usableL = (settings.SheetLength ?? 0) - (settings.EdgeOffset * 2);
        double usableW = (settings.SheetWidth ?? 0) - (settings.EdgeOffset * 2);

        if (settings.SheetLength <= 0 || details.Count == 0 || usableL <= 0)
        {
            await App.Current.MainPage.DisplayAlert("Ошибка", "Проверьте данные листа и список деталей", "OK");
            return;
        }

        // 2. Сброс старых данных в ViewModel (Перезапись)
        if (ViewModel.LastResult == null)
            ViewModel.LastResult = new CuttingProcessForm();

        ViewModel.LastResult.Clear(); // Очищаем старые листы

        // 3. Расчет раскроя
        // Где у вас происходит создание packer (обычно в CuttingProcessForm или ViewModel)
        var packer = new GuillotinePacker(
              usableL,
              usableW,
              settings.CutWidth,
              settings.EdgeOffset, // [FIX] Добавлен аргумент отступа
              settings.CuttingMethod
        );

        var sheets = packer.Pack(details);


        // 4. Сохраняем результат в ViewModel (чтобы он выжил при смене вкладок)
        foreach (var sheet in sheets)
        {
            sheet.SheetW = settings.SheetLength ?? 0;
            sheet.SheetH = settings.SheetWidth ?? 0;
            ViewModel.LastResult.Sheets.Add(sheet);

        }

        // Уведомляем систему, что свойства статистики изменились
        ViewModel.LastResult.NotifyAllProperties();



        // 5. Отрисовываем UI
        RefreshUI();
    }

    // Метод, который берет данные из ViewModel и строит список карточек
    private void RefreshUI()
    {
        if (ViewModel?.LastResult?.Sheets == null) return;

        SheetsContainer.Children.Clear();

        foreach (var sheet in ViewModel.LastResult.Sheets)
        {
            var sheetFrame = CreateSheetUI(sheet, ViewModel.Settings.EdgeOffset);
            SheetsContainer.Children.Add(sheetFrame);
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
            Margin = new Thickness(0, 0, 0, 20)
        };

        var stack = new VerticalStackLayout { BackgroundColor = Colors.White };
        var colorText = string.IsNullOrEmpty(sheet.ColorName) ? "" : $" [{sheet.ColorName}]";

        stack.Children.Add(new Label
        {
            Text = $"Стол {sheet.SheetIndex}{colorText} ({sheet.SheetW}x{sheet.SheetH})",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            Padding = new Thickness(15, 10, 15, 5),
            TextColor = Colors.Black
        });

        var graphicsView = new GraphicsView
        {
            HeightRequest = 450,
            BackgroundColor = Colors.White,
            Drawable = new CuttingDiagramDrawable(sheet, edgeOffset)
        };
        stack.Children.Add(graphicsView);

        double sheetPartsArea = sheet.Parts.Sum(p => p.Length * p.Width);
        stack.Children.Add(new Label
        {
            Text = $"Деталей: {sheet.Parts.Count} | Заполнение: {Math.Round(sheetPartsArea)} кв.ед.",
            FontSize = 12,
            TextColor = Colors.DimGray,
            Padding = new Thickness(15, 5, 15, 15)
        });

        frame.Content = stack;
        return frame;
    }
}