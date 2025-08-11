using System;
using System.Collections.Generic;

namespace QLHV.Models;

public partial class KyLuat
{
    public int MaKyLuat { get; set; }

    public string TenKyLuat { get; set; } = null!;

    public string? MucDo { get; set; }

    public virtual ICollection<KyLuatNguoi> KyLuatNguois { get; set; } = new List<KyLuatNguoi>();
}
