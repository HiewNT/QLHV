using System;
using System.Collections.Generic;

namespace QLHV.Models;

public partial class Role
{
    public int MaRole { get; set; }

    public string TenRole { get; set; } = null!;

    public string? MoTa { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
