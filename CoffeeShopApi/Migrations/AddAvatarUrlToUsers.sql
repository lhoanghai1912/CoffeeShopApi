-- Migration: Add AvatarUrl to Users table

USE CoffeeShopDb;
GO

-- Thêm column AvatarUrl
ALTER TABLE Users
ADD AvatarUrl NVARCHAR(500) NULL;
GO

-- Comment
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'URL của ảnh đại diện (avatar) của user', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'Users',
    @level2type = N'COLUMN', @level2name = N'AvatarUrl';
GO

PRINT '✅ Added AvatarUrl column to Users table';
GO

-- Verify
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'AvatarUrl';
GO
