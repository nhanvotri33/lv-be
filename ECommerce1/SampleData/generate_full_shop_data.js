const fs = require('fs');
const path = require('path');

const categories = [
    // Parent Categories
    { id: 1, name: 'Điện thoại', slug: 'dien-thoai', code: 'PHONE', parentId: null, icon: 'smartphone', desc: 'Điện thoại thông minh chính hãng Apple, Samsung, Xiaomi, OPPO...' },
    { id: 2, name: 'Laptop & Máy tính', slug: 'laptop', code: 'LAPTOP', parentId: null, icon: 'laptop', desc: 'Laptop văn phòng, laptop gaming, MacBook chính hãng' },
    { id: 3, name: 'Máy tính bảng', slug: 'may-tinh-bang', code: 'TABLET', parentId: null, icon: 'tablet', desc: 'iPad, Samsung Galaxy Tab, Xiaomi Pad' },
    { id: 4, name: 'Đồng hồ thông minh', slug: 'dong-ho-thong-minh', code: 'WATCH', parentId: null, icon: 'watch', desc: 'Apple Watch, Galaxy Watch, Garmin' },
    { id: 5, name: 'Tai nghe & Âm thanh', slug: 'tai-nghe-am-thanh', code: 'AUDIO', parentId: null, icon: 'headphones', desc: 'Tai nghe Bluetooth, Loa Bluetooth, Tai nghe chụp tai' },
    { id: 6, name: 'Phụ kiện điện thoại', slug: 'phu-kien-dien-thoai', code: 'PHONE_ACC', parentId: null, icon: 'cable', desc: 'Sạc dự phòng, Cáp sạc, Ốp lưng, Kính cường lực' },
    { id: 7, name: 'Phụ kiện máy tính', slug: 'phu-kien-may-tinh', code: 'PC_ACC', parentId: null, icon: 'mouse', desc: 'Chuột, Bàn phím, Thẻ nhớ, Ổ cứng di động' },

    // Sub Categories
    { id: 10, name: 'iPhone', slug: 'iphone', code: 'IPHONE', parentId: 1, icon: 'smartphone', desc: 'Điện thoại iPhone chính hãng Apple' },
    { id: 11, name: 'Samsung Galaxy', slug: 'samsung-galaxy', code: 'SAMSUNG_PHONE', parentId: 1, icon: 'smartphone', desc: 'Điện thoại Samsung Galaxy S, Fold, Z Flip, Series A' },
    { id: 12, name: 'Xiaomi Phone', slug: 'xiaomi-phone', code: 'XIAOMI_PHONE', parentId: 1, icon: 'smartphone', desc: 'Điện thoại Xiaomi, Redmi, POCO' },
    { id: 13, name: 'OPPO Phone', slug: 'oppo-phone', code: 'OPPO_PHONE', parentId: 1, icon: 'smartphone', desc: 'Điện thoại OPPO Find, Reno, Series A' },

    { id: 14, name: 'MacBook', slug: 'macbook', code: 'MACBOOK', parentId: 2, icon: 'laptop', desc: 'MacBook Air, MacBook Pro M1 M2 M3' },
    { id: 15, name: 'Laptop Gaming', slug: 'laptop-gaming', code: 'LAPTOP_GAMING', parentId: 2, icon: 'laptop', desc: 'Laptop cấu hình cao chơi game đồ họa' },
    { id: 16, name: 'Laptop Văn Phòng', slug: 'laptop-van-phong', code: 'LAPTOP_OFFICE', parentId: 2, icon: 'laptop', desc: 'Laptop mỏng nhẹ, pin trâu cho học sinh sinh viên văn phòng' },

    { id: 17, name: 'iPad', slug: 'ipad', code: 'IPAD', parentId: 3, icon: 'tablet', desc: 'iPad Pro, iPad Air, iPad Gen, iPad Mini' },
    { id: 18, name: 'Samsung Tab', slug: 'samsung-tab', code: 'SAMSUNG_TAB', parentId: 3, icon: 'tablet', desc: 'Samsung Galaxy Tab S, Tab A' },

    { id: 19, name: 'Apple Watch', slug: 'apple-watch', code: 'APPLE_WATCH', parentId: 4, icon: 'watch', desc: 'Apple Watch Series, Apple Watch Ultra, SE' },
    { id: 20, name: 'Galaxy Watch', slug: 'galaxy-watch', code: 'GALAXY_WATCH', parentId: 4, icon: 'watch', desc: 'Samsung Galaxy Watch Classic, Galaxy Watch FE' },

    { id: 21, name: 'Tai nghe Bluetooth', slug: 'tai-nghe-bluetooth', code: 'TWS_EARPHONES', parentId: 5, icon: 'headphones', desc: 'Tai nghe không dây True Wireless' },
    { id: 22, name: 'Loa Bluetooth', slug: 'loa-bluetooth', code: 'SPEAKER_BT', parentId: 5, icon: 'speaker', desc: 'Loa di động chống nước âm thanh hay' },

    { id: 23, name: 'Sạc dự phòng', slug: 'sac-du-phong', code: 'POWERBANK', parentId: 6, icon: 'battery-charging', desc: 'Pin sạc dự phòng sạc nhanh 10000mAh, 20000mAh, 100W' },
    { id: 24, name: 'Cáp sạc & Củ sạc', slug: 'cap-cu-sac', code: 'CHARGER_CABLE', parentId: 6, icon: 'zap', desc: 'Bộ sạc nhanh 20W, 30W, 65W, 100W Anker, Apple, Samsung' },
    { id: 25, name: 'Ốp lưng & Kính cường lực', slug: 'op-lung-kinh', code: 'CASES_GLASS', parentId: 6, icon: 'shield', desc: 'Ốp lưng chống sốc MagSafe, kính cường lực 9H' },

    { id: 26, name: 'Chuột & Bàn phím', slug: 'chuot-ban-phim', code: 'MOUSE_KEYBOARD', parentId: 7, icon: 'mouse', desc: 'Chuột không dây, bàn phím cơ Logitech, ASUS' },
    { id: 27, name: 'Thẻ nhớ & Ổ cứng', slug: 'the-nho-o-cung', code: 'STORAGE_MEDIA', parentId: 7, icon: 'hard-drive', desc: 'SSD di động, Thẻ nhớ MicroSD SanDisk, Kingston, Samsung' }
];

const brands = [
    { id: 1, name: 'Apple', code: 'APPLE', slug: 'apple', desc: 'Thương hiệu công nghệ hàng đầu thế giới từ Mỹ', img: 'https://images.unsplash.com/photo-1611186871348-b1ce696e52c9?q=80&w=200&auto=format&fit=crop' },
    { id: 2, name: 'Samsung', code: 'SAMSUNG', slug: 'samsung', desc: 'Tập đoàn điện tử công nghệ số 1 Hàn Quốc', img: 'https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?q=80&w=200&auto=format&fit=crop' },
    { id: 3, name: 'Xiaomi', code: 'XIAOMI', slug: 'xiaomi', desc: 'Thương hiệu thiết bị thông minh sáng tạo', img: 'https://images.unsplash.com/photo-1598327105666-5b89351aff97?q=80&w=200&auto=format&fit=crop' },
    { id: 4, name: 'ASUS', code: 'ASUS', slug: 'asus', desc: 'Thương hiệu máy tính & phần cứng Republic of Gamers', img: 'https://images.unsplash.com/photo-1603302576837-37561b2e2302?q=80&w=200&auto=format&fit=crop' },
    { id: 5, name: 'Dell', code: 'DELL', slug: 'dell', desc: 'Hãng sản xuất máy tính xách tay & máy trạm bền bỉ', img: 'https://images.unsplash.com/photo-1593642632823-8f785ba67e45?q=80&w=200&auto=format&fit=crop' },
    { id: 6, name: 'HP', code: 'HP', slug: 'hp', desc: 'Máy tính & thiết bị văn phòng chuyên nghiệp', img: 'https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?q=80&w=200&auto=format&fit=crop' },
    { id: 7, name: 'Lenovo', code: 'LENOVO', slug: 'lenovo', desc: 'Dòng sản phẩm ThinkPad huyền thoại & Legion Gaming', img: 'https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?q=80&w=200&auto=format&fit=crop' },
    { id: 8, name: 'OPPO', code: 'OPPO', slug: 'oppo', desc: 'Chuyên gia chụp ảnh chân thực & smartphone thời trang', img: 'https://images.unsplash.com/photo-1546054454-aa26e2b734c7?q=80&w=200&auto=format&fit=crop' },
    { id: 9, name: 'Sony', code: 'SONY', slug: 'sony', desc: 'Đỉnh cao âm thanh chống noise & máy ảnh cao cấp', img: 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?q=80&w=200&auto=format&fit=crop' },
    { id: 10, name: 'Anker', code: 'ANKER', slug: 'anker', desc: 'Thương hiệu phụ kiện sạc & pin sạc dự phòng số 1 thế giới', img: 'https://images.unsplash.com/photo-1609592424089-980f55c5df38?q=80&w=200&auto=format&fit=crop' },
    { id: 11, name: 'Baseus', code: 'BASEUS', slug: 'baseus', desc: 'Phụ kiện công nghệ thông minh, thiết kế tinh tế', img: 'https://images.unsplash.com/photo-1622445268465-840246e47683?q=80&w=200&auto=format&fit=crop' },
    { id: 12, name: 'JBL', code: 'JBL', slug: 'jbl', desc: 'Thương hiệu loa & tai nghe âm trầm sống động Harman', img: 'https://images.unsplash.com/photo-1545454675-3531b543be5d?q=80&w=200&auto=format&fit=crop' },
    { id: 13, name: 'Garmin', code: 'GARMIN', slug: 'garmin', desc: 'Đồng hồ thông minh định vị GPS cho thể thao đỉnh cao', img: 'https://images.unsplash.com/photo-1579586337278-3befd40fd17a?q=80&w=200&auto=format&fit=crop' },
    { id: 14, name: 'SanDisk', code: 'SANDISK', slug: 'sandisk', desc: 'Thẻ nhớ, USB & ổ cứng SSD lưu trữ tốc độ cao', img: 'https://images.unsplash.com/photo-1597872200969-2b65d56bd16b?q=80&w=200&auto=format&fit=crop' },
    { id: 15, name: 'Kingston', code: 'KINGSTON', slug: 'kingston', desc: 'Bộ nhớ RAM, USB & SSD lưu trữ dữ liệu an toàn', img: 'https://images.unsplash.com/photo-1597872200969-2b65d56bd16b?q=80&w=200&auto=format&fit=crop' },
    { id: 16, name: 'Logitech', code: 'LOGITECH', slug: 'logitech', desc: 'Chuột, bàn phím & thiết bị ngoại vi hàng đầu', img: 'https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?q=80&w=200&auto=format&fit=crop' },
    { id: 17, name: 'Spigen', code: 'SPIGEN', slug: 'spigen', desc: 'Ốp lưng & phụ kiện bảo vệ điện thoại cao cấp từ Mỹ', img: 'https://images.unsplash.com/photo-1601784551446-20c9e07cdbdb?q=80&w=200&auto=format&fit=crop' },
    { id: 18, name: 'Marshall', code: 'MARSHALL', slug: 'marshall', desc: 'Hãng âm thanh phong cách cổ điển Rock & Roll', img: 'https://images.unsplash.com/photo-1583394838336-acd977736f90?q=80&w=200&auto=format&fit=crop' }
];

const products = [
    // --- IPHONE ---
    {
        catId: 10, brandId: 1, isFeatured: true,
        name: 'iPhone 16 Pro Max', slug: 'iphone-16-pro-max',
        code: 'PROD-IP16PM', basePrice: 34990000, origPrice: 36990000,
        img: 'https://images.unsplash.com/photo-1695048133142-1a20484d2569?q=80&w=800&auto=format&fit=crop',
        desc: 'iPhone 16 Pro Max sở hữu khung vỏ Titanium cấp 5 siêu nhẹ, chip Apple A18 Pro 3nm mạnh mẽ nhất thế giới, nút Camera Control đột phá, hỗ trợ quay video 4K 120fps Dolby Vision và hệ thống trí tuệ nhân tạo Apple Intelligence.',
        specs: JSON.stringify({ "Màn hình": "6.9 inch Super Retina XDR OLED, 120Hz ProMotion", "Chip": "Apple A18 Pro (3nm)", "RAM": "8GB", "Camera sau": "Chính 48MP + Góc siêu rộng 48MP + Tele 5x 12MP", "Camera trước": "12MP TrueDepth", "Pin & Sạc": "Sạc nhanh 30W, Sạc không dây MagSafe 25W", "Chất liệu": "Khung Titanium, mặt lưng kính nhám" }),
        variants: [
            { name: 'Titan Sa Mạc / 256GB', sku: 'IP16PM-256-DESERT', price: 34990000, stock: 45, attr: JSON.stringify({ "Màu sắc": "Titan Sa Mạc (Desert Titanium)", "Dung lượng": "256GB" }) },
            { name: 'Titan Tự Nhiên / 256GB', sku: 'IP16PM-256-NATURAL', price: 34990000, stock: 30, attr: JSON.stringify({ "Màu sắc": "Titan Tự Nhiên (Natural Titanium)", "Dung lượng": "256GB" }) },
            { name: 'Titan Đen / 512GB', sku: 'IP16PM-512-BLACK', price: 40990000, stock: 20, attr: JSON.stringify({ "Màu sắc": "Titan Đen (Black Titanium)", "Dung lượng": "512GB" }) }
        ]
    },
    {
        catId: 10, brandId: 1, isFeatured: true,
        name: 'iPhone 15 Pro Max', slug: 'iphone-15-pro-max',
        code: 'PROD-IP15PM', basePrice: 28990000, origPrice: 32990000,
        img: 'https://images.unsplash.com/photo-1695048133142-1a20484d2569?q=80&w=800&auto=format&fit=crop',
        desc: 'iPhone 15 Pro Max trang bị vi xử lý Apple A17 Pro mạnh mẽ, camera zoom quang học 5x, nút Action Button tiện lợi cùng cổng sạc chuẩn USB-C tốc độ cao 10Gbps.',
        specs: JSON.stringify({ "Màn hình": "6.7 inch Super Retina XDR OLED, 120Hz ProMotion", "Chip": "Apple A17 Pro (3nm)", "RAM": "8GB", "Camera sau": "48MP + 12MP + 12MP (Zoom 5x)", "Cổng sạc": "USB-C 3.0", "Khung máy": "Titanium" }),
        variants: [
            { name: 'Titan Tự Nhiên / 256GB', sku: 'IP15PM-256-NAT', price: 28990000, stock: 35, attr: JSON.stringify({ "Màu sắc": "Titan Tự Nhiên", "Dung lượng": "256GB" }) },
            { name: 'Titan Xanh / 512GB', sku: 'IP15PM-512-BLUE', price: 34990000, stock: 15, attr: JSON.stringify({ "Màu sắc": "Titan Xanh", "Dung lượng": "512GB" }) }
        ]
    },
    {
        catId: 10, brandId: 1, isFeatured: false,
        name: 'iPhone 15 128GB', slug: 'iphone-15-128gb',
        code: 'PROD-IP15', basePrice: 19490000, origPrice: 22990000,
        img: 'https://images.unsplash.com/photo-1592750475338-74b7b21085ab?q=80&w=800&auto=format&fit=crop',
        desc: 'iPhone 15 đột phá với màn hình Dynamic Island linh hoạt, camera chính 48MP cực sắc nét, mặt lưng kính pha màu thời thượng và cổng kết nối USB-C chuẩn mực.',
        specs: JSON.stringify({ "Màn hình": "6.1 inch OLED Super Retina XDR", "Chip": "Apple A16 Bionic", "RAM": "6GB", "Camera": "48MP + 12MP", "Tính năng": "Dynamic Island, USB-C" }),
        variants: [
            { name: 'Màu Hồng / 128GB', sku: 'IP15-128-PINK', price: 19490000, stock: 50, attr: JSON.stringify({ "Màu sắc": "Hồng (Pink)", "Dung lượng": "128GB" }) },
            { name: 'Màu Xanh Lá / 128GB', sku: 'IP15-128-GREEN', price: 19490000, stock: 40, attr: JSON.stringify({ "Màu sắc": "Xanh Lá (Green)", "Dung lượng": "128GB" }) }
        ]
    },

    // --- SAMSUNG ---
    {
        catId: 11, brandId: 2, isFeatured: true,
        name: 'Samsung Galaxy S24 Ultra', slug: 'samsung-galaxy-s24-ultra',
        code: 'PROD-S24U', basePrice: 29990000, origPrice: 33990000,
        img: 'https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?q=80&w=800&auto=format&fit=crop',
        desc: 'Samsung Galaxy S24 Ultra quyền năng AI vượt trội (Galaxy AI: Khoanh vùng tìm kiếm, Trợ lý quyền năng, Phiên dịch trực tiếp), khung vỏ Titanium phẳng cứng cáp và bút S Pen tích hợp.',
        specs: JSON.stringify({ "Màn hình": "6.8 inch Dynamic AMOLED 2X, 120Hz 2600 nits", "Chip": "Snapdragon 8 Gen 3 for Galaxy", "RAM": "12GB", "Camera": "200MP + 50MP + 12MP + 10MP", "Pin": "5000mAh, Sạc 45W", "Bút cảm ứng": "Tích hợp S-Pen" }),
        variants: [
            { name: 'Xám Titanium / 256GB', sku: 'S24U-256-GRAY', price: 29990000, stock: 40, attr: JSON.stringify({ "Màu sắc": "Xám Titanium", "Dung lượng": "256GB" }) },
            { name: 'Đen Titanium / 512GB', sku: 'S24U-512-BLACK', price: 34490000, stock: 25, attr: JSON.stringify({ "Màu sắc": "Đen Titanium", "Dung lượng": "512GB" }) }
        ]
    },
    {
        catId: 11, brandId: 2, isFeatured: true,
        name: 'Samsung Galaxy Z Fold6 5G', slug: 'samsung-galaxy-z-fold6',
        code: 'PROD-ZFOLD6', basePrice: 41990000, origPrice: 43990000,
        img: 'https://images.unsplash.com/photo-1580910051074-3eb694886505?q=80&w=800&auto=format&fit=crop',
        desc: 'Galaxy Z Fold6 thiết kế siêu mỏng nhẹ vuông vức hoàn hảo, bản lề FlexHinge thế hệ mới bền bỉ, màn hình cực đại 7.6 inch nâng tầm hiệu suất làm việc đa nhiệm cùng Galaxy AI.',
        specs: JSON.stringify({ "Màn hình chính": "7.6 inch Dynamic AMOLED 2X 120Hz", "Màn hình phụ": "6.3 inch 120Hz", "Chip": "Snapdragon 8 Gen 3", "RAM": "12GB", "Bộ nhớ": "256GB/512GB" }),
        variants: [
            { name: 'Xám Metal / 256GB', sku: 'ZFOLD6-256-GRAY', price: 41990000, stock: 15, attr: JSON.stringify({ "Màu sắc": "Xám Metal", "Dung lượng": "256GB" }) },
            { name: 'Xanh Navy / 512GB', sku: 'ZFOLD6-512-NAVY', price: 46990000, stock: 10, attr: JSON.stringify({ "Màu sắc": "Xanh Navy", "Dung lượng": "512GB" }) }
        ]
    },
    {
        catId: 11, brandId: 2, isFeatured: false,
        name: 'Samsung Galaxy A55 5G', slug: 'samsung-galaxy-a55-5g',
        code: 'PROD-A55', basePrice: 9690000, origPrice: 10990000,
        img: 'https://images.unsplash.com/photo-1565849904461-04a58ad377e0?q=80&w=800&auto=format&fit=crop',
        desc: 'Galaxy A55 5G khung kim loại sang trọng, camera đêm 50MP nét vượt trội, kháng nước chống bụi IP67 và vi xử lý Exynos 1480 4nm tiết kiệm pin.',
        specs: JSON.stringify({ "Màn hình": "6.6 inch Super AMOLED 120Hz", "Chip": "Exynos 1480 (4nm)", "RAM": "8GB", "Pin": "5000mAh", "Kháng nước": "IP67" }),
        variants: [
            { name: 'Xanh Băng / 128GB', sku: 'A55-128-ICE', price: 9690000, stock: 60, attr: JSON.stringify({ "Màu sắc": "Xanh Băng (Iceblue)", "Dung lượng": "128GB" }) },
            { name: 'Tím Mới / 256GB', sku: 'A55-256-PURPLE', price: 10690000, stock: 50, attr: JSON.stringify({ "Màu sắc": "Tím Lilac", "Dung lượng": "256GB" }) }
        ]
    },

    // --- XIAOMI ---
    {
        catId: 12, brandId: 3, isFeatured: true,
        name: 'Xiaomi 14 Ultra 5G', slug: 'xiaomi-14-ultra',
        code: 'PROD-XM14U', basePrice: 29990000, origPrice: 32990000,
        img: 'https://images.unsplash.com/photo-1598327105666-5b89351aff97?q=80&w=800&auto=format&fit=crop',
        desc: 'Xiaomi 14 Ultra kết hợp cùng ống kính Leica Summilux huyền thoại, cảm biến 1-inch khẩu độ vô cấp LYT-900, chip Snapdragon 8 Gen 3 và công nghệ sạc siêu tốc 90W HyperCharge.',
        specs: JSON.stringify({ "Màn hình": "6.73 inch AMOLED 2K+ 120Hz LTPO", "Ống kính": "Bộ 4 camera 50MP Leica", "Chip": "Snapdragon 8 Gen 3", "RAM": "16GB", "Pin": "5000mAh, Sạc 90W" }),
        variants: [
            { name: 'Màu Đen / 512GB', sku: 'XM14U-512-BLK', price: 29990000, stock: 20, attr: JSON.stringify({ "Màu sắc": "Đen da tổng hợp", "Dung lượng": "512GB" }) },
            { name: 'Màu Trắng / 512GB', sku: 'XM14U-512-WHT', price: 29990000, stock: 15, attr: JSON.stringify({ "Màu sắc": "Trắng", "Dung lượng": "512GB" }) }
        ]
    },
    {
        catId: 12, brandId: 3, isFeatured: false,
        name: 'Xiaomi Redmi Note 13 Pro+ 5G', slug: 'redmi-note-13-pro-plus',
        code: 'PROD-RN13PP', basePrice: 9490000, origPrice: 10990000,
        img: 'https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?q=80&w=800&auto=format&fit=crop',
        desc: 'Redmi Note 13 Pro+ 5G trang bị màn hình cong AMOLED 1.5K 120Hz, camera siêu phân giải 200MP chống rung OIS và sạc thần tốc 120W đầy pin trong 19 phút.',
        specs: JSON.stringify({ "Màn hình": "6.67 inch AMOLED 1.5K 120Hz", "Camera": "200MP OIS", "Chip": "Dimensity 7200-Ultra", "Sạc nhanh": "120W HyperCharge", "Kháng nước": "IP68" }),
        variants: [
            { name: 'Đen Đêm / 256GB', sku: 'RN13PP-256-BLK', price: 9490000, stock: 55, attr: JSON.stringify({ "Màu sắc": "Đen Đêm", "Dung lượng": "256GB" }) }
        ]
    },

    // --- OPPO ---
    {
        catId: 13, brandId: 8, isFeatured: true,
        name: 'OPPO Find N3 5G', slug: 'oppo-find-n3',
        code: 'PROD-OPFINDN3', basePrice: 41990000, origPrice: 44990000,
        img: 'https://images.unsplash.com/photo-1546054454-aa26e2b734c7?q=80&w=800&auto=format&fit=crop',
        desc: 'OPPO Find N3 thiết kế gập đỉnh cao với camera Hasselblad sắc nét hàng đầu phân khúc, màn hình sáng nits kỉ lục và công nghệ làm việc đa nhiệm không nếp gấp.',
        specs: JSON.stringify({ "Màn hình gập": "7.82 inch AMOLED 120Hz", "Camera": "Chính 48MP + Tele 64MP Hasselblad", "Chip": "Snapdragon 8 Gen 2", "RAM": "16GB", "Bộ nhớ": "512GB" }),
        variants: [
            { name: 'Vàng Hoàng Kim / 512GB', sku: 'FINDN3-512-GOLD', price: 41990000, stock: 12, attr: JSON.stringify({ "Màu sắc": "Vàng Hoàng Kim", "Dung lượng": "512GB" }) }
        ]
    },

    // --- MACBOOK ---
    {
        catId: 14, brandId: 1, isFeatured: true,
        name: 'MacBook Air 13 inch M3 2024', slug: 'macbook-air-13-m3-2024',
        code: 'PROD-MBA13M3', basePrice: 26990000, origPrice: 27990000,
        img: 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8?q=80&w=800&auto=format&fit=crop',
        desc: 'MacBook Air 13 inch chip M3 siêu mỏng nhẹ 1.24kg, hỗ trợ xuất 2 màn hình ngoài, thời lượng pin ấn tượng tới 18 giờ liên tục.',
        specs: JSON.stringify({ "Màn hình": "13.6 inch Liquid Retina 500 nits", "Chip": "Apple M3 (8-core CPU, 8-core/10-core GPU)", "RAM": "8GB / 16GB Unified", "SSD": "256GB / 512GB", "Pin": "18 giờ liên tục" }),
        variants: [
            { name: 'Midnight (Đen Đêm) / 8GB / 256GB', sku: 'MBA13M3-8-256-MID', price: 26990000, stock: 30, attr: JSON.stringify({ "Màu sắc": "Midnight (Đen Đêm)", "RAM": "8GB", "SSD": "256GB" }) },
            { name: 'Starlight (Vàng Ánh Kim) / 16GB / 512GB', sku: 'MBA13M3-16-512-STL', price: 36990000, stock: 20, attr: JSON.stringify({ "Màu sắc": "Starlight", "RAM": "16GB", "SSD": "512GB" }) }
        ]
    },
    {
        catId: 14, brandId: 1, isFeatured: true,
        name: 'MacBook Pro 14 inch M3 Pro', slug: 'macbook-pro-14-m3-pro',
        code: 'PROD-MBP14M3P', basePrice: 49990000, origPrice: 54990000,
        img: 'https://images.unsplash.com/photo-1611186871348-b1ce696e52c9?q=80&w=800&auto=format&fit=crop',
        desc: 'MacBook Pro 14 M3 Pro màu Space Black ấn tượng, màn hình XDR ProMotion 120Hz chuyên nghiệp cho lập trình viên, nhà thiết kế 3D và dựng phim 8K.',
        specs: JSON.stringify({ "Màn hình": "14.2 inch Liquid Retina XDR (3024x1964) 120Hz", "Chip": "Apple M3 Pro (11-core CPU, 14-core GPU)", "RAM": "18GB Unified", "SSD": "512GB NVMe" }),
        variants: [
            { name: 'Space Black / 18GB / 512GB', sku: 'MBP14M3P-18-512-BLK', price: 49990000, stock: 15, attr: JSON.stringify({ "Màu sắc": "Space Black (Đen Thạch Anh)", "RAM": "18GB", "SSD": "512GB" }) }
        ]
    },

    // --- LAPTOP GAMING & OFFICE ---
    {
        catId: 15, brandId: 4, isFeatured: true,
        name: 'ASUS ROG Zephyrus G14 OLED 2024', slug: 'asus-rog-zephyrus-g14-2024',
        code: 'PROD-ZEPHYRUSG14', basePrice: 42990000, origPrice: 46990000,
        img: 'https://images.unsplash.com/photo-1603302576837-37561b2e2302?q=80&w=800&auto=format&fit=crop',
        desc: 'ASUS ROG Zephyrus G14 thiết kế nhôm nguyên khối siêu mỏng, màn hình ROG Nebula OLED 3K 120Hz chuẩn màu 100% DCI-P3 và GPU RTX 4060 chiến mượt mọi tựa game AAA.',
        specs: JSON.stringify({ "Màn hình": "14.0 inch 3K OLED 120Hz 0.2ms", "CPU": "AMD Ryzen 9 8945HS", "VGA": "NVIDIA GeForce RTX 4060 8GB GDDR6", "RAM": "16GB LPDDR5X", "SSD": "1TB PCIe 4.0" }),
        variants: [
            { name: 'Platinum White / Ryzen 9 / RTX 4060', sku: 'G14-R9-4060-WHT', price: 42990000, stock: 15, attr: JSON.stringify({ "Màu sắc": "Platinum White", "Cấu hình": "Ryzen 9 / RTX 4060 / 16GB / 1TB" }) }
        ]
    },
    {
        catId: 16, brandId: 5, isFeatured: false,
        name: 'Dell XPS 13 9340 Core Ultra 7', slug: 'dell-xps-13-9340',
        code: 'PROD-DELLXPS13', basePrice: 44990000, origPrice: 47990000,
        img: 'https://images.unsplash.com/photo-1593642632823-8f785ba67e45?q=80&w=800&auto=format&fit=crop',
        desc: 'Dell XPS 13 chuẩn mực ultrabook tương lai với kính Gorillaglass tràn viền, phím bấm hàng chức năng cảm ứng Touch Bar hiện đại, trang bị chip Intel Core Ultra AI.',
        specs: JSON.stringify({ "Màn hình": "13.4 inch FHD+ InfinityEdge IPS", "CPU": "Intel Core Ultra 7 155H (NPU AI)", "RAM": "16GB LPDDR5X", "SSD": "512GB NVMe" }),
        variants: [
            { name: 'Màu Platinum / Core Ultra 7', sku: 'XPS9340-U7-16-512', price: 44990000, stock: 18, attr: JSON.stringify({ "Màu sắc": "Platinum", "CPU": "Intel Core Ultra 7", "RAM": "16GB" }) }
        ]
    },

    // --- TABLETS ---
    {
        catId: 17, brandId: 1, isFeatured: true,
        name: 'iPad Pro 11 inch M4 2024 Ultra Retina OLED', slug: 'ipad-pro-11-m4-2024',
        code: 'PROD-IPADPROM4', basePrice: 28490000, origPrice: 29990000,
        img: 'https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0?q=80&w=800&auto=format&fit=crop',
        desc: 'iPad Pro M4 2024 mỏng chưa từng có chỉ 5.3mm, đột phá công nghệ màn hình Tandem OLED Ultra Retina XDR và chip Apple M4 xử lý AI đồ họa cực đại.',
        specs: JSON.stringify({ "Màn hình": "11 inch Ultra Retina Tandem OLED 120Hz", "Chip": "Apple M4 (9-core CPU, 10-core GPU)", "Độ mỏng": "5.3 mm", "Hỗ trợ": "Apple Pencil Pro, Magic Keyboard M4" }),
        variants: [
            { name: 'Space Black / WiFi / 256GB', sku: 'IPADPROM4-11-256-BLK', price: 28490000, stock: 25, attr: JSON.stringify({ "Màu sắc": "Space Black", "Kết nối": "Wi-Fi", "Dung lượng": "256GB" }) },
            { name: 'Silver / WiFi + 5G / 512GB', sku: 'IPADPROM4-11-512-5G', price: 37490000, stock: 12, attr: JSON.stringify({ "Màu sắc": "Silver", "Kết nối": "Wi-Fi + 5G", "Dung lượng": "512GB" }) }
        ]
    },
    {
        catId: 18, brandId: 2, isFeatured: false,
        name: 'Samsung Galaxy Tab S9 Ultra', slug: 'samsung-galaxy-tab-s9-ultra',
        code: 'PROD-TABS9U', basePrice: 26990000, origPrice: 29990000,
        img: 'https://images.unsplash.com/photo-1585790050230-5dd28404ccb9?q=80&w=800&auto=format&fit=crop',
        desc: 'Galaxy Tab S9 Ultra màn hình siêu lớn 14.6 inch AMOLED 120Hz, kèm bút S Pen chống nước IP68, đáp ứng hoàn hảo nhu cầu vẽ đồ họa, thiết kế và làm việc chuyên nghiệp.',
        specs: JSON.stringify({ "Màn hình": "14.6 inch Dynamic AMOLED 2X 120Hz", "Chip": "Snapdragon 8 Gen 2 for Galaxy", "RAM": "12GB", "Pin": "11200mAh", "Kháng nước": "IP68" }),
        variants: [
            { name: 'Màu Xám / 256GB / Wifi', sku: 'TABS9U-256-GRAY', price: 26990000, stock: 20, attr: JSON.stringify({ "Màu sắc": "Xám (Graphite)", "Dung lượng": "256GB" }) }
        ]
    },

    // --- WATCHES ---
    {
        catId: 19, brandId: 1, isFeatured: true,
        name: 'Apple Watch Ultra 2 GPS + Cellular 49mm', slug: 'apple-watch-ultra-2-49mm',
        code: 'PROD-AWULTRA2', basePrice: 20990000, origPrice: 21990000,
        img: 'https://images.unsplash.com/photo-1508685096489-7aacd43bd3b1?q=80&w=800&auto=format&fit=crop',
        desc: 'Apple Watch Ultra 2 vỏ Titanium siêu bền chống nước 100m, màn hình sáng kỷ lục 3000 nits, chip S9 SIP chạm hai lần Double Tap thông minh và định vị GPS tần số kép cực chính xác.',
        specs: JSON.stringify({ "Kích thước": "49mm Titanium Case", "Màn hình": "OLED 3000 nits Always-On", "Chip": "Apple S9 SiP", "Tính năng": "Double Tap, Còi báo động 86dB, Lặn 40m", "Pin": "Up to 36 hours (60 hours Low Power)" }),
        variants: [
            { name: 'Dây Alpine Loop Size M / Cam', sku: 'AWULTRA2-ALP-ORG', price: 20990000, stock: 25, attr: JSON.stringify({ "Loại dây": "Alpine Loop", "Màu dây": "Cam", "Size": "49mm" }) }
        ]
    },
    {
        catId: 20, brandId: 13, isFeatured: false,
        name: 'Garmin Fenix 7 Pro Sapphire Solar Titanium', slug: 'garmin-fenix-7-pro-sapphire-solar',
        code: 'PROD-GARMINF7P', basePrice: 21990000, origPrice: 23990000,
        img: 'https://images.unsplash.com/photo-1579586337278-3befd40fd17a?q=80&w=800&auto=format&fit=crop',
        desc: 'Garmin Fenix 7 Pro tích hợp kính sạc năng lượng mặt trời Sapphire chống trầy, đèn quắc LED chiếu sáng khẩn cấp, cảm biến nhịp tim Elevate Gen 5 và bản đồ địa hình đa lục địa.',
        specs: JSON.stringify({ "Mặt đồng hồ": "47mm Kính Sapphire Solar", "Đèn pin": "Đèn pin LED tích hợp", "Cảm biến": "Elevate Gen 5", "Pin": "Lên đến 22 ngày ở chế độ Smartwatch" }),
        variants: [
            { name: 'Titanium Gray / Dây Silicone Đen', sku: 'FENIX7P-TIT-BLK', price: 21990000, stock: 15, attr: JSON.stringify({ "Chất liệu": "Titanium", "Màu sắc": "Đen Titanium" }) }
        ]
    },

    // --- AUDIO ---
    {
        catId: 21, brandId: 1, isFeatured: true,
        name: 'AirPods Pro 2 USB-C (MagSafe Case)', slug: 'airpods-pro-2-usbc',
        code: 'PROD-APP2USBC', basePrice: 5690000, origPrice: 6190000,
        img: 'https://images.unsplash.com/photo-1600294037681-c80b4cb5b434?q=80&w=800&auto=format&fit=crop',
        desc: 'AirPods Pro 2 bản nâng cấp cổng sạc USB-C, chip Apple H2 chống ồn chủ động gấp 2 lần, tính năng Âm thanh thích ứng (Adaptive Audio) và chuẩn kháng bụi nước IP54.',
        specs: JSON.stringify({ "Chip": "Apple H2 trong tai nghe, Apple U1 trong hộp sạc", "Chống ồn": "Active Noise Cancellation (ANC) x2", "Cổng sạc": "USB-C & MagSafe", "Thời lượng pin": "6 giờ (hộp sạc lên 30 giờ)" }),
        variants: [
            { name: 'Màu Trắng / USB-C', sku: 'APP2-USBC-WHT', price: 5690000, stock: 80, attr: JSON.stringify({ "Màu sắc": "Trắng", "Cổng sạc": "USB-C" }) }
        ]
    },
    {
        catId: 21, brandId: 9, isFeatured: true,
        name: 'Tai nghe Sony WH-1000XM5 Noise Canceling', slug: 'sony-wh-1000xm5',
        code: 'PROD-SONYXM5', basePrice: 7990000, origPrice: 8990000,
        img: 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?q=80&w=800&auto=format&fit=crop',
        desc: 'Sony WH-1000XM5 với 8 micro và 2 bộ xử lý chống ồn V1/QN1 mang lại trải nghiệm âm thanh tĩnh lặng tuyệt đối, hỗ trợ LDAC Hi-Res Audio không dây và đàm thoại siêu rõ nét.',
        specs: JSON.stringify({ "Kiểu dáng": "Chụp tai Over-Ear", "Bộ xử lý": "HD Noise Canceling Processor QN1 + V1", "Pin": "30 giờ bật ANC (Sạc 3 phút dùng 3 giờ)", "Codec": "LDAC, AAC, SBC" }),
        variants: [
            { name: 'Màu Đen / Black', sku: 'XM5-HEADPHONE-BLK', price: 7990000, stock: 35, attr: JSON.stringify({ "Màu sắc": "Đen" }) },
            { name: 'Màu Bạc Bạch Kim / Silver', sku: 'XM5-HEADPHONE-SLV', price: 7990000, stock: 25, attr: JSON.stringify({ "Màu sắc": "Bạc Bạch Kim" }) }
        ]
    },
    {
        catId: 22, brandId: 12, isFeatured: false,
        name: 'Loa Bluetooth JBL Charge 5 40W IP67', slug: 'jbl-charge-5',
        code: 'PROD-JBLCHARGE5', basePrice: 3490000, origPrice: 3990000,
        img: 'https://images.unsplash.com/photo-1545454675-3531b543be5d?q=80&w=800&auto=format&fit=crop',
        desc: 'Loa JBL Charge 5 âm thanh JBL Original Pro Sound sống động với loa woofer riêng biệt, công suất 40W RMS, chống nước chống bụi IP67 chuẩn quân đội và hỗ trợ sạc ngược pin cho điện thoại.',
        specs: JSON.stringify({ "Công suất": "40W RMS (30W Woofer + 10W Tweeter)", "Kháng nước": "IP67 waterproof & dustproof", "Thời lượng pin": "20 giờ phát liên tục", "Tính năng": "PartyBoost kết nối nhiều loa, Powerbank sạc ngược" }),
        variants: [
            { name: 'Màu Đen (Black)', sku: 'JBLCHARGE5-BLK', price: 3490000, stock: 40, attr: JSON.stringify({ "Màu sắc": "Đen" }) },
            { name: 'Màu Xanh Dương (Blue)', sku: 'JBLCHARGE5-BLU', price: 3490000, stock: 30, attr: JSON.stringify({ "Màu sắc": "Xanh Dương" }) }
        ]
    },

    // --- ACCESSORIES ---
    {
        catId: 23, brandId: 10, isFeatured: true,
        name: 'Sạc dự phòng Anker 737 Power Bank (Prime 24,000mAh 140W)', slug: 'anker-737-power-bank-140w',
        code: 'PROD-ANKER737', basePrice: 2490000, origPrice: 2890000,
        img: 'https://images.unsplash.com/photo-1609592424089-980f55c5df38?q=80&w=800&auto=format&fit=crop',
        desc: 'Anker 737 PowerBank dung lượng siêu lớn 24.000mAh công nghệ sạc nhanh PD 3.1 140W hai chiều sạc mượt cả MacBook Pro, trang bị màn hình kĩ thuật số hiển thị công suất và nhiệt độ pin tức thì.',
        specs: JSON.stringify({ "Dung lượng": "24,000mAh / 86.4Wh", "Công suất ra": "Tối đa 140W (USB-C1/C2)", "Màn hình": "Smart Digital Display", "Cổng sạc": "2 USB-C, 1 USB-A" }),
        variants: [
            { name: 'Màu Đen Xám 140W', sku: 'ANKER737-24K-140W', price: 2490000, stock: 50, attr: JSON.stringify({ "Màu sắc": "Đen Xám", "Dung lượng": "24000mAh" }) }
        ]
    },
    {
        catId: 24, brandId: 10, isFeatured: false,
        name: 'Củ sạc nhanh Anker 511 Nano 3 30W Type-C GaN', slug: 'anker-nano-3-30w',
        code: 'PROD-ANKERNANO3', basePrice: 390000, origPrice: 450000,
        img: 'https://images.unsplash.com/photo-1583863788434-e58a36330cf0?q=80&w=800&auto=format&fit=crop',
        desc: 'Củ sạc Anker Nano 3 30W siêu nhỏ gọn gấp 70% củ sạc thông thường nhờ công nghệ GaN, sạc nhanh chuẩn cho iPhone 15/16 Pro Max và iPad Air/Pro.',
        specs: JSON.stringify({ "Công suất": "30W Power Delivery", "Công nghệ": "GaN (Gallium Nitride)", "Chân sạc": "Gập gọn 90 độ" }),
        variants: [
            { name: 'Màu Trắng / 30W', sku: 'ANKER-NANO3-30W-WHT', price: 390000, stock: 100, attr: JSON.stringify({ "Màu sắc": "Trắng", "Công suất": "30W" }) }
        ]
    },
    {
        catId: 25, brandId: 17, isFeatured: false,
        name: 'Ốp lưng Spigen Ultra Hybrid MagFit iPhone 16 Pro Max', slug: 'spigen-ultra-hybrid-magfit-ip16pm',
        code: 'PROD-SPIGEN16PM', basePrice: 690000, origPrice: 790000,
        img: 'https://images.unsplash.com/photo-1601784551446-20c9e07cdbdb?q=80&w=800&auto=format&fit=crop',
        desc: 'Ốp lưng Spigen Ultra Hybrid đạt chứng nhận chống sốc chuẩn quân đội Mỹ Air Cushion Technology, viền dẻo TPU lưng cứng PC trong suốt chống ố vàng và tích hợp vòng nam châm MagSafe mạnh mẽ.',
        specs: JSON.stringify({ "Chất liệu": "Lưng Polycarbonate cứng + Viền TPU dẻo", "Tính năng": "Khung nam châm MagSafe, Công nghệ đệm khí Air Cushion" }),
        variants: [
            { name: 'Trong Suốt (White Clear)', sku: 'SPG-IP16PM-CLR', price: 690000, stock: 70, attr: JSON.stringify({ "Màu sắc": "Trong Suốt" }) }
        ]
    },
    {
        catId: 26, brandId: 16, isFeatured: true,
        name: 'Chuột không dây Logitech MX Master 3S Quiet Clicks 8K DPI', slug: 'logitech-mx-master-3s',
        code: 'PROD-MXMASTER3S', basePrice: 2290000, origPrice: 2590000,
        img: 'https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?q=80&w=800&auto=format&fit=crop',
        desc: 'Logitech MX Master 3S chuột không dây cao cấp nhất cho công việc với nút click siêu êm Quiet Clicks 90%, con cuộn MagSpeed cuộn 1000 dòng/giây và cảm biến 8000 DPI dùng tốt trên mặt kính.',
        specs: JSON.stringify({ "Cảm biến": "Darkfield 8000 DPI (hoạt động trên kính)", "Nút bấm": "Quiet Clicks yên tĩnh", "Con cuộn": "MagSpeed cuộn từ tính", "Kết nối": "Logi Bolt & Bluetooth (3 thiết bị)" }),
        variants: [
            { name: 'Màu Đen Graphpas', sku: 'MXM3S-BLK', price: 2290000, stock: 40, attr: JSON.stringify({ "Màu sắc": "Đen Graphite" }) },
            { name: 'Màu Xám Pale Gray', sku: 'MXM3S-GRY', price: 2290000, stock: 30, attr: JSON.stringify({ "Màu sắc": "Xám Nhạt" }) }
        ]
    },
    {
        catId: 27, brandId: 14, isFeatured: false,
        name: 'SSD Di Động SanDisk Extreme Portable 1TB V2 1050MB/s', slug: 'sandisk-extreme-portable-1tb-v2',
        code: 'PROD-SANDISK1TB', basePrice: 2690000, origPrice: 2990000,
        img: 'https://images.unsplash.com/photo-1544652478-6653e09f18a2?q=80&w=800&auto=format&fit=crop',
        desc: 'Ổ cứng SSD di động SanDisk Extreme V2 dung lượng 1TB tốc độ đọc 1050MB/s, chuẩn chống nước chống bụi IP55, rơi vỡ từ độ cao 2 mét an toàn cho nhiếp ảnh gia và quay phim.',
        specs: JSON.stringify({ "Dung lượng": "1TB NVMe SSD", "Tốc độ đọc/ghi": "1050MB/s / 1000MB/s", "Độ bền": "IP55 water/dust resistance, Chống rơi 2m", "Giao tiếp": "USB 3.2 Gen 2 Type-C" }),
        variants: [
            { name: 'Màu Đen Viền Cam / 1TB', sku: 'SD-EXT1TB-BLK', price: 2690000, stock: 35, attr: JSON.stringify({ "Dung lượng": "1TB", "Màu sắc": "Đen Cam" }) }
        ]
    }
];

const banners = [
    { type: 'Slider', pos: 1, img: 'https://images.unsplash.com/photo-1695048133142-1a20484d2569?q=80&w=1600&auto=format&fit=crop', link: '/category/iphone' },
    { type: 'Slider', pos: 2, img: 'https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?q=80&w=1600&auto=format&fit=crop', link: '/product/samsung-galaxy-s24-ultra' },
    { type: 'Slider', pos: 3, img: 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8?q=80&w=1600&auto=format&fit=crop', link: '/category/macbook' },
    { type: 'Slider', pos: 4, img: 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?q=80&w=1600&auto=format&fit=crop', link: '/category/tai-nghe-am-thanh' },
    { type: 'Top', pos: 1, img: 'https://images.unsplash.com/photo-1607082348824-0a96f2a4b9da?q=80&w=1200&auto=format&fit=crop', link: '/promotions' },
    { type: 'Right', pos: 1, img: 'https://images.unsplash.com/photo-1526738549149-8e07eca6c147?q=80&w=600&auto=format&fit=crop', link: '/category/phu-kien-dien-thoai' }
];

const promotions = [
    { code: 'WELCOME100K', type: 'Fixed', val: 100000, minOrd: 1000000, maxDisc: 100000, startDaysAgo: 10, endDaysLater: 60, limit: 500, used: 42 },
    { code: 'TECHFEST500K', type: 'Fixed', val: 500000, minOrd: 10000000, maxDisc: 500000, startDaysAgo: 15, endDaysLater: 30, limit: 200, used: 18 },
    { code: 'FREESHIP30K', type: 'Fixed', val: 30000, minOrd: 300000, maxDisc: 30000, startDaysAgo: 20, endDaysLater: 90, limit: 1000, used: 154 },
    { code: 'SUPERDEAL10', type: 'Percentage', val: 10, minOrd: 2000000, maxDisc: 1000000, startDaysAgo: 5, endDaysLater: 45, limit: 300, used: 67 }
];

const sampleUsers = [
    { guid: 'A1111111-1111-1111-1111-111111111111', user: 'nguyenvanan', email: 'an.nguyen@gmail.com', role: 'User', points: 150, accum: 450 },
    { guid: 'B2222222-2222-2222-2222-222222222222', user: 'tranthimai', email: 'mai.tran@gmail.com', role: 'User', points: 300, accum: 1200 },
    { guid: 'C3333333-3333-3333-3333-333333333333', user: 'lehoangnam', email: 'nam.le@gmail.com', role: 'User', points: 50, accum: 200 },
    { guid: 'D4444444-4444-4444-4444-444444444444', user: 'phamminhtuan', email: 'tuan.pham@gmail.com', role: 'User', points: 500, accum: 2500 }
];

const sampleReviews = [
    { rating: 5, comment: 'Máy giao siêu nhanh, đóng gói cẩn thận 2 lớp chống sốc. Dùng mượt mà không có điểm gì chê!', reply: 'Dạ Shop xin cảm ơn quý khách đã tin tưởng và ủng hộ ạ!' },
    { rating: 5, comment: 'Chụp ảnh quá đỉnh luôn, màu sắc chân thực sắc nét. Hàng chính hãng VNA chuẩn seal!', reply: 'Cảm ơn bạn nhiều nha! Chúc bạn có trải nghiệm tuyệt vời cùng sản phẩm!' },
    { rating: 4, comment: 'Sản phẩm đẹp, pin dùng được 1.5 ngày thoải mái. Nhân viên tư vấn nhiệt tình.', reply: null },
    { rating: 5, comment: 'Chất lượng tuyệt vời trong tầm giá, sạc nhanh và không bị nóng máy.', reply: null },
    { rating: 5, comment: 'Đã mua sản phẩm thứ 3 ở shop, lần nào cũng vô cùng hài lòng từ dịch vụ tới hậu mãi.', reply: 'Shop luôn sẵn sàng hỗ trợ bạn ạ, cảm ơn sự đồng hành của bạn!' }
];

// Construct SQL string step by step
let parts = [];

parts.push(`-- ============================================================================
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
`);

categories.forEach(c => {
    const parentVal = c.parentId ? c.parentId : 'NULL';
    parts.push(`INSERT INTO Categories (Id, Name, CategoryCode, Slug, IsActive, CreatedAt, UpdatedAt, Description, IconUrl, MetaTitle, MetaDescription, ParentId)
VALUES (${c.id}, N'${c.name}', '${c.code}', '${c.slug}', 1, GETDATE(), GETDATE(), N'${c.desc}', '${c.icon}', N'${c.name} Chính Hãng', N'Mua ${c.name} chính hãng giá tốt nhất', ${parentVal});\n`);
});
parts.push(`SET IDENTITY_INSERT Categories OFF;\n\n`);

// 2. SEED BRANDS
parts.push(`-- ==========================================
-- 2. SEED BRANDS
-- ==========================================
PRINT N'---> Seeding Brands...';
SET IDENTITY_INSERT Brands ON;
`);

brands.forEach(b => {
    parts.push(`INSERT INTO Brands (Id, Name, BrandCode, Slug, Description, ImageUrl, IsActive, CreatedAt)
VALUES (${b.id}, N'${b.name}', '${b.code}', '${b.slug}', N'${b.desc}', '${b.img}', 1, GETDATE());\n`);
});
parts.push(`SET IDENTITY_INSERT Brands OFF;\n\n`);

// 3. SEED CATEGORY BRAND DEFAULTS
parts.push(`-- ==========================================
-- 3. SEED CATEGORY-BRAND RELATIONSHIPS
-- ==========================================
PRINT N'---> Seeding CategoryBrandDefaults...';
`);
categories.forEach(c => {
    brands.forEach(b => {
        parts.push(`INSERT INTO CategoryBrandDefaults (CategoryId, BrandId, CreatedAt, UpdatedAt) VALUES (${c.id}, ${b.id}, GETDATE(), GETDATE());\n`);
    });
});
parts.push(`\n`);

// 4. SEED PRODUCTS, VARIANTS, STOCKS
parts.push(`-- ==========================================
-- 4. SEED PRODUCTS, VARIANTS & STOCKS
-- ==========================================
PRINT N'---> Seeding Products & ProductVariants & Stock...';
DECLARE @ProdId INT;
DECLARE @VarId INT;
`);

products.forEach((p, pIdx) => {
    const imagesJson = JSON.stringify([p.img, p.img]).replace(/'/g, "''");
    const specsStr = p.specs.replace(/'/g, "''");
    const descStr = p.desc.replace(/'/g, "''");
    const nameStr = p.name.replace(/'/g, "''");

    parts.push(`
-- Product: ${p.name}
INSERT INTO Products (Name, ProductCode, Slug, Description, Specs, BasePrice, OriginalPrice, TotalStock, ReservedStock, IsActive, IsFeatured, CreatedAt, UpdatedAt, CategoryId, BrandId, ThumbnailImage, MainImage, Images)
VALUES (N'${nameStr}', '${p.code}', '${p.slug}', N'${descStr}', N'${specsStr}', ${p.basePrice}, ${p.origPrice}, 0, 0, 1, ${p.isFeatured ? 1 : 0}, GETDATE(), GETDATE(), ${p.catId}, ${p.brandId}, '${p.img}', '${p.img}', N'${imagesJson}');
SET @ProdId = SCOPE_IDENTITY();
`);

    p.variants.forEach((v, vIdx) => {
        const vAttr = v.attr.replace(/'/g, "''");
        const vName = v.name.replace(/'/g, "''");
        const costPrice = Math.round(v.price * 0.78);

        parts.push(`
INSERT INTO ProductVariants (Name, Sku, Price, TotalStock, ReservedStock, CreatedAt, UpdatedAt, IsActive, ProductId, ImageId, Attributes)
VALUES (N'${vName}', '${v.sku}', ${v.price}, ${v.stock}, 0, GETDATE(), GETDATE(), 1, @ProdId, '', N'${vAttr}');
SET @VarId = SCOPE_IDENTITY();

-- Initial Stock Batch (FIFO)
INSERT INTO Stock (ProductId, VariantId, QuantityIn, QuantityRemaining, Unit, Price, ReceivedDate)
VALUES (@ProdId, @VarId, ${v.stock}, ${v.stock}, N'Cái', ${costPrice}, DATEADD(day, -${10 + vIdx * 5}, GETDATE()));

-- Update total stock on Product
UPDATE Products SET TotalStock = TotalStock + ${v.stock} WHERE Id = @ProdId;
`);
    });
});

// 5. SEED BANNERS
parts.push(`
-- ==========================================
-- 5. SEED BANNERS
-- ==========================================
PRINT N'---> Seeding Banners...';
`);
banners.forEach((b, idx) => {
    parts.push(`INSERT INTO Banners (ImageUrl, LinkUrl, Type, IsActive, Position, IsDraft, CreatedAt)
VALUES ('${b.img}', '${b.link}', '${b.type}', 1, ${b.pos}, 0, GETDATE());\n`);
});

// 6. SEED PROMOTIONS
parts.push(`
-- ==========================================
-- 6. SEED PROMOTIONS
-- ==========================================
PRINT N'---> Seeding Promotions...';
`);
promotions.forEach(pr => {
    parts.push(`INSERT INTO Promotions (Code, DiscountType, DiscountValue, StartDate, EndDate, IsActive, UsageLimit, UsedCount, MinOrderAmount, MaxDiscountAmount, MaxPerUser)
VALUES ('${pr.code}', '${pr.type}', ${pr.val}, DATEADD(day, -${pr.startDaysAgo}, GETDATE()), DATEADD(day, ${pr.endDaysLater}, GETDATE()), 1, ${pr.limit}, ${pr.used}, ${pr.minOrd}, ${pr.maxDisc}, 2);\n`);
});

// 7. SEED SAMPLE USERS
parts.push(`
-- ==========================================
-- 7. SEED USERS
-- ==========================================
PRINT N'---> Seeding Sample Users...';
`);
const defaultPwdHash = 'AQAAAAEAACcQAAAAELhZ7uyPbdI/P5HnELm9jlcFgQAoKFKXUvnXUC/bsWY7NK8pjLvM1pBBh31Yz1Ya4w=='; // User123! or Admin123!

sampleUsers.forEach(u => {
    parts.push(`IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = '${u.email}')
BEGIN
    INSERT INTO Users (Id, Username, Email, PasswordHash, Role, IsActive, IsEmailVerified, FailedLoginCount, CreatedAt, RewardPoints, AccumulatedPoints)
    VALUES ('${u.guid}', '${u.user}', '${u.email}', '${defaultPwdHash}', '${u.role}', 1, 1, 0, DATEADD(day, -30, GETDATE()), ${u.points}, ${u.accum});
END\n`);
});

// 8. SEED REVIEWS
parts.push(`
-- ==========================================
-- 8. SEED REVIEWS
-- ==========================================
PRINT N'---> Seeding Product Reviews...';
DECLARE @RevProdId INT;
`);

sampleUsers.forEach((u, uIdx) => {
    sampleReviews.forEach((r, rIdx) => {
        const comm = r.comment.replace(/'/g, "''");
        const reply = r.reply ? `N'${r.reply.replace(/'/g, "''")}'` : 'NULL';
        const replyDate = r.reply ? `DATEADD(day, -${rIdx + 1}, GETDATE())` : 'NULL';
        
        parts.push(`
SELECT TOP 1 @RevProdId = Id FROM Products ORDER BY NEWID();
INSERT INTO Reviews (Rating, Comment, CreatedAt, AdminReply, RepliedAt, IsHidden, ProductId, UserId)
VALUES (${r.rating}, N'${comm}', DATEADD(day, -${(uIdx + 1) * 3 + rIdx}, GETDATE()), ${reply}, ${replyDate}, 0, @RevProdId, '${u.guid}');
`);
    });
});

// 9. SEED ORDERS & ORDER ITEMS
parts.push(`
-- ==========================================
-- 9. SEED SAMPLE ORDERS & REVENUE DATA
-- ==========================================
PRINT N'---> Seeding Sample Orders & OrderItems...';
DECLARE @OrdId INT;
DECLARE @OrdVarId INT;
DECLARE @OrdVarPrice DECIMAL(18,2);

`);

const orderStatuses = [
    { statusId: 4, name: 'Nguyễn Văn An', phone: '0903123456', addr: '123 Nguyễn Huệ, Phường Bến Nghé, Quận 1', daysAgo: 25, pm: 'Stripe' },
    { statusId: 4, name: 'Trần Thị Mai', phone: '0918234567', addr: '456 Điện Biên Phủ, Phường Đa Kao, Quận 1', daysAgo: 20, pm: 'VnPay' },
    { statusId: 4, name: 'Lê Hoàng Nam', phone: '0987654321', addr: '789 Lạc Long Quân, Phường 3, Quận 11', daysAgo: 15, pm: 'COD' },
    { statusId: 3, name: 'Phạm Minh Tuấn', phone: '0978112233', addr: '12 Hoàng Diệu, Phường Phước Ninh, Hải Châu', daysAgo: 3, pm: 'COD' },
    { statusId: 2, name: 'Vũ Hoàng Linh', phone: '0933445566', addr: '88 Nguyễn Chí Thanh, Phường Láng Hạ, Đống Đa', daysAgo: 1, pm: 'VnPay' },
    { statusId: 1, name: 'Nguyễn Văn An', phone: '0903123456', addr: '123 Nguyễn Huệ, Phường Bến Nghé, Quận 1', daysAgo: 0, pm: 'COD' }
];

orderStatuses.forEach((ord, oIdx) => {
    const userGuid = sampleUsers[oIdx % sampleUsers.length].guid;
    parts.push(`
-- Sample Order ${oIdx + 1}
INSERT INTO Orders (TotalPrice, CreatedAt, UserId, OrderStatusId, ReceiverName, ReceiverPhone, ShippingAddressLine, ShippingWard, ShippingProvince, PaymentMethod, PointsEarned, PointsRedeemed, DiscountFromPoints, AddonDiscountAmount)
VALUES (0, DATEADD(day, -${ord.daysAgo}, GETDATE()), '${userGuid}', ${ord.statusId}, N'${ord.name}', '${ord.phone}', N'${ord.addr}', N'Phường Bến Nghé', N'Hồ Chí Minh', '${ord.pm}', 100, 0, 0, 0);
SET @OrdId = SCOPE_IDENTITY();

-- Order Item 1
SELECT TOP 1 @OrdVarId = Id, @OrdVarPrice = Price FROM ProductVariants ORDER BY NEWID();
INSERT INTO OrderItems (Quantity, PriceAtPurchase, OrderId, VariantId, CampaignDiscountAmount, IsAddon, WarrantyPrice, InspectionStatus)
VALUES (1, @OrdVarPrice, @OrdId, @OrdVarId, 0, 0, 0, 'NOT_REQUIRED');

-- Update Order Total
UPDATE Orders SET TotalPrice = (SELECT SUM(Quantity * PriceAtPurchase) FROM OrderItems WHERE OrderId = @OrdId) WHERE Id = @OrdId;
`);
});

parts.push(`
COMMIT TRANSACTION;
PRINT N'======================================================';
PRINT N'SUCCESS: Full Shop Seed Data applied successfully!';
PRINT N'======================================================';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT N'ERROR occurred during seeding: ' + ERROR_MESSAGE();
END CATCH
`);

const finalSql = '\uFEFF' + parts.join('');
const outputPath = path.join(__dirname, 'seed_full_shop_data.sql');
fs.writeFileSync(outputPath, finalSql, 'utf8');
console.log(`[Success] SQL script generated at: ${outputPath}`);
