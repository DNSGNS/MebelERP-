using System;
using System.Collections.Generic;
using System.Text;

namespace MyApp1;

public class MonthlyReportItem
{
    // Поля, приходящие из API
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal TotalSales { get; set; }
    public decimal Realization { get; set; }
    public decimal TotalExpenses { get; set; }

    // --- НОВЫЕ ВЫЧИСЛЯЕМЫЕ ПОЛЯ ---

    // Чистая прибыль: Реализация (деньги в кассе) минус Расходы
    public decimal NetProfit => Realization - TotalExpenses;

    // Маржинальность: Продажи делить на Чистую прибыль
    // Добавляем проверку на 0, чтобы приложение не упало при делении
    public decimal Margin
    {
        get
        {
            if (TotalSales == 0) return 0;
            if (NetProfit == 0) return 0;
            return (NetProfit / TotalSales)*100;
        }
    }

    // Дополнительно: Процент рентабельности (часто полезнее в отчетах)
    // (Чистая прибыль / Продажи) * 100
    public decimal ProfitabilityPercent => TotalSales == 0 ? 0 : (NetProfit / TotalSales) * 100;

    // Свойство для удобного отображения в UI (цвет текста для прибыли)
    public Color ProfitColor => NetProfit >= 0 ? Colors.Green : Colors.Red;
}