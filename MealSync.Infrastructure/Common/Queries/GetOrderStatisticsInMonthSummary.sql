SELECT
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
            WHEN o.status = 12 -- Resolved
            AND o.reason_identity = @DeliveredReportedByCustomer
            AND EXISTS (
                SELECT
                    1
                FROM
                    report r
                WHERE
                    r.order_id = o.id
                    AND r.customer_id IS NOT NULL
                    AND r.status = 3 -- Rejected
            ) THEN 1
            ELSE 0
        END
    ) AS TotalDeliveredResolvedRejectReport,
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
                (
                    o.reason_identity = @DeliveredReportedByCustomer
                    AND EXISTS (
                        SELECT
                            1
                        FROM
                            report r
                        WHERE
                            r.order_id = o.id
                            AND r.customer_id IS NOT NULL
                            AND r.status = 2 -- Approved
                    )
                )
                OR o.reason_identity = @DeliveryFailByCustomerReportedByCustomer
                OR o.reason_identity = @DeliveryFailByShopReportedByCustomer
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
                (
                    o.reason_identity = @DeliveredReportedByCustomer
                    AND EXISTS (
                        SELECT
                            1
                        FROM
                            report r
                        WHERE
                            r.order_id = o.id
                            AND r.customer_id IS NOT NULL
                            AND r.status = 2 -- Approved
                    )
                )
                OR o.reason_identity = @DeliveryFailByCustomerReportedByCustomer
                OR o.reason_identity = @DeliveryFailByShopReportedByCustomer
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
                WHEN o.status = 12 -- Resolved
                AND o.is_report = TRUE
                AND (
                    o.reason_identity = @DeliveryFailByCustomerReportedByCustomer
                    OR o.reason_identity = @DeliveryFailByShopReportedByCustomer
                )
                AND o.is_refund = FALSE
                AND p.payment_methods IN @PaymentOnlineList
                AND p.type = 1 -- Payment
                AND p.status = 2 -- PaidSuccess
                THEN o.total_price - o.total_promotion - o.charge_fee
            END
        ),
        0
    ) AS Revenue
FROM
    `order` o
    LEFT JOIN payment p ON o.id = p.order_id
    AND p.type = 1 -- Payment
    AND p.payment_methods IN @PaymentOnlineList
WHERE
    o.shop_id = @ShopId
    AND o.intended_receive_date BETWEEN @StartDate
    AND @EndDate