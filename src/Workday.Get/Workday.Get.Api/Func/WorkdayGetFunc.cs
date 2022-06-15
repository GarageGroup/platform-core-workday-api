using System;
using System.Net.Http;

namespace GGroupp.Platform;

using IWorkdayGetFunc = IAsyncValueFunc<WorkdayGetIn, Result<WorkdayGetOut, Failure<WorkdayGetFailureCode>>>;

internal sealed partial class WorkdayGetFunc : IWorkdayGetFunc
{
    private const string BaseAddressUrl = "https://isdayoff.ru/";

    private const int UnknownWorkdayCode = 101;

    private const int WorkdayCode = 0;

    private const int DayOffCode = 1;

    public static WorkdayGetFunc Create(HttpMessageHandler httpMessageHandler)
        =>
        new(
            httpMessageHandler ?? throw new ArgumentNullException(nameof(httpMessageHandler)));

    private readonly HttpMessageHandler httpMessageHandler;

    private WorkdayGetFunc(HttpMessageHandler httpMessageHandler)
        =>
        this.httpMessageHandler = httpMessageHandler;
}