using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLHV.Models;

namespace QLHV.Controllers;

public class NhanVienController : Controller
{
    private readonly QlhvContext _context;

    public NhanVienController(QlhvContext context)
    {
        _context = context;
    }

    // GET: NhanVien
    public async Task<IActionResult> Index()
    {
        await PopulateDropDownListsAsync();
        return View();
    }

    // GET: NhanVien/GetData - For DataTables AJAX
    public async Task<IActionResult> GetData(
        string? hoTen = null,
        string? capBac = null,
        string? chucVu = null,
        string? truongHoc = null,
        string? quaTruong = null,
        int? maTrinhDo = null,
        int? maLoaiDangVien = null,
        bool? doanVien = null)
    {
        var query = _context.Nguois
            .Where(n => n.LoaiNguoi == "NhanVien");

        // Áp dụng các bộ lọc
        if (!string.IsNullOrEmpty(hoTen))
        {
            query = query.Where(n => n.HoTen.Contains(hoTen));
        }

        if (!string.IsNullOrEmpty(capBac))
        {
            query = query.Where(n => n.CapBac != null && n.CapBac.Contains(capBac));
        }

        if (!string.IsNullOrEmpty(chucVu))
        {
            query = query.Where(n => n.ChucVu != null && n.ChucVu.Contains(chucVu));
        }

        if (!string.IsNullOrEmpty(truongHoc))
        {
            query = query.Where(n => n.TruongHoc != null && n.TruongHoc.Contains(truongHoc));
        }

        if (!string.IsNullOrEmpty(quaTruong))
        {
            query = query.Where(n => n.QuaTruong != null && n.QuaTruong.ToString().Contains(quaTruong));
        }

        if (maTrinhDo.HasValue)
        {
            query = query.Where(n => n.MaTrinhDo == maTrinhDo.Value);
        }

        if (maLoaiDangVien.HasValue)
        {
            query = query.Where(n => n.MaLoaiDangVien == maLoaiDangVien.Value);
        }

        if (doanVien.HasValue)
        {
            query = query.Where(n => n.DoanVien == doanVien.Value);
        }

        var nhanViens = await query
            .Include(n => n.MaTrinhDoNavigation)
            .Include(n => n.MaLoaiDangVienNavigation)
            .Include(n => n.KhenThuongNguois)
            .Include(n => n.KyLuatNguois)
            .Select(n => new
            {
                n.MaNguoi,
                n.HoTen,
                n.NgaySinh,
                n.CapBac,
                n.ChucVu,
                LoaiDangVien = n.MaLoaiDangVienNavigation != null ? n.MaLoaiDangVienNavigation.TenLoai : "",
                TrinhDo = n.MaTrinhDoNavigation != null ? n.MaTrinhDoNavigation.TenTrinhDo : "",
                n.TruongHoc,
                n.QuaTruong,
                n.NamNhapNgu,
                n.NgayVaoDangChinhThuc,
                n.NgayVaoDangDuBi,
                n.DoanVien,
                KhenThuong = n.KhenThuongNguois.Count,
                KyLuat = n.KyLuatNguois.Count
            })
            .ToListAsync();

        return Json(new { data = nhanViens });
    }

    // GET: NhanVien/Details/5
    public async Task<IActionResult> Details(string id)
    {
        if (id == null) return NotFound();

        var nhanVien = await _context.Nguois
            .Include(n => n.MaTrinhDoNavigation)
            .Include(n => n.MaLoaiDangVienNavigation)
            .Include(n => n.KhenThuongNguois)
            .Include(n => n.KyLuatNguois)
            .FirstOrDefaultAsync(m => m.MaNguoi == id && m.LoaiNguoi == "NhanVien");

        if (nhanVien == null) return NotFound();
        return View(nhanVien);
    }

    // GET: NhanVien/Create
    public async Task<IActionResult> Create()
    {
        // Generate automatic MaNguoi
        var nextMaNguoi = await GenerateNextMaNguoiAsync("NV");
        ViewBag.NextMaNguoi = nextMaNguoi;
        
        await PopulateDropDownListsAsync();
        return View();
    }

    // POST: NhanVien/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Nguoi nhanVien)
    {
        // Generate automatic MaNguoi if not provided
        if (string.IsNullOrEmpty(nhanVien.MaNguoi))
        {
            nhanVien.MaNguoi = await GenerateNextMaNguoiAsync("NV");
        }
        
        // Log the received data
        Console.WriteLine($"Received data: MaNguoi={nhanVien.MaNguoi}, HoTen={nhanVien.HoTen}");
        
        // Set required fields
        nhanVien.LoaiNguoi = "NhanVien";
        nhanVien.QuocTich = "Việt Nam";
        
        // Xử lý trường DoanVien từ checkbox
        nhanVien.DoanVien = Request.Form.ContainsKey("DoanVien");

        // Clear ModelState errors for the fields we just set
        ModelState.Remove("LoaiNguoi");
        ModelState.Remove("QuocTich");

        if (ModelState.IsValid)
        {
            try
            {
                Console.WriteLine("ModelState is valid, attempting to save...");
                _context.Add(nhanVien);
                var result = await _context.SaveChangesAsync();
                Console.WriteLine($"SaveChanges result: {result} rows affected");
                TempData["Success"] = "Thêm nhân viên thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during save: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError("", $"Lỗi khi lưu: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("ModelState is invalid:");
            // Log validation errors
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    Console.WriteLine($"Validation error: {error.ErrorMessage}");
                }
            }
        }
        
        await PopulateDropDownListsAsync(nhanVien);
        return View(nhanVien);
    }

    // GET: NhanVien/Edit/5
    public async Task<IActionResult> Edit(string id)
    {
        if (id == null) return NotFound();

        var nhanVien = await _context.Nguois
            .FirstOrDefaultAsync(n => n.MaNguoi == id && n.LoaiNguoi == "NhanVien");
        if (nhanVien == null) return NotFound();

        await PopulateDropDownListsAsync(nhanVien);
        return View(nhanVien);
    }

    // POST: NhanVien/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, Nguoi nhanVien)
    {
        Console.WriteLine($"Edit action called with id: {id}, nhanVien.MaNguoi: {nhanVien.MaNguoi}");
        
        if (id != nhanVien.MaNguoi)
        {
            Console.WriteLine("ID mismatch - returning NotFound");
            return NotFound();
        }

        // Set required fields
        nhanVien.LoaiNguoi = "NhanVien";
        nhanVien.QuocTich = "Việt Nam";
        
        // Xử lý trường DoanVien từ checkbox
        nhanVien.DoanVien = Request.Form.ContainsKey("DoanVien");

        // Clear ModelState errors for the fields we just set
        ModelState.Remove("LoaiNguoi");
        ModelState.Remove("QuocTich");

        if (ModelState.IsValid)
        {
            try
            {
                Console.WriteLine("ModelState is valid, attempting to update...");
                _context.Update(nhanVien);
                var result = await _context.SaveChangesAsync();
                Console.WriteLine($"SaveChanges result: {result} rows affected");
                TempData["Success"] = "Cập nhật nhân viên thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                Console.WriteLine("DbUpdateConcurrencyException occurred");
                if (!NhanVienExists(nhanVien.MaNguoi))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during update: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError("", $"Lỗi khi cập nhật: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("ModelState is invalid:");
            // Log validation errors
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    Console.WriteLine($"Validation error: {error.ErrorMessage}");
                    ModelState.AddModelError("", error.ErrorMessage);
                }
            }
        }
        
        await PopulateDropDownListsAsync(nhanVien);
        return View(nhanVien);
    }

    // GET: NhanVien/Delete/5
    public async Task<IActionResult> Delete(string id)
    {
        if (id == null) return NotFound();

        var nhanVien = await _context.Nguois
            .Include(n => n.MaTrinhDoNavigation)
            .Include(n => n.MaLoaiDangVienNavigation)
            .FirstOrDefaultAsync(m => m.MaNguoi == id && m.LoaiNguoi == "NhanVien");

        if (nhanVien == null) return NotFound();
        return View(nhanVien);
    }

    // POST: NhanVien/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var nhanVien = await _context.Nguois
            .FirstOrDefaultAsync(n => n.MaNguoi == id && n.LoaiNguoi == "NhanVien");
        if (nhanVien != null)
        {
            _context.Nguois.Remove(nhanVien);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa nhân viên thành công!";
        }
        return RedirectToAction(nameof(Index));
    }

    private bool NhanVienExists(string id)
    {
        return _context.Nguois.Any(e => e.MaNguoi == id && e.LoaiNguoi == "NhanVien");
    }

    private async Task PopulateDropDownListsAsync(Nguoi? nhanVien = null)
    {
        ViewData["MaTrinhDo"] = new SelectList(await _context.TrinhDos.ToListAsync(), "MaTrinhDo", "TenTrinhDo", nhanVien?.MaTrinhDo);
        ViewData["MaLoaiDangVien"] = new SelectList(await _context.LoaiDangViens.ToListAsync(), "MaLoaiDangVien", "TenLoai", nhanVien?.MaLoaiDangVien);
    }

    private async Task<string> GenerateNextMaNguoiAsync(string prefix)
    {
        // Get the last MaNguoi with the given prefix
        var lastMaNguoi = await _context.Nguois
            .Where(n => n.MaNguoi.StartsWith(prefix))
            .OrderByDescending(n => n.MaNguoi)
            .Select(n => n.MaNguoi)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(lastMaNguoi))
        {
            // First entry
            return $"{prefix}001";
        }

        // Extract the number part and increment
        var numberPart = lastMaNguoi.Substring(prefix.Length);
        if (int.TryParse(numberPart, out int currentNumber))
        {
            return $"{prefix}{(currentNumber + 1):D3}";
        }

        // Fallback if parsing fails
        return $"{prefix}001";
    }
}

