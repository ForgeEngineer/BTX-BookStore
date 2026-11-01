using BookStore.Application;
using BookStore.Application.Features.Books;
using BookStore.Domain.Books;
using BookStore.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Tests;

public sealed class BookHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly BookStoreDbContext _db;

    public BookHandlerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _db = new BookStoreDbContext(new DbContextOptionsBuilder<BookStoreDbContext>().UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private static AddBookCommand Command(string key, string? isbn, Genre genre = Genre.SciFi) =>
        new("Dune", "Frank Herbert", isbn, 1965, null, key, null, genre, 19.99m, 5);

    private Task<BookDto> Add(AddBookCommand command) =>
        new AddBookHandler(_db, TimeProvider.System).Handle(command, CancellationToken.None);

    [Fact]
    public async Task AddBook_SavesBook()
    {
        var result = await Add(Command("/works/OL1W", "9780441172719"));

        var saved = await _db.Books.AsNoTracking().SingleAsync();
        Assert.Equal(result.Id, saved.Id);
        Assert.Equal("Dune", saved.Title);
        Assert.Equal(19.99m, saved.Price);
    }

    [Theory]
    [InlineData("/works/OL1W", "9780000000002")]
    [InlineData("/works/OL2W", "9780441172719")]
    public async Task AddBook_Duplicate_ThrowsConflict(string key, string isbn)
    {
        await Add(Command("/works/OL1W", "9780441172719"));

        await Assert.ThrowsAsync<ConflictException>(() => Add(Command(key, isbn)));
    }

    [Fact]
    public async Task GetBooks_FiltersByGenre()
    {
        await Add(Command("/works/OL1W", "9780441172719", Genre.SciFi));
        await Add(Command("/works/OL2W", "9780000000002", Genre.History));

        var result = await new GetBooksHandler(_db).Handle(new GetBooksQuery(Genre.History), CancellationToken.None);

        var book = Assert.Single(result.Items);
        Assert.Equal(Genre.History, book.Genre);
        Assert.Equal(1, result.TotalCount);
    }
}
