using System;
using System.Collections.Generic;

namespace QLHV.Models;

public partial class HocKy
{
    public int MaHocKy { get; set; }

    public string? TenHocKy { get; set; }

    public string? NamHoc { get; set; }

    public virtual ICollection<KetQuaHocTap> KetQuaHocTaps { get; set; } = new List<KetQuaHocTap>();
}
