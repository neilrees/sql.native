using System;
using System.Data;

namespace Sql.Native
{
    public static class DbBuilderExtensions
    {
        public static IDbPropertyMapBuilder<T, TProperty> HasColumnReader<T, TProperty>(this IDbPropertyMapBuilder<T, TProperty> instance, 
            Func<IDataRecord, int, TProperty> columnReader)
        {
            instance.HasColumnReader(new DelegateDbColumnReader<TProperty>(columnReader));
            return instance;
        }
    }
}