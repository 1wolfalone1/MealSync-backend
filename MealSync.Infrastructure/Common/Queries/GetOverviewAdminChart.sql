/*
 CreatedBy: ThongNV
 Date: 17/11/2024
 
 */
-- SET @DateFrom:='2024-11-14';
-- SET @DateTo:='2024-11-14';
-- SET @PaymentOnlineList:='1';
-- SET @DeliveryFailByCustomerReportedByCustomer:='DeliveryFailByCustomerReportedByCustomer';
-- SET @DeliveryFailByShopReportedByCustomer:='DeliveryFailByShopReportedByCustomer';
-- SET @DeliveredReportedByCustomer:='DeliveredReportedByCustomer';
-- SET @DeliveryFailByCustomer:='DeliveryFailByCustomer';
-- SET @CustomerCancel:='CustomerCancel';
-- SET @ShopCancel:='ShopCancel';
-- SET @DeliveryFailByShop:='DeliveryFailByShop';
WITH OrderFilterByDate AS (
    SELECT
        id,
        status,
        reason_identity,
        total_price,
        total_promotion,
        charge_fee,
        is_refund,
        is_report
    FROM
        `order` o
    WHERE
        DATE_FORMAT(o.intended_receive_date, '%Y-%m-%d') BETWEEN @DateFrom
        AND @DateTo
),
TotalUser AS (
    SELECT
        count(a.id) AS total_users
    FROM
        account a
    WHERE
        a.role_id IN (1, 2) -- Cus, shop
        AND status = 2 -- Verify
        AND DATE_FORMAT(a.created_date, '%Y-%m-%d') BETWEEN @DateFrom
        AND @DateTo
),
TotalOrder AS (
    SELECT
        COUNT(o.id) AS total_order
    FROM
        OrderFilterByDate o
        LEFT JOIN payment p ON o.id = p.order_id
        AND p.type = 1 -- Payment
        AND p.payment_methods IN @PaymentOnlineList
    WHERE
        o.status = 7
        OR o.status = 9 -- Completed
        AND o.reason_identity
        OR o.status = 9 -- Completed
        AND o.reason_identity = @DeliveryFailByCustomer
        AND p.type = 1 -- Payment
        AND p.payment_methods IN @PaymentOnlineList
        OR o.status = 12 -- Resolved
        AND o.is_report = TRUE
        AND o.reason_identity = @DeliveredReportedByCustomer
        OR o.status = 12 -- Resolved
        AND o.is_report = TRUE
        AND (
            o.reason_identity = @DeliveryFailByCustomerReportedByCustomer
            OR o.reason_identity = @DeliveryFailByShopReportedByCustomer
        )
        AND o.is_refund = FALSE
        AND p.payment_methods IN @PaymentOnlineList
        AND p.type = 1 -- Payment
        AND p.status = 2 -- PaidSuccess
),
TotalTrading AS (
    SELECT
        SUM(
            CASE
                WHEN o.status = 7 THEN o.total_price - o.total_promotion
                WHEN o.status = 9 -- Completed
                AND o.reason_identity IS NULL THEN o.total_price - o.total_promotion
                WHEN o.status = 9 -- Completed
                AND o.reason_identity = @DeliveryFailByCustomer
                AND p.type = 1 -- Payment
                AND p.payment_methods IN @PaymentOnlineList THEN o.total_price - o.total_promotion
                WHEN o.status = 12 -- Resolved
                AND o.is_report = TRUE
                AND o.reason_identity = @DeliveredReportedByCustomer THEN o.total_price - o.total_promotion
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
                THEN o.total_price - o.total_promotion
                ELSE 0
            END
        ) AS total_trading
    FROM
        OrderFilterByDate o
        LEFT JOIN payment p ON o.id = p.order_id
        AND p.type = 1 -- Payment
        AND p.payment_methods IN @PaymentOnlineList
),
TotalChargeFee AS (
    SELECT
        SUM(
            CASE
                WHEN o.status = 7 THEN o.charge_fee
                WHEN o.status = 9 -- Completed
                AND o.reason_identity IS NULL THEN o.charge_fee
                WHEN o.status = 9 -- Completed
                AND o.reason_identity = @DeliveryFailByCustomer
                AND p.type = 1 -- Payment
                AND p.payment_methods IN @PaymentOnlineList THEN o.charge_fee
                WHEN o.status = 12 -- Resolved
                AND o.is_report = TRUE
                AND o.reason_identity = @DeliveredReportedByCustomer THEN o.charge_fee
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
                THEN o.charge_fee
                ELSE 0
            END
        ) AS total_charge_fee
    FROM
        OrderFilterByDate o
        LEFT JOIN payment p ON o.id = p.order_id
        AND p.type = 1 -- Payment
        AND p.payment_methods IN @PaymentOnlineList
)
SELECT
    (
        SELECT
            total_users
        FROM
            TotalUser
    ) AS TotalUser,
    (
        SELECT
            total_order
        FROM
            TotalOrder
    ) AS TotalOrder,
    (
        SELECT
            total_trading
        FROM
            TotalTrading
    ) AS TotalTradingAmount,
    (
        SELECT
            total_charge_fee
        FROM
            TotalChargeFee
    ) AS TotalChargeFee;