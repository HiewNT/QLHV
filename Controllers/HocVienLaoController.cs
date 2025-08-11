using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLHV.Models;

namespace QLHV.Controllers;

public class HocVienLaoController : Controller
{
    private readonly QlhvContext _context;

    public HocVienLaoController(QlhvContext context)
    {
        _context = context;
    }

    // GET: HocVienLao
    public async Task<IActionResult> Index()
    {
        await PopulateDropDownListsAsync();
        return View();
    }

    // GET: HocVienLao/GetData - For DataTables AJAX
    public async Task<IActionResult> GetData()
    {
        var hocViens = await _context.Nguois
            .Where(h => h.LoaiNguoi == "HocVien" && h.QuocTich == "Lào")
            .Include(h => h.MaTrinhDoNavigation)
            .Include(h => h.MaLoaiDangVienNavigation)
            .Include(h => h.KetQuaHocTaps)
            .Include(h => h.KhenThuongNguois)
            .Include(h => h.KyLuatNguois)
            .Select(h => new
            {
                h.MaNguoi,
                h.HoTen,
                h.NgaySinh,
                h.CapBac,
                TrinhDo = h.MaTrinhDoNavigation != null ? h.MaTrinhDoNavigation.TenTrinhDo : "",
                h.ChuyenNganh,
                h.KhoaHoc,
                h.NgayNhapHoc,
                h.NgayTotNghiep,
                LoaiDangVien = h.MaLoaiDangVienNavigation != null ? h.MaLoaiDangVienNavigation.TenLoai : "",
                h.NgayVaoDangChinhThuc,
                h.NgayVaoDangDuBi,
                KetQuaHocTap = h.KetQuaHocTaps.Count,
                KhenThuong = h.KhenThuongNguois.Count,
                KyLuat = h.KyLuatNguois.Count
            })
            .ToListAsync();

        return Json(new { data = hocViens });
    }

    // GET: HocVienLao/Details/5
    public async Task<IActionResult> Details(string id)
    {
        if (id == null) return NotFound();

        var hocVien = await _context.Nguois
            .Include(h => h.MaTrinhDoNavigation)
            .Include(h => h.MaLoaiDangVienNavigation)
            .Include(h => h.KetQuaHocTaps)
            .Include(h => h.KhenThuongNguois)
            .Include(h => h.KyLuatNguois)
            .FirstOrDefaultAsync(m => m.MaNguoi == id && m.LoaiNguoi == "HocVien" && m.QuocTich == "Lào");

        if (hocVien == null) return NotFound();
        return View(hocVien);
    }

    // GET: HocVienLao/Create
    public async Task<IActionResult> Create()
    {
        // Generate automatic MaNguoi
        var nextMaNguoi = await GenerateNextMaNguoiAsync("HV");
        ViewBag.NextMaNguoi = nextMaNguoi;
        
        await PopulateDropDownListsAsync();
        return View();
    }

    // POST: HocVienLao/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Nguoi hocVien)
    {
        // Generate automatic MaNguoi if not provided
        if (string.IsNullOrEmpty(hocVien.MaNguoi))
        {
            hocVien.MaNguoi = await GenerateNextMaNguoiAsync("HV");
        }
        
        // Set required fields
        hocVien.LoaiNguoi = "HocVien";
        hocVien.QuocTich = "Lào";

        // Clear ModelState errors for the fields we just set
        ModelState.Remove("LoaiNguoi");
        ModelState.Remove("QuocTich");
        
        if (ModelState.IsValid)
        {
            _context.Add(hocVien);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Thêm học viên Lào thành công!";
            return RedirectToAction(nameof(Index));
        }
        await PopulateDropDownListsAsync(hocVien);
        return View(hocVien);
    }

    // GET: HocVienLao/Edit/5
    public async Task<IActionResult> Edit(string id)
    {
        if (id == null) return NotFound();

        var hocVien = await _context.Nguois
            .FirstOrDefaultAsync(h => h.MaNguoi == id && h.LoaiNguoi == "HocVien" && h.QuocTich == "Lào");
        if (hocVien == null) return NotFound();

        await PopulateDropDownListsAsync(hocVien);
        return View(hocVien);
    }

    // POST: HocVienLao/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, Nguoi hocVien)
    {
        if (id != hocVien.MaNguoi) return NotFound();

        // Set required fields
        hocVien.LoaiNguoi = "HocVien";
        hocVien.QuocTich = "Lào";

        // Clear ModelState errors for the fields we just set
        ModelState.Remove("LoaiNguoi");
        ModelState.Remove("QuocTich");

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(hocVien);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật học viên Lào thành công!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HocVienExists(hocVien.MaNguoi))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }
        await PopulateDropDownListsAsync(hocVien);
        return View(hocVien);
    }

    // GET: HocVienLao/Delete/5
    public async Task<IActionResult> Delete(string id)
    {
        if (id == null) return NotFound();

        var hocVien = await _context.Nguois
            .Include(h => h.MaTrinhDoNavigation)
            .Include(h => h.MaLoaiDangVienNavigation)
            .FirstOrDefaultAsync(m => m.MaNguoi == id && m.LoaiNguoi == "HocVien" && m.QuocTich == "Lào");

        if (hocVien == null) return NotFound();
        return View(hocVien);
    }

    // POST: HocVienLao/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var hocVien = await _context.Nguois
            .FirstOrDefaultAsync(h => h.MaNguoi == id && h.LoaiNguoi == "HocVien" && h.QuocTich == "Lào");
        if (hocVien != null)
        {
            _context.Nguois.Remove(hocVien);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa học viên Lào thành công!";
        }
        return RedirectToAction(nameof(Index));
    }

    private bool HocVienExists(string id)
    {
        return _context.Nguois.Any(e => e.MaNguoi == id && e.LoaiNguoi == "HocVien" && e.QuocTich == "Lào");
    }

    private async Task PopulateDropDownListsAsync(Nguoi? hocVien = null)
    {
        ViewData["MaTrinhDo"] = new SelectList(await _context.TrinhDos.ToListAsync(), "MaTrinhDo", "TenTrinhDo", hocVien?.MaTrinhDo);
        ViewData["MaLoaiDangVien"] = new SelectList(await _context.LoaiDangViens.ToListAsync(), "MaLoaiDangVien", "TenLoai", hocVien?.MaLoaiDangVien);
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
