using System;
using System.Data;

namespace Sql.Native
{
    public class DbEntity<T> : DbCommandObject<DbEntity<T>>
    {
        private readonly IDbClassMap<T> map;

        public DbEntity(IDbConnection connection, IDbClassMap<T> map)
            : base(connection)
        {
            this.map = map;
        }

        public IDbObjectReader<T> SqlQuery(string sql)
        {
            return ExecuteReader(new DbMethod(sql, Type.EmptyTypes, new object[] { }));
        }

        public IDbObjectReader<T> SqlQuery<TArg1>(string sql, TArg1 arg1)
        {
            return ExecuteReader(new DbMethod(sql, new[] { typeof(TArg1) }, new object[] { arg1 }));
        }

        public IDbObjectReader<T> SqlQuery<TArg1, TArg2>(string sql, TArg1 arg1, TArg2 arg2)
        {
            return ExecuteReader(new DbMethod(sql, new[] { typeof(TArg1), typeof(TArg2) }, new object[] { arg1, arg2 }));
        }

        private IDbObjectReader<T> ExecuteReader(DbMethod method)
        {
            return method.ExecuteReader(Connection, r => new DbObjectReader<T>(r, map));
        }
    }
}