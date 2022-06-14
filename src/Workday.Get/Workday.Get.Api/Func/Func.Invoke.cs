using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Platform;

partial class WorkdayGetFunc
{
    public ValueTask<Result<WorkdayGetOut, Failure<WorkdayGetFailureCode>>> InvokeAsync(
        WorkdayGetIn input, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<Result<WorkdayGetOut, Failure<WorkdayGetFailureCode>>>(cancellationToken);
        }

        return InnerInvokeAsync(input, cancellationToken);
    }

    private async ValueTask<Result<WorkdayGetOut, Failure<WorkdayGetFailureCode>>> InnerInvokeAsync(
        WorkdayGetIn input, CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient(httpMessageHandler, disposeHandler: false)
        {
            BaseAddress = new(BaseAddressUrl)
        };

        var response = await httpClient.GetAsync(input.Date.ToString("yyyyMMdd"), cancellationToken).ConfigureAwait(false);

        var content = await ReadContentAsync(response, cancellationToken).ConfigureAwait(false);
        var code = ParseCodeOrNull(content);

        if (response.IsSuccessStatusCode is false && response.StatusCode is HttpStatusCode.NotFound && code is UnknownWorkdayCode)
        {
            return Failure.Create(WorkdayGetFailureCode.UnknownWorkdayStatus, $"Workday {input.Date} information was not found");
        }

        if (response.IsSuccessStatusCode is false)
        {
            return CreateUnexpectedResponseMessageFailureCode();
        }

        return code switch
        {
            WorkdayCode => new WorkdayGetOut(WorkdayStatus.Workday),
            DayOffCode => new WorkdayGetOut(WorkdayStatus.DayOff),
            _ => CreateUnexpectedResponseMessageFailureCode()
        };

        Failure<WorkdayGetFailureCode> CreateUnexpectedResponseMessageFailureCode()
            =>
            new(WorkdayGetFailureCode.Unknown, $"An unexpected message was received: {content}");
    }

    private static async Task<string> ReadContentAsync(HttpResponseMessage responseMessage, CancellationToken cancellationToken)
    {
        if (responseMessage.Content is null)
        {
            return string.Empty;
        }

        var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return content ?? string.Empty;
    }

    private static int? ParseCodeOrNull(string content)
        =>
        int.TryParse(content, NumberStyles.Integer, CultureInfo.InvariantCulture, out var code) ? code : null;
}