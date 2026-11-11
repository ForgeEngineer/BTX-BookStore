using BookStore.Application;
using BookStore.Application.Features.Books;
using FastEndpoints;
using FluentValidation;
using MediatR;

namespace BookStore.Api.Endpoints.Books;

public sealed class UpdateBookEndpoint(ISender sender) : Endpoint<UpdateBookCommand, BookDto>
{
    public override void Configure()
    {
        Put("/api/books/{id:guid}");
        AllowAnonymous();
    }

    public override Task<BookDto> ExecuteAsync(UpdateBookCommand req, CancellationToken ct) =>
        sender.Send(req, ct);
}

public sealed class UpdateBookValidator : Validator<UpdateBookCommand>
{
    public UpdateBookValidator()
    {
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
    }
}
