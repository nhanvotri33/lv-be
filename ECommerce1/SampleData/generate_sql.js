const fs = require('fs');

const products = [
    // 12 Sạc dự phòng
    { cat: 12, brand: 7, name: 'Sạc dự phòng Anker PowerCore 10000mAh', slug: 'anker-powercore-10000', price: 500000, specs: '{"Dung lượng pin": "10000mAh", "Công suất": "12W", "Cổng sạc": "1 USB-A, 1 Micro USB"}', attr: '{"Màu sắc": "Đen"}' },
    { cat: 12, brand: 3, name: 'Sạc dự phòng Xiaomi Redmi 20000mAh', slug: 'xiaomi-redmi-20000', price: 450000, specs: '{"Dung lượng pin": "20000mAh", "Công suất": "18W", "Cổng sạc": "2 USB-A, 1 Type-C, 1 Micro USB"}', attr: '{"Màu sắc": "Trắng"}' },
    { cat: 12, brand: 10, name: 'Sạc dự phòng Baseus Bipow 15W 10000mAh', slug: 'baseus-bipow-15w-10000', price: 300000, specs: '{"Dung lượng pin": "10000mAh", "Công suất": "15W", "Cổng sạc": "2 USB-A, 1 Type-C"}', attr: '{"Màu sắc": "Đen"}' },

    // 13 Cáp, sạc
    { cat: 13, brand: 1, name: 'Củ sạc nhanh Apple 20W Type-C', slug: 'apple-20w-type-c', price: 550000, specs: '{"Công suất": "20W", "Cổng sạc": "1 Type-C", "Tương thích": "iPhone, iPad"}', attr: '[]' },
    { cat: 13, brand: 7, name: 'Cáp sạc Anker PowerLine III Type-C to Lightning', slug: 'anker-powerline-iii-c-to-l', price: 350000, specs: '{"Chiều dài": "0.9m", "Công suất": "Tối đa 60W", "Chất liệu": "Nhựa PVC"}', attr: '{"Màu sắc": "Trắng"}' },
    { cat: 13, brand: 2, name: 'Bộ sạc nhanh Samsung 25W Type-C', slug: 'samsung-25w-type-c', price: 400000, specs: '{"Công suất": "25W", "Cổng sạc": "1 Type-C", "Tương thích": "Galaxy S, Note"}', attr: '{"Màu sắc": "Đen"}' },

    // 14 Ốp lưng điện thoại
    { cat: 14, brand: 1, name: 'Ốp lưng iPhone 15 Pro Max Clear Case with MagSafe', slug: 'iphone-15-pm-clear-case', price: 1200000, specs: '{"Chất liệu": "Polycarbonate, Nhựa dẻo", "Tính năng": "Hỗ trợ MagSafe, Chống ố vàng"}', attr: '{"Màu sắc": "Trong suốt"}' },
    { cat: 14, brand: 2, name: 'Ốp lưng Samsung Galaxy S24 Ultra Silicone', slug: 'samsung-s24-ultra-silicone', price: 800000, specs: '{"Chất liệu": "Silicone", "Tính năng": "Chống sốc, Mềm mại"}', attr: '{"Màu sắc": "Xanh dương"}' },
    { cat: 14, brand: 10, name: 'Ốp lưng siêu mỏng Baseus Wing Case cho iPhone 14', slug: 'baseus-wing-case-ip14', price: 150000, specs: '{"Chất liệu": "Nhựa PP", "Tính năng": "Siêu mỏng 0.4mm, Nhám chống vân tay"}', attr: '{"Màu sắc": "Đen mờ"}' },

    // 15 Ốp lưng máy tính bảng
    { cat: 15, brand: 1, name: 'Bao da iPad Pro 11 inch Smart Folio', slug: 'ipad-pro-11-smart-folio', price: 2000000, specs: '{"Chất liệu": "Polyurethane", "Tính năng": "Đóng mở màn hình tự động, Dựng được nhiều góc"}', attr: '{"Màu sắc": "Trắng"}' },
    { cat: 15, brand: 2, name: 'Bao da Samsung Galaxy Tab S9 Smart Book Cover', slug: 'samsung-tab-s9-smart-cover', price: 1500000, specs: '{"Chất liệu": "Da PU", "Tính năng": "Kháng khuẩn, Có khe cắm S Pen"}', attr: '{"Màu sắc": "Đen"}' },
    { cat: 15, brand: 10, name: 'Bao da Baseus Safattach cho iPad Air 5', slug: 'baseus-safattach-ipad-air5', price: 450000, specs: '{"Chất liệu": "Da nhân tạo, PC, TPU", "Tính năng": "Gắn từ tính, Bảo vệ 360 độ"}', attr: '{"Màu sắc": "Xanh lá"}' },

    // 16 Dán màn hình
    { cat: 16, brand: 10, name: 'Kính cường lực Baseus 0.3mm cho iPhone 15', slug: 'baseus-glass-03mm-ip15', price: 150000, specs: '{"Chất liệu": "Kính cường lực", "Độ dày": "0.3mm", "Tính năng": "Chống vỡ, Chống chói"}', attr: '[]' },
    { cat: 16, brand: 7, name: 'Dán màn hình Anker GlassGuard cho iPhone 14 Pro Max', slug: 'anker-glassguard-ip14-pm', price: 250000, specs: '{"Chất liệu": "Kính Aluminosilicate", "Độ cứng": "9H", "Tính năng": "Chống xước vượt trội"}', attr: '[]' },
    { cat: 16, brand: 2, name: 'Miếng dán chống chói Samsung Galaxy S23 Ultra Anti-Reflecting', slug: 'samsung-s23u-anti-reflecting', price: 350000, specs: '{"Chất liệu": "Film PET", "Tính năng": "Chống chói, Bảo vệ vân tay"}', attr: '[]' },

    // 17 Dây đeo điện thoại
    { cat: 17, brand: 10, name: 'Dây đeo điện thoại Baseus Lanyard', slug: 'baseus-lanyard', price: 90000, specs: '{"Chất liệu": "Nylon, Hợp kim nhôm", "Độ dài": "Tùy chỉnh", "Tính năng": "Chắc chắn, Thời trang"}', attr: '{"Màu sắc": "Đen"}' },
    { cat: 17, brand: 10, name: 'Dây đeo cổ nhẫn Ringke Lanyard', slug: 'ringke-lanyard', price: 120000, specs: '{"Chất liệu": "Vải dù", "Độ dài": "40cm", "Tính năng": "Tháo lắp nhanh chóng"}', attr: '{"Màu sắc": "Xanh dương"}' },
    { cat: 17, brand: 10, name: 'Dây đeo cổ tay Spigen Wrist Strap', slug: 'spigen-wrist-strap', price: 150000, specs: '{"Chất liệu": "Dacron dệt kim", "Độ dài": "20cm", "Tính năng": "Siêu bền, Thoải mái"}', attr: '{"Màu sắc": "Xám"}' },

    // 18 Hộp đựng tai nghe
    { cat: 18, brand: 1, name: 'Hộp đựng AirPods Pro silicon', slug: 'airpods-pro-silicone-case', price: 100000, specs: '{"Chất liệu": "Silicone", "Tương thích": "AirPods Pro", "Tính năng": "Chống xước, Kèm móc khóa"}', attr: '{"Màu sắc": "Cam"}' },
    { cat: 18, brand: 2, name: 'Ốp lưng Galaxy Buds2 Pro dạng trong suốt', slug: 'galaxy-buds2-pro-clear-case', price: 150000, specs: '{"Chất liệu": "Nhựa PC trong suốt", "Tương thích": "Galaxy Buds2 Pro", "Tính năng": "Chống va đập"}', attr: '[]' },
    { cat: 18, brand: 10, name: 'Hộp đựng Baseus mỏng nhẹ cho tai nghe TWS', slug: 'baseus-tws-pouch', price: 80000, specs: '{"Chất liệu": "Vải nỉ EVA", "Tính năng": "Kéo khóa, Lưới chống sốc bên trong"}', attr: '{"Màu sắc": "Đen"}' },

    // 19 Giá đỡ điện thoại
    { cat: 19, brand: 10, name: 'Giá đỡ điện thoại để bàn Baseus Desktop Stand', slug: 'baseus-desktop-stand', price: 200000, specs: '{"Chất liệu": "Hợp kim nhôm", "Khả năng xoay": "Lên xuống 35 độ", "Tương thích": "Điện thoại 4-7 inch"}', attr: '{"Màu sắc": "Bạc"}' },
    { cat: 19, brand: 7, name: 'Giá đỡ kẹp khe gió ô tô Anker Magnetic', slug: 'anker-magnetic-car-mount', price: 450000, specs: '{"Chất liệu": "Nhựa ABS, Nam châm", "Khả năng xoay": "360 độ", "Tương thích": "Có Magsafe"}', attr: '{"Màu sắc": "Đen"}' },
    { cat: 19, brand: 3, name: 'Gậy tự sướng kèm giá đỡ tripod Xiaomi 3 chân', slug: 'xiaomi-tripod-selfie-stick', price: 300000, specs: '{"Chất liệu": "Nhựa, Nhôm", "Chiều dài tối đa": "70cm", "Kết nối": "Bluetooth"}', attr: '{"Màu sắc": "Đen"}' },

    // 20 Thẻ nhớ
    { cat: 20, brand: 10, name: 'Thẻ nhớ MicroSD SanDisk Extreme Pro 128GB', slug: 'sandisk-extreme-pro-128gb', price: 600000, specs: '{"Dung lượng": "128GB", "Tốc độ đọc": "200MB/s", "Tốc độ ghi": "90MB/s", "Chuẩn": "U3, V30, A2"}', attr: '[]', customBrand: 'SanDisk' },
    { cat: 20, brand: 2, name: 'Thẻ nhớ MicroSD Samsung EVO Plus 256GB', slug: 'samsung-evo-plus-256gb', price: 750000, specs: '{"Dung lượng": "256GB", "Tốc độ đọc": "130MB/s", "Chuẩn": "U3, V30, A2"}', attr: '[]' },
    { cat: 20, brand: 10, name: 'Thẻ nhớ MicroSD Kingston Canvas Go Plus 64GB', slug: 'kingston-canvas-go-plus-64gb', price: 300000, specs: '{"Dung lượng": "64GB", "Tốc độ đọc": "170MB/s", "Tốc độ ghi": "70MB/s", "Chuẩn": "U3, V30, A2"}', attr: '[]', customBrand: 'Kingston' },

    // 21 USB
    { cat: 21, brand: 10, name: 'USB Flash SanDisk Ultra Dual Drive Go Type-C 64GB', slug: 'sandisk-ultra-dual-type-c-64gb', price: 250000, specs: '{"Dung lượng": "64GB", "Kết nối": "Type-C và Type-A", "Tốc độ đọc": "150MB/s"}', attr: '{"Màu sắc": "Đen"}', customBrand: 'SanDisk' },
    { cat: 21, brand: 10, name: 'USB Kingston DataTraveler Exodia 32GB', slug: 'kingston-dt-exodia-32gb', price: 120000, specs: '{"Dung lượng": "32GB", "Kết nối": "Type-A USB 3.2 Gen 1", "Chất liệu": "Nhựa"}', attr: '{"Màu sắc": "Đen Xanh"}', customBrand: 'Kingston' },
    { cat: 21, brand: 2, name: 'USB Flash Drive Samsung FIT Plus 128GB', slug: 'samsung-fit-plus-128gb', price: 500000, specs: '{"Dung lượng": "128GB", "Kết nối": "Type-A USB 3.1", "Tốc độ đọc": "400MB/s", "Thiết kế": "Siêu nhỏ gọn"}', attr: '[]' },

    // 22 Ổ cứng di động
    { cat: 22, brand: 2, name: 'Ổ cứng SSD di động Samsung T7 Touch 500GB', slug: 'samsung-t7-touch-500gb', price: 2200000, specs: '{"Dung lượng": "500GB", "Tốc độ đọc": "1050MB/s", "Tốc độ ghi": "1000MB/s", "Bảo mật": "Vân tay"}', attr: '{"Màu sắc": "Bạc"}' },
    { cat: 22, brand: 10, name: 'Ổ cứng SSD SanDisk Extreme Portable 1TB V2', slug: 'sandisk-extreme-portable-1tb-v2', price: 3500000, specs: '{"Dung lượng": "1TB", "Tốc độ đọc": "1050MB/s", "Tốc độ ghi": "1000MB/s", "Độ bền": "IP55, Chống sốc 2m"}', attr: '[]', customBrand: 'SanDisk' },
    { cat: 22, brand: 10, name: 'Ổ cứng HDD WD My Passport 2TB', slug: 'wd-my-passport-2tb', price: 1800000, specs: '{"Dung lượng": "2TB", "Kết nối": "USB 3.2 Gen 1", "Kích thước": "2.5 inch", "Phần mềm": "WD Backup"}', attr: '{"Màu sắc": "Đen"}', customBrand: 'WD' }
];

let sql = \`
USE csdl_phone;
GO

-- Add Brands
IF NOT EXISTS (SELECT 1 FROM Brands WHERE Name = 'SanDisk') INSERT INTO Brands (Name, Slug, Logo, IsActive, CreatedAt, UpdatedAt) VALUES ('SanDisk', 'sandisk', '', 1, GETDATE(), GETDATE());
IF NOT EXISTS (SELECT 1 FROM Brands WHERE Name = 'Kingston') INSERT INTO Brands (Name, Slug, Logo, IsActive, CreatedAt, UpdatedAt) VALUES ('Kingston', 'kingston', '', 1, GETDATE(), GETDATE());
IF NOT EXISTS (SELECT 1 FROM Brands WHERE Name = 'WD') INSERT INTO Brands (Name, Slug, Logo, IsActive, CreatedAt, UpdatedAt) VALUES ('WD', 'wd', '', 1, GETDATE(), GETDATE());

DECLARE @BrandId INT;
DECLARE @ProductId INT;
\`;

products.forEach((p, idx) => {
    const productCode = \`PCODE-\${p.slug.toUpperCase().replace(/[^A-Z0-9]/g, '').substring(0, 8)}-\${Math.floor(Math.random() * 10000)}\`;
    const sku = \`SKU-\${p.slug.toUpperCase().replace(/[^A-Z0-9]/g, '').substring(0, 8)}-DEF-\${idx}\`;
    
    sql += \`
-- Product: \${p.name}
IF '\${p.customBrand || ''}' != ''
BEGIN
    SELECT @BrandId = Id FROM Brands WHERE Name = '\${p.customBrand}';
END
ELSE
BEGIN
    SET @BrandId = \${p.brand};
END

INSERT INTO Products (Name, Slug, Description, BasePrice, TotalStock, IsActive, CreatedAt, UpdatedAt, CategoryId, ThumbnailImage, MainImage, Images, ReservedStock, BrandId, IsFeatured, ProductCode, Specs)
VALUES (
    N'\${p.name.replace(/'/g, "''")}', '\${p.slug}-\${idx}', N'Đây là mô tả mẫu cho sản phẩm \${p.name.replace(/'/g, "''")}. Thiết kế đẹp, chất lượng cao, bền bỉ theo thời gian.', \${p.price}, 0, 1, GETDATE(), GETDATE(), \${p.cat}, '', '', '[]', 0, @BrandId, 0, '\${productCode}', N'\${p.specs.replace(/'/g, "''")}'
);
SET @ProductId = SCOPE_IDENTITY();

INSERT INTO ProductVariants (Name, Price, TotalStock, CreatedAt, UpdatedAt, ProductId, ImageId, ReservedStock, Attributes, IsActive, Sku)
VALUES (
    N'Mặc định', \${p.price}, 0, GETDATE(), GETDATE(), @ProductId, '', 0, N'\${p.attr.replace(/'/g, "''")}', 1, '\${sku}'
);
\`;
});

fs.writeFileSync('insert_sample_products.sql', sql);
console.log('Done writing insert_sample_products.sql');
