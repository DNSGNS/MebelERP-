namespace MyApp1;

public class TakeTaskRequest
{
    public bool IsTaken { get; set; }
    public string WorkerName { get; set; } = string.Empty;
}

public class UpdateStatusRequest
{
    public ProductionTaskStatus NewStatus { get; set; }

    public string WorkerName { get; set; } = string.Empty;
}