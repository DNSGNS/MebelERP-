using System;
using System.Collections.Generic;
using System.Text;

namespace MyApp1;

    public class ChartsForm
    {
        public string Name { get; set; }
        public decimal TotalAmount { get; set; }
        public double Percentage { get; set; } // от 0 до 1 (например 0.5 для 50%)
        public Color CategoryColor { get; set; } // Цвет, совпадающий с диаграммой
    }

