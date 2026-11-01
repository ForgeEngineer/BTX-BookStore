using BookStore.Application.Features.Books;
using BookStore.Application.Features.Catalog;
using BookStore.Domain.Books;
using MediatR;

namespace BookStore.Api;

public static class BookEndpoints
{
    public static void MapBookEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/catalog/search", async (string q, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new SearchCatalogQuery(q), ct)));

        var books = app.MapGroup("/api/books");

        books.MapGet("/", async (
            ISender sender,
            CancellationToken ct,
            Genre? genre,
            int page = 1,
            int pageSize = 10,
            string? sortField = null,
            bool sortDesc = false) =>
            Results.Ok(await sender.Send(new GetBooksQuery(genre, page, pageSize, sortField, sortDesc), ct)));

        books.MapGet("/genres", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetGenreCountsQuery(), ct)));

        books.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetBookQuery(id), ct)));

        books.MapPost("/", async (AddBookCommand command, ISender sender, CancellationToken ct) =>
        {
            var book = await sender.Send(command, ct);
            return Results.Created($"/api/books/{book.Id}", book);
        });

        books.MapPut("/{id:guid}", async (Guid id, UpdateBookCommand command, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(command with { Id = id }, ct)));

        books.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new DeleteBookCommand(id), ct);
            return Results.NoContent();
        });
    }
}
