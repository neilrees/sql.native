using System;
using System.Collections.ObjectModel;
using System.Data;

namespace Sql.Native
{
    public interface IDbClassMap<T> : IDbClassMap
    {
        Func<IDataRecord, T> EntityFactory { get; }
    }

    public interface IDbClassMap
    {
        Type Type { get; }
        ReadOnlyCollection<IDbPropertyMap> Properties { get; }
    }
}