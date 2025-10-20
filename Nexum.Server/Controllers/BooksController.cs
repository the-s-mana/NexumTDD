using Microsoft.AspNetCore.Mvc;
using Nexum.Server.Data;
using Nexum.Server.Data.Models;
using Nexum.Server.Models.Book;
using Nexum.Server.Services;
using static System.Reflection.Metadata.BlobBuilder;

namespace Nexum.Server.Controllers;

[ApiController]
[Route("api/[controller]")] // URL จะเป็น /api/books
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

    // Nexum
    [HttpGet]
    [Route("GetAllBooksNexum")]
    public async Task<IActionResult> GetAllBooksNexum()
    {
        var books = await _bookService.GetAllBooksNexumAsync();
        return Ok(books);
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
}