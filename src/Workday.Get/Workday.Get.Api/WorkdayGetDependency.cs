using System;
using System.Net.Http;
using PrimeFuncPack;

namespace GGroupp.Platform;

using IWorkdayGetFunc = IAsyncValueFunc<WorkdayGetIn, Result<WorkdayGetOut, Failure<WorkdayGetFailureCode>>>;

public static class WorkdayGetDependency
{
    public static Dependency<IWorkdayGetFunc> UseWorkdayGetApi(this Dependency<HttpMessageHandler> dependency)
    {
        _ = dependency ?? throw new ArgumentNullException(nameof(dependency));

        return dependency.Map<IWorkdayGetFunc>(WorkdayGetFunc.Create);
    }
}