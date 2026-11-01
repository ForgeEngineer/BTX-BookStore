using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Application.Features.Books;

public sealed record GetBookQuery(Guid Id) : IRequest<BookDto>;

public sealed class GetBookHandler(IAppDbContext db) : IRequestHandler<GetBookQuery, BookDto>
{
    public async Task<BookDto> Handle(GetBookQuery request, CancellationToken cancellationToken)
    {
        var book = await db.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Book '{request.Id}' was not found.");

        return BookDto.FromEntity(book);
    }
}
