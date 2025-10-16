using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Nexum.Server.Data;
using Nexum.Server.Data.Models;
using Nexum.Server.Services;

namespace Nexum.Server.Controllers;

[ApiController]
[Route("api/[controller]")] // URL จะเป็น /api/books
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    // 1. รับ IBookService ผ่าน Dependency Injection
    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    // 2. สร้าง Endpoint สำหรับ "GET" เพื่อดึงข้อมูลทั้งหมด
    // GET /api/books
    [HttpGet]
    public async Task<IActionResult> GetAllBooks()
    {
        var books = await _bookService.GetAllBooksAsync();
        return Ok(books); // ส่งผลลัพธ์กลับไปเป็น HTTP 200 OK พร้อมข้อมูล JSON
    }

    // 3. สร้าง Endpoint สำหรับ "POST" เพื่อสร้างข้อมูลใหม่
    // POST /api/books
    [HttpPost]
    public async Task<IActionResult> CreateBook([FromBody] Book book)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdBook = await _bookService.CreateBookAsync(book);

        // ส่ง HTTP 201 Created พร้อม Location ของ Resource ที่สร้างใหม่
        return CreatedAtAction(nameof(GetAllBooks), new { id = createdBook.Id.ToString() }, createdBook);
    }
}