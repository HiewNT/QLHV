using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLHV.Models;

namespace QLHV.Controllers;

public class KyLuatController : Controller
{
    private readonly QlhvContext _context;

    public KyLuatController(QlhvContext context)
    {
        _context = context;
    }

    // GET: KyLuat
    public async Task<IActionResult> Index()
    {
        return View();
    }

    // GET: KyLuat/GetData - For DataTables AJAX
    public async Task<IActionResult> GetData()
    {
        var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;

        var kyLuats = await _context.KyLuats
            .Include(k => k.KyLuatNguois)
            .ThenInclude(ktn => ktn.MaNguoiNavigation)
            .ToListAsync();

        if (userRole == "LopTruongLao")
        {
            foreach (var kl in kyLuats)
            {
                kl.KyLuatNguois = kl.KyLuatNguois
                    .Where(ktn => ktn.MaNguoiNavigation?.LoaiNguoi == "HocVien" && ktn.MaNguoiNavigation?.QuocTich.Contains("Lào") == true)
                    .ToList();
            }
        }
        else if (userRole == "LopTruongCam")
        {
            foreach (var kl in kyLuats)
            {
                kl.KyLuatNguois = kl.KyLuatNguois
                    .Where(ktn => ktn.MaNguoiNavigation?.LoaiNguoi == "HocVien" && ktn.MaNguoiNavigation?.QuocTich.Contains("Campuchia") == true)
                    .ToList();
            }
        }

        var result = kyLuats.Select(k => new
        {
            k.MaKyLuat,
            k.TenKyLuat,
            k.MucDo,
            SoNguoiBiKyLuat = k.KyLuatNguois.Count
        }).ToList();

        return Json(new { data = result });
    }

    // GET: KyLuat/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var kyLuat = await _context.KyLuats
            .Include(k => k.KyLuatNguois)
            .ThenInclude(kt => kt.MaNguoiNavigation)
            .FirstOrDefaultAsync(m => m.MaKyLuat == id);

        if (kyLuat == null) return NotFound();

        // Lấy danh sách người theo quyền
        var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
        var nguoiList = await GetNguoiListByRole(userRole);
        ViewBag.NguoiList = nguoiList;

        return View(kyLuat);
    }

    // GET: KyLuat/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: KyLuat/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("TenKyLuat,MucDo")] KyLuat kyLuat)
    {
        if (ModelState.IsValid)
        {
            _context.Add(kyLuat);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(kyLuat);
    }

    // POST: KyLuat/AddPerson
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPerson(int maKyLuat, string maNguoi, string lyDo, DateOnly? ngayKyLuat)
    {
        if (string.IsNullOrEmpty(maNguoi))
        {
            return BadRequest("Vui lòng chọn người bị kỷ luật");
        }

        var kyLuatNguoi = new KyLuatNguoi
        {
            MaKyLuat = maKyLuat,
            MaNguoi = maNguoi,
            LyDo = lyDo,
            NgayKyLuat = ngayKyLuat
        };

        _context.KyLuatNguois.Add(kyLuatNguoi);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = maKyLuat });
    }

    // GET: KyLuat/CreatePerson
    public async Task<IActionResult> CreatePerson(int maKyLuat)
    {
        var kyLuat = await _context.KyLuats.FindAsync(maKyLuat);
        if (kyLuat == null) return NotFound();

        ViewBag.MaKyLuat = maKyLuat;
        ViewBag.TenKyLuat = kyLuat.TenKyLuat;
        ViewBag.MucDo = kyLuat.MucDo;
        
        // Lấy danh sách người theo quyền
        var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
        var nguoiList = await GetNguoiListByRole(userRole);
        ViewBag.NguoiList = nguoiList;
        
        return View();
    }

    // POST: KyLuat/RemovePerson
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePerson(int maKyLuat, int maKyLuatNguoi)
    {
        var kyLuatNguoi = await _context.KyLuatNguois.FindAsync(maKyLuatNguoi);
        if (kyLuatNguoi != null)
        {
            _context.KyLuatNguois.Remove(kyLuatNguoi);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id = maKyLuat });
    }

    // GET: KyLuat/EditPerson/5
    public async Task<IActionResult> EditPerson(int? id, int maKyLuat)
    {
        if (id == null) return NotFound();

        var kyLuatNguoi = await _context.KyLuatNguois
            .Include(kt => kt.MaNguoiNavigation)
            .FirstOrDefaultAsync(kt => kt.MaKyLuatNguoi == id);

        if (kyLuatNguoi == null) return NotFound();

        var kyLuat = await _context.KyLuats.FindAsync(maKyLuat);
        if (kyLuat == null) return NotFound();

        ViewBag.MaKyLuat = maKyLuat;
        ViewBag.TenKyLuat = kyLuat.TenKyLuat;
        ViewBag.MucDo = kyLuat.MucDo;
        
        // Lấy danh sách người theo quyền
        var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
        var nguoiList = await GetNguoiListByRole(userRole);
        ViewBag.NguoiList = nguoiList;

        return View(kyLuatNguoi);
    }

    // POST: KyLuat/EditPerson/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPerson(int id, int maKyLuat, string maNguoi, string lyDo, DateOnly? ngayKyLuat)
    {
        var kyLuatNguoi = await _context.KyLuatNguois.FindAsync(id);
        if (kyLuatNguoi == null) return NotFound();

        if (string.IsNullOrEmpty(maNguoi))
        {
            ViewBag.MaKyLuat = maKyLuat;
            
            // Lấy danh sách người theo quyền
            var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            var nguoiList = await GetNguoiListByRole(userRole);
            ViewBag.NguoiList = nguoiList;
            
            ModelState.AddModelError("", "Vui lòng chọn người bị kỷ luật");
            return View(kyLuatNguoi);
        }

        kyLuatNguoi.MaNguoi = maNguoi;
        kyLuatNguoi.LyDo = lyDo;
        kyLuatNguoi.NgayKyLuat = ngayKyLuat;

        _context.Update(kyLuatNguoi);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = maKyLuat });
    }

    // GET: KyLuat/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var kyLuat = await _context.KyLuats.FindAsync(id);
        if (kyLuat == null) return NotFound();
        return View(kyLuat);
    }

    // POST: KyLuat/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("MaKyLuat,TenKyLuat,MucDo")] KyLuat kyLuat)
    {
        if (id != kyLuat.MaKyLuat) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(kyLuat);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!KyLuatExists(kyLuat.MaKyLuat))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(kyLuat);
    }

    // GET: KyLuat/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var kyLuat = await _context.KyLuats
            .Include(k => k.KyLuatNguois)
            .FirstOrDefaultAsync(m => m.MaKyLuat == id);

        if (kyLuat == null) return NotFound();
        return View(kyLuat);
    }

    // POST: KyLuat/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var kyLuat = await _context.KyLuats
            .Include(k => k.KyLuatNguois)
            .FirstOrDefaultAsync(k => k.MaKyLuat == id);

        if (kyLuat != null)
        {
            if (kyLuat.KyLuatNguois.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            _context.KyLuats.Remove(kyLuat);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool KyLuatExists(int id)
    {
        return _context.KyLuats.Any(e => e.MaKyLuat == id);
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