using BookStore.Domain.Books;

namespace BookStore.Application;

public sealed record BookDto(
    Guid Id,
    string Title,
    string Author,
    string Isbn,
    int? PublishedYear,
    string? CoverUrl,
    string OpenLibraryKey,
    string? Description,
    Genre Genre,
    decimal Price,
    int Stock,
    DateTime CreatedAtUtc)
{
    public static BookDto FromEntity(Book book) => new(
        book.Id,
        book.Title,
        book.Author,
        book.Isbn,
        book.PublishedYear,
        book.CoverUrl,
        book.OpenLibraryKey,
        book.Description,
        book.Genre,
        book.Price,
        book.Stock,
        book.CreatedAtUtc);
}
