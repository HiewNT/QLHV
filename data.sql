-- Thêm dữ liệu mẫu cho các bảng tra cứu

-- Thêm dữ liệu cho bảng LoaiDangVien
INSERT INTO LoaiDangVien (TenLoai) VALUES 
(N'Đảng viên chính thức'),
(N'Đảng viên dự bị'),
(N'Quần chúng');

-- Thêm dữ liệu cho bảng TrinhDo
INSERT INTO TrinhDo (TenTrinhDo) VALUES 
(N'Trung học phổ thông'),
(N'Trung cấp'),
(N'Cao đẳng'),
(N'Đại học'),
(N'Thạc sĩ'),
(N'Tiến sĩ');

-- Thêm dữ liệu cho bảng KhenThuong
INSERT INTO KhenThuong (TenKhenThuong, CapKhenThuong) VALUES 
(N'Chiến sĩ thi đua cấp cơ sở', N'Cấp cơ sở'),
(N'Chiến sĩ thi đua cấp bộ', N'Cấp bộ'),
(N'Lao động tiên tiến', N'Cấp cơ sở'),
(N'Giấy khen', N'Cấp cơ sở');

-- Thêm dữ liệu cho bảng KyLuat
INSERT INTO KyLuat (TenKyLuat, MucDo) VALUES 
(N'Khiển trách', N'Nhẹ'),
(N'Cảnh cáo', N'Trung bình'),
(N'Hạ bậc', N'Nặng');

-- Thêm dữ liệu cho bảng PhongTraoThiDua
INSERT INTO PhongTraoThiDua (TenPhongTrao, ThoiGianBatDau, ThoiGianKetThuc, PhanTramKhenThuong, MoTa) VALUES 
(N'Phong trào thi đua năm 2024', '2024-01-01', '2024-12-31', 15.5, N'Thi đua lao động sản xuất năm 2024'),
(N'Tháng cao điểm an toàn', '2024-06-01', '2024-06-30', 20.0, N'Tháng cao điểm an toàn lao động');

-- THÊM DỮ LIỆU CHO NHÂN VIÊN (LoaiNguoi = 'NhanVien')
INSERT INTO Nguoi (
    MaNguoi, LoaiNguoi, QuocTich, HoTen, NgaySinh, CapBac, ChucVu, 
    MaTrinhDo, TruongHoc, QuaTruong, NamNhapNgu, MaLoaiDangVien, 
    NgayVaoDangChinhThuc, NgayVaoDangDuBi
) VALUES 
-- Nhân viên 1 - Bếp trưởng
('NV001', 'NhanVien', N'Việt Nam', N'Nguyễn Văn Anh', '1985-03-15', N'Thượng sĩ', N'Bếp Trưởng', 
 4, N'Trường Đại học Kinh tế Quốc dân', '2010-07-15', '2005-01-20', 1, 
 '2015-05-10', '2013-05-10'),

-- Nhân viên 2 - Bếp phó
('NV002', 'NhanVien', N'Việt Nam', N'Trần Thị Bình', '1988-07-20', N'Trung sĩ', N'Bếp Phó', 
 3, N'Trường Cao đẳng Công nghệ Thực phẩm', '2012-06-20', '2008-03-10', 1, 
 '2018-12-15', '2016-12-15'),

-- Nhân viên 3 - Nhân viên bếp
('NV003', 'NhanVien', N'Việt Nam', N'Lê Văn Cường', '1990-11-08', N'Hạ sĩ', N'Nhân viên Bếp', 
 2, N'Trường Trung cấp Nghề Hà Nội', '2014-05-10', '2010-09-15', 2, 
 NULL, '2020-08-20'),

-- Nhân viên 4 - Nhân viên bếp
('NV004', 'NhanVien', N'Việt Nam', N'Phạm Thị Dung', '1992-01-25', N'Binh nhất', N'Nhân viên Bếp', 
 1, N'Trường THPT Chu Văn An', '2015-06-30', '2012-02-28', 3, 
 NULL, NULL),

-- Nhân viên 5 - Bếp phó
('NV005', 'NhanVien', N'Việt Nam', N'Hoàng Văn Em', '1987-09-12', N'Trung sĩ nhất', N'Bếp Phó', 
 4, N'Trường Đại học Nông Lâm', '2011-08-25', '2007-01-15', 1, 
 '2017-03-20', '2015-03-20');

-- THÊM DỮ LIỆU CHO HỌC VIÊN (LoaiNguoi = 'HocVien')
INSERT INTO Nguoi (
    MaNguoi, LoaiNguoi, QuocTich, HoTen, NgaySinh, CapBac, 
    MaTrinhDo, ChuyenNganh, KhoaHoc, ChucVu, NgayNhapHoc, NgayTotNghiep, 
    MaLoaiDangVien, NgayVaoDangChinhThuc, NgayVaoDangDuBi
) VALUES 
-- Học viên Campuchia 1
('HV001', 'HocVien', N'Campuchia', N'Som Raksa', '1995-04-10', N'Thiếu úy', 
 4, N'Kỹ thuật Cơ khí', N'K48', N'Lớp Trưởng', '2020-09-01', '2024-07-15', 
 2, NULL, '2022-05-15'),

-- Học viên Campuchia 2
('HV002', 'HocVien', N'Campuchia', N'Chea Sophea', '1996-08-22', N'Thiếu úy', 
 4, N'Công nghệ Thông tin', N'K48', N'Học Viên', '2020-09-01', '2024-07-15', 
 3, NULL, NULL),

-- Học viên Lào 1
('HV003', 'HocVien', N'Lào', N'Bounthong Sisavath', '1994-12-05', N'Trung úy', 
 4, N'Quân sự', N'K47', N'Lớp Trưởng', '2019-09-01', '2023-07-15', 
 1, '2021-10-01', '2019-10-01'),

-- Học viên Lào 2
('HV004', 'HocVien', N'Lào', N'Khamphone Vongsa', '1997-02-18', N'Thiếu úy', 
 4, N'Kinh tế Quốc phòng', N'K49', N'Học Viên', '2021-09-01', '2025-07-15', 
 2, NULL, '2023-03-10'),

-- Học viên Campuchia 3
('HV005', 'HocVien', N'Campuchia', N'Pich Vireak', '1995-11-30', N'Thiếu úy', 
 4, N'Logistics', N'K48', N'Học Viên', '2020-09-01', '2024-07-15', 
 3, NULL, NULL);

-- Thêm dữ liệu khen thưởng
INSERT INTO KhenThuong_Nguoi (MaKhenThuong, MaNguoi, LyDo, NgayKhenThuong) VALUES 
(1, 'NV001', N'Hoàn thành xuất sắc nhiệm vụ', '2024-01-15'),
(2, 'HV001', N'Đạt thành tích cao trong học tập', '2024-03-20'),
(1, 'NV002', N'Tích cực trong công việc', '2024-05-10'),
(1, 'HV003', N'Xuất sắc trong phong trào thi đua', '2024-02-28');

-- Thêm dữ liệu kỷ luật
INSERT INTO KyLuat_Nguoi (MaKyLuat, MaNguoi, LyDo, NgayKyLuat) VALUES 
(1, 'NV004', N'Vi phạm nội quy', '2024-04-15'),
(1, 'HV002', N'Đi học muộn nhiều lần', '2024-06-10');

-- Thêm dữ liệu phong trào thi đua
INSERT INTO NguoiDatDuocPTTD (MaPTTD, MaNguoi, GhiChu) VALUES 
(1, 'NV001', N'Đạt thành tích xuất sắc'),
(1, 'HV001', N'Học tập tốt, rèn luyện tốt'),
(1, 'NV002', N'Hoàn thành tốt nhiệm vụ'),
(2, 'HV003', N'Tuân thủ nghiêm an toàn lao động');

-- Thêm kết quả học tập cho học viên
INSERT INTO KetQuaHocTap (MaNguoi, NamHoc, DiemTrungBinh, GhiChu) VALUES 
('HV001', '2023-2024', 8.5, N'Học lực giỏi'),
('HV002', '2023-2024', 7.2, N'Học lực khá'),
('HV003', '2022-2023', 9.0, N'Học lực xuất sắc'),
('HV004', '2023-2024', 7.8, N'Học lực khá'),
('HV005', '2023-2024', 6.8, N'Học lực trung bình');

-- Thêm thống kê
INSERT INTO ThongKe (NamThongKe, TongHocVien, TongQuanNhan, TongLoaiDangVien, GhiChu) VALUES 
(2024, 5, 5, 10, N'Thống kê cuối năm 2024'),
(2023, 4, 5, 9, N'Thống kê cuối năm 2023');
