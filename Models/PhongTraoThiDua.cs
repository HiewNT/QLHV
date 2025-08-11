using System;
using System.Collections.Generic;

namespace QLHV.Models;

public partial class PhongTraoThiDua
{
    public int MaPttd { get; set; }

    public string TenPhongTrao { get; set; } = null!;

    public DateOnly? ThoiGianBatDau { get; set; }

    public DateOnly? ThoiGianKetThuc { get; set; }

    public decimal? PhanTramKhenThuong { get; set; }

    public string? MoTa { get; set; }

    public virtual ICollection<NguoiDatDuocPttd> NguoiDatDuocPttds { get; set; } = new List<NguoiDatDuocPttd>();
}
