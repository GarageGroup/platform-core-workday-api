namespace GGroupp.Platform;

public readonly record struct WorkdayGetOut
{
    public WorkdayGetOut(WorkdayStatus status) => Status = status;

    public WorkdayStatus Status { get; }
}