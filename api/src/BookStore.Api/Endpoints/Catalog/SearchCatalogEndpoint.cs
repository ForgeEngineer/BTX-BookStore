using BookStore.Application;
using BookStore.Application.Features.Catalog;
using FastEndpoints;
using FluentValidation;
using MediatR;

namespace BookStore.Api.Endpoints.Catalog;

public sealed class SearchCatalogRequest
{
    public string Q { get; init; } = string.Empty;
}

public sealed class SearchCatalogEndpoint(ISender sender) : Endpoint<SearchCatalogRequest, IReadOnlyList<CatalogBook>>
{
    public override void Configure()
    {
        Get("/api/catalog/search");
        AllowAnonymous();
    }

    public override Task<IReadOnlyList<CatalogBook>> ExecuteAsync(SearchCatalogRequest req, CancellationToken ct) =>
        sender.Send(new SearchCatalogQuery(req.Q), ct);
}

public sealed class SearchCatalogValidator : Validator<SearchCatalogRequest>
{
    public SearchCatalogValidator()
    {
        RuleFor(x => x.Q).NotEmpty().MinimumLength(2);
    }
}
