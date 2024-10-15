/*
 CreatedBy: ThongNV
 Date: 15/10/2024
 
 @ShopId:=5 int
 @Offset int
 @PageSize int
 */

-- SET @ShopId:=5;
-- SET @PageSize:=12;
-- SET @OffSet:=0;
WITH ShopCategoryTable AS (
    SELECT
        id,
        shop_id,
        name,
        description,
        image_url,
        display_order
    FROM
        shop_category AS sc
    WHERE
        sc.shop_id = @ShopId
    ORDER BY
        sc.display_order
),
WithFoodCondition AS (
    SELECT
        id,
        shop_id,
        platform_category_id,
        shop_category_id,
        name,
        description,
        price,
        image_url,
        total_order,
        status,
        is_sold_out
    FROM
        food f
    WHERE
        f.status IN (1, 2) -- Active AND InActive
)
SELECT
    sc.id AS Id,
    sc.name AS Name,
    sc.description AS Description,
    sc.image_url AS ImageUrl,
    sc.display_order AS DisplayOrder,
    f.id AS FoodId,
    f.id AS Id,
    f.platform_category_id AS PlatformCategory,
    f.description AS Description,
    f.price AS Price,
    f.image_url AS ImageUrl,
    f.total_order AS TotalOrder,
    f.status AS Status,
    f.is_sold_out AS IsSoldOut
FROM
    ShopCategoryTable AS sc
    LEFT JOIN WithFoodCondition f ON sc.id = f.shop_category_id
-- LIMIT
--     @PageSize OFFSET @OffSet;