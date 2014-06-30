using System;

namespace Sql.Native
{
    public interface IDbPropertyMapBuilder<T, TProperty> : IDbPropertyMap<T, TProperty>
    {
        IDbPropertyMapBuilder<T, TProperty> HasColumnName(string columnName);
        IDbPropertyMapBuilder<T, TProperty> HasColumnReader(IDbColumnReader<TProperty> columnReader);
        IDbPropertyMapBuilder<T, TProperty> HasPropertySetter(Action<T, TProperty> propertySetter);
        IDbPropertyMap<T, TProperty> CreateMap();
    }

    public interface IDbPropertyMapBuilder : IDbPropertyMap
    {
        IDbPropertyMapBuilder HasColumnName(string columnName);
        IDbPropertyMapBuilder HasColumnReader(IDbColumnReader columnReader);
        IDbPropertyMap CreateMap();
        void HasPropertySetter(Delegate propertySetter);
    }
}