using BookStore.Application;
using BookStore.Domain.Books;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure;

public sealed class BookStoreDbContext(DbContextOptions<BookStoreDbContext> options)
    : DbContext(options), IAppDbContext
{
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(book =>
        {
            book.HasKey(b => b.Id);
            book.Property(b => b.Title).HasMaxLength(Book.TitleMaxLength);
            book.Property(b => b.Author).HasMaxLength(Book.AuthorMaxLength);
            book.Property(b => b.Isbn).HasMaxLength(Book.IsbnMaxLength);
            book.Property(b => b.OpenLibraryKey).HasMaxLength(Book.OpenLibraryKeyMaxLength);
            book.Property(b => b.CoverUrl).HasMaxLength(Book.CoverUrlMaxLength);
            book.Property(b => b.Description).HasMaxLength(Book.DescriptionMaxLength);
            book.Property(b => b.Genre).HasConversion<string>();
            book.Property(b => b.Price).HasConversion<double>();
            book.Property(b => b.CreatedAtUtc).HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            book.HasIndex(b => b.OpenLibraryKey).IsUnique();
        });
    }
}
