using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLHV.Models;

namespace QLHV.Controllers
{
    public class DangVienController : Controller
    {
        private readonly QlhvContext _context;
        public DangVienController(QlhvContext context)
        {
            _context = context;
        }

        // GET: DangVien/Index
        public async Task<IActionResult> Index()
        {
            // Kiểm tra quyền truy cập
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole))
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy danh sách loại người
            ViewData["LoaiNguois"] = new List<string>
            {
                "NhanVien",
                "HocVien"
            };

            // Lấy danh sách quốc tịch dựa trên role
            var quocTichs = new List<string>();
            if (userRole == "Admin")
            {
                quocTichs.AddRange(new[] { "Lào", "Campuchia" });
            }
            else if (userRole == "LopTruongLao")
            {
                quocTichs.Add("Lào");
            }
            else if (userRole == "LopTruongCam")
            {
                quocTichs.Add("Campuchia");
            }
            ViewData["QuocTichs"] = quocTichs;

            // Lấy danh sách loại đảng viên (trừ Quần chúng)
            ViewData["LoaiDangViens"] = await _context.LoaiDangViens
                .Where(l => l.TenLoai != "Quần chúng")
                .Select(l => l.TenLoai)
                .ToListAsync();

            // Lưu role vào ViewBag để view có thể sử dụng
            ViewBag.UserRole = userRole;

            return View();
        }

        // GET: DangVien/GetData
        public async Task<IActionResult> GetData(string? loaiNguoi = null, string? quocTich = null, string? loaiDangVien = null)
        {
            // Kiểm tra quyền truy cập
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole))
            {
                return Json(new { data = new List<object>() });
            }

            var query = _context.Nguois
                .Include(n => n.MaLoaiDangVienNavigation)
                .Where(n => n.MaLoaiDangVien != null && n.MaLoaiDangVienNavigation != null)
                .Where(n => n.MaLoaiDangVienNavigation!.TenLoai != "Quần chúng");

            // Áp dụng phân quyền theo role
            if (userRole == "LopTruongLao")
            {
                // Chỉ hiển thị học viên Lào
                query = query.Where(n => n.LoaiNguoi == "HocVien" && n.QuocTich == "Lào");
            }
            else if (userRole == "LopTruongCam")
            {
                // Chỉ hiển thị học viên Campuchia
                query = query.Where(n => n.LoaiNguoi == "HocVien" && n.QuocTich == "Campuchia");
            }
            // Admin có thể xem tất cả, không cần lọc thêm

            // Lọc theo loại người
            if (!string.IsNullOrEmpty(loaiNguoi))
            {
                query = query.Where(n => n.LoaiNguoi == loaiNguoi);
            }

            // Lọc theo quốc tịch cho học viên (chỉ áp dụng nếu là Admin)
            if (!string.IsNullOrEmpty(quocTich) && userRole == "Admin" && query.Any(n => n.LoaiNguoi == "HocVien"))
            {
                query = query.Where(n => n.QuocTich == quocTich);
            }

            // Lọc theo loại đảng viên
            if (!string.IsNullOrEmpty(loaiDangVien))
            {
                query = query.Where(n => n.MaLoaiDangVienNavigation!.TenLoai == loaiDangVien);
            }

            var danhSachDangVien = await query
                .Select(n => new
                {
                    Id = n.MaNguoi,
                    n.HoTen,
                    n.LoaiNguoi,
                    n.QuocTich,
                    LoaiDangVien = n.MaLoaiDangVienNavigation!.TenLoai,
                    NgayVaoDangDuBi = n.NgayVaoDangDuBi,
                    NgayVaoDangChinhThuc = n.NgayVaoDangChinhThuc,
                    n.CapBac,
                    n.ChucVu
                })
                .ToListAsync();

            return Json(new { data = danhSachDangVien });
        }

        // GET: DangVien/DetailsHocVien/5
        public async Task<IActionResult> DetailsHocVien(string id)
        {
            if (id == null) return NotFound();

            // Kiểm tra quyền truy cập
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole))
            {
                return RedirectToAction("Login", "Account");
            }

            var hocVien = await _context.Nguois
                .Include(h => h.MaLoaiDangVienNavigation)
                .Include(h => h.MaTrinhDoNavigation)
                .FirstOrDefaultAsync(h => h.MaNguoi == id);

            if (hocVien == null) return NotFound();

            // Kiểm tra quyền truy cập vào học viên cụ thể
            if (!CanAccessHocVien(hocVien, userRole))
            {
                TempData["Error"] = "Bạn không có quyền truy cập vào thông tin học viên này.";
                return RedirectToAction("Index");
            }

            ViewData["LoaiDangViens"] = new SelectList(
                await _context.LoaiDangViens
                    .Where(l => l.TenLoai != "Quần chúng")
                    .ToListAsync(),
                "MaLoaiDangVien",
                "TenLoai");

            return View(hocVien);
        }

        // POST: DangVien/DetailsHocVien/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DetailsHocVien(string id, [Bind("MaNguoi,LoaiNguoi,QuocTich,MaLoaiDangVien")] Nguoi nguoi)
        {
            try
            {
                // Kiểm tra quyền truy cập
                var userRole = HttpContext.Session.GetString("UserRole");
                if (string.IsNullOrEmpty(userRole))
                {
                    return RedirectToAction("Login", "Account");
                }

                Console.WriteLine("=== DangVien DetailsHocVien POST START ===");
                Console.WriteLine($"ID from route: {id}");
                Console.WriteLine($"ID from model: {nguoi?.MaNguoi}");
                
                if (id != nguoi?.MaNguoi) 
                {
                    Console.WriteLine("ID mismatch - returning NotFound");
                    return NotFound();
                }

                // Kiểm tra quyền truy cập vào học viên cụ thể
                var existing = await _context.Nguois.FindAsync(id);
                if (existing == null)
                {
                    Console.WriteLine("Existing record not found - returning NotFound");
                    return NotFound();
                }

                if (!CanAccessHocVien(existing, userRole))
                {
                    TempData["Error"] = "Bạn không có quyền cập nhật thông tin học viên này.";
                    return RedirectToAction("Index");
                }

                // Get form data directly
                var ngayVaoDangDuBiStr = Request.Form["NgayVaoDangDuBi"].ToString();
                var ngayVaoDangChinhThucStr = Request.Form["NgayVaoDangChinhThuc"].ToString();
                
                Console.WriteLine($"Raw form data:");
                Console.WriteLine($"  NgayVaoDangDuBi: '{ngayVaoDangDuBiStr}'");
                Console.WriteLine($"  NgayVaoDangChinhThuc: '{ngayVaoDangChinhThucStr}'");

                // Parse dates
                DateOnly? ngayVaoDangDuBi = null;
                DateOnly? ngayVaoDangChinhThuc = null;
                
                if (!string.IsNullOrEmpty(ngayVaoDangDuBiStr))
                {
                    if (DateTime.TryParseExact(ngayVaoDangDuBiStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime ngayVaoDangDuBiDate))
                    {
                        ngayVaoDangDuBi = DateOnly.FromDateTime(ngayVaoDangDuBiDate);
                        Console.WriteLine($"  NgayVaoDangDuBi parsed successfully: {ngayVaoDangDuBi}");
                    }
                    else
                    {
                        Console.WriteLine($"  NgayVaoDangDuBi parsing failed for: {ngayVaoDangDuBiStr}");
                    }
                }

                if (!string.IsNullOrEmpty(ngayVaoDangChinhThucStr))
                {
                    if (DateTime.TryParseExact(ngayVaoDangChinhThucStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime ngayVaoDangChinhThucDate))
                    {
                        ngayVaoDangChinhThuc = DateOnly.FromDateTime(ngayVaoDangChinhThucDate);
                        Console.WriteLine($"  NgayVaoDangChinhThuc parsed successfully: {ngayVaoDangChinhThuc}");
                    }
                    else
                    {
                        Console.WriteLine($"  NgayVaoDangChinhThuc parsing failed for: {ngayVaoDangChinhThucStr}");
                    }
                }

                // Log model data
                Console.WriteLine($"Model data:");
                Console.WriteLine($"  MaNguoi: {nguoi.MaNguoi}");
                Console.WriteLine($"  LoaiNguoi: {nguoi.LoaiNguoi}");
                Console.WriteLine($"  QuocTich: {nguoi.QuocTich}");
                Console.WriteLine($"  MaLoaiDangVien: {nguoi.MaLoaiDangVien}");

                // Check ModelState
                Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("ModelState errors:");
                    foreach (var key in ModelState.Keys)
                    {
                        var errors = ModelState[key].Errors;
                        if (errors.Any())
                        {
                            Console.WriteLine($"  {key}: {string.Join(", ", errors.Select(e => e.ErrorMessage))}");
                        }
                    }
                }

                Console.WriteLine($"Existing record found: {existing.HoTen}");

                // Update the record
                existing.MaLoaiDangVien = nguoi.MaLoaiDangVien;
                existing.NgayVaoDangDuBi = ngayVaoDangDuBi;
                existing.NgayVaoDangChinhThuc = ngayVaoDangChinhThuc;

                Console.WriteLine($"Updated values:");
                Console.WriteLine($"  MaLoaiDangVien: {existing.MaLoaiDangVien}");
                Console.WriteLine($"  NgayVaoDangDuBi: {existing.NgayVaoDangDuBi}");
                Console.WriteLine($"  NgayVaoDangChinhThuc: {existing.NgayVaoDangChinhThuc}");

                // Save changes
                _context.Update(existing);
                var saveResult = await _context.SaveChangesAsync();
                Console.WriteLine($"SaveChanges result: {saveResult} rows affected");
                
                TempData["Success"] = "Cập nhật thông tin đảng viên thành công!";
                
                // Always redirect back to DangVien Index
                Console.WriteLine("Redirecting to DangVien Index");
                return RedirectToAction("Index", "DangVien");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine($"DbUpdateConcurrencyException: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                if (!await _context.Nguois.AnyAsync(e => e.MaNguoi == id))
                {
                    Console.WriteLine("Record no longer exists - returning NotFound");
                    return NotFound();
                }
                else
                {
                    Console.WriteLine("Re-throwing DbUpdateConcurrencyException");
                    throw;
                }
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"DbUpdateException: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật cơ sở dữ liệu.";
                
                // Repopulate ViewData and return view
                ViewData["LoaiDangViens"] = new SelectList(
                    await _context.LoaiDangViens
                        .Where(l => l.TenLoai != "Quần chúng")
                        .ToListAsync(),
                    "MaLoaiDangVien",
                    "TenLoai");
                
                return View(nguoi);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Exception: {ex.Message}");
                Console.WriteLine($"Exception Type: {ex.GetType().Name}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật thông tin đảng viên.";
                
                // Repopulate ViewData and return view
                ViewData["LoaiDangViens"] = new SelectList(
                    await _context.LoaiDangViens
                        .Where(l => l.TenLoai != "Quần chúng")
                        .ToListAsync(),
                    "MaLoaiDangVien",
                    "TenLoai");
                
                return View(nguoi);
            }
        }

        // Helper method để kiểm tra quyền truy cập
        private bool CanAccessHocVien(Nguoi hocVien, string userRole)
        {
            if (userRole == "Admin")
            {
                return true; // Admin có thể truy cập tất cả
            }
            else if (userRole == "LopTruongLao")
            {
                // LopTruongLao chỉ có thể truy cập học viên Lào
                return hocVien.LoaiNguoi == "HocVien" && hocVien.QuocTich == "Lào";
            }
            else if (userRole == "LopTruongCam")
            {
                // LopTruongCam chỉ có thể truy cập học viên Campuchia
                return hocVien.LoaiNguoi == "HocVien" && hocVien.QuocTich == "Campuchia";
            }
            
            return false; // Các role khác không có quyền truy cập
        }
    }
}
