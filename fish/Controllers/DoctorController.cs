using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Services.Description;
using fish.Models; // Import model User và DbContext
using CustomService = fish.Models.Service;
namespace fish.Controllers
{
    public class DoctorController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        
        public ActionResult DoctorSchedule()
        {
            if (Session["UserId"] == null || Session["Role"]?.ToString() != "Doctor")
            {
                // Nếu chưa đăng nhập hoặc không phải là bác sĩ, chuyển hướng về trang đăng nhập
                return RedirectToAction("Login", "Account");
            }





            // Lấy danh sách các lịch khám mà bác sĩ được gán
            int doctorId = Convert.ToInt32(Session["UserId"]);
            var bookings = db.Bookings.Where(b => b.DoctorId == doctorId).ToList();

            return View("~/Views/BacSi/DoctorSchedule.cshtml", bookings);
        }
    }

}