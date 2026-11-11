using BookStore.Domain.Books;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(BookStoreDbContext db, TimeProvider timeProvider, CancellationToken cancellationToken = default)
    {
        if (await db.Books.AnyAsync(cancellationToken))
            return;

        var now = timeProvider.GetUtcNow().UtcDateTime;

        Book Seed(string title, string author, string isbn, int year, string workKey, Genre genre, decimal price, int stock) =>
            Book.Create(title, author, isbn, year, $"https://covers.openlibrary.org/b/isbn/{isbn}-M.jpg", workKey, null, genre, price, stock, now);

        db.Books.AddRange(
            Seed("To Kill a Mockingbird", "Harper Lee", "9780061120084", 1960, "/works/OL3140822W", Genre.Fiction, 14.99m, 12),
            Seed("Dune", "Frank Herbert", "9780441172719", 1965, "/works/OL893414W", Genre.SciFi, 18.99m, 8),
            Seed("The Hobbit", "J.R.R. Tolkien", "9780547928227", 1937, "/works/OL27482W", Genre.Fantasy, 15.50m, 20),
            Seed("And Then There Were None", "Agatha Christie", "9780062073488", 1939, "/works/OL471565W", Genre.Mystery, 12.99m, 6),
            Seed("The Diary of a Young Girl", "Anne Frank", "9780553296983", 1944, "/works/OL266178W", Genre.Biography, 9.99m, 15),
            Seed("A Brief History of Time", "Stephen Hawking", "9780553380163", 1988, "/works/OL1892617W", Genre.NonFiction, 17.00m, 9),
            Seed("Charlotte's Web", "E. B. White", "9780064400558", 1952, "/works/OL483391W", Genre.Children, 8.99m, 25),
            Seed("Guns, Germs, and Steel", "Jared Diamond", "9780393317558", 1997, "/works/OL276558W", Genre.History, 19.95m, 4));

        await db.SaveChangesAsync(cancellationToken);
    }
}
