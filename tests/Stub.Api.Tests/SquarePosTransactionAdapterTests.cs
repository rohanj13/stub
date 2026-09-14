using System.Net;
using Microsoft.Extensions.Options;
using Square;
using Stub.Infrastructure.Providers.Square;

namespace Stub.Api.Tests;

public class SquarePosTransactionAdapterTests
{
    [Fact]
    public async Task FetchTransactionAsync_UsesSquareSdkAndReturnsSerializedPayment()
    {
        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"payment":{"id":"txn-123"}}""")
            });

        var squareClient = new SquareClient(
            "test-token",
            new ClientOptions
            {
                BaseUrl = "https://connect.squareupsandbox.com",
                HttpClient = new HttpClient(handler)
            });

        var options = Options.Create(new SquareOptions
        {
            BaseUrl = "https://connect.squareupsandbox.com",
            AccessToken = "test-token"
        });

        var adapter = new SquarePosTransactionAdapter(squareClient, options);

        var payload = await adapter.FetchTransactionAsync("txn-123");

        Assert.Contains("\"id\":\"txn-123\"", payload);
        Assert.Equal("/v2/payments/txn-123", handler.LastRequest?.RequestUri?.AbsolutePath);
        Assert.Equal("Bearer", handler.LastRequest?.Headers.Authorization?.Scheme);
        Assert.Equal("test-token", handler.LastRequest?.Headers.Authorization?.Parameter);
    }

    [Fact]
    public async Task FetchTransactionAsync_ThrowsWhenAccessTokenMissing()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var squareClient = new SquareClient("unused", new ClientOptions { HttpClient = new HttpClient(handler) });
        var options = Options.Create(new SquareOptions { AccessToken = "" });
        var adapter = new SquarePosTransactionAdapter(squareClient, options);

        await Assert.ThrowsAsync<InvalidOperationException>(() => adapter.FetchTransactionAsync("txn-123"));
        Assert.Null(handler.LastRequest);
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder = responder;
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(_responder(request));
        }
    }
}
