using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLHV.Models;
using System.Security.Cryptography;
using System.Text;

namespace QLHV.Controllers
{
    public class AccountController : Controller
    {
        private readonly QlhvContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountController(QlhvContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            if (IsUserLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin đăng nhập");
                return View();
            }

            var user = await _context.Users
                .Include(u => u.MaNguoiNavigation)
                .Include(u => u.MaRoleNavigation)
                .FirstOrDefaultAsync(u => u.Username == username && u.Active == true);

            if (user == null)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
                return View();
            }

            // Kiểm tra mật khẩu
            var hashedPassword = HashPassword(password);
            if (user.PasswordHash != hashedPassword)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
                return View();
            }

            // Lưu thông tin vào session
            HttpContext.Session.SetString("UserId", user.MaUser.ToString());
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("UserRole", user.MaRoleNavigation.TenRole);
            HttpContext.Session.SetString("UserFullName", user.MaNguoiNavigation?.HoTen ?? "");

            // Chuyển hướng dựa trên role
            if (user.MaRoleNavigation.TenRole == "Admin")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Profile", "Home");
            }
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            if (IsUserLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string username, string password, string confirmPassword, string maNguoi)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(maNguoi))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin");
                return View();
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không khớp");
                return View();
            }

            // Kiểm tra username đã tồn tại
            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                ModelState.AddModelError("", "Tên đăng nhập đã tồn tại");
                return View();
            }

            // Kiểm tra mã người có tồn tại
            var nguoi = await _context.Nguois.FindAsync(maNguoi);
            if (nguoi == null)
            {
                ModelState.AddModelError("", "Mã người không tồn tại trong hệ thống");
                return View();
            }

            // Tạo user mới với role mặc định (có thể thay đổi)
            var newUser = new User
            {
                Username = username,
                PasswordHash = HashPassword(password),
                MaNguoi = maNguoi,
                MaRole = 2, // Role mặc định (có thể thay đổi)
                Active = true
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        // Helper methods
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == hash;
        }

        private bool IsUserLoggedIn()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            return !string.IsNullOrEmpty(session.GetString("UserId"));
        }
    }
}
