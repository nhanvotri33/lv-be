USE csdl_phone;
GO

-- Add Brands
IF NOT EXISTS (SELECT 1 FROM Brands WHERE Name = 'SanDisk') INSERT INTO Brands (Name, Slug, Description, ImageUrl, IsActive, CreatedAt, BrandCode) VALUES ('SanDisk', 'sandisk', 'Thương hiệu SanDisk', '', 1, GETDATE(), 'SANDISK');
IF NOT EXISTS (SELECT 1 FROM Brands WHERE Name = 'Kingston') INSERT INTO Brands (Name, Slug, Description, ImageUrl, IsActive, CreatedAt, BrandCode) VALUES ('Kingston', 'kingston', 'Thương hiệu Kingston', '', 1, GETDATE(), 'KINGSTON');
IF NOT EXISTS (SELECT 1 FROM Brands WHERE Name = 'WD') INSERT INTO Brands (Name, Slug, Description, ImageUrl, IsActive, CreatedAt, BrandCode) VALUES ('WD', 'wd', 'Thương hiệu Western Digital', '', 1, GETDATE(), 'WD');

DECLARE @BrandId INT;
DECLARE @ProductId INT;

-- 12 Sạc dự phòng
SET @BrandId = 7;
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Sạc dự phòng Anker PowerCore 10000mAh', 'anker-powercore-10000', N'Mô tả sản phẩm', 500000, 0, 1, GETDATE(), GETDATE(), 12, '', '', '[]', 0, @BrandId, 0, 'PCODE-ANKERPWC-1', N'{"Dung lượng pin": "10000mAh", "Công suất": "12W", "Cổng sạc": "1 USB-A, 1 Micro USB"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 500000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'{"Màu sắc": "Đen"}', 1, 'SKU-ANKERPWC-1');

SET @BrandId = 3;
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Sạc dự phòng Xiaomi Redmi 20000mAh', 'xiaomi-redmi-20000', N'Mô tả sản phẩm', 450000, 0, 1, GETDATE(), GETDATE(), 12, '', '', '[]', 0, @BrandId, 0, 'PCODE-XIAOMI20-1', N'{"Dung lượng pin": "20000mAh", "Công suất": "18W", "Cổng sạc": "2 USB-A, 1 Type-C"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 450000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'{"Màu sắc": "Trắng"}', 1, 'SKU-XIAOMI20-1');

SET @BrandId = 10;
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Sạc dự phòng Baseus Bipow 15W 10000mAh', 'baseus-bipow-15w-10000', N'Mô tả sản phẩm', 300000, 0, 1, GETDATE(), GETDATE(), 12, '', '', '[]', 0, @BrandId, 0, 'PCODE-BASEUSBW-1', N'{"Dung lượng pin": "10000mAh", "Công suất": "15W", "Cổng sạc": "2 USB-A, 1 Type-C"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 300000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'{"Màu sắc": "Đen"}', 1, 'SKU-BASEUSBW-1');

-- 13 Cáp, sạc
SET @BrandId = 1;
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Củ sạc nhanh Apple 20W Type-C', 'apple-20w-type-c', N'Mô tả sản phẩm', 550000, 0, 1, GETDATE(), GETDATE(), 13, '', '', '[]', 0, @BrandId, 0, 'PCODE-APPLE20W-1', N'{"Công suất": "20W", "Cổng sạc": "1 Type-C", "Tương thích": "iPhone, iPad"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 550000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'[]', 1, 'SKU-APPLE20W-1');

SET @BrandId = 7;
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Cáp sạc Anker PowerLine III Type-C to Lightning', 'anker-powerline-iii', N'Mô tả sản phẩm', 350000, 0, 1, GETDATE(), GETDATE(), 13, '', '', '[]', 0, @BrandId, 0, 'PCODE-ANKERPWL-1', N'{"Chiều dài": "0.9m", "Công suất": "Tối đa 60W"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 350000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'{"Màu sắc": "Trắng"}', 1, 'SKU-ANKERPWL-1');

SET @BrandId = 2;
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Bộ sạc nhanh Samsung 25W Type-C', 'samsung-25w-type-c', N'Mô tả sản phẩm', 400000, 0, 1, GETDATE(), GETDATE(), 13, '', '', '[]', 0, @BrandId, 0, 'PCODE-SAMSUNG2-1', N'{"Công suất": "25W", "Cổng sạc": "1 Type-C"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 400000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'{"Màu sắc": "Đen"}', 1, 'SKU-SAMSUNG2-1');

-- 14 Ốp lưng điện thoại
SET @BrandId = 1;
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Ốp lưng iPhone 15 Pro Max Clear Case', 'ip15-pm-clear-case', N'Mô tả sản phẩm', 1200000, 0, 1, GETDATE(), GETDATE(), 14, '', '', '[]', 0, @BrandId, 0, 'PCODE-IP15PMCC-1', N'{"Chất liệu": "Polycarbonate", "Tính năng": "Hỗ trợ MagSafe"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 1200000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'{"Màu sắc": "Trong suốt"}', 1, 'SKU-IP15PMCC-1');

SET @BrandId = 2;
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Ốp lưng Samsung Galaxy S24 Ultra Silicone', 's24-ultra-silicone', N'Mô tả sản phẩm', 800000, 0, 1, GETDATE(), GETDATE(), 14, '', '', '[]', 0, @BrandId, 0, 'PCODE-S24USILI-1', N'{"Chất liệu": "Silicone", "Tính năng": "Chống sốc"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 800000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'{"Màu sắc": "Xanh dương"}', 1, 'SKU-S24USILI-1');

SET @BrandId = 10;
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Ốp lưng Baseus Wing Case cho iPhone 14', 'baseus-wing-ip14', N'Mô tả sản phẩm', 150000, 0, 1, GETDATE(), GETDATE(), 14, '', '', '[]', 0, @BrandId, 0, 'PCODE-BSWINGIP-1', N'{"Chất liệu": "Nhựa PP", "Tính năng": "Siêu mỏng 0.4mm"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 150000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'{"Màu sắc": "Đen mờ"}', 1, 'SKU-BSWINGIP-1');

-- 15 Ốp lưng máy tính bảng
SET @BrandId = 1;
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Bao da iPad Pro 11 inch Smart Folio', 'ipad-pro-11-folio', N'Mô tả sản phẩm', 2000000, 0, 1, GETDATE(), GETDATE(), 15, '', '', '[]', 0, @BrandId, 0, 'PCODE-IP11FOLI-1', N'{"Chất liệu": "Polyurethane", "Tính năng": "Đóng mở màn hình tự động"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 2000000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'{"Màu sắc": "Trắng"}', 1, 'SKU-IP11FOLI-1');

SET @BrandId = 2;
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Bao da Samsung Galaxy Tab S9 Smart Cover', 'tab-s9-smart-cover', N'Mô tả sản phẩm', 1500000, 0, 1, GETDATE(), GETDATE(), 15, '', '', '[]', 0, @BrandId, 0, 'PCODE-TABS9COV-1', N'{"Chất liệu": "Da PU", "Tính năng": "Kháng khuẩn"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 1500000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'{"Màu sắc": "Đen"}', 1, 'SKU-TABS9COV-1');

SET @BrandId = 10;
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Bao da Baseus Safattach cho iPad Air 5', 'baseus-ipad-air5', N'Mô tả sản phẩm', 450000, 0, 1, GETDATE(), GETDATE(), 15, '', '', '[]', 0, @BrandId, 0, 'PCODE-BSIPAIR5-1', N'{"Chất liệu": "Da nhân tạo", "Tính năng": "Gắn từ tính"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 450000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'{"Màu sắc": "Xanh lá"}', 1, 'SKU-BSIPAIR5-1');

-- 16 Dán màn hình
SET @BrandId = 10;
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Kính cường lực Baseus 0.3mm cho iPhone 15', 'baseus-glass-ip15', N'Mô tả sản phẩm', 150000, 0, 1, GETDATE(), GETDATE(), 16, '', '', '[]', 0, @BrandId, 0, 'PCODE-BSGLAIP1-1', N'{"Chất liệu": "Kính cường lực", "Độ dày": "0.3mm"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 150000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'[]', 1, 'SKU-BSGLAIP1-1');

SET @BrandId = 7;
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Dán màn hình Anker GlassGuard cho iPhone 14 Pro Max', 'anker-glass-ip14', N'Mô tả sản phẩm', 250000, 0, 1, GETDATE(), GETDATE(), 16, '', '', '[]', 0, @BrandId, 0, 'PCODE-AKGLAIP1-1', N'{"Độ cứng": "9H", "Tính năng": "Chống xước"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 250000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'[]', 1, 'SKU-AKGLAIP1-1');

SET @BrandId = 2;
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Dán màn hình Samsung Galaxy S23 Ultra', 'ss-glass-s23', N'Mô tả sản phẩm', 350000, 0, 1, GETDATE(), GETDATE(), 16, '', '', '[]', 0, @BrandId, 0, 'PCODE-SSGLAS23-1', N'{"Chất liệu": "Film PET", "Tính năng": "Chống chói"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 350000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'[]', 1, 'SKU-SSGLAS23-1');

-- 17 Dây đeo điện thoại
SET @BrandId = 10;
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Dây đeo điện thoại Baseus Lanyard', 'baseus-lanyard-1', N'Mô tả sản phẩm', 90000, 0, 1, GETDATE(), GETDATE(), 17, '', '', '[]', 0, @BrandId, 0, 'PCODE-BSLANYAR-1', N'{"Chất liệu": "Nylon", "Độ dài": "Tùy chỉnh"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 90000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'{"Màu sắc": "Đen"}', 1, 'SKU-BSLANYAR-1');

SET @BrandId = 10;
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Dây đeo cổ nhẫn Ringke Lanyard', 'ringke-lanyard-1', N'Mô tả sản phẩm', 120000, 0, 1, GETDATE(), GETDATE(), 17, '', '', '[]', 0, @BrandId, 0, 'PCODE-RGLANYAR-1', N'{"Chất liệu": "Vải dù", "Độ dài": "40cm"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 120000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'{"Màu sắc": "Xanh dương"}', 1, 'SKU-RGLANYAR-1');

SET @BrandId = 10;
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Dây đeo cổ tay Spigen Wrist Strap', 'spigen-wrist-1', N'Mô tả sản phẩm', 150000, 0, 1, GETDATE(), GETDATE(), 17, '', '', '[]', 0, @BrandId, 0, 'PCODE-SPWRIST1-1', N'{"Chất liệu": "Dacron dệt kim", "Độ dài": "20cm"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 150000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'{"Màu sắc": "Xám"}', 1, 'SKU-SPWRIST1-1');

-- 18 Hộp đựng tai nghe
SET @BrandId = 1;
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Hộp đựng AirPods Pro silicon', 'airpods-pro-case-1', N'Mô tả sản phẩm', 100000, 0, 1, GETDATE(), GETDATE(), 18, '', '', '[]', 0, @BrandId, 0, 'PCODE-APCASE1-1', N'{"Chất liệu": "Silicone", "Tương thích": "AirPods Pro"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 100000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'{"Màu sắc": "Cam"}', 1, 'SKU-APCASE1-1');

SET @BrandId = 2;
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Ốp lưng Galaxy Buds2 Pro trong suốt', 'buds2-pro-case-1', N'Mô tả sản phẩm', 150000, 0, 1, GETDATE(), GETDATE(), 18, '', '', '[]', 0, @BrandId, 0, 'PCODE-SSCASE1-1', N'{"Chất liệu": "Nhựa PC", "Tương thích": "Galaxy Buds2 Pro"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 150000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'[]', 1, 'SKU-SSCASE1-1');

SET @BrandId = 10;
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Hộp đựng Baseus cho tai nghe TWS', 'baseus-tws-pouch', N'Mô tả sản phẩm', 80000, 0, 1, GETDATE(), GETDATE(), 18, '', '', '[]', 0, @BrandId, 0, 'PCODE-BSTWS1-1', N'{"Chất liệu": "Vải nỉ EVA", "Tính năng": "Kéo khóa"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 80000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'{"Màu sắc": "Đen"}', 1, 'SKU-BSTWS1-1');

-- 19 Giá đỡ điện thoại
SET @BrandId = 10;
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Giá đỡ điện thoại Baseus Desktop Stand', 'baseus-stand-1', N'Mô tả sản phẩm', 200000, 0, 1, GETDATE(), GETDATE(), 19, '', '', '[]', 0, @BrandId, 0, 'PCODE-BSSTAND1-1', N'{"Chất liệu": "Hợp kim nhôm", "Khả năng xoay": "Lên xuống 35 độ"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 200000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'{"Màu sắc": "Bạc"}', 1, 'SKU-BSSTAND1-1');

SET @BrandId = 7;
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Giá đỡ Anker Magnetic Car Mount', 'anker-mount-1', N'Mô tả sản phẩm', 450000, 0, 1, GETDATE(), GETDATE(), 19, '', '', '[]', 0, @BrandId, 0, 'PCODE-AKMOUNT1-1', N'{"Chất liệu": "Nhựa ABS", "Khả năng xoay": "360 độ"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 450000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'{"Màu sắc": "Đen"}', 1, 'SKU-AKMOUNT1-1');

SET @BrandId = 3;
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Gậy tự sướng Xiaomi Tripod', 'xiaomi-tripod-1', N'Mô tả sản phẩm', 300000, 0, 1, GETDATE(), GETDATE(), 19, '', '', '[]', 0, @BrandId, 0, 'PCODE-XMTRIPOD-1', N'{"Chất liệu": "Nhựa, Nhôm", "Kết nối": "Bluetooth"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 300000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'{"Màu sắc": "Đen"}', 1, 'SKU-XMTRIPOD-1');

-- 20 Thẻ nhớ
SELECT @BrandId = Id FROM Brands WHERE Name = 'SanDisk';
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Thẻ nhớ MicroSD SanDisk Extreme Pro 128GB', 'sandisk-128gb-1', N'Mô tả sản phẩm', 600000, 0, 1, GETDATE(), GETDATE(), 20, '', '', '[]', 0, @BrandId, 0, 'PCODE-SD128GB-1', N'{"Dung lượng": "128GB", "Tốc độ đọc": "200MB/s"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 600000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'[]', 1, 'SKU-SD128GB-1');

SELECT @BrandId = Id FROM Brands WHERE Name = 'Kingston';
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Thẻ nhớ MicroSD Kingston Canvas Go 64GB', 'kingston-64gb-1', N'Mô tả sản phẩm', 300000, 0, 1, GETDATE(), GETDATE(), 20, '', '', '[]', 0, @BrandId, 0, 'PCODE-KS64GB-1', N'{"Dung lượng": "64GB", "Tốc độ đọc": "170MB/s"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 300000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'[]', 1, 'SKU-KS64GB-1');

SET @BrandId = 2;
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Thẻ nhớ MicroSD Samsung EVO Plus 256GB', 'samsung-256gb-1', N'Mô tả sản phẩm', 750000, 0, 1, GETDATE(), GETDATE(), 20, '', '', '[]', 0, @BrandId, 0, 'PCODE-SS256GB-1', N'{"Dung lượng": "256GB", "Tốc độ đọc": "130MB/s"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 750000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'[]', 1, 'SKU-SS256GB-1');

-- 21 USB
SELECT @BrandId = Id FROM Brands WHERE Name = 'SanDisk';
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'USB Flash SanDisk Dual Drive 64GB', 'sandisk-usb-64gb-1', N'Mô tả sản phẩm', 250000, 0, 1, GETDATE(), GETDATE(), 21, '', '', '[]', 0, @BrandId, 0, 'PCODE-SDUSB64-1', N'{"Dung lượng": "64GB", "Kết nối": "Type-C và Type-A"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 250000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'{"Màu sắc": "Đen"}', 1, 'SKU-SDUSB64-1');

SELECT @BrandId = Id FROM Brands WHERE Name = 'Kingston';
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'USB Kingston DataTraveler 32GB', 'kingston-usb-32gb-1', N'Mô tả sản phẩm', 120000, 0, 1, GETDATE(), GETDATE(), 21, '', '', '[]', 0, @BrandId, 0, 'PCODE-KSUSB32-1', N'{"Dung lượng": "32GB", "Kết nối": "Type-A USB 3.2"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 120000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'{"Màu sắc": "Đen"}', 1, 'SKU-KSUSB32-1');

SET @BrandId = 2;
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'USB Flash Samsung FIT Plus 128GB', 'samsung-usb-128gb-1', N'Mô tả sản phẩm', 500000, 0, 1, GETDATE(), GETDATE(), 21, '', '', '[]', 0, @BrandId, 0, 'PCODE-SSUSB128-1', N'{"Dung lượng": "128GB", "Tốc độ đọc": "400MB/s"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 500000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'[]', 1, 'SKU-SSUSB128-1');

-- 22 Ổ cứng di động
SET @BrandId = 2;
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Ổ cứng SSD Samsung T7 Touch 500GB', 'samsung-t7-500gb-1', N'Mô tả sản phẩm', 2200000, 0, 1, GETDATE(), GETDATE(), 22, '', '', '[]', 0, @BrandId, 0, 'PCODE-SST7500-1', N'{"Dung lượng": "500GB", "Tốc độ đọc": "1050MB/s"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 2200000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'{"Màu sắc": "Bạc"}', 1, 'SKU-SST7500-1');

SELECT @BrandId = Id FROM Brands WHERE Name = 'SanDisk';
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Ổ cứng SSD SanDisk Extreme 1TB', 'sandisk-ssd-1tb-1', N'Mô tả sản phẩm', 3500000, 0, 1, GETDATE(), GETDATE(), 22, '', '', '[]', 0, @BrandId, 0, 'PCODE-SDSSD1TB-1', N'{"Dung lượng": "1TB", "Tốc độ đọc": "1050MB/s"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 3500000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'[]', 1, 'SKU-SDSSD1TB-1');

SELECT @BrandId = Id FROM Brands WHERE Name = 'WD';
INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (N'Ổ cứng HDD WD My Passport 2TB', 'wd-hdd-2tb-1', N'Mô tả sản phẩm', 1800000, 0, 1, GETDATE(), GETDATE(), 22, '', '', '[]', 0, @BrandId, 0, 'PCODE-WDHDD2TB-1', N'{"Dung lượng": "2TB", "Kết nối": "USB 3.2 Gen 1"}');
SET @ProductId = SCOPE_IDENTITY();
INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku) VALUES (N'Mặc định', 1800000, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'{"Màu sắc": "Đen"}', 1, 'SKU-WDHDD2TB-1');
