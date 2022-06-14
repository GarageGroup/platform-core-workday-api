using System;

namespace GGroupp.Platform;

public readonly record struct WorkdayGetIn
{
    public WorkdayGetIn(DateOnly date) => Date = date;

    public DateOnly Date { get; }
}