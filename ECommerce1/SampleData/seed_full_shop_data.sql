-- ============================================================================
-- FULL SHOP SEED DATA SCRIPT FOR E-COMMERCE DATABASE (csdl_phone)
-- Generated automatically with high-quality real tech products, brands, orders
-- ============================================================================
USE csdl_phone;
GO

SET NOCOUNT ON;
BEGIN TRANSACTION;
BEGIN TRY

PRINT N'---> Clearing existing sample data in correct dependency order...';
DELETE FROM Payments;
DELETE FROM ShippingInfos;
DELETE FROM PromotionUsages;
DELETE FROM OrderItems;
DELETE FROM Orders;
DELETE FROM CartItems;
DELETE FROM Carts;
DELETE FROM CustomerDevices;
DELETE FROM RefreshTokens;
DELETE FROM AuditLogs;
DELETE FROM Reviews;
DELETE FROM Stock;
DELETE FROM InventoryTransactions;
DELETE FROM CampaignMainProductRules;
DELETE FROM CampaignAddonProductRules;
DELETE FROM WarrantyPackageRules;
DELETE FROM Warranties;
DELETE FROM ProductVariants;
DELETE FROM Products;
DELETE FROM CategoryBrandDefaults;
DELETE FROM Brands;
DELETE FROM Categories;
DELETE FROM Banners;
DELETE FROM Promotions;
DELETE FROM PromotionCampaigns;

-- ==========================================
-- 1. SEED CATEGORIES
-- ==========================================
PRINT N'---> Seeding Categories...';
SET IDENTITY_INSERT Categories ON;
INSERT INTO Categories (Id, Name, CategoryCode, Slug, IsActive, CreatedAt, UpdatedAt, Description, IconUrl, MetaTitle, MetaDescription, ParentId)
VALUES (1, N'Điện thoại', 'PHONE', 'dien-thoai', 1, GETDATE(), GETDATE(), N'Điện thoại thông minh chính hãng Apple, Samsung, Xiaomi, OPPO...', 'smartphone', N'Điện thoại Chính Hãng', N'Mua Điện thoại chính hãng giá tốt nhất', NULL);
INSERT INTO Categories (Id, Name, CategoryCode, Slug, IsActive, CreatedAt, UpdatedAt, Description, IconUrl, MetaTitle, MetaDescription, ParentId)
VALUES (2, N'Laptop & Máy tính', 'LAPTOP', 'laptop', 1, GETDATE(), GETDATE(), N'Laptop văn phòng, laptop gaming, MacBook chính hãng', 'laptop', N'Laptop & Máy tính Chính Hãng', N'Mua Laptop & Máy tính chính hãng giá tốt nhất', NULL);
INSERT INTO Categories (Id, Name, CategoryCode, Slug, IsActive, CreatedAt, UpdatedAt, Description, IconUrl, MetaTitle, MetaDescription, ParentId)
VALUES (3, N'Máy tính bảng', 'TABLET', 'may-tinh-bang', 1, GETDATE(), GETDATE(), N'iPad, Samsung Galaxy Tab, Xiaomi Pad', 'tablet', N'Máy tính bảng Chính Hãng', N'Mua Máy tính bảng chính hãng giá tốt nhất', NULL);
INSERT INTO Categories (Id, Name, CategoryCode, Slug, IsActive, CreatedAt, UpdatedAt, Description, IconUrl, MetaTitle, MetaDescription, ParentId)
VALUES (4, N'Đồng hồ thông minh', 'WATCH', 'dong-ho-thong-minh', 1, GETDATE(), GETDATE(), N'Apple Watch, Galaxy Watch, Garmin', 'watch', N'Đồng hồ thông minh Chính Hãng', N'Mua Đồng hồ thông minh chính hãng giá tốt nhất', NULL);
INSERT INTO Categories (Id, Name, CategoryCode, Slug, IsActive, CreatedAt, UpdatedAt, Description, IconUrl, MetaTitle, MetaDescription, ParentId)
VALUES (5, N'Tai nghe & Âm thanh', 'AUDIO', 'tai-nghe-am-thanh', 1, GETDATE(), GETDATE(), N'Tai nghe Bluetooth, Loa Bluetooth, Tai nghe chụp tai', 'headphones', N'Tai nghe & Âm thanh Chính Hãng', N'Mua Tai nghe & Âm thanh chính hãng giá tốt nhất', NULL);
INSERT INTO Categories (Id, Name, CategoryCode, Slug, IsActive, CreatedAt, UpdatedAt, Description, IconUrl, MetaTitle, MetaDescription, ParentId)
VALUES (6, N'Phụ kiện điện thoại', 'PHONE_ACC', 'phu-kien-dien-thoai', 1, GETDATE(), GETDATE(), N'Sạc dự phòng, Cáp sạc, Ốp lưng, Kính cường lực', 'cable', N'Phụ kiện điện thoại Chính Hãng', N'Mua Phụ kiện điện thoại chính hãng giá tốt nhất', NULL);
INSERT INTO Categories (Id, Name, CategoryCode, Slug, IsActive, CreatedAt, UpdatedAt, Description, IconUrl, MetaTitle, MetaDescription, ParentId)
VALUES (7, N'Phụ kiện máy tính', 'PC_ACC', 'phu-kien-may-tinh', 1, GETDATE(), GETDATE(), N'Chuột, Bàn phím, Thẻ nhớ, Ổ cứng di động', 'mouse', N'Phụ kiện máy tính Chính Hãng', N'Mua Phụ kiện máy tính chính hãng giá tốt nhất', NULL);
INSERT INTO Categories (Id, Name, CategoryCode, Slug, IsActive, CreatedAt, UpdatedAt, Description, IconUrl, MetaTitle, MetaDescription, ParentId)
VALUES (10, N'iPhone', 'IPHONE', 'iphone', 1, GETDATE(), GETDATE(), N'Điện thoại iPhone chính hãng Apple', 'smartphone', N'iPhone Chính Hãng', N'Mua iPhone chính hãng giá tốt nhất', 1);
INSERT INTO Categories (Id, Name, CategoryCode, Slug, IsActive, CreatedAt, UpdatedAt, Description, IconUrl, MetaTitle, MetaDescription, ParentId)
VALUES (11, N'Samsung Galaxy', 'SAMSUNG_PHONE', 'samsung-galaxy', 1, GETDATE(), GETDATE(), N'Điện thoại Samsung Galaxy S, Fold, Z Flip, Series A', 'smartphone', N'Samsung Galaxy Chính Hãng', N'Mua Samsung Galaxy chính hãng giá tốt nhất', 1);
INSERT INTO Categories (Id, Name, CategoryCode, Slug, IsActive, CreatedAt, UpdatedAt, Description, IconUrl, MetaTitle, MetaDescription, ParentId)
VALUES (12, N'Xiaomi Phone', 'XIAOMI_PHONE', 'xiaomi-phone', 1, GETDATE(), GETDATE(), N'Điện thoại Xiaomi, Redmi, POCO', 'smartphone', N'Xiaomi Phone Chính Hãng', N'Mua Xiaomi Phone chính hãng giá tốt nhất', 1);
INSERT INTO Categories (Id, Name, CategoryCode, Slug, IsActive, CreatedAt, UpdatedAt, Description, IconUrl, MetaTitle, MetaDescription, ParentId)
VALUES (13, N'OPPO Phone', 'OPPO_PHONE', 'oppo-phone', 1, GETDATE(), GETDATE(), N'Điện thoại OPPO Find, Reno, Series A', 'smartphone', N'OPPO Phone Chính Hãng', N'Mua OPPO Phone chính hãng giá tốt nhất', 1);
INSERT INTO Categories (Id, Name, CategoryCode, Slug, IsActive, CreatedAt, UpdatedAt, Description, IconUrl, MetaTitle, MetaDescription, ParentId)
VALUES (14, N'MacBook', 'MACBOOK', 'macbook', 1, GETDATE(), GETDATE(), N'MacBook Air, MacBook Pro M1 M2 M3', 'laptop', N'MacBook Chính Hãng', N'Mua MacBook chính hãng giá tốt nhất', 2);
INSERT INTO Categories (Id, Name, CategoryCode, Slug, IsActive, CreatedAt, UpdatedAt, Description, IconUrl, MetaTitle, MetaDescription, ParentId)
VALUES (15, N'Laptop Gaming', 'LAPTOP_GAMING', 'laptop-gaming', 1, GETDATE(), GETDATE(), N'Laptop cấu hình cao chơi game đồ họa', 'laptop', N'Laptop Gaming Chính Hãng', N'Mua Laptop Gaming chính hãng giá tốt nhất', 2);
INSERT INTO Categories (Id, Name, CategoryCode, Slug, IsActive, CreatedAt, UpdatedAt, Description, IconUrl, MetaTitle, MetaDescription, ParentId)
VALUES (16, N'Laptop Văn Phòng', 'LAPTOP_OFFICE', 'laptop-van-phong', 1, GETDATE(), GETDATE(), N'Laptop mỏng nhẹ, pin trâu cho học sinh sinh viên văn phòng', 'laptop', N'Laptop Văn Phòng Chính Hãng', N'Mua Laptop Văn Phòng chính hãng giá tốt nhất', 2);
INSERT INTO Categories (Id, Name, CategoryCode, Slug, IsActive, CreatedAt, UpdatedAt, Description, IconUrl, MetaTitle, MetaDescription, ParentId)
VALUES (17, N'iPad', 'IPAD', 'ipad', 1, GETDATE(), GETDATE(), N'iPad Pro, iPad Air, iPad Gen, iPad Mini', 'tablet', N'iPad Chính Hãng', N'Mua iPad chính hãng giá tốt nhất', 3);
INSERT INTO Categories (Id, Name, CategoryCode, Slug, IsActive, CreatedAt, UpdatedAt, Description, IconUrl, MetaTitle, MetaDescription, ParentId)
VALUES (18, N'Samsung Tab', 'SAMSUNG_TAB', 'samsung-tab', 1, GETDATE(), GETDATE(), N'Samsung Galaxy Tab S, Tab A', 'tablet', N'Samsung Tab Chính Hãng', N'Mua Samsung Tab chính hãng giá tốt nhất', 3);
INSERT INTO Categories (Id, Name, CategoryCode, Slug, IsActive, CreatedAt, UpdatedAt, Description, IconUrl, MetaTitle, MetaDescription, ParentId)
VALUES (19, N'Apple Watch', 'APPLE_WATCH', 'apple-watch', 1, GETDATE(), GETDATE(), N'Apple Watch Series, Apple Watch Ultra, SE', 'watch', N'Apple Watch Chính Hãng', N'Mua Apple Watch chính hãng giá tốt nhất', 4);
INSERT INTO Categories (Id, Name, CategoryCode, Slug, IsActive, CreatedAt, UpdatedAt, Description, IconUrl, MetaTitle, MetaDescription, ParentId)
VALUES (20, N'Galaxy Watch', 'GALAXY_WATCH', 'galaxy-watch', 1, GETDATE(), GETDATE(), N'Samsung Galaxy Watch Classic, Galaxy Watch FE', 'watch', N'Galaxy Watch Chính Hãng', N'Mua Galaxy Watch chính hãng giá tốt nhất', 4);
INSERT INTO Categories (Id, Name, CategoryCode, Slug, IsActive, CreatedAt, UpdatedAt, Description, IconUrl, MetaTitle, MetaDescription, ParentId)
VALUES (21, N'Tai nghe Bluetooth', 'TWS_EARPHONES', 'tai-nghe-bluetooth', 1, GETDATE(), GETDATE(), N'Tai nghe không dây True Wireless', 'headphones', N'Tai nghe Bluetooth Chính Hãng', N'Mua Tai nghe Bluetooth chính hãng giá tốt nhất', 5);
INSERT INTO Categories (Id, Name, CategoryCode, Slug, IsActive, CreatedAt, UpdatedAt, Description, IconUrl, MetaTitle, MetaDescription, ParentId)
VALUES (22, N'Loa Bluetooth', 'SPEAKER_BT', 'loa-bluetooth', 1, GETDATE(), GETDATE(), N'Loa di động chống nước âm thanh hay', 'speaker', N'Loa Bluetooth Chính Hãng', N'Mua Loa Bluetooth chính hãng giá tốt nhất', 5);
INSERT INTO Categories (Id, Name, CategoryCode, Slug, IsActive, CreatedAt, UpdatedAt, Description, IconUrl, MetaTitle, MetaDescription, ParentId)
VALUES (23, N'Sạc dự phòng', 'POWERBANK', 'sac-du-phong', 1, GETDATE(), GETDATE(), N'Pin sạc dự phòng sạc nhanh 10000mAh, 20000mAh, 100W', 'battery-charging', N'Sạc dự phòng Chính Hãng', N'Mua Sạc dự phòng chính hãng giá tốt nhất', 6);
INSERT INTO Categories (Id, Name, CategoryCode, Slug, IsActive, CreatedAt, UpdatedAt, Description, IconUrl, MetaTitle, MetaDescription, ParentId)
VALUES (24, N'Cáp sạc & Củ sạc', 'CHARGER_CABLE', 'cap-cu-sac', 1, GETDATE(), GETDATE(), N'Bộ sạc nhanh 20W, 30W, 65W, 100W Anker, Apple, Samsung', 'zap', N'Cáp sạc & Củ sạc Chính Hãng', N'Mua Cáp sạc & Củ sạc chính hãng giá tốt nhất', 6);
INSERT INTO Categories (Id, Name, CategoryCode, Slug, IsActive, CreatedAt, UpdatedAt, Description, IconUrl, MetaTitle, MetaDescription, ParentId)
VALUES (25, N'Ốp lưng & Kính cường lực', 'CASES_GLASS', 'op-lung-kinh', 1, GETDATE(), GETDATE(), N'Ốp lưng chống sốc MagSafe, kính cường lực 9H', 'shield', N'Ốp lưng & Kính cường lực Chính Hãng', N'Mua Ốp lưng & Kính cường lực chính hãng giá tốt nhất', 6);
INSERT INTO Categories (Id, Name, CategoryCode, Slug, IsActive, CreatedAt, UpdatedAt, Description, IconUrl, MetaTitle, MetaDescription, ParentId)
VALUES (26, N'Chuột & Bàn phím', 'MOUSE_KEYBOARD', 'chuot-ban-phim', 1, GETDATE(), GETDATE(), N'Chuột không dây, bàn phím cơ Logitech, ASUS', 'mouse', N'Chuột & Bàn phím Chính Hãng', N'Mua Chuột & Bàn phím chính hãng giá tốt nhất', 7);
INSERT INTO Categories (Id, Name, CategoryCode, Slug, IsActive, CreatedAt, UpdatedAt, Description, IconUrl, MetaTitle, MetaDescription, ParentId)
VALUES (27, N'Thẻ nhớ & Ổ cứng', 'STORAGE_MEDIA', 'the-nho-o-cung', 1, GETDATE(), GETDATE(), N'SSD di động, Thẻ nhớ MicroSD SanDisk, Kingston, Samsung', 'hard-drive', N'Thẻ nhớ & Ổ cứng Chính Hãng', N'Mua Thẻ nhớ & Ổ cứng chính hãng giá tốt nhất', 7);
SET IDENTITY_INSERT Categories OFF;

-- ==========================================
-- 2. SEED BRANDS
-- ==========================================
PRINT N'---> Seeding Brands...';
SET IDENTITY_INSERT Brands ON;
INSERT INTO Brands (Id, Name, BrandCode, Slug, Description, ImageUrl, IsActive, CreatedAt)
VALUES (1, N'Apple', 'APPLE', 'apple', N'Thương hiệu công nghệ hàng đầu thế giới từ Mỹ', 'https://images.unsplash.com/photo-1611186871348-b1ce696e52c9?q=80&w=200&auto=format&fit=crop', 1, GETDATE());
INSERT INTO Brands (Id, Name, BrandCode, Slug, Description, ImageUrl, IsActive, CreatedAt)
VALUES (2, N'Samsung', 'SAMSUNG', 'samsung', N'Tập đoàn điện tử công nghệ số 1 Hàn Quốc', 'https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?q=80&w=200&auto=format&fit=crop', 1, GETDATE());
INSERT INTO Brands (Id, Name, BrandCode, Slug, Description, ImageUrl, IsActive, CreatedAt)
VALUES (3, N'Xiaomi', 'XIAOMI', 'xiaomi', N'Thương hiệu thiết bị thông minh sáng tạo', 'https://images.unsplash.com/photo-1598327105666-5b89351aff97?q=80&w=200&auto=format&fit=crop', 1, GETDATE());
INSERT INTO Brands (Id, Name, BrandCode, Slug, Description, ImageUrl, IsActive, CreatedAt)
VALUES (4, N'ASUS', 'ASUS', 'asus', N'Thương hiệu máy tính & phần cứng Republic of Gamers', 'https://images.unsplash.com/photo-1603302576837-37561b2e2302?q=80&w=200&auto=format&fit=crop', 1, GETDATE());
INSERT INTO Brands (Id, Name, BrandCode, Slug, Description, ImageUrl, IsActive, CreatedAt)
VALUES (5, N'Dell', 'DELL', 'dell', N'Hãng sản xuất máy tính xách tay & máy trạm bền bỉ', 'https://images.unsplash.com/photo-1593642632823-8f785ba67e45?q=80&w=200&auto=format&fit=crop', 1, GETDATE());
INSERT INTO Brands (Id, Name, BrandCode, Slug, Description, ImageUrl, IsActive, CreatedAt)
VALUES (6, N'HP', 'HP', 'hp', N'Máy tính & thiết bị văn phòng chuyên nghiệp', 'https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?q=80&w=200&auto=format&fit=crop', 1, GETDATE());
INSERT INTO Brands (Id, Name, BrandCode, Slug, Description, ImageUrl, IsActive, CreatedAt)
VALUES (7, N'Lenovo', 'LENOVO', 'lenovo', N'Dòng sản phẩm ThinkPad huyền thoại & Legion Gaming', 'https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?q=80&w=200&auto=format&fit=crop', 1, GETDATE());
INSERT INTO Brands (Id, Name, BrandCode, Slug, Description, ImageUrl, IsActive, CreatedAt)
VALUES (8, N'OPPO', 'OPPO', 'oppo', N'Chuyên gia chụp ảnh chân thực & smartphone thời trang', 'https://images.unsplash.com/photo-1546054454-aa26e2b734c7?q=80&w=200&auto=format&fit=crop', 1, GETDATE());
INSERT INTO Brands (Id, Name, BrandCode, Slug, Description, ImageUrl, IsActive, CreatedAt)
VALUES (9, N'Sony', 'SONY', 'sony', N'Đỉnh cao âm thanh chống noise & máy ảnh cao cấp', 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?q=80&w=200&auto=format&fit=crop', 1, GETDATE());
INSERT INTO Brands (Id, Name, BrandCode, Slug, Description, ImageUrl, IsActive, CreatedAt)
VALUES (10, N'Anker', 'ANKER', 'anker', N'Thương hiệu phụ kiện sạc & pin sạc dự phòng số 1 thế giới', 'https://images.unsplash.com/photo-1609592424089-980f55c5df38?q=80&w=200&auto=format&fit=crop', 1, GETDATE());
INSERT INTO Brands (Id, Name, BrandCode, Slug, Description, ImageUrl, IsActive, CreatedAt)
VALUES (11, N'Baseus', 'BASEUS', 'baseus', N'Phụ kiện công nghệ thông minh, thiết kế tinh tế', 'https://images.unsplash.com/photo-1622445268465-840246e47683?q=80&w=200&auto=format&fit=crop', 1, GETDATE());
INSERT INTO Brands (Id, Name, BrandCode, Slug, Description, ImageUrl, IsActive, CreatedAt)
VALUES (12, N'JBL', 'JBL', 'jbl', N'Thương hiệu loa & tai nghe âm trầm sống động Harman', 'https://images.unsplash.com/photo-1545454675-3531b543be5d?q=80&w=200&auto=format&fit=crop', 1, GETDATE());
INSERT INTO Brands (Id, Name, BrandCode, Slug, Description, ImageUrl, IsActive, CreatedAt)
VALUES (13, N'Garmin', 'GARMIN', 'garmin', N'Đồng hồ thông minh định vị GPS cho thể thao đỉnh cao', 'https://images.unsplash.com/photo-1579586337278-3befd40fd17a?q=80&w=200&auto=format&fit=crop', 1, GETDATE());
INSERT INTO Brands (Id, Name, BrandCode, Slug, Description, ImageUrl, IsActive, CreatedAt)
VALUES (14, N'SanDisk', 'SANDISK', 'sandisk', N'Thẻ nhớ, USB & ổ cứng SSD lưu trữ tốc độ cao', 'https://images.unsplash.com/photo-1597872200969-2b65d56bd16b?q=80&w=200&auto=format&fit=crop', 1, GETDATE());
INSERT INTO Brands (Id, Name, BrandCode, Slug, Description, ImageUrl, IsActive, CreatedAt)
VALUES (15, N'Kingston', 'KINGSTON', 'kingston', N'Bộ nhớ RAM, USB & SSD lưu trữ dữ liệu an toàn', 'https://images.unsplash.com/photo-1597872200969-2b65d56bd16b?q=80&w=200&auto=format&fit=crop', 1, GETDATE());
INSERT INTO Brands (Id, Name, BrandCode, Slug, Description, ImageUrl, IsActive, CreatedAt)
VALUES (16, N'Logitech', 'LOGITECH', 'logitech', N'Chuột, bàn phím & thiết bị ngoại vi hàng đầu', 'https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?q=80&w=200&auto=format&fit=crop', 1, GETDATE());
INSERT INTO Brands (Id, Name, BrandCode, Slug, Description, ImageUrl, IsActive, CreatedAt)
VALUES (17, N'Spigen', 'SPIGEN', 'spigen', N'Ốp lưng & phụ kiện bảo vệ điện thoại cao cấp từ Mỹ', 'https://images.unsplash.com/photo-1601784551446-20c9e07cdbdb?q=80&w=200&auto=format&fit=crop', 1, GETDATE());
INSERT INTO Brands (Id, Name, BrandCode, Slug, Description, ImageUrl, IsActive, CreatedAt)
VALUES (18, N'Marshall', 'MARSHALL', 'marshall', N'Hãng âm thanh phong cách cổ điển Rock & Roll', 'https://images.unsplash.com/photo-1583394838336-acd977736f90?q=80&w=200&auto=format&fit=crop', 1, GETDATE());
SET IDENTITY_INSERT Brands OFF;

-- ==========================================
-- 3. SEED CATEGORY-BRAND RELATIONSHIPS
-- ==========================================
PRINT N'---> Seeding CategoryBrandDefaults...';
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (1, 1, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (1, 2, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (1, 3, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (1, 4, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (1, 5, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (1, 6, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (1, 7, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (1, 8, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (1, 9, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (1, 10, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (1, 11, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (1, 12, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (1, 13, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (1, 14, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (1, 15, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (1, 16, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (1, 17, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (1, 18, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (2, 1, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (2, 2, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (2, 3, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (2, 4, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (2, 5, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (2, 6, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (2, 7, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (2, 8, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (2, 9, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (2, 10, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (2, 11, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (2, 12, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (2, 13, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (2, 14, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (2, 15, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (2, 16, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (2, 17, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (2, 18, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (3, 1, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (3, 2, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (3, 3, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (3, 4, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (3, 5, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (3, 6, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (3, 7, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (3, 8, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (3, 9, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (3, 10, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (3, 11, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (3, 12, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (3, 13, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (3, 14, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (3, 15, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (3, 16, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (3, 17, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (3, 18, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (4, 1, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (4, 2, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (4, 3, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (4, 4, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (4, 5, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (4, 6, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (4, 7, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (4, 8, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (4, 9, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (4, 10, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (4, 11, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (4, 12, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (4, 13, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (4, 14, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (4, 15, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (4, 16, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (4, 17, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (4, 18, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (5, 1, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (5, 2, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (5, 3, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (5, 4, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (5, 5, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (5, 6, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (5, 7, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (5, 8, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (5, 9, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (5, 10, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (5, 11, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (5, 12, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (5, 13, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (5, 14, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (5, 15, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (5, 16, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (5, 17, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (5, 18, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (6, 1, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (6, 2, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (6, 3, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (6, 4, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (6, 5, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (6, 6, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (6, 7, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (6, 8, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (6, 9, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (6, 10, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (6, 11, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (6, 12, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (6, 13, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (6, 14, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (6, 15, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (6, 16, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (6, 17, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (6, 18, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (7, 1, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (7, 2, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (7, 3, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (7, 4, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (7, 5, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (7, 6, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (7, 7, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (7, 8, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (7, 9, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (7, 10, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (7, 11, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (7, 12, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (7, 13, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (7, 14, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (7, 15, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (7, 16, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (7, 17, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (7, 18, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (10, 1, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (10, 2, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (10, 3, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (10, 4, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (10, 5, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (10, 6, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (10, 7, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (10, 8, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (10, 9, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (10, 10, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (10, 11, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (10, 12, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (10, 13, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (10, 14, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (10, 15, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (10, 16, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (10, 17, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (10, 18, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (11, 1, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (11, 2, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (11, 3, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (11, 4, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (11, 5, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (11, 6, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (11, 7, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (11, 8, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (11, 9, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (11, 10, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (11, 11, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (11, 12, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (11, 13, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (11, 14, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (11, 15, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (11, 16, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (11, 17, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (11, 18, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (12, 1, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (12, 2, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (12, 3, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (12, 4, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (12, 5, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (12, 6, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (12, 7, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (12, 8, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (12, 9, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (12, 10, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (12, 11, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (12, 12, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (12, 13, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (12, 14, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (12, 15, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (12, 16, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (12, 17, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (12, 18, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (13, 1, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (13, 2, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (13, 3, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (13, 4, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (13, 5, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (13, 6, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (13, 7, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (13, 8, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (13, 9, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (13, 10, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (13, 11, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (13, 12, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (13, 13, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (13, 14, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (13, 15, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (13, 16, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (13, 17, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (13, 18, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (14, 1, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (14, 2, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (14, 3, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (14, 4, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (14, 5, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (14, 6, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (14, 7, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (14, 8, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (14, 9, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (14, 10, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (14, 11, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (14, 12, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (14, 13, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (14, 14, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (14, 15, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (14, 16, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (14, 17, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (14, 18, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (15, 1, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (15, 2, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (15, 3, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (15, 4, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (15, 5, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (15, 6, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (15, 7, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (15, 8, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (15, 9, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (15, 10, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (15, 11, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (15, 12, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (15, 13, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (15, 14, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (15, 15, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (15, 16, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (15, 17, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (15, 18, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (16, 1, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (16, 2, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (16, 3, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (16, 4, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (16, 5, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (16, 6, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (16, 7, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (16, 8, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (16, 9, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (16, 10, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (16, 11, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (16, 12, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (16, 13, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (16, 14, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (16, 15, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (16, 16, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (16, 17, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (16, 18, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (17, 1, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (17, 2, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (17, 3, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (17, 4, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (17, 5, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (17, 6, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (17, 7, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (17, 8, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (17, 9, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (17, 10, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (17, 11, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (17, 12, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (17, 13, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (17, 14, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (17, 15, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (17, 16, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (17, 17, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (17, 18, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (18, 1, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (18, 2, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (18, 3, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (18, 4, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (18, 5, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (18, 6, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (18, 7, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (18, 8, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (18, 9, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (18, 10, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (18, 11, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (18, 12, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (18, 13, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (18, 14, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (18, 15, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (18, 16, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (18, 17, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (18, 18, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (19, 1, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (19, 2, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (19, 3, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (19, 4, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (19, 5, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (19, 6, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (19, 7, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (19, 8, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (19, 9, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (19, 10, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (19, 11, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (19, 12, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (19, 13, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (19, 14, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (19, 15, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (19, 16, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (19, 17, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (19, 18, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (20, 1, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (20, 2, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (20, 3, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (20, 4, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (20, 5, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (20, 6, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (20, 7, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (20, 8, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (20, 9, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (20, 10, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (20, 11, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (20, 12, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (20, 13, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (20, 14, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (20, 15, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (20, 16, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (20, 17, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (20, 18, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (21, 1, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (21, 2, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (21, 3, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (21, 4, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (21, 5, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (21, 6, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (21, 7, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (21, 8, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (21, 9, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (21, 10, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (21, 11, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (21, 12, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (21, 13, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (21, 14, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (21, 15, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (21, 16, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (21, 17, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (21, 18, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (22, 1, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (22, 2, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (22, 3, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (22, 4, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (22, 5, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (22, 6, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (22, 7, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (22, 8, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (22, 9, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (22, 10, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (22, 11, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (22, 12, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (22, 13, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (22, 14, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (22, 15, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (22, 16, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (22, 17, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (22, 18, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (23, 1, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (23, 2, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (23, 3, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (23, 4, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (23, 5, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (23, 6, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (23, 7, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (23, 8, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (23, 9, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (23, 10, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (23, 11, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (23, 12, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (23, 13, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (23, 14, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (23, 15, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (23, 16, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (23, 17, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (23, 18, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (24, 1, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (24, 2, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (24, 3, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (24, 4, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (24, 5, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (24, 6, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (24, 7, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (24, 8, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (24, 9, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (24, 10, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (24, 11, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (24, 12, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (24, 13, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (24, 14, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (24, 15, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (24, 16, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (24, 17, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (24, 18, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (25, 1, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (25, 2, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (25, 3, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (25, 4, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (25, 5, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (25, 6, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (25, 7, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (25, 8, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (25, 9, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (25, 10, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (25, 11, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (25, 12, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (25, 13, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (25, 14, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (25, 15, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (25, 16, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (25, 17, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (25, 18, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (26, 1, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (26, 2, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (26, 3, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (26, 4, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (26, 5, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (26, 6, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (26, 7, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (26, 8, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (26, 9, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (26, 10, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (26, 11, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (26, 12, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (26, 13, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (26, 14, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (26, 15, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (26, 16, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (26, 17, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (26, 18, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (27, 1, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (27, 2, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (27, 3, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (27, 4, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (27, 5, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (27, 6, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (27, 7, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (27, 8, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (27, 9, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (27, 10, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (27, 11, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (27, 12, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (27, 13, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (27, 14, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (27, 15, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (27, 16, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (27, 17, GETDATE(), GETDATE());
INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (27, 18, GETDATE(), GETDATE());

-- ==========================================
-- 4. SEED PRODUCTS, VARIANTS & STOCKS
-- ==========================================
PRINT N'---> Seeding Products & ProductVariants & Stock...';
DECLARE @ProdId INT;
DECLARE @VarId INT;

-- Product: iPhone 16 Pro Max
INSERT INTO Products (Name, ProductCode, Slug, Description, Specs, BasePrice, OriginalPrice, TotalStock, ReservedStock, IsActive, IsFeatured, CreatedAt, UpdatedAt, CategoryId, BrandId, ThumbnailImage, MainImage, Images)
VALUES (N'iPhone 16 Pro Max', 'PROD-IP16PM', 'iphone-16-pro-max', N'iPhone 16 Pro Max sở hữu khung vỏ Titanium cấp 5 siêu nhẹ, chip Apple A18 Pro 3nm mạnh mẽ nhất thế giới, nút Camera Control đột phá, hỗ trợ quay video 4K 120fps Dolby Vision và hệ thống trí tuệ nhân tạo Apple Intelligence.', N'{"Màn hình":"6.9 inch Super Retina XDR OLED, 120Hz ProMotion","Chip":"Apple A18 Pro (3nm)","RAM":"8GB","Camera sau":"Chính 48MP + Góc siêu rộng 48MP + Tele 5x 12MP","Camera trước":"12MP TrueDepth","Pin & Sạc":"Sạc nhanh 30W, Sạc không dây MagSafe 25W","Chất liệu":"Khung Titanium, mặt lưng kính nhám"}', 34990000, 36990000, 0, 0, 1, 1, GETDATE(), GETDATE(), 10, 1, 'https://images.unsplash.com/photo-1695048133142-1a20484d2569?q=80&w=800&auto=format&fit=crop', 'https://images.unsplash.com/photo-1695048133142-1a20484d2569?q=80&w=800&auto=format&fit=crop', N'["https://images.unsplash.com/photo-1695048133142-1a20484d2569?q=80&w=800&auto=format&fit=crop","https://images.unsplash.com/photo-1695048133142-1a20484d2569?q=80&w=800&auto=format&fit=crop"]');
SET @ProdId = SCOPE_IDENTITY();

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Titan Sa Mạc / 256GB', 'IP16PM-256-DESERT', 34990000, 45, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Titan Sa Mạc (Desert Titanium)","Dung lượng":"256GB"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 45, 45, N'Cái', 27292200, DATEADD(day, -10, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 45 WHERE Id = @ProdId;

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Titan Tự Nhiên / 256GB', 'IP16PM-256-NATURAL', 34990000, 30, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Titan Tự Nhiên (Natural Titanium)","Dung lượng":"256GB"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 30, 30, N'Cái', 27292200, DATEADD(day, -15, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 30 WHERE Id = @ProdId;

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Titan Đen / 512GB', 'IP16PM-512-BLACK', 40990000, 20, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Titan Đen (Black Titanium)","Dung lượng":"512GB"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 20, 20, N'Cái', 31972200, DATEADD(day, -20, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 20 WHERE Id = @ProdId;

-- Product: iPhone 15 Pro Max
INSERT INTO Products (Name, ProductCode, Slug, Description, Specs, BasePrice, OriginalPrice, TotalStock, ReservedStock, IsActive, IsFeatured, CreatedAt, UpdatedAt, CategoryId, BrandId, ThumbnailImage, MainImage, Images)
VALUES (N'iPhone 15 Pro Max', 'PROD-IP15PM', 'iphone-15-pro-max', N'iPhone 15 Pro Max trang bị vi xử lý Apple A17 Pro mạnh mẽ, camera zoom quang học 5x, nút Action Button tiện lợi cùng cổng sạc chuẩn USB-C tốc độ cao 10Gbps.', N'{"Màn hình":"6.7 inch Super Retina XDR OLED, 120Hz ProMotion","Chip":"Apple A17 Pro (3nm)","RAM":"8GB","Camera sau":"48MP + 12MP + 12MP (Zoom 5x)","Cổng sạc":"USB-C 3.0","Khung máy":"Titanium"}', 28990000, 32990000, 0, 0, 1, 1, GETDATE(), GETDATE(), 10, 1, 'https://images.unsplash.com/photo-1695048133142-1a20484d2569?q=80&w=800&auto=format&fit=crop', 'https://images.unsplash.com/photo-1695048133142-1a20484d2569?q=80&w=800&auto=format&fit=crop', N'["https://images.unsplash.com/photo-1695048133142-1a20484d2569?q=80&w=800&auto=format&fit=crop","https://images.unsplash.com/photo-1695048133142-1a20484d2569?q=80&w=800&auto=format&fit=crop"]');
SET @ProdId = SCOPE_IDENTITY();

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Titan Tự Nhiên / 256GB', 'IP15PM-256-NAT', 28990000, 35, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Titan Tự Nhiên","Dung lượng":"256GB"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 35, 35, N'Cái', 22612200, DATEADD(day, -10, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 35 WHERE Id = @ProdId;

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Titan Xanh / 512GB', 'IP15PM-512-BLUE', 34990000, 15, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Titan Xanh","Dung lượng":"512GB"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 15, 15, N'Cái', 27292200, DATEADD(day, -15, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 15 WHERE Id = @ProdId;

-- Product: iPhone 15 128GB
INSERT INTO Products (Name, ProductCode, Slug, Description, Specs, BasePrice, OriginalPrice, TotalStock, ReservedStock, IsActive, IsFeatured, CreatedAt, UpdatedAt, CategoryId, BrandId, ThumbnailImage, MainImage, Images)
VALUES (N'iPhone 15 128GB', 'PROD-IP15', 'iphone-15-128gb', N'iPhone 15 đột phá với màn hình Dynamic Island linh hoạt, camera chính 48MP cực sắc nét, mặt lưng kính pha màu thời thượng và cổng kết nối USB-C chuẩn mực.', N'{"Màn hình":"6.1 inch OLED Super Retina XDR","Chip":"Apple A16 Bionic","RAM":"6GB","Camera":"48MP + 12MP","Tính năng":"Dynamic Island, USB-C"}', 19490000, 22990000, 0, 0, 1, 0, GETDATE(), GETDATE(), 10, 1, 'https://images.unsplash.com/photo-1592750475338-74b7b21085ab?q=80&w=800&auto=format&fit=crop', 'https://images.unsplash.com/photo-1592750475338-74b7b21085ab?q=80&w=800&auto=format&fit=crop', N'["https://images.unsplash.com/photo-1592750475338-74b7b21085ab?q=80&w=800&auto=format&fit=crop","https://images.unsplash.com/photo-1592750475338-74b7b21085ab?q=80&w=800&auto=format&fit=crop"]');
SET @ProdId = SCOPE_IDENTITY();

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Màu Hồng / 128GB', 'IP15-128-PINK', 19490000, 50, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Hồng (Pink)","Dung lượng":"128GB"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 50, 50, N'Cái', 15202200, DATEADD(day, -10, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 50 WHERE Id = @ProdId;

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Màu Xanh Lá / 128GB', 'IP15-128-GREEN', 19490000, 40, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Xanh Lá (Green)","Dung lượng":"128GB"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 40, 40, N'Cái', 15202200, DATEADD(day, -15, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 40 WHERE Id = @ProdId;

-- Product: Samsung Galaxy S24 Ultra
INSERT INTO Products (Name, ProductCode, Slug, Description, Specs, BasePrice, OriginalPrice, TotalStock, ReservedStock, IsActive, IsFeatured, CreatedAt, UpdatedAt, CategoryId, BrandId, ThumbnailImage, MainImage, Images)
VALUES (N'Samsung Galaxy S24 Ultra', 'PROD-S24U', 'samsung-galaxy-s24-ultra', N'Samsung Galaxy S24 Ultra quyền năng AI vượt trội (Galaxy AI: Khoanh vùng tìm kiếm, Trợ lý quyền năng, Phiên dịch trực tiếp), khung vỏ Titanium phẳng cứng cáp và bút S Pen tích hợp.', N'{"Màn hình":"6.8 inch Dynamic AMOLED 2X, 120Hz 2600 nits","Chip":"Snapdragon 8 Gen 3 for Galaxy","RAM":"12GB","Camera":"200MP + 50MP + 12MP + 10MP","Pin":"5000mAh, Sạc 45W","Bút cảm ứng":"Tích hợp S-Pen"}', 29990000, 33990000, 0, 0, 1, 1, GETDATE(), GETDATE(), 11, 2, 'https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?q=80&w=800&auto=format&fit=crop', 'https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?q=80&w=800&auto=format&fit=crop', N'["https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?q=80&w=800&auto=format&fit=crop","https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?q=80&w=800&auto=format&fit=crop"]');
SET @ProdId = SCOPE_IDENTITY();

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Xám Titanium / 256GB', 'S24U-256-GRAY', 29990000, 40, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Xám Titanium","Dung lượng":"256GB"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 40, 40, N'Cái', 23392200, DATEADD(day, -10, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 40 WHERE Id = @ProdId;

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Đen Titanium / 512GB', 'S24U-512-BLACK', 34490000, 25, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Đen Titanium","Dung lượng":"512GB"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 25, 25, N'Cái', 26902200, DATEADD(day, -15, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 25 WHERE Id = @ProdId;

-- Product: Samsung Galaxy Z Fold6 5G
INSERT INTO Products (Name, ProductCode, Slug, Description, Specs, BasePrice, OriginalPrice, TotalStock, ReservedStock, IsActive, IsFeatured, CreatedAt, UpdatedAt, CategoryId, BrandId, ThumbnailImage, MainImage, Images)
VALUES (N'Samsung Galaxy Z Fold6 5G', 'PROD-ZFOLD6', 'samsung-galaxy-z-fold6', N'Galaxy Z Fold6 thiết kế siêu mỏng nhẹ vuông vức hoàn hảo, bản lề FlexHinge thế hệ mới bền bỉ, màn hình cực đại 7.6 inch nâng tầm hiệu suất làm việc đa nhiệm cùng Galaxy AI.', N'{"Màn hình chính":"7.6 inch Dynamic AMOLED 2X 120Hz","Màn hình phụ":"6.3 inch 120Hz","Chip":"Snapdragon 8 Gen 3","RAM":"12GB","Bộ nhớ":"256GB/512GB"}', 41990000, 43990000, 0, 0, 1, 1, GETDATE(), GETDATE(), 11, 2, 'https://images.unsplash.com/photo-1580910051074-3eb694886505?q=80&w=800&auto=format&fit=crop', 'https://images.unsplash.com/photo-1580910051074-3eb694886505?q=80&w=800&auto=format&fit=crop', N'["https://images.unsplash.com/photo-1580910051074-3eb694886505?q=80&w=800&auto=format&fit=crop","https://images.unsplash.com/photo-1580910051074-3eb694886505?q=80&w=800&auto=format&fit=crop"]');
SET @ProdId = SCOPE_IDENTITY();

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Xám Metal / 256GB', 'ZFOLD6-256-GRAY', 41990000, 15, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Xám Metal","Dung lượng":"256GB"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 15, 15, N'Cái', 32752200, DATEADD(day, -10, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 15 WHERE Id = @ProdId;

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Xanh Navy / 512GB', 'ZFOLD6-512-NAVY', 46990000, 10, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Xanh Navy","Dung lượng":"512GB"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 10, 10, N'Cái', 36652200, DATEADD(day, -15, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 10 WHERE Id = @ProdId;

-- Product: Samsung Galaxy A55 5G
INSERT INTO Products (Name, ProductCode, Slug, Description, Specs, BasePrice, OriginalPrice, TotalStock, ReservedStock, IsActive, IsFeatured, CreatedAt, UpdatedAt, CategoryId, BrandId, ThumbnailImage, MainImage, Images)
VALUES (N'Samsung Galaxy A55 5G', 'PROD-A55', 'samsung-galaxy-a55-5g', N'Galaxy A55 5G khung kim loại sang trọng, camera đêm 50MP nét vượt trội, kháng nước chống bụi IP67 và vi xử lý Exynos 1480 4nm tiết kiệm pin.', N'{"Màn hình":"6.6 inch Super AMOLED 120Hz","Chip":"Exynos 1480 (4nm)","RAM":"8GB","Pin":"5000mAh","Kháng nước":"IP67"}', 9690000, 10990000, 0, 0, 1, 0, GETDATE(), GETDATE(), 11, 2, 'https://images.unsplash.com/photo-1565849904461-04a58ad377e0?q=80&w=800&auto=format&fit=crop', 'https://images.unsplash.com/photo-1565849904461-04a58ad377e0?q=80&w=800&auto=format&fit=crop', N'["https://images.unsplash.com/photo-1565849904461-04a58ad377e0?q=80&w=800&auto=format&fit=crop","https://images.unsplash.com/photo-1565849904461-04a58ad377e0?q=80&w=800&auto=format&fit=crop"]');
SET @ProdId = SCOPE_IDENTITY();

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Xanh Băng / 128GB', 'A55-128-ICE', 9690000, 60, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Xanh Băng (Iceblue)","Dung lượng":"128GB"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 60, 60, N'Cái', 7558200, DATEADD(day, -10, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 60 WHERE Id = @ProdId;

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Tím Mới / 256GB', 'A55-256-PURPLE', 10690000, 50, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Tím Lilac","Dung lượng":"256GB"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 50, 50, N'Cái', 8338200, DATEADD(day, -15, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 50 WHERE Id = @ProdId;

-- Product: Xiaomi 14 Ultra 5G
INSERT INTO Products (Name, ProductCode, Slug, Description, Specs, BasePrice, OriginalPrice, TotalStock, ReservedStock, IsActive, IsFeatured, CreatedAt, UpdatedAt, CategoryId, BrandId, ThumbnailImage, MainImage, Images)
VALUES (N'Xiaomi 14 Ultra 5G', 'PROD-XM14U', 'xiaomi-14-ultra', N'Xiaomi 14 Ultra kết hợp cùng ống kính Leica Summilux huyền thoại, cảm biến 1-inch khẩu độ vô cấp LYT-900, chip Snapdragon 8 Gen 3 và công nghệ sạc siêu tốc 90W HyperCharge.', N'{"Màn hình":"6.73 inch AMOLED 2K+ 120Hz LTPO","Ống kính":"Bộ 4 camera 50MP Leica","Chip":"Snapdragon 8 Gen 3","RAM":"16GB","Pin":"5000mAh, Sạc 90W"}', 29990000, 32990000, 0, 0, 1, 1, GETDATE(), GETDATE(), 12, 3, 'https://images.unsplash.com/photo-1598327105666-5b89351aff97?q=80&w=800&auto=format&fit=crop', 'https://images.unsplash.com/photo-1598327105666-5b89351aff97?q=80&w=800&auto=format&fit=crop', N'["https://images.unsplash.com/photo-1598327105666-5b89351aff97?q=80&w=800&auto=format&fit=crop","https://images.unsplash.com/photo-1598327105666-5b89351aff97?q=80&w=800&auto=format&fit=crop"]');
SET @ProdId = SCOPE_IDENTITY();

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Màu Đen / 512GB', 'XM14U-512-BLK', 29990000, 20, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Đen da tổng hợp","Dung lượng":"512GB"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 20, 20, N'Cái', 23392200, DATEADD(day, -10, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 20 WHERE Id = @ProdId;

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Màu Trắng / 512GB', 'XM14U-512-WHT', 29990000, 15, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Trắng","Dung lượng":"512GB"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 15, 15, N'Cái', 23392200, DATEADD(day, -15, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 15 WHERE Id = @ProdId;

-- Product: Xiaomi Redmi Note 13 Pro+ 5G
INSERT INTO Products (Name, ProductCode, Slug, Description, Specs, BasePrice, OriginalPrice, TotalStock, ReservedStock, IsActive, IsFeatured, CreatedAt, UpdatedAt, CategoryId, BrandId, ThumbnailImage, MainImage, Images)
VALUES (N'Xiaomi Redmi Note 13 Pro+ 5G', 'PROD-RN13PP', 'redmi-note-13-pro-plus', N'Redmi Note 13 Pro+ 5G trang bị màn hình cong AMOLED 1.5K 120Hz, camera siêu phân giải 200MP chống rung OIS và sạc thần tốc 120W đầy pin trong 19 phút.', N'{"Màn hình":"6.67 inch AMOLED 1.5K 120Hz","Camera":"200MP OIS","Chip":"Dimensity 7200-Ultra","Sạc nhanh":"120W HyperCharge","Kháng nước":"IP68"}', 9490000, 10990000, 0, 0, 1, 0, GETDATE(), GETDATE(), 12, 3, 'https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?q=80&w=800&auto=format&fit=crop', 'https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?q=80&w=800&auto=format&fit=crop', N'["https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?q=80&w=800&auto=format&fit=crop","https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?q=80&w=800&auto=format&fit=crop"]');
SET @ProdId = SCOPE_IDENTITY();

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Đen Đêm / 256GB', 'RN13PP-256-BLK', 9490000, 55, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Đen Đêm","Dung lượng":"256GB"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 55, 55, N'Cái', 7402200, DATEADD(day, -10, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 55 WHERE Id = @ProdId;

-- Product: OPPO Find N3 5G
INSERT INTO Products (Name, ProductCode, Slug, Description, Specs, BasePrice, OriginalPrice, TotalStock, ReservedStock, IsActive, IsFeatured, CreatedAt, UpdatedAt, CategoryId, BrandId, ThumbnailImage, MainImage, Images)
VALUES (N'OPPO Find N3 5G', 'PROD-OPFINDN3', 'oppo-find-n3', N'OPPO Find N3 thiết kế gập đỉnh cao với camera Hasselblad sắc nét hàng đầu phân khúc, màn hình sáng nits kỉ lục và công nghệ làm việc đa nhiệm không nếp gấp.', N'{"Màn hình gập":"7.82 inch AMOLED 120Hz","Camera":"Chính 48MP + Tele 64MP Hasselblad","Chip":"Snapdragon 8 Gen 2","RAM":"16GB","Bộ nhớ":"512GB"}', 41990000, 44990000, 0, 0, 1, 1, GETDATE(), GETDATE(), 13, 8, 'https://images.unsplash.com/photo-1546054454-aa26e2b734c7?q=80&w=800&auto=format&fit=crop', 'https://images.unsplash.com/photo-1546054454-aa26e2b734c7?q=80&w=800&auto=format&fit=crop', N'["https://images.unsplash.com/photo-1546054454-aa26e2b734c7?q=80&w=800&auto=format&fit=crop","https://images.unsplash.com/photo-1546054454-aa26e2b734c7?q=80&w=800&auto=format&fit=crop"]');
SET @ProdId = SCOPE_IDENTITY();

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Vàng Hoàng Kim / 512GB', 'FINDN3-512-GOLD', 41990000, 12, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Vàng Hoàng Kim","Dung lượng":"512GB"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 12, 12, N'Cái', 32752200, DATEADD(day, -10, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 12 WHERE Id = @ProdId;

-- Product: MacBook Air 13 inch M3 2024
INSERT INTO Products (Name, ProductCode, Slug, Description, Specs, BasePrice, OriginalPrice, TotalStock, ReservedStock, IsActive, IsFeatured, CreatedAt, UpdatedAt, CategoryId, BrandId, ThumbnailImage, MainImage, Images)
VALUES (N'MacBook Air 13 inch M3 2024', 'PROD-MBA13M3', 'macbook-air-13-m3-2024', N'MacBook Air 13 inch chip M3 siêu mỏng nhẹ 1.24kg, hỗ trợ xuất 2 màn hình ngoài, thời lượng pin ấn tượng tới 18 giờ liên tục.', N'{"Màn hình":"13.6 inch Liquid Retina 500 nits","Chip":"Apple M3 (8-core CPU, 8-core/10-core GPU)","RAM":"8GB / 16GB Unified","SSD":"256GB / 512GB","Pin":"18 giờ liên tục"}', 26990000, 27990000, 0, 0, 1, 1, GETDATE(), GETDATE(), 14, 1, 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8?q=80&w=800&auto=format&fit=crop', 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8?q=80&w=800&auto=format&fit=crop', N'["https://images.unsplash.com/photo-1517336714731-489689fd1ca8?q=80&w=800&auto=format&fit=crop","https://images.unsplash.com/photo-1517336714731-489689fd1ca8?q=80&w=800&auto=format&fit=crop"]');
SET @ProdId = SCOPE_IDENTITY();

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Midnight (Đen Đêm) / 8GB / 256GB', 'MBA13M3-8-256-MID', 26990000, 30, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Midnight (Đen Đêm)","RAM":"8GB","SSD":"256GB"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 30, 30, N'Cái', 21052200, DATEADD(day, -10, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 30 WHERE Id = @ProdId;

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Starlight (Vàng Ánh Kim) / 16GB / 512GB', 'MBA13M3-16-512-STL', 36990000, 20, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Starlight","RAM":"16GB","SSD":"512GB"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 20, 20, N'Cái', 28852200, DATEADD(day, -15, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 20 WHERE Id = @ProdId;

-- Product: MacBook Pro 14 inch M3 Pro
INSERT INTO Products (Name, ProductCode, Slug, Description, Specs, BasePrice, OriginalPrice, TotalStock, ReservedStock, IsActive, IsFeatured, CreatedAt, UpdatedAt, CategoryId, BrandId, ThumbnailImage, MainImage, Images)
VALUES (N'MacBook Pro 14 inch M3 Pro', 'PROD-MBP14M3P', 'macbook-pro-14-m3-pro', N'MacBook Pro 14 M3 Pro màu Space Black ấn tượng, màn hình XDR ProMotion 120Hz chuyên nghiệp cho lập trình viên, nhà thiết kế 3D và dựng phim 8K.', N'{"Màn hình":"14.2 inch Liquid Retina XDR (3024x1964) 120Hz","Chip":"Apple M3 Pro (11-core CPU, 14-core GPU)","RAM":"18GB Unified","SSD":"512GB NVMe"}', 49990000, 54990000, 0, 0, 1, 1, GETDATE(), GETDATE(), 14, 1, 'https://images.unsplash.com/photo-1611186871348-b1ce696e52c9?q=80&w=800&auto=format&fit=crop', 'https://images.unsplash.com/photo-1611186871348-b1ce696e52c9?q=80&w=800&auto=format&fit=crop', N'["https://images.unsplash.com/photo-1611186871348-b1ce696e52c9?q=80&w=800&auto=format&fit=crop","https://images.unsplash.com/photo-1611186871348-b1ce696e52c9?q=80&w=800&auto=format&fit=crop"]');
SET @ProdId = SCOPE_IDENTITY();

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Space Black / 18GB / 512GB', 'MBP14M3P-18-512-BLK', 49990000, 15, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Space Black (Đen Thạch Anh)","RAM":"18GB","SSD":"512GB"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 15, 15, N'Cái', 38992200, DATEADD(day, -10, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 15 WHERE Id = @ProdId;

-- Product: ASUS ROG Zephyrus G14 OLED 2024
INSERT INTO Products (Name, ProductCode, Slug, Description, Specs, BasePrice, OriginalPrice, TotalStock, ReservedStock, IsActive, IsFeatured, CreatedAt, UpdatedAt, CategoryId, BrandId, ThumbnailImage, MainImage, Images)
VALUES (N'ASUS ROG Zephyrus G14 OLED 2024', 'PROD-ZEPHYRUSG14', 'asus-rog-zephyrus-g14-2024', N'ASUS ROG Zephyrus G14 thiết kế nhôm nguyên khối siêu mỏng, màn hình ROG Nebula OLED 3K 120Hz chuẩn màu 100% DCI-P3 và GPU RTX 4060 chiến mượt mọi tựa game AAA.', N'{"Màn hình":"14.0 inch 3K OLED 120Hz 0.2ms","CPU":"AMD Ryzen 9 8945HS","VGA":"NVIDIA GeForce RTX 4060 8GB GDDR6","RAM":"16GB LPDDR5X","SSD":"1TB PCIe 4.0"}', 42990000, 46990000, 0, 0, 1, 1, GETDATE(), GETDATE(), 15, 4, 'https://images.unsplash.com/photo-1603302576837-37561b2e2302?q=80&w=800&auto=format&fit=crop', 'https://images.unsplash.com/photo-1603302576837-37561b2e2302?q=80&w=800&auto=format&fit=crop', N'["https://images.unsplash.com/photo-1603302576837-37561b2e2302?q=80&w=800&auto=format&fit=crop","https://images.unsplash.com/photo-1603302576837-37561b2e2302?q=80&w=800&auto=format&fit=crop"]');
SET @ProdId = SCOPE_IDENTITY();

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Platinum White / Ryzen 9 / RTX 4060', 'G14-R9-4060-WHT', 42990000, 15, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Platinum White","Cấu hình":"Ryzen 9 / RTX 4060 / 16GB / 1TB"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 15, 15, N'Cái', 33532200, DATEADD(day, -10, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 15 WHERE Id = @ProdId;

-- Product: Dell XPS 13 9340 Core Ultra 7
INSERT INTO Products (Name, ProductCode, Slug, Description, Specs, BasePrice, OriginalPrice, TotalStock, ReservedStock, IsActive, IsFeatured, CreatedAt, UpdatedAt, CategoryId, BrandId, ThumbnailImage, MainImage, Images)
VALUES (N'Dell XPS 13 9340 Core Ultra 7', 'PROD-DELLXPS13', 'dell-xps-13-9340', N'Dell XPS 13 chuẩn mực ultrabook tương lai với kính Gorillaglass tràn viền, phím bấm hàng chức năng cảm ứng Touch Bar hiện đại, trang bị chip Intel Core Ultra AI.', N'{"Màn hình":"13.4 inch FHD+ InfinityEdge IPS","CPU":"Intel Core Ultra 7 155H (NPU AI)","RAM":"16GB LPDDR5X","SSD":"512GB NVMe"}', 44990000, 47990000, 0, 0, 1, 0, GETDATE(), GETDATE(), 16, 5, 'https://images.unsplash.com/photo-1593642632823-8f785ba67e45?q=80&w=800&auto=format&fit=crop', 'https://images.unsplash.com/photo-1593642632823-8f785ba67e45?q=80&w=800&auto=format&fit=crop', N'["https://images.unsplash.com/photo-1593642632823-8f785ba67e45?q=80&w=800&auto=format&fit=crop","https://images.unsplash.com/photo-1593642632823-8f785ba67e45?q=80&w=800&auto=format&fit=crop"]');
SET @ProdId = SCOPE_IDENTITY();

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Màu Platinum / Core Ultra 7', 'XPS9340-U7-16-512', 44990000, 18, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Platinum","CPU":"Intel Core Ultra 7","RAM":"16GB"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 18, 18, N'Cái', 35092200, DATEADD(day, -10, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 18 WHERE Id = @ProdId;

-- Product: iPad Pro 11 inch M4 2024 Ultra Retina OLED
INSERT INTO Products (Name, ProductCode, Slug, Description, Specs, BasePrice, OriginalPrice, TotalStock, ReservedStock, IsActive, IsFeatured, CreatedAt, UpdatedAt, CategoryId, BrandId, ThumbnailImage, MainImage, Images)
VALUES (N'iPad Pro 11 inch M4 2024 Ultra Retina OLED', 'PROD-IPADPROM4', 'ipad-pro-11-m4-2024', N'iPad Pro M4 2024 mỏng chưa từng có chỉ 5.3mm, đột phá công nghệ màn hình Tandem OLED Ultra Retina XDR và chip Apple M4 xử lý AI đồ họa cực đại.', N'{"Màn hình":"11 inch Ultra Retina Tandem OLED 120Hz","Chip":"Apple M4 (9-core CPU, 10-core GPU)","Độ mỏng":"5.3 mm","Hỗ trợ":"Apple Pencil Pro, Magic Keyboard M4"}', 28490000, 29990000, 0, 0, 1, 1, GETDATE(), GETDATE(), 17, 1, 'https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0?q=80&w=800&auto=format&fit=crop', 'https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0?q=80&w=800&auto=format&fit=crop', N'["https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0?q=80&w=800&auto=format&fit=crop","https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0?q=80&w=800&auto=format&fit=crop"]');
SET @ProdId = SCOPE_IDENTITY();

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Space Black / WiFi / 256GB', 'IPADPROM4-11-256-BLK', 28490000, 25, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Space Black","Kết nối":"Wi-Fi","Dung lượng":"256GB"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 25, 25, N'Cái', 22222200, DATEADD(day, -10, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 25 WHERE Id = @ProdId;

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Silver / WiFi + 5G / 512GB', 'IPADPROM4-11-512-5G', 37490000, 12, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Silver","Kết nối":"Wi-Fi + 5G","Dung lượng":"512GB"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 12, 12, N'Cái', 29242200, DATEADD(day, -15, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 12 WHERE Id = @ProdId;

-- Product: Samsung Galaxy Tab S9 Ultra
INSERT INTO Products (Name, ProductCode, Slug, Description, Specs, BasePrice, OriginalPrice, TotalStock, ReservedStock, IsActive, IsFeatured, CreatedAt, UpdatedAt, CategoryId, BrandId, ThumbnailImage, MainImage, Images)
VALUES (N'Samsung Galaxy Tab S9 Ultra', 'PROD-TABS9U', 'samsung-galaxy-tab-s9-ultra', N'Galaxy Tab S9 Ultra màn hình siêu lớn 14.6 inch AMOLED 120Hz, kèm bút S Pen chống nước IP68, đáp ứng hoàn hảo nhu cầu vẽ đồ họa, thiết kế và làm việc chuyên nghiệp.', N'{"Màn hình":"14.6 inch Dynamic AMOLED 2X 120Hz","Chip":"Snapdragon 8 Gen 2 for Galaxy","RAM":"12GB","Pin":"11200mAh","Kháng nước":"IP68"}', 26990000, 29990000, 0, 0, 1, 0, GETDATE(), GETDATE(), 18, 2, 'https://images.unsplash.com/photo-1585790050230-5dd28404ccb9?q=80&w=800&auto=format&fit=crop', 'https://images.unsplash.com/photo-1585790050230-5dd28404ccb9?q=80&w=800&auto=format&fit=crop', N'["https://images.unsplash.com/photo-1585790050230-5dd28404ccb9?q=80&w=800&auto=format&fit=crop","https://images.unsplash.com/photo-1585790050230-5dd28404ccb9?q=80&w=800&auto=format&fit=crop"]');
SET @ProdId = SCOPE_IDENTITY();

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Màu Xám / 256GB / Wifi', 'TABS9U-256-GRAY', 26990000, 20, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Xám (Graphite)","Dung lượng":"256GB"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 20, 20, N'Cái', 21052200, DATEADD(day, -10, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 20 WHERE Id = @ProdId;

-- Product: Apple Watch Ultra 2 GPS + Cellular 49mm
INSERT INTO Products (Name, ProductCode, Slug, Description, Specs, BasePrice, OriginalPrice, TotalStock, ReservedStock, IsActive, IsFeatured, CreatedAt, UpdatedAt, CategoryId, BrandId, ThumbnailImage, MainImage, Images)
VALUES (N'Apple Watch Ultra 2 GPS + Cellular 49mm', 'PROD-AWULTRA2', 'apple-watch-ultra-2-49mm', N'Apple Watch Ultra 2 vỏ Titanium siêu bền chống nước 100m, màn hình sáng kỷ lục 3000 nits, chip S9 SIP chạm hai lần Double Tap thông minh và định vị GPS tần số kép cực chính xác.', N'{"Kích thước":"49mm Titanium Case","Màn hình":"OLED 3000 nits Always-On","Chip":"Apple S9 SiP","Tính năng":"Double Tap, Còi báo động 86dB, Lặn 40m","Pin":"Up to 36 hours (60 hours Low Power)"}', 20990000, 21990000, 0, 0, 1, 1, GETDATE(), GETDATE(), 19, 1, 'https://images.unsplash.com/photo-1508685096489-7aacd43bd3b1?q=80&w=800&auto=format&fit=crop', 'https://images.unsplash.com/photo-1508685096489-7aacd43bd3b1?q=80&w=800&auto=format&fit=crop', N'["https://images.unsplash.com/photo-1508685096489-7aacd43bd3b1?q=80&w=800&auto=format&fit=crop","https://images.unsplash.com/photo-1508685096489-7aacd43bd3b1?q=80&w=800&auto=format&fit=crop"]');
SET @ProdId = SCOPE_IDENTITY();

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Dây Alpine Loop Size M / Cam', 'AWULTRA2-ALP-ORG', 20990000, 25, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Loại dây":"Alpine Loop","Màu dây":"Cam","Size":"49mm"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 25, 25, N'Cái', 16372200, DATEADD(day, -10, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 25 WHERE Id = @ProdId;

-- Product: Garmin Fenix 7 Pro Sapphire Solar Titanium
INSERT INTO Products (Name, ProductCode, Slug, Description, Specs, BasePrice, OriginalPrice, TotalStock, ReservedStock, IsActive, IsFeatured, CreatedAt, UpdatedAt, CategoryId, BrandId, ThumbnailImage, MainImage, Images)
VALUES (N'Garmin Fenix 7 Pro Sapphire Solar Titanium', 'PROD-GARMINF7P', 'garmin-fenix-7-pro-sapphire-solar', N'Garmin Fenix 7 Pro tích hợp kính sạc năng lượng mặt trời Sapphire chống trầy, đèn quắc LED chiếu sáng khẩn cấp, cảm biến nhịp tim Elevate Gen 5 và bản đồ địa hình đa lục địa.', N'{"Mặt đồng hồ":"47mm Kính Sapphire Solar","Đèn pin":"Đèn pin LED tích hợp","Cảm biến":"Elevate Gen 5","Pin":"Lên đến 22 ngày ở chế độ Smartwatch"}', 21990000, 23990000, 0, 0, 1, 0, GETDATE(), GETDATE(), 20, 13, 'https://images.unsplash.com/photo-1579586337278-3befd40fd17a?q=80&w=800&auto=format&fit=crop', 'https://images.unsplash.com/photo-1579586337278-3befd40fd17a?q=80&w=800&auto=format&fit=crop', N'["https://images.unsplash.com/photo-1579586337278-3befd40fd17a?q=80&w=800&auto=format&fit=crop","https://images.unsplash.com/photo-1579586337278-3befd40fd17a?q=80&w=800&auto=format&fit=crop"]');
SET @ProdId = SCOPE_IDENTITY();

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Titanium Gray / Dây Silicone Đen', 'FENIX7P-TIT-BLK', 21990000, 15, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Chất liệu":"Titanium","Màu sắc":"Đen Titanium"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 15, 15, N'Cái', 17152200, DATEADD(day, -10, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 15 WHERE Id = @ProdId;

-- Product: AirPods Pro 2 USB-C (MagSafe Case)
INSERT INTO Products (Name, ProductCode, Slug, Description, Specs, BasePrice, OriginalPrice, TotalStock, ReservedStock, IsActive, IsFeatured, CreatedAt, UpdatedAt, CategoryId, BrandId, ThumbnailImage, MainImage, Images)
VALUES (N'AirPods Pro 2 USB-C (MagSafe Case)', 'PROD-APP2USBC', 'airpods-pro-2-usbc', N'AirPods Pro 2 bản nâng cấp cổng sạc USB-C, chip Apple H2 chống ồn chủ động gấp 2 lần, tính năng Âm thanh thích ứng (Adaptive Audio) và chuẩn kháng bụi nước IP54.', N'{"Chip":"Apple H2 trong tai nghe, Apple U1 trong hộp sạc","Chống ồn":"Active Noise Cancellation (ANC) x2","Cổng sạc":"USB-C & MagSafe","Thời lượng pin":"6 giờ (hộp sạc lên 30 giờ)"}', 5690000, 6190000, 0, 0, 1, 1, GETDATE(), GETDATE(), 21, 1, 'https://images.unsplash.com/photo-1600294037681-c80b4cb5b434?q=80&w=800&auto=format&fit=crop', 'https://images.unsplash.com/photo-1600294037681-c80b4cb5b434?q=80&w=800&auto=format&fit=crop', N'["https://images.unsplash.com/photo-1600294037681-c80b4cb5b434?q=80&w=800&auto=format&fit=crop","https://images.unsplash.com/photo-1600294037681-c80b4cb5b434?q=80&w=800&auto=format&fit=crop"]');
SET @ProdId = SCOPE_IDENTITY();

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Màu Trắng / USB-C', 'APP2-USBC-WHT', 5690000, 80, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Trắng","Cổng sạc":"USB-C"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 80, 80, N'Cái', 4438200, DATEADD(day, -10, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 80 WHERE Id = @ProdId;

-- Product: Tai nghe Sony WH-1000XM5 Noise Canceling
INSERT INTO Products (Name, ProductCode, Slug, Description, Specs, BasePrice, OriginalPrice, TotalStock, ReservedStock, IsActive, IsFeatured, CreatedAt, UpdatedAt, CategoryId, BrandId, ThumbnailImage, MainImage, Images)
VALUES (N'Tai nghe Sony WH-1000XM5 Noise Canceling', 'PROD-SONYXM5', 'sony-wh-1000xm5', N'Sony WH-1000XM5 với 8 micro và 2 bộ xử lý chống ồn V1/QN1 mang lại trải nghiệm âm thanh tĩnh lặng tuyệt đối, hỗ trợ LDAC Hi-Res Audio không dây và đàm thoại siêu rõ nét.', N'{"Kiểu dáng":"Chụp tai Over-Ear","Bộ xử lý":"HD Noise Canceling Processor QN1 + V1","Pin":"30 giờ bật ANC (Sạc 3 phút dùng 3 giờ)","Codec":"LDAC, AAC, SBC"}', 7990000, 8990000, 0, 0, 1, 1, GETDATE(), GETDATE(), 21, 9, 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?q=80&w=800&auto=format&fit=crop', 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?q=80&w=800&auto=format&fit=crop', N'["https://images.unsplash.com/photo-1505740420928-5e560c06d30e?q=80&w=800&auto=format&fit=crop","https://images.unsplash.com/photo-1505740420928-5e560c06d30e?q=80&w=800&auto=format&fit=crop"]');
SET @ProdId = SCOPE_IDENTITY();

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Màu Đen / Black', 'XM5-HEADPHONE-BLK', 7990000, 35, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Đen"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 35, 35, N'Cái', 6232200, DATEADD(day, -10, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 35 WHERE Id = @ProdId;

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Màu Bạc Bạch Kim / Silver', 'XM5-HEADPHONE-SLV', 7990000, 25, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Bạc Bạch Kim"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 25, 25, N'Cái', 6232200, DATEADD(day, -15, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 25 WHERE Id = @ProdId;

-- Product: Loa Bluetooth JBL Charge 5 40W IP67
INSERT INTO Products (Name, ProductCode, Slug, Description, Specs, BasePrice, OriginalPrice, TotalStock, ReservedStock, IsActive, IsFeatured, CreatedAt, UpdatedAt, CategoryId, BrandId, ThumbnailImage, MainImage, Images)
VALUES (N'Loa Bluetooth JBL Charge 5 40W IP67', 'PROD-JBLCHARGE5', 'jbl-charge-5', N'Loa JBL Charge 5 âm thanh JBL Original Pro Sound sống động với loa woofer riêng biệt, công suất 40W RMS, chống nước chống bụi IP67 chuẩn quân đội và hỗ trợ sạc ngược pin cho điện thoại.', N'{"Công suất":"40W RMS (30W Woofer + 10W Tweeter)","Kháng nước":"IP67 waterproof & dustproof","Thời lượng pin":"20 giờ phát liên tục","Tính năng":"PartyBoost kết nối nhiều loa, Powerbank sạc ngược"}', 3490000, 3990000, 0, 0, 1, 0, GETDATE(), GETDATE(), 22, 12, 'https://images.unsplash.com/photo-1545454675-3531b543be5d?q=80&w=800&auto=format&fit=crop', 'https://images.unsplash.com/photo-1545454675-3531b543be5d?q=80&w=800&auto=format&fit=crop', N'["https://images.unsplash.com/photo-1545454675-3531b543be5d?q=80&w=800&auto=format&fit=crop","https://images.unsplash.com/photo-1545454675-3531b543be5d?q=80&w=800&auto=format&fit=crop"]');
SET @ProdId = SCOPE_IDENTITY();

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Màu Đen (Black)', 'JBLCHARGE5-BLK', 3490000, 40, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Đen"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 40, 40, N'Cái', 2722200, DATEADD(day, -10, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 40 WHERE Id = @ProdId;

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Màu Xanh Dương (Blue)', 'JBLCHARGE5-BLU', 3490000, 30, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Xanh Dương"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 30, 30, N'Cái', 2722200, DATEADD(day, -15, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 30 WHERE Id = @ProdId;

-- Product: Sạc dự phòng Anker 737 Power Bank (Prime 24,000mAh 140W)
INSERT INTO Products (Name, ProductCode, Slug, Description, Specs, BasePrice, OriginalPrice, TotalStock, ReservedStock, IsActive, IsFeatured, CreatedAt, UpdatedAt, CategoryId, BrandId, ThumbnailImage, MainImage, Images)
VALUES (N'Sạc dự phòng Anker 737 Power Bank (Prime 24,000mAh 140W)', 'PROD-ANKER737', 'anker-737-power-bank-140w', N'Anker 737 PowerBank dung lượng siêu lớn 24.000mAh công nghệ sạc nhanh PD 3.1 140W hai chiều sạc mượt cả MacBook Pro, trang bị màn hình kĩ thuật số hiển thị công suất và nhiệt độ pin tức thì.', N'{"Dung lượng":"24,000mAh / 86.4Wh","Công suất ra":"Tối đa 140W (USB-C1/C2)","Màn hình":"Smart Digital Display","Cổng sạc":"2 USB-C, 1 USB-A"}', 2490000, 2890000, 0, 0, 1, 1, GETDATE(), GETDATE(), 23, 10, 'https://images.unsplash.com/photo-1609592424089-980f55c5df38?q=80&w=800&auto=format&fit=crop', 'https://images.unsplash.com/photo-1609592424089-980f55c5df38?q=80&w=800&auto=format&fit=crop', N'["https://images.unsplash.com/photo-1609592424089-980f55c5df38?q=80&w=800&auto=format&fit=crop","https://images.unsplash.com/photo-1609592424089-980f55c5df38?q=80&w=800&auto=format&fit=crop"]');
SET @ProdId = SCOPE_IDENTITY();

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Màu Đen Xám 140W', 'ANKER737-24K-140W', 2490000, 50, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Đen Xám","Dung lượng":"24000mAh"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 50, 50, N'Cái', 1942200, DATEADD(day, -10, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 50 WHERE Id = @ProdId;

-- Product: Củ sạc nhanh Anker 511 Nano 3 30W Type-C GaN
INSERT INTO Products (Name, ProductCode, Slug, Description, Specs, BasePrice, OriginalPrice, TotalStock, ReservedStock, IsActive, IsFeatured, CreatedAt, UpdatedAt, CategoryId, BrandId, ThumbnailImage, MainImage, Images)
VALUES (N'Củ sạc nhanh Anker 511 Nano 3 30W Type-C GaN', 'PROD-ANKERNANO3', 'anker-nano-3-30w', N'Củ sạc Anker Nano 3 30W siêu nhỏ gọn gấp 70% củ sạc thông thường nhờ công nghệ GaN, sạc nhanh chuẩn cho iPhone 15/16 Pro Max và iPad Air/Pro.', N'{"Công suất":"30W Power Delivery","Công nghệ":"GaN (Gallium Nitride)","Chân sạc":"Gập gọn 90 độ"}', 390000, 450000, 0, 0, 1, 0, GETDATE(), GETDATE(), 24, 10, 'https://images.unsplash.com/photo-1583863788434-e58a36330cf0?q=80&w=800&auto=format&fit=crop', 'https://images.unsplash.com/photo-1583863788434-e58a36330cf0?q=80&w=800&auto=format&fit=crop', N'["https://images.unsplash.com/photo-1583863788434-e58a36330cf0?q=80&w=800&auto=format&fit=crop","https://images.unsplash.com/photo-1583863788434-e58a36330cf0?q=80&w=800&auto=format&fit=crop"]');
SET @ProdId = SCOPE_IDENTITY();

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Màu Trắng / 30W', 'ANKER-NANO3-30W-WHT', 390000, 100, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Trắng","Công suất":"30W"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 100, 100, N'Cái', 304200, DATEADD(day, -10, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 100 WHERE Id = @ProdId;

-- Product: Ốp lưng Spigen Ultra Hybrid MagFit iPhone 16 Pro Max
INSERT INTO Products (Name, ProductCode, Slug, Description, Specs, BasePrice, OriginalPrice, TotalStock, ReservedStock, IsActive, IsFeatured, CreatedAt, UpdatedAt, CategoryId, BrandId, ThumbnailImage, MainImage, Images)
VALUES (N'Ốp lưng Spigen Ultra Hybrid MagFit iPhone 16 Pro Max', 'PROD-SPIGEN16PM', 'spigen-ultra-hybrid-magfit-ip16pm', N'Ốp lưng Spigen Ultra Hybrid đạt chứng nhận chống sốc chuẩn quân đội Mỹ Air Cushion Technology, viền dẻo TPU lưng cứng PC trong suốt chống ố vàng và tích hợp vòng nam châm MagSafe mạnh mẽ.', N'{"Chất liệu":"Lưng Polycarbonate cứng + Viền TPU dẻo","Tính năng":"Khung nam châm MagSafe, Công nghệ đệm khí Air Cushion"}', 690000, 790000, 0, 0, 1, 0, GETDATE(), GETDATE(), 25, 17, 'https://images.unsplash.com/photo-1601784551446-20c9e07cdbdb?q=80&w=800&auto=format&fit=crop', 'https://images.unsplash.com/photo-1601784551446-20c9e07cdbdb?q=80&w=800&auto=format&fit=crop', N'["https://images.unsplash.com/photo-1601784551446-20c9e07cdbdb?q=80&w=800&auto=format&fit=crop","https://images.unsplash.com/photo-1601784551446-20c9e07cdbdb?q=80&w=800&auto=format&fit=crop"]');
SET @ProdId = SCOPE_IDENTITY();

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Trong Suốt (White Clear)', 'SPG-IP16PM-CLR', 690000, 70, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Trong Suốt"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 70, 70, N'Cái', 538200, DATEADD(day, -10, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 70 WHERE Id = @ProdId;

-- Product: Chuột không dây Logitech MX Master 3S Quiet Clicks 8K DPI
INSERT INTO Products (Name, ProductCode, Slug, Description, Specs, BasePrice, OriginalPrice, TotalStock, ReservedStock, IsActive, IsFeatured, CreatedAt, UpdatedAt, CategoryId, BrandId, ThumbnailImage, MainImage, Images)
VALUES (N'Chuột không dây Logitech MX Master 3S Quiet Clicks 8K DPI', 'PROD-MXMASTER3S', 'logitech-mx-master-3s', N'Logitech MX Master 3S chuột không dây cao cấp nhất cho công việc với nút click siêu êm Quiet Clicks 90%, con cuộn MagSpeed cuộn 1000 dòng/giây và cảm biến 8000 DPI dùng tốt trên mặt kính.', N'{"Cảm biến":"Darkfield 8000 DPI (hoạt động trên kính)","Nút bấm":"Quiet Clicks yên tĩnh","Con cuộn":"MagSpeed cuộn từ tính","Kết nối":"Logi Bolt & Bluetooth (3 thiết bị)"}', 2290000, 2590000, 0, 0, 1, 1, GETDATE(), GETDATE(), 26, 16, 'https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?q=80&w=800&auto=format&fit=crop', 'https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?q=80&w=800&auto=format&fit=crop', N'["https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?q=80&w=800&auto=format&fit=crop","https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?q=80&w=800&auto=format&fit=crop"]');
SET @ProdId = SCOPE_IDENTITY();

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Màu Đen Graphpas', 'MXM3S-BLK', 2290000, 40, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Đen Graphite"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 40, 40, N'Cái', 1786200, DATEADD(day, -10, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 40 WHERE Id = @ProdId;

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Màu Xám Pale Gray', 'MXM3S-GRY', 2290000, 30, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Màu sắc":"Xám Nhạt"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 30, 30, N'Cái', 1786200, DATEADD(day, -15, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 30 WHERE Id = @ProdId;

-- Product: SSD Di Động SanDisk Extreme Portable 1TB V2 1050MB/s
INSERT INTO Products (Name, ProductCode, Slug, Description, Specs, BasePrice, OriginalPrice, TotalStock, ReservedStock, IsActive, IsFeatured, CreatedAt, UpdatedAt, CategoryId, BrandId, ThumbnailImage, MainImage, Images)
VALUES (N'SSD Di Động SanDisk Extreme Portable 1TB V2 1050MB/s', 'PROD-SANDISK1TB', 'sandisk-extreme-portable-1tb-v2', N'Ổ cứng SSD di động SanDisk Extreme V2 dung lượng 1TB tốc độ đọc 1050MB/s, chuẩn chống nước chống bụi IP55, rơi vỡ từ độ cao 2 mét an toàn cho nhiếp ảnh gia và quay phim.', N'{"Dung lượng":"1TB NVMe SSD","Tốc độ đọc/ghi":"1050MB/s / 1000MB/s","Độ bền":"IP55 water/dust resistance, Chống rơi 2m","Giao tiếp":"USB 3.2 Gen 2 Type-C"}', 2690000, 2990000, 0, 0, 1, 0, GETDATE(), GETDATE(), 27, 14, 'https://images.unsplash.com/photo-1544652478-6653e09f18a2?q=80&w=800&auto=format&fit=crop', 'https://images.unsplash.com/photo-1544652478-6653e09f18a2?q=80&w=800&auto=format&fit=crop', N'["https://images.unsplash.com/photo-1544652478-6653e09f18a2?q=80&w=800&auto=format&fit=crop","https://images.unsplash.com/photo-1544652478-6653e09f18a2?q=80&w=800&auto=format&fit=crop"]');
SET @ProdId = SCOPE_IDENTITY();

INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'Màu Đen Viền Cam / 1TB', 'SD-EXT1TB-BLK', 2690000, 35, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'{"Dung lượng":"1TB","Màu sắc":"Đen Cam"}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, 35, 35, N'Cái', 2098200, DATEADD(day, -10, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + 35 WHERE Id = @ProdId;

-- ==========================================
-- 5. SEED BANNERS
-- ==========================================
PRINT N'---> Seeding Banners...';
INSERT INTO Banners (ImageUrl, LinkUrl, Type, IsActive, Position, IsDraft, CreatedAt)
VALUES ('https://images.unsplash.com/photo-1695048133142-1a20484d2569?q=80&w=1600&auto=format&fit=crop', '/category/iphone', 'Slider', 1, 1, 0, GETDATE());
INSERT INTO Banners (ImageUrl, LinkUrl, Type, IsActive, Position, IsDraft, CreatedAt)
VALUES ('https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?q=80&w=1600&auto=format&fit=crop', '/product/samsung-galaxy-s24-ultra', 'Slider', 1, 2, 0, GETDATE());
INSERT INTO Banners (ImageUrl, LinkUrl, Type, IsActive, Position, IsDraft, CreatedAt)
VALUES ('https://images.unsplash.com/photo-1517336714731-489689fd1ca8?q=80&w=1600&auto=format&fit=crop', '/category/macbook', 'Slider', 1, 3, 0, GETDATE());
INSERT INTO Banners (ImageUrl, LinkUrl, Type, IsActive, Position, IsDraft, CreatedAt)
VALUES ('https://images.unsplash.com/photo-1505740420928-5e560c06d30e?q=80&w=1600&auto=format&fit=crop', '/category/tai-nghe-am-thanh', 'Slider', 1, 4, 0, GETDATE());
INSERT INTO Banners (ImageUrl, LinkUrl, Type, IsActive, Position, IsDraft, CreatedAt)
VALUES ('https://images.unsplash.com/photo-1607082348824-0a96f2a4b9da?q=80&w=1200&auto=format&fit=crop', '/promotions', 'Top', 1, 1, 0, GETDATE());
INSERT INTO Banners (ImageUrl, LinkUrl, Type, IsActive, Position, IsDraft, CreatedAt)
VALUES ('https://images.unsplash.com/photo-1526738549149-8e07eca6c147?q=80&w=600&auto=format&fit=crop', '/category/phu-kien-dien-thoai', 'Right', 1, 1, 0, GETDATE());

-- ==========================================
-- 6. SEED PROMOTIONS
-- ==========================================
PRINT N'---> Seeding Promotions...';
INSERT INTO Promotions (Code, DiscountType, DiscountValue, StartDate, EndDate, IsActive, UsageLimit, UsedCount, MinOrderAmount, MaxDiscountAmount, MaxPerUser)
VALUES ('WELCOME100K', 'Fixed', 100000, DATEADD(day, -10, GETDATE()), DATEADD(day, 60, GETDATE()), 1, 500, 42, 1000000, 100000, 2);
INSERT INTO Promotions (Code, DiscountType, DiscountValue, StartDate, EndDate, IsActive, UsageLimit, UsedCount, MinOrderAmount, MaxDiscountAmount, MaxPerUser)
VALUES ('TECHFEST500K', 'Fixed', 500000, DATEADD(day, -15, GETDATE()), DATEADD(day, 30, GETDATE()), 1, 200, 18, 10000000, 500000, 2);
INSERT INTO Promotions (Code, DiscountType, DiscountValue, StartDate, EndDate, IsActive, UsageLimit, UsedCount, MinOrderAmount, MaxDiscountAmount, MaxPerUser)
VALUES ('FREESHIP30K', 'Fixed', 30000, DATEADD(day, -20, GETDATE()), DATEADD(day, 90, GETDATE()), 1, 1000, 154, 300000, 30000, 2);
INSERT INTO Promotions (Code, DiscountType, DiscountValue, StartDate, EndDate, IsActive, UsageLimit, UsedCount, MinOrderAmount, MaxDiscountAmount, MaxPerUser)
VALUES ('SUPERDEAL10', 'Percentage', 10, DATEADD(day, -5, GETDATE()), DATEADD(day, 45, GETDATE()), 1, 300, 67, 2000000, 1000000, 2);

-- ==========================================
-- 7. SEED USERS
-- ==========================================
PRINT N'---> Seeding Sample Users...';
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'an.nguyen@gmail.com')
BEGIN
    INSERT INTO Users (Id, Username, Email, PasswordHash, Role, IsActive, IsEmailVerified, FailedLoginCount, CreatedAt, RewardPoints, AccumulatedPoints)
    VALUES ('A1111111-1111-1111-1111-111111111111', 'nguyenvanan', 'an.nguyen@gmail.com', 'AQAAAAEAACcQAAAAELhZ7uyPbdI/P5HnELm9jlcFgQAoKFKXUvnXUC/bsWY7NK8pjLvM1pBBh31Yz1Ya4w==', 'User', 1, 1, 0, DATEADD(day, -30, GETDATE()), 150, 450);
END
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'mai.tran@gmail.com')
BEGIN
    INSERT INTO Users (Id, Username, Email, PasswordHash, Role, IsActive, IsEmailVerified, FailedLoginCount, CreatedAt, RewardPoints, AccumulatedPoints)
    VALUES ('B2222222-2222-2222-2222-222222222222', 'tranthimai', 'mai.tran@gmail.com', 'AQAAAAEAACcQAAAAELhZ7uyPbdI/P5HnELm9jlcFgQAoKFKXUvnXUC/bsWY7NK8pjLvM1pBBh31Yz1Ya4w==', 'User', 1, 1, 0, DATEADD(day, -30, GETDATE()), 300, 1200);
END
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'nam.le@gmail.com')
BEGIN
    INSERT INTO Users (Id, Username, Email, PasswordHash, Role, IsActive, IsEmailVerified, FailedLoginCount, CreatedAt, RewardPoints, AccumulatedPoints)
    VALUES ('C3333333-3333-3333-3333-333333333333', 'lehoangnam', 'nam.le@gmail.com', 'AQAAAAEAACcQAAAAELhZ7uyPbdI/P5HnELm9jlcFgQAoKFKXUvnXUC/bsWY7NK8pjLvM1pBBh31Yz1Ya4w==', 'User', 1, 1, 0, DATEADD(day, -30, GETDATE()), 50, 200);
END
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'tuan.pham@gmail.com')
BEGIN
    INSERT INTO Users (Id, Username, Email, PasswordHash, Role, IsActive, IsEmailVerified, FailedLoginCount, CreatedAt, RewardPoints, AccumulatedPoints)
    VALUES ('D4444444-4444-4444-4444-444444444444', 'phamminhtuan', 'tuan.pham@gmail.com', 'AQAAAAEAACcQAAAAELhZ7uyPbdI/P5HnELm9jlcFgQAoKFKXUvnXUC/bsWY7NK8pjLvM1pBBh31Yz1Ya4w==', 'User', 1, 1, 0, DATEADD(day, -30, GETDATE()), 500, 2500);
END

-- ==========================================
-- 8. SEED REVIEWS
-- ==========================================
PRINT N'---> Seeding Product Reviews...';
DECLARE @RevProdId INT;

SELECT TOP 1 @RevProdId = Id FROM Products ORDER BY NEWID();
INSERT INTO Reviews (Rating, Comment, CreatedAt, AdminReply, RepliedAt, IsHidden, ProductId, UserId)
VALUES (5, N'Máy giao siêu nhanh, đóng gói cẩn thận 2 lớp chống sốc. Dùng mượt mà không có điểm gì chê!', DATEADD(day, -3, GETDATE()), N'Dạ Shop xin cảm ơn quý khách đã tin tưởng và ủng hộ ạ!', DATEADD(day, -1, GETDATE()), 0, @RevProdId, 'A1111111-1111-1111-1111-111111111111');

SELECT TOP 1 @RevProdId = Id FROM Products ORDER BY NEWID();
INSERT INTO Reviews (Rating, Comment, CreatedAt, AdminReply, RepliedAt, IsHidden, ProductId, UserId)
VALUES (5, N'Chụp ảnh quá đỉnh luôn, màu sắc chân thực sắc nét. Hàng chính hãng VNA chuẩn seal!', DATEADD(day, -4, GETDATE()), N'Cảm ơn bạn nhiều nha! Chúc bạn có trải nghiệm tuyệt vời cùng sản phẩm!', DATEADD(day, -2, GETDATE()), 0, @RevProdId, 'A1111111-1111-1111-1111-111111111111');

SELECT TOP 1 @RevProdId = Id FROM Products ORDER BY NEWID();
INSERT INTO Reviews (Rating, Comment, CreatedAt, AdminReply, RepliedAt, IsHidden, ProductId, UserId)
VALUES (4, N'Sản phẩm đẹp, pin dùng được 1.5 ngày thoải mái. Nhân viên tư vấn nhiệt tình.', DATEADD(day, -5, GETDATE()), NULL, NULL, 0, @RevProdId, 'A1111111-1111-1111-1111-111111111111');

SELECT TOP 1 @RevProdId = Id FROM Products ORDER BY NEWID();
INSERT INTO Reviews (Rating, Comment, CreatedAt, AdminReply, RepliedAt, IsHidden, ProductId, UserId)
VALUES (5, N'Chất lượng tuyệt vời trong tầm giá, sạc nhanh và không bị nóng máy.', DATEADD(day, -6, GETDATE()), NULL, NULL, 0, @RevProdId, 'A1111111-1111-1111-1111-111111111111');

SELECT TOP 1 @RevProdId = Id FROM Products ORDER BY NEWID();
INSERT INTO Reviews (Rating, Comment, CreatedAt, AdminReply, RepliedAt, IsHidden, ProductId, UserId)
VALUES (5, N'Đã mua sản phẩm thứ 3 ở shop, lần nào cũng vô cùng hài lòng từ dịch vụ tới hậu mãi.', DATEADD(day, -7, GETDATE()), N'Shop luôn sẵn sàng hỗ trợ bạn ạ, cảm ơn sự đồng hành của bạn!', DATEADD(day, -5, GETDATE()), 0, @RevProdId, 'A1111111-1111-1111-1111-111111111111');

SELECT TOP 1 @RevProdId = Id FROM Products ORDER BY NEWID();
INSERT INTO Reviews (Rating, Comment, CreatedAt, AdminReply, RepliedAt, IsHidden, ProductId, UserId)
VALUES (5, N'Máy giao siêu nhanh, đóng gói cẩn thận 2 lớp chống sốc. Dùng mượt mà không có điểm gì chê!', DATEADD(day, -6, GETDATE()), N'Dạ Shop xin cảm ơn quý khách đã tin tưởng và ủng hộ ạ!', DATEADD(day, -1, GETDATE()), 0, @RevProdId, 'B2222222-2222-2222-2222-222222222222');

SELECT TOP 1 @RevProdId = Id FROM Products ORDER BY NEWID();
INSERT INTO Reviews (Rating, Comment, CreatedAt, AdminReply, RepliedAt, IsHidden, ProductId, UserId)
VALUES (5, N'Chụp ảnh quá đỉnh luôn, màu sắc chân thực sắc nét. Hàng chính hãng VNA chuẩn seal!', DATEADD(day, -7, GETDATE()), N'Cảm ơn bạn nhiều nha! Chúc bạn có trải nghiệm tuyệt vời cùng sản phẩm!', DATEADD(day, -2, GETDATE()), 0, @RevProdId, 'B2222222-2222-2222-2222-222222222222');

SELECT TOP 1 @RevProdId = Id FROM Products ORDER BY NEWID();
INSERT INTO Reviews (Rating, Comment, CreatedAt, AdminReply, RepliedAt, IsHidden, ProductId, UserId)
VALUES (4, N'Sản phẩm đẹp, pin dùng được 1.5 ngày thoải mái. Nhân viên tư vấn nhiệt tình.', DATEADD(day, -8, GETDATE()), NULL, NULL, 0, @RevProdId, 'B2222222-2222-2222-2222-222222222222');

SELECT TOP 1 @RevProdId = Id FROM Products ORDER BY NEWID();
INSERT INTO Reviews (Rating, Comment, CreatedAt, AdminReply, RepliedAt, IsHidden, ProductId, UserId)
VALUES (5, N'Chất lượng tuyệt vời trong tầm giá, sạc nhanh và không bị nóng máy.', DATEADD(day, -9, GETDATE()), NULL, NULL, 0, @RevProdId, 'B2222222-2222-2222-2222-222222222222');

SELECT TOP 1 @RevProdId = Id FROM Products ORDER BY NEWID();
INSERT INTO Reviews (Rating, Comment, CreatedAt, AdminReply, RepliedAt, IsHidden, ProductId, UserId)
VALUES (5, N'Đã mua sản phẩm thứ 3 ở shop, lần nào cũng vô cùng hài lòng từ dịch vụ tới hậu mãi.', DATEADD(day, -10, GETDATE()), N'Shop luôn sẵn sàng hỗ trợ bạn ạ, cảm ơn sự đồng hành của bạn!', DATEADD(day, -5, GETDATE()), 0, @RevProdId, 'B2222222-2222-2222-2222-222222222222');

SELECT TOP 1 @RevProdId = Id FROM Products ORDER BY NEWID();
INSERT INTO Reviews (Rating, Comment, CreatedAt, AdminReply, RepliedAt, IsHidden, ProductId, UserId)
VALUES (5, N'Máy giao siêu nhanh, đóng gói cẩn thận 2 lớp chống sốc. Dùng mượt mà không có điểm gì chê!', DATEADD(day, -9, GETDATE()), N'Dạ Shop xin cảm ơn quý khách đã tin tưởng và ủng hộ ạ!', DATEADD(day, -1, GETDATE()), 0, @RevProdId, 'C3333333-3333-3333-3333-333333333333');

SELECT TOP 1 @RevProdId = Id FROM Products ORDER BY NEWID();
INSERT INTO Reviews (Rating, Comment, CreatedAt, AdminReply, RepliedAt, IsHidden, ProductId, UserId)
VALUES (5, N'Chụp ảnh quá đỉnh luôn, màu sắc chân thực sắc nét. Hàng chính hãng VNA chuẩn seal!', DATEADD(day, -10, GETDATE()), N'Cảm ơn bạn nhiều nha! Chúc bạn có trải nghiệm tuyệt vời cùng sản phẩm!', DATEADD(day, -2, GETDATE()), 0, @RevProdId, 'C3333333-3333-3333-3333-333333333333');

SELECT TOP 1 @RevProdId = Id FROM Products ORDER BY NEWID();
INSERT INTO Reviews (Rating, Comment, CreatedAt, AdminReply, RepliedAt, IsHidden, ProductId, UserId)
VALUES (4, N'Sản phẩm đẹp, pin dùng được 1.5 ngày thoải mái. Nhân viên tư vấn nhiệt tình.', DATEADD(day, -11, GETDATE()), NULL, NULL, 0, @RevProdId, 'C3333333-3333-3333-3333-333333333333');

SELECT TOP 1 @RevProdId = Id FROM Products ORDER BY NEWID();
INSERT INTO Reviews (Rating, Comment, CreatedAt, AdminReply, RepliedAt, IsHidden, ProductId, UserId)
VALUES (5, N'Chất lượng tuyệt vời trong tầm giá, sạc nhanh và không bị nóng máy.', DATEADD(day, -12, GETDATE()), NULL, NULL, 0, @RevProdId, 'C3333333-3333-3333-3333-333333333333');

SELECT TOP 1 @RevProdId = Id FROM Products ORDER BY NEWID();
INSERT INTO Reviews (Rating, Comment, CreatedAt, AdminReply, RepliedAt, IsHidden, ProductId, UserId)
VALUES (5, N'Đã mua sản phẩm thứ 3 ở shop, lần nào cũng vô cùng hài lòng từ dịch vụ tới hậu mãi.', DATEADD(day, -13, GETDATE()), N'Shop luôn sẵn sàng hỗ trợ bạn ạ, cảm ơn sự đồng hành của bạn!', DATEADD(day, -5, GETDATE()), 0, @RevProdId, 'C3333333-3333-3333-3333-333333333333');

SELECT TOP 1 @RevProdId = Id FROM Products ORDER BY NEWID();
INSERT INTO Reviews (Rating, Comment, CreatedAt, AdminReply, RepliedAt, IsHidden, ProductId, UserId)
VALUES (5, N'Máy giao siêu nhanh, đóng gói cẩn thận 2 lớp chống sốc. Dùng mượt mà không có điểm gì chê!', DATEADD(day, -12, GETDATE()), N'Dạ Shop xin cảm ơn quý khách đã tin tưởng và ủng hộ ạ!', DATEADD(day, -1, GETDATE()), 0, @RevProdId, 'D4444444-4444-4444-4444-444444444444');

SELECT TOP 1 @RevProdId = Id FROM Products ORDER BY NEWID();
INSERT INTO Reviews (Rating, Comment, CreatedAt, AdminReply, RepliedAt, IsHidden, ProductId, UserId)
VALUES (5, N'Chụp ảnh quá đỉnh luôn, màu sắc chân thực sắc nét. Hàng chính hãng VNA chuẩn seal!', DATEADD(day, -13, GETDATE()), N'Cảm ơn bạn nhiều nha! Chúc bạn có trải nghiệm tuyệt vời cùng sản phẩm!', DATEADD(day, -2, GETDATE()), 0, @RevProdId, 'D4444444-4444-4444-4444-444444444444');

SELECT TOP 1 @RevProdId = Id FROM Products ORDER BY NEWID();
INSERT INTO Reviews (Rating, Comment, CreatedAt, AdminReply, RepliedAt, IsHidden, ProductId, UserId)
VALUES (4, N'Sản phẩm đẹp, pin dùng được 1.5 ngày thoải mái. Nhân viên tư vấn nhiệt tình.', DATEADD(day, -14, GETDATE()), NULL, NULL, 0, @RevProdId, 'D4444444-4444-4444-4444-444444444444');

SELECT TOP 1 @RevProdId = Id FROM Products ORDER BY NEWID();
INSERT INTO Reviews (Rating, Comment, CreatedAt, AdminReply, RepliedAt, IsHidden, ProductId, UserId)
VALUES (5, N'Chất lượng tuyệt vời trong tầm giá, sạc nhanh và không bị nóng máy.', DATEADD(day, -15, GETDATE()), NULL, NULL, 0, @RevProdId, 'D4444444-4444-4444-4444-444444444444');

SELECT TOP 1 @RevProdId = Id FROM Products ORDER BY NEWID();
INSERT INTO Reviews (Rating, Comment, CreatedAt, AdminReply, RepliedAt, IsHidden, ProductId, UserId)
VALUES (5, N'Đã mua sản phẩm thứ 3 ở shop, lần nào cũng vô cùng hài lòng từ dịch vụ tới hậu mãi.', DATEADD(day, -16, GETDATE()), N'Shop luôn sẵn sàng hỗ trợ bạn ạ, cảm ơn sự đồng hành của bạn!', DATEADD(day, -5, GETDATE()), 0, @RevProdId, 'D4444444-4444-4444-4444-444444444444');

-- ==========================================
-- 9. SEED SAMPLE ORDERS & REVENUE DATA
-- ==========================================
PRINT N'---> Seeding Sample Orders & OrderItems...';
DECLARE @OrdId INT;
DECLARE @OrdVarId INT;
DECLARE @OrdVarPrice DECIMAL(18,2);


-- Sample Order 1
INSERT INTO Orders (TotalPrice, CreatedAt, UserId, OrderStatusId, ReceiverName, ReceiverPhone, ShippingAddressLine, ShippingWard, ShippingProvince, PaymentMethod, PointsEarned, PointsRedeemed, DiscountFromPoints, AddonDiscountAmount)
VALUES (0, DATEADD(day, -25, GETDATE()), 'A1111111-1111-1111-1111-111111111111', 4, N'Nguyễn Văn An', '0903123456', N'123 Nguyễn Huệ, Phường Bến Nghé, Quận 1', N'Phường Bến Nghé', N'Hồ Chí Minh', 'Stripe', 100, 0, 0, 0);
SET @OrdId = SCOPE_IDENTITY();

-- Order Item 1
SELECT TOP 1 @OrdVarId = Id, @OrdVarPrice = Price FROM ProductVariants ORDER BY NEWID();
INSERT INTO OrderItems (Quantity, PriceAtPurchase, OrderId, VariantId, CampaignDiscountAmount, IsAddon, WarrantyPrice, InspectionStatus)
VALUES (1, @OrdVarPrice, @OrdId, @OrdVarId, 0, 0, 0, 'NOT_REQUIRED');

-- Update Order Total
UPDATE Orders SET TotalPrice = (SELECT SUM(Quantity * PriceAtPurchase) FROM OrderItems WHERE OrderId = @OrdId) WHERE Id = @OrdId;

-- Sample Order 2
INSERT INTO Orders (TotalPrice, CreatedAt, UserId, OrderStatusId, ReceiverName, ReceiverPhone, ShippingAddressLine, ShippingWard, ShippingProvince, PaymentMethod, PointsEarned, PointsRedeemed, DiscountFromPoints, AddonDiscountAmount)
VALUES (0, DATEADD(day, -20, GETDATE()), 'B2222222-2222-2222-2222-222222222222', 4, N'Trần Thị Mai', '0918234567', N'456 Điện Biên Phủ, Phường Đa Kao, Quận 1', N'Phường Bến Nghé', N'Hồ Chí Minh', 'VnPay', 100, 0, 0, 0);
SET @OrdId = SCOPE_IDENTITY();

-- Order Item 1
SELECT TOP 1 @OrdVarId = Id, @OrdVarPrice = Price FROM ProductVariants ORDER BY NEWID();
INSERT INTO OrderItems (Quantity, PriceAtPurchase, OrderId, VariantId, CampaignDiscountAmount, IsAddon, WarrantyPrice, InspectionStatus)
VALUES (1, @OrdVarPrice, @OrdId, @OrdVarId, 0, 0, 0, 'NOT_REQUIRED');

-- Update Order Total
UPDATE Orders SET TotalPrice = (SELECT SUM(Quantity * PriceAtPurchase) FROM OrderItems WHERE OrderId = @OrdId) WHERE Id = @OrdId;

-- Sample Order 3
INSERT INTO Orders (TotalPrice, CreatedAt, UserId, OrderStatusId, ReceiverName, ReceiverPhone, ShippingAddressLine, ShippingWard, ShippingProvince, PaymentMethod, PointsEarned, PointsRedeemed, DiscountFromPoints, AddonDiscountAmount)
VALUES (0, DATEADD(day, -15, GETDATE()), 'C3333333-3333-3333-3333-333333333333', 4, N'Lê Hoàng Nam', '0987654321', N'789 Lạc Long Quân, Phường 3, Quận 11', N'Phường Bến Nghé', N'Hồ Chí Minh', 'COD', 100, 0, 0, 0);
SET @OrdId = SCOPE_IDENTITY();

-- Order Item 1
SELECT TOP 1 @OrdVarId = Id, @OrdVarPrice = Price FROM ProductVariants ORDER BY NEWID();
INSERT INTO OrderItems (Quantity, PriceAtPurchase, OrderId, VariantId, CampaignDiscountAmount, IsAddon, WarrantyPrice, InspectionStatus)
VALUES (1, @OrdVarPrice, @OrdId, @OrdVarId, 0, 0, 0, 'NOT_REQUIRED');

-- Update Order Total
UPDATE Orders SET TotalPrice = (SELECT SUM(Quantity * PriceAtPurchase) FROM OrderItems WHERE OrderId = @OrdId) WHERE Id = @OrdId;

-- Sample Order 4
INSERT INTO Orders (TotalPrice, CreatedAt, UserId, OrderStatusId, ReceiverName, ReceiverPhone, ShippingAddressLine, ShippingWard, ShippingProvince, PaymentMethod, PointsEarned, PointsRedeemed, DiscountFromPoints, AddonDiscountAmount)
VALUES (0, DATEADD(day, -3, GETDATE()), 'D4444444-4444-4444-4444-444444444444', 3, N'Phạm Minh Tuấn', '0978112233', N'12 Hoàng Diệu, Phường Phước Ninh, Hải Châu', N'Phường Bến Nghé', N'Hồ Chí Minh', 'COD', 100, 0, 0, 0);
SET @OrdId = SCOPE_IDENTITY();

-- Order Item 1
SELECT TOP 1 @OrdVarId = Id, @OrdVarPrice = Price FROM ProductVariants ORDER BY NEWID();
INSERT INTO OrderItems (Quantity, PriceAtPurchase, OrderId, VariantId, CampaignDiscountAmount, IsAddon, WarrantyPrice, InspectionStatus)
VALUES (1, @OrdVarPrice, @OrdId, @OrdVarId, 0, 0, 0, 'NOT_REQUIRED');

-- Update Order Total
UPDATE Orders SET TotalPrice = (SELECT SUM(Quantity * PriceAtPurchase) FROM OrderItems WHERE OrderId = @OrdId) WHERE Id = @OrdId;

-- Sample Order 5
INSERT INTO Orders (TotalPrice, CreatedAt, UserId, OrderStatusId, ReceiverName, ReceiverPhone, ShippingAddressLine, ShippingWard, ShippingProvince, PaymentMethod, PointsEarned, PointsRedeemed, DiscountFromPoints, AddonDiscountAmount)
VALUES (0, DATEADD(day, -1, GETDATE()), 'A1111111-1111-1111-1111-111111111111', 2, N'Vũ Hoàng Linh', '0933445566', N'88 Nguyễn Chí Thanh, Phường Láng Hạ, Đống Đa', N'Phường Bến Nghé', N'Hồ Chí Minh', 'VnPay', 100, 0, 0, 0);
SET @OrdId = SCOPE_IDENTITY();

-- Order Item 1
SELECT TOP 1 @OrdVarId = Id, @OrdVarPrice = Price FROM ProductVariants ORDER BY NEWID();
INSERT INTO OrderItems (Quantity, PriceAtPurchase, OrderId, VariantId, CampaignDiscountAmount, IsAddon, WarrantyPrice, InspectionStatus)
VALUES (1, @OrdVarPrice, @OrdId, @OrdVarId, 0, 0, 0, 'NOT_REQUIRED');

-- Update Order Total
UPDATE Orders SET TotalPrice = (SELECT SUM(Quantity * PriceAtPurchase) FROM OrderItems WHERE OrderId = @OrdId) WHERE Id = @OrdId;

-- Sample Order 6
INSERT INTO Orders (TotalPrice, CreatedAt, UserId, OrderStatusId, ReceiverName, ReceiverPhone, ShippingAddressLine, ShippingWard, ShippingProvince, PaymentMethod, PointsEarned, PointsRedeemed, DiscountFromPoints, AddonDiscountAmount)
VALUES (0, DATEADD(day, -0, GETDATE()), 'B2222222-2222-2222-2222-222222222222', 1, N'Nguyễn Văn An', '0903123456', N'123 Nguyễn Huệ, Phường Bến Nghé, Quận 1', N'Phường Bến Nghé', N'Hồ Chí Minh', 'COD', 100, 0, 0, 0);
SET @OrdId = SCOPE_IDENTITY();

-- Order Item 1
SELECT TOP 1 @OrdVarId = Id, @OrdVarPrice = Price FROM ProductVariants ORDER BY NEWID();
INSERT INTO OrderItems (Quantity, PriceAtPurchase, OrderId, VariantId, CampaignDiscountAmount, IsAddon, WarrantyPrice, InspectionStatus)
VALUES (1, @OrdVarPrice, @OrdId, @OrdVarId, 0, 0, 0, 'NOT_REQUIRED');

-- Update Order Total
UPDATE Orders SET TotalPrice = (SELECT SUM(Quantity * PriceAtPurchase) FROM OrderItems WHERE OrderId = @OrdId) WHERE Id = @OrdId;

COMMIT TRANSACTION;
PRINT N'======================================================';
PRINT N'SUCCESS: Full Shop Seed Data applied successfully!';
PRINT N'======================================================';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT N'ERROR occurred during seeding: ' + ERROR_MESSAGE();
END CATCH
