# Migration: Add AllowedItemIdsJson to ProductOptionGroup

## SQL Script

```sql
-- Add AllowedItemIdsJson column to ProductOptionGroups table
ALTER TABLE ProductOptionGroups
ADD AllowedItemIdsJson nvarchar(max) NULL;

-- Update existing data (set NULL for all existing records)
-- NULL means "lấy tất cả items trong OptionGroup"
```

## Commands to run

### Using EF Core Migrations:

```bash
# 1. Tạo migration
dotnet ef migrations add AddAllowedItemIdsToProductOptionGroup --project CoffeeShopApi

# 2. Update database
dotnet ef database update --project CoffeeShopApi
```

### Or using SQL Server Management Studio:

```sql
USE CoffeeShopDb;
GO

ALTER TABLE ProductOptionGroups
ADD AllowedItemIdsJson nvarchar(max) NULL;
GO
```

## Verification

After running migration, verify the column exists:

```sql
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ProductOptionGroups'
AND COLUMN_NAME = 'AllowedItemIdsJson';
```

Expected result:
```
COLUMN_NAME           DATA_TYPE    IS_NULLABLE
AllowedItemIdsJson    nvarchar     YES
```

## Test Data

Insert test data to verify:

```sql
-- Tạo Product với filter (chỉ Size S và M)
INSERT INTO ProductOptionGroups (ProductId, OptionGroupId, DisplayOrder, AllowedItemIdsJson)
VALUES (1, 1, 1, '[1,2]');

-- Tạo Product không filter (lấy tất cả)
INSERT INTO ProductOptionGroups (ProductId, OptionGroupId, DisplayOrder, AllowedItemIdsJson)
VALUES (2, 1, 1, NULL);
```

## Rollback (if needed)

```sql
ALTER TABLE ProductOptionGroups
DROP COLUMN AllowedItemIdsJson;
```
