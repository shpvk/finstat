using System.Net;
using System.Net.Http.Json;
using Finstat.Application;
using Finstat.Infrastructure.Categories;

namespace Finstat.Tests;

public class ExpenseApiTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static readonly Guid Food = CategoryConfiguration.FoodId;

    private static async Task<CategoryDto> NewCategoryAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/categories", new { name = $"E-{Guid.NewGuid():N}"[..30] }, Ct);
        return (await response.Content.ReadFromJsonAsync<CategoryDto>(Ct))!;
    }

    private static async Task<CategoryDto> NewArchivedCategoryAsync(HttpClient client)
    {
        var category = await NewCategoryAsync(client);
        await client.PostAsync($"/api/categories/{category.Id}/archive", null, Ct);
        return category;
    }

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, string amount, Guid categoryId, string date, string? comment = null) =>
        client.PostAsJsonAsync("/api/expenses", new { amount, categoryId, date, comment }, Ct);

    private static async Task<ExpenseDto> CreateAsync(HttpClient client, string amount, Guid categoryId, string date, string? comment = null)
    {
        var response = await PostAsync(client, amount, categoryId, date, comment);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ExpenseDto>(Ct))!;
    }

    private static async Task<List<ExpenseDto>> ListAsync(HttpClient client, string month) =>
        (await client.GetFromJsonAsync<List<ExpenseDto>>($"/api/expenses?month={month}", Ct))!;

    private static void AssertProblem(HttpResponseMessage response, HttpStatusCode status)
    {
        Assert.Equal(status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData("0.01", "0.01")]
    [InlineData("12.50", "12.50")]
    [InlineData("12.5", "12.50")]
    [InlineData("1234567.89", "1234567.89")]
    public async Task Amount_round_trips_exactly_as_string(string sent, string expected)
    {
        using var client = api.CreateClient();
        var created = await CreateAsync(client, sent, Food, "2031-03-10");

        Assert.Equal(expected, created.Amount);
        Assert.Equal(expected, (await ListAsync(client, "2031-03")).Single(e => e.Id == created.Id).Amount);
    }

    [Fact]
    public async Task Create_returns_location_and_stores_trimmed_comment_and_date()
    {
        using var client = api.CreateClient();
        var response = await PostAsync(client, "5", Food, "2031-04-05", "  обід  ");
        var created = (await response.Content.ReadFromJsonAsync<ExpenseDto>(Ct))!;

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.EndsWith($"/api/expenses/{created.Id}", response.Headers.Location?.ToString());
        Assert.Equal("обід", created.Comment);
        Assert.Equal(new DateOnly(2031, 4, 5), created.Date);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("1.005")]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData("10000000000")]
    public async Task Invalid_amount_is_rejected(string amount)
    {
        using var client = api.CreateClient();
        AssertProblem(await PostAsync(client, amount, Food, "2031-05-01"), HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Invalid_update_amount_leaves_expense_unchanged()
    {
        using var client = api.CreateClient();
        var created = await CreateAsync(client, "10", Food, "2031-06-01");

        var response = await client.PutAsJsonAsync($"/api/expenses/{created.Id}", new { amount = "1.234", categoryId = Food, date = "2031-06-01" }, Ct);

        AssertProblem(response, HttpStatusCode.BadRequest);
        Assert.Equal("10.00", (await ListAsync(client, "2031-06")).Single(e => e.Id == created.Id).Amount);
    }

    [Fact]
    public async Task Month_filter_includes_first_and_last_day_and_excludes_neighbours()
    {
        using var client = api.CreateClient();
        var prevLast = await CreateAsync(client, "1", Food, "2032-01-31");
        var first = await CreateAsync(client, "2", Food, "2032-02-01");
        var last = await CreateAsync(client, "3", Food, "2032-02-29");
        var nextFirst = await CreateAsync(client, "4", Food, "2032-03-01");

        var ids = (await ListAsync(client, "2032-02")).Select(e => e.Id).ToList();

        Assert.Equal([last.Id, first.Id], ids);
        Assert.DoesNotContain(prevLast.Id, ids);
        Assert.DoesNotContain(nextFirst.Id, ids);
    }

    [Fact]
    public async Task December_month_ends_at_january_first()
    {
        using var client = api.CreateClient();
        var dec = await CreateAsync(client, "1", Food, "2033-12-31");
        var jan = await CreateAsync(client, "2", Food, "2034-01-01");

        var ids = (await ListAsync(client, "2033-12")).Select(e => e.Id).ToList();

        Assert.Contains(dec.Id, ids);
        Assert.DoesNotContain(jan.Id, ids);
    }

    [Fact]
    public async Task List_is_ordered_by_date_then_creation_descending()
    {
        using var client = api.CreateClient();
        var older = await CreateAsync(client, "1", Food, "2035-02-01");
        var sameDayFirst = await CreateAsync(client, "2", Food, "2035-02-10");
        var sameDayLater = await CreateAsync(client, "3", Food, "2035-02-10");

        var ids = (await ListAsync(client, "2035-02")).Select(e => e.Id).ToList();

        Assert.Equal([sameDayLater.Id, sameDayFirst.Id, older.Id], ids);
    }

    [Theory]
    [InlineData("")]
    [InlineData("2032")]
    [InlineData("2032-13")]
    [InlineData("2032-2")]
    [InlineData("2032-02-01")]
    [InlineData("9999-12")]
    public async Task Missing_or_invalid_month_is_rejected(string month)
    {
        using var client = api.CreateClient();
        AssertProblem(await client.GetAsync($"/api/expenses?month={month}", Ct), HttpStatusCode.BadRequest);
        AssertProblem(await client.GetAsync("/api/expenses", Ct), HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Missing_date_and_unknown_category_are_rejected()
    {
        using var client = api.CreateClient();
        AssertProblem(await client.PostAsJsonAsync("/api/expenses", new { amount = "1", categoryId = Food }, Ct), HttpStatusCode.BadRequest);
        var unknown = await PostAsync(client, "1", Guid.NewGuid(), "2032-01-01");
        AssertProblem(unknown, HttpStatusCode.BadRequest);
        Assert.Contains("не знайдено", await unknown.Content.ReadAsStringAsync(Ct));
    }

    [Fact]
    public async Task Past_and_future_dates_are_allowed()
    {
        using var client = api.CreateClient();
        await CreateAsync(client, "1", Food, "1999-01-01");
        await CreateAsync(client, "1", Food, "2999-12-31");
    }

    [Fact]
    public async Task Create_in_archived_category_is_rejected()
    {
        using var client = api.CreateClient();
        var archived = await NewArchivedCategoryAsync(client);

        AssertProblem(await PostAsync(client, "1", archived.Id, "2036-01-01"), HttpStatusCode.BadRequest);
        Assert.Empty(await ListAsync(client, "2036-01"));
    }

    [Fact]
    public async Task Expense_in_archived_category_can_be_edited_in_place()
    {
        using var client = api.CreateClient();
        var category = await NewCategoryAsync(client);
        var created = await CreateAsync(client, "10", category.Id, "2036-02-01");
        await client.PostAsync($"/api/categories/{category.Id}/archive", null, Ct);

        var response = await client.PutAsJsonAsync($"/api/expenses/{created.Id}",
            new { amount = "20.50", categoryId = category.Id, date = "2036-02-15", comment = "змінено" }, Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = (await response.Content.ReadFromJsonAsync<ExpenseDto>(Ct))!;
        Assert.Equal(("20.50", category.Id, new DateOnly(2036, 2, 15), "змінено"), (updated.Amount, updated.CategoryId, updated.Date, updated.Comment));
    }

    [Fact]
    public async Task Expense_can_move_from_archived_to_active_category()
    {
        using var client = api.CreateClient();
        var category = await NewCategoryAsync(client);
        var created = await CreateAsync(client, "10", category.Id, "2036-03-01");
        await client.PostAsync($"/api/categories/{category.Id}/archive", null, Ct);

        var response = await client.PutAsJsonAsync($"/api/expenses/{created.Id}", new { amount = "10", categoryId = Food, date = "2036-03-01" }, Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(Food, (await response.Content.ReadFromJsonAsync<ExpenseDto>(Ct))!.CategoryId);
    }

    [Fact]
    public async Task Expense_cannot_move_into_archived_category()
    {
        using var client = api.CreateClient();
        var archived = await NewArchivedCategoryAsync(client);
        var created = await CreateAsync(client, "10", Food, "2036-04-01");

        var response = await client.PutAsJsonAsync($"/api/expenses/{created.Id}", new { amount = "10", categoryId = archived.Id, date = "2036-04-01" }, Ct);

        AssertProblem(response, HttpStatusCode.BadRequest);
        Assert.Equal(Food, (await ListAsync(client, "2036-04")).Single(e => e.Id == created.Id).CategoryId);
    }

    [Fact]
    public async Task Update_can_clear_comment_and_move_date_to_another_month()
    {
        using var client = api.CreateClient();
        var created = await CreateAsync(client, "10", Food, "2036-05-01", "x");

        await client.PutAsJsonAsync($"/api/expenses/{created.Id}", new { amount = "10", categoryId = Food, date = "2036-06-01" }, Ct);

        Assert.Empty(await ListAsync(client, "2036-05"));
        Assert.Null((await ListAsync(client, "2036-06")).Single(e => e.Id == created.Id).Comment);
    }

    [Fact]
    public async Task Delete_removes_expense_and_second_delete_is_404()
    {
        using var client = api.CreateClient();
        var created = await CreateAsync(client, "1", Food, "2037-01-01");

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/expenses/{created.Id}", Ct)).StatusCode);
        Assert.Empty(await ListAsync(client, "2037-01"));
        AssertProblem(await client.DeleteAsync($"/api/expenses/{created.Id}", Ct), HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Unknown_id_returns_404_for_update_and_delete()
    {
        using var client = api.CreateClient();
        var id = Guid.NewGuid();

        AssertProblem(await client.PutAsJsonAsync($"/api/expenses/{id}", new { amount = "1", categoryId = Food, date = "2037-01-01" }, Ct), HttpStatusCode.NotFound);
        AssertProblem(await client.DeleteAsync($"/api/expenses/{id}", Ct), HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("{\"amount\":\"1\",\"categoryId\":\"c0000000-0000-0000-0000-000000000000\",\"date\":\"2031-02-30\"}")]
    [InlineData("{\"amount\":\"1\",\"categoryId\":\"not-a-guid\",\"date\":\"2031-02-10\"}")]
    [InlineData("{\"amount\":12.5,\"categoryId\":\"c0000000-0000-0000-0000-000000000000\",\"date\":\"2031-02-10\"}")]
    public async Task Malformed_body_returns_400_problem(string json)
    {
        using var client = api.CreateClient();
        var body = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        AssertProblem(await client.PostAsync("/api/expenses", body, Ct), HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Wrong_content_type_returns_415()
    {
        using var client = api.CreateClient();
        var body = new StringContent("x", System.Text.Encoding.UTF8, "text/plain");
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, (await client.PostAsync("/api/expenses", body, Ct)).StatusCode);
    }

    [Fact]
    public async Task Invalid_query_parameter_returns_400_problem()
    {
        using var client = api.CreateClient();
        AssertProblem(await client.GetAsync("/api/categories?includeArchived=abc", Ct), HttpStatusCode.BadRequest);
    }
}
