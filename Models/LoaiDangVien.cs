using System;
using System.Collections.Generic;

namespace QLHV.Models;

public partial class LoaiDangVien
{
    public int MaLoaiDangVien { get; set; }

    public string TenLoai { get; set; } = null!;

    public virtual ICollection<Nguoi> Nguois { get; set; } = new List<Nguoi>();
}
