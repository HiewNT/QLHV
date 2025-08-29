using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLHV.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QLHV.Controllers;

public class KetQuaHocTapController : Controller
{
    private readonly QlhvContext _context;
    public KetQuaHocTapController(QlhvContext context) { _context = context; }

    // GET: KetQuaHocTap
    public async Task<IActionResult> Index()
    {
        // Lưu role để View có thể hiển thị phù hợp nếu cần
        ViewBag.UserRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;

        // Lấy danh sách học kỳ theo phân quyền
        var userRole = ViewBag.UserRole as string ?? string.Empty;
        var hocKyQuery = _context.KetQuaHocTaps
            .Include(k => k.MaNguoiNavigation)
            .Include(k => k.MaHocKyNavigation)
            .AsQueryable();

        if (userRole == "LopTruongLao")
        {
            hocKyQuery = hocKyQuery.Where(k => k.MaNguoiNavigation.LoaiNguoi == "HocVien" && k.MaNguoiNavigation.QuocTich == "Lào");
        }
        else if (userRole == "LopTruongCam")
        {
            hocKyQuery = hocKyQuery.Where(k => k.MaNguoiNavigation.LoaiNguoi == "HocVien" && k.MaNguoiNavigation.QuocTich == "Campuchia");
        }

        var hocKyList = await hocKyQuery
            .Where(k => k.MaHocKyNavigation != null)
            .Select(k => new { k.MaHocKyNavigation.MaHocKy, k.MaHocKyNavigation.TenHocKy, k.MaHocKyNavigation.NamHoc })
            .Distinct()
            .OrderByDescending(h => h.NamHoc)
            .ThenBy(h => h.MaHocKy)
            .ToListAsync();
        ViewBag.HocKyList = hocKyList;

        // Lấy danh sách kết quả học tập để truyền vào view
        var ketQuaList = await hocKyQuery.ToListAsync();
        return View(ketQuaList);
    }

    // GET: KetQuaHocTap/Create
    public async Task<IActionResult> Create()
    {
        var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
        if (userRole != "Admin" && userRole != "LopTruongLao" && userRole != "LopTruongCam")
        {
            TempData["Error"] = "Bạn không có quyền thêm kết quả học tập.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateNguoiSelectListForRole(userRole);
        await PopulateHocKySelectList(null, null);
        var model = new KetQuaHocTapViewModel();
        return View(model);
    }

    // POST: KetQuaHocTap/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(KetQuaHocTapViewModel model)
    {
        var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
        if (userRole != "Admin" && userRole != "LopTruongLao" && userRole != "LopTruongCam")
        {
            TempData["Error"] = "Bạn không có quyền thêm kết quả học tập.";
            return RedirectToAction(nameof(Index));
        }

        // Xử lý parse điểm linh hoạt nếu binder lỗi
        if (ModelState.ContainsKey(nameof(KetQuaHocTap.DiemTrungBinh)) && ModelState[nameof(KetQuaHocTap.DiemTrungBinh)]!.Errors.Count > 0)
        {
            var raw = Request.Form[nameof(KetQuaHocTap.DiemTrungBinh)].FirstOrDefault();
            if (TryParseDecimalFlexible(raw, out var parsed))
            {
                ModelState[nameof(KetQuaHocTap.DiemTrungBinh)]!.Errors.Clear();
                model.DiemTrungBinh = parsed;
            }
        }

        // Kiểm tra trùng lặp MaNguoi + MaHocKy
        if (!string.IsNullOrWhiteSpace(model.MaNguoi) && model.MaHocKy > 0)
        {
            var exists = await _context.KetQuaHocTaps.AnyAsync(k => k.MaNguoi == model.MaNguoi && k.MaHocKy == model.MaHocKy);
            if (exists)
            {
                ModelState.AddModelError(nameof(KetQuaHocTap.MaHocKy), "Đã tồn tại kết quả cho học viên này trong học kỳ đã chọn");
            }
        }

        // Kiểm tra quyền theo học viên chọn
        Nguoi? nguoi = null;
        if (!string.IsNullOrWhiteSpace(model.MaNguoi))
        {
            nguoi = await _context.Nguois
                .Where(n => n.MaNguoi == model.MaNguoi)
                .Distinct()
                .FirstOrDefaultAsync();
            if (!CanAccessNguoi(nguoi, userRole))
            {
                ModelState.AddModelError(nameof(KetQuaHocTap.MaNguoi), "Bạn không có quyền chọn học viên này");
            }
        }

        if (!ModelState.IsValid)
        {
            await PopulateNguoiSelectListForRole(userRole, model.MaNguoi);
            await PopulateHocKySelectList(model.MaHocKy, model.MaNguoi);
            return View(model);
        }

        try
        {
            // Tạo entity mới và chỉ set các properties cần thiết
            var entity = new KetQuaHocTap();
            entity.MaNguoi = model.MaNguoi;
            entity.MaHocKy = model.MaHocKy;
            entity.DiemTrungBinh = model.DiemTrungBinh;
            entity.GhiChu = model.GhiChu;

            _context.KetQuaHocTaps.Add(entity);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Lỗi khi lưu dữ liệu: " + ex.Message);
            await PopulateNguoiSelectListForRole(userRole, model.MaNguoi);
            await PopulateHocKySelectList(model.MaHocKy, model.MaNguoi);
            return View(model);
        }
        TempData["Success"] = "Thêm kết quả học tập thành công.";
        return RedirectToAction(nameof(Index));
    }

    // GET: KetQuaHocTap/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var ketQua = await _context.KetQuaHocTaps
            .Include(k => k.MaNguoiNavigation)
            .Include(k => k.MaHocKyNavigation)
            .FirstOrDefaultAsync(k => k.MaKetQua == id);
        if (ketQua == null) return NotFound();

        var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
        if (!CanAccessNguoi(ketQua.MaNguoiNavigation, userRole))
        {
            TempData["Error"] = "Bạn không có quyền sửa kết quả học tập này.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateNguoiSelectListForRole(userRole, ketQua.MaNguoi);
        await PopulateHocKySelectList(ketQua.MaHocKy);
        return View(ketQua);
    }

    // POST: KetQuaHocTap/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Edit")]
    public async Task<IActionResult> EditPost(int id, KetQuaHocTap model)
    {
        if (id != model.MaKetQua) return NotFound();

        var ketQua = await _context.KetQuaHocTaps
            .Include(k => k.MaNguoiNavigation)
            .FirstOrDefaultAsync(k => k.MaKetQua == id);
        if (ketQua == null) return NotFound();

        var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
        if (!CanAccessNguoi(ketQua.MaNguoiNavigation, userRole))
        {
            TempData["Error"] = "Bạn không có quyền sửa kết quả học tập này.";
            return RedirectToAction(nameof(Index));
        }

        // Xử lý parse điểm linh hoạt nếu binder lỗi
        if (ModelState.ContainsKey(nameof(KetQuaHocTap.DiemTrungBinh)) && ModelState[nameof(KetQuaHocTap.DiemTrungBinh)]!.Errors.Count > 0)
        {
            var raw = Request.Form[nameof(KetQuaHocTap.DiemTrungBinh)].FirstOrDefault();
            if (TryParseDecimalFlexible(raw, out var parsed))
            {
                ModelState[nameof(KetQuaHocTap.DiemTrungBinh)]!.Errors.Clear();
                model.DiemTrungBinh = parsed;
            }
        }

        // Kiểm tra trùng lặp MaNguoi + MaHocKy (trừ bản ghi hiện tại)
        if (!string.IsNullOrWhiteSpace(model.MaNguoi) && model.MaHocKy > 0)
        {
            var exists = await _context.KetQuaHocTaps
                .AnyAsync(k => k.MaNguoi == model.MaNguoi && k.MaHocKy == model.MaHocKy && k.MaKetQua != id);
            if (exists)
            {
                ModelState.AddModelError(nameof(KetQuaHocTap.MaHocKy), "Đã tồn tại kết quả cho học viên này trong học kỳ đã chọn");
            }
        }

        // Kiểm tra quyền theo học viên chọn (nếu đổi học viên)
        Nguoi? nguoi = null;
        if (!string.IsNullOrWhiteSpace(model.MaNguoi))
        {
            nguoi = await _context.Nguois
                .Where(n => n.MaNguoi == model.MaNguoi)
                .Distinct()
                .FirstOrDefaultAsync();
            if (!CanAccessNguoi(nguoi, userRole))
            {
                ModelState.AddModelError(nameof(KetQuaHocTap.MaNguoi), "Bạn không có quyền chọn học viên này");
            }
        }

        if (!ModelState.IsValid)
        {
            await PopulateNguoiSelectListForRole(userRole, model.MaNguoi);
            await PopulateHocKySelectList(model.MaHocKy, model.MaNguoi);
            return View(model);
        }

        try
        {
            ketQua.MaNguoi = model.MaNguoi;
            ketQua.MaHocKy = model.MaHocKy;
            ketQua.DiemTrungBinh = model.DiemTrungBinh;
            ketQua.GhiChu = model.GhiChu;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Lỗi khi cập nhật dữ liệu: " + ex.Message);
            await PopulateNguoiSelectListForRole(userRole, model.MaNguoi);
            await PopulateHocKySelectList(model.MaHocKy, model.MaNguoi);
            return View(model);
        }
        TempData["Success"] = "Cập nhật kết quả học tập thành công.";
        return RedirectToAction(nameof(Index));
    }

    // GET: KetQuaHocTap/GetData
    [HttpGet]
    public async Task<IActionResult> GetData(string? quocTich = null, int? maHocKy = null)
    {
        try
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            List<object> ketQuas;

            // Xây dựng query cơ bản theo phân quyền
            var query = _context.KetQuaHocTaps
                .Include(k => k.MaNguoiNavigation)
                .Include(k => k.MaHocKyNavigation)
                .AsNoTracking()
                .AsQueryable();

            if (userRole == "LopTruongLao")
            {
                query = query.Where(k => k.MaNguoiNavigation.LoaiNguoi == "HocVien" && k.MaNguoiNavigation.QuocTich == "Lào");
            }
            else if (userRole == "LopTruongCam")
            {
                query = query.Where(k => k.MaNguoiNavigation.LoaiNguoi == "HocVien" && k.MaNguoiNavigation.QuocTich == "Campuchia");
            }
            else if (userRole != "Admin")
            {
                // Role khác không thấy gì
                return Json(new { data = new List<object>() });
            }

            // Áp dụng bộ lọc tự chọn
            if (!string.IsNullOrWhiteSpace(quocTich))
            {
                query = query.Where(k => k.MaNguoiNavigation.QuocTich == quocTich);
            }

            if (maHocKy.HasValue)
            {
                query = query.Where(k => k.MaHocKy == maHocKy.Value);
            }

            var data = await query
                .Select(k => new
                {
                    k.MaKetQua,
                    k.MaNguoi,
                    HoTen = k.MaNguoiNavigation != null ? k.MaNguoiNavigation.HoTen : string.Empty,
                    HocKy = k.MaHocKyNavigation != null ? k.MaHocKyNavigation.TenHocKy : string.Empty,
                    NamHoc = k.MaHocKyNavigation != null ? k.MaHocKyNavigation.NamHoc : string.Empty,
                    k.DiemTrungBinh,
                    k.GhiChu
                })
                .ToListAsync();
            ketQuas = data.Cast<object>().ToList();

            return Json(new { data = ketQuas });
        }
        catch (Exception)
        {
            // Trả về mảng rỗng nếu có lỗi để DataTables không báo lỗi Ajax
            return Json(new { data = new List<object>() });
        }
    }

    // GET: KetQuaHocTap/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var ketQua = await _context.KetQuaHocTaps
            .Include(k => k.MaNguoiNavigation)
            .Include(k => k.MaHocKyNavigation)
            .FirstOrDefaultAsync(k => k.MaKetQua == id);

        if (ketQua == null) return NotFound();

        var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
        if (!CanAccessNguoi(ketQua.MaNguoiNavigation, userRole))
        {
            TempData["Error"] = "Bạn không có quyền xem kết quả học tập này.";
            return RedirectToAction(nameof(Index));
        }

        return View(ketQua);
    }

    // GET: KetQuaHocTap/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var ketQua = await _context.KetQuaHocTaps
            .Include(k => k.MaNguoiNavigation)
            .Include(k => k.MaHocKyNavigation)
            .FirstOrDefaultAsync(k => k.MaKetQua == id);

        if (ketQua == null) return NotFound();

        var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
        if (!CanAccessNguoi(ketQua.MaNguoiNavigation, userRole))
        {
            TempData["Error"] = "Bạn không có quyền xóa kết quả học tập này.";
            return RedirectToAction(nameof(Index));
        }

        return View(ketQua);
    }

    // POST: KetQuaHocTap/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var ketQua = await _context.KetQuaHocTaps
            .Include(k => k.MaNguoiNavigation)
            .FirstOrDefaultAsync(k => k.MaKetQua == id);

        if (ketQua == null) return NotFound();

        var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
        if (!CanAccessNguoi(ketQua.MaNguoiNavigation, userRole))
        {
            TempData["Error"] = "Bạn không có quyền xóa kết quả học tập này.";
            return RedirectToAction(nameof(Index));
        }

        _context.KetQuaHocTaps.Remove(ketQua);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Xóa kết quả học tập thành công.";
        return RedirectToAction(nameof(Index));
    }

    private bool KetQuaHocTapExists(int id)
    {
        return _context.KetQuaHocTaps.Any(e => e.MaKetQua == id);
    }

    private async Task PopulateNguoiSelectListForRole(string userRole, string? selectedValue = null)
    {
        var query = _context.Nguois.AsQueryable();

        // Chỉ lấy HocVien, không lấy NhanVien
        query = query.Where(n => n.LoaiNguoi == "HocVien");

        // Áp dụng phân quyền
        if (userRole == "LopTruongLao")
        {
            query = query.Where(n => n.QuocTich == "Lào");
        }
        else if (userRole == "LopTruongCam")
        {
            query = query.Where(n => n.QuocTich == "Campuchia");
        }
        else if (userRole != "Admin")
        {
            // Role khác không thấy ai
            ViewData["MaNguoi"] = new SelectList(new List<SelectListItem>(), "Value", "Text", selectedValue);
            return;
        }

        var nguoiList = await query
            .OrderBy(n => n.HoTen)
            .Select(n => new SelectListItem
            {
                Value = n.MaNguoi,
                Text = $"{n.MaNguoi} - {n.HoTen}"
            })
            .ToListAsync();

        ViewData["MaNguoi"] = new SelectList(nguoiList, "Value", "Text", selectedValue);
    }

    private async Task PopulateHocKySelectList(int? selectedValue = null, string? maNguoi = null)
    {
        var query = _context.HocKies.AsQueryable();

        // Nếu có chọn học viên, loại bỏ những học kỳ đã có kết quả
        if (!string.IsNullOrWhiteSpace(maNguoi))
        {
            var existingHocKyIds = await _context.KetQuaHocTaps
                .Where(k => k.MaNguoi == maNguoi)
                .Select(k => k.MaHocKy)
                .ToListAsync();

            query = query.Where(h => !existingHocKyIds.Contains(h.MaHocKy));
        }

        var hocKyList = await query
            .OrderByDescending(h => h.NamHoc)
            .ThenBy(h => h.MaHocKy)
            .Select(h => new SelectListItem
            {
                Value = h.MaHocKy.ToString(),
                Text = $"{h.TenHocKy} - {h.NamHoc}"
            })
            .ToListAsync();

        ViewData["MaHocKy"] = new SelectList(hocKyList, "Value", "Text", selectedValue);
    }

    private bool CanAccessNguoi(Nguoi? nguoi, string userRole)
    {
        if (nguoi == null) return false;

        if (userRole == "Admin") return true;

        if (userRole == "LopTruongLao")
        {
            return nguoi.LoaiNguoi == "HocVien" && nguoi.QuocTich == "Lào";
        }

        if (userRole == "LopTruongCam")
        {
            return nguoi.LoaiNguoi == "HocVien" && nguoi.QuocTich == "Campuchia";
        }

        return false;
    }

    private bool TryParseDecimalFlexible(string? input, out decimal result)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(input)) return false;

        // Thử parse trực tiếp
        if (decimal.TryParse(input, out result)) return true;

        // Thử thay thế dấu phẩy bằng dấu chấm
        var normalized = input.Replace(',', '.');
        if (decimal.TryParse(normalized, out result)) return true;

        // Thử thay thế dấu chấm bằng dấu phẩy
        normalized = input.Replace('.', ',');
        return decimal.TryParse(normalized, out result);
    }

    // GET: KetQuaHocTap/GetAvailableHocKy - API để lấy danh sách học kỳ chưa có kết quả
    [HttpGet]
    public async Task<IActionResult> GetAvailableHocKy(string maNguoi)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(maNguoi))
            {
                return Json(new List<object>());
            }

            // Lấy danh sách học kỳ đã có kết quả của học viên này
            var existingHocKyIds = await _context.KetQuaHocTaps
                .Where(k => k.MaNguoi == maNguoi)
                .Select(k => k.MaHocKy)
                .ToListAsync();

            // Lấy danh sách học kỳ chưa có kết quả
            var availableHocKy = await _context.HocKies
                .Where(h => !existingHocKyIds.Contains(h.MaHocKy))
                .OrderByDescending(h => h.NamHoc)
                .ThenBy(h => h.MaHocKy)
                .Select(h => new
                {
                    value = h.MaHocKy.ToString(),
                    text = $"{h.TenHocKy} - {h.NamHoc}"
                })
                .ToListAsync();

            return Json(availableHocKy);
        }
        catch (Exception)
        {
            return Json(new List<object>());
        }
    }
}

