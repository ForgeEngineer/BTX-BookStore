namespace BookStore.Application;

public interface IBookCatalog
{
    Task<IReadOnlyList<CatalogBook>> SearchAsync(string query, CancellationToken cancellationToken);
}

public sealed record CatalogBook(
    string Title,
    string Author,
    string? Isbn,
    int? PublishedYear,
    string? CoverUrl,
    string OpenLibraryKey);
