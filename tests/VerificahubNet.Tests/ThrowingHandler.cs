namespace VerificahubNet.Tests;

/// <summary>A test <see cref="HttpMessageHandler"/> that always throws, to simulate transport failures.</summary>
internal sealed class ThrowingHandler : HttpMessageHandler
{
    private readonly Exception _exception;

    public ThrowingHandler(Exception exception) => _exception = exception;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => throw _exception;
}
