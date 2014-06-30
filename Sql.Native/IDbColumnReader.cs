using System.Data;

namespace Sql.Native
{
    /// <summary>
    /// Reads column values from an <see cref="IDataRecord"/>.
    /// </summary>
    public interface IDbColumnReader
    {
        object Read(IDataRecord reader, int index);
    }

    /// <summary>
    /// Reads column values from an <see cref="IDataRecord"/>.
    /// </summary>
    public interface IDbColumnReader<out TProperty> : IDbColumnReader
    {
        new TProperty Read(IDataRecord reader, int index);
    }
}