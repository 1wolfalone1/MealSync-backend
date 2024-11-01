﻿/*
 CreatedBy: ThongNV
 Date: 16/10/2024
 
 @PageSize
 @DeliveryPackageId
 */
-- SET @ShopId:=2;
-- SET @DeliveryPackageId:=11;
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
        created_date
    FROM
        `order` o
    WHERE
        o.shop_id = @ShopId
        AND delivery_package_id = @DeliveryPackageId
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
    -- Customer
    o.customer_id AS CustomerSection,
    o.customer_id AS Id,
    o.full_name AS FullName,
    o.phone_number AS PhoneNumber,
    -- Shop Delivery Staff or Shop Owner Information (conditional)
    dp.id AS ShopDeliverySection,
    dp.id AS DeliveryPackageId,
    CASE
        WHEN dp.shop_delivery_staff_id IS NOT NULL THEN dp.shop_delivery_staff_id
        ELSE 0
    END AS Id,
    CASE
        WHEN dp.shop_delivery_staff_id IS NOT NULL THEN accShip.full_name
        ELSE accShop.full_name
    END AS FullName,
    CASE
        WHEN dp.shop_delivery_staff_id IS NOT NULL THEN accShip.avatar_url
        ELSE accShop.avatar_url
    END AS AvatarUrl,
    CASE
        WHEN dp.shop_delivery_staff_id IS NULL THEN TRUE
        ELSE FALSE
    END AS IsShopOwnerShip,
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
    LEFT JOIN account accShop ON dp.shop_id = accShop.id
    INNER JOIN order_detail od ON o.id = od.order_id
    INNER JOIN food f ON od.food_id = f.id
ORDER BY
    o.start_time ASC,
    o.order_date ASC;