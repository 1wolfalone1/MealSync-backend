/*
 CreatedBy: ThongNV
 Date: 28/11/2024
 
 */
-- SET @SearchValue:=null;
-- SET @StatusMode:=0;
-- SET @DateFrom:='2024-10-10';
-- SET @DateTo:='2024-12-12';
-- SET @ShopId:=2;
-- SET @CurrentDate:='2024-11-25';
-- SET @PageIndex:=1;
-- SET @PageSize:=15;
WITH ReviewCustomerOfShopOnly AS (
    SELECT
        r.id,
        r.customer_id,
        order_id,
        rating,
        comment,
        image_url,
        r.created_date,
        r.updated_date,
        entity,
        r.shop_id,
        (
            SELECT
                DATE_FORMAT(
                    DATE_ADD(o.created_date, INTERVAL 24 HOUR),
                    '%Y-%m-%d %H:%i:%s'
                ) >= DATE_FORMAT(@CurrentDate, '%Y-%m-%d %H:%i:%s')
                AND order_id IN (
                    SELECT
                        order_id
                    FROM
                        review
                    GROUP BY
                        (order_id)
                    HAVING
                        COUNT(*) = 1
                )
        ) AS IsAllowShopReply,
        a2.avatar_url,
        a2.full_name,
        ROW_NUMBER() OVER (
            ORDER BY
                r.id
        ) AS RowNum,
        COUNT(r.id) OVER () AS TotalCount
    FROM
        review r
        INNER JOIN `order` o ON r.order_id = o.id
        INNER JOIN account a2 ON r.customer_id = a2.id
    WHERE
        r.entity = 1 -- customer ONLY
        AND (
            @SearchValue IS NULL
            OR r.id LIKE CONCAT('%', @SearchValue, '%')
            OR r.order_id LIKE CONCAT('%', @SearchValue, '%')
            OR a2.full_name LIKE CONCAT('%', @SearchValue, '%')
        )
        AND o.shop_id = @ShopId
        AND (
            @DateFrom IS NULL
            AND @DateTo IS NULL
            OR DATE_FORMAT(o.created_date, '%Y-%m-%d') BETWEEN DATE_FORMAT(@DateFrom, '%Y-%m-%d')
            AND DATE_FORMAT(@DateTo, '%Y-%m-%d')
        )
        AND (
            @StatusMode = 0 -- GET all
            OR @StatusMode = 1 -- GET review NOT reply
            AND DATE_FORMAT(
                DATE_ADD(o.created_date, INTERVAL 24 HOUR),
                '%Y-%m-%d %H:%i:%s'
            ) >= DATE_FORMAT(@CurrentDate, '%Y-%m-%d %H:%i:%s')
            AND order_id IN (
                SELECT
                    order_id
                FROM
                    review
                GROUP BY
                    (order_id)
                HAVING
                    COUNT(*) = 1
            )
            OR @StatusMode = 2 -- GET review replied
            AND order_id IN (
                SELECT
                    order_id
                FROM
                    review
                GROUP BY
                    (order_id)
                HAVING
                    COUNT(*) = 2
            )
            OR @StatusMode = 3 -- OVER TIME TO REPLY
            AND DATE_FORMAT(
                DATE_ADD(o.created_date, INTERVAL 24 HOUR),
                '%Y-%m-%d %H:%i:%s'
            ) < DATE_FORMAT(@CurrentDate, '%Y-%m-%d %H:%i:%s')
            AND order_id IN (
                SELECT
                    order_id
                FROM
                    review
                GROUP BY
                    (order_id)
                HAVING
                    COUNT(*) = 1
            )
        )
)
SELECT
    r.id AS Id,
    order_id AS OrderId,
    rating AS Rating,
    comment AS Comment,
    image_url AS ImageUrl,
    created_date AS CreatedDate,
    updated_date AS UpdatedDate,
    entity AS Entity,
    shop_id AS ShopId,
    r.IsAllowShopReply AS IsAllowShopReply,
    r.TotalCount AS TotalCount,
    r.customer_id AS CustomerSection,
    r.customer_id AS Id,
    r.avatar_url AS AvartarUrl,
    r.full_name AS FullName
FROM
    ReviewCustomerOfShopOnly r
WHERE
    RowNum BETWEEN (@PageIndex - 1) * @PageSize + 1
    AND @PageIndex * @PageSize
ORDER BY
    created_date DESC;