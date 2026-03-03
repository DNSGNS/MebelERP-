using System;
using System.Collections.Generic;
using System.Text;

namespace MyApp1;

public class CompleteProjectRequestDto
{
    public List<string> SelectedInstallers { get; set; } = new();
    // Список всех дат, выбранных для установки
    public List<DateTime> InstallDates { get; set; } = new();
}

