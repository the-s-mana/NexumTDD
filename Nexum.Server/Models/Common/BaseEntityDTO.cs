using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexum.Server.Models
{
    public abstract class BaseEntityDTO
    {
        public DateTime CreateDate { get; set; }
        public string? CreateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public string? UpdateBy { get; set; }
    }
}
