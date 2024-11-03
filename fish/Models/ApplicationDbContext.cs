using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity; // Sử dụng cho Entity Framework 6
using fish.Models; // Thay "fish" bằng tên không gian tên của bạn nếu khác

namespace fish.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("DefaultConnection") { }

        public DbSet<User> Users { get; set; } // Tạo DbSet cho bảng Users
        public DbSet<Booking> Bookings { get; set; }
    }
}
