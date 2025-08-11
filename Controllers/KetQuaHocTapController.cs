using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLHV.Models;

namespace QLHV.Controllers;

public class KetQuaHocTapController : Controller
{
    private readonly QlhvContext _context;
    public KetQuaHocTapController(QlhvContext context) { _context = context; }

    // GET: KetQuaHocTap
    public async Task<IActionResult> Index()
    {
        return View();
    }

    // GET: KetQuaHocTap/GetData
    public async Task<IActionResult> GetData()
    {
        var ketQuas = await _context.KetQuaHocTaps
            .Include(k => k.MaNguoiNavigation)
            .Select(k => new
            {
                k.MaKetQua,
                k.MaNguoi,
                HoTen = k.MaNguoiNavigation.HoTen,
                k.NamHoc,
                k.DiemTrungBinh,
                k.GhiChu
            })
            .ToListAsync();

        return Json(new { data = ketQuas });
    }

    // GET: KetQuaHocTap/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var ketQua = await _context.KetQuaHocTaps
            .Include(k => k.MaNguoiNavigation)
            .FirstOrDefaultAsync(k => k.MaKetQua == id);

        if (ketQua == null) return NotFound();

        return View(ketQua);
    }

    // GET: KetQuaHocTap/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.MaNguoi = new SelectList(await _context.Nguois.ToListAsync(), "MaNguoi", "HoTen");
        return View();
    }

    // POST: KetQuaHocTap/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("MaNguoi,NamHoc,DiemTrungBinh,GhiChu")] KetQuaHocTap ketQua)
    {
        if (ModelState.IsValid)
        {
            // Kiểm tra xem đã có kết quả học tập cho người này trong năm học này chưa
            var exists = await _context.KetQuaHocTaps
                .AnyAsync(k => k.MaNguoi == ketQua.MaNguoi && k.NamHoc == ketQua.NamHoc);

            if (exists)
            {
                ModelState.AddModelError("", "Đã có kết quả học tập cho người này trong năm học này!");
                ViewBag.MaNguoi = new SelectList(await _context.Nguois.ToListAsync(), "MaNguoi", "HoTen");
                return View(ketQua);
            }

            _context.Add(ketQua);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewBag.MaNguoi = new SelectList(await _context.Nguois.ToListAsync(), "MaNguoi", "HoTen");
        return View(ketQua);
    }

    // GET: KetQuaHocTap/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var ketQua = await _context.KetQuaHocTaps.FindAsync(id);
        if (ketQua == null) return NotFound();

        ViewBag.MaNguoi = new SelectList(await _context.Nguois.ToListAsync(), "MaNguoi", "HoTen");
        return View(ketQua);
    }

    // POST: KetQuaHocTap/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("MaKetQua,MaNguoi,NamHoc,DiemTrungBinh,GhiChu")] KetQuaHocTap ketQua)
    {
        if (id != ketQua.MaKetQua) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                // Kiểm tra xem có kết quả học tập khác cho người này trong năm học này không
                var exists = await _context.KetQuaHocTaps
                    .AnyAsync(k => k.MaKetQua != ketQua.MaKetQua && 
                                   k.MaNguoi == ketQua.MaNguoi && 
                                   k.NamHoc == ketQua.NamHoc);

                if (exists)
                {
                    ModelState.AddModelError("", "Đã có kết quả học tập cho người này trong năm học này!");
                    ViewBag.MaNguoi = new SelectList(await _context.Nguois.ToListAsync(), "MaNguoi", "HoTen");
                    return View(ketQua);
                }

                _context.Update(ketQua);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!KetQuaExists(ketQua.MaKetQua))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }

        ViewBag.MaNguoi = new SelectList(await _context.Nguois.ToListAsync(), "MaNguoi", "HoTen");
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

        return View(ketQua);
    }

    // POST: KetQuaHocTap/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var ketQua = await _context.KetQuaHocTaps.FindAsync(id);
        if (ketQua != null)
        {
            _context.KetQuaHocTaps.Remove(ketQua);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool KetQuaExists(int id)
    {
        return _context.KetQuaHocTaps.Any(e => e.MaKetQua == id);
    }
}

