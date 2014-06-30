using System;
using System.Data;

namespace Sql.Native
{
    /// <summary>
    /// Extension methods for the data record class
    /// </summary>
    public static class DataRecordExtensions
    {
        /// <summary>
        /// Gets the string value of the column or <c>null</c> if the column is DBNull.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="index">The index.</param>
        /// <returns>The string value.</returns>
        public static string GetStringOrDefault(this IDataRecord instance, int index)
        {
            return instance.IsDBNull(index) ? null : instance.GetString(index);
        }

        /// <summary>
        /// Gets the Int32 value of the column or <c>0</c> if the column is DBNull.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="index">The index.</param>
        /// <returns>The Int32 value.</returns>
        public static int GetInt32OrDefault(this IDataRecord instance, int index)
        {
            return instance.IsDBNull(index) ? 0 : instance.GetInt32(index);
        }

        /// <summary>
        /// Gets the Int64 value of the column or <c>0</c> if the column is DBNull.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="index">The index.</param>
        /// <returns>The Int64 value.</returns>
        public static long GetInt64OrDefault(this IDataRecord instance, int index)
        {
            return instance.IsDBNull(index) ? 0 : instance.GetInt64(index);
        }

        /// <summary>
        /// Gets the boolean value of the columns or <c>false</c> if the column is <c>DBNull</c>.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="index">The index.</param>
        /// <returns>The boolean value.</returns>
        public static bool GetBooleanOrDefault(this IDataRecord instance, int index)
        {
            return instance.IsDBNull(index) ? false : instance.GetBoolean(index);
        }

        /// <summary>
        /// Reads from the reader but checks for null first
        /// </summary>
        /// <typeparam name="T">The return type of the read</typeparam>
        /// <param name="reader">The reader.</param>
        /// <param name="read">The read from the erader.</param>
        /// <param name="index">The index.</param>
        /// <returns>The value, or default(T) if dbnull</returns>
        public static T SafeRead<T>(this IDataRecord reader, Func<IDataRecord, int, T> read, int index)
        {
            return reader.IsDBNull(index) ? default(T) : read(reader, index);
        }
    }
}
