using BookStore.Application.Features.Books;
using FastEndpoints;
using MediatR;

namespace BookStore.Api.Endpoints.Books;

public sealed class DeleteBookEndpoint(ISender sender) : Endpoint<DeleteBookCommand>
{
    public override void Configure()
    {
        Delete("/api/books/{id:guid}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(DeleteBookCommand req, CancellationToken ct)
    {
        await sender.Send(req, ct);
        await Send.NoContentAsync(ct);
    }
}
