-- SET @OrderIds:=
WITH OrderFilter AS (
    SELECT
        o.id,
        o.building_id,
        o.customer_id,
        o.shop_id,
        o.customer_location_id,
        o.status
    FROM
        `order` o
    WHERE
        o.id IN @OrderIds
        AND o.status IN (5, 6)
),
OrderDetailGroup AS (
    SELECT
        o.id AS order_id,
        SUM(od2.quantity * fpu.weight) AS weight
    FROM
        OrderFilter o
        INNER JOIN order_detail od2 ON o.id = od2.order_id
        INNER JOIN food f ON od2.food_id = f.id
        INNER JOIN food_packing_unit fpu ON f.food_packing_unit_id = fpu.id
    GROUP BY
        o.id
)
SELECT
    o.id AS Id,
    o.status AS Status,
    o.customer_id AS CustomerId,
    o.shop_id AS ShopId,
    o.building_id AS BuildingId,
    b.dormitory_id AS DormitoryId,
    od.weight AS Weight,
    l.address AS CustomerAddress,
    l.latitude AS CustomerLatitude,
    l.longitude AS CustomerLongitude
FROM
    OrderFilter o
    INNER JOIN OrderDetailGroup od ON o.id = od.order_id
    INNER JOIN building b ON o.building_id = b.id
    INNER JOIN location l ON o.customer_location_id = l.id;