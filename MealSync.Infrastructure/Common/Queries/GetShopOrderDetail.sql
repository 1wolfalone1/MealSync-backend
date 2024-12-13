/*
 CreatedBy: ThongNV
 Date: 17/10/2024
 
 @OrderDetail 
 */
-- SET @OrderId:=6;
WITH OrderInfor AS (
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
        resolve_at,
        cancel_at,
        reason_identity,
        delivery_success_image_url,
        evidence_delivery_fail_json,
        reject_at,
        lastest_delivery_fail_at,
        is_refund,
        is_report,
        reason,
        intended_receive_date
    FROM
        `order` o
    WHERE
        o.id = @OrderId
),
OrderDetailWithFood AS (
    SELECT
        od.id,
        od.food_id,
        f.name AS f_name,
        f.description AS f_desc,
        f.image_url AS f_image_url,
        od.quantity,
        od.order_id,
        od.total_price,
        od.basic_price,
        od.description,
        od.note
    FROM
        order_detail od
        INNER JOIN food f ON od.food_id = f.id
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
    o.reason_identity AS ReasonIdentity,
    o.reason AS Reason,
    o.start_time AS StartTime,
    o.end_time AS EndTime,
    o.resolve_at AS ResolveAt,
    o.cancel_at AS CancelAt,
    o.evidence_delivery_fail_json AS EvidenceDeliveryFailJson,
    o.delivery_success_image_url AS DeliverySuccessImageUrl,
    o.reject_at AS RejectAt,
    o.lastest_delivery_fail_at AS LatestDeliveryFailAt,
    d.id AS DormitoryId,
    d.name AS DormitoryName,
    (
        SELECT
            CASE
                WHEN p.status = 2 THEN 1 -- Success
                ELSE 0
            END
        FROM
            payment p
        WHERE
            p.order_id = o.id
            AND p.`type` = 1 -- payment
    ) AS IsCustomerPaid,
    -- Customer
    a.id AS CustomerSection,
    a.id AS Id,
    o.full_name AS FullName,
    o.phone_number AS PhoneNumber,
    a.avatar_url AS AvartarUrl,
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
    od.f_name AS Name,
    od.f_image_url AS ImageUrl,
    od.f_desc AS Description,
    od.quantity AS Quantity,
    od.total_price AS TotalPrice,
    od.basic_price AS BasicPrice,
    od.description AS OrderDescription,
    od.note AS Note
FROM
    OrderInfor o
    INNER JOIN building b ON o.building_id = b.id
    INNER JOIN dormitory d ON b.dormitory_id = d.id
    INNER JOIN OrderDetailWithFood od ON o.id = od.order_id
    INNER JOIN location l ON o.customer_location_id = l.id
    LEFT JOIN promotion p ON o.promotion_id = p.id
    INNER JOIN account a ON a.id = o.customer_id
    LEFT JOIN delivery_package dp ON o.delivery_package_id = dp.id
    LEFT JOIN account accShip ON dp.shop_delivery_staff_id = accShip.id
    LEFT JOIN account accShop ON dp.shop_id = accShop.id;