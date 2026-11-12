using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Application.Features.Books;

public sealed record GetGenreCountsQuery : IRequest<IReadOnlyList<GenreCount>>;

public sealed record GenreCount(string Genre, int Count);

public sealed class GetGenreCountsHandler(IAppDbContext db)
    : IRequestHandler<GetGenreCountsQuery, IReadOnlyList<GenreCount>>
{
    public async Task<IReadOnlyList<GenreCount>> Handle(GetGenreCountsQuery request, CancellationToken cancellationToken) =>
        await db.Database
            .SqlQuery<GenreCount>($"""SELECT genre AS "Genre", count AS "Count" FROM get_genre_counts()""")
            .ToListAsync(cancellationToken);
}
