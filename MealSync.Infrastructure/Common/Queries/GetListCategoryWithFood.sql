/*
 CreatedBy: ThongNV
 Date: 15/10/2024
 
 @ShopId:=5 int
 @FilterMode int
 */
-- SET @ShopId:=2;
-- SET @CurrentHours:=2128;
-- SET @StartLastTwoHour:=1000;
-- SET @FilterMode:=1;
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
        f.id
    FROM
        food f
        LEFT JOIN food_operating_slot fos ON f.id = fos.food_id
        LEFT JOIN operating_slot os ON fos.operating_slot_id = os.id
    WHERE
        f.shop_id = @ShopId
        AND (
            @FilterMode = 0
            AND f.status IN (1, 2)
            OR @FilterMode = 1 -- IN time frame
            AND(
                os.start_time <= @CurrentHours
                AND os.end_time >= @CurrentHours
            )
            OR @FilterMode = 2 -- OUT CURRENT time frame
            AND(
                os.start_time > @CurrentHours
                OR os.end_time < @CurrentHours
            )
            OR @FilterMode = 3 -- Food selling
            AND f.status = 1
            AND f.is_sold_out = 0
            OR @FilterMode = 4 -- Food Sold OUT
            AND f.status = 1
            AND f.is_sold_out = 1
            OR @FilterMode = 5 -- Food Inactive
            AND f.status = 2
        )
    GROUP BY
        f.id
),
WithFoodAndOperatingSlot AS (
    SELECT
        f.id,
        fo.shop_id,
        fo.platform_category_id,
        fo.shop_category_id,
        fo.name,
        fo.description,
        fo.price,
        fo.image_url,
        fo.total_order,
        fo.status,
        fo.is_sold_out,
        os.id AS os_id,
        os.title,
        os.start_time,
        os.end_time,
        fo.created_date
    FROM
        WithFoodCondition f
        INNER JOIN food fo ON f.id = fo.id
        LEFT JOIN food_operating_slot fos ON f.id = fos.food_id
        LEFT JOIN operating_slot os ON fos.operating_slot_id = os.id
),
WitNumberOrderIn2HoursAdvance AS (
    SELECT
        f.id,
        o.id AS asdasdasd
    FROM
        WithFoodCondition f
        INNER JOIN order_detail od ON f.id = od.food_id
        INNER JOIN `order` o ON od.order_id = o.id
    WHERE
        o.start_time >= @StartLastTwoHour
    GROUP BY
        o.id,
        f.id
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
    f.name AS Name,
    f.price AS Price,
    f.image_url AS ImageUrl,
    f.total_order AS TotalOrder,
    f.status AS Status,
    f.is_sold_out AS IsSoldOut,
    (
        SELECT
            Count(*)
        FROM
            WitNumberOrderIn2HoursAdvance fn
        WHERE
            fn.id = f.id
    ) AS TotalOrderInNextTwoHours,
    f.os_id AS OperatingSection,
    f.os_id AS Id,
    f.title AS Title,
    f.start_time AS StartTime,
    f.end_time AS EndTime
FROM
    ShopCategoryTable AS sc
    LEFT JOIN WithFoodAndOperatingSlot f ON sc.id = f.shop_category_id
ORDER BY
    sc.display_order,
    sc.id,
    f.created_date;