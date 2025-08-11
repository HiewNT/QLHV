using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLHV.Models;
using System.Security.Cryptography;
using System.Text;

namespace QLHV.Controllers
{
    public class UserController : Controller
    {
        private readonly QlhvContext _context;

        public UserController(QlhvContext context)
        {
            _context = context;
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.MaRoleNavigation)
                .Include(u => u.MaNguoiNavigation)
                .Where(u => u.Username != "admin") // Ẩn user admin
                .ToListAsync();
            return View(users);
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.MaRoleNavigation)
                .Include(u => u.MaNguoiNavigation)
                .FirstOrDefaultAsync(m => m.MaUser == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: User/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropDownListsAsync(true);
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string Username, string MaNguoi, int MaRole, string password, bool Active = true)
        {
            // Validation cho password
            if (string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("password", "Mật khẩu không được để trống");
            }
            else if (password.Length < 1)
            {
                ModelState.AddModelError("password", "Mật khẩu phải có ít nhất 1 ký tự");
            }

            // Validation cho các trường khác
            if (string.IsNullOrEmpty(Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập không được để trống");
            }

            if (string.IsNullOrEmpty(MaNguoi))
            {
                ModelState.AddModelError("MaNguoi", "Vui lòng chọn người");
            }

            if (MaRole == 0)
            {
                ModelState.AddModelError("MaRole", "Vui lòng chọn vai trò");
            }

            if (ModelState.IsValid)
            {
                // Kiểm tra username đã tồn tại
                if (await _context.Users.AnyAsync(u => u.Username == Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                    await PopulateDropDownListsAsync(true);
                    return View();
                }

                var user = new User
                {
                    Username = Username,
                    MaNguoi = MaNguoi,
                    MaRole = MaRole,
                    PasswordHash = HashPassword(password),
                    Active = Active
                };
                
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            await PopulateDropDownListsAsync(true);
            return View();
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.MaRoleNavigation)
                .Include(u => u.MaNguoiNavigation)
                .FirstOrDefaultAsync(m => m.MaUser == id);
            if (user == null)
            {
                return NotFound();
            }
            
            await PopulateDropDownListsAsync(false);
            return View(user);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Edit")]
        public async Task<IActionResult> EditPost(int id, string Username, string MaNguoi, int MaRole, string newPassword, bool Active = true)
        {
            // Validation cho các trường
            if (string.IsNullOrEmpty(Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập không được để trống");
            }

            if (string.IsNullOrEmpty(MaNguoi))
            {
                ModelState.AddModelError("MaNguoi", "Vui lòng chọn người");
            }

            if (MaRole == 0)
            {
                ModelState.AddModelError("MaRole", "Vui lòng chọn vai trò");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users.FindAsync(id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }
                    
                    // Kiểm tra username đã tồn tại (trừ user hiện tại)
                    if (await _context.Users.AnyAsync(u => u.Username == Username && u.MaUser != id))
                    {
                        ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                        await PopulateDropDownListsAsync(false);
                        return View(existingUser);
                    }
                    
                    existingUser.Username = Username;
                    existingUser.MaNguoi = MaNguoi;
                    existingUser.MaRole = MaRole;
                    existingUser.Active = Active;
                    
                    // Nếu có mật khẩu mới, cập nhật
                    if (!string.IsNullOrEmpty(newPassword))
                    {
                        existingUser.PasswordHash = HashPassword(newPassword);
                    }
                    
                    _context.Update(existingUser);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            
            await PopulateDropDownListsAsync(false);
            var user = await _context.Users
                .Include(u => u.MaRoleNavigation)
                .Include(u => u.MaNguoiNavigation)
                .FirstOrDefaultAsync(m => m.MaUser == id);
            return View(user);
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.MaRoleNavigation)
                .Include(u => u.MaNguoiNavigation)
                .FirstOrDefaultAsync(m => m.MaUser == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.MaUser == id);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private async Task PopulateDropDownListsAsync(bool forCreate = true)
        {
            if (forCreate)
            {
                // Lấy danh sách người chưa có tài khoản
                var existingUserMaNguois = await _context.Users.Select(u => u.MaNguoi).ToListAsync();
                var availableNguois = await _context.Nguois
                    .Where(n => !existingUserMaNguois.Contains(n.MaNguoi))
                    .OrderBy(n => n.HoTen)
                    .Select(n => new
                    {
                        n.MaNguoi,
                        DisplayText = $"{n.HoTen} - {n.ChucVu} ({n.LoaiNguoi})"
                    })
                    .ToListAsync();
                
                ViewBag.MaNguoi = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(availableNguois, "MaNguoi", "DisplayText");
            }
            else
            {
                // Trong Edit, hiển thị tất cả người
                var allNguois = await _context.Nguois
                    .OrderBy(n => n.HoTen)
                    .Select(n => new
                    {
                        n.MaNguoi,
                        DisplayText = $"{n.HoTen} - {n.ChucVu} ({n.LoaiNguoi})"
                    })
                    .ToListAsync();
                
                ViewBag.MaNguoi = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(allNguois, "MaNguoi", "DisplayText");
            }
            
            ViewBag.MaRole = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Roles, "MaRole", "TenRole");
        }

        private void AddModelErrors()
        {
            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        ModelState.AddModelError("", error.ErrorMessage);
                    }
                }
            }
        }
    }
}
