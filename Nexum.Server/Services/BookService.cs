
using Mapster;
using MapsterMapper;
using Nexum.Server.Data;
using Nexum.Server.Data.Models;
using Nexum.Server.Extensions;
using Nexum.Server.Models.Book;
using SurrealDb.Net.Models;
using static Nexum.Server.Data.IDbProviderFactory;

namespace Nexum.Server.Services;
public interface IBookService
{
    // Surreal
    Task<List<BookResponseDTO>> GetAllBooksSurrealAsync();
    Task<BookResponseDTO> CreateBookSurrealAsync(CreateBookRequestDTO newBook);
    Task<BookResponseDTO> UpdateBookSurrealAsync(UpdateBookRequestDTO updateRequest);

    // Nexum
    Task<List<BookResponseDTO>> GetAllBooksNexumAsync();
    Task<BookResponseDTO> CreateBookNexumAsync(CreateBookRequestDTO newBook);
    Task<BookResponseDTO> UpdateBookNexumAsync(UpdateBookRequestDTO updateRequest);
}
public class BookService : IBookService
{
    private readonly ISurrealDbProvider<Book, BookResponseDTO> _bookDbProvider;
    private readonly SurrealDbProviderFactoryBase _surrealDbProviderFactory;
    private readonly IMapper _mapper;

    public BookService(ISurrealDbProvider<Book, BookResponseDTO> bookDbProvider
        , SurrealDbProviderFactoryBase surrealDbProviderFactory
        , IMapper mapper)
    {
        _surrealDbProviderFactory = surrealDbProviderFactory;
        _bookDbProvider = surrealDbProviderFactory.Create<Book, BookResponseDTO>();
        _mapper = mapper;
    }

    // Surreal
    public async Task<List<BookResponseDTO>> GetAllBooksSurrealAsync()
    {
        var res = await _bookDbProvider.ListSurreal();

        if (res == null)
        {
            throw new Exception("Failed to retrieve books.");
        }

        List<Book> bookList = res.ToList();

        List<BookResponseDTO> dtos = _mapper.Map<List<BookResponseDTO>>(bookList);

        return dtos;
    }

    public async Task<BookResponseDTO> CreateBookSurrealAsync(CreateBookRequestDTO creatRequest)
    {
        Book create = new Book
        {
            Title = creatRequest.Title,
            Author = creatRequest.Author,
            PublishYear = creatRequest.PublishYear,
            StoreId = RecordId.From(nameof(Book), creatRequest.StoreId),
        };

        var res = await _bookDbProvider.CreateSurreal(create);
        BookResponseDTO dtos = _mapper.Map<BookResponseDTO>(res);
        return dtos;
    }

    public async Task<BookResponseDTO> UpdateBookSurrealAsync(UpdateBookRequestDTO updateRequest)
    {
        var dataToMerge = new Dictionary<string, object?>();

        if (!string.IsNullOrEmpty(updateRequest.Title))
        {
            dataToMerge.Add(nameof(Book.Title), updateRequest.Title);
        }

        if (!string.IsNullOrEmpty(updateRequest.Author))
        {
            dataToMerge.Add(nameof(Book.Author), updateRequest.Author);
        }

        if (updateRequest.PublishYear != default(int))
        {
            dataToMerge.Add(nameof(Book.PublishYear), updateRequest.PublishYear);
        }

        var res = await _bookDbProvider.UpdateSurreal(updateRequest.Id, dataToMerge);
        BookResponseDTO dtos = _mapper.Map<BookResponseDTO>(res);
        return dtos;
    }

    // Nexum
    public async Task<List<BookResponseDTO>> GetAllBooksNexumAsync()
    {
        var booksEnumerable = await _bookDbProvider.ListNexum();

        return booksEnumerable.ToList();
    }

    public async Task<BookResponseDTO> CreateBookNexumAsync(CreateBookRequestDTO creatRequest)
    {
        BookResponseDTO create = new BookResponseDTO
        {
            Title = creatRequest.Title,
            Author = creatRequest.Author,
            PublishYear = creatRequest.PublishYear,
            StoreId = creatRequest.StoreId
        };
        return await _bookDbProvider.CreateNexum(create);
    }

    public async Task<BookResponseDTO> UpdateBookNexumAsync(UpdateBookRequestDTO updateRequest)
    {
        var dataToMerge = new Dictionary<string, object?>();

        if (!string.IsNullOrEmpty(updateRequest.Title))
        {
            dataToMerge.Add(nameof(BookResponseDTO.Title), updateRequest.Title);
        }

        if (!string.IsNullOrEmpty(updateRequest.Author))
        {
            dataToMerge.Add(nameof(BookResponseDTO.Author), updateRequest.Author);
        }

        if (updateRequest.PublishYear != default(int))
        {
            dataToMerge.Add(nameof(BookResponseDTO.PublishYear), updateRequest.PublishYear);
        }

        return await _bookDbProvider.UpdateNexum(updateRequest.Id, dataToMerge);
    }
}