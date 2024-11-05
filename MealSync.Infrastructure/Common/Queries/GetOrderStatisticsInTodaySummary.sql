SELECT
    SUM(
        CASE
            WHEN o.status = 1 -- Pending
            THEN 1
            ELSE 0
        END
    ) AS TotalOrderPending,
    SUM(
        CASE
            WHEN o.status = 3 -- Confirmed
            THEN 1
            ELSE 0
        END
    ) AS TotalOrderConfirmed,
    SUM(
        CASE
            WHEN o.status = 5 -- Preparing
            THEN 1
            ELSE 0
        END
    ) AS TotalOrderPreparing,
    SUM(
        CASE
            WHEN o.status = 6 -- Delivering
            THEN 1
            ELSE 0
        END
    ) AS TotalOrderDelivering,
    SUM(
        CASE
            WHEN o.status = 8 -- FailDelivery
            THEN 1
            ELSE 0
        END
    ) AS TotalOrderFailDelivery,
    SUM(
        CASE
            WHEN o.status = 7 -- Delivered
            OR (
                o.status = 9 -- Completed
                AND o.reason_identity IS NULL
            ) THEN 1
            ELSE 0
        END
    ) AS TotalOrderCompleted
FROM
    `order` o
WHERE
    o.shop_id = @ShopId
    AND o.intended_receive_date = @Today