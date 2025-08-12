using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLHV.Models;

namespace QLHV.Controllers;

public class PhongTraoThiDuaController : Controller
{
    private readonly QlhvContext _context;
    public PhongTraoThiDuaController(QlhvContext context) { _context = context; }

    // GET: PhongTraoThiDua
    public async Task<IActionResult> Index()
    {
        return View();
    }

    // GET: PhongTraoThiDua/GetData
    public async Task<IActionResult> GetData()
    {
        var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;

        var phongTraos = await _context.PhongTraoThiDuas
            .Include(p => p.NguoiDatDuocPttds)
            .ThenInclude(ndd => ndd.MaNguoiNavigation)
            .ToListAsync();

        if (userRole == "LopTruongLao")
        {
            foreach (var pt in phongTraos)
            {
                pt.NguoiDatDuocPttds = pt.NguoiDatDuocPttds
                    .Where(ndd => ndd.MaNguoiNavigation?.LoaiNguoi == "HocVien" && ndd.MaNguoiNavigation?.QuocTich.Contains("Lào") == true)
                    .ToList();
            }
        }
        else if (userRole == "LopTruongCam")
        {
            foreach (var pt in phongTraos)
            {
                pt.NguoiDatDuocPttds = pt.NguoiDatDuocPttds
                    .Where(ndd => ndd.MaNguoiNavigation?.LoaiNguoi == "HocVien" && ndd.MaNguoiNavigation?.QuocTich.Contains("Campuchia") == true)
                    .ToList();
            }
        }

        var result = phongTraos.Select(p => new
        {
            p.MaPttd,
            p.TenPhongTrao,
            p.ThoiGianBatDau,
            p.ThoiGianKetThuc,
            p.PhanTramKhenThuong,
            p.MoTa,
            SoNguoiDatDuoc = p.NguoiDatDuocPttds.Count
        }).ToList();

        return Json(new { data = result });
    }

    // GET: PhongTraoThiDua/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var phongTrao = await _context.PhongTraoThiDuas
            .Include(p => p.NguoiDatDuocPttds)
                .ThenInclude(n => n.MaNguoiNavigation)
            .FirstOrDefaultAsync(p => p.MaPttd == id);

        if (phongTrao == null) return NotFound();

        // Lấy danh sách người theo quyền
        var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
        var nguoiList = await GetNguoiListByRole(userRole);
        ViewBag.NguoiList = nguoiList;

        return View(phongTrao);
    }

    // GET: PhongTraoThiDua/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: PhongTraoThiDua/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("TenPhongTrao,ThoiGianBatDau,ThoiGianKetThuc,PhanTramKhenThuong,MoTa")] PhongTraoThiDua phongTrao)
    {
        if (ModelState.IsValid)
        {
            _context.Add(phongTrao);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(phongTrao);
    }

    // POST: PhongTraoThiDua/AddPerson
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPerson(int maPttd, string maNguoi, string ghiChu)
    {
        if (string.IsNullOrEmpty(maNguoi))
        {
            return BadRequest("Vui lòng chọn người đạt được phong trào");
        }

        // Kiểm tra xem người này đã đạt được phong trào này chưa
        var exists = await _context.NguoiDatDuocPttds
            .AnyAsync(n => n.MaPttd == maPttd && n.MaNguoi == maNguoi);

        if (exists)
        {
            return BadRequest("Người này đã đạt được phong trào này rồi!");
        }

        var nguoiDatDuoc = new NguoiDatDuocPttd
        {
            MaPttd = maPttd,
            MaNguoi = maNguoi,
            GhiChu = ghiChu
        };

        _context.NguoiDatDuocPttds.Add(nguoiDatDuoc);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = maPttd });
    }

    // GET: PhongTraoThiDua/CreatePerson
    public async Task<IActionResult> CreatePerson(int maPttd)
    {
        var phongTrao = await _context.PhongTraoThiDuas.FindAsync(maPttd);
        if (phongTrao == null) return NotFound();

        ViewBag.MaPttd = maPttd;
        ViewBag.TenPhongTrao = phongTrao.TenPhongTrao;
        ViewBag.ThoiGianBatDau = phongTrao.ThoiGianBatDau;
        ViewBag.ThoiGianKetThuc = phongTrao.ThoiGianKetThuc;
        ViewBag.PhanTramKhenThuong = phongTrao.PhanTramKhenThuong;
        ViewBag.MoTa = phongTrao.MoTa;
        
        // Lấy danh sách người theo quyền
        var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
        var nguoiList = await GetNguoiListByRole(userRole);
        ViewBag.NguoiList = nguoiList;
        
        return View();
    }

    // POST: PhongTraoThiDua/RemovePerson
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePerson(int maPttd, int maDatDuoc)
    {
        var nguoiDatDuoc = await _context.NguoiDatDuocPttds.FindAsync(maDatDuoc);
        if (nguoiDatDuoc != null)
        {
            _context.NguoiDatDuocPttds.Remove(nguoiDatDuoc);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id = maPttd });
    }

    // GET: PhongTraoThiDua/EditPerson/5
    public async Task<IActionResult> EditPerson(int? id, int maPttd)
    {
        if (id == null) return NotFound();

        var nguoiDatDuoc = await _context.NguoiDatDuocPttds
            .Include(nt => nt.MaNguoiNavigation)
            .FirstOrDefaultAsync(nt => nt.MaDatDuoc == id);

        if (nguoiDatDuoc == null) return NotFound();

        var phongTrao = await _context.PhongTraoThiDuas.FindAsync(maPttd);
        if (phongTrao == null) return NotFound();

        ViewBag.MaPttd = maPttd;
        ViewBag.TenPhongTrao = phongTrao.TenPhongTrao;
        ViewBag.ThoiGianBatDau = phongTrao.ThoiGianBatDau;
        ViewBag.ThoiGianKetThuc = phongTrao.ThoiGianKetThuc;
        ViewBag.PhanTramKhenThuong = phongTrao.PhanTramKhenThuong;
        ViewBag.MoTa = phongTrao.MoTa;
        
        // Lấy danh sách người theo quyền
        var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
        var nguoiList = await GetNguoiListByRole(userRole);
        ViewBag.NguoiList = nguoiList;

        return View(nguoiDatDuoc);
    }

    // POST: PhongTraoThiDua/EditPerson/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPerson(int id, int maPttd, string maNguoi, string ghiChu)
    {
        var nguoiDatDuoc = await _context.NguoiDatDuocPttds.FindAsync(id);
        if (nguoiDatDuoc == null) return NotFound();

        if (string.IsNullOrEmpty(maNguoi))
        {
            ViewBag.MaPttd = maPttd;
            
            // Lấy danh sách người theo quyền
            var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            var nguoiList = await GetNguoiListByRole(userRole);
            ViewBag.NguoiList = nguoiList;
            
            ModelState.AddModelError("", "Vui lòng chọn người đạt được phong trào");
            return View(nguoiDatDuoc);
        }

        nguoiDatDuoc.MaNguoi = maNguoi;
        nguoiDatDuoc.GhiChu = ghiChu;

        _context.Update(nguoiDatDuoc);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = maPttd });
    }

    // GET: PhongTraoThiDua/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var phongTrao = await _context.PhongTraoThiDuas.FindAsync(id);
        if (phongTrao == null) return NotFound();

        return View(phongTrao);
    }

    // POST: PhongTraoThiDua/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("MaPttd,TenPhongTrao,ThoiGianBatDau,ThoiGianKetThuc,PhanTramKhenThuong,MoTa")] PhongTraoThiDua phongTrao)
    {
        if (id != phongTrao.MaPttd) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(phongTrao);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PhongTraoExists(phongTrao.MaPttd))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(phongTrao);
    }

    // GET: PhongTraoThiDua/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var phongTrao = await _context.PhongTraoThiDuas
            .Include(p => p.NguoiDatDuocPttds)
            .FirstOrDefaultAsync(m => m.MaPttd == id);

        if (phongTrao == null) return NotFound();

        return View(phongTrao);
    }

    // POST: PhongTraoThiDua/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var phongTrao = await _context.PhongTraoThiDuas
            .Include(p => p.NguoiDatDuocPttds)
            .FirstOrDefaultAsync(p => p.MaPttd == id);

        if (phongTrao != null)
        {
            if (phongTrao.NguoiDatDuocPttds.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            _context.PhongTraoThiDuas.Remove(phongTrao);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool PhongTraoExists(int id)
    {
        return _context.PhongTraoThiDuas.Any(e => e.MaPttd == id);
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
