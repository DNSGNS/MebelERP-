using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace MyApp1;

public partial class SalesChartPage : ContentPage
{
    private readonly List<MonthlyReportItem> _sourceData;
    private const string AllTimeText = "Всё время";

    public ObservableCollection<ISeries> Series { get; set; } = new();
    public Axis[] XAxes { get; set; }
    public Axis[] YAxes { get; set; }

    public SalesChartPage(List<MonthlyReportItem> data)
    {
        InitializeComponent();
        _sourceData = data ?? new List<MonthlyReportItem>();

        SetupPicker();

        // Цвета для оформления
        var axisLabelColor = SKColor.Parse("#616161"); // Мягкий серый для осей
        var gridLineColor = SKColor.Parse("#E0E0E0"); // Очень светлый для сетки

        YAxes = new Axis[]
        {
            new Axis
            {
                Labeler = FormatLargeNumber,
                LabelsPaint = new SolidColorPaint(axisLabelColor),
                SeparatorsPaint = new SolidColorPaint(gridLineColor) { StrokeThickness = 1 },
                MinLimit = 0,


            }
        };

        XAxes = new Axis[]
        {
            new Axis
            {
                LabelsPaint = new SolidColorPaint(axisLabelColor),
                LabelsRotation = -45, // Наклон для мобилок
                Padding = new LiveChartsCore.Drawing.Padding(0, 10, 0, 0)
            }
        };


        BindingContext = this;
        YearPicker.SelectedIndex = 0; // Выберет "Всё время"
    }

    private void SetupPicker()
    {
        var years = _sourceData
            .Select(x => x.Year.ToString())
            .Distinct()
            .OrderByDescending(x => x)
            .ToList();

        var options = new List<string> { AllTimeText };
        options.AddRange(years);
        YearPicker.ItemsSource = options;
    }

    private void OnYearSelected(object sender, EventArgs e)
    {
        if (YearPicker.SelectedItem is string selected)
            UpdateChart(selected);
    }

    private void UpdateChart(string period)
    {
        List<MonthlyReportItem> filtered;
        bool isAllTime = period == AllTimeText;

        if (isAllTime)
        {
            filtered = _sourceData.OrderBy(x => x.Year).ThenBy(x => x.Month).ToList();
            ChartTitle.Text = "Продажи за весь период";
        }
        else
        {
            filtered = _sourceData.Where(x => x.Year.ToString() == period).OrderBy(x => x.Month).ToList();
            ChartTitle.Text = $"Продажи за {period} год";
        }

        if (!filtered.Any()) return;

        bool isScrollRequired = filtered.Count >= 3;

        if (isScrollRequired)
        {
            double calculatedWidth = filtered.Count * 70;
            MainChart.WidthRequest = calculatedWidth;
            ScrollSlider.IsVisible = true;
        }
        else
        {
            // -1 означает "растянуть по родителю"
            MainChart.WidthRequest = -1;
            MainChart.HorizontalOptions = LayoutOptions.Fill;
            ScrollSlider.IsVisible = false;

            MainThread.BeginInvokeOnMainThread(async () => {
                await ChartScroll.ScrollToAsync(0, 0, false);
            });
        }

        // Настраиваем максимум ползунка после того как layout обновится
        if (isScrollRequired)
        {
            MainThread.BeginInvokeOnMainThread(async () => {
                await Task.Delay(100); // ждём layout pass
                double maxScroll = Math.Max(0.1, MainChart.WidthRequest - ChartScroll.Width);
                ScrollSlider.Maximum = maxScroll;
                ScrollSlider.Value = 0;
            });
        }

        // Обновляем данные
        Series.Clear();
        Series.Add(new ColumnSeries<decimal>
        {
            Values = filtered.Select(x => x.TotalSales).ToArray(),
            Name = "Продажи",
            Fill = new SolidColorPaint(SKColor.Parse("#5C6BC0")),
            Rx = 8,
            Ry = 8,
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse("#2B3C51")),
            DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
            DataLabelsSize = 10,
            DataLabelsFormatter = (point) => FormatLargeNumber(point.Coordinate.PrimaryValue)
        });

        XAxes[0].Labels = filtered.Select(x => isAllTime
            ? $"{x.MonthName.Substring(0, 3)} {x.Year.ToString().Substring(2)}"
            : x.MonthName).ToArray();
    }

    // 1. Когда пользователь двигает ползунок — двигаем график
    private void OnSliderValueChanged(object sender, EventArgs e)
    {
        ChartScroll.ScrollToAsync(ScrollSlider.Value, 0, false);
    }

    // 2. Когда пользователь скроллит график мышкой/пальцем — двигаем ползунок
    private void OnChartScrolled(object sender, ScrolledEventArgs e)
    {
        // Чтобы не зацикливать события, обновляем только если разница существенна
        if (Math.Abs(ScrollSlider.Value - e.ScrollX) > 1)
        {
            ScrollSlider.Value = e.ScrollX;
        }
    }

    private string FormatLargeNumber(double value)
    {
        if (value >= 1000000) return (value / 1000000).ToString("N1") + " М";
        if (value >= 1000) return (value / 1000).ToString("N1") + " к";
        return value.ToString("N0");
    }
}