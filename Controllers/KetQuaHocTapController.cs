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

        // Lấy danh sách năm học theo phân quyền
        var userRole = ViewBag.UserRole as string ?? string.Empty;
        var namHocQuery = _context.KetQuaHocTaps
            .Include(k => k.MaNguoiNavigation)
            .AsQueryable();

        if (userRole == "LopTruongLao")
        {
            namHocQuery = namHocQuery.Where(k => k.MaNguoiNavigation.LoaiNguoi == "HocVien" && k.MaNguoiNavigation.QuocTich == "Lào");
        }
        else if (userRole == "LopTruongCam")
        {
            namHocQuery = namHocQuery.Where(k => k.MaNguoiNavigation.LoaiNguoi == "HocVien" && k.MaNguoiNavigation.QuocTich == "Campuchia");
        }

        var namHocList = await namHocQuery
            .Where(k => k.NamHoc != null && k.NamHoc != "")
            .Select(k => k.NamHoc!)
            .Distinct()
            .OrderByDescending(n => n)
            .ToListAsync();
        ViewBag.NamHocList = namHocList;

        return View();
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
        var model = new KetQuaHocTap
        {
            NamHoc = GetDefaultNamHoc()
        };
        return View(model);
    }

    // POST: KetQuaHocTap/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(KetQuaHocTap ketQua)
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
                ketQua.DiemTrungBinh = parsed;
            }
        }

        // Kiểm tra trùng lặp MaNguoi + NamHoc
        if (!string.IsNullOrWhiteSpace(ketQua.MaNguoi) && !string.IsNullOrWhiteSpace(ketQua.NamHoc))
        {
            var exists = await _context.KetQuaHocTaps.AnyAsync(k => k.MaNguoi == ketQua.MaNguoi && k.NamHoc == ketQua.NamHoc);
            if (exists)
            {
                ModelState.AddModelError(nameof(KetQuaHocTap.NamHoc), "Đã tồn tại kết quả cho học viên này trong năm học đã chọn");
            }
        }

        // Kiểm tra quyền theo học viên chọn
        Nguoi? nguoi = null;
        if (!string.IsNullOrWhiteSpace(ketQua.MaNguoi))
        {
            nguoi = await _context.Nguois.FirstOrDefaultAsync(n => n.MaNguoi == ketQua.MaNguoi);
            if (!CanAccessNguoi(nguoi, userRole))
            {
                ModelState.AddModelError(nameof(KetQuaHocTap.MaNguoi), "Bạn không có quyền chọn học viên này");
            }
        }

        if (!ModelState.IsValid)
        {
            await PopulateNguoiSelectListForRole(userRole, ketQua.MaNguoi);
            return View(ketQua);
        }

        _context.KetQuaHocTaps.Add(ketQua);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Thêm kết quả học tập thành công.";
        return RedirectToAction(nameof(Index));
    }

    // GET: KetQuaHocTap/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

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

        if (string.IsNullOrWhiteSpace(ketQua.NamHoc))
        {
            ketQua.NamHoc = GetDefaultNamHoc();
        }
        await PopulateNguoiSelectListForRole(userRole, ketQua.MaNguoi);
        return View(ketQua);
    }

    // POST: KetQuaHocTap/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Edit")]
    public async Task<IActionResult> EditPost(int id, KetQuaHocTap ketQua)
    {
        if (id != ketQua.MaKetQua) return NotFound();

        var existingKetQua = await _context.KetQuaHocTaps
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
                ketQua.DiemTrungBinh = parsed;
            }
        }

        // Kiểm tra trùng lặp MaNguoi + NamHoc (trừ bản ghi hiện tại)
        if (!string.IsNullOrWhiteSpace(ketQua.MaNguoi) && !string.IsNullOrWhiteSpace(ketQua.NamHoc))
        {
            var exists = await _context.KetQuaHocTaps
                .AnyAsync(k => k.MaNguoi == ketQua.MaNguoi && k.NamHoc == ketQua.NamHoc && k.MaKetQua != id);
            if (exists)
            {
                ModelState.AddModelError(nameof(KetQuaHocTap.NamHoc), "Đã tồn tại kết quả cho học viên này trong năm học đã chọn");
            }
        }

        // Kiểm tra quyền theo học viên chọn (nếu đổi học viên)
        Nguoi? nguoi = null;
        if (!string.IsNullOrWhiteSpace(ketQua.MaNguoi))
        {
            nguoi = await _context.Nguois.FirstOrDefaultAsync(n => n.MaNguoi == ketQua.MaNguoi);
            if (!CanAccessNguoi(nguoi, userRole))
            {
                ModelState.AddModelError(nameof(KetQuaHocTap.MaNguoi), "Bạn không có quyền chọn học viên này");
            }
        }

        if (!ModelState.IsValid)
        {
            await PopulateNguoiSelectListForRole(userRole, ketQua.MaNguoi);
            return View(ketQua);
        }

        existingKetQua.MaNguoi = ketQua.MaNguoi;
        existingKetQua.NamHoc = ketQua.NamHoc;
        existingKetQua.DiemTrungBinh = ketQua.DiemTrungBinh;
        existingKetQua.GhiChu = ketQua.GhiChu;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Cập nhật kết quả học tập thành công.";
        return RedirectToAction(nameof(Index));
    }

    // GET: KetQuaHocTap/GetData
    [HttpGet]
    public async Task<IActionResult> GetData(string? quocTich = null, string? namHoc = null)
    {
        try
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            List<object> ketQuas;

            // Xây dựng query cơ bản theo phân quyền
            var query = _context.KetQuaHocTaps
                .Include(k => k.MaNguoiNavigation)
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

            if (!string.IsNullOrWhiteSpace(namHoc))
            {
                query = query.Where(k => k.NamHoc == namHoc);
            }

            var data = await query
                .Select(k => new
                {
                    k.MaKetQua,
                    k.MaNguoi,
                    HoTen = k.MaNguoiNavigation != null ? k.MaNguoiNavigation.HoTen : string.Empty,
                    k.NamHoc,
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
            .FirstOrDefaultAsync(m => m.MaKetQua == id);

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
        if (ketQua == null) return RedirectToAction(nameof(Index));

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

    private bool KetQuaExists(int id)
    {
        return _context.KetQuaHocTaps.Any(e => e.MaKetQua == id);
    }

    // Helper: kiểm tra quyền truy cập theo role
    private bool CanAccessNguoi(Nguoi? nguoi, string userRole)
    {
        if (nguoi == null) return false;
        if (userRole == "Admin") return true;
        if (userRole == "LopTruongLao")
            return nguoi.LoaiNguoi == "HocVien" && nguoi.QuocTich == "Lào";
        if (userRole == "LopTruongCam")
            return nguoi.LoaiNguoi == "HocVien" && nguoi.QuocTich == "Campuchia";
        return false;
    }

    private async Task PopulateNguoiSelectListForRole(string userRole, string? selectedMaNguoi = null)
    {
        List<Nguoi> nguoiList;
        if (userRole == "Admin")
        {
            nguoiList = await _context.Nguois
                .Where(n => n.LoaiNguoi == "HocVien")
                .ToListAsync();
        }
        else if (userRole == "LopTruongLao")
        {
            nguoiList = await _context.Nguois
                .Where(n => n.LoaiNguoi == "HocVien" && n.QuocTich == "Lào")
                .ToListAsync();
        }
        else if (userRole == "LopTruongCam")
        {
            nguoiList = await _context.Nguois
                .Where(n => n.LoaiNguoi == "HocVien" && n.QuocTich == "Campuchia")
                .ToListAsync();
        }
        else
        {
            nguoiList = new List<Nguoi>();
        }
        
        ViewBag.MaNguoi = new SelectList(nguoiList, "MaNguoi", "HoTen", selectedMaNguoi);
        
        // Tránh circular reference khi serialize JSON
        var quocTichMap = nguoiList.ToDictionary(n => n.MaNguoi, n => n.QuocTich ?? string.Empty);
        ViewBag.NguoiQuocTichMap = JsonSerializer.Serialize(quocTichMap, new JsonSerializerOptions 
        { 
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles 
        });
        
        // Đảm bảo NamHocOptions được set đúng
        var namHocOptions = BuildNamHocOptions().ToList();
        ViewBag.NamHocOptions = namHocOptions;
        
        // Debug log
        System.Diagnostics.Debug.WriteLine($"PopulateNguoiSelectListForRole: userRole={userRole}, nguoiCount={nguoiList.Count}, namHocCount={namHocOptions.Count}");
    }

    // Helper: parse decimal hỗ trợ cả dấu chấm và dấu phẩy
    private bool TryParseDecimalFlexible(string? input, out decimal value)
    {
        value = 0m;
        if (string.IsNullOrWhiteSpace(input)) return false;

        // Thử theo InvariantCulture (dấu chấm)
        if (decimal.TryParse(input, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out value))
            return true;

        // Thử thay dấu phẩy -> chấm
        var replacedDot = input.Replace(',', '.');
        if (decimal.TryParse(replacedDot, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out value))
            return true;

        // Thử theo văn hóa vi-VN (dấu phẩy)
        var vi = new System.Globalization.CultureInfo("vi-VN");
        if (decimal.TryParse(input, System.Globalization.NumberStyles.Number, vi, out value))
            return true;

        return false;
    }

    private IEnumerable<string> BuildNamHocOptions(int yearsBack = 15, int yearsForward = 1)
    {
        var currentYear = DateTime.Now.Year;
        var start = currentYear - yearsBack;
        var end = currentYear + yearsForward;
        for (var y = end; y >= start; y--)
        {
            var prev = y - 1;
            yield return $"{prev}-{y}";
        }
    }

    private string GetDefaultNamHoc()
    {
        var y = DateTime.Now.Year;
        return $"{y - 1}-{y}";
    }

    // Remote validation: đảm bảo MaNguoi + NamHoc là duy nhất (trừ khi đang sửa bản ghi hiện tại)
    [AcceptVerbs("Get", "Post")]
    public async Task<IActionResult> ValidateUnique(string? namHoc, string? maNguoi, int? maKetQua)
    {
        if (string.IsNullOrWhiteSpace(maNguoi) || string.IsNullOrWhiteSpace(namHoc))
        {
            return Json(true);
        }
        var exists = await _context.KetQuaHocTaps
            .AnyAsync(k => k.MaNguoi == maNguoi && k.NamHoc == namHoc && (!maKetQua.HasValue || k.MaKetQua != maKetQua.Value));
        return Json(!exists);
    }

    [HttpGet]
    public async Task<IActionResult> CheckDuplicate(string maNguoi, string namHoc, int? id)
    {
        if (string.IsNullOrWhiteSpace(maNguoi) || string.IsNullOrWhiteSpace(namHoc))
        {
            return Json(new { exists = false });
        }
        var exists = await _context.KetQuaHocTaps
            .AnyAsync(k => k.MaNguoi == maNguoi && k.NamHoc == namHoc && (!id.HasValue || k.MaKetQua != id.Value));
        return Json(new { exists });
    }
}

