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
            // Thống kê tổng quan (loại trừ Admin)
            ViewBag.TongHocVien = await _context.Nguois.CountAsync(n => n.LoaiNguoi == "HocVien");
            ViewBag.TongNhanVien = await _context.Nguois.CountAsync(n => n.LoaiNguoi == "NhanVien");
            ViewBag.TongLao = await _context.Nguois.CountAsync(n => n.LoaiNguoi == "HocVien" && n.QuocTich == "Lào");
            ViewBag.TongCam = await _context.Nguois.CountAsync(n => n.LoaiNguoi == "HocVien" && n.QuocTich == "Campuchia");

            // Thống kê Đảng viên (loại trừ Admin)
            ViewBag.DangVienChinhThuc = await _context.Nguois.CountAsync(n => n.NgayVaoDangChinhThuc != null && n.LoaiNguoi != "Admin");
            ViewBag.DangVienDuBi = await _context.Nguois.CountAsync(n => n.NgayVaoDangDuBi != null && n.NgayVaoDangChinhThuc == null && n.LoaiNguoi != "Admin");
            ViewBag.DoanVien = await _context.Nguois.CountAsync(n => n.DoanVien == true && n.LoaiNguoi != "Admin");

            // Thống kê Khen thưởng, Kỷ luật, Phong trào thi đua
            ViewBag.TongKhenThuong = await _context.KhenThuongs.CountAsync();
            ViewBag.TongKyLuat = await _context.KyLuats.CountAsync();
            ViewBag.TongPhongTrao = await _context.PhongTraoThiDuas.CountAsync();
            
            // Thống kê số lượt người được khen thưởng/kỷ luật/phong trào
            ViewBag.LuotKhenThuong = await _context.KhenThuongNguois.CountAsync();
            ViewBag.LuotKyLuat = await _context.KyLuatNguois.CountAsync();
            ViewBag.LuotPhongTrao = await _context.NguoiDatDuocPttds.CountAsync();
            
            // Thống kê kết quả học tập theo năm
            ViewBag.NamHocList = await _context.KetQuaHocTaps
                .Select(k => k.NamHoc)
                .Distinct()
                .OrderByDescending(n => n)
                .ToListAsync();

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

        // GET: ThongKe/GetDoanVienData - API cho biểu đồ Đoàn viên
        public async Task<IActionResult> GetDoanVienData()
        {
            var doanVien = await _context.Nguois.CountAsync(n => n.DoanVien == true && n.LoaiNguoi != "Admin");
            var khongDoanVien = await _context.Nguois.CountAsync(n => n.DoanVien == false && n.LoaiNguoi != "Admin");
            var chuaXacDinh = await _context.Nguois.CountAsync(n => n.DoanVien == null && n.LoaiNguoi != "Admin");

            var data = new
            {
                labels = new[] { "Đoàn viên", "Không phải Đoàn viên", "Chưa xác định" },
                data = new[] { doanVien, khongDoanVien, chuaXacDinh },
                backgroundColor = new[] { "#28a745", "#dc3545", "#6c757d" }
            };

            return Json(data);
        }



        // GET: ThongKe/GetChiTietDangVien - API cho thống kê chi tiết Đảng viên
        public async Task<IActionResult> GetChiTietDangVien()
        {
            var chiTiet = await _context.Nguois
                .Include(n => n.MaLoaiDangVienNavigation)
                .Where(n => n.NgayVaoDangChinhThuc != null || n.NgayVaoDangDuBi != null)
                .GroupBy(n => n.MaLoaiDangVienNavigation != null ? n.MaLoaiDangVienNavigation.TenLoai : "Chưa phân loại")
                .Select(g => new
                {
                    LoaiDangVien = g.Key,
                    SoLuong = g.Count(),
                    ChinhThuc = g.Count(x => x.NgayVaoDangChinhThuc != null),
                    DuBi = g.Count(x => x.NgayVaoDangDuBi != null && x.NgayVaoDangChinhThuc == null)
                })
                .ToListAsync();

            return Json(chiTiet);
        }

        // GET: ThongKe/GetChiTietHocVien - API cho thống kê chi tiết Học viên
        public async Task<IActionResult> GetChiTietHocVien()
        {
            var chiTiet = await _context.Nguois
                .Where(n => n.LoaiNguoi == "HocVien")
                .GroupBy(n => n.QuocTich)
                .Select(g => new
                {
                    QuocTich = g.Key,
                    SoLuong = g.Count(),
                    DoanVien = g.Count(x => x.DoanVien == true),
                    DangVien = g.Count(x => x.NgayVaoDangChinhThuc != null || x.NgayVaoDangDuBi != null)
                })
                .ToListAsync();

            return Json(chiTiet);
        }

        // GET: ThongKe/GetKetQuaHocTapData - API cho thống kê kết quả học tập theo năm
        public async Task<IActionResult> GetKetQuaHocTapData(string namHoc)
        {
            var query = _context.KetQuaHocTaps.AsQueryable();
            
            if (!string.IsNullOrEmpty(namHoc))
            {
                query = query.Where(k => k.NamHoc == namHoc);
            }

            var ketQua = await query.ToListAsync();

            var xuatSac = ketQua.Count(k => k.DiemTrungBinh >= 9.0m);
            var gioi = ketQua.Count(k => k.DiemTrungBinh >= 8.0m && k.DiemTrungBinh < 9.0m);
            var kha = ketQua.Count(k => k.DiemTrungBinh >= 7.0m && k.DiemTrungBinh < 8.0m);
            var trungBinh = ketQua.Count(k => k.DiemTrungBinh >= 5.0m && k.DiemTrungBinh < 7.0m);
            var yeu = ketQua.Count(k => k.DiemTrungBinh < 5.0m);

            var data = new
            {
                labels = new[] { "Xuất sắc (9.0+)", "Giỏi (8.0-8.9)", "Khá (7.0-7.9)", "Trung bình (5.0-6.9)", "Yếu (<5.0)" },
                data = new[] { xuatSac, gioi, kha, trungBinh, yeu },
                backgroundColor = new[] { "#dc3545", "#fd7e14", "#ffc107", "#28a745", "#6c757d" },
                tongSo = ketQua.Count,
                diemTrungBinh = ketQua.Any() ? Math.Round((double)ketQua.Average(k => k.DiemTrungBinh), 2) : 0
            };

            return Json(data);
        }
    }
}
