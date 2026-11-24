using System.Net;
using System.Net.Http.Json;
using Finstat.Application;
using Finstat.Infrastructure.Categories;

namespace Finstat.Tests;

public class StatisticsApiTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static readonly Guid Food = CategoryConfiguration.FoodId;

    private static async Task<CategoryDto> NewCategoryAsync(HttpClient client, string prefix)
    {
        var response = await client.PostAsJsonAsync("/api/categories", new { name = $"{prefix}-{Guid.NewGuid():N}"[..30] }, Ct);
        return (await response.Content.ReadFromJsonAsync<CategoryDto>(Ct))!;
    }

    private static async Task<ExpenseDto> CreateAsync(HttpClient client, string amount, Guid categoryId, string date)
    {
        var response = await client.PostAsJsonAsync("/api/expenses", new { amount, categoryId, date }, Ct);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ExpenseDto>(Ct))!;
    }

    private static Task<MonthStatisticsDto> StatsAsync(HttpClient client, string month) =>
        client.GetFromJsonAsync<MonthStatisticsDto>($"/api/statistics?month={month}", Ct)!;

    [Fact]
    public async Task Empty_month_has_zero_total_and_no_categories_or_recent()
    {
        using var client = api.CreateClient();

        var stats = await StatsAsync(client, "2040-01");

        Assert.Equal("2040-01", stats.Month);
        Assert.Equal("0.00", stats.Total);
        Assert.Empty(stats.Categories);
        Assert.Empty(stats.Recent);
    }

    [Fact]
    public async Task Month_boundaries_include_first_and_last_day_and_exclude_neighbours()
    {
        using var client = api.CreateClient();
        await CreateAsync(client, "1", Food, "2040-02-29");
        await CreateAsync(client, "2", Food, "2040-03-01");
        await CreateAsync(client, "4", Food, "2040-03-31");
        await CreateAsync(client, "8", Food, "2040-04-01");

        var stats = await StatsAsync(client, "2040-03");

        Assert.Equal("6.00", stats.Total);
        Assert.Equal(["4.00", "2.00"], stats.Recent.Select(e => e.Amount));
    }

    [Fact]
    public async Task December_ends_at_january_first()
    {
        using var client = api.CreateClient();
        await CreateAsync(client, "3", Food, "2040-12-31");
        await CreateAsync(client, "5", Food, "2041-01-01");

        Assert.Equal("3.00", (await StatsAsync(client, "2040-12")).Total);
    }

    [Fact]
    public async Task Totals_and_shares_keep_kopecks_and_include_archived_category()
    {
        using var client = api.CreateClient();
        var a = await NewCategoryAsync(client, "A");
        var b = await NewCategoryAsync(client, "B");
        var c = await NewCategoryAsync(client, "C");
        await CreateAsync(client, "10.10", a.Id, "2041-02-01");
        await CreateAsync(client, "20.20", a.Id, "2041-02-02");
        await CreateAsync(client, "0.01", b.Id, "2041-02-03");
        await CreateAsync(client, "50.00", c.Id, "2041-02-04");
        await client.PostAsync($"/api/categories/{c.Id}/archive", null, Ct);

        var stats = await StatsAsync(client, "2041-02");

        Assert.Equal("80.31", stats.Total);
        Assert.Equal(
            [(c.Id, true, "50.00", "62.3"), (a.Id, false, "30.30", "37.7"), (b.Id, false, "0.01", "0.0")],
            stats.Categories.Select(x => (x.CategoryId, x.IsArchived, x.Total, x.Share)));
    }

    [Fact]
    public async Task Categories_are_ordered_by_total_descending_then_name()
    {
        using var client = api.CreateClient();
        var small = await NewCategoryAsync(client, "Z");
        var tieB = await NewCategoryAsync(client, "M");
        var tieA = await NewCategoryAsync(client, "B");
        var big = await NewCategoryAsync(client, "Y");
        await CreateAsync(client, "1", small.Id, "2041-03-01");
        await CreateAsync(client, "5", tieB.Id, "2041-03-01");
        await CreateAsync(client, "5", tieA.Id, "2041-03-01");
        await CreateAsync(client, "9", big.Id, "2041-03-01");

        var stats = await StatsAsync(client, "2041-03");

        Assert.Equal([big.Id, tieA.Id, tieB.Id, small.Id], stats.Categories.Select(x => x.CategoryId));
    }

    [Fact]
    public async Task Recent_are_three_latest_by_date_then_creation()
    {
        using var client = api.CreateClient();
        await CreateAsync(client, "1", Food, "2041-04-01");
        var d10a = await CreateAsync(client, "2", Food, "2041-04-10");
        var d10b = await CreateAsync(client, "3", Food, "2041-04-10");
        var d20 = await CreateAsync(client, "4", Food, "2041-04-20");
        await CreateAsync(client, "5", Food, "2041-04-05");

        var stats = await StatsAsync(client, "2041-04");

        Assert.Equal([d20.Id, d10b.Id, d10a.Id], stats.Recent.Select(e => e.Id));
    }

    [Fact]
    public async Task Statistics_recalculate_after_update_and_delete()
    {
        using var client = api.CreateClient();
        var other = await NewCategoryAsync(client, "O");
        var expense = await CreateAsync(client, "10", Food, "2042-05-10");
        var second = await CreateAsync(client, "30", Food, "2042-05-11");
        Assert.Equal("40.00", (await StatsAsync(client, "2042-05")).Total);

        await client.PutAsJsonAsync($"/api/expenses/{expense.Id}", new { amount = "20.50", categoryId = Food, date = "2042-05-10" }, Ct);
        var afterAmount = await StatsAsync(client, "2042-05");
        Assert.Equal("50.50", afterAmount.Total);
        Assert.Equal("20.50", afterAmount.Recent.Single(e => e.Id == expense.Id).Amount);

        await client.PutAsJsonAsync($"/api/expenses/{expense.Id}", new { amount = "20.50", categoryId = other.Id, date = "2042-05-10" }, Ct);
        var afterMove = await StatsAsync(client, "2042-05");
        Assert.Equal([("30.00", "59.4"), ("20.50", "40.6")], afterMove.Categories.Select(x => (x.Total, x.Share)));
        Assert.Equal([Food, other.Id], afterMove.Categories.Select(x => x.CategoryId));

        await client.PutAsJsonAsync($"/api/expenses/{expense.Id}", new { amount = "20.50", categoryId = other.Id, date = "2042-06-01" }, Ct);
        Assert.Equal("30.00", (await StatsAsync(client, "2042-05")).Total);
        Assert.Equal("20.50", (await StatsAsync(client, "2042-06")).Total);

        await client.DeleteAsync($"/api/expenses/{second.Id}", Ct);
        var afterDelete = await StatsAsync(client, "2042-05");
        Assert.Equal("0.00", afterDelete.Total);
        Assert.Empty(afterDelete.Categories);
        Assert.Empty(afterDelete.Recent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("2040")]
    [InlineData("2040-13")]
    [InlineData("2040-3")]
    [InlineData("2040-03-01")]
    [InlineData("9999-12")]
    public async Task Missing_or_invalid_month_is_rejected(string month)
    {
        using var client = api.CreateClient();
        foreach (var url in new[] { $"/api/statistics?month={month}", "/api/statistics" })
        {
            var response = await client.GetAsync(url, Ct);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        }
    }
}
