using System;
using System.Collections.Generic;

namespace QLHV.Models;

public partial class KyLuatNguoi
{
    public int MaKyLuatNguoi { get; set; }

    public int MaKyLuat { get; set; }

    public string MaNguoi { get; set; } = null!;

    public string? LyDo { get; set; }

    public DateOnly? NgayKyLuat { get; set; }

    public virtual KyLuat MaKyLuatNavigation { get; set; } = null!;

    public virtual Nguoi MaNguoiNavigation { get; set; } = null!;
}
