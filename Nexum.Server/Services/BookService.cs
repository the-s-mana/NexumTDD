
using Nexum.Server.Data;
using Nexum.Server.Data.Models;
using SurrealDb.Net;

namespace Nexum.Server.Services;
public interface IBookService
{
    Task<Book> CreateBookAsync(Book newBook);
    Task<List<Book>> GetAllBooksAsync();
}
public class BookService : IBookService
{
    private readonly SurrealDbClient _dbClient;

    // เราขอ ISurrealDbProvider จาก DI Container ผ่าน Constructor
    public BookService(ISurrealDbProvider dbProvider)
    {
        _dbClient = dbProvider.Client;
    }

    public async Task<Book> CreateBookAsync(Book newBook)
    {
        return await _dbClient.Create("book", newBook);
    }

    public async Task<List<Book>> GetAllBooksAsync()
    {
        var booksEnumerable = await _dbClient.Select<Book>("book");

        return booksEnumerable.ToList();
    }
}