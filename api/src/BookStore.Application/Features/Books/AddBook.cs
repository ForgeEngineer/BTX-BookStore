using BookStore.Domain.Books;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Application.Features.Books;

public sealed record AddBookCommand(
    string Title,
    string Author,
    string? Isbn,
    int? PublishedYear,
    string? CoverUrl,
    string OpenLibraryKey,
    string? Description,
    Genre Genre,
    decimal Price,
    int Stock) : IRequest<BookDto>;

public sealed class AddBookHandler(IAppDbContext db, TimeProvider timeProvider)
    : IRequestHandler<AddBookCommand, BookDto>
{
    public async Task<BookDto> Handle(AddBookCommand request, CancellationToken cancellationToken)
    {
        var book = Book.Create(
            request.Title,
            request.Author,
            request.Isbn,
            request.PublishedYear,
            request.CoverUrl,
            request.OpenLibraryKey,
            request.Description,
            request.Genre,
            request.Price,
            request.Stock,
            timeProvider.GetUtcNow().UtcDateTime);

        if (await db.Books.AnyAsync(b => b.OpenLibraryKey == book.OpenLibraryKey, cancellationToken))
            throw new ConflictException($"A book with Open Library key '{book.OpenLibraryKey}' already exists.");

        if (book.Isbn != string.Empty && await db.Books.AnyAsync(b => b.Isbn == book.Isbn, cancellationToken))
            throw new ConflictException($"A book with ISBN '{book.Isbn}' already exists.");

        db.Books.Add(book);
        await db.SaveChangesAsync(cancellationToken);

        return BookDto.FromEntity(book);
    }
}
