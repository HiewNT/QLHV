using System;
using System.Collections.Generic;

namespace QLHV.Models;

public partial class User
{
    public int MaUser { get; set; }

    public string? MaNguoi { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int MaRole { get; set; }

    public bool? Active { get; set; }

    public virtual Nguoi? MaNguoiNavigation { get; set; }

    public virtual Role MaRoleNavigation { get; set; } = null!;
}
