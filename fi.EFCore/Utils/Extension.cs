using fi.Core;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace fi.EFCore
{
    public static class Extension
    {
        static readonly ConcurrentDictionary<string, string> concSafe = new();
        static string InternConcurrent(string s) => concSafe.GetOrAdd(s, s);

        public static List<T> SqlReader<T>(this DbContext context, string query, CommandType commandType, Func<DbDataReader, T> map) where T : class, new()
        {
            List<T> entities;

            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = commandType;
                context.Database.OpenConnection();
                using (var result = command.ExecuteReader())
                {
                    entities = new List<T>();

                    while (result.Read())
                        entities.Add(map(result));
                }
                context.Database.CloseConnection();
            }

            return entities;
        }

        public static async Task<List<T>> SqlReaderAsync<T>(this DbContext context, string query, CommandType commandType, Func<DbDataReader, T> map, CancellationToken cancellationToken = default) where T : class, new()
        {
            List<T> entities;

            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = commandType;
                context.Database.OpenConnection();
                using (var result = await command.ExecuteReaderAsync(cancellationToken))
                {
                    entities = new List<T>();

                    while (result.Read())
                        entities.Add(map(result));
                }
                context.Database.CloseConnection();
            }

            return entities;
        }

        public static object SqlScalar(this DbContext context, string query, CommandType commandType)
        {
            object obj;

            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = commandType;
                context.Database.OpenConnection();
                obj = command.ExecuteScalar();
                context.Database.CloseConnection();
            }

            return obj;
        }

        public static async Task<object> SqlScalarAsync(this DbContext context, string query, CommandType commandType, CancellationToken cancellationToken = default)
        {
            object obj;

            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = commandType;
                context.Database.OpenConnection();
                obj = await command.ExecuteScalarAsync(cancellationToken);
                context.Database.CloseConnection();
            }

            return obj;
        }

        public static int SqlNonQuery(this DbContext context, string query, CommandType commandType)
        {
            int obj;

            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = commandType;
                context.Database.OpenConnection();
                obj = command.ExecuteNonQuery();
                context.Database.CloseConnection();
            }

            return obj;
        }

        public static async Task<int> SqlNonQueryAsync(this DbContext context, string query, CommandType commandType, CancellationToken cancellationToken = default)
        {
            int obj;

            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = commandType;
                context.Database.OpenConnection();
                obj = await command.ExecuteNonQueryAsync(cancellationToken);
                context.Database.CloseConnection();
            }

            return obj;
        }

        public static int SqlNonQueryWithParameters(this DbContext context, string query, CommandType commandType, SqlParameter[] sqlParameters)
        {
            int obj;

            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = commandType;
                command.Parameters.AddRange(sqlParameters);
                context.Database.OpenConnection();
                obj = command.ExecuteNonQuery();
                context.Database.CloseConnection();
            }

            return obj;
        }

        public static async Task<int> SqlNonQueryWithParametersAsync(this DbContext context, string query, CommandType commandType, SqlParameter[] sqlParameters, CancellationToken cancellationToken = default)
        {
            int obj;

            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = commandType;
                command.Parameters.AddRange(sqlParameters);
                context.Database.OpenConnection();
                obj = await command.ExecuteNonQueryAsync(cancellationToken);
                context.Database.CloseConnection();
            }

            return obj;
        }

        public static List<T> ToListFromCache<T>(this IQueryable<T> query, ICache cacheService, TimeSpan expireTime) where T : class
        {
            return ToListFromCache(query, cacheService, query.GetCacheKey(), expireTime);
        }

        public static List<T> ToListFromCache<T>(this IQueryable<T> query, ICache cacheService, string cacheKey, TimeSpan expireTime)
        {
            if (cacheKey is null)
                throw new ArgumentNullException($"'cacheKey' alanı null olamaz.");

            if (TimeSpan.Equals(expireTime, TimeSpan.Zero))
                throw new ArgumentException($"'expireTime' alanı 00:00:00 dan büyük olmalıdır.");

            if (string.IsNullOrWhiteSpace(cacheKey))
                throw new FormatException($"'cacheKey' alanı string.Empty yada boşluk bırakılamaz.");

            List<T> result = cacheService.Get<List<T>>(cacheKey);

            if (result is null)
            {
                lock (InternConcurrent(cacheKey))
                {
                    result = cacheService.Get<List<T>>(cacheKey);

                    if (result is null)
                    {
                        result = query.ToList();

                        if (result.Any())
                            cacheService.Add(cacheKey, result, expireTime);
                    }
                }
            }

            return result;
        }

        public static T FirstOrDefaultFromCache<T>(this IQueryable<T> query, ICache cacheService, TimeSpan expireTime) where T : class
        {
            return FirstOrDefaultFromCache(query, cacheService, query.GetCacheKey(), expireTime);
        }

        public static T FirstOrDefaultFromCache<T>(this IQueryable<T> query, ICache cacheService, string cacheKey, TimeSpan expireTime)
        {
            if (cacheKey is null)
                throw new ArgumentNullException($"'cacheKey' alanı null olamaz.");

            if (TimeSpan.Equals(expireTime, TimeSpan.Zero))
                throw new ArgumentException($"'expireTime' alanı 00:00:00 dan büyük olmalıdır.");

            if (string.IsNullOrWhiteSpace(cacheKey))
                throw new FormatException($"'cacheKey' alanı string.Empty yada boşluk bırakılamaz.");

            T result = cacheService.Get<T>(cacheKey);

            if (result == null)
            {
                lock (InternConcurrent(cacheKey))
                {
                    result = cacheService.Get<T>(cacheKey);

                    if (result == null)
                    {
                        result = query.FirstOrDefault();

                        if (null != result)
                            cacheService.Add(cacheKey, result, expireTime);
                    }
                }
            }

            return result;
        }

        public static string GetCacheKey<T>(this IQueryable<T> query) where T : class => query.ToQueryString().ToMd5Fingerprint();
        
        public static string ToMd5Fingerprint(this string s)
        {
            var bytes = Encoding.Unicode.GetBytes(s.ToCharArray());
            var hash = new MD5CryptoServiceProvider().ComputeHash(bytes);
            return hash.Aggregate(new StringBuilder(32), (sb, b) => sb.Append(b.ToString("X2"))).ToString();
        }

        [Obsolete(".NET 5 EFCore için ToQueryString() kullanılmalıdır.")]
        public static string ToSql<TEntity>(this IQueryable<TEntity> query) where TEntity : class
        {
            var enumerator = query.Provider.Execute<IEnumerable<TEntity>>(query.Expression).GetEnumerator();
            var relationalCommandCache = enumerator.Private("_relationalCommandCache");
            var selectExpression = relationalCommandCache.Private<SelectExpression>("_selectExpression");
            var factory = relationalCommandCache.Private<IQuerySqlGeneratorFactory>("_querySqlGeneratorFactory");
            var queryContextFieldInfo = enumerator.GetType().GetField("_relationalQueryContext", BindingFlags.NonPublic | BindingFlags.Instance);

            var queryContext = queryContextFieldInfo.GetValue(enumerator) as RelationalQueryContext;

            var sqlGenerator = factory.Create();
            var command = sqlGenerator.GetCommand(selectExpression);

            var parametersDict = queryContext.ParameterValues;
            string sql = command.CommandText;

            foreach (var item in parametersDict)
            {
                sql = sql.Replace($"@{item.Key}", $"'{item.Value}'");
            }

            return sql;
        }

        private static object Private(this object obj, string privateField) => obj?.GetType().GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);
        private static T Private<T>(this object obj, string privateField) => (T)obj?.GetType().GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);
    }
}