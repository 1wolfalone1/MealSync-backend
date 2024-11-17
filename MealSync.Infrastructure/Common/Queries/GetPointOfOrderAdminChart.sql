/*
 CreatedBy: ThongNV
 Date: 17/11/2024
 
 */
-- SET @DateFrom:='2024-11-14';
-- SET @DateTo:='2024-11-14';
-- SET @PaymentOnlineList:='1';
-- SET @DeliveryFailReportedByCustomer:='DeliveryFailReportedByCustomer';
-- SET @DeliveredReportedByCustomer:='DeliveredReportedByCustomer';
-- SET @DeliveryFailByCustomer:='DeliveryFailByCustomer';
-- SET @CustomerCancel:='CustomerCancel';
-- SET @ShopCancel:='ShopCancel';
-- SET @DeliveryFailByShop
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
        DATE_FORMAT(o.intended_receive_date, '%Y-%m-%d') BETWEEN DATE_FORMAT(@DateFrom, '%Y-%m-%d')
        AND DATE_FORMAT(@DateTo, '%Y-%m-%d')
)
SELECT
    (
        SELECT
            COUNT(*)
        FROM
            OrderFilterByDate -- total
    ) AS TotalOfOrder,
    (
        SELECT
            COUNT(*)
        FROM
            OrderFilterByDate
        WHERE
            status IN (1, 13) -- Pending, pending payment
    ) AS Pending,
    (
        SELECT
            COUNT(*)
        FROM
            OrderFilterByDate
        WHERE
            status IN (2) -- Rejected
    ) AS Rejected,
    (
        SELECT
            COUNT(*)
        FROM
            OrderFilterByDate
        WHERE
            status IN (3, 5, 6) -- Confirmed, Preparing, Delivering
    ) AS ProcessingOrder,
    (
        SELECT
            COUNT(*)
        FROM
            OrderFilterByDate
        WHERE
            status IN (7) -- Delivered
    ) AS Delivered,
    (
        SELECT
            COUNT(*)
        FROM
            OrderFilterByDate
        WHERE
            status IN (8)
            OR reason_identity = @DeliveryFailByCustomer
            OR reason_identity = @DeliveryFailByShop -- Delivery failed
    ) AS FailDelivered,
    (
        SELECT
            COUNT(*)
        FROM
            OrderFilterByDate
        WHERE
            status IN (4)
            OR reason_identity = @CustomerCancel
            OR reason_identity = @ShopCancel -- Canceled
    ) AS Canceled,
    (
        SELECT
            COUNT(*)
        FROM
            OrderFilterByDate
        WHERE
            status IN (9)
            AND reason_identity IS NULL -- Delivered
    ) AS Successful,
    (
        SELECT
            COUNT(*)
        FROM
            OrderFilterByDate
        WHERE
            status IN (10, 11) -- Issued Reported, UNDER review
    ) AS IssueProcessing,
    (
        SELECT
            COUNT(*)
        FROM
            OrderFilterByDate
        WHERE
            status IN (12)
            AND reason_identity IS NULL -- Resolved
    ) AS Resolved,
    -- (
    -- SELECT SUM(trading_amout) FROM OrderFilterByDate o WHERE 
    -- OR status = 7 AND is_report = 0 -- Delivered
    -- OR status = 9 AND reason_identity IS NULL
    -- OR status = 9 AND reason_identity = 'DeliveryFailByCustomer' AND EXISTS (
    -- SELECT id FROM payment p WHERE order_id = o.id AND payment_methods != 2 /* diff COD */ AND p.status = 2 /* success*/ AND p.type = 1 /* payment not refund*/
    -- )-- Completed but delivery fail but customer pre paid online success
    -- OR status = 12 AND 
    -- ) AS TotalTradingAmount,
    (
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
                    AND o.reason_identity = @DeliveryFailReportedByCustomer
                    AND o.is_refund = FALSE
                    AND p.payment_methods IN @PaymentOnlineList
                    AND p.type = 1 -- Payment
                    AND p.status = 2 -- PaidSuccess
                    THEN o.total_price - o.total_promotion
                    ELSE 0
                END
            ) AS total
        FROM
            OrderFilterByDate o
            LEFT JOIN payment p ON o.id = p.order_id
            AND p.type = 1 -- Payment
            AND p.payment_methods IN @PaymentOnlineList
    ) AS TotalTradingAmount,
    (
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
                    AND o.reason_identity = @DeliveryFailReportedByCustomer
                    AND o.is_refund = FALSE
                    AND p.payment_methods IN @PaymentOnlineList
                    AND p.type = 1 -- Payment
                    AND p.status = 2 -- PaidSuccess
                    THEN o.charge_fee
                    ELSE 0
                END
            ) AS total
        FROM
            OrderFilterByDate o
            LEFT JOIN payment p ON o.id = p.order_id
            AND p.type = 1 -- Payment
            AND p.payment_methods IN @PaymentOnlineList
    ) AS TotalChargeFee,
    @DateTo AS LabelDate