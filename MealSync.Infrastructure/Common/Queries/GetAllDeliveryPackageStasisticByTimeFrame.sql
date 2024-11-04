/*
 CreatedBy: ThongNV
 Date: 01/11/2024
 
 @IntendedReceiveDate Date
 @ShopId long
 @ShopDeliveryStaffId long
 @IsShopOwnerShip bool
 @Status int[]
 @StartTime int
 @EndTime int
 */
-- SET @IntendedReceiveDate:='2024-10-31';
-- SET @ShopId:=2;
-- SET @ShopDeliveryStaffId:=6;
-- SET @IsShopOwnerShip:='FALSE';
-- SET @Status:='1, 2';
-- SET @StartTime:=
-- SET @EndTime:=
WITH OrderHaveAssignToDeliveryPackage AS (
    SELECT
        o.id,
        o.delivery_package_id,
        o.status,
        o.shop_id,
        o.building_id,
        b.dormitory_id,
        CASE
            WHEN dp.shop_delivery_staff_id IS NOT NULL THEN dp.shop_delivery_staff_id
            ELSE dp.shop_id
        END AS ship_id,
        (dp.shop_delivery_staff_id IS NULL) AS is_shop_owner_ship
    FROM
        `order` o
        INNER JOIN building b ON o.building_id = b.id
        INNER JOIN delivery_package dp ON o.delivery_package_id = dp.id
    WHERE
        o.delivery_package_id IS NOT NULL
        AND o.intended_receive_date = @IntendedReceiveDate
        AND o.start_time >= @StartTime
        AND o.end_time <= @EndTime
        AND o.shop_id = @ShopId
),
DeliveryPackageInFrame AS (
    SELECT
        dp.id,
        dp.shop_delivery_staff_id,
        dp.shop_id,
        dp.delivery_date,
        dp.start_time,
        dp.end_time,
        CASE
            WHEN dp.shop_delivery_staff_id IS NOT NULL THEN dp.shop_delivery_staff_id
            ELSE dp.shop_id
        END AS ship_id,
        (dp.shop_delivery_staff_id IS NULL) AS is_shop_owner_ship,
        dp.created_date
    FROM
        delivery_package dp
    WHERE
        dp.delivery_date = @IntendedReceiveDate
        AND (
            @IsShopOwnerShip = 1
            AND dp.shop_id = @ShopId
            OR dp.shop_delivery_staff_id = @ShopDeliveryStaffId
        )
        AND dp.status IN @Status
      AND dp.start_time >= @StartTime
      AND dp.end_time <= @EndTime
),
-- preparing5 delevering6 deliverd7 deliveryfailed8
DormitoryStasticAndInfor AS (
    SELECT
        o.ship_id,
        o.delivery_package_id,
        d.id,
        d.name,
        o.is_shop_owner_ship,
        (
            SELECT
                COUNT(*)
            FROM
                OrderHaveAssignToDeliveryPackage oTemp
            WHERE
                o.delivery_package_id = oTemp.delivery_package_id
                AND oTemp.dormitory_id = d.id
                AND oTemp.status IN (5, 6, 7, 8)
            GROUP BY
                oTemp.ship_id
        ) AS total,
        (
            SELECT
                COUNT(*)
            FROM
                OrderHaveAssignToDeliveryPackage oTemp
            WHERE
                o.delivery_package_id = oTemp.delivery_package_id
                AND oTemp.dormitory_id = d.id
                AND oTemp.status = 5
            GROUP BY
                oTemp.ship_id
        ) AS waiting,
        -- Prepare
        (
            SELECT
                COUNT(*)
            FROM
                OrderHaveAssignToDeliveryPackage oTemp
            WHERE
                o.delivery_package_id = oTemp.delivery_package_id
                AND oTemp.dormitory_id = d.id
                AND oTemp.status = 6
            GROUP BY
                oTemp.ship_id
        ) AS delivering,
        -- Delivering
        (
            SELECT
                COUNT(*)
            FROM
                OrderHaveAssignToDeliveryPackage oTemp
            WHERE
                o.delivery_package_id = oTemp.delivery_package_id
                AND oTemp.dormitory_id = d.id
                AND oTemp.status = 7
            GROUP BY
                oTemp.ship_id
        ) AS successful,
        -- Successful
        (
            SELECT
                COUNT(*)
            FROM
                OrderHaveAssignToDeliveryPackage oTemp
            WHERE
                o.delivery_package_id = oTemp.delivery_package_id
                AND oTemp.dormitory_id = d.id
                AND oTemp.status = 8
            GROUP BY
                oTemp.ship_id
        ) AS failed -- Delivery failed
    FROM
        dormitory d
        INNER JOIN OrderHaveAssignToDeliveryPackage o ON d.id = o.dormitory_id
    GROUP BY
        o.ship_id,
        o.delivery_package_id,
        d.id,
        d.name
)
SELECT
    dp.id AS DeliveryPackageId,
    (
        SELECT
            COUNT(*)
        FROM
            OrderHaveAssignToDeliveryPackage oTemp
        WHERE
            dp.id = oTemp.delivery_package_id
            AND dp.ship_id = oTemp.ship_id
            AND oTemp.status IN (5, 6, 7, 8)
    ) AS Total,
    (
        SELECT
            COUNT(*)
        FROM
            OrderHaveAssignToDeliveryPackage oTemp
        WHERE
            dp.id = oTemp.delivery_package_id
            AND dp.ship_id = oTemp.ship_id
            AND oTemp.status = 5
    ) AS Waiting,
    -- Prepare
    (
        SELECT
            COUNT(*)
        FROM
            OrderHaveAssignToDeliveryPackage oTemp
        WHERE
            dp.id = oTemp.delivery_package_id
            AND dp.ship_id = oTemp.ship_id
            AND oTemp.status = 6
    ) AS Delivering,
    -- Delivering
    (
        SELECT
            COUNT(*)
        FROM
            OrderHaveAssignToDeliveryPackage oTemp
        WHERE
            dp.id = oTemp.delivery_package_id
            AND dp.ship_id = oTemp.ship_id
            AND oTemp.status = 7
    ) AS Successful,
    -- Successful
    (
        SELECT
            COUNT(*)
        FROM
            OrderHaveAssignToDeliveryPackage oTemp
        WHERE
            dp.id = oTemp.delivery_package_id
            AND dp.ship_id = oTemp.ship_id
            AND oTemp.status = 8
    ) AS Failed,
    -- Delivery failed,
    -- Shop Delivery Staff or Shop Owner Information (conditional)
    dp.id AS ShopDeliverySection,
    dp.id AS DeliveryPackageId,
    CASE
        WHEN do.is_shop_owner_ship THEN 0
        ELSE accShipperInDp.id
    END AS Id,
    accShipperInDp.full_name AS FullName,
    accShipperInDp.phone_number AS PhoneNumber,
    accShipperInDp.avatar_url AS AvatarUrl,
    dp.is_shop_owner_ship AS IsShopOwnerShip,
    -- Dormitory stastic for each staff infor
    do.id AS DormitorySection,
    do.id AS Id,
    do.name AS Name,
    do.total AS Total,
    do.waiting AS Waiting,
    do.delivering AS Delivering,
    do.successful AS Successful,
    do.failed AS Failed,
    -- Shop delivery staff each dorm infor
    accShipperInDor.id AS ShopDeliveryInDorSection,
    do.delivery_package_id AS DeliveryPackageId,
    CASE
        WHEN do.is_shop_owner_ship THEN 0
        ELSE accShipperInDor.id
    END AS Id,
    accShipperInDor.full_name AS FullName,
    accShipperInDor.phone_number AS PhoneNumber,
    accShipperInDor.avatar_url AS AvatarUrl,
    do.is_shop_owner_ship AS IsShopOwnerShip
FROM
    DeliveryPackageInFrame dp
    INNER JOIN account accShipperInDp ON dp.ship_id = accShipperInDp.id
    INNER JOIN DormitoryStasticAndInfor do ON dp.id = do.delivery_package_id
    INNER JOIN account accShipperInDor ON do.ship_id = accShipperInDor.id
ORDER BY
    dp.start_time,
    dp.end_time,
    dp.delivery_date,
    dp.created_date;