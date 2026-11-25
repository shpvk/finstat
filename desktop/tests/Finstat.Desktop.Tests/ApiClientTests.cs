using System.Net;
using Finstat.Desktop.Core.Api;

namespace Finstat.Desktop.Tests;

public class ApiClientTests
{
    [Fact]
    public async Task Problem_details_detail_is_shown_to_the_user()
    {
        var api = new FakeApi()
            .On("POST", "/api/categories", request => FakeApi.Problem("Категорія з такою назвою вже існує"));

        var exception = await Assert.ThrowsAsync<ApiException>(
            () => api.CreateClient().CreateCategoryAsync("їжа", CancellationToken.None));

        Assert.Equal(400, exception.Status);
        Assert.Equal("Категорія з такою назвою вже існує", exception.Message);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.GatewayTimeout)]
    public async Task Unavailable_server_is_explained(HttpStatusCode status)
    {
        var api = new FakeApi()
            .On("GET", "/api/categories", request => new HttpResponseMessage(status));

        var exception = await Assert.ThrowsAsync<ApiException>(
            () => api.CreateClient().ListCategoriesAsync(true, CancellationToken.None));

        Assert.Equal(FinstatApiClient.UnavailableMessage, exception.Message);
    }

    [Fact]
    public async Task Unreachable_server_is_reported_with_status_zero()
    {
        var http = new HttpClient { BaseAddress = new Uri("http://127.0.0.1:1/") };
        var client = new FinstatApiClient(http);

        var exception = await Assert.ThrowsAsync<ApiException>(
            () => client.ListCategoriesAsync(true, CancellationToken.None));

        Assert.Equal(0, exception.Status);
        Assert.Equal(FinstatApiClient.ConnectionFailedMessage, exception.Message);
    }

    [Fact]
    public async Task Amounts_are_sent_and_received_as_exact_strings()
    {
        var api = new FakeApi()
            .On("POST", "/api/expenses", request => FakeApi.Json(
                "{\"id\":\"7d1c8a52-6b7e-4d0a-9a41-0c2f5e7b1d01\",\"amount\":\"1250.50\",\"categoryId\":\"3f1c8a52-6b7e-4d0a-9a41-0c2f5e7b1d01\",\"date\":\"2026-11-21\",\"comment\":null}",
                HttpStatusCode.Created));

        var created = await api.CreateClient().CreateExpenseAsync(
            new Core.DTOs.ExpenseRequestDto("1250.5", Guid.Parse("3f1c8a52-6b7e-4d0a-9a41-0c2f5e7b1d01"), new DateOnly(2026, 11, 21), null),
            CancellationToken.None);

        Assert.Equal("1250.50", created.Amount);
        Assert.Equal(
            "{\"amount\":\"1250.5\",\"categoryId\":\"3f1c8a52-6b7e-4d0a-9a41-0c2f5e7b1d01\",\"date\":\"2026-11-21\",\"comment\":null}",
            api.Requests[0].Body);
    }
}
