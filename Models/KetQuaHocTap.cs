using System;
using System.Collections.Generic;

namespace QLHV.Models;

public partial class KetQuaHocTap
{
    public int MaKetQua { get; set; }

    public string MaNguoi { get; set; } = null!;

    public string? NamHoc { get; set; }

    public decimal? DiemTrungBinh { get; set; }

    public string? GhiChu { get; set; }

    public virtual Nguoi MaNguoiNavigation { get; set; } = null!;
}
