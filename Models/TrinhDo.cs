using System;
using System.Collections.Generic;

namespace QLHV.Models;

public partial class TrinhDo
{
    public int MaTrinhDo { get; set; }

    public string TenTrinhDo { get; set; } = null!;

    public virtual ICollection<Nguoi> Nguois { get; set; } = new List<Nguoi>();
}
