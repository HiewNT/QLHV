using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QLHV.Models;

public partial class QlhvContext : DbContext
{
    public QlhvContext()
    {
    }

    public QlhvContext(DbContextOptions<QlhvContext> options)
        : base(options)
    {
    }

    public virtual DbSet<HocKy> HocKies { get; set; }

    public virtual DbSet<KetQuaHocTap> KetQuaHocTaps { get; set; }

    public virtual DbSet<KhenThuong> KhenThuongs { get; set; }

    public virtual DbSet<KhenThuongNguoi> KhenThuongNguois { get; set; }

    public virtual DbSet<KyLuat> KyLuats { get; set; }

    public virtual DbSet<KyLuatNguoi> KyLuatNguois { get; set; }

    public virtual DbSet<LoaiDangVien> LoaiDangViens { get; set; }

    public virtual DbSet<Nguoi> Nguois { get; set; }

    public virtual DbSet<NguoiDatDuocPttd> NguoiDatDuocPttds { get; set; }

    public virtual DbSet<PhongTraoThiDua> PhongTraoThiDuas { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<TrinhDo> TrinhDos { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=HIEW;Database=QLHV;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HocKy>(entity =>
        {
            entity.HasKey(e => e.MaHocKy).HasName("PK__HocKy__1EB5511090462E34");

            entity.ToTable("HocKy");

            entity.Property(e => e.NamHoc).HasMaxLength(20);
            entity.Property(e => e.TenHocKy).HasMaxLength(50);
        });

        modelBuilder.Entity<KetQuaHocTap>(entity =>
        {
            entity.HasKey(e => e.MaKetQua).HasName("PK__KetQuaHo__D5B3102ACE552A7B");

            entity.ToTable("KetQuaHocTap");

            entity.Property(e => e.DiemTrungBinh).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.MaNguoi).HasMaxLength(50);

            entity.HasOne(d => d.MaHocKyNavigation).WithMany(p => p.KetQuaHocTaps)
                .HasForeignKey(d => d.MaHocKy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KetQuaHoc__MaHocKy__4F7CD00E");

            entity.HasOne(d => d.MaNguoiNavigation).WithMany(p => p.KetQuaHocTaps)
                .HasForeignKey(d => d.MaNguoi)
                .HasConstraintName("FK__KetQuaHoc__MaNgu__4F7CD00D");
        });

        modelBuilder.Entity<KhenThuong>(entity =>
        {
            entity.HasKey(e => e.MaKhenThuong).HasName("PK__KhenThuo__50C85FF76E8BED35");

            entity.ToTable("KhenThuong");

            entity.Property(e => e.CapKhenThuong).HasMaxLength(100);
            entity.Property(e => e.TenKhenThuong).HasMaxLength(255);
        });

        modelBuilder.Entity<KhenThuongNguoi>(entity =>
        {
            entity.HasKey(e => e.MaKhenThuongNguoi).HasName("PK__KhenThuo__7DD6DF4E989BC68E");

            entity.ToTable("KhenThuong_Nguoi");

            entity.Property(e => e.LyDo).HasMaxLength(255);
            entity.Property(e => e.MaNguoi).HasMaxLength(50);

            entity.HasOne(d => d.MaKhenThuongNavigation).WithMany(p => p.KhenThuongNguois)
                .HasForeignKey(d => d.MaKhenThuong)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KhenThuon__MaKhe__534D60F1");

            entity.HasOne(d => d.MaNguoiNavigation).WithMany(p => p.KhenThuongNguois)
                .HasForeignKey(d => d.MaNguoi)
                .HasConstraintName("FK__KhenThuon__MaNgu__5165187F");
        });

        modelBuilder.Entity<KyLuat>(entity =>
        {
            entity.HasKey(e => e.MaKyLuat).HasName("PK__KyLuat__A675BE7C1EED23F1");

            entity.ToTable("KyLuat");

            entity.Property(e => e.MucDo).HasMaxLength(100);
            entity.Property(e => e.TenKyLuat).HasMaxLength(255);
        });

        modelBuilder.Entity<KyLuatNguoi>(entity =>
        {
            entity.HasKey(e => e.MaKyLuatNguoi).HasName("PK__KyLuat_N__BCE7669D0C70D304");

            entity.ToTable("KyLuat_Nguoi");

            entity.Property(e => e.LyDo).HasMaxLength(255);
            entity.Property(e => e.MaNguoi).HasMaxLength(50);

            entity.HasOne(d => d.MaKyLuatNavigation).WithMany(p => p.KyLuatNguois)
                .HasForeignKey(d => d.MaKyLuat)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KyLuat_Ng__MaKyL__5535A963");

            entity.HasOne(d => d.MaNguoiNavigation).WithMany(p => p.KyLuatNguois)
                .HasForeignKey(d => d.MaNguoi)
                .HasConstraintName("FK__KyLuat_Ng__MaNgu__534D60F1");
        });

        modelBuilder.Entity<LoaiDangVien>(entity =>
        {
            entity.HasKey(e => e.MaLoaiDangVien).HasName("PK__LoaiDang__AA3AF98B52A700A5");

            entity.ToTable("LoaiDangVien");

            entity.Property(e => e.TenLoai).HasMaxLength(50);
        });

        modelBuilder.Entity<Nguoi>(entity =>
        {
            entity.HasKey(e => e.MaNguoi).HasName("PK__Nguoi__ABEE19FD51E84ADD");

            entity.ToTable("Nguoi");

            entity.HasIndex(e => e.HoTen, "IDX_Nguoi_HoTen");

            entity.Property(e => e.MaNguoi).HasMaxLength(50);
            entity.Property(e => e.CapBac).HasMaxLength(100);
            entity.Property(e => e.ChucVu).HasMaxLength(100);
            entity.Property(e => e.ChuyenNganh).HasMaxLength(100);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.KhoaHoc).HasMaxLength(50);
            entity.Property(e => e.LoaiNguoi).HasMaxLength(20);
            entity.Property(e => e.QuocTich).HasMaxLength(50);
            entity.Property(e => e.TruongHoc).HasMaxLength(100);

            entity.HasOne(d => d.MaLoaiDangVienNavigation).WithMany(p => p.Nguois)
                .HasForeignKey(d => d.MaLoaiDangVien)
                .HasConstraintName("FK__Nguoi__MaLoaiDan__571DF1D5");

            entity.HasOne(d => d.MaTrinhDoNavigation).WithMany(p => p.Nguois)
                .HasForeignKey(d => d.MaTrinhDo)
                .HasConstraintName("FK__Nguoi__MaTrinhDo__5812160E");
        });

        modelBuilder.Entity<NguoiDatDuocPttd>(entity =>
        {
            entity.HasKey(e => e.MaDatDuoc).HasName("PK__NguoiDat__87968C72EB1A8AF2");

            entity.ToTable("NguoiDatDuocPTTD");

            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.MaNguoi).HasMaxLength(50);
            entity.Property(e => e.MaPttd).HasColumnName("MaPTTD");

            entity.HasOne(d => d.MaNguoiNavigation).WithMany(p => p.NguoiDatDuocPttds)
                .HasForeignKey(d => d.MaNguoi)
                .HasConstraintName("FK__NguoiDatD__MaNgu__5629CD9C");

            entity.HasOne(d => d.MaPttdNavigation).WithMany(p => p.NguoiDatDuocPttds)
                .HasForeignKey(d => d.MaPttd)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NguoiDatD__MaPTT__59FA5E80");
        });

        modelBuilder.Entity<PhongTraoThiDua>(entity =>
        {
            entity.HasKey(e => e.MaPttd).HasName("PK__PhongTra__B30A28729E79DFC5");

            entity.ToTable("PhongTraoThiDua");

            entity.Property(e => e.MaPttd).HasColumnName("MaPTTD");
            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.PhanTramKhenThuong).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TenPhongTrao).HasMaxLength(255);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.MaRole).HasName("PK__Roles__0639A0FD546B9E63");

            entity.HasIndex(e => e.TenRole, "UQ__Roles__37A723F3598B0A75").IsUnique();

            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenRole).HasMaxLength(50);
        });

        modelBuilder.Entity<TrinhDo>(entity =>
        {
            entity.HasKey(e => e.MaTrinhDo).HasName("PK__TrinhDo__B64C90D371710FC1");

            entity.ToTable("TrinhDo");

            entity.Property(e => e.TenTrinhDo).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.MaUser).HasName("PK__Users__55DAC4B7A729DF68");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E44AF3BCD6").IsUnique();

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.MaNguoi).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.MaNguoiNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.MaNguoi)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Users__MaNguoi__5812160E");

            entity.HasOne(d => d.MaRoleNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.MaRole)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__MaRole__5BE2A6F2");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
