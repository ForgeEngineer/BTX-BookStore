using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Application.Features.Books;

public sealed record DeleteBookCommand(Guid Id) : IRequest;

public sealed class DeleteBookHandler(IAppDbContext db) : IRequestHandler<DeleteBookCommand>
{
    public async Task Handle(DeleteBookCommand request, CancellationToken cancellationToken)
    {
        var book = await db.Books.FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Book '{request.Id}' was not found.");

        db.Books.Remove(book);
        await db.SaveChangesAsync(cancellationToken);
    }
}
