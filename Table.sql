-- Bảng LoaiDangVien (Loại đảng viên)
CREATE TABLE LoaiDangVien (
    MaLoaiDangVien INT PRIMARY KEY IDENTITY(1,1),
    TenLoai NVARCHAR(50) NOT NULL  -- Ví dụ: Đảng viên chính thức, Dự bị
);

-- Bảng TrinhDo (Trình độ)
CREATE TABLE TrinhDo (
    MaTrinhDo INT PRIMARY KEY IDENTITY(1,1),
    TenTrinhDo NVARCHAR(100) NOT NULL  -- Ví dụ: Đại học, Thạc sĩ
);

-- Bảng Nguoi (Gộp QuanNhan và HocVien)
CREATE TABLE Nguoi (
    MaNguoi NVARCHAR(50) PRIMARY KEY,
    LoaiNguoi NVARCHAR(20) NOT NULL,  -- 'QuanNhan' hoặc 'HocVien'
    QuocTich NVARCHAR(50),  -- NULL cho QuanNhan, bắt buộc cho HocVien
    HoTen NVARCHAR(100) NOT NULL,
    NgaySinh DATE,
    CapBac NVARCHAR(100),
    ChucVu NVARCHAR(100),
    MaTrinhDo INT,         -- Trình độ
    ChuyenNganh NVARCHAR(100),  -- Chuyên ngành đào tạo
    TruongHoc NVARCHAR(100),
    QuaTruong Date,
    NamNhapNgu Date,
    KhoaHoc NVARCHAR(50),  -- Khóa học
    NgayNhapHoc DATE,      -- Năm nhập học
    NgayTotNghiep DATE,    -- Năm tốt nghiệp
    MaLoaiDangVien INT,    -- Loại đảng viên
    NgayVaoDangChinhThuc DATE,
    NgayVaoDangDuBi DATE,
    FOREIGN KEY (MaTrinhDo) REFERENCES TrinhDo(MaTrinhDo),
    FOREIGN KEY (MaLoaiDangVien) REFERENCES LoaiDangVien(MaLoaiDangVien),
    CHECK (LoaiNguoi IN ('NhanVien', 'HocVien')),
    CHECK (LoaiNguoi = 'HocVien' OR QuocTich IS NOT NULL)  -- QuocTich bắt buộc với HocVien
);

-- Bảng KhenThuong (Thông tin chung về khen thưởng)
CREATE TABLE KhenThuong (
    MaKhenThuong INT PRIMARY KEY IDENTITY(1,1),
    TenKhenThuong NVARCHAR(255) NOT NULL,
    CapKhenThuong NVARCHAR(100)
);

-- Bảng KhenThuong_Nguoi (Liên kết khen thưởng với nhiều người)
CREATE TABLE KhenThuong_Nguoi (
    MaKhenThuongNguoi INT PRIMARY KEY IDENTITY(1,1),
    MaKhenThuong INT NOT NULL,
    MaNguoi NVARCHAR(50) NOT NULL,
    LyDo NVARCHAR(255),
    NgayKhenThuong DATE,
    FOREIGN KEY (MaKhenThuong) REFERENCES KhenThuong(MaKhenThuong),
    FOREIGN KEY (MaNguoi) REFERENCES Nguoi(MaNguoi)
);

-- Bảng KyLuat (Thông tin chung về kỷ luật)
CREATE TABLE KyLuat (
    MaKyLuat INT PRIMARY KEY IDENTITY(1,1),
    TenKyLuat NVARCHAR(255) NOT NULL,  -- Ví dụ: Khiển trách, Cảnh cáo
    MucDo NVARCHAR(100)
);

-- Bảng KyLuat_Nguoi (Liên kết kỷ luật với nhiều người)
CREATE TABLE KyLuat_Nguoi (
    MaKyLuatNguoi INT PRIMARY KEY IDENTITY(1,1),
    MaKyLuat INT NOT NULL,
    MaNguoi NVARCHAR(50) NOT NULL,
    LyDo NVARCHAR(255),
    NgayKyLuat DATE,
    FOREIGN KEY (MaKyLuat) REFERENCES KyLuat(MaKyLuat),
    FOREIGN KEY (MaNguoi) REFERENCES Nguoi(MaNguoi)
);


-- Bảng PhongTraoThiDua (Phong trào thi đua)
CREATE TABLE PhongTraoThiDua (
    MaPTTD INT PRIMARY KEY IDENTITY(1,1),
    TenPhongTrao NVARCHAR(255) NOT NULL,
    ThoiGianBatDau DATE,
    ThoiGianKetThuc DATE,
    PhanTramKhenThuong DECIMAL(5,2),
    MoTa NVARCHAR(255)
);

-- Bảng NguoiDatDuocPTTD (Người đạt được trong Phong trào thi đua)
CREATE TABLE NguoiDatDuocPTTD (
    MaDatDuoc INT PRIMARY KEY IDENTITY(1,1),
    MaPTTD INT NOT NULL,
    MaNguoi NVARCHAR(50) NOT NULL,
    GhiChu NVARCHAR(255),
    FOREIGN KEY (MaPTTD) REFERENCES PhongTraoThiDua(MaPTTD),
    FOREIGN KEY (MaNguoi) REFERENCES Nguoi(MaNguoi)
);


-- Bảng KetQuaHocTap (Kết quả học tập, chỉ áp dụng cho HocVien)
CREATE TABLE KetQuaHocTap (
    MaKetQua INT PRIMARY KEY IDENTITY(1,1),
    MaNguoi NVARCHAR(50) NOT NULL,
    NamHoc NVARCHAR(9),  -- Ví dụ: 2024-2025
    DiemTrungBinh DECIMAL(5,2),
    GhiChu NVARCHAR(255),
    FOREIGN KEY (MaNguoi) REFERENCES Nguoi(MaNguoi)
);

-- Bảng ThongKe (Thống kê chung)
CREATE TABLE ThongKe (
    MaThongKe INT PRIMARY KEY IDENTITY(1,1),
    NamThongKe INT,  -- Năm thống kê
    TongHocVien INT,  -- Tổng số học viên
    TongQuanNhan INT,  -- Tổng số quân nhân
    TongLoaiDangVien INT,
    GhiChu NVARCHAR(255)
);

-- Bảng Roles (Vai trò)
CREATE TABLE Roles (
    MaRole INT PRIMARY KEY IDENTITY(1,1),
    TenRole NVARCHAR(50) NOT NULL UNIQUE,  -- Ví dụ: Admin, User, Guest
    MoTa NVARCHAR(255)
);

CREATE TABLE Users (
    MaUser INT PRIMARY KEY IDENTITY(1,1),
    MaNguoi NVARCHAR(50),  -- Liên kết với Nguoi
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    MaRole INT NOT NULL,
    Active BIT DEFAULT 1,
    FOREIGN KEY (MaNguoi) REFERENCES Nguoi(MaNguoi),
    FOREIGN KEY (MaRole) REFERENCES Roles(MaRole)
);


-- Thêm index để tối ưu
CREATE INDEX IDX_Nguoi_HoTen ON Nguoi(HoTen);
GO