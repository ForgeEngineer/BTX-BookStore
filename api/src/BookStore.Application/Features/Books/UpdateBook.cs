using BookStore.Domain.Books;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Application.Features.Books;

public sealed record UpdateBookCommand(Guid Id, Genre Genre, decimal Price, int Stock, string? Description)
    : IRequest<BookDto>;

public sealed class UpdateBookHandler(IAppDbContext db) : IRequestHandler<UpdateBookCommand, BookDto>
{
    public async Task<BookDto> Handle(UpdateBookCommand request, CancellationToken cancellationToken)
    {
        var book = await db.Books.FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Book '{request.Id}' was not found.");

        book.UpdateStoreDetails(request.Genre, request.Price, request.Stock, request.Description);
        await db.SaveChangesAsync(cancellationToken);

        return BookDto.FromEntity(book);
    }
}
