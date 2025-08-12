using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLHV.Models;

namespace QLHV.Controllers;

public class KhenThuongController : Controller
{
    private readonly QlhvContext _context;

    public KhenThuongController(QlhvContext context)
    {
        _context = context;
    }

    // GET: KhenThuong
    public async Task<IActionResult> Index()
    {
        return View();
    }

    // GET: KhenThuong/GetData - For DataTables AJAX
    public async Task<IActionResult> GetData()
    {
        var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;

        var khenThuongs = await _context.KhenThuongs
            .Include(k => k.KhenThuongNguois)
            .ThenInclude(ktn => ktn.MaNguoiNavigation)
            .ToListAsync();

        if (userRole == "LopTruongLao")
        {
            foreach (var kt in khenThuongs)
            {
                kt.KhenThuongNguois = kt.KhenThuongNguois
                    .Where(ktn => ktn.MaNguoiNavigation?.LoaiNguoi == "HocVien" && ktn.MaNguoiNavigation?.QuocTich.Contains("Lào") == true)
                    .ToList();
            }
        }
        else if (userRole == "LopTruongCam")
        {
            foreach (var kt in khenThuongs)
            {
                kt.KhenThuongNguois = kt.KhenThuongNguois
                    .Where(ktn => ktn.MaNguoiNavigation?.LoaiNguoi == "HocVien" && ktn.MaNguoiNavigation?.QuocTich.Contains("Campuchia") == true)
                    .ToList();
            }
        }

        var result = khenThuongs.Select(k => new
        {
            k.MaKhenThuong,
            k.TenKhenThuong,
            k.CapKhenThuong,
            SoNguoiDuocKhen = k.KhenThuongNguois.Count
        }).ToList();

        return Json(new { data = result });
    }

    // GET: KhenThuong/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var khenThuong = await _context.KhenThuongs
            .Include(k => k.KhenThuongNguois)
            .ThenInclude(kt => kt.MaNguoiNavigation)
            .FirstOrDefaultAsync(m => m.MaKhenThuong == id);

        if (khenThuong == null) return NotFound();

        // Thêm danh sách người để hiển thị trong dropdown
        ViewBag.NguoiList = await _context.Nguois.ToListAsync();

        return View(khenThuong);
    }

    // GET: KhenThuong/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: KhenThuong/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("TenKhenThuong,CapKhenThuong")] KhenThuong khenThuong)
    {
        if (ModelState.IsValid)
        {
            _context.Add(khenThuong);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(khenThuong);
    }

    // POST: KhenThuong/CreateWithPerson
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateWithPerson([Bind("TenKhenThuong,CapKhenThuong,MaNguoi,LyDo,NgayKhenThuong")] KhenThuong khenThuong, string maNguoi, string lyDo, DateOnly? ngayKhenThuong)
    {
        if (ModelState.IsValid)
        {
            // Tạo KhenThuong trước
            _context.Add(khenThuong);
            await _context.SaveChangesAsync();

            // Nếu có thông tin người, tạo KhenThuongNguoi
            if (!string.IsNullOrEmpty(maNguoi))
            {
                var khenThuongNguoi = new KhenThuongNguoi
                {
                    MaKhenThuong = khenThuong.MaKhenThuong,
                    MaNguoi = maNguoi,
                    LyDo = lyDo,
                    NgayKhenThuong = ngayKhenThuong
                };
                _context.KhenThuongNguois.Add(khenThuongNguoi);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        return View("Create", khenThuong);
    }

    // POST: KhenThuong/AddPerson
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPerson(int maKhenThuong, string maNguoi, string lyDo, DateOnly? ngayKhenThuong)
    {
        if (string.IsNullOrEmpty(maNguoi))
        {
            return BadRequest("Vui lòng chọn người được khen thưởng");
        }

        var khenThuongNguoi = new KhenThuongNguoi
        {
            MaKhenThuong = maKhenThuong,
            MaNguoi = maNguoi,
            LyDo = lyDo,
            NgayKhenThuong = ngayKhenThuong
        };

        _context.KhenThuongNguois.Add(khenThuongNguoi);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = maKhenThuong });
    }

    // GET: KhenThuong/CreatePerson
    public async Task<IActionResult> CreatePerson(int maKhenThuong)
    {
        var khenThuong = await _context.KhenThuongs.FindAsync(maKhenThuong);
        if (khenThuong == null) return NotFound();

        ViewBag.MaKhenThuong = maKhenThuong;
        ViewBag.TenKhenThuong = khenThuong.TenKhenThuong;
        ViewBag.CapKhenThuong = khenThuong.CapKhenThuong;
        
        // Lấy danh sách người theo quyền
        var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
        System.Diagnostics.Debug.WriteLine($"CreatePerson: UserRole from session: '{userRole}'");
        
        var nguoiList = await GetNguoiListByRole(userRole);
        System.Diagnostics.Debug.WriteLine($"CreatePerson: NguoiList count: {nguoiList.Count}");
        
        ViewBag.NguoiList = nguoiList;
        
        return View();
    }

    // POST: KhenThuong/RemovePerson
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePerson(int maKhenThuong, int maKhenThuongNguoi)
    {
        var khenThuongNguoi = await _context.KhenThuongNguois.FindAsync(maKhenThuongNguoi);
        if (khenThuongNguoi != null)
        {
            _context.KhenThuongNguois.Remove(khenThuongNguoi);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id = maKhenThuong });
    }

    // GET: KhenThuong/EditPerson/5
    public async Task<IActionResult> EditPerson(int? id, int maKhenThuong)
    {
        if (id == null) return NotFound();

        var khenThuongNguoi = await _context.KhenThuongNguois
            .Include(kt => kt.MaNguoiNavigation)
            .FirstOrDefaultAsync(kt => kt.MaKhenThuongNguoi == id);
            
        if (khenThuongNguoi == null) return NotFound();

        var khenThuong = await _context.KhenThuongs.FindAsync(maKhenThuong);
        if (khenThuong == null) return NotFound();

        ViewBag.MaKhenThuong = maKhenThuong;
        ViewBag.TenKhenThuong = khenThuong.TenKhenThuong;
        ViewBag.CapKhenThuong = khenThuong.CapKhenThuong;
        
        // Lấy danh sách người theo quyền
        var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
        var nguoiList = await GetNguoiListByRole(userRole);
        ViewBag.NguoiList = nguoiList;

        return View(khenThuongNguoi);
    }

    // POST: KhenThuong/EditPerson/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPerson(int id, int maKhenThuong, string maNguoi, string lyDo, DateOnly? ngayKhenThuong)
    {
        var khenThuongNguoi = await _context.KhenThuongNguois.FindAsync(id);
        if (khenThuongNguoi == null) return NotFound();

        if (string.IsNullOrEmpty(maNguoi))
        {
            ViewBag.MaKhenThuong = maKhenThuong;
            
            // Lấy danh sách người theo quyền
            var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            var nguoiList = await GetNguoiListByRole(userRole);
            ViewBag.NguoiList = nguoiList;
            
            ModelState.AddModelError("", "Vui lòng chọn người được khen thưởng");
            return View(khenThuongNguoi);
        }

        khenThuongNguoi.MaNguoi = maNguoi;
        khenThuongNguoi.LyDo = lyDo;
        khenThuongNguoi.NgayKhenThuong = ngayKhenThuong;

        _context.Update(khenThuongNguoi);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = maKhenThuong });
    }

    // GET: KhenThuong/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var khenThuong = await _context.KhenThuongs.FindAsync(id);
        if (khenThuong == null) return NotFound();
        return View(khenThuong);
    }

    // POST: KhenThuong/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("MaKhenThuong,TenKhenThuong,CapKhenThuong")] KhenThuong khenThuong)
    {
        if (id != khenThuong.MaKhenThuong) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(khenThuong);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!KhenThuongExists(khenThuong.MaKhenThuong))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(khenThuong);
    }

    // GET: KhenThuong/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var khenThuong = await _context.KhenThuongs
            .Include(k => k.KhenThuongNguois)
            .FirstOrDefaultAsync(m => m.MaKhenThuong == id);

        if (khenThuong == null) return NotFound();
        return View(khenThuong);
    }

    // POST: KhenThuong/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var khenThuong = await _context.KhenThuongs
            .Include(k => k.KhenThuongNguois)
            .FirstOrDefaultAsync(k => k.MaKhenThuong == id);

        if (khenThuong != null)
        {
            if (khenThuong.KhenThuongNguois.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            _context.KhenThuongs.Remove(khenThuong);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool KhenThuongExists(int id)
    {
        return _context.KhenThuongs.Any(e => e.MaKhenThuong == id);
    }

    // Helper method: Lấy danh sách người theo role
    private async Task<List<Nguoi>> GetNguoiListByRole(string userRole)
    {
        if (userRole == "Admin")
        {
            // Admin: Tất cả người (bao gồm cả Nhân viên HQT và Học viên)
            return await _context.Nguois.ToListAsync();
        }
        else if (userRole == "LopTruongLao")
        {
            // LopTruongLao: Chỉ học viên Lào
            return await _context.Nguois
                .Where(n => n.LoaiNguoi == "HocVien" && n.QuocTich.Contains("Lào"))
                .ToListAsync();
        }
        else if (userRole == "LopTruongCam")
        {
            // LopTruongCam: Chỉ học viên Campuchia
            return await _context.Nguois
                .Where(n => n.LoaiNguoi == "HocVien" && n.QuocTich.Contains("Campuchia"))
                .ToListAsync();
        }
        else
        {
            // Role khác: Không có quyền
            return new List<Nguoi>();
        }
    }
} 