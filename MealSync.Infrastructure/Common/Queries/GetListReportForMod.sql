WITH FilteredReports AS (
    SELECT
        r.id AS Id,
        s.name AS ShopName,
        a.full_name AS CustomerName,
        r.order_id AS OrderId,
        r.title AS Title,
        r.content AS Content,
        r.status AS Status,
        CASE
            WHEN o.status = 11 THEN 1
            ELSE 0
        END AS IsUnderReview,
        r.created_date AS CreatedDate,
        CASE
            WHEN r.status NOT IN (2, 3) -- Approved, Rejected
            AND (
                @Now > DATE_ADD(
                    CAST(o.intended_receive_date AS DATETIME),
                    INTERVAL FLOOR(o.end_time / 100) HOUR
                ) + INTERVAL (o.end_time % 100) MINUTE + INTERVAL 20 HOUR
                OR EXISTS (
                    SELECT
                        1
                    FROM
                        report r2
                    WHERE
                        r2.order_id = o.id
                        AND r2.shop_id IS NOT NULL
                )
            ) THEN 1
            ELSE 0
        END AS IsAllowAction
    FROM
        report r
        JOIN `order` o ON r.order_id = o.id
        JOIN building b ON o.building_id = b.id
        JOIN shop s ON o.shop_id = s.id
        JOIN customer c ON o.customer_id = c.id
        JOIN account a ON a.id = c.id
    WHERE
        r.customer_id IS NOT NULL
        AND b.dormitory_id IN @DormitoryIds
        AND (
            @SearchValue IS NULL
            OR s.name LIKE CONCAT('%', @SearchValue, '%')
            OR a.full_name LIKE CONCAT('%', @SearchValue, '%')
            OR r.title LIKE CONCAT('%', @SearchValue, '%')
            OR r.content LIKE CONCAT('%', @SearchValue, '%')
            OR (
                CAST(r.id AS CHAR) LIKE CONCAT('%', @SearchValue, '%')
                OR CAST(r.order_id AS CHAR) LIKE CONCAT('%', @SearchValue, '%')
            )
        )
        AND r.status IN @StatusList
        AND (
            @DormitoryId IS NULL
            OR @DormitoryId = 0
            OR b.dormitory_id = @DormitoryId
        )
        AND (
            @DateFrom IS NULL
            OR r.created_date >= @DateFrom
        )
        AND (
            @DateTo IS NULL
            OR r.created_date <= @DateTo
        )
)
SELECT
    COUNT(*) OVER() AS TotalCount,
    Id,
    ShopName,
    CustomerName,
    OrderId,
    Title,
    Content,
    Status,
    IsUnderReview,
    CreatedDate,
    IsAllowAction
FROM
    FilteredReports
WHERE
    (
        (
            @IsAllStatus = 1
            AND (
                Status IN (2, 3)
                OR IsAllowAction = 1
            )
        )
        OR (
            @IsAllStatus = 0
            AND IsAllowAction = @IsAllowAction
            AND (
                (
                    @IsUnderReview = 1
                    AND IsUnderReview = 1
                )
                OR (
                    @IsUnderReview = 0
                    AND IsUnderReview = 0
                )
            )
        )
    )
ORDER BY
    IF(
        @OrderBy = 1
        AND @Direction = 1,
        CreatedDate,
        NULL
    ) ASC,
    IF(
        @OrderBy = 2
        AND @Direction = 1,
        ShopName,
        NULL
    ) ASC,
    IF(
        @OrderBy = 3
        AND @Direction = 1,
        CustomerName,
        NULL
    ) ASC,
    IF(
        @OrderBy = 4
        AND @Direction = 1,
        Title,
        NULL
    ) ASC,
    IF(
        @OrderBy = 5
        AND @Direction = 1,
        Content,
        NULL
    ) ASC,
    IF(
        @OrderBy = 1
        AND @Direction = 2,
        CreatedDate,
        NULL
    ) DESC,
    IF(
        @OrderBy = 2
        AND @Direction = 2,
        ShopName,
        NULL
    ) DESC,
    IF(
        @OrderBy = 3
        AND @Direction = 2,
        CustomerName,
        NULL
    ) DESC,
    IF(
        @OrderBy = 4
        AND @Direction = 2,
        Title,
        NULL
    ) DESC,
    IF(
        @OrderBy = 5
        AND @Direction = 2,
        Content,
        NULL
    ) DESC
LIMIT
    @PageSize OFFSET @Offset;