using System;
using System.Data;

namespace Sql.Native
{
    /// <summary>
    /// The db converter.
    /// </summary>
    public static class DbConverter
    {
        /// <summary>
        /// The to dbtype method.
        /// </summary>
        /// <param name="type">The source type</param>
        /// <returns>The destination type.</returns>
        public static DbType ToDbType(Type type)
        {
            if (type.IsByRef && type.HasElementType)
            {
                type = type.GetElementType();
            }

            type = Nullable.GetUnderlyingType(type) ?? type;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.DBNull:
                    return DbType.String;
                case TypeCode.Boolean:
                    return DbType.Boolean;
                case TypeCode.Char:
                    return DbType.String;
                case TypeCode.SByte:
                    return DbType.SByte;
                case TypeCode.Byte:
                    return DbType.Byte;
                case TypeCode.Int16:
                    return DbType.Int16;
                case TypeCode.UInt16:
                    return DbType.UInt16;
                case TypeCode.Int32:
                    return DbType.Int32;
                case TypeCode.UInt32:
                    return DbType.UInt32;
                case TypeCode.Int64:
                    return DbType.Int64;
                case TypeCode.UInt64:
                    return DbType.UInt32;
                case TypeCode.Single:
                    return DbType.Single;
                case TypeCode.Double:
                    return DbType.Double;
                case TypeCode.Decimal:
                    return DbType.Decimal;
                case TypeCode.DateTime:
                    return DbType.DateTime;
                case TypeCode.String:
                    return DbType.String;
                default:
                    if (type == typeof(Guid))
                    {
                        return DbType.Guid;
                    }
                    return DbType.Object;
            }
        }

        /// <summary>
        /// The to db object method.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="type">The db type.</param>
        /// <returns>The db object.</returns>
        internal static object ToDbObject(object obj, DbType type)
        {
            // is the value null?
            if (obj == null)
            {
                return DBNull.Value;
            }

            // is the value already of the correct type?
            var sourceType = ToDbType(obj.GetType());
            if (sourceType == type)
            {
                return obj;
            }

            // convert
            switch (type)
            {
                case DbType.AnsiString:
                    return Convert.ToString(obj);
                case DbType.Binary:
                    if (obj is byte[])
                    {
                        return obj;
                    }

                    throw new NotSupportedException();
                case DbType.Byte:
                    return Convert.ToByte(obj);
                case DbType.Boolean:
                    return Convert.ToBoolean(obj);
                case DbType.Currency:
                    return Convert.ToDecimal(obj);
                case DbType.Date:
                    return Convert.ToDateTime(obj).Date;
                case DbType.DateTime:
                    return Convert.ToDateTime(obj);
                case DbType.Decimal:
                    return Convert.ToDecimal(obj);
                case DbType.Double:
                    return Convert.ToDouble(obj);
                case DbType.Guid:
                    if (obj is Guid)
                    {
                        return (Guid)obj;
                    }

                    if (obj is byte[])
                    {
                        return new Guid((byte[])obj);
                    }

                    return new Guid(obj.ToString());
                case DbType.Int16:
                    return Convert.ToInt16(obj);
                case DbType.Int32:
                    return Convert.ToInt32(obj);
                case DbType.Int64:
                    return Convert.ToInt64(obj);
                case DbType.Object:
                    return obj;
                case DbType.SByte:
                    return Convert.ToSByte(obj);
                case DbType.Single:
                    return Convert.ToSingle(obj);
                case DbType.String:
                    return Convert.ToString(obj);
                case DbType.UInt16:
                    return Convert.ToUInt16(obj);
                case DbType.UInt32:
                    return Convert.ToUInt32(obj);
                case DbType.UInt64:
                    return Convert.ToUInt64(obj);
                case DbType.VarNumeric:
                    return Convert.ToDecimal(obj);
                case DbType.AnsiStringFixedLength:
                    return Convert.ToString(obj);
                case DbType.StringFixedLength:
                    return Convert.ToString(obj);
                default:
                    throw new ArgumentOutOfRangeException("type", "Invalid or unsupported type.");
            }
        }

        /// <summary>
        /// The to db object method.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The db object.</returns>
        internal static object ToDbObject(object obj)
        {
            return obj ?? DBNull.Value;
        }
    }
}
