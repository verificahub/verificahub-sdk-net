using System.Net;
using System.Text;

namespace VerificahubNet.Tests;

/// <summary>
/// A test <see cref="HttpMessageHandler"/> that records the outgoing request and returns a
/// canned response body + status.
/// </summary>
internal sealed class RecordingHandler : HttpMessageHandler
{
    private readonly string _responseBody;
    private readonly HttpStatusCode _statusCode;

    public RecordingHandler(string responseBody, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _responseBody = responseBody;
        _statusCode = statusCode;
    }

    public HttpMethod? Method { get; private set; }

    public Uri? RequestUri { get; private set; }

    public string? Body { get; private set; }

    public string? AuthorizationHeader { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Method = request.Method;
        RequestUri = request.RequestUri;
        AuthorizationHeader = request.Headers.Authorization?.ToString();
        if (request.Content != null)
        {
            Body = await request.Content.ReadAsStringAsync(cancellationToken);
        }

        return new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_responseBody, Encoding.UTF8, "application/json")
        };
    }
}
