namespace Espada.Tests.Common.Http;

public sealed class DelegateHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

    public DelegateHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler) : this((request, _) => handler(request))
    {
    }

    public DelegateHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _handler = handler;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => _handler(request, cancellationToken);
}