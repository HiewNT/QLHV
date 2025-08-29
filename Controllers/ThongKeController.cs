using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLHV.Models;
using System.Linq;

namespace QLHV.Controllers
{
    public class ThongKeController : Controller
    {
        private readonly QlhvContext _context;

        public ThongKeController(QlhvContext context)
        {
            _context = context;
        }

        // GET: ThongKe
        public async Task<IActionResult> Index()
        {
            // Thống kê tổng quan
            ViewBag.TongNguoi = await _context.Nguois.CountAsync();
            ViewBag.TongHocVien = await _context.Nguois.CountAsync(n => n.LoaiNguoi == "HocVien");
            ViewBag.TongNhanVien = await _context.Nguois.CountAsync(n => n.LoaiNguoi == "NhanVien");
            ViewBag.TongDangVien = await _context.Nguois.CountAsync(n => (n.NgayVaoDangChinhThuc != null || n.NgayVaoDangDuBi != null) && n.LoaiNguoi != "Admin");

            return View();
        }

        // GET: ThongKe/GetDangVienData - API cho biểu đồ Đảng viên
        public async Task<IActionResult> GetDangVienData()
        {
            var dangVienChinhThuc = await _context.Nguois.CountAsync(n => n.NgayVaoDangChinhThuc != null && n.LoaiNguoi != "Admin");
            var dangVienDuBi = await _context.Nguois.CountAsync(n => n.NgayVaoDangDuBi != null && n.NgayVaoDangChinhThuc == null && n.LoaiNguoi != "Admin");
            var khongDangVien = await _context.Nguois.CountAsync(n => n.NgayVaoDangChinhThuc == null && n.NgayVaoDangDuBi == null && n.LoaiNguoi != "Admin");

            var data = new
            {
                labels = new[] { "Đảng viên chính thức", "Đảng viên dự bị", "Chưa vào Đảng" },
                data = new[] { dangVienChinhThuc, dangVienDuBi, khongDangVien },
                backgroundColor = new[] { "#28a745", "#ffc107", "#6c757d" }
            };

            return Json(data);
        }

        // GET: ThongKe/GetHocVienData - API cho biểu đồ Học viên
        public async Task<IActionResult> GetHocVienData()
        {
            var hocVienLao = await _context.Nguois.CountAsync(n => n.LoaiNguoi == "HocVien" && n.QuocTich == "Lào");
            var hocVienCam = await _context.Nguois.CountAsync(n => n.LoaiNguoi == "HocVien" && n.QuocTich == "Campuchia");
            var nhanVien = await _context.Nguois.CountAsync(n => n.LoaiNguoi == "NhanVien");

            var data = new
            {
                labels = new[] { "Học viên Lào", "Học viên Campuchia", "Nhân viên" },
                data = new[] { hocVienLao, hocVienCam, nhanVien },
                backgroundColor = new[] { "#007bff", "#dc3545", "#28a745" }
            };

            return Json(data);
        }


    }
}
