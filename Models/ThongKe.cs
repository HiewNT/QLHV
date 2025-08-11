using System;
using System.Collections.Generic;

namespace QLHV.Models;

public partial class ThongKe
{
    public int MaThongKe { get; set; }

    public int? NamThongKe { get; set; }

    public int? TongHocVien { get; set; }

    public int? TongQuanNhan { get; set; }

    public int? TongLoaiDangVien { get; set; }

    public string? GhiChu { get; set; }
}
