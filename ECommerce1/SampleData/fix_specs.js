const fs = require('fs');

const products = [
    { cat: 12, slug: 'anker-powercore-10000', specs: '{"Dung lượng pin": "10000mAh", "Công suất": "12W", "Cổng sạc": "1 USB-A, 1 Micro USB"}' },
    { cat: 12, slug: 'xiaomi-redmi-20000', specs: '{"Dung lượng pin": "20000mAh", "Công suất": "18W", "Cổng sạc": "2 USB-A, 1 Type-C"}' },
    { cat: 12, slug: 'baseus-bipow-15w-10000', specs: '{"Dung lượng pin": "10000mAh", "Công suất": "15W", "Cổng sạc": "2 USB-A, 1 Type-C"}' },
    { cat: 13, slug: 'apple-20w-type-c', specs: '{"Công suất": "20W", "Cổng sạc": "1 Type-C", "Tương thích": "iPhone, iPad"}' },
    { cat: 13, slug: 'anker-powerline-iii', specs: '{"Chiều dài": "0.9m", "Công suất": "Tối đa 60W"}' },
    { cat: 13, slug: 'samsung-25w-type-c', specs: '{"Công suất": "25W", "Cổng sạc": "1 Type-C"}' },
    { cat: 14, slug: 'ip15-pm-clear-case', specs: '{"Chất liệu": "Polycarbonate", "Tính năng": "Hỗ trợ MagSafe"}' },
    { cat: 14, slug: 's24-ultra-silicone', specs: '{"Chất liệu": "Silicone", "Tính năng": "Chống sốc"}' },
    { cat: 14, slug: 'baseus-wing-ip14', specs: '{"Chất liệu": "Nhựa PP", "Tính năng": "Siêu mỏng 0.4mm"}' },
    { cat: 15, slug: 'ipad-pro-11-folio', specs: '{"Chất liệu": "Polyurethane", "Tính năng": "Đóng mở màn hình tự động"}' },
    { cat: 15, slug: 'tab-s9-smart-cover', specs: '{"Chất liệu": "Da PU", "Tính năng": "Kháng khuẩn"}' },
    { cat: 15, slug: 'baseus-ipad-air5', specs: '{"Chất liệu": "Da nhân tạo", "Tính năng": "Gắn từ tính"}' },
    { cat: 16, slug: 'baseus-glass-ip15', specs: '{"Chất liệu": "Kính cường lực", "Độ dày": "0.3mm"}' },
    { cat: 16, slug: 'anker-glass-ip14', specs: '{"Độ cứng": "9H", "Tính năng": "Chống xước"}' },
    { cat: 16, slug: 'ss-glass-s23', specs: '{"Chất liệu": "Film PET", "Tính năng": "Chống chói"}' },
    { cat: 17, slug: 'baseus-lanyard-1', specs: '{"Chất liệu": "Nylon", "Độ dài": "Tùy chỉnh"}' },
    { cat: 17, slug: 'ringke-lanyard-1', specs: '{"Chất liệu": "Vải dù", "Độ dài": "40cm"}' },
    { cat: 17, slug: 'spigen-wrist-1', specs: '{"Chất liệu": "Dacron dệt kim", "Độ dài": "20cm"}' },
    { cat: 18, slug: 'airpods-pro-case-1', specs: '{"Chất liệu": "Silicone", "Tương thích": "AirPods Pro"}' },
    { cat: 18, slug: 'buds2-pro-case-1', specs: '{"Chất liệu": "Nhựa PC", "Tương thích": "Galaxy Buds2 Pro"}' },
    { cat: 18, slug: 'baseus-tws-pouch', specs: '{"Chất liệu": "Vải nỉ EVA", "Tính năng": "Kéo khóa"}' },
    { cat: 19, slug: 'baseus-stand-1', specs: '{"Chất liệu": "Hợp kim nhôm", "Khả năng xoay": "Lên xuống 35 độ"}' },
    { cat: 19, slug: 'anker-mount-1', specs: '{"Chất liệu": "Nhựa ABS", "Khả năng xoay": "360 độ"}' },
    { cat: 19, slug: 'xiaomi-tripod-1', specs: '{"Chất liệu": "Nhựa, Nhôm", "Kết nối": "Bluetooth"}' },
    { cat: 20, slug: 'sandisk-128gb-1', specs: '{"Dung lượng": "128GB", "Tốc độ đọc": "200MB/s"}' },
    { cat: 20, slug: 'kingston-64gb-1', specs: '{"Dung lượng": "64GB", "Tốc độ đọc": "170MB/s"}' },
    { cat: 20, slug: 'samsung-256gb-1', specs: '{"Dung lượng": "256GB", "Tốc độ đọc": "130MB/s"}' },
    { cat: 21, slug: 'sandisk-usb-64gb-1', specs: '{"Dung lượng": "64GB", "Kết nối": "Type-C và Type-A"}' },
    { cat: 21, slug: 'kingston-usb-32gb-1', specs: '{"Dung lượng": "32GB", "Kết nối": "Type-A USB 3.2"}' },
    { cat: 21, slug: 'samsung-usb-128gb-1', specs: '{"Dung lượng": "128GB", "Tốc độ đọc": "400MB/s"}' },
    { cat: 22, slug: 'samsung-t7-500gb-1', specs: '{"Dung lượng": "500GB", "Tốc độ đọc": "1050MB/s"}' },
    { cat: 22, slug: 'sandisk-ssd-1tb-1', specs: '{"Dung lượng": "1TB", "Tốc độ đọc": "1050MB/s"}' },
    { cat: 22, slug: 'wd-hdd-2tb-1', specs: '{"Dung lượng": "2TB", "Kết nối": "USB 3.2 Gen 1"}' }
];

let sql = `USE csdl_phone;\nGO\n\n`;

for (const p of products) {
    const parsed = JSON.parse(p.specs);
    const items = Object.keys(parsed).map(key => ({ key: key, value: parsed[key] }));
    
    const formattedSpecs = [
        {
            groupName: "Thông số kỹ thuật",
            items: items
        }
    ];

    const specsJson = JSON.stringify(formattedSpecs);
    sql += `UPDATE Products SET Specs = N'${specsJson}' WHERE Slug = '${p.slug}';\n`;
}

sql += `GO\n`;

fs.writeFileSync('fix_product_specs.sql', sql);
console.log("SQL script generated at fix_product_specs.sql");
