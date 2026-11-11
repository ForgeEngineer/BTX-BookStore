using BookStore.Application;
using BookStore.Application.Features.Books;
using FastEndpoints;
using FluentValidation;
using MediatR;

namespace BookStore.Api.Endpoints.Books;

public sealed class GetBooksEndpoint(ISender sender) : Endpoint<GetBooksQuery, PagedResult<BookDto>>
{
    public override void Configure()
    {
        Get("/api/books");
        AllowAnonymous();
    }

    public override Task<PagedResult<BookDto>> ExecuteAsync(GetBooksQuery req, CancellationToken ct) =>
        sender.Send(req, ct);
}

public sealed class GetBooksValidator : Validator<GetBooksQuery>
{
    public GetBooksValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}
