using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLHV.Models;

namespace QLHV.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly QlhvContext _context;

        public HomeController(ILogger<HomeController> logger, QlhvContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Kiểm tra role của user
            var userRole = HttpContext.Session.GetString("UserRole");
            
            // Nếu không phải Admin, chuyển hướng đến Profile
            if (userRole != "Admin")
            {
                return RedirectToAction("Profile");
            }

            // Chỉ Admin mới thấy dashboard
            ViewBag.TongUser = await _context.Users.CountAsync();
            return View();
        }

        private string GetUserRole()
        {
            return HttpContext.Session.GetString("UserRole") ?? "";
        }

        public async Task<IActionResult> Profile()
        {
            // Lấy thông tin user đang đăng nhập
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users
                .Include(u => u.MaNguoiNavigation)
                .Include(u => u.MaRoleNavigation)
                .FirstOrDefaultAsync(u => u.MaUser.ToString() == userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin khen thưởng và kỷ luật
            var khenThuongs = await _context.KhenThuongNguois
                .Include(kt => kt.MaKhenThuongNavigation)
                .Where(kt => kt.MaNguoi == user.MaNguoi)
                .OrderByDescending(kt => kt.NgayKhenThuong)
                .ToListAsync();

            var kyLuats = await _context.KyLuatNguois
                .Include(kl => kl.MaKyLuatNavigation)
                .Where(kl => kl.MaNguoi == user.MaNguoi)
                .OrderByDescending(kl => kl.NgayKyLuat)
                .ToListAsync();

            // Lấy thông tin kết quả học tập nếu là học viên
            var ketQuaHocTap = new List<KetQuaHocTap>();
            if (user.MaNguoiNavigation?.LoaiNguoi == "HocVien")
            {
                ketQuaHocTap = await _context.KetQuaHocTaps
                    .Include(kq => kq.MaHocKyNavigation)
                    .Where(kq => kq.MaNguoi == user.MaNguoi)
                    .OrderByDescending(kq => kq.MaHocKyNavigation.NamHoc)
                    .ThenBy(kq => kq.MaHocKyNavigation.MaHocKy)
                    .ToListAsync();
            }

            ViewBag.KhenThuongs = khenThuongs;
            ViewBag.KyLuats = kyLuats;
            ViewBag.KetQuaHocTap = ketQuaHocTap;

            return View(user);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // GET: Home/EditProfile
        public async Task<IActionResult> EditProfile()
        {
            // Lấy thông tin user đang đăng nhập
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users
                .Include(u => u.MaNguoiNavigation)
                .Include(u => u.MaRoleNavigation)
                .FirstOrDefaultAsync(u => u.MaUser.ToString() == userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Populate dropdown lists
            await PopulateDropDownListsAsync();

            return View(user);
        }

        // POST: Home/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(string HoTen, string? NgaySinh, string? CapBac, string? ChucVu, 
            string? ChuyenNganh, string? TruongHoc, string? KhoaHoc, string? NgayNhapHoc, string? NgayTotNghiep,
            string? Username, string? CurrentPassword, string? NewPassword, string? ConfirmPassword)
        {
            // Lấy thông tin user đang đăng nhập
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users
                .Include(u => u.MaNguoiNavigation)
                .FirstOrDefaultAsync(u => u.MaUser.ToString() == userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var hasAnyChange = false;

                    // Cập nhật thông tin cá nhân (bảng Nguoi)
                    if (user.MaNguoiNavigation != null)
                    {
                        user.MaNguoiNavigation.HoTen = HoTen;

                        if (!string.IsNullOrEmpty(NgaySinh) && DateOnly.TryParse(NgaySinh, out var ngaySinh))
                        {
                            user.MaNguoiNavigation.NgaySinh = ngaySinh;
                        }

                        user.MaNguoiNavigation.CapBac = CapBac;
                        user.MaNguoiNavigation.ChucVu = ChucVu;
                        user.MaNguoiNavigation.ChuyenNganh = ChuyenNganh;
                        user.MaNguoiNavigation.TruongHoc = TruongHoc;
                        user.MaNguoiNavigation.KhoaHoc = KhoaHoc;

                        if (!string.IsNullOrEmpty(NgayNhapHoc) && DateOnly.TryParse(NgayNhapHoc, out var ngayNhapHoc))
                        {
                            user.MaNguoiNavigation.NgayNhapHoc = ngayNhapHoc;
                        }

                        if (!string.IsNullOrEmpty(NgayTotNghiep) && DateOnly.TryParse(NgayTotNghiep, out var ngayTotNghiep))
                        {
                            user.MaNguoiNavigation.NgayTotNghiep = ngayTotNghiep;
                        }

                        // Cập nhật trường Đoàn viên
                        user.MaNguoiNavigation.DoanVien = Request.Form.ContainsKey("DoanVien");

                        hasAnyChange = true;
                    }

                    // Đổi tên đăng nhập nếu có
                    if (!string.IsNullOrWhiteSpace(Username) && Username != user.Username)
                    {
                        var existed = await _context.Users.AnyAsync(u => u.Username == Username && u.MaUser != user.MaUser);
                        if (existed)
                        {
                            ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                            await PopulateDropDownListsAsync();
                            return View(user);
                        }

                        user.Username = Username;
                        HttpContext.Session.SetString("Username", user.Username);
                        hasAnyChange = true;
                    }

                    // Đổi mật khẩu nếu có nhập
                    if (!string.IsNullOrWhiteSpace(CurrentPassword) || !string.IsNullOrWhiteSpace(NewPassword) || !string.IsNullOrWhiteSpace(ConfirmPassword))
                    {
                        if (string.IsNullOrWhiteSpace(CurrentPassword) || string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmPassword))
                        {
                            ModelState.AddModelError("Password", "Vui lòng nhập đủ mật khẩu hiện tại, mật khẩu mới và xác nhận mật khẩu mới");
                            await PopulateDropDownListsAsync();
                            return View(user);
                        }

                        if (user.PasswordHash != HashPassword(CurrentPassword))
                        {
                            ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng");
                            await PopulateDropDownListsAsync();
                            return View(user);
                        }

                        if (NewPassword != ConfirmPassword)
                        {
                            ModelState.AddModelError("ConfirmPassword", "Mật khẩu xác nhận không khớp");
                            await PopulateDropDownListsAsync();
                            return View(user);
                        }

                        user.PasswordHash = HashPassword(NewPassword);
                        hasAnyChange = true;
                    }

                    if (hasAnyChange)
                    {
                        _context.Update(user);
                        await _context.SaveChangesAsync();
                        TempData["Success"] = "Cập nhật thành công";
                    }

                    return RedirectToAction("Profile");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật thông tin: " + ex.Message);
                }
            }

            // Populate dropdown lists for error case
            await PopulateDropDownListsAsync();
            return View(user);
        }

        private string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return System.Convert.ToBase64String(hashedBytes);
        }

        private async Task PopulateDropDownListsAsync()
        {
            // Populate các dropdown cần thiết cho form edit
            ViewBag.MaTrinhDo = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.TrinhDos, "MaTrinhDo", "TenTrinhDo");
            ViewBag.MaLoaiDangVien = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.LoaiDangViens, "MaLoaiDangVien", "TenLoai");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
