using System;
using System.Reflection;

namespace Sql.Native
{
    public interface IDbPropertyMap
    {
        MemberInfo Member { get; }
        string ColumnName { get; }
        IDbColumnReader ColumnReader { get; }
        Delegate PropertySetter { get; }
    }

    public interface IDbPropertyMap<T, TProperty> : IDbPropertyMap
    {
        new IDbColumnReader<TProperty> ColumnReader { get; }
        new Action<T, TProperty> PropertySetter { get; }
    }
}