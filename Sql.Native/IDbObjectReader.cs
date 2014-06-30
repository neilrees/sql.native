using System;
using System.Collections.Generic;

namespace Sql.Native
{
    public interface IDbObjectReader<out T> : IEnumerable<T>, IDisposable
    {
    }
}