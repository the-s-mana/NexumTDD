using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Nexum.Server.Models.Book;
public class UpdateBookRequestDTO
{
    [DefaultValue("2tg9soxbn9nkgx0lclj6")]
    public string Id { get; set; }
    [Required]
    [DefaultValue("test1")]
    public string Title { get; set; }

    [Required]
    [DefaultValue("test1")]
    public string Author { get; set; }
    [Required]
    [DefaultValue(10)]
    public int PublishYear { get; set; }
 
}