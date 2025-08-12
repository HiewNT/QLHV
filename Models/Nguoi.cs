using System;
using System.Collections.Generic;

namespace QLHV.Models;

public partial class Nguoi
{
    public string MaNguoi { get; set; } = null!;

    public string LoaiNguoi { get; set; } = null!;

    public string? QuocTich { get; set; }

    public string HoTen { get; set; } = null!;

    public DateOnly? NgaySinh { get; set; }

    public string? CapBac { get; set; }

    public string? ChucVu { get; set; }

    public int? MaTrinhDo { get; set; }

    public string? ChuyenNganh { get; set; }

    public string? TruongHoc { get; set; }

    public DateOnly? QuaTruong { get; set; }

    public DateOnly? NamNhapNgu { get; set; }

    public string? KhoaHoc { get; set; }

    public DateOnly? NgayNhapHoc { get; set; }

    public DateOnly? NgayTotNghiep { get; set; }

    public int? MaLoaiDangVien { get; set; }

    public DateOnly? NgayVaoDangChinhThuc { get; set; }

    public DateOnly? NgayVaoDangDuBi { get; set; }

    public bool? DoanVien { get; set; }

    public virtual ICollection<KetQuaHocTap> KetQuaHocTaps { get; set; } = new List<KetQuaHocTap>();

    public virtual ICollection<KhenThuongNguoi> KhenThuongNguois { get; set; } = new List<KhenThuongNguoi>();

    public virtual ICollection<KyLuatNguoi> KyLuatNguois { get; set; } = new List<KyLuatNguoi>();

    public virtual LoaiDangVien? MaLoaiDangVienNavigation { get; set; }

    public virtual TrinhDo? MaTrinhDoNavigation { get; set; }

    public virtual ICollection<NguoiDatDuocPttd> NguoiDatDuocPttds { get; set; } = new List<NguoiDatDuocPttd>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
