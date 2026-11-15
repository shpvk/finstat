using Finstat.Domain;

namespace Finstat.Tests;

public class CategoryTests
{
    [Fact]
    public void Name_is_trimmed()
    {
        var category = new Category(Guid.NewGuid(), "  Їжа  ");
        Assert.Equal("Їжа", category.Name);
        Assert.False(category.IsArchived);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_name_is_rejected(string? name) =>
        Assert.Throws<DomainException>(() => new Category(Guid.NewGuid(), name!));

    [Fact]
    public void Name_longer_than_limit_is_rejected()
    {
        Assert.Throws<DomainException>(() => new Category(Guid.NewGuid(), new string('a', 51)));
        Assert.Equal(50, new Category(Guid.NewGuid(), new string('a', 50)).Name.Length);
    }

    [Fact]
    public void Rename_trims_and_validates()
    {
        var category = new Category(Guid.NewGuid(), "Old");
        category.Rename(" New ");
        Assert.Equal("New", category.Name);
        Assert.Throws<DomainException>(() => category.Rename(" "));
        Assert.Equal("New", category.Name);
    }

    [Fact]
    public void Archive_is_idempotent_and_rename_still_allowed()
    {
        var category = new Category(Guid.NewGuid(), "Old");
        category.Archive();
        category.Archive();
        category.Rename("Renamed");
        Assert.True(category.IsArchived);
        Assert.Equal("Renamed", category.Name);
    }
}
