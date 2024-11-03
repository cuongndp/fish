using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
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
            // Chuyển hướng đến trang đăng nhập hoặc trang chủ sau khi đăng xuất
            return RedirectToAction("Login", "Account");
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
