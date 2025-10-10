using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexum.Server.Models
{
    /// <summary>
    /// Base class สำหรับ entity ทั้งหมดที่มี audit fields
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// วันที่สร้างข้อมูล
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// ผู้สร้างข้อมูล
        /// </summary>
        public string? CreateBy { get; set; }

        /// <summary>
        /// วันที่แก้ไขข้อมูลล่าสุด
        /// </summary>
        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// ผู้แก้ไขข้อมูลล่าสุด
        /// </summary>
        public string? UpdateBy { get; set; }
    }
}
