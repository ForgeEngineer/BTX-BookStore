using BookStore.Api.Endpoints.Books;
using BookStore.Api.Endpoints.Catalog;
using BookStore.Application.Features.Books;
using BookStore.Domain.Books;

namespace BookStore.Tests;

public sealed class ValidatorTests
{
    [Fact]
    public void AddBook_NegativePrice_IsRejected()
    {
        var command = new AddBookCommand("Dune", "Frank Herbert", null, 1965, null, "/works/OL1W", null, Genre.SciFi, -1m, 5);

        var result = new AddBookValidator().Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AddBookCommand.Price));
    }

    [Fact]
    public void SearchCatalog_SingleCharacter_IsRejected()
    {
        var result = new SearchCatalogValidator().Validate(new SearchCatalogRequest { Q = "d" });

        Assert.False(result.IsValid);
    }
}
