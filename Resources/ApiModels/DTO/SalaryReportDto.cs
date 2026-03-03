using System;
using System.Collections.Generic;
using System.Text;

namespace MyApp1;

public class SalaryReportDto
{
    public DateTime DatePerformed { get; set; }

    public Guid Id { get; set; }
    public Guid WorkerId { get; set; }
    public string WorkerName { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public MaterialType? Material { get; set; }

    // Работы
    public decimal Saw { get; set; }
    public decimal Edging { get; set; }
    public decimal Additive { get; set; }
    public int DoorCanvas { get; set; }
    public int DoorSectional { get; set; }
    public decimal Packaging { get; set; }
    public decimal Installation { get; set; }
    public decimal GrindingSoap { get; set; }
    public decimal GrindingFrez { get; set; }
    public decimal Milling { get; set; }
    public decimal Additionally { get; set; }

    public decimal Measurement { get; set; }

    public string? Comment { get; set; }
    public decimal TotalSalary { get; set; }
}

public class SalaryReportResponseDto
{
    public List<SalaryReportDto> Reports { get; set; } = new();
    public List<ProjectSimpleDto> Projects { get; set; } = new();

    public List<WorkerSimpleDto> Workers { get; set; } = new();
}


public class WorkerSimpleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

}

public class ProjectSimpleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public decimal TotalProjectPrice { get; set; }
}

public class SalaryMonthReportDto
{
    public Guid WorkManId { get; set; }
    public string WorkManName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}