/*
 CreatedBy: ThongNV
 Date: 17/11/2024
 
 */
-- SET @DateOfYear:='2024-11-14';
-- SET @PaymentOnlineList:='1';
-- SET @DeliveryFailByCustomerReportedByCustomer:='DeliveryFailByCustomerReportedByCustomer';
-- SET @DeliveryFailByShopReportedByCustomer:='DeliveryFailByShopReportedByCustomer';
-- SET @DeliveredReportedByCustomer:='DeliveredReportedByCustomer';
-- SET @DeliveryFailByCustomer:='DeliveryFailByCustomer';
-- SET @CustomerCancel:='CustomerCancel';
-- SET @ShopCancel:='ShopCancel';
-- SET @DeliveryFailByShop:='DeliveryFailByShop';
WITH Revenue AS (
    SELECT
        o.id,
        o.intended_receive_date,
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
        END AS amount
    FROM
        `order` o
        LEFT JOIN payment p ON o.id = p.order_id
        AND p.type = 1 -- Payment
        AND p.payment_methods IN @PaymentOnlineList
    WHERE
        YEAR(o.intended_receive_date) = YEAR(@DateOfYear)
),
PreviousRevenue AS (
    SELECT
        o.id,
        o.intended_receive_date,
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
        END AS amount
    FROM
        `order` o
        LEFT JOIN payment p ON o.id = p.order_id
        AND p.type = 1 -- Payment
        AND p.payment_methods IN @PaymentOnlineList
    WHERE
        YEAR(o.intended_receive_date) = YEAR(DATE_SUB(@DateOfYear, INTERVAL 1 YEAR))
),
TwelveMonthRevenue AS (
    SELECT
        (
            SELECT
                SUM(amount)
            FROM
                Revenue
            WHERE
                MONTH(intended_receive_date) = 1
        ) AS this_year,
        (
            SELECT
                SUM(amount)
            FROM
                PreviousRevenue
            WHERE
                MONTH(intended_receive_date) = 1
        ) AS last_year
    UNION
    ALL
    SELECT
        (
            SELECT
                SUM(amount)
            FROM
                Revenue
            WHERE
                MONTH(intended_receive_date) = 2
        ) AS this_year,
        (
            SELECT
                SUM(amount)
            FROM
                PreviousRevenue
            WHERE
                MONTH(intended_receive_date) = 2
        ) AS last_year
    UNION
    ALL
    SELECT
        (
            SELECT
                SUM(amount)
            FROM
                Revenue
            WHERE
                MONTH(intended_receive_date) = 3
        ) AS this_year,
        (
            SELECT
                SUM(amount)
            FROM
                PreviousRevenue
            WHERE
                MONTH(intended_receive_date) = 3
        ) AS last_year
    UNION
    ALL
    SELECT
        (
            SELECT
                SUM(amount)
            FROM
                Revenue
            WHERE
                MONTH(intended_receive_date) = 4
        ) AS this_year,
        (
            SELECT
                SUM(amount)
            FROM
                PreviousRevenue
            WHERE
                MONTH(intended_receive_date) = 4
        ) AS last_year
    UNION
    ALL
    SELECT
        (
            SELECT
                SUM(amount)
            FROM
                Revenue
            WHERE
                MONTH(intended_receive_date) = 5
        ) AS this_year,
        (
            SELECT
                SUM(amount)
            FROM
                PreviousRevenue
            WHERE
                MONTH(intended_receive_date) = 5
        ) AS last_year
    UNION
    ALL
    SELECT
        (
            SELECT
                SUM(amount)
            FROM
                Revenue
            WHERE
                MONTH(intended_receive_date) = 6
        ) AS this_year,
        (
            SELECT
                SUM(amount)
            FROM
                PreviousRevenue
            WHERE
                MONTH(intended_receive_date) = 6
        ) AS last_year
    UNION
    ALL
    SELECT
        (
            SELECT
                SUM(amount)
            FROM
                Revenue
            WHERE
                MONTH(intended_receive_date) = 7
        ) AS this_year,
        (
            SELECT
                SUM(amount)
            FROM
                PreviousRevenue
            WHERE
                MONTH(intended_receive_date) = 7
        ) AS last_year
    UNION
    ALL
    SELECT
        (
            SELECT
                SUM(amount)
            FROM
                Revenue
            WHERE
                MONTH(intended_receive_date) = 8
        ) AS this_year,
        (
            SELECT
                SUM(amount)
            FROM
                PreviousRevenue
            WHERE
                MONTH(intended_receive_date) = 8
        ) AS last_year
    UNION
    ALL
    SELECT
        (
            SELECT
                SUM(amount)
            FROM
                Revenue
            WHERE
                MONTH(intended_receive_date) = 9
        ) AS this_year,
        (
            SELECT
                SUM(amount)
            FROM
                PreviousRevenue
            WHERE
                MONTH(intended_receive_date) = 9
        ) AS last_year
    UNION
    ALL
    SELECT
        (
            SELECT
                SUM(amount)
            FROM
                Revenue
            WHERE
                MONTH(intended_receive_date) = 10
        ) AS this_year,
        (
            SELECT
                SUM(amount)
            FROM
                PreviousRevenue
            WHERE
                MONTH(intended_receive_date) = 10
        ) AS last_year
    UNION
    ALL
    SELECT
        (
            SELECT
                SUM(amount)
            FROM
                Revenue
            WHERE
                MONTH(intended_receive_date) = 11
        ) AS this_year,
        (
            SELECT
                SUM(amount)
            FROM
                PreviousRevenue
            WHERE
                MONTH(intended_receive_date) = 11
        ) AS last_year
    UNION
    ALL
    SELECT
        (
            SELECT
                SUM(amount)
            FROM
                Revenue
            WHERE
                MONTH(intended_receive_date) = 12
        ) AS this_year,
        (
            SELECT
                SUM(amount)
            FROM
                PreviousRevenue
            WHERE
                MONTH(intended_receive_date) = 12
        ) AS last_year
)
SELECT
    YEAR(@DateOfYear) AS ThisYear,
    YEAR(DATE_SUB(@DateOfYear, INTERVAL 1 YEAR)) AS LastYear,
    (
        SELECT
            SUM(amount)
        FROM
            Revenue
    ) AS ThisYearStr,
    (
        SELECT
            SUM(amount)
        FROM
            PreviousRevenue
    ) AS LastYearStr,
    (
        SELECT
            JSON_ARRAYAGG(
                JSON_OBJECT(
                    'ThisYearStr',
                    this_year,
                    'LastYearStr',
                    last_year
                )
            )
        FROM
            TwelveMonthRevenue
    ) AS TwelveMonthRevenueStr;