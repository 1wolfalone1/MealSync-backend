WITH FilteredOrders AS (SELECT o.id                    AS Id,
                               o.status                AS Status,
                               o.shipping_fee          AS ShippingFee,
                               o.total_price           AS TotalPrice,
                               o.total_promotion       AS TotalPromotion,
                               o.order_date            AS OrderDate,
                               o.intended_receive_date AS IntendedReceiveDate,
                               o.start_time            AS StartTime,
                               o.end_time              AS EndTime,
                               s.id                    AS ShopId,
                               s.name                  AS ShopName,
                               s.logo_url              AS ShopLogoUrl,
                               COUNT(r.id)             AS ReviewCount,
                               o.created_date          AS CreatedDate
                        FROM `order` o
                                 INNER JOIN `shop` s ON o.shop_id = s.id
                                 LEFT JOIN `review` r ON o.id = r.order_id
                        WHERE o.customer_id = @CustomerId
                          AND o.status IN @FilterStatusList
                        GROUP BY o.id, o.status, o.shipping_fee, o.total_price,
                                 o.total_promotion, o.order_date, o.intended_receive_date,
                                 o.start_time, o.end_time, s.id, s.name, s.logo_url, o.created_date)
SELECT COUNT(*) OVER() AS TotalCount, Id,
       Status,
       ShippingFee,
       TotalPrice,
       TotalPromotion,
       OrderDate,
       IntendedReceiveDate,
       StartTime,
       EndTime,
       ShopName,
       ShopLogoUrl
FROM FilteredOrders
WHERE (@ReviewMode = 0 OR (
                              Status IN @ReviewStatusList
                                  AND ReviewCount = 0
                                  AND @Now BETWEEN
                                  DATE_ADD(
                                          CAST(IntendedReceiveDate AS DATETIME),
                                          INTERVAL FLOOR(EndTime / 100) HOUR
            ) + INTERVAL (EndTime % 100) MINUTE
    AND
                          DATE_ADD(
                                  CAST(IntendedReceiveDate AS DATETIME),
                                  INTERVAL FLOOR(EndTime / 100) HOUR
            ) + INTERVAL (EndTime % 100) MINUTE + INTERVAL 24 HOUR
    ))
ORDER BY CreatedDate DESC
    LIMIT @PageSize
OFFSET @Offset;