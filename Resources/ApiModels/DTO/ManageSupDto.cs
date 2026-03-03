using System;
using System.Collections.Generic;
using System.Text;

namespace MyApp1;

public class InstallationInfoResponse
{
    public List<Project> Projects { get; set; } = new();
    public List<WorkMans> AllInstallers { get; set; } = new();
    public List<WorkMans> AssignedWorkers { get; set; } = new();
}

public class InstallationUpdateDto
{
    public Guid ProjectId { get; set; }
    public ProjectStatus Status { get; set; }
    public List<DateTime> InstallDates { get; set; } = new();
    public List<Guid> InstallerIds { get; set; } = new();
}
public class MonthlyReportDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal TotalSales { get; set; }
    public decimal Realization { get; set; }
    public decimal TotalExpenses { get; set; }
}