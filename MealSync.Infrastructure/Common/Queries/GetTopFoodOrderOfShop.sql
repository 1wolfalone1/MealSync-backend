WITH TopFoodOrder AS (
  SELECT
    od.food_id,
    COUNT(DISTINCT o.id) AS total_orders
  FROM
    `order` o
    JOIN order_detail od ON o.id = od.order_id
    JOIN food f ON od.food_id = f.id
  WHERE
    o.shop_id = @ShopId
    AND o.status = 9 -- Completed
    AND o.reason_identity IS NULL
    AND o.intended_receive_date BETWEEN @StartDate
    AND @EndDate
  GROUP BY
    od.food_id
  ORDER BY
    total_orders DESC
  LIMIT
    @NumberTopProduct
)
SELECT
  tfo.food_id AS id,
  f.name AS Name,
  f.image_url AS ImageUrl,
  f.status AS Status,
  tfo.total_orders AS TotalOrders
FROM
  food f
  JOIN TopFoodOrder tfo ON f.id = tfo.food_id