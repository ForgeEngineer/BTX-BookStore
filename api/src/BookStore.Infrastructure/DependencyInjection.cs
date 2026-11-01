using BookStore.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookStore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BookStoreDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default")));
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<BookStoreDbContext>());

        services.AddHttpClient<IBookCatalog, OpenLibraryClient>(client =>
        {
            client.BaseAddress = new Uri("https://openlibrary.org");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("BookStoreAssessment/1.0");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        return services;
    }
}
