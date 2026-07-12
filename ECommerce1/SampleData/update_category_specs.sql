USE csdl_phone;
GO

UPDATE Categories SET SpecsTemplate = N'[{"groupName":"Thông số kỹ thuật","items":["Dung lượng pin","Công suất","Cổng sạc"]}]' WHERE Id = 12;
UPDATE Categories SET SpecsTemplate = N'[{"groupName":"Thông số kỹ thuật","items":["Công suất","Cổng sạc","Chiều dài","Tương thích"]}]' WHERE Id = 13;
UPDATE Categories SET SpecsTemplate = N'[{"groupName":"Thông số kỹ thuật","items":["Chất liệu","Tính năng"]}]' WHERE Id = 14;
UPDATE Categories SET SpecsTemplate = N'[{"groupName":"Thông số kỹ thuật","items":["Chất liệu","Tính năng"]}]' WHERE Id = 15;
UPDATE Categories SET SpecsTemplate = N'[{"groupName":"Thông số kỹ thuật","items":["Chất liệu","Độ dày","Độ cứng","Tính năng"]}]' WHERE Id = 16;
UPDATE Categories SET SpecsTemplate = N'[{"groupName":"Thông số kỹ thuật","items":["Chất liệu","Độ dài"]}]' WHERE Id = 17;
UPDATE Categories SET SpecsTemplate = N'[{"groupName":"Thông số kỹ thuật","items":["Chất liệu","Tương thích","Tính năng"]}]' WHERE Id = 18;
UPDATE Categories SET SpecsTemplate = N'[{"groupName":"Thông số kỹ thuật","items":["Chất liệu","Khả năng xoay","Kết nối","Chiều dài tối đa"]}]' WHERE Id = 19;
UPDATE Categories SET SpecsTemplate = N'[{"groupName":"Thông số kỹ thuật","items":["Dung lượng","Tốc độ đọc","Tốc độ ghi","Chuẩn"]}]' WHERE Id = 20;
UPDATE Categories SET SpecsTemplate = N'[{"groupName":"Thông số kỹ thuật","items":["Dung lượng","Kết nối","Tốc độ đọc","Chất liệu","Thiết kế"]}]' WHERE Id = 21;
UPDATE Categories SET SpecsTemplate = N'[{"groupName":"Thông số kỹ thuật","items":["Dung lượng","Tốc độ đọc","Kết nối"]}]' WHERE Id = 22;

GO
