using System.Linq.Expressions;
using BookStore.Domain.Books;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Application.Features.Books;

public sealed record GetBooksQuery(
    Genre? Genre = null,
    int Page = 1,
    int PageSize = 10,
    string? SortField = null,
    bool SortDesc = false) : IRequest<PagedResult<BookDto>>;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);

public sealed class GetBooksHandler(IAppDbContext db) : IRequestHandler<GetBooksQuery, PagedResult<BookDto>>
{
    private const int MaxPageSize = 50;

    public async Task<PagedResult<BookDto>> Handle(GetBooksQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);

        var query = db.Books.AsNoTracking();
        if (request.Genre is { } genre)
            query = query.Where(b => b.Genre == genre);

        var totalCount = await query.CountAsync(cancellationToken);

        var sortKey = SortKey(request.SortField);
        var sorted = request.SortDesc ? query.OrderByDescending(sortKey) : query.OrderBy(sortKey);

        var books = await sorted
            .ThenBy(b => b.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<BookDto>(books.Select(BookDto.FromEntity).ToList(), page, pageSize, totalCount);
    }

    private static Expression<Func<Book, object?>> SortKey(string? sortField) =>
        sortField?.ToLowerInvariant() switch
        {
            "author" => b => b.Author,
            "publishedyear" => b => b.PublishedYear,
            "price" => b => b.Price,
            "stock" => b => b.Stock,
            _ => b => b.Title
        };
}
