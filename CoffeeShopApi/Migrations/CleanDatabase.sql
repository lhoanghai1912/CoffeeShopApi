-- Script để clean database khi bị lỗi duplicate data
-- Chạy script này trong SQL Server Management Studio hoặc Azure Data Studio

USE CoffeeShopDb;
GO

PRINT '🧹 Starting database cleanup...';

-- Tắt foreign key constraints tạm thời
PRINT 'Disabling foreign key constraints...';
EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';
GO

-- Xóa data theo thứ tự (child tables trước)
PRINT 'Deleting OrderItemOptions...';
DELETE FROM OrderItemOptions;

PRINT 'Deleting OrderItems...';
DELETE FROM OrderItems;

PRINT 'Deleting Orders...';
DELETE FROM Orders;

PRINT 'Deleting ProductOptionGroups...';
DELETE FROM ProductOptionGroups;

PRINT 'Deleting OptionItems...';
DELETE FROM OptionItems;

PRINT 'Deleting OptionGroups...';
DELETE FROM OptionGroups;

PRINT 'Deleting Products...';
DELETE FROM Products;

PRINT 'Deleting Categories...';
DELETE FROM Categories;

PRINT 'Deleting UserVouchers...';
DELETE FROM UserVouchers;

PRINT 'Deleting OrderVouchers...';
DELETE FROM OrderVouchers;

PRINT 'Deleting VoucherUsages...';
DELETE FROM VoucherUsages;

PRINT 'Deleting Vouchers...';
DELETE FROM Vouchers;

PRINT 'Deleting UserAddresses...';
DELETE FROM UserAddresses;

PRINT 'Deleting UserRoles...';
DELETE FROM UserRoles;

PRINT 'Deleting RolePermissions...';
DELETE FROM RolePermissions;

-- Bật lại foreign key constraints
PRINT 'Re-enabling foreign key constraints...';
EXEC sp_MSForEachTable 'ALTER TABLE ? CHECK CONSTRAINT ALL';
GO

-- Reset identity cho các bảng chính
PRINT 'Resetting identity seeds...';
DBCC CHECKIDENT ('OptionGroups', RESEED, 0);
DBCC CHECKIDENT ('OptionItems', RESEED, 0);
DBCC CHECKIDENT ('Products', RESEED, 0);
DBCC CHECKIDENT ('Categories', RESEED, 0);
DBCC CHECKIDENT ('Orders', RESEED, 0);
DBCC CHECKIDENT ('OrderItems', RESEED, 0);
DBCC CHECKIDENT ('OrderItemOptions', RESEED, 0);
DBCC CHECKIDENT ('ProductOptionGroups', RESEED, 0);
DBCC CHECKIDENT ('Vouchers', RESEED, 0);
DBCC CHECKIDENT ('VoucherUsages', RESEED, 0);
DBCC CHECKIDENT ('UserAddresses', RESEED, 0);
GO

PRINT '✅ Database cleanup completed!';
PRINT '';
PRINT '🚀 Next steps:';
PRINT '1. Run your application';
PRINT '2. Data will be seeded automatically on first start';
GO

-- Verify cleanup
SELECT 
    'OptionGroups' as TableName, COUNT(*) as RowCount FROM OptionGroups
UNION ALL SELECT 'OptionItems', COUNT(*) FROM OptionItems
UNION ALL SELECT 'Products', COUNT(*) FROM Products
UNION ALL SELECT 'Categories', COUNT(*) FROM Categories
UNION ALL SELECT 'Orders', COUNT(*) FROM Orders
UNION ALL SELECT 'Vouchers', COUNT(*) FROM Vouchers
UNION ALL SELECT 'UserAddresses', COUNT(*) FROM UserAddresses;
GO
