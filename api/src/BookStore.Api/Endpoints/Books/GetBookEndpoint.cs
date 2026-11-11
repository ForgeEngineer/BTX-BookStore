using BookStore.Application;
using BookStore.Application.Features.Books;
using FastEndpoints;
using MediatR;

namespace BookStore.Api.Endpoints.Books;

public sealed class GetBookEndpoint(ISender sender) : Endpoint<GetBookQuery, BookDto>
{
    public override void Configure()
    {
        Get("/api/books/{id:guid}");
        AllowAnonymous();
    }

    public override Task<BookDto> ExecuteAsync(GetBookQuery req, CancellationToken ct) =>
        sender.Send(req, ct);
}
