using Microsoft.AspNetCore.Mvc;
using Nexum.Server.Data;
using Nexum.Server.Data.Models;
using Nexum.Server.Models.Book;
using Nexum.Server.Services;
using System.ComponentModel;
using static System.Reflection.Metadata.BlobBuilder;

namespace Nexum.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;
    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    // Surreal
    [HttpGet]
    [Route("GetAllSurreal")]
    public async Task<IActionResult> GetAllBooksSurreal()
    {
        var res = await _bookService.GetAllBooksSurrealAsync();
        return Ok(res);
    }

    [HttpGet("GetBookByIdSurreal/{id}")]
    public async Task<ActionResult<BookResponseDTO>> GetBookByIdSurreal(string id)
    {
        var bookDto = await _bookService.GetBookByIdSurrealAsync(id);

        if (bookDto == null)
            return NotFound();

        return Ok(bookDto);
    }

    [HttpPost]
    [Route("CreateBookSurreal")]
    public async Task<IActionResult> CreateBookSurreal(CreateBookRequestDTO book)
    {
        var res = await _bookService.CreateBookSurrealAsync(book);
        return Ok(res);
    }

    [HttpPut]
    [Route("UpdateBookSurreal")]
    public async Task<IActionResult> UpdateBookSurreal(UpdateBookRequestDTO book)
    {
        var res = await _bookService.UpdateBookSurrealAsync(book);
        return Ok(res);
    }

    [HttpPost]
    [Route("UpsertBookSurreal")]
    public async Task<IActionResult> UpsertBookSurreal(UpdateBookRequestDTO book)
    {
        var res = await _bookService.UpsertBookSurrealAsync(book);

        return Ok(res);
    }

    [HttpDelete("DeleteBookSurreal/{id}")]
    public async Task<IActionResult> DeleteBookSurreal(string id)
    {
        await _bookService.DeleteBookSurrealAsync(id);
        return NoContent();
    }

    // Nexum
    [HttpGet]
    [Route("GetAllBooksNexum")]
    public async Task<IActionResult> GetAllBooksNexum()
    {
        var books = await _bookService.GetAllBooksNexumAsync();
        return Ok(books);
    }

    [HttpGet("GetBookByIdNexum/{id}")]
    public async Task<ActionResult<BookResponseDTO>> GetBookByIdNexum(string id)
    {
        var bookDto = await _bookService.GetBookByIdNexumAsync(id);

        if (bookDto == null)
            return NotFound();

        return Ok(bookDto);
    }

    [HttpPost]
    [Route("CreateBookNexum")]
    public async Task<IActionResult> CreateBookNexum(CreateBookRequestDTO book)
    {
        var res = await _bookService.CreateBookNexumAsync(book);

        return Ok(res);
    }

    [HttpPut]
    [Route("UpdateBookNexum")]
    public async Task<IActionResult> UpdateBookNexum(UpdateBookRequestDTO book)
    {
        var res = await _bookService.UpdateBookNexumAsync(book);
        return Ok(res);
    }

    [HttpPost]
    [Route("UpsertBookNexum")]
    public async Task<IActionResult> UpsertBookNexum(UpdateBookRequestDTO book)
    {
        var res = await _bookService.UpsertBookNexumAsync(book);

        return Ok(res);
    }

    [HttpDelete("DeleteBookNexum/{id}")]
    public async Task<IActionResult> DeleteBookNexum(string id)
    {
        await _bookService.DeleteBookNexumAsync(id);
        return NoContent();
    }

    // Query
    [HttpDelete("DeleteBooksRawQuery")]
    public async Task<IActionResult> DeleteBooksRawQuery(
        [DefaultValue("test1")] string? authorName,
        [DefaultValue(99)] int? publishedBeforeYear)
    {
        if (string.IsNullOrEmpty(authorName) && !publishedBeforeYear.HasValue)
        {
            return BadRequest("At least one filter is required to delete books.");
        }

        await _bookService.DeleteBooksRawQueryAsync(
            authorName,
            publishedBeforeYear
        );

        return NoContent();
    }

    [HttpGet("GetBooksRawQuerySurreal")]
    public async Task<ActionResult<IEnumerable<BookResponseDTO>>> GetBooksRawQuerySurreal(
        [DefaultValue("test1")] string? titleName,
        [DefaultValue("test1")] string? authorName,
        [DefaultValue(99)] int? publishedBeforeYear)
    {
        var books = await _bookService.GetBooksRawQuerySurrealAsync(
            titleName,
            authorName,
            publishedBeforeYear
        );

        return Ok(books);
    }

    [HttpGet("GetBooksRawQueryNexum")]
    public async Task<ActionResult<IEnumerable<BookResponseDTO>>> GetBooksRawQueryNexum(
        [DefaultValue("test1")] string? titleName,
        [DefaultValue("test1")] string? authorName,
        [DefaultValue(99)] int? publishedBeforeYear)
    {
        var books = await _bookService.GetBooksRawQueryNexumAsync(
            titleName,
            authorName,
            publishedBeforeYear
        );

        return Ok(books);
    }
}