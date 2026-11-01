using BookStore.Domain.Common;

namespace BookStore.Domain.Books;

/// <summary>
/// A book owned by the store. Catalog fields (title, author, ISBN, etc.) are captured
/// from Open Library once at creation and are immutable afterwards; store fields
/// (genre, price, stock, description) are the only ones that can change.
/// </summary>
public sealed class Book
{
    public const int TitleMaxLength = 500;
    public const int AuthorMaxLength = 500;
    public const int IsbnMaxLength = 20;
    public const int OpenLibraryKeyMaxLength = 100;
    public const int CoverUrlMaxLength = 500;
    public const int DescriptionMaxLength = 4000;

    public Guid Id { get; private set; }

    // Catalog fields — sourced from Open Library at add-time.
    public string Title { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;
    public string Isbn { get; private set; } = string.Empty;
    public int? PublishedYear { get; private set; }
    public string? CoverUrl { get; private set; }
    public string OpenLibraryKey { get; private set; } = string.Empty;

    // Store fields — owned and editable by the store.
    public string? Description { get; private set; }
    public Genre Genre { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>Optimistic concurrency token; rotated on every update.</summary>
    public Guid ConcurrencyStamp { get; private set; }

    private Book() { } // EF Core

    public static Book Create(
        string title,
        string author,
        string? isbn,
        int? publishedYear,
        string? coverUrl,
        string openLibraryKey,
        string? description,
        Genre genre,
        decimal price,
        int stock,
        DateTime createdAtUtc)
    {
        EnsureValidStoreDetails(genre, price, stock);

        return new Book
        {
            Id = Guid.NewGuid(),
            Title = Required(title, nameof(Title), TitleMaxLength),
            Author = Required(author, nameof(Author), AuthorMaxLength),
            Isbn = Optional(isbn, nameof(Isbn), IsbnMaxLength) ?? string.Empty,
            PublishedYear = publishedYear,
            CoverUrl = Optional(coverUrl, nameof(CoverUrl), CoverUrlMaxLength),
            OpenLibraryKey = Required(openLibraryKey, nameof(OpenLibraryKey), OpenLibraryKeyMaxLength),
            Description = Optional(description, nameof(Description), DescriptionMaxLength),
            Genre = genre,
            Price = price,
            Stock = stock,
            CreatedAtUtc = createdAtUtc,
            ConcurrencyStamp = Guid.NewGuid()
        };
    }

    public void UpdateStoreDetails(Genre genre, decimal price, int stock, string? description)
    {
        EnsureValidStoreDetails(genre, price, stock);

        Genre = genre;
        Price = price;
        Stock = stock;
        Description = Optional(description, nameof(Description), DescriptionMaxLength);
        ConcurrencyStamp = Guid.NewGuid();
    }

    private static void EnsureValidStoreDetails(Genre genre, decimal price, int stock)
    {
        if (!Enum.IsDefined(genre))
            throw new DomainException($"'{genre}' is not a valid genre.");
        if (price < 0)
            throw new DomainException("Price cannot be negative.");
        if (stock < 0)
            throw new DomainException("Stock cannot be negative.");
    }

    private static string Required(string? value, string field, int maxLength) =>
        Optional(value, field, maxLength)
        ?? throw new DomainException($"{field} is required.");

    private static string? Optional(string? value, string field, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength
            ? trimmed
            : throw new DomainException($"{field} must be {maxLength} characters or fewer.");
    }
}