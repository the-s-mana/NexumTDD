using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Nexum.Server.Models.Book;
public class UpdateBookRequestDTO
{
    [DefaultValue("")]
    public string Id { get; set; }
    [Required]
    [DefaultValue("test999")]
    public string Title { get; set; }

    [Required]
    [DefaultValue("test999")]
    public string Author { get; set; }
    [Required]
    [DefaultValue(999)]
    public int PublishYear { get; set; }
 
}