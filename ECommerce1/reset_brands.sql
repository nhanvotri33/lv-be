DELETE FROM Brands WHERE Id IN (6, 7);
DBCC CHECKIDENT ('Brands', RESEED, 0);
INSERT INTO Brands (Name, Slug, Description, ImageUrl, IsActive, CreatedAt)
VALUES 
('Samsung', 'samsung', 'Samsung Electronics .Inc', 'https://res.cloudinary.com/daz0oicfs/image/upload/v1781522322/ecommerce_uploads/d4zg8qqjlajvcyoeg1bd.webp', 1, GETDATE()),
('Apple', 'apple', 'Apple .Inc From USA', 'https://res.cloudinary.com/daz0oicfs/image/upload/v1781522383/ecommerce_uploads/znqus8dzzlxclvkpc3wh.png', 1, GETDATE());
