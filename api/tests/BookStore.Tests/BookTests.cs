using BookStore.Domain.Books;
using BookStore.Domain.Common;

namespace BookStore.Tests;

public class BookTests
{
    private static Book CreateValidBook(
        string title = "Dune",
        decimal price = 19.99m,
        int stock = 5) =>
        Book.Create(
            title: title,
            author: "Frank Herbert",
            isbn: "9780441172719",
            publishedYear: 1965,
            coverUrl: "https://covers.openlibrary.org/b/isbn/9780441172719-M.jpg",
            openLibraryKey: "/works/OL893415W",
            description: null,
            genre: Genre.SciFi,
            price: price,
            stock: stock,
            createdAtUtc: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    [Fact]
    public void Create_WithValidData_SetsFields()
    {
        var book = CreateValidBook();

        Assert.NotEqual(Guid.Empty, book.Id);
        Assert.NotEqual(Guid.Empty, book.ConcurrencyStamp);
        Assert.Equal("Dune", book.Title);
        Assert.Equal(Genre.SciFi, book.Genre);
        Assert.Equal(19.99m, book.Price);
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var book = CreateValidBook(title: "  Dune  ");

        Assert.Equal("Dune", book.Title);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankTitle_Throws(string title)
    {
        Assert.Throws<DomainException>(() => CreateValidBook(title: title));
    }

    [Fact]
    public void Create_WithNegativePrice_Throws()
    {
        Assert.Throws<DomainException>(() => CreateValidBook(price: -1m));
    }

    [Fact]
    public void UpdateStoreDetails_ChangesFieldsAndRotatesConcurrencyStamp()
    {
        var book = CreateValidBook();
        var originalStamp = book.ConcurrencyStamp;

        book.UpdateStoreDetails(Genre.Fiction, 25m, 10, "A desert planet epic.");

        Assert.Equal(Genre.Fiction, book.Genre);
        Assert.Equal(25m, book.Price);
        Assert.Equal(10, book.Stock);
        Assert.Equal("A desert planet epic.", book.Description);
        Assert.NotEqual(originalStamp, book.ConcurrencyStamp);
    }

    [Fact]
    public void UpdateStoreDetails_WithNegativeStock_Throws()
    {
        var book = CreateValidBook();

        Assert.Throws<DomainException>(() => book.UpdateStoreDetails(Genre.SciFi, 10m, -1, null));
    }
}