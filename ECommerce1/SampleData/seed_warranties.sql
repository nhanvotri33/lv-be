USE csdl_phone;
GO

IF NOT EXISTS (SELECT 1 FROM Warranties WHERE Code = 'BH-VIP-12M')
BEGIN
    INSERT INTO Warranties (Code, Name, Description, TermsHtml, DurationMonths, BasePrice, RequiresInspection, IsActive, CreatedAt, UpdatedAt)
    VALUES 
    (N'BH-VIP-12M', N'Gói Bảo Hành Rơi Vỡ - Vào Nước 12 Tháng VIP', N'Miễn phí sửa chữa 100% linh kiện chính hãng cho sự cố rơi vỡ, vào nước trong 12 tháng.', N'<p>Điều khoản gói bảo hành VIP 12 tháng: Hỗ trợ 100% chi phí linh kiện thay thế.</p>', 12, 490000.00, 0, 1, GETDATE(), GETDATE()),
    (N'BH-1DOI1-12M', N'Gói Bảo Hành 1 Đổi 1 Trong 12 Tháng', N'Đổi ngay máy mới tương đương nếu có lỗi từ nhà sản xuất hoặc sự cố hỏng hóc nặng trong 12 tháng.', N'<p>Điều khoản đổi mới: Đổi ngay trong 15 phút tại hệ thống cửa hàng.</p>', 12, 690000.00, 0, 1, GETDATE(), GETDATE()),
    (N'BH-MO-RONG-24M', N'Gói Bảo Hành Mở Rộng 24 Tháng Toàn Diện', N'Bảo hành toàn bộ phần cứng, nguồn, màn hình, pin trong 2 năm liên tục.', N'<p>Bảo hành mở rộng 24 tháng toàn diện.</p>', 24, 990000.00, 0, 1, GETDATE(), GETDATE());
END
GO
