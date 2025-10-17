using Nexum.Server.Data;
using Nexum.Server.Data.Models;
using static Nexum.Server.Data.IDbProviderFactory;

namespace Nexum.Server.Services.test
{
    public interface IBookService
    {
        Task<Book> CreateBookAsync(Book newBook);
        Task<List<Book>> GetAllBooksAsync();
    }
    public class BookService : IBookService
    {
        ISurrealDbProvider<Book> bookDbProvider;
        SurrealDbProviderFactoryBase surrealDbProviderFactory;

        // เราขอ ISurrealDbProvider จาก DI Container ผ่าน Constructor
        public BookService(ISurrealDbProvider<Book> bookDbProvider, SurrealDbProviderFactoryBase surrealDbProviderFactory)
        {
            this.surrealDbProviderFactory = surrealDbProviderFactory;
            this.bookDbProvider = surrealDbProviderFactory.Create<Book>();


        }

        public async Task<Book> CreateBookAsync(Book newBook)
        {
            //return await _dbClient.Create("book", newBook);
            throw new NotImplementedException();
        }

        public async Task<List<Book>> GetAllBooksAsync()
        {
            var booksEnumerable = await bookDbProvider.List();

            return booksEnumerable.ToList();
        }
    }
}
