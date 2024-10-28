namespace MealSync.Application.Common.Services.Dapper;

public interface IDapperService
{
    IEnumerable<T> Query<T>(QueryName queryName, object queryParams = null);

    Task<T> ExecuteScalarAsync<T>(QueryName queryName, object queryParams);

    T SingleOrDefault<T>(QueryName queryName, object queryParams);

    Task<T> SingleOrDefaultAsync<T>(QueryName queryName, object queryParams);

    Task<dynamic> SelectAsync(string query);

    Task<IEnumerable<T>> SelectAsync<T>(QueryName queryName, object queryParams);

    Task<IEnumerable<TReturn>> SelectAsync<TFirst, TSecond, TReturn>(QueryName queryName, Func<TFirst, TSecond, TReturn> map, object queryParams, string splitOn);

    Task<IEnumerable<TReturn>> SelectAsync<TFirst, TSecond, TThird, TReturn>(QueryName queryName, Func<TFirst, TSecond, TThird, TReturn> map, object queryParams, string splitOn);

    Task<IEnumerable<TReturn>> SelectAsync<TFirst, TSecond, TThird, TFourth, TReturn>(QueryName queryName, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object queryParams, string splitOn);

    Task<IEnumerable<TReturn>> SelectAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(QueryName queryName, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object queryParams, string splitOn);
}

public enum QueryName
{
    TestQuery,
    GetListCategoryWithFood,
    GetListOrderForShopByStatus,
    GetShopOrderDetail,
    GetOrderHistoryOfCustomer,
    GetListOrderForShopWebByStatus,
}