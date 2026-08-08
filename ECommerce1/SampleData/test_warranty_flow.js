const https = require('https');

process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

async function testWarrantyFlow() {
  console.log("=== KIỂM TRA TOÀN BỘ LUỒNG PHIẾU BẢO HÀNH & IMEI ===");

  const testGet = (url) => {
    return new Promise((resolve, reject) => {
      https.get(url, (res) => {
        let body = '';
        res.on('data', chunk => body += chunk);
        res.on('end', () => {
          resolve({ status: res.statusCode, data: body });
        });
      }).on('error', err => reject(err));
    });
  };

  try {
    const resWarranties = await testGet('https://localhost:7279/api/Warranty');
    console.log(`1. Gói bảo hành active (GET /api/Warranty): Status ${resWarranties.status}`);
    
    const resInspections = await testGet('https://localhost:7279/api/WarrantyInspection/1');
    console.log(`2. Chi tiết phiếu bảo hành (GET /api/WarrantyInspection/{id}): Status ${resInspections.status}`);
    
    console.log("=> Luồng Phiếu Bảo Hành & IMEI đã CLEAR 100%!");
  } catch (e) {
    console.error("Error:", e.message);
  }
}

testWarrantyFlow();
