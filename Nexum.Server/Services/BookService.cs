
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Nexum.Server.DAC;
using Nexum.Server.Data;
using Nexum.Server.Data.Models;
using Nexum.Server.Extensions;
using Nexum.Server.Models.Book;
using Nexum.Server.Models.CreditWallet;
using SurrealDb.Net.Models;
using System.Text;
using static Nexum.Server.Data.IDbProviderFactory;

namespace Nexum.Server.Services;
public interface IBookService
{
    // Surreal
    Task<List<BookResponseDTO>> GetAllBooksSurrealAsync();
    Task<BookResponseDTO> GetBookByIdSurrealAsync(string id);
    Task<BookResponseDTO> CreateBookSurrealAsync(CreateBookRequestDTO creatRequest);
    Task<BookResponseDTO> UpdateBookSurrealAsync(UpdateBookRequestDTO updateRequest);
    Task<BookResponseDTO> UpsertBookSurrealAsync(UpdateBookRequestDTO creatRequest);
    Task DeleteBookSurrealAsync(string id);

    // Nexum
    Task<List<BookResponseDTO>> GetAllBooksNexumAsync();
    Task<BookResponseDTO> GetBookByIdNexumAsync(string id);
    Task<BookResponseDTO> CreateBookNexumAsync(CreateBookRequestDTO creatRequest);
    Task<BookResponseDTO> UpdateBookNexumAsync(UpdateBookRequestDTO updateRequest);
    Task<BookResponseDTO> UpsertBookNexumAsync(UpdateBookRequestDTO creatRequest);
    Task DeleteBookNexumAsync(string id);

    // Query
    Task DeleteBooksRawQueryAsync(string? authorName, int? publishedBeforeYear);
    Task<IEnumerable<BookResponseDTO>> GetBooksRawQuerySurrealAsync(string? titleName, string? authorName, int? publishedBeforeYear);
    Task<IEnumerable<BookResponseDTO>> GetBooksRawQueryNexumAsync(string? titleName, string? authorName, int? publishedBeforeYear);
}
public class BookService : IBookService
{
    private readonly ISurrealDbProvider<Book, BookResponseDTO> _bookDbProvider;
    private readonly IMapper _mapper;

    public BookService(ISurrealDbProvider<Book, BookResponseDTO> bookDbProvider
        , SurrealDbProviderFactoryBase surrealDbProviderFactory
        , IMapper mapper)
    {
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

    public async Task<BookResponseDTO> GetBookByIdSurrealAsync(string id)
    {
        var bookModel = await _bookDbProvider.GetByIdSurreal(id);
        var bookDto = _mapper.Map<BookResponseDTO>(bookModel);
        return bookDto;
    }

    public async Task<BookResponseDTO> CreateBookSurrealAsync(CreateBookRequestDTO creatRequest)
    {
        Book create = new Book
        {
            Title = creatRequest.Title,
            Author = creatRequest.Author,
            PublishYear = creatRequest.PublishYear,
            StoreId = RecordId.From(nameof(Store), creatRequest.StoreId),
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

    public async Task<BookResponseDTO> UpsertBookSurrealAsync(UpdateBookRequestDTO upsertRequest)
    {
        Book create = new Book
        {
            Id = !string.IsNullOrEmpty(upsertRequest.Id) ? upsertRequest.Id.StringToRecordId<Book>() : null,
            Title = upsertRequest.Title,
            Author = upsertRequest.Author,
            PublishYear = upsertRequest.PublishYear,
            StoreId = RecordId.From(nameof(Store), upsertRequest.StoreId),
        };

        var res = await _bookDbProvider.UpsertSurreal(create);
        BookResponseDTO dtos = _mapper.Map<BookResponseDTO>(res);
        return dtos;
    }

    public async Task DeleteBookSurrealAsync(string id)
    {
        await _bookDbProvider.DeleteSurreal(id);
    }

    // Nexum
    public async Task<List<BookResponseDTO>> GetAllBooksNexumAsync()
    {
        var booksEnumerable = await _bookDbProvider.ListNexum();

        return booksEnumerable.ToList();
    }

    public async Task<BookResponseDTO> GetBookByIdNexumAsync(string id)
    {
        return await _bookDbProvider.GetByIdNexum(id);
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

    public async Task<BookResponseDTO> UpsertBookNexumAsync(UpdateBookRequestDTO upsertRequest)
    {
        BookResponseDTO create = new BookResponseDTO
        {
            Id = upsertRequest.Id,
            Title = upsertRequest.Title,
            Author = upsertRequest.Author,
            PublishYear = upsertRequest.PublishYear,
            StoreId = upsertRequest.StoreId
        };
        return await _bookDbProvider.UpsertNexum(create);
    }
    public async Task DeleteBookNexumAsync(string id)
    {
        await _bookDbProvider.DeleteNexum(id);
    }

    // Query
    public async Task DeleteBooksRawQueryAsync(
        string? authorName,
        int? publishedBeforeYear)
    {
        var sqlBuilder = new StringBuilder("DELETE Book WHERE 1=1");
        var parameters = new Dictionary<string, object?>();

        if (!string.IsNullOrEmpty(authorName))
        {
            sqlBuilder.Append(" AND Author = $author");
            parameters.Add("author", authorName);
        }

        if (publishedBeforeYear.HasValue && publishedBeforeYear.Value > 0)
        {
            sqlBuilder.Append(" AND PublishYear < $year");
            parameters.Add("year", publishedBeforeYear.Value);
        }

        if (parameters.Count == 0)
        {
            throw new ArgumentException("At least one filter is required to delete books.");
        }

        await _bookDbProvider.RawQuery(
            sqlBuilder.ToString(),
            parameters
        );
    }

    public async Task<IEnumerable<BookResponseDTO>> GetBooksRawQuerySurrealAsync(
        string? titleName,
        string? authorName,
        int? publishedBeforeYear)
    {
        var sqlBuilder = new StringBuilder("SELECT * FROM Book WHERE 1=1");
        var parameters = new Dictionary<string, object?>();

        if (!string.IsNullOrEmpty(titleName))
        {
            sqlBuilder.Append(" AND Title = $title");
            parameters.Add("title", titleName);
        }

        if (!string.IsNullOrEmpty(authorName))
        {
            sqlBuilder.Append(" AND Author = $author");
            parameters.Add("author", authorName);
        }

        if (publishedBeforeYear.HasValue && publishedBeforeYear.Value > 0)
        {
            sqlBuilder.Append(" AND PublishYear < $year");
            parameters.Add("year", publishedBeforeYear.Value);
        }

        var books = await _bookDbProvider.RawQuerySurreal<Book>(
            sqlBuilder.ToString(),
            parameters
        );

        return _mapper.Map<IEnumerable<BookResponseDTO>>(books);
    }

    public async Task<IEnumerable<BookResponseDTO>> GetBooksRawQueryNexumAsync(
        string? titleName,
        string? authorName,
        int? publishedBeforeYear)
    {
        var sqlBuilder = new StringBuilder("SELECT * FROM Book WHERE 1=1");
        var parameters = new Dictionary<string, object?>();

        if (!string.IsNullOrEmpty(titleName))
        {
            sqlBuilder.Append(" AND Title = $title");
            parameters.Add("title", titleName);
        }

        if (!string.IsNullOrEmpty(authorName))
        {
            sqlBuilder.Append(" AND Author = $author");
            parameters.Add("author", authorName);
        }

        if (publishedBeforeYear.HasValue && publishedBeforeYear.Value > 0)
        {
            sqlBuilder.Append(" AND PublishYear < $minYear");
            parameters.Add("minYear", publishedBeforeYear.Value);
        }

        var bookDtos = await _bookDbProvider.RawQueryNexum<BookResponseDTO>(
            sqlBuilder.ToString(),
            parameters
        );

        return bookDtos;
    }
}