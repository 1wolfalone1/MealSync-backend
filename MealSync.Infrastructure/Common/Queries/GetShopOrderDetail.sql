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
        delivery_success_image_url,
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
        od.description
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
    o.completed_at AS CompletedAt,
    o.intended_receive_date AS IntendedReceiveDate,
    o.start_time AS StartTime,
    o.end_time AS EndTime,
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
    -- Delivery Package
    dp.id AS DeliveryPackageSection,
    dp.staff_delivery_id AS Id,
    a2.full_name AS FullName,
    a2.phone_number AS PhoneNumber,
    a2.email AS Email,
    a2.avatar_url AS AvatarUrl,
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
    od.description AS OrderDescription
FROM
    OrderInfor o
    INNER JOIN OrderDetailWithFood od ON o.id = od.order_id
    INNER JOIN location l ON o.customer_location_id = l.id
    LEFT JOIN promotion p ON o.promotion_id = p.id
    INNER JOIN account a ON a.id = o.customer_id
    LEFT JOIN delivery_package dp ON o.delivery_package_id = dp.id
    LEFT JOIN account a2 ON dp.staff_delivery_id = a2.id;