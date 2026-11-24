using System.Net;
using System.Text;
using Finstat.Desktop.Core.Api;

namespace Finstat.Desktop.Tests;

public record FakeRequest(HttpMethod Method, string PathAndQuery, string? Body);

public class FakeApi : HttpMessageHandler
{
    private readonly Dictionary<string, Func<FakeRequest, HttpResponseMessage>> routes = new();

    public List<FakeRequest> Requests { get; } = new();

    public FakeApi On(string method, string path, Func<FakeRequest, HttpResponseMessage> respond)
    {
        routes[method + " " + path] = respond;
        return this;
    }

    public FinstatApiClient CreateClient()
    {
        var http = new HttpClient(this) { BaseAddress = new Uri("http://localhost:8080/") };
        return new FinstatApiClient(http);
    }

    public static HttpResponseMessage Json(string json, HttpStatusCode status = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(status)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
    }

    public static HttpResponseMessage Problem(string detail, HttpStatusCode status = HttpStatusCode.BadRequest)
    {
        var json = "{\"title\":\"Порушено бізнес-правило\",\"status\":" + (int)status + ",\"detail\":\"" + detail + "\"}";
        return new HttpResponseMessage(status)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/problem+json"),
        };
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string? body = null;
        if (request.Content is not null)
            body = await request.Content.ReadAsStringAsync(cancellationToken);

        var uri = request.RequestUri!;
        var fake = new FakeRequest(request.Method, uri.PathAndQuery, body);
        Requests.Add(fake);

        if (routes.TryGetValue(request.Method.Method + " " + uri.AbsolutePath, out var respond))
            return respond(fake);
        return new HttpResponseMessage(HttpStatusCode.NotFound);
    }
}
