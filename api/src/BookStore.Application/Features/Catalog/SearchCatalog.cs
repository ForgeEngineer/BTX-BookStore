using MediatR;

namespace BookStore.Application.Features.Catalog;

public sealed record SearchCatalogQuery(string Query) : IRequest<IReadOnlyList<CatalogBook>>;

public sealed class SearchCatalogHandler(IBookCatalog catalog)
    : IRequestHandler<SearchCatalogQuery, IReadOnlyList<CatalogBook>>
{
    public async Task<IReadOnlyList<CatalogBook>> Handle(SearchCatalogQuery request, CancellationToken cancellationToken) =>
        string.IsNullOrWhiteSpace(request.Query)
            ? []
            : await catalog.SearchAsync(request.Query, cancellationToken);
}
