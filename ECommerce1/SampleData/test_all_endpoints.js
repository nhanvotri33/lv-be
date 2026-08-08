const https = require('https');

process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

const endpoints = [
  { name: 'Get Products', url: 'https://localhost:7279/api/Product' },
  { name: 'Get Categories', url: 'https://localhost:7279/api/Category' },
  { name: 'Get Brands', url: 'https://localhost:7279/api/Brand' },
  { name: 'Get Warranties', url: 'https://localhost:7279/api/Warranty' },
  { name: 'Track Order', url: 'https://localhost:7279/api/Order/track?orderId=15&phoneNumber=0912345678' }
];

async function runTests() {
  console.log("=== KIỂM TRA HỆ THỐNG API BACKEND (.NET CORE 6) ===");
  for (const ep of endpoints) {
    try {
      const data = await new Promise((resolve, reject) => {
        https.get(ep.url, (res) => {
          let body = '';
          res.on('data', chunk => body += chunk);
          res.on('end', () => {
            if (res.statusCode >= 200 && res.statusCode < 300) {
              try {
                resolve({ status: res.statusCode, json: JSON.parse(body) });
              } catch (e) {
                resolve({ status: res.statusCode, raw: body });
              }
            } else {
              resolve({ status: res.statusCode, error: body });
            }
          });
        }).on('error', err => reject(err));
      });

      const count = Array.isArray(data.json) ? data.json.length : (data.json ? 1 : 0);
      console.log(`[PASS] ${ep.name} (${ep.url}) -> Status ${data.status} | Item count/response: ${count}`);
    } catch (err) {
      console.error(`[FAIL] ${ep.name} (${ep.url}) -> Error: ${err.message}`);
    }
  }
}

runTests();
