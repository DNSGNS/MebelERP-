using Microcharts; // Пространство имен библиотеки
using MyApp1;
using SkiaSharp;   // Используется библиотекой для цветов

namespace MyApp1;

public partial class ChartsPage : ContentPage
{
    public ChartsPage(ExpensesForm viewModel)
    {
        InitializeComponent();
        GenerateChartData(viewModel);
    }

    private void GenerateChartData(ExpensesForm viewModel)
    {
        // 1. Фильтруем пустые траты
        var validExpenses = viewModel.AllExpenses
            .Where(x => x.Amount > 0 && x.Category != null)
            .ToList();

        if (!validExpenses.Any()) return; // Или показать сообщение "Нет данных"

        decimal grandTotal = validExpenses.Sum(x => x.Amount);

        // 2. Группируем по названию категории
        var groupedData = validExpenses
            .GroupBy(x => x.Category.Name)
            .Select(g => new
            {
                Name = g.Key,
                Total = g.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.Total) // Сортируем: большие траты сверху
            .ToList();

        // Списки для UI
        var chartEntries = new List<ChartEntry>();
        var summaryList = new List<ChartsForm>();

        // Палитра цветов (SkiaSharp SKColor)
        var colors = new SKColor[]
        {
            SKColor.Parse("#266489"), SKColor.Parse("#68B9C0"), SKColor.Parse("#90D585"),
            SKColor.Parse("#F3C151"), SKColor.Parse("#F37F64"), SKColor.Parse("#424856"),
            SKColor.Parse("#8F97A4"), SKColor.Parse("#DAC096"), SKColor.Parse("#76846E"),
            SKColor.Parse("#DABFAF")
        };

        int colorIndex = 0;

        foreach (var item in groupedData)
        {
            // Вычисляем процент
            float percentage = (float)(item.Total / grandTotal);

            // Выбираем цвет (циклично, если категорий больше, чем цветов)
            var skColor = colors[colorIndex % colors.Length];
            var mauiColor = Color.FromRgb(skColor.Red, skColor.Green, skColor.Blue);
            colorIndex++;

            // 3. Создаем запись для Диаграммы
            // Label - название, ValueLabel - то, что будет написано на/рядом с диаграммой
            var entry = new ChartEntry((float)item.Total)
            {
                Label = item.Name,
                ValueLabel = percentage.ToString("P0"), // Вывод процентов (напр. 25%)
                Color = skColor,
                TextColor = skColor
            };
            chartEntries.Add(entry);

            // 4. Создаем запись для Списка снизу
            summaryList.Add(new ChartsForm
            {
                Name = item.Name,
                TotalAmount = item.Total,
                Percentage = percentage,
                CategoryColor = mauiColor
            });
        }

        // 5. Настраиваем сам чарт
        ExpensesChart.Chart = new PieChart
        {
            Entries = chartEntries,
            LabelTextSize = 30,
            HoleRadius = 0,
            GraphPosition = GraphPosition.Center, // Центрируем, так как места теперь больше
            LabelMode = LabelMode.None          // 
        };

        // 6. Заполняем список снизу
        CategoriesList.ItemsSource = summaryList;
    }
}