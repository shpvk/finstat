using System.Globalization;
using System.Net;
using System.Text.Json;
using Finstat.Desktop.Core.Api;
using Finstat.Desktop.Core.DTOs;

namespace Finstat.Desktop.Tests;

public class FakeBackend
{
    public static readonly Guid FoodId = Guid.Parse("3f1c8a52-6b7e-4d0a-9a41-0c2f5e7b1d01");
    public static readonly Guid TaxiId = Guid.Parse("3f1c8a52-6b7e-4d0a-9a41-0c2f5e7b1d02");

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public FakeBackend()
    {
        Api = new FakeApi()
            .On("GET", "/api/categories", request => Respond(Categories))
            .On("GET", "/api/expenses", request => Respond(ExpensesOf(MonthOf(request))))
            .On("POST", "/api/expenses", Create)
            .On("GET", "/api/statistics", request => Respond(Statistics(MonthOf(request))));
    }

    public FakeApi Api { get; }

    public List<CategoryResponseDto> Categories { get; } = new()
    {
        new CategoryResponseDto(FoodId, "Їжа", false),
        new CategoryResponseDto(TaxiId, "Таксі", true),
    };

    public List<ExpenseResponseDto> Expenses { get; } = new();

    public FinstatApiClient CreateClient()
    {
        return Api.CreateClient();
    }

    private HttpResponseMessage Create(FakeRequest request)
    {
        var body = JsonSerializer.Deserialize<ExpenseRequestDto>(request.Body!, Json)!;
        var expense = new ExpenseResponseDto(Guid.NewGuid(), Format(Parse(body.Amount)), body.CategoryId, body.Date, body.Comment);
        Expenses.Add(expense);
        return FakeApi.Json(JsonSerializer.Serialize(expense, Json), HttpStatusCode.Created);
    }

    private List<ExpenseResponseDto> ExpensesOf(string month)
    {
        var list = new List<ExpenseResponseDto>();
        foreach (var expense in Expenses)
        {
            if (expense.Date.ToString("yyyy-MM", CultureInfo.InvariantCulture) == month)
                list.Add(expense);
        }
        list.Sort((a, b) => b.Date.CompareTo(a.Date));
        return list;
    }

    private MonthStatisticsResponseDto Statistics(string month)
    {
        var expenses = ExpensesOf(month);
        var total = 0m;
        var byCategory = new Dictionary<Guid, decimal>();
        foreach (var expense in expenses)
        {
            var amount = Parse(expense.Amount);
            total += amount;
            byCategory.TryGetValue(expense.CategoryId, out var sum);
            byCategory[expense.CategoryId] = sum + amount;
        }

        var categories = new List<CategoryStatisticsResponseDto>();
        foreach (var pair in byCategory)
        {
            var category = Categories.Find(c => c.Id == pair.Key)!;
            var share = decimal.Round(pair.Value / total * 100, 1, MidpointRounding.AwayFromZero);
            categories.Add(new CategoryStatisticsResponseDto(category.Id, category.Name, category.IsArchived,
                Format(pair.Value), share.ToString("0.0", CultureInfo.InvariantCulture)));
        }

        var recent = expenses.GetRange(0, Math.Min(3, expenses.Count));
        return new MonthStatisticsResponseDto(month, Format(total), categories, recent);
    }

    private static string MonthOf(FakeRequest request)
    {
        var query = request.PathAndQuery;
        return query.Substring(query.IndexOf("month=", StringComparison.Ordinal) + 6, 7);
    }

    private static decimal Parse(string amount)
    {
        return decimal.Parse(amount, CultureInfo.InvariantCulture);
    }

    private static string Format(decimal amount)
    {
        return amount.ToString("0.00", CultureInfo.InvariantCulture);
    }

    private static HttpResponseMessage Respond<T>(T value)
    {
        return FakeApi.Json(JsonSerializer.Serialize(value, Json));
    }
}
