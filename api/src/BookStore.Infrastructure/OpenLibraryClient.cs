using System.Net.Http.Json;
using System.Text.Json;
using BookStore.Application;
using Microsoft.Extensions.Logging;

namespace BookStore.Infrastructure;

public sealed class OpenLibraryClient(HttpClient httpClient, ILogger<OpenLibraryClient> logger) : IBookCatalog
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task<IReadOnlyList<CatalogBook>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        var url = $"search.json?q={Uri.EscapeDataString(query)}&fields=key,title,author_name,first_publish_year,isbn,cover_i&limit=10";

        try
        {
            var response = await httpClient.GetFromJsonAsync<SearchResponse>(url, JsonOptions, cancellationToken);

            var books = (response?.Docs ?? [])
                .Where(doc => !string.IsNullOrWhiteSpace(doc.Key) && !string.IsNullOrWhiteSpace(doc.Title))
                .Select(ToCatalogBook)
                .ToList();

            logger.LogInformation("Open Library search for {Query} returned {ResultCount} results", query, books.Count);
            return books;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Open Library search for {Query} timed out", query);
            throw new CatalogUnavailableException("The book catalog timed out.", ex);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Open Library search for {Query} failed with status {StatusCode}", query, ex.StatusCode);
            throw new CatalogUnavailableException("The book catalog is unavailable.", ex);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Open Library search for {Query} returned an invalid response", query);
            throw new CatalogUnavailableException("The book catalog returned an invalid response.", ex);
        }
    }

    private static CatalogBook ToCatalogBook(SearchDoc doc) => new(
        doc.Title!,
        string.Join(", ", doc.AuthorName ?? []),
        doc.Isbn?.FirstOrDefault(),
        doc.FirstPublishYear,
        doc.CoverI is { } coverId ? $"https://covers.openlibrary.org/b/id/{coverId}-M.jpg" : null,
        doc.Key!);

    private sealed record SearchResponse(List<SearchDoc>? Docs);

    private sealed record SearchDoc(
        string? Key,
        string? Title,
        List<string>? AuthorName,
        int? FirstPublishYear,
        List<string>? Isbn,
        long? CoverI);
}
