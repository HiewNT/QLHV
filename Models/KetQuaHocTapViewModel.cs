using System.ComponentModel.DataAnnotations;

namespace QLHV.Models
{
    public class KetQuaHocTapViewModel
    {
        public int MaKetQua { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn học viên")]
        [Display(Name = "Học viên")]
        public string MaNguoi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn học kỳ")]
        [Display(Name = "Học kỳ")]
        public int MaHocKy { get; set; }

        [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
        [Display(Name = "Điểm trung bình")]
        public decimal? DiemTrungBinh { get; set; }

        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // Navigation properties cho hiển thị
        public string? HoTen { get; set; }
        public string? TenHocKy { get; set; }
        public string? NamHoc { get; set; }
    }
}
