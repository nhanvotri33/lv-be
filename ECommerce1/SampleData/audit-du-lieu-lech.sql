-- ==========================================================================
-- MODULE: audit-du-lieu-lech.sql
-- MỤC ĐÍCH: Rà soát hai tập dữ liệu đã lệch từ TRƯỚC khi các lỗi dưới đây được vá.
--           Script này CHỈ ĐỌC, không sửa gì. Chạy trên database thật để biết
--           phạm vi ảnh hưởng trước khi quyết định cách khắc phục.
--
-- Bối cảnh:
--  (1) Đơn thanh toán online (VNPAY/Stripe) trước đây được xác nhận bằng cách gán
--      thẳng Orders.OrderStatusId = 2 trong PaymentController, bỏ qua
--      OrderService.UpdateOrderStatusAsync - nơi duy nhất trừ TotalStock và nhả
--      ReservedStock. Hệ quả: tồn kho cao hơn thực tế.
--  (2) Giao dịch kho IMPORT_RETURN trước đây tự đẩy đơn sang OrderStatusId = 7 mà
--      không cần yêu cầu đổi trả nào được duyệt, nên tiền hoàn, điểm thưởng và bản
--      ghi Payments của những đơn đó chưa hề được xử lý.
-- ==========================================================================

PRINT '=== (1) ĐƠN THANH TOÁN ONLINE THÀNH CÔNG NHƯNG NGHI CHƯA TRỪ KHO ===';
-- Dấu hiệu: đơn đã thanh toán online thành công và đã rời trạng thái Chờ thanh toán,
-- nhưng KHÔNG có bản ghi xuất kho EXPORT_SELL nào cho các biến thể của đơn.
SELECT
    o.Id                AS DonHang,
    o.OrderStatusId     AS TrangThai,
    o.PaymentMethod     AS HinhThuc,
    p.Provider          AS Cong,
    p.Status            AS TrangThaiTT,
    o.TotalPrice        AS TongTien,
    o.CreatedAt         AS NgayDat
FROM Orders o
JOIN Payments p ON p.OrderId = o.Id
WHERE p.Status = 'succeeded'
  AND p.Provider <> 'COD'
  AND o.OrderStatusId IN (2, 3, 4)
  AND NOT EXISTS (
        SELECT 1
        FROM InventoryTransactions t
        JOIN OrderItems oi ON oi.VariantId = t.VariantId AND oi.OrderId = o.Id
        WHERE t.TransactionType = 'EXPORT_SELL'
          AND t.CreatedAt >= o.CreatedAt
  )
ORDER BY o.CreatedAt DESC;

PRINT '';
PRINT '=== (1b) SO LUONG CAN TRU KHO BU CHO TUNG BIEN THE ===';
SELECT
    oi.VariantId,
    pv.Name             AS BienThe,
    SUM(oi.Quantity)    AS SoLuongCanTru,
    pv.TotalStock       AS TonKhoHienTai,
    pv.ReservedStock    AS DangGiuCho
FROM Orders o
JOIN Payments p   ON p.OrderId = o.Id
JOIN OrderItems oi ON oi.OrderId = o.Id
JOIN ProductVariants pv ON pv.Id = oi.VariantId
WHERE p.Status = 'succeeded'
  AND p.Provider <> 'COD'
  AND o.OrderStatusId IN (2, 3, 4)
  AND NOT EXISTS (
        SELECT 1
        FROM InventoryTransactions t
        JOIN OrderItems oi2 ON oi2.VariantId = t.VariantId AND oi2.OrderId = o.Id
        WHERE t.TransactionType = 'EXPORT_SELL'
          AND t.CreatedAt >= o.CreatedAt
  )
GROUP BY oi.VariantId, pv.Name, pv.TotalStock, pv.ReservedStock
ORDER BY SoLuongCanTru DESC;

PRINT '';
PRINT '=== (2) ĐƠN BỊ ĐẨY SANG "ĐÃ HOÀN TIỀN" MÀ KHÔNG QUA DUYỆT ĐỔI TRẢ ===';
-- Đơn đang ở trạng thái 7 nhưng không có ReturnRequest nào được duyệt.
-- Với những đơn này: tiền chưa hoàn về cổng, điểm chưa trả lại, Payments chưa cập nhật.
SELECT
    o.Id                AS DonHang,
    o.TotalPrice        AS TongTien,
    o.PointsRedeemed    AS DiemDaTieu,
    o.PointsEarned      AS DiemDaCong,
    o.PaymentMethod     AS HinhThuc,
    p.Provider          AS Cong,
    p.Status            AS TrangThaiTT,
    o.CreatedAt         AS NgayDat
FROM Orders o
LEFT JOIN Payments p ON p.OrderId = o.Id
WHERE o.OrderStatusId = 7
  AND NOT EXISTS (
        SELECT 1 FROM ReturnRequests r
        WHERE r.OrderId = o.Id AND r.Status = 1   -- 1 = Approved
  )
ORDER BY o.CreatedAt DESC;

PRINT '';
PRINT '=== (2b) GIAO DICH IMPORT_RETURN KHONG GAN VOI YEU CAU DOI TRA NAO ===';
SELECT
    t.Id                AS GiaoDich,
    t.VariantId,
    pv.Name             AS BienThe,
    t.QuantityChanged   AS SoLuong,
    t.Note              AS GhiChu,
    t.CreatedAt         AS ThoiDiem
FROM InventoryTransactions t
JOIN ProductVariants pv ON pv.Id = t.VariantId
WHERE t.TransactionType = 'IMPORT_RETURN'
  AND t.ReturnRequestId IS NULL
ORDER BY t.CreatedAt DESC;

PRINT '';
PRINT '=== (3) DONG HANG CHUA KHAI GIA VON (bi loai khoi bao cao loi nhuan) ===';
SELECT
    pr.Name             AS SanPham,
    pv.Name             AS BienThe,
    COUNT(*)            AS SoDongHang,
    SUM(oi.Quantity)    AS SoLuongDaBan,
    SUM(oi.PriceAtPurchase * oi.Quantity) AS DoanhThu
FROM OrderItems oi
JOIN Orders o  ON o.Id = oi.OrderId
JOIN ProductVariants pv ON pv.Id = oi.VariantId
JOIN Products pr ON pr.Id = pv.ProductId
WHERE o.OrderStatusId = 4
  AND oi.CostPriceAtPurchase <= 0
  AND ISNULL(pv.CostPrice, 0) <= 0
  AND ISNULL(pr.CostPrice, 0) <= 0
GROUP BY pr.Name, pv.Name
ORDER BY DoanhThu DESC;
