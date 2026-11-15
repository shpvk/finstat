using System.Net;
using System.Net.Http.Json;
using System.Text;
using Finstat.Application;

namespace Finstat.Tests;

public class CategoryApiTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static string Unique(string prefix = "Cat") => $"{prefix}-{Guid.NewGuid():N}"[..30];

    private static async Task<CategoryDto> CreateAsync(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/api/categories", new { name }, Ct);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<CategoryDto>(Ct))!;
    }

    private static async Task<List<CategoryDto>> ListAsync(HttpClient client, bool includeArchived = false) =>
        (await client.GetFromJsonAsync<List<CategoryDto>>($"/api/categories?includeArchived={includeArchived}", Ct))!;

    private static void AssertProblem(HttpResponseMessage response, HttpStatusCode status)
    {
        Assert.Equal(status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Seed_categories_are_present_and_active()
    {
        using var client = api.CreateClient();
        var names = (await ListAsync(client)).Select(c => c.Name).ToList();
        Assert.Contains("Їжа", names);
        Assert.Contains("Комунальні послуги", names);
    }

    [Fact]
    public async Task Created_category_appears_in_list_with_location()
    {
        using var client = api.CreateClient();
        var name = Unique();
        var response = await client.PostAsJsonAsync("/api/categories", new { name = $"  {name}  " }, Ct);
        var created = (await response.Content.ReadFromJsonAsync<CategoryDto>(Ct))!;

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.EndsWith($"/api/categories/{created.Id}", response.Headers.Location?.ToString());
        Assert.Equal(name, created.Name);
        Assert.Contains(await ListAsync(client), c => c.Id == created.Id && !c.IsArchived);
    }

    [Fact]
    public async Task Duplicate_name_differing_by_case_and_spaces_is_rejected()
    {
        using var client = api.CreateClient();
        var name = Unique("Dup");
        await CreateAsync(client, name);

        var response = await client.PostAsJsonAsync("/api/categories", new { name = $" {name.ToUpperInvariant()} " }, Ct);

        AssertProblem(response, HttpStatusCode.BadRequest);
        Assert.Contains("вже існує", await response.Content.ReadAsStringAsync(Ct));
    }

    [Fact]
    public async Task Duplicate_of_seed_cyrillic_name_is_rejected()
    {
        using var client = api.CreateClient();
        var response = await client.PostAsJsonAsync("/api/categories", new { name = "їЖА" }, Ct);
        AssertProblem(response, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Duplicate_of_archived_category_name_is_rejected()
    {
        using var client = api.CreateClient();
        var category = await CreateAsync(client, Unique("Arch"));
        await client.PostAsync($"/api/categories/{category.Id}/archive", null, Ct);

        var response = await client.PostAsJsonAsync("/api/categories", new { name = category.Name }, Ct);

        AssertProblem(response, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Rename_to_own_name_with_different_case_is_allowed()
    {
        using var client = api.CreateClient();
        var category = await CreateAsync(client, Unique("lower"));

        var response = await client.PutAsJsonAsync($"/api/categories/{category.Id}", new { name = category.Name.ToUpperInvariant() }, Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var renamed = (await response.Content.ReadFromJsonAsync<CategoryDto>(Ct))!;
        Assert.Equal(category.Name.ToUpperInvariant(), renamed.Name);
    }

    [Fact]
    public async Task Rename_to_another_existing_name_is_rejected()
    {
        using var client = api.CreateClient();
        var first = await CreateAsync(client, Unique("A"));
        var second = await CreateAsync(client, Unique("B"));

        var response = await client.PutAsJsonAsync($"/api/categories/{second.Id}", new { name = first.Name }, Ct);

        AssertProblem(response, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Renaming_archived_category_is_allowed()
    {
        using var client = api.CreateClient();
        var category = await CreateAsync(client, Unique("Old"));
        await client.PostAsync($"/api/categories/{category.Id}/archive", null, Ct);
        var newName = Unique("New");

        var response = await client.PutAsJsonAsync($"/api/categories/{category.Id}", new { name = newName }, Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var renamed = (await response.Content.ReadFromJsonAsync<CategoryDto>(Ct))!;
        Assert.Equal(newName, renamed.Name);
        Assert.True(renamed.IsArchived);
    }

    [Fact]
    public async Task Archived_category_is_hidden_by_default_and_listed_with_flag()
    {
        using var client = api.CreateClient();
        var category = await CreateAsync(client, Unique("Hide"));

        var response = await client.PostAsync($"/api/categories/{category.Id}/archive", null, Ct);
        var again = await client.PostAsync($"/api/categories/{category.Id}/archive", null, Ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, again.StatusCode);
        Assert.DoesNotContain(await ListAsync(client), c => c.Id == category.Id);
        Assert.Contains(await ListAsync(client, true), c => c.Id == category.Id && c.IsArchived);
    }

    [Fact]
    public async Task List_is_ordered_by_name()
    {
        using var client = api.CreateClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var second = await CreateAsync(client, $"bbb-{suffix}");
        var first = await CreateAsync(client, $"aaa-{suffix}");

        var ids = (await ListAsync(client)).Select(c => c.Id).ToList();

        Assert.True(ids.IndexOf(first.Id) < ids.IndexOf(second.Id));
    }

    [Fact]
    public async Task Unknown_id_returns_404_problem_for_rename_and_archive()
    {
        using var client = api.CreateClient();
        var id = Guid.NewGuid();

        AssertProblem(await client.PutAsJsonAsync($"/api/categories/{id}", new { name = Unique() }, Ct), HttpStatusCode.NotFound);
        AssertProblem(await client.PostAsync($"/api/categories/{id}/archive", null, Ct), HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"name\":null}")]
    [InlineData("{\"name\":\"   \"}")]
    public async Task Missing_or_blank_name_is_rejected(string json)
    {
        using var client = api.CreateClient();
        var existing = await CreateAsync(client, Unique());

        AssertProblem(await client.PostAsync("/api/categories", Json(json), Ct), HttpStatusCode.BadRequest);
        AssertProblem(await client.PutAsync($"/api/categories/{existing.Id}", Json(json), Ct), HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Too_long_name_is_rejected()
    {
        using var client = api.CreateClient();
        var response = await client.PostAsJsonAsync("/api/categories", new { name = new string('x', 51) }, Ct);
        AssertProblem(response, HttpStatusCode.BadRequest);
    }

    private static StringContent Json(string json) => new(json, Encoding.UTF8, "application/json");
}
