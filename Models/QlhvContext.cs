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
        modelBuilder.Entity<KetQuaHocTap>(entity =>
        {
            entity.HasKey(e => e.MaKetQua).HasName("PK__KetQuaHo__D5B3102AEE9A3E1A");

            entity.ToTable("KetQuaHocTap");

            entity.Property(e => e.DiemTrungBinh).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.MaNguoi).HasMaxLength(50);
            entity.Property(e => e.NamHoc).HasMaxLength(9);

            entity.HasOne(d => d.MaNguoiNavigation).WithMany(p => p.KetQuaHocTaps)
                .HasForeignKey(d => d.MaNguoi)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KetQuaHoc__MaNgu__52593CB8");
        });

        modelBuilder.Entity<KhenThuong>(entity =>
        {
            entity.HasKey(e => e.MaKhenThuong).HasName("PK__KhenThuo__50C85FF719023CDD");

            entity.ToTable("KhenThuong");

            entity.Property(e => e.CapKhenThuong).HasMaxLength(100);
            entity.Property(e => e.TenKhenThuong).HasMaxLength(255);
        });

        modelBuilder.Entity<KhenThuongNguoi>(entity =>
        {
            entity.HasKey(e => e.MaKhenThuongNguoi).HasName("PK__KhenThuo__7DD6DF4EA80EC119");

            entity.ToTable("KhenThuong_Nguoi");

            entity.Property(e => e.LyDo).HasMaxLength(255);
            entity.Property(e => e.MaNguoi).HasMaxLength(50);

            entity.HasOne(d => d.MaKhenThuongNavigation).WithMany(p => p.KhenThuongNguois)
                .HasForeignKey(d => d.MaKhenThuong)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KhenThuon__MaKhe__4316F928");

            entity.HasOne(d => d.MaNguoiNavigation).WithMany(p => p.KhenThuongNguois)
                .HasForeignKey(d => d.MaNguoi)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KhenThuon__MaNgu__440B1D61");
        });

        modelBuilder.Entity<KyLuat>(entity =>
        {
            entity.HasKey(e => e.MaKyLuat).HasName("PK__KyLuat__A675BE7C7BA648E7");

            entity.ToTable("KyLuat");

            entity.Property(e => e.MucDo).HasMaxLength(100);
            entity.Property(e => e.TenKyLuat).HasMaxLength(255);
        });

        modelBuilder.Entity<KyLuatNguoi>(entity =>
        {
            entity.HasKey(e => e.MaKyLuatNguoi).HasName("PK__KyLuat_N__BCE7669DD802821C");

            entity.ToTable("KyLuat_Nguoi");

            entity.Property(e => e.LyDo).HasMaxLength(255);
            entity.Property(e => e.MaNguoi).HasMaxLength(50);

            entity.HasOne(d => d.MaKyLuatNavigation).WithMany(p => p.KyLuatNguois)
                .HasForeignKey(d => d.MaKyLuat)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KyLuat_Ng__MaKyL__48CFD27E");

            entity.HasOne(d => d.MaNguoiNavigation).WithMany(p => p.KyLuatNguois)
                .HasForeignKey(d => d.MaNguoi)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KyLuat_Ng__MaNgu__49C3F6B7");
        });

        modelBuilder.Entity<LoaiDangVien>(entity =>
        {
            entity.HasKey(e => e.MaLoaiDangVien).HasName("PK__LoaiDang__AA3AF98BDCAC3774");

            entity.ToTable("LoaiDangVien");

            entity.Property(e => e.TenLoai).HasMaxLength(50);
        });

        modelBuilder.Entity<Nguoi>(entity =>
        {
            entity.HasKey(e => e.MaNguoi).HasName("PK__Nguoi__ABEE19FDF79E4D52");

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
                .HasConstraintName("FK__Nguoi__MaLoaiDan__3C69FB99");

            entity.HasOne(d => d.MaTrinhDoNavigation).WithMany(p => p.Nguois)
                .HasForeignKey(d => d.MaTrinhDo)
                .HasConstraintName("FK__Nguoi__MaTrinhDo__3B75D760");
        });

        modelBuilder.Entity<NguoiDatDuocPttd>(entity =>
        {
            entity.HasKey(e => e.MaDatDuoc).HasName("PK__NguoiDat__87968C721ED65089");

            entity.ToTable("NguoiDatDuocPTTD");

            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.MaNguoi).HasMaxLength(50);
            entity.Property(e => e.MaPttd).HasColumnName("MaPTTD");

            entity.HasOne(d => d.MaNguoiNavigation).WithMany(p => p.NguoiDatDuocPttds)
                .HasForeignKey(d => d.MaNguoi)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NguoiDatD__MaNgu__4F7CD00D");

            entity.HasOne(d => d.MaPttdNavigation).WithMany(p => p.NguoiDatDuocPttds)
                .HasForeignKey(d => d.MaPttd)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NguoiDatD__MaPTT__4E88ABD4");
        });

        modelBuilder.Entity<PhongTraoThiDua>(entity =>
        {
            entity.HasKey(e => e.MaPttd).HasName("PK__PhongTra__B30A28729391A85F");

            entity.ToTable("PhongTraoThiDua");

            entity.Property(e => e.MaPttd).HasColumnName("MaPTTD");
            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.PhanTramKhenThuong).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TenPhongTrao).HasMaxLength(255);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.MaRole).HasName("PK__Roles__0639A0FDD555EDCB");

            entity.HasIndex(e => e.TenRole, "UQ__Roles__37A723F3F362FE5C").IsUnique();

            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenRole).HasMaxLength(50);
        });

        modelBuilder.Entity<TrinhDo>(entity =>
        {
            entity.HasKey(e => e.MaTrinhDo).HasName("PK__TrinhDo__B64C90D335ECCCF0");

            entity.ToTable("TrinhDo");

            entity.Property(e => e.TenTrinhDo).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.MaUser).HasName("PK__Users__55DAC4B716992D2A");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E4380C07F8").IsUnique();

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.MaNguoi).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.MaNguoiNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.MaNguoi)
                .HasConstraintName("FK__Users__MaNguoi__6477ECF3");

            entity.HasOne(d => d.MaRoleNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.MaRole)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__MaRole__656C112C");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
