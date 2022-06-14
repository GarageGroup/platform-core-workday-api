using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using PrimeFuncPack;

namespace GGroupp.Platform.Core.Workday.Get.Tests;

using IWorkdayGetFunc = IAsyncValueFunc<WorkdayGetIn, Result<WorkdayGetOut, Failure<WorkdayGetFailureCode>>>;

public sealed partial class WorkdayGetFuncTest
{
    private const string WorkdayCode = "0";

    private const string DayOffCode = "1";

    private const string NotFoundCode = "101";

    private static IWorkdayGetFunc CreateFunc(HttpMessageHandler httpMessageHandler)
        =>
        Dependency.Of(httpMessageHandler).UseWorkdayGetApi().Resolve(Mock.Of<IServiceProvider>());

    private static Mock<IAsyncFunc<HttpRequestMessage, HttpResponseMessage>> CreateMockProxyHandler(
        HttpResponseMessage responseMessage, Action<HttpRequestMessage>? callback = default)
    {
        var mock = new Mock<IAsyncFunc<HttpRequestMessage, HttpResponseMessage>>();

        var m = mock
            .Setup(p => p.InvokeAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(responseMessage));

        if (callback is not null)
        {
            _ = m.Callback<HttpRequestMessage, CancellationToken>(
                (r, _) => callback.Invoke(r));
        }

        return mock;
    }
}