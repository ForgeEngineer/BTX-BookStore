using BookStore.Application.Features.Books;
using FastEndpoints;
using MediatR;

namespace BookStore.Api.Endpoints.Books;

public sealed class GetGenreCountsEndpoint(ISender sender) : EndpointWithoutRequest<IReadOnlyList<GenreCount>>
{
    public override void Configure()
    {
        Get("/api/books/genres");
        AllowAnonymous();
    }

    public override Task<IReadOnlyList<GenreCount>> ExecuteAsync(CancellationToken ct) =>
        sender.Send(new GetGenreCountsQuery(), ct);
}
