namespace Nexum.Server.Data.Models;
public record Book
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public int PublishYear { get; set; }

    // Constructor ที่ไม่มี Id สำหรับสร้าง Object ใหม่
    public Book(string title, string author, int publishYear)
    {
        Title = title;
        Author = author;
        PublishYear = publishYear;
    }
}