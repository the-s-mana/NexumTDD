namespace Nexum.Server.Models.Book;
public class BookResponseDTO
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public int PublishYear { get; set; }
    public string? StoreId { get; set; }
}