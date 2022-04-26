using fi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
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

        #region SqlReader
        public static List<T> SqlReader<T>(this DbContext context, string query, CommandType commandType, Func<DbDataReader, T> map) where T : class, new()
        {
            using var transaction = context.Database.BeginTransaction();
            var entities = context.SqlReader(query, commandType, transaction.GetDbTransaction(), map);
            transaction.Commit();

            return entities;
        }

        public static List<T> SqlReader<T>(this DbContext context, string query, CommandType commandType, DbTransaction transaction, Func<DbDataReader, T> map) where T : class, new()
        {
            List<T> entities;

            using (DbCommand command = context.Database.GetDbConnection().CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = query;
                command.CommandType = commandType;
                using var result = command.ExecuteReader();
                entities = new List<T>();

                while (result.Read())
                    entities.Add(map(result));
            }

            return entities;
        }
        #endregion

        #region SqlReaderAsync
        public static async Task<List<T>> SqlReaderAsync<T>(this DbContext context, string query, CommandType commandType, Func<DbDataReader, T> map, CancellationToken cancellationToken = default) where T : class, new()
        {
            using var transaction = context.Database.BeginTransaction();
            var entities = await context.SqlReaderAsync(query, commandType, transaction.GetDbTransaction(), map, cancellationToken);
            transaction.Commit();
            return entities;
        }

        public static async Task<List<T>> SqlReaderAsync<T>(this DbContext context, string query, CommandType commandType, DbTransaction transaction, Func<DbDataReader, T> map, CancellationToken cancellationToken = default) where T : class, new()
        {
            List<T> entities;

            using (DbCommand command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = commandType;
                command.Transaction = transaction;
                using var result = await command.ExecuteReaderAsync(cancellationToken);
                entities = new List<T>();

                while (result.Read())
                    entities.Add(map(result));
            }

            return entities;
        }
        #endregion

        #region SqlScalar
        public static object SqlScalar(this DbContext context, string query, CommandType commandType)
        {
            using var transaction = context.Database.BeginTransaction();
            var obj = context.SqlScalar(query, commandType, transaction);
            transaction.Commit();

            return obj;
        }

        public static object SqlScalar(this DbContext context, string query, CommandType commandType, IDbContextTransaction transaction)
        {
            object obj;

            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = commandType;
                command.Transaction = transaction.GetDbTransaction();
                obj = command.ExecuteScalar();
            }

            return obj;
        }
        #endregion

        #region SqlScalarAsync
        public static async Task<object> SqlScalarAsync(this DbContext context, string query, CommandType commandType, CancellationToken cancellationToken = default)
        {
            using var transaction = context.Database.BeginTransaction();
            object obj = await context.SqlScalarAsync(query, commandType, transaction, cancellationToken);
            transaction.Commit();

            return obj;
        }

        public static async Task<object> SqlScalarAsync(this DbContext context, string query, CommandType commandType, IDbContextTransaction transaction, CancellationToken cancellationToken = default)
        {
            object obj;

            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = commandType;
                command.Transaction = transaction.GetDbTransaction();
                obj = await command.ExecuteScalarAsync(cancellationToken);
            }

            return obj;
        }
        #endregion

        #region SqlNonQuery
        public static int SqlNonQuery(this DbContext context, string query, CommandType commandType)
        {
            int obj;

            using var transaction = context.Database.BeginTransaction();
            obj = context.SqlNonQuery(query, commandType, transaction);
            transaction.Commit();

            return obj;
        }

        public static int SqlNonQuery(this DbContext context, string query, CommandType commandType, IDbContextTransaction transaction)
        {
            int obj;

            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = commandType;
                command.Transaction = transaction.GetDbTransaction();
                obj = command.ExecuteNonQuery();
            }

            return obj;
        }
        #endregion

        #region SqlNonQueryAsync
        public static async Task<int> SqlNonQueryAsync(this DbContext context, string query, CommandType commandType, CancellationToken cancellationToken = default)
        {
            int obj;

            using var transaction = context.Database.BeginTransaction();
            obj = await context.SqlNonQueryAsync(query, commandType, transaction, cancellationToken);
            transaction.Commit();
            return obj;
        }
        public static async Task<int> SqlNonQueryAsync(this DbContext context, string query, CommandType commandType, IDbContextTransaction transaction, CancellationToken cancellationToken = default)
        {
            int obj;

            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = commandType;
                command.Transaction = transaction.GetDbTransaction();
                obj = await command.ExecuteNonQueryAsync(cancellationToken);
            }

            return obj;
        }
        #endregion

        #region SqlNonQueryWithParameters
        public static int SqlNonQueryWithParameters(this DbContext context, string query, CommandType commandType, DbParameter[] sqlParameters)
        {
            int obj;

            using var transaction = context.Database.BeginTransaction();
            obj = context.SqlNonQueryWithParameters(query, commandType, transaction, sqlParameters);
            transaction.Commit();
            return obj;
        }

        public static int SqlNonQueryWithParameters(this DbContext context, string query, CommandType commandType, IDbContextTransaction transaction, DbParameter[] sqlParameters)
        {
            int obj;

            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.Transaction = transaction.GetDbTransaction();
                command.CommandText = query;
                command.CommandType = commandType;
                command.Parameters.AddRange(sqlParameters);
                obj = command.ExecuteNonQuery();
            }

            return obj;
        }
        #endregion

        #region SqlNonQueryWithParametersAsync
        public static async Task<int> SqlNonQueryWithParametersAsync(this DbContext context, string query, CommandType commandType, DbParameter[] sqlParameters, CancellationToken cancellationToken = default)
        {
            int obj;

            using var transaction = context.Database.BeginTransaction();
            obj = await context.SqlNonQueryWithParametersAsync(query, commandType, transaction, sqlParameters, cancellationToken);
            transaction.Commit();
            return obj;
        }
        public static async Task<int> SqlNonQueryWithParametersAsync(this DbContext context, string query, CommandType commandType, IDbContextTransaction transaction, DbParameter[] sqlParameters, CancellationToken cancellationToken = default)
        {
            int obj;

            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.Transaction = transaction.GetDbTransaction();
                command.CommandText = query;
                command.CommandType = commandType;
                command.Parameters.AddRange(sqlParameters);
                obj = await command.ExecuteNonQueryAsync(cancellationToken);
            }

            return obj;
        }
        #endregion

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
            var hash = MD5.Create().ComputeHash(bytes);
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