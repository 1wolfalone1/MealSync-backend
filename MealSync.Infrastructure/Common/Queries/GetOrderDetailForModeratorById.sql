/*
 CreatedBy: ThongNV
 Date: 16/10/2024
 
 @OrderId
 */
-- SET @OrderId:=1;
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
        resolve_at,
        cancel_at,
        reason_identity,
        evidence_delivery_fail_json,
        reject_at,
        lastest_delivery_fail_at,
        is_paid_to_shop,
        is_refund,
        is_report,
        reason
    FROM
        `order` o
    WHERE
        o.id = @OrderId
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
    o.total_promotion AS TotalPromotion,
    o.total_price AS TotalPrice,
    o.order_date AS OrderDate,
    o.receive_at AS ReceiveAt,
    o.note AS Note,
    o.completed_at AS CompletedAt,
    o.intended_receive_date AS IntendedReceiveDate,
    o.start_time AS StartTime,
    o.end_time AS EndTime,
    o.resolve_at AS ResolveAt,
    o.cancel_at AS CancelAt,
    o.reason_identity AS ReasonIdentity,
    o.evidence_delivery_fail_json AS EvidenceDeliveryFailJson,
    o.reject_at AS RejectAt,
    o.lastest_delivery_fail_at AS LatestDeliveryFailAt,
    o.is_paid_to_shop AS IsPaidToShop,
    o.is_refund AS IsRefund,
    o.is_report AS IsReport,
    o.reason AS Reason,
    d.id AS DormitoryId,
    d.name AS DormitoryName,
    -- Customer
    accCus.id AS CustomerSection,
    accCus.id AS Id,
    o.full_name AS FullName,
    o.phone_number AS PhoneNumber,
    accCus.avatar_url AS AvartarUrl,
    l.id AS LocationId,
    l.address AS Address,
    l.latitude AS Latitude,
    l.longitude AS Longitude,
    -- Promotion
    p.id AS PromotionSection,
    p.id AS Id,
    p.title AS Title,
    p.description AS Description,
    p.banner_url AS BannerUrl,
    p.amount_rate AS AmountRate,
    p.amount_value AS AmountValue,
    p.min_ordervalue AS MinOrderValue,
    p.apply_type AS ApplyType,
    p.maximum_apply_value AS MaximumApplyValue,
    -- Shop Delivery Staff or Shop Owner Information (conditional)
    dp.id AS DeliveryPackageSection,
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
        WHEN dp.shop_delivery_staff_id IS NOT NULL THEN accShip.phone_number
        ELSE accShop.phone_number
    END AS PhoneNumber,
    CASE
        WHEN dp.shop_delivery_staff_id IS NOT NULL THEN accShip.email
        ELSE accShop.email
    END AS Email,
    CASE
        WHEN dp.shop_delivery_staff_id IS NULL THEN TRUE
        ELSE FALSE
    END AS IsShopOwnerShip,
    -- OrderDetail
    od.id AS OrderDetailSection,
    od.id AS Id,
    od.food_id AS FoodId,
    f.name AS Name,
    f.image_url AS ImageUrl,
    f.description AS Description,
    od.quantity AS Quantity,
    od.total_price AS TotalPrice,
    od.basic_price AS BasicPrice,
    od.description AS OrderDescription,
    od.note AS Note
FROM
    OrdersOfShop o
    INNER JOIN building b ON o.building_id = b.id
    INNER JOIN dormitory d ON b.dormitory_id = d.id
    INNER JOIN location l ON o.customer_location_id = l.id
    LEFT JOIN promotion p ON o.promotion_id = p.id
    INNER JOIN account accCus ON o.customer_id = accCus.id
    LEFT JOIN delivery_package dp ON o.delivery_package_id = dp.id
    LEFT JOIN account accShip ON dp.shop_delivery_staff_id = accShip.id
    LEFT JOIN account accShop ON dp.shop_id = accShop.id
    INNER JOIN order_detail od ON o.id = od.order_id
    INNER JOIN food f ON od.food_id = f.id
ORDER BY
    o.start_time ASC,
    o.order_date ASC;