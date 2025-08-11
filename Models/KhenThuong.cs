using System;
using System.Collections.Generic;

namespace QLHV.Models;

public partial class KhenThuong
{
    public int MaKhenThuong { get; set; }

    public string TenKhenThuong { get; set; } = null!;

    public string? CapKhenThuong { get; set; }

    public virtual ICollection<KhenThuongNguoi> KhenThuongNguois { get; set; } = new List<KhenThuongNguoi>();
}
