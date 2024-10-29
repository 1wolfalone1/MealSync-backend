/*
 CreatedBy: ThongNV
 Date: 16/10/2024
 
 @PageSize
 @OffSet
 @ShopId
 @Status int[]
 @IntendedRecieveDate
 @StartTime
 @EndTime
 @PhoneNumber
 @OrderId 
 */
-- SET @ShopId:=2;
-- SET @Status:='1, 2';
-- SET @IntendedRecieveDate:='2024-10-16';
-- SET @StartTime:=0;
-- SET @EndTime:=2300;
-- SET @PhoneNumber:=NULL;
-- SET @OrderId:=NULL;
WITH OrdersOfShop AS (
    SELECT
        id,
        promotion_id,
        shop_id,
        customer_id,
        delivery_package_id,
        shop_location_id,
        customer_location_id,
        building_id,
        building_name,
        status,
        note,
        shipping_fee,
        total_price,
        total_promotion,
        charge_fee,
        full_name,
        phone_number,
        order_date,
        receive_at,
        completed_at,
        start_time,
        end_time,
        intended_receive_date,
        created_date,
        ROW_NUMBER() OVER (
            ORDER BY
                o.id
        ) AS RowNum,
        COUNT(id) OVER () AS TotalPages
    FROM
        `order` o
    WHERE
        o.shop_id = @ShopId
        AND (
            @IntendedRecieveDate IS NULL
            OR DATE_FORMAT(o.intended_receive_date, '%Y-%m-%d') = @IntendedRecieveDate
        )
        AND o.status IN @Status
        AND (
            @OrderId IS NOT NULL
            AND CONCAT('MS-', o.id) LIKE CONCAT('%', @OrderId, '%')
            OR @PhoneNumber IS NOT NULL
            AND o.phone_number LIKE CONCAT('%', CONCAT(@PhoneNumber, '%'))
            OR @OrderId IS NULL
            AND @PhoneNumber IS NULL
        )
        AND (
            o.start_time >= @StartTime
            AND o.end_time <= @EndTime
        )
    ORDER BY
        o.start_time ASC,
        o.order_date ASC
)
SELECT
    -- Order
    o.id AS Id,
    o.status AS Status,
    o.building_id AS BuildingId,
    o.building_name AS BuildingName,
    o.total_price AS TotalPrice,
    o.total_promotion AS TotalPromotion,
    o.order_date AS OrderDate,
    o.receive_at AS ReceiveAt,
    o.completed_at AS CompletedAt,
    o.start_time AS StartTime,
    o.end_time AS EndTime,
    o.intended_receive_date AS IntendedReceiveDate,
    o.created_date AS CreatedDate,
    d.id AS DormitoryId,
    d.name AS DormitoryName,
    TotalPages,
    -- Customer
    o.customer_id AS CustomerSection,
    o.customer_id AS Id,
    o.full_name AS FullName,
    accCus.avatar_url AS AvatarUrl,
    -- Shop Delivery Staff
    dp.id AS ShopDeliverySection,
    dp.id AS DeliveryPackageId,
    accShip.id AS Id,
    accShip.full_name AS FullName,
    accShip.avatar_url AS AvatarUrl,
    (dp.shop_delivery_staff_id IS NULL) AS IsShopOwnerShip,
    -- Food
    f.id AS FoodSection,
    f.id AS Id,
    f.name AS Name,
    f.image_url AS ImageUrl,
    od.quantity AS Quantity
FROM
    OrdersOfShop o
    INNER JOIN building b ON o.building_id = b.id
    INNER JOIN dormitory d ON b.dormitory_id = d.id
    INNER JOIN account accCus ON o.customer_id = accCus.id
    LEFT JOIN delivery_package dp ON o.delivery_package_id = dp.id
    LEFT JOIN account accShip ON dp.shop_delivery_staff_id = accShip.id
    INNER JOIN order_detail od ON o.id = od.order_id
    INNER JOIN food f ON od.food_id = f.id
WHERE
    RowNum BETWEEN (@PageIndex - 1) * @PageSize + 1
    AND @PageIndex * @PageSize
ORDER BY
    o.start_time ASC,
    o.order_date ASC;