/*
 CreatedBy: ThongNV
 Date: 16/10/2024
 
 @SearchText string
 @IntendedReceiveDate Date
 @StartTime int
 @EndTime int
 @ShopId long
 */
-- SET @SearchText:=NULL;
-- SET @IntendedReceiveDate:='2024-10-28';
-- SET @StartTime:=2000;
-- SET @EndTime:=2030;
-- SET @ShopId:=2;
WITH OrderHaveAssignToDeliveryPackage AS (
    SELECT
        o.id,
        o.delivery_package_id,
        o.status,
        o.shop_id
    FROM
        `order` o
    WHERE
        o.shop_id = @ShopId
        AND o.delivery_package_id IS NOT NULL
        AND o.intended_receive_date = @IntendedReceiveDate
        AND o.start_time = @StartTime
        AND o.end_time = @EndTime
),
ShopOwnerDeliveryTask AS (
    SELECT
        o.id,
        o.delivery_package_id,
        o.status,
        o.shop_id
    FROM
        OrderHaveAssignToDeliveryPackage o
        INNER JOIN delivery_package dp ON o.delivery_package_id = dp.id
    WHERE
        dp.shop_id IS NOT NULL
),
ShopDeliveryStaffTask AS (
    SELECT
        o.id,
        o.delivery_package_id,
        o.status,
        dp.shop_delivery_staff_id
    FROM
        OrderHaveAssignToDeliveryPackage o
        INNER JOIN delivery_package dp ON o.delivery_package_id = dp.id
    WHERE
        dp.shop_delivery_staff_id IS NOT NULL
),
-- preparing5 delevering6 deliverd7 deliveryfailed8
ShopOwnerDeliveryTaskAndInfor AS (
    SELECT
        0 AS id,
        a.full_name,
        a.avatar_url,
        a.phone_number,
        a.email,
        'true' AS is_shop_owner,
        a.id AS statistic,
        (
            SELECT
                count(*)
            FROM
                ShopOwnerDeliveryTask
            WHERE
                status IN (5, 6, 7, 8, 9, 10, 11, 12)
        ) AS total,
        -- Total
        (
            SELECT
                count(*)
            FROM
                ShopOwnerDeliveryTask
            WHERE
                status = 5
        ) AS waiting,
        -- Preparing
        (
            SELECT
                count(*)
            FROM
                ShopOwnerDeliveryTask
            WHERE
                status = 6
        ) AS delivering,
        -- Delivering
        (
            SELECT
                count(*)
            FROM
                ShopOwnerDeliveryTask
            WHERE
                status IN (7, 9)
        ) AS successful,
        -- Delivered
        (
            SELECT
                count(*)
            FROM
                ShopOwnerDeliveryTask
            WHERE
                status = 8
        ) AS failed -- Delivery Failed
    FROM
        account a
    WHERE
        a.id = @ShopId
),
ShopDeliveryStaffTaskAndInfor AS (
    SELECT
        a.id,
        a.full_name,
        a.avatar_url,
        a.phone_number,
        a.email,
        'false' AS is_shop_owner,
        a.id AS statistic,
        (
            SELECT
                count(*)
            FROM
                ShopDeliveryStaffTask sds
            WHERE
                sds.status IN (5, 6, 7, 8, 9, 10, 11, 12)
                AND shop_delivery_staff_id = a.id
        ) AS total,
        -- Total
        (
            SELECT
                count(*)
            FROM
                ShopDeliveryStaffTask sds
            WHERE
                sds.status = 5
                AND shop_delivery_staff_id = a.id
        ) AS waiting,
        -- Preparing
        (
            SELECT
                count(*)
            FROM
                ShopDeliveryStaffTask sds
            WHERE
                sds.status = 6
                AND shop_delivery_staff_id = a.id
        ) AS delivering,
        -- Delivering
        (
            SELECT
                count(*)
            FROM
                ShopDeliveryStaffTask sds
            WHERE
                sds.status IN (7, 9)
                AND shop_delivery_staff_id = a.id
        ) AS successful,
        -- Delivered
        (
            SELECT
                count(*)
            FROM
                ShopDeliveryStaffTask sds
            WHERE
                sds.status = 8
                AND shop_delivery_staff_id = a.id
        ) AS failed -- Delivery Failed
    FROM
        shop_delivery_staff sds
        INNER JOIN account a ON sds.id = a.id
    WHERE
        sds.shop_id = @ShopId
        AND sds.status = 1
--         AND (
--             @SearchText IS NULL
--             OR a.full_name LIKE CONCAT('%', @SearchText, '%')
--             OR a.phone_number LIKE CONCAT('%', @SearchText, '%')
--             OR a.email LIKE CONCAT('%', @SearchText, '%')
--         )
    ORDER BY
        (waiting + delivering)
)
SELECT
    total AS Total,
    waiting AS Waiting,
    delivering AS Delivering,
    successful AS Successful,
    failed AS Failed,
    id AS StaffInforSection,
    id AS Id,
    full_name AS FullName,
    avatar_url AS AvatarUrl,
    phone_number AS PhoneNumber,
    email AS Email,
    is_shop_owner AS IsShopOwner
FROM
    ShopOwnerDeliveryTaskAndInfor
UNION
ALL
SELECT
    total AS Total,
    waiting AS Waiting,
    delivering AS Delivering,
    successful AS Successful,
    failed AS Failed,
    id AS StaffInforSection,
    id AS Id,
    full_name AS FullName,
    avatar_url AS AvatarUrl,
    phone_number AS PhoneNumber,
    email AS Email,
    is_shop_owner AS IsShopOwner
FROM
    ShopDeliveryStaffTaskAndInfor;