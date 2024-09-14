
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using System.Data.Common;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using MealSync.Application.Common.Services.Dapper;

namespace MealSync.Infrastructure.Services.Dapper;

public class DapperService : IDapperService
{
    private static readonly HashSet<QueryName> CachedQuery = new HashSet<QueryName>()
    {
    };

    private static readonly TimeSpan CacheSpan = TimeSpan.FromMinutes(10);
    private readonly IDbConnection db;
    private readonly IMemoryCache cache;
    private readonly Assembly assembly;
    public DapperService(IDbConnection db, IMemoryCache cache)
    {
        this.db = db;
        this.cache = cache;
        this.assembly = this.GetType().Assembly;
    }

    public Task<T> ExecuteScalarAsync<T>(QueryName queryName, object queryParams)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<T> Query<T>(QueryName queryName, object queryParams = null)
    {
        if (this.TryGetCache(queryName, queryParams, out IEnumerable<T> result, out string cacheKey))
        {
            return result;
        }

        string query = this.GetQueryString(queryName);
        bool isClosed = this.db.State == ConnectionState.Closed;

        try
        {
            if (isClosed)
            {
                this.db.Open();
            }

            result = this.db.Query<T>(query, queryParams);
            this.SetCache(cacheKey, result);

            return result;
        }
        finally
        {
            if (isClosed)
                this.db.Close();
        }
    }

    public async Task<dynamic> SelectAsync(string query)
    {
        Debug.WriteLine("DapperSerice: " + query);
        bool isClosed = this.db.State == ConnectionState.Closed;
        try
        {
            if (isClosed)
            {
                await ((DbConnection)this.db).OpenAsync().ConfigureAwait(false);
            }

            return await this.db.QueryAsync(query).ConfigureAwait(false);
        }
        catch
        {
            throw;
        }
        finally
        {
            await ((DbConnection)this.db).CloseAsync().ConfigureAwait(false);
        }

    }

    public async Task<IEnumerable<T>> SelectAsync<T>(QueryName queryName, object queryParams)
    {
        if (this.TryGetCache(queryName, queryParams, out IEnumerable<T> result, out string cacheKey))
        {
            return result;
        }

        string query = await this.GetQueryStringAsync(queryName).ConfigureAwait(false);
        bool isClosed = this.db.State == ConnectionState.Closed;

        try
        {
            if (isClosed)
            {
                await ((DbConnection)this.db).OpenAsync().ConfigureAwait(false);
            }

            result = await this.db.QueryAsync<T>(query, queryParams).ConfigureAwait(false);


            this.SetCache(cacheKey, result);

            return result;
        }
        finally
        {
            if (isClosed)
                await ((DbConnection)this.db).CloseAsync().ConfigureAwait(false);
        }
    }

    public async Task<IEnumerable<TReturn>> SelectAsync<TFirst, TSecond, TReturn>(QueryName queryName, Func<TFirst, TSecond, TReturn> map, object queryParams, string splitOn)
    {
        string query = await this.GetQueryStringAsync(queryName).ConfigureAwait(false);
        bool isClosed = this.db.State == ConnectionState.Closed;

        try
        {
            if (isClosed)
            {
                await ((DbConnection)this.db).OpenAsync().ConfigureAwait(false);
            }

            return await this.db.QueryAsync<TFirst, TSecond, TReturn>(query, map, queryParams, splitOn: splitOn).ConfigureAwait(false);

        }
        finally
        {
            if (isClosed)
                await ((DbConnection)this.db).CloseAsync().ConfigureAwait(false);
        }
    }

    public async Task<IEnumerable<TReturn>> SelectAsync<TFirst, TSecond, TThird, TReturn>(QueryName queryName, Func<TFirst, TSecond, TThird, TReturn> map, object queryParams, string splitOn)
    {
        string query = await this.GetQueryStringAsync(queryName).ConfigureAwait(false);
        bool isClosed = this.db.State == ConnectionState.Closed;

        try
        {
            if (isClosed)
            {
                await ((DbConnection)this.db).OpenAsync().ConfigureAwait(false);
            }

            return await this.db.QueryAsync<TFirst, TSecond, TThird, TReturn>(query, map, queryParams, splitOn: splitOn).ConfigureAwait(false);

        }
        finally
        {
            if (isClosed)
                await ((DbConnection)this.db).CloseAsync().ConfigureAwait(false);
        }
    }

    public async Task<IEnumerable<TReturn>> SelectAsync<TFirst, TSecond, TThird, TFourth, TReturn>(QueryName queryName, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object queryParams, string splitOn)
    {
        string query = await this.GetQueryStringAsync(queryName).ConfigureAwait(false);
        bool isClosed = this.db.State == ConnectionState.Closed;

        try
        {
            if (isClosed)
            {
                await ((DbConnection)this.db).OpenAsync().ConfigureAwait(false);
            }

            return await this.db.QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(query, map, queryParams, splitOn: splitOn).ConfigureAwait(false);

        }
        finally
        {
            if (isClosed)
                await ((DbConnection)this.db).CloseAsync().ConfigureAwait(false);
        }
    }

    public async Task<IEnumerable<TReturn>> SelectAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(QueryName queryName, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object queryParams, string splitOn)
    {
        string query = await this.GetQueryStringAsync(queryName).ConfigureAwait(false);
        bool isClosed = this.db.State == ConnectionState.Closed;

        try
        {
            if (isClosed)
            {
                await ((DbConnection)this.db).OpenAsync().ConfigureAwait(false);
            }

            return await this.db.QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(query, map, queryParams, splitOn: splitOn).ConfigureAwait(false);

        }
        finally
        {
            if (isClosed)
                await ((DbConnection)this.db).CloseAsync().ConfigureAwait(false);
        }
    }

    public async Task<T> SingleOrDefaultAsync<T>(QueryName queryName, object queryParams)
    {
        if (this.TryGetCache(queryName, queryParams, out T result, out string cacheKey))
        {
            return result;
        }

        string query = await this.GetQueryStringAsync(queryName).ConfigureAwait(false);
        bool isClosed = this.db.State == ConnectionState.Closed;

        try
        {
            if (isClosed)
            {
                await ((DbConnection)this.db).OpenAsync().ConfigureAwait(false);
            }

            var tmpResult = await this.db.QueryAsync<T>(query, queryParams).ConfigureAwait(false);
            if (tmpResult.Count() > 1)
            {
                throw new InvalidOperationException($"Query {queryName} have more than 1 record");
            }
            else
            {
                result = tmpResult.FirstOrDefault();
            }

            this.SetCache(cacheKey, result);

            return result;
        }
        finally
        {
            if (isClosed)
                await ((DbConnection)this.db).CloseAsync().ConfigureAwait(false);
        }
    }

    public T SingleOrDefault<T>(QueryName queryName, object queryParams)
    {
        if (this.TryGetCache(queryName, queryParams, out T result, out string cacheKey))
        {
            return result;
        }

        string query = this.GetQueryString(queryName);
        bool isClosed = this.db.State == ConnectionState.Closed;

        try
        {
            if (isClosed)
            {
                this.db.Open();
            }

            var tmpResult = this.db.Query<T>(query, queryParams);
            if (tmpResult.Count() > 1)
            {
                throw new InvalidOperationException($"Query {queryName} have more than 1 record");
            }
            else
            {
                result = tmpResult.FirstOrDefault();
            }

            this.SetCache(cacheKey, result);

            return result;
        }
        finally
        {
            if (isClosed)
                this.db.Close();
        }
    }

    private string GetQueryString(QueryName queryName)
    {
        string result = null;

        string sqlFile = queryName.ToString() + ".sql";
        Stream stream = this.assembly.GetManifestResourceStream(this.assembly.GetName().Name + ".Common.Queries." + sqlFile);
        StreamReader reader = new StreamReader(stream);
        result = reader.ReadToEnd();

        if (string.IsNullOrWhiteSpace(result))
        {
            throw new ArgumentException("Not exist file sql with name ", sqlFile);
        }

        Debug.WriteLine("Dapper: " + sqlFile);
        return result;
    }

    private async Task<string> GetQueryStringAsync(QueryName queryName)
    {
        string result = null;

        string sqlFile = queryName.ToString() + ".sql";
        Stream stream = this.assembly.GetManifestResourceStream(this.assembly.GetName().Name + ".Common.Queries." + sqlFile);
        StreamReader reader = new StreamReader(stream);
        result = await reader.ReadToEndAsync().ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(result))
        {
            throw new ArgumentException("Not exist file sql with name ", sqlFile);
        }

        Debug.WriteLine("Dapper: " + sqlFile);
        return result;
    }

    private object AddValue<T>()
    {
        SqlMapper.SetTypeMap(typeof(T), new TypeMap<T>());
        return null;
    }

    private object UpdateValue(Type type, object existValue)
    {
        return null;
    }

    private void SetCache<T>(string cacheKey, T value)
    {
        if (cacheKey != default)
        {
            this.cache.Set(cacheKey, value, CacheSpan);
        }
    }

    private bool TryGetCache<T>(QueryName queryName, object queryParams, out T result, out string cacheKey)
    {
        if (!CachedQuery.Contains(queryName))
        {
            result = default;
            cacheKey = default;
            return false;
        }

        cacheKey = queryName.ToString() + typeof(T).FullName;
        if (queryName != null)
        {
            cacheKey += JsonSerializer.Serialize(queryParams);
        }

        return this.cache.TryGetValue(cacheKey, out result);
    }
}
