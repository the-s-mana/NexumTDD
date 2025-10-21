using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Nexum.Server.Models.Book;
public class CreateBookRequestDTO
{
    [Required]
    [DefaultValue("test1")]
    public string Title { get; set; }

    [Required]
    [DefaultValue("test1")]
    public string Author { get; set; }
    [Required]
    [DefaultValue(10)]
    public int PublishYear { get; set; }
    [DefaultValue("ohk845qxa6pmmtckn3xk")]
    public string StoreId { get; set; }
}