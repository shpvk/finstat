using Finstat.Desktop.Core.ViewModels;

namespace Finstat.Desktop.Tests;

public class ExpenseFlowTests
{
    private static readonly DateOnly Today = new(2026, 11, 24);

    [Fact]
    public async Task A_new_expense_updates_the_list_and_the_overview()
    {
        var backend = new FakeBackend();
        var client = backend.CreateClient();
        var month = new MonthState(() => Today);
        var notifications = new NotificationCenter();
        var overview = new OverviewViewModel(client, month);
        var expenses = new ExpensesViewModel(client, month, notifications);

        await overview.LoadAsync();
        await expenses.LoadAsync();
        Assert.True(overview.IsEmpty);
        Assert.True(expenses.IsEmpty);
        Assert.Equal(new[] { "Їжа" }, OptionLabels(expenses.Form));

        expenses.Form.AmountText = "1 250,5";
        expenses.Form.SelectedCategory = expenses.Form.Options[0];
        expenses.Form.Date = new DateTime(2026, 11, 21);
        expenses.Form.Comment = "  Сільпо ";
        await expenses.Form.SubmitCommand.ExecuteAsync();

        var post = backend.Api.Requests.Find(r => r.Method == HttpMethod.Post)!;
        Assert.Equal(
            "{\"amount\":\"1250.5\",\"categoryId\":\"3f1c8a52-6b7e-4d0a-9a41-0c2f5e7b1d01\",\"date\":\"2026-11-21\",\"comment\":\"Сільпо\"}",
            post.Body);
        Assert.Equal("Витрату додано", notifications.Message);
        Assert.Single(expenses.Rows);
        Assert.Equal("1 витрата", expenses.CountText);
        Assert.Equal("", expenses.Form.AmountText);

        await overview.LoadAsync();
        Assert.True(overview.IsReady);
        Assert.Equal("1 250,50 грн", Normalize(overview.TotalText));
        Assert.Equal("Їжа", overview.TopCategoryName);
        Assert.Equal("100,0%", overview.TopCategoryShare);
        Assert.Equal("21 листопада · Сільпо", overview.Recent[0].MetaText);
        Assert.Single(overview.Segments);
    }

    [Fact]
    public async Task Invalid_input_is_not_sent()
    {
        var backend = new FakeBackend();
        var expenses = new ExpensesViewModel(backend.CreateClient(), new MonthState(() => Today), new NotificationCenter());
        await expenses.LoadAsync();

        expenses.Form.AmountText = "12,345";
        await expenses.Form.SubmitCommand.ExecuteAsync();

        Assert.Equal("Сума може мати не більше двох знаків після коми.", expenses.Form.AmountError);
        Assert.Equal("Оберіть категорію.", expenses.Form.CategoryError);
        Assert.DoesNotContain(backend.Api.Requests, r => r.Method == HttpMethod.Post);
    }

    [Fact]
    public async Task An_expense_for_another_month_switches_to_that_month()
    {
        var backend = new FakeBackend();
        var month = new MonthState(() => Today);
        var notifications = new NotificationCenter();
        var expenses = new ExpensesViewModel(backend.CreateClient(), month, notifications);
        await expenses.LoadAsync();

        expenses.Form.AmountText = "300";
        expenses.Form.SelectedCategory = expenses.Form.Options[0];
        expenses.Form.Date = new DateTime(2026, 10, 5);
        await expenses.Form.SubmitCommand.ExecuteAsync();

        Assert.Equal("Жовтень 2026", month.Label);
        Assert.Equal("Витрату додано за жовтень 2026", notifications.Message);
        Assert.Single(expenses.Rows);
        Assert.Equal("5 жовтня", expenses.Rows[0].DayText);
    }

    [Fact]
    public async Task Switching_the_month_reloads_the_overview()
    {
        var backend = new FakeBackend();
        backend.Expenses.Add(new Core.DTOs.ExpenseResponseDto(Guid.NewGuid(), "75.00", FakeBackend.TaxiId, new DateOnly(2026, 10, 30), null));
        var month = new MonthState(() => Today);
        var overview = new OverviewViewModel(backend.CreateClient(), month);
        await overview.LoadAsync();
        Assert.True(overview.IsEmpty);

        await month.PreviousCommand.ExecuteAsync();

        Assert.True(overview.IsReady);
        Assert.Equal("Витрачено за жовтень 2026", overview.TotalLabel);
        Assert.Equal("Таксі", overview.Rows[0].Name);
        Assert.True(overview.Rows[0].IsArchived);
        Assert.Contains(backend.Api.Requests, r => r.PathAndQuery == "/api/statistics?month=2026-10");
    }

    [Fact]
    public async Task Editing_an_expense_in_an_archived_category_keeps_that_category_available()
    {
        var backend = new FakeBackend();
        backend.Expenses.Add(new Core.DTOs.ExpenseResponseDto(Guid.NewGuid(), "90.00", FakeBackend.TaxiId, new DateOnly(2026, 11, 2), null));
        var expenses = new ExpensesViewModel(backend.CreateClient(), new MonthState(() => Today), new NotificationCenter());
        await expenses.LoadAsync();

        await expenses.EditCommand.ExecuteAsync(expenses.Rows[0]);

        Assert.Equal("Редагування витрати", expenses.Form.Title);
        Assert.Equal("90,00", expenses.Form.AmountText);
        Assert.Equal(new[] { "Їжа", "Таксі (в архіві)" }, OptionLabels(expenses.Form));
        Assert.Equal(FakeBackend.TaxiId, expenses.Form.SelectedCategory!.Id);
    }

    private static List<string> OptionLabels(ExpenseFormViewModel form)
    {
        var labels = new List<string>();
        foreach (var option in form.Options)
            labels.Add(option.Label);
        return labels;
    }

    private static string Normalize(string text)
    {
        return text.Replace(' ', ' ').Replace(' ', ' ');
    }
}
