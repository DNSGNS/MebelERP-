
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace MyApp1;


public class SalaryCell
{
    public string Text { get; set; }
    public decimal Value { get; set; } // Числовое значение (для суммы)
    public bool IsHeader { get; set; }
    public bool IsWeek { get; set; } // Это неделя или итог месяца?
    public int MonthIndex { get; set; } // 1-12
    public double Width { get; set; } = 80;
    public Color BgColor { get; set; } = Colors.Transparent;
    public Color TextColor { get; set; } = Colors.Black;
    public FontAttributes FontAttr { get; set; } = FontAttributes.None;

    // Команда для заголовка (раскрыть/скрыть)
    public ICommand TapCommand { get; set; }
}

// Класс для строки таблицы (Один работник)
public class SalaryRow
{
    public string WorkerName { get; set; }
    public ObservableCollection<SalaryCell> Cells { get; set; } = new();
}
public class SalaryMonthlyForm : INotifyPropertyChanged
{


    private readonly ApiService _apiService;
    private bool _isBusy;
    private int _selectedYear = DateTime.Now.Year;



    // Храним "сырые" данные, чтобы не дергать API при сворачивании колонок
    private List<SalaryMonthReportDto> _allData = new();

    // Храним индексы развернутых месяцев
    private HashSet<int> _expandedMonths = new();

    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    public List<int> Years { get; set; } = new();

    public int SelectedYear
    {
        get => _selectedYear;
        set
        {
            if (_selectedYear != value)
            {
                _selectedYear = value;
                OnPropertyChanged();
                // Запускаем загрузку без await, так как это сеттер, 
                // но внутри LoadDataAsync уже есть обработка IsBusy
                _ = LoadDataAsync();
            }
        }
    }
    // Заголовки колонок (Январь, Февраль... + Недели)
    public ObservableCollection<SalaryCell> HeaderCells { get; set; } = new();

    // Строки таблицы
    public ObservableCollection<SalaryRow> Rows { get; set; } = new();

    public ICommand ToggleMonthCommand { get; }

    public SalaryMonthlyForm()
    {
        _apiService = new ApiService();
        ToggleMonthCommand = new Command<int>(OnToggleMonth);
        

        // Генерируем список годов (например, текущий -2 года ... текущий + 1 год)
        int current = DateTime.Now.Year;
        for (int i = current - 2; i <= current + 1; i++)
        {
            Years.Add(i);
        }
    }



    public async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        _allData.Clear();

        try
        {
            // Загружаем данные параллельно за 12 месяцев
            var tasks = new List<Task<List<SalaryMonthReportDto>>>();
            for (int m = 1; m <= 12; m++)
            {
                tasks.Add(_apiService.GetMonthlySalariesAsync(_selectedYear, m));
            }

            var results = await Task.WhenAll(tasks);

            foreach (var list in results)
            {
                if (list != null) _allData.AddRange(list);
            }

            RebuildTable();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnToggleMonth(int month)
    {
        if (_expandedMonths.Contains(month))
            _expandedMonths.Remove(month);
        else
            _expandedMonths.Add(month);

        RebuildTable();
    }

    private void RebuildTable()
    {
        HeaderCells.Clear();
        Rows.Clear();
        string[] monthNames = { "Янв", "Фев", "Мар", "Апр", "Май", "Июн", "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек" };

        for (int m = 1; m <= 12; m++)
        {
            bool isExpanded = _expandedMonths.Contains(m);

            HeaderCells.Add(new SalaryCell
            {
                Text = monthNames[m - 1],
                IsHeader = true,
                MonthIndex = m,
                Width = 70,
                // СТИЛЬ КАК В ФАЙЛЕ: Темно-синий фон, Белый текст
                // Если раскрыт - делаем фиолетовым (#6750A4), чтобы выделялся, или оставляем синим
                BgColor = isExpanded ? Color.FromArgb("#6750A4") : Color.FromArgb("#2B3C51"),
                TextColor = Colors.White,
                FontAttr = FontAttributes.Bold,
                TapCommand = ToggleMonthCommand
            });

            if (isExpanded)
            {
                for (int w = 1; w <= 4; w++)
                {
                    string weekText = w == 4 ? "22+" : $"{1 + (w - 1) * 7}-{(w) * 7}";
                    HeaderCells.Add(new SalaryCell
                    {
                        Text = weekText,
                        IsHeader = true,
                        IsWeek = true,
                        Width = 50,
                        BgColor = Color.FromArgb("#E0E0E0"), // Светло-серый для подзаголовков недель
                        TextColor = Color.FromArgb("#2B3C51"),
                        FontAttr = FontAttributes.None,
                        MonthIndex = m,
                        TapCommand = ToggleMonthCommand
                    });
                }
            }
        }

        // 2. Группируем данные по работникам
        var groupedByWorker = _allData
            .GroupBy(x => new { x.WorkManId, x.WorkManName })
            .OrderBy(g => g.Key.WorkManName)
            .ToList();

        // 3. Формируем строки
        foreach (var workerGroup in groupedByWorker)
        {
            var row = new SalaryRow { WorkerName = workerGroup.Key.WorkManName };

            for (int m = 1; m <= 12; m++)
            {
                bool isExpanded = _expandedMonths.Contains(m);

                // Данные за этот месяц
                var monthData = workerGroup.Where(x => x.Date.Month == m).ToList();
                decimal monthTotal = monthData.Sum(x => x.Amount);

                // Ячейка "Итог месяца"
                row.Cells.Add(new SalaryCell
                {
                    Text = monthTotal == 0 ? "-" : monthTotal.ToString("N0"),
                    Value = monthTotal,
                    Width = 70,
                    BgColor = isExpanded ? Color.FromArgb("#F3E5F5") : Colors.White, // Подсветка открытого
                    TextColor = monthTotal > 0 ? Colors.Black : Colors.LightGray,
                    FontAttr = FontAttributes.Bold
                });

                // Ячейки "Недели", если раскрыто
                if (isExpanded)
                {
                    // Разбиваем на 4 интервала
                    // 1-8, 9-15, 16-22, 23+ (как вы просили: 1-8 это первая, дальше по 7)
                    // Или просто по неделям: 1-7, 8-14, 15-21, 22+

                    var w1 = monthData.Where(x => x.Date.Day >= 1 && x.Date.Day <= 8).Sum(x => x.Amount);
                    var w2 = monthData.Where(x => x.Date.Day >= 9 && x.Date.Day <= 15).Sum(x => x.Amount);
                    var w3 = monthData.Where(x => x.Date.Day >= 16 && x.Date.Day <= 22).Sum(x => x.Amount);
                    var w4 = monthData.Where(x => x.Date.Day >= 23).Sum(x => x.Amount);

                    decimal[] weeks = { w1, w2, w3, w4 };

                    foreach (var wVal in weeks)
                    {
                        row.Cells.Add(new SalaryCell
                        {
                            Text = wVal == 0 ? "" : wVal.ToString("N0"), // Пусто если 0, чтобы не засорять
                            Value = wVal,
                            Width = 50,
                            BgColor = Colors.White,
                            TextColor = Colors.DimGray,
                            FontAttr = FontAttributes.None
                        });
                    }
                }
            }
            Rows.Add(row);
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
