using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Finstat.Desktop.Core.DTOs;

namespace Finstat.Desktop.Core.Api;

public class FinstatApiClient(HttpClient http)
{
    public const string ConnectionFailedMessage = "Не вдалося з’єднатися із сервером.";
    public const string UnavailableMessage = "Сервер зараз недоступний. Перевірте, що застосунок запущено, і спробуйте ще раз.";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
    };

    public async Task<List<CategoryResponseDto>> ListCategoriesAsync(bool includeArchived, CancellationToken ct)
    {
        var path = "api/categories?includeArchived=false";
        if (includeArchived)
            path = "api/categories?includeArchived=true";
        return await SendAsync<List<CategoryResponseDto>>(HttpMethod.Get, path, null, ct);
    }

    public async Task<CategoryResponseDto> CreateCategoryAsync(string name, CancellationToken ct)
    {
        return await SendAsync<CategoryResponseDto>(HttpMethod.Post, "api/categories", new CategoryRequestDto(name), ct);
    }

    public async Task<CategoryResponseDto> RenameCategoryAsync(Guid id, string name, CancellationToken ct)
    {
        return await SendAsync<CategoryResponseDto>(HttpMethod.Put, $"api/categories/{id}", new CategoryRequestDto(name), ct);
    }

    public async Task ArchiveCategoryAsync(Guid id, CancellationToken ct)
    {
        await SendAsync(HttpMethod.Post, $"api/categories/{id}/archive", null, ct);
    }

    public async Task<List<ExpenseResponseDto>> ListExpensesAsync(string month, CancellationToken ct)
    {
        return await SendAsync<List<ExpenseResponseDto>>(HttpMethod.Get, $"api/expenses?month={month}", null, ct);
    }

    public async Task<ExpenseResponseDto> CreateExpenseAsync(ExpenseRequestDto request, CancellationToken ct)
    {
        return await SendAsync<ExpenseResponseDto>(HttpMethod.Post, "api/expenses", request, ct);
    }

    public async Task<ExpenseResponseDto> UpdateExpenseAsync(Guid id, ExpenseRequestDto request, CancellationToken ct)
    {
        return await SendAsync<ExpenseResponseDto>(HttpMethod.Put, $"api/expenses/{id}", request, ct);
    }

    public async Task DeleteExpenseAsync(Guid id, CancellationToken ct)
    {
        await SendAsync(HttpMethod.Delete, $"api/expenses/{id}", null, ct);
    }

    public async Task<MonthStatisticsResponseDto> GetStatisticsAsync(string month, CancellationToken ct)
    {
        return await SendAsync<MonthStatisticsResponseDto>(HttpMethod.Get, $"api/statistics?month={month}", null, ct);
    }

    private async Task<T> SendAsync<T>(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        using var response = await SendAsync(method, path, body, ct);
        try
        {
            var value = await response.Content.ReadFromJsonAsync<T>(Json, ct);
            if (value is null)
                throw new ApiException((int)response.StatusCode, "Сервер повернув некоректну відповідь.");
            return value;
        }
        catch (JsonException)
        {
            throw new ApiException((int)response.StatusCode, "Сервер повернув некоректну відповідь.");
        }
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        var request = new HttpRequestMessage(method, path);
        if (body is not null)
            request.Content = JsonContent.Create(body, body.GetType(), options: Json);

        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(request, ct);
        }
        catch (HttpRequestException)
        {
            throw new ApiException(0, ConnectionFailedMessage);
        }

        if (response.IsSuccessStatusCode)
            return response;

        var status = (int)response.StatusCode;
        var detail = await ReadDetailAsync(response, ct);
        response.Dispose();
        throw new ApiException(status, ErrorMessage(status, detail));
    }

    private static async Task<string?> ReadDetailAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var mediaType = response.Content.Headers.ContentType?.MediaType;
        if (mediaType != "application/problem+json")
            return null;
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>(Json, ct);
            return problem?.Detail;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string ErrorMessage(int status, string? detail)
    {
        if (!string.IsNullOrWhiteSpace(detail))
            return detail;
        if (status == 502 || status == 503 || status == 504)
            return UnavailableMessage;
        return $"Сервер повернув помилку ({status}).";
    }
}
