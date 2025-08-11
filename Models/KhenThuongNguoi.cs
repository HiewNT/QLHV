using System;
using System.Collections.Generic;

namespace QLHV.Models;

public partial class KhenThuongNguoi
{
    public int MaKhenThuongNguoi { get; set; }

    public int MaKhenThuong { get; set; }

    public string MaNguoi { get; set; } = null!;

    public string? LyDo { get; set; }

    public DateOnly? NgayKhenThuong { get; set; }

    public virtual KhenThuong MaKhenThuongNavigation { get; set; } = null!;

    public virtual Nguoi MaNguoiNavigation { get; set; } = null!;
}
