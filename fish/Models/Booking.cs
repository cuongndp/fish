using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

// Models/Booking.cs
namespace fish.Models
{
    public class Booking
    {
        public int Id { get; set; } // Khóa chính

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public DateTime NgayHen { get; set; }

        public TimeSpan GioHen { get; set; }


        public string MoTa { get; set; }



        public int UserId { get; set; } // Khóa ngoại
        public virtual User User { get; set; } // Navigation property
    }
}
