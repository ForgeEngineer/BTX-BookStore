using BookStore.Application;
using BookStore.Application.Features.Books;
using FastEndpoints;
using FluentValidation;
using MediatR;

namespace BookStore.Api.Endpoints.Books;

public sealed class AddBookEndpoint(ISender sender) : Endpoint<AddBookCommand, BookDto>
{
    public override void Configure()
    {
        Post("/api/books");
        AllowAnonymous();
    }

    public override async Task HandleAsync(AddBookCommand req, CancellationToken ct)
    {
        var book = await sender.Send(req, ct);
        HttpContext.Response.Headers.Location = $"/api/books/{book.Id}";
        await Send.ResponseAsync(book, StatusCodes.Status201Created, ct);
    }
}

public sealed class AddBookValidator : Validator<AddBookCommand>
{
    public AddBookValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Author).NotEmpty();
        RuleFor(x => x.OpenLibraryKey).NotEmpty();
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
    }
}
