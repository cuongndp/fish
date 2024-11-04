using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace fish.Models
{
    public class Service
    {
        public int Id { get; set; } // Khóa chính
        public string TenDichVu { get; set; } // Tên dịch vụ
        public string MoTaDichVu { get; set; } // Mô tả dịch vụ
        public decimal Gia { get; set; } // Giá của dịch vụ

        // Quan hệ: Một dịch vụ có thể có nhiều booking
        public virtual ICollection<Booking> Bookings { get; set; }
    }
}
