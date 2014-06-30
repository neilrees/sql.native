using System;
using System.Data;

namespace Sql.Native
{
    public static class DbConnectionExtensions
    {
        public static DbEntity<T> Entity<T>(this IDbConnection connection, IDbClassMap<T> map)
        {
            return new DbEntity<T>(connection, map);
        }

        public static DbEntity<T> Entity<T>(this IDbConnection connection)
        {
            return connection.Entity(DbClassMap<T>.Default);
        }

        public static int ExecuteNonQuery(this IDbConnection connection, string sql)
        {
            return new DbMethod(sql).ExecuteNonQuery(connection);
        }

        public static int ExecuteNonQuery<TArg1>(this IDbConnection connection, string sql, TArg1 arg1)
        {
            return new DbMethod(sql, new[] {typeof(TArg1)}, new object[] {arg1}).ExecuteNonQuery(connection);
        }

        public static int ExecuteNonQuery<TArg1, TArg2>(this IDbConnection connection, string sql, TArg1 arg1, TArg2 arg2)
        {
            return new DbMethod(sql, new[] { typeof(TArg1), typeof(TArg2) }, new object[] { arg1, arg2 }).ExecuteNonQuery(connection);
        }

        public static int ExecuteNonQuery<TArg1, TArg2, TArg3>(this IDbConnection connection, string sql, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            return new DbMethod(sql, new[] { typeof(TArg1), typeof(TArg2), typeof(TArg3) }, new object[] { arg1, arg2, arg3 }).ExecuteNonQuery(connection);
        }

        public static int ExecuteNonQuery<TArg1, TArg2, TArg3, TArg4>(this IDbConnection connection, string sql, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            return new DbMethod(sql, new[] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4) }, new object[] { arg1, arg2, arg3, arg4 }).ExecuteNonQuery(connection);
        }

        public static TReturn ExecuteReader<TReturn>(this IDbConnection connection, string sql, Func<IDataReader, TReturn> transform)
        {
            return new DbMethod(sql, Type.EmptyTypes, new object[] { }).ExecuteReader(connection, transform);
        }

        public static TReturn ExecuteReader<TArg1, TReturn>(this IDbConnection connection, string sql, Func<IDataReader, TReturn> transform, TArg1 arg1)
        {
            return new DbMethod(sql, new [] { typeof(TArg1) }, new object[] { arg1 }).ExecuteReader(connection, transform);
        }

        public static TReturn ExecuteReader<TArg1, TArg2, TReturn>(this IDbConnection connection, string sql, Func<IDataReader, TReturn> transform, TArg1 arg1, TArg2 arg2)
        {
            return new DbMethod(sql, new[] { typeof(TArg1), typeof(TArg2) }, new object[] { arg1, arg2 }).ExecuteReader(connection, transform);
        }

        public static TReturn ExecuteReader<TArg1, TArg2, TArg3, TReturn>(this IDbConnection connection, string sql, Func<IDataReader, TReturn> transform, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            return new DbMethod(sql, new[] { typeof(TArg1), typeof(TArg2), typeof(TArg3) }, new object[] { arg1, arg2, arg3 }).ExecuteReader(connection, transform);
        }

        public static TReturn ExecuteReader<TArg1, TArg2, TArg3, TArg4, TReturn>(this IDbConnection connection, string sql, Func<IDataReader, TReturn> transform, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            return new DbMethod(sql, new[] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4) }, new object[] { arg1, arg2, arg3, arg4 }).ExecuteReader(connection, transform);
        }

        public static TReturn ExecuteScalar<TReturn>(this IDbConnection connection, string sql)
        {
            return new DbMethod(sql, Type.EmptyTypes, new object[] { }).ExecuteScalar<TReturn>(connection);
        }

        public static TReturn ExecuteScalar<TArg1, TReturn>(this IDbConnection connection, string sql, TArg1 arg1)
        {
            return new DbMethod(sql, new[] { typeof(TArg1) }, new object[] { arg1 }).ExecuteScalar<TReturn>(connection);
        }

        public static TReturn ExecuteScalar<TArg1, TArg2, TReturn>(this IDbConnection connection, string sql, TArg1 arg1, TArg2 arg2)
        {
            return new DbMethod(sql, new[] { typeof(TArg1), typeof(TArg2) }, new object[] { arg1, arg2 }).ExecuteScalar<TReturn>(connection);
        }

        public static TReturn ExecuteScalar<TArg1, TArg2, TArg3, TReturn>(this IDbConnection connection, string sql, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            return new DbMethod(sql, new[] { typeof(TArg1), typeof(TArg2), typeof(TArg3) }, new object[] { arg1, arg2, arg3 }).ExecuteScalar<TReturn>(connection);
        }

        public static TReturn ExecuteScalar<TArg1, TArg2, TArg3, TArg4, TReturn>(this IDbConnection connection, string sql, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            return new DbMethod(sql, new[] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4) }, new object[] { arg1, arg2, arg3, arg4 }).ExecuteScalar<TReturn>(connection);
        }
    }
}