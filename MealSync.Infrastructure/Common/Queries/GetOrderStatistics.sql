SELECT
    SUM(
        CASE
            WHEN o.status NOT IN (4, 2, 9, 12) -- Rejected, Cancelled, Completed, Resolved
            THEN 1
            ELSE 0
        END
    ) AS TotalOrderInProcess,
    SUM(
        CASE
            WHEN o.status IN (4, 2, 9, 12) -- Rejected, Cancelled, Completed, Resolved
            THEN 1
            ELSE 0
        END
    ) AS TotalOrderDone,
    SUM(
        CASE
            WHEN o.status = 4 -- Cancelled
            AND o.reason_identity = @CustomerCancel THEN 1
            ELSE 0
        END
    ) AS TotalCancelByCustomer,
    SUM(
        CASE
            WHEN o.status = 4 -- Cancelled
            AND o.reason_identity = @ShopCancel THEN 1
            ELSE 0
        END
    ) AS TotalCancelByShop,
    SUM(
        CASE
            WHEN o.status = 2 -- Rejected
            THEN 1
            ELSE 0
        END
    ) AS TotalReject,
    SUM(
        CASE
            WHEN o.status = 9 -- Completed
            AND o.reason_identity IS NULL THEN 1
            ELSE 0
        END
    ) AS TotalDeliveredCompleted,
    SUM(
        CASE
            WHEN o.status = 9 -- Completed
            AND o.reason_identity = @DeliveryFailByCustomer THEN 1
            ELSE 0
        END
    ) AS TotalFailDeliveredByCustomerCompleted,
    SUM(
        CASE
            WHEN o.status = 9 -- Completed
            AND o.reason_identity = @DeliveryFailByShop THEN 1
            ELSE 0
        END
    ) AS TotalFailDeliveredByShopCompleted,
    SUM(
        CASE
            WHEN o.status = 12 -- Resolved
            AND o.is_report = TRUE
            AND (
                o.reason_identity = @DeliveredReportedByCustomer
                OR o.reason_identity = @DeliveryFailReportedByCustomer
            )
            AND o.is_refund = TRUE THEN 1
            ELSE 0
        END
    ) AS TotalReportResolvedHaveRefund,
    SUM(
        CASE
            WHEN o.status = 12 -- Resolved
            AND o.is_report = TRUE
            AND (
                o.reason_identity = @DeliveredReportedByCustomer
                OR o.reason_identity = @DeliveryFailReportedByCustomer
            )
            AND o.is_refund = FALSE THEN 1
            ELSE 0
        END
    ) AS TotalReportResolvedNotHaveRefund,
    COALESCE(
        SUM(
            CASE
                WHEN o.status = 9 -- Completed
                AND o.reason_identity IS NULL THEN o.total_price - o.total_promotion - o.charge_fee
                WHEN o.status = 9 -- Completed
                AND o.reason_identity = @DeliveryFailByCustomer
                AND p.type = 1 -- Payment
                AND p.payment_methods IN @PaymentOnlineList THEN o.total_price - o.total_promotion - o.charge_fee
                WHEN o.status = 12 -- Resolved
                AND o.is_report = TRUE
                AND o.reason_identity = @DeliveredReportedByCustomer THEN o.total_price - o.total_promotion - o.charge_fee
            END
        ),
        0
    ) AS Revenue,
    ROUND(
        (
            SUM(
                CASE
                    WHEN o.status = 9 -- Completed
                    AND o.reason_identity IS NULL THEN 1
                    ELSE 0
                END
            ) / NULLIF(
                COUNT(*) - SUM(
                    CASE
                        WHEN o.status NOT IN (4, 2, 9, 12) -- Rejected, Cancelled, Completed, Resolved
                        THEN 1
                        ELSE 0
                    END
                ),
                0
            )
        ) * 100,
        2
    ) AS SuccessfulOrderPercentage
FROM
    `order` o
    LEFT JOIN payment p ON o.id = p.order_id
    AND p.type = 1 -- Payment
    AND p.payment_methods IN @PaymentOnlineList
WHERE
    o.shop_id = @ShopId
    AND o.intended_receive_date BETWEEN @StartDate
    AND @EndDate;