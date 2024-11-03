using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using System.Globalization;
using System.Web.Security;
using fish.Models; // Import model User và DbContext

namespace fish.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext(); // Khai báo DbContext để làm việc với cơ sở dữ liệu

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        
        // POST: Account/Login
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            // Hash mật khẩu trước khi kiểm tra với cơ sở dữ liệu
            string hashedPassword = HashPassword(password);

            var user = db.Users.FirstOrDefault(u => u.Username == username && u.Password == hashedPassword);
            if (user != null)
            {
                // Đăng nhập thành công
                Session["UserId"] = user.Id;
                Session["FullName"] = user.FullName;
                Session["PhoneNumber"] = user.PhoneNumber; // Lưu số điện thoại vào Session
                Session["Email"] = user.Email;


                FormsAuthentication.SetAuthCookie(username, false);

                return RedirectToAction("Index", "Home");
            }

            // Xác thực thất bại
            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không chính xác.";
            return View();
        }


        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        public ActionResult Register(string fullName, string email, string phoneNumber, string username, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu không khớp.";
                return View();
            }

            // Kiểm tra xem người dùng đã tồn tại chưa
            if (db.Users.Any(u => u.Username == username))
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại.";
                return View();
            }

            // Tạo người dùng mới và thêm vào cơ sở dữ liệu
            // Tạo người dùng mới và thêm vào cơ sở dữ liệu
            var user = new User
            {
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                Username = username,
                Password = HashPassword(password) // Lưu mật khẩu đã hash để bảo mật
            };
            db.Users.Add(user);
            db.SaveChanges();


            // Đăng ký thành công, chuyển hướng đến trang đăng nhập
            return RedirectToAction("Login");
        }


        public ActionResult Logout()
        {
            // Xóa tất cả thông tin lưu trữ trong Session
            Session.Clear();

            // Xóa cookie xác thực Forms
            FormsAuthentication.SignOut();

            // Chuyển hướng đến trang chủ sau khi đăng xuất
            return RedirectToAction("Index", "Home");
        }




        [Authorize]
        public ActionResult BookingForm()
        {
            // Gỡ lỗi: kiểm tra xem Session có hoạt động không
            if (Session["UserId"] == null)
            {
                // Nếu không có thông tin đăng nhập, chuyển hướng về trang đăng nhập
                return RedirectToAction("Login", "Account");
            }


            return View("~/Views/DichVu/Form.cshtml");
        }




        [HttpPost]
        [Authorize] // Cũng yêu cầu đăng nhập khi gửi dữ liệu
        public ActionResult SubmitBooking(string fullName, string phoneNumber, string email, string ngayHen, string gioHen, string moTa)
        {
            DateTime parsedNgayHen;
            TimeSpan parsedGioHen;

            // Chuyển đổi chuỗi ngày thành DateTime
            string[] formats = { "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy", "dd-MM-yyyy", "yyyy/MM/dd" };

            if (!DateTime.TryParseExact(ngayHen, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedNgayHen))
            {
                ViewBag.Error = "Ngày hẹn không hợp lệ.";
                return View("~/Views/DichVu/Form.cshtml");
            }





            // Chuyển đổi chuỗi giờ thành TimeSpan
            if (!TimeSpan.TryParse(gioHen, out parsedGioHen))
            {
                ViewBag.Error = "Giờ hẹn không hợp lệ.";
                return View("~/Views/DichVu/Form.cshtml");
            }

            // Tạo một đối tượng Booking mới
            var booking = new Booking
            {
                FullName = Session["FullName"]?.ToString(), // Lấy từ Session
                PhoneNumber = Session["PhoneNumber"]?.ToString(), // Lấy từ Session
                Email = Session["Email"]?.ToString(), // Lấy từ Session
                NgayHen = parsedNgayHen, // Sử dụng DateTime cho Ngày Hẹn
                GioHen = parsedGioHen,   // Sử dụng TimeSpan cho Giờ Hẹn
                MoTa = moTa,             // Gán mô tả từ form
                UserId = Convert.ToInt32(Session["UserId"]) // Gán UserId từ Session
            };

            // Thêm đối tượng vào DbSet và lưu thay đổi
            try
            {
                db.Bookings.Add(booking);
                db.SaveChanges();
                ViewBag.Message = "Đặt lịch thành công!";
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Đã xảy ra lỗi: " + ex.Message;
                return View("~/Views/DichVu/Form.cshtml");
            }


            return View("~/Views/DichVu/Form.cshtml");

        }





        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }





    
    

}
