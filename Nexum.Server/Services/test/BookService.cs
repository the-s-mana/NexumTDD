using Nexum.Server.Data.Models;

namespace Nexum.Server.Services.test
{
    public interface IBookService
    {
        Task<IEnumerable<Book>> GetAllBooksAsync();
        Task<Book> CreateBookAsync(Book book);
    }
    public class BookService : IBookService
    {
        public Task<Book> CreateBookAsync(Book book)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Book>> GetAllBooksAsync()
        {
            throw new NotImplementedException();
        }
    }
}
