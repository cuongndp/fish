using System;
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
                Session["Role"] = user.Role; // Thêm vai trò của người dùng vào Session


                Console.WriteLine("Role after login: " + Session["Role"]);
               
                FormsAuthentication.SetAuthCookie(username, false);



                // Chuyển hướng đến trang dành cho Admin nếu người dùng là Admin
                if (user.Role == "Admin")
                {
                    return RedirectToAction("AdminOnlyAction", "Admin");
                }




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
        public ActionResult Register(string fullName, string email, string phoneNumber, string username, string role, string password/*, string confirmPassword*/, string rolePassword)
        {
            string doctorPassword = "khongcapchotao";
            string adminPassword = "taokhongcap";


            

            // Kiểm tra mật khẩu cấp quyền cho Bác Sĩ
            if (role == "Doctor" && rolePassword != doctorPassword)
            {
                ViewBag.Error = "Mật khẩu không đúng cho vai trò Bác Sĩ.";
                return View();
            }

            // Kiểm tra mật khẩu cấp quyền cho Quản Trị Viên
            if (role == "Admin" && rolePassword != adminPassword)
            {
                ViewBag.Error = "Mật khẩu không đúng cho vai trò Quản Trị Viên.";
                return View();
            }
            /*
            // Kiểm tra nếu mật khẩu xác nhận không khớp (chỉ dành cho người dùng thông thường)
            if (role == "Customer" && password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View();
            }
            */
            // Kiểm tra xem người dùng đã tồn tại chưa
            if (db.Users.Any(u => u.Username == username))
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại.";
                return View();
            }



            // Kiểm tra xem người dùng đã tồn tại chưa
            if (db.Users.Any(u => u.Email == email))
            {
                ViewBag.Error = "Email đã tồn tại.";
                return View();
            }


            if (db.Users.Any(u => u.PhoneNumber == phoneNumber))
            {
                ViewBag.Error = "Số điện thoại đã tồn tại.";
                return View();
            }


            // Tạo người dùng mới và thêm vào cơ sở dữ liệu
            var user = new User
            {
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                Username = username,
                Role = role, // Lưu vai trò của người dùng
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

        public ActionResult AccessDenied()
        {
            return View(); // Một trang thông báo về quyền truy cập bị từ chối
        }

        [Authorize(Roles = "Admin,Doctor")]
        public ActionResult AdminPage()
        {
            // Kiểm tra xem người dùng có vai trò Admin không
            if (Session["Role"]?.ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            // Nếu là Admin, cho phép truy cập
            return View();
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
        [Authorize]
        public ActionResult SubmitBooking(string ngayHen, string gioHen, string moTa)
        {


            if (Session["UserId"] == null)
            {
                ViewBag.Error = "Bạn cần đăng nhập để đặt lịch.";
                return View("~/Views/DichVu/Form.cshtml");
            }

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
                FullName = Session["FullName"]?.ToString(),
                PhoneNumber = Session["PhoneNumber"]?.ToString(),
                Email = Session["Email"]?.ToString(),
                NgayHen = parsedNgayHen,
                GioHen = parsedGioHen,
                MoTa = moTa,
                UserId = Convert.ToInt32(Session["UserId"]),
                 // Thêm ServiceId vào Booking
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



          public ActionResult RegisterWithRole(string role = null)
             {
                  ViewBag.SelectedRole = role; // Lưu vai trò đã chọn vào ViewBag
                 return View("Register");
             }

    }


    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext(); // Khai báo DbContext để làm việc với cơ sở dữ liệu

        [Authorize(Roles = "Admin,Doctor")]
        public ActionResult ManageServices()
        {
            // Lấy danh sách các dịch vụ từ cơ sở dữ liệu để hiển thị
            var services = db.Services.ToList();
            return View(services);
        }



        /*
        [Authorize]
       
        public ActionResult AdminOnlyAction(int? editUserId = null)
        {
            if (Session["Role"]?.ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.Users = db.Users.ToList();
            ViewBag.Services = db.Services.ToList();


            if (editUserId.HasValue)
            {
                var userToEdit = db.Users.Find(editUserId);
                if (userToEdit != null)
                {
                    ViewBag.UserToEdit = userToEdit;
                }
            }

        


            return View();
        }
        */







        public ActionResult AdminOnlyAction(int? editBookingId = null)
        {
            if (Session["Role"]?.ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy danh sách người dùng và dịch vụ từ database
            ViewBag.Users = db.Users.ToList();
            ViewBag.Services = db.Services.ToList();
            ViewBag.Bookings = db.Bookings.Include("Service").Include("User").ToList(); // Lấy danh sách bookings với thông tin liên quan

            // Kiểm tra nếu có editBookingId để chỉnh sửa thông tin booking
            if (editBookingId.HasValue)
            {
                var bookingToEdit = db.Bookings.Include("Service").Include("User").FirstOrDefault(b => b.Id == editBookingId);
                if (bookingToEdit != null)
                {
                    ViewBag.BookingToEdit = bookingToEdit;
                }
            }

            return View();
        }









            [Authorize(Roles = "Admin")]
        public ActionResult CreateService()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult CreateService(fish.Models.Service service)
        {
            if (ModelState.IsValid)
            {
                db.Services.Add(service);
                db.SaveChanges();
                return RedirectToAction("ManageServices");
            }
            return View(service);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult EditService(int id)
        {
            var service = db.Services.Find(id);
            if (service == null)
            {
                return HttpNotFound();
            }
            return View(service);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult EditService(fish.Models.Service service)
        {
            if (ModelState.IsValid)
            {
                db.Entry(service).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ManageServices");
            }
            return View(service);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken] // Sử dụng để xác thực mã AntiForgeryToken
        public ActionResult DeleteService(int id)
        {
            var service = db.Services.Find(id);
            if (service == null)
            {
                return HttpNotFound();
            }
            db.Services.Remove(service);
            db.SaveChanges();
            return RedirectToAction("ManageServices");
        }

        /*
        // Hiển thị danh sách lịch đặt của khách hàng
        [Authorize(Roles = "Admin,Doctor")]
        public ActionResult ManageBookings()
        {
            var bookings = db.Bookings.Include("Bookings").Include("Service").Include("User").ToList();
            ViewBag.Bookings = db.Bookings.ToList();// Gán danh sách lịch đặt vào ViewBag
            return View(bookings);
        }*/


        public ActionResult ManageBookings(int? editBookingId = null)
        {
            if (Session["Role"]?.ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy danh sách người dùng và dịch vụ từ database
            ViewBag.Users = db.Users.ToList();
            ViewBag.Services = db.Services.ToList();
            ViewBag.Bookings = db.Bookings.Include("Service").Include("User").ToList(); // Lấy danh sách bookings với thông tin liên quan

            // Kiểm tra nếu có editBookingId để chỉnh sửa thông tin booking
            if (editBookingId.HasValue)
            {
                var bookingToEdit = db.Bookings.Include("Service").Include("User").FirstOrDefault(b => b.Id == editBookingId);
                if (bookingToEdit != null)
                {
                    ViewBag.BookingToEdit = bookingToEdit;
                }
            }

            return View();
        }















        // Chọn bác sĩ để khám cho lịch đặt
        [Authorize(Roles = "Admin")]
        public ActionResult AssignDoctor(int id)
        {
            var booking = db.Bookings.Find(id);

            if (booking == null)
            {
                return HttpNotFound();
            }

            // Lấy danh sách bác sĩ từ cơ sở dữ liệu để chọn
            ViewBag.Doctors = db.Users.Where(u => u.Role == "Doctor").ToList();

            return View(booking);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult AssignDoctor(int id, int doctorId)
        {
            var booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }

            var doctor = db.Users.Find(doctorId);
            if (doctor == null || doctor.Role != "Doctor")
            {
                ViewBag.Error = "Bác sĩ không hợp lệ.";
                ViewBag.Doctors = db.Users.Where(u => u.Role == "Doctor").ToList();
                return View(booking);
            }

            // Gán bác sĩ cho lịch đặt
            booking.UserId = doctorId;
            db.Entry(booking).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("ManageBookings");
        }

        // Quản lý thông tin người dùng (thêm, sửa, xóa)
        [Authorize(Roles = "Admin")]
        public ActionResult ManageUsers()
        {
            var users = db.Users.ToList();
            return View(users);
        }


        [HttpPost]
        
        [ValidateAntiForgeryToken]
        public ActionResult EditUserInAdmin(User user)
        {
            if (Session["Role"]?.ToString() != "Admin")
            {
                
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                var existingUser = db.Users.Find(user.Id);
                if (existingUser == null)
                {
                    return HttpNotFound();
                }

                // Cập nhật thông tin người dùng
                existingUser.FullName = user.FullName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.Role = user.Role;

                db.Entry(existingUser).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("AdminOnlyAction");
            }

            // Nếu không thành công, truyền lại danh sách người dùng và dịch vụ
            ViewBag.Users = db.Users.ToList();
            ViewBag.Services = db.Services.ToList();
            ViewBag.UserToEdit = user;  // Giữ lại thông tin người dùng vừa nhập để hiển thị lại

            return View("AdminOnlyAction");
        }






        [HttpPost]
      
        [ValidateAntiForgeryToken] // Sử dụng để xác thực mã AntiForgeryToken
        public ActionResult DeleteUser(int id)
        {
            if (Session["Role"]?.ToString() != "Admin")
            {

                return RedirectToAction("Login", "Account");
            }
            var user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("AdminOnlyAction"); // Trang quản lý người dùng
        }

    }


}


