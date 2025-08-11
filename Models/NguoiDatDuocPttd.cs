using System;
using System.Collections.Generic;

namespace QLHV.Models;

public partial class NguoiDatDuocPttd
{
    public int MaDatDuoc { get; set; }

    public int MaPttd { get; set; }

    public string MaNguoi { get; set; } = null!;

    public string? GhiChu { get; set; }

    public virtual Nguoi MaNguoiNavigation { get; set; } = null!;

    public virtual PhongTraoThiDua MaPttdNavigation { get; set; } = null!;
}
