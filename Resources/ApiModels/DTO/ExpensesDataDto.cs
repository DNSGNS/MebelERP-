using System;
using System.Collections.Generic;
using System.Text;

namespace MyApp1;

    public class ExpensesDataDto
    {
        // Список всех трат
        public List<Expense> Expenses { get; set; } = new();

        // Список доступных категорий (для выпадающего списка)
        public List<ExpenseCategory> Categories { get; set; } = new();
    }
