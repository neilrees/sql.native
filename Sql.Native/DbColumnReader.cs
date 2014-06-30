using System;
using System.Data;

namespace Sql.Native
{
    public static class DbColumnReader
    {
        public static Func<IDataRecord, int, T> StringToEnum<T>() where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new Exception();
            }
            return (reader, index) =>
                {
                    if (reader.IsDBNull(index))
                    {
                        return default(T);
                    }
                    return (T)Enum.Parse(typeof(T), reader.GetString(index));
                };
        }
    }
}