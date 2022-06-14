using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace GGroupp.Platform.Core.Workday.Get.Tests;

partial class WorkdayGetFuncTest
{
    [Fact]
    public void InvokeAsync_CancellationTokenIsCanceled_ExpectValueTaskIsCanceled()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(WorkdayCode)
        };

        var mockProxyHandler = CreateMockProxyHandler(response);
        using var httpMessageHandler = new StubHttpMessageHandler(mockProxyHandler.Object);

        var func = CreateFunc(httpMessageHandler);

        var input = new WorkdayGetIn(new(2022, 03, 15));
        var cancellationToken = new CancellationToken(canceled: true);

        var actual = func.InvokeAsync(input, cancellationToken);
        Assert.True(actual.IsCanceled);
    }

    [Theory]
    [InlineData(2022, 01, 17)]
    [InlineData(1, 11, 27)]
    [InlineData(2020, 02, 05)]
    [InlineData(2015, 12, 02)]
    public async Task InvokeAsync_CancellationTokenIsNotCanceled_ExpectCallGetWebServiceOnce(
        int year, int month, int day)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(WorkdayCode)
        };

        var mockProxyHandler = CreateMockProxyHandler(response, IsActualRequest);
        using var httpMessageHandler = new StubHttpMessageHandler(mockProxyHandler.Object);

        var func = CreateFunc(httpMessageHandler);

        var input = new WorkdayGetIn(new(year, month, day));
        var cancellationToken = new CancellationToken(false);

        _ = await func.InvokeAsync(input, cancellationToken);

        mockProxyHandler.Verify(h => h.InvokeAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Once);

        void IsActualRequest(HttpRequestMessage request)
        {
            Assert.Equal(HttpMethod.Get, request.Method);

            var expectedUrl = $"https://isdayoff.ru/{year:0000}{month:00}{day:00}";
            Assert.Equal(expectedUrl, request.RequestUri?.AbsoluteUri);
        }
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, NotFoundCode, WorkdayGetFailureCode.UnknownWorkdayStatus)]
    [InlineData(HttpStatusCode.OK, NotFoundCode, WorkdayGetFailureCode.Unknown)]
    [InlineData(HttpStatusCode.Created, "", WorkdayGetFailureCode.Unknown)]
    [InlineData(HttpStatusCode.NoContent, null, WorkdayGetFailureCode.Unknown)]
    [InlineData(HttpStatusCode.NotFound, WorkdayCode, WorkdayGetFailureCode.Unknown)]
    [InlineData(HttpStatusCode.NotFound, DayOffCode, WorkdayGetFailureCode.Unknown)]
    [InlineData(HttpStatusCode.NotFound, null, WorkdayGetFailureCode.Unknown)]
    [InlineData(HttpStatusCode.BadRequest, NotFoundCode, WorkdayGetFailureCode.Unknown)]
    [InlineData(HttpStatusCode.BadRequest, "", WorkdayGetFailureCode.Unknown)]
    [InlineData(HttpStatusCode.BadRequest, "100", WorkdayGetFailureCode.Unknown)]
    [InlineData(HttpStatusCode.Unauthorized, NotFoundCode, WorkdayGetFailureCode.Unknown)]
    [InlineData(HttpStatusCode.InternalServerError, "Some text", WorkdayGetFailureCode.Unknown)]
    [InlineData(HttpStatusCode.InternalServerError, null, WorkdayGetFailureCode.Unknown)]
    public async Task InvokeAsync_HttpResultIsFailureOrContentIsUnexpected_ExpectFailure(
        HttpStatusCode statusCode, string? content, WorkdayGetFailureCode expectedFailureCode)
    {
        using var response = new HttpResponseMessage(statusCode)
        {
            Content = content is null ? null : new StringContent(content)
        };

        var mockProxyHandler = CreateMockProxyHandler(response);
        using var httpMessageHandler = new StubHttpMessageHandler(mockProxyHandler.Object);

        var func = CreateFunc(httpMessageHandler);
        var input = new WorkdayGetIn(new(2021, 11, 07));

        var actual = await func.InvokeAsync(input, default);
        Assert.True(actual.IsFailure);

        var actualFailureCode = actual.FailureOrThrow().FailureCode;
        Assert.Equal(expectedFailureCode, actualFailureCode);
    }

    [Theory]
    [InlineData(WorkdayCode, WorkdayStatus.Workday)]
    [InlineData(DayOffCode, WorkdayStatus.DayOff)]
    public async Task InvokeAsync_HttpResultIsCorrectSuccess_ExpectSuccess(
        string content, WorkdayStatus expectedWorkdayStatus)
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(content)
        };

        var mockProxyHandler = CreateMockProxyHandler(response);
        using var httpMessageHandler = new StubHttpMessageHandler(mockProxyHandler.Object);

        var func = CreateFunc(httpMessageHandler);
        var input = new WorkdayGetIn(new(2011, 05, 17));

        var actual = await func.InvokeAsync(input, CancellationToken.None);
        var expected = new WorkdayGetOut(expectedWorkdayStatus);

        Assert.Equal(expected, actual);
    }
}