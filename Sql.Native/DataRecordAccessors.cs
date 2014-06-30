using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Sql.Native
{
    public static class DataRecordAccessors
    {
        private static readonly Dictionary<Type, MethodInfo> DefaultGetMethods = new Dictionary<Type, MethodInfo>()
            {
                {typeof(Int16), TypeRevealer<IDataRecord>.Method(x => x.GetInt16(0))},
                {typeof(Int32), TypeRevealer<IDataRecord>.Method(x => x.GetInt32(0))},
                {typeof(Int64), TypeRevealer<IDataRecord>.Method(x => x.GetInt64(0))},
                {typeof(Decimal), TypeRevealer<IDataRecord>.Method(x => x.GetDecimal(0))},
                {typeof(Boolean), TypeRevealer<IDataRecord>.Method(x => x.GetBoolean(0))},
                {typeof(String), TypeRevealer<IDataRecord>.Method(x => x.GetString(0))},
                {typeof(DateTime), TypeRevealer<IDataRecord>.Method(x => x.GetDateTime(0))},
                {typeof(Single), TypeRevealer<IDataRecord>.Method(x => x.GetFloat(0))},
                {typeof(Double), TypeRevealer<IDataRecord>.Method(x => x.GetDouble(0))},
                {typeof(Char), TypeRevealer<IDataRecord>.Method(x => x.GetChar(0))},
                {typeof(Byte), TypeRevealer<IDataRecord>.Method(x => x.GetByte(0))},
                {typeof(Guid), TypeRevealer<IDataRecord>.Method(x => x.GetGuid(0))},
                {typeof(Object), TypeRevealer<IDataRecord>.Method(x => x.GetValue(0))},
            };

        public static MethodInfo IsDBNull = TypeRevealer<IDataRecord>.Method(x => x.IsDBNull(0));
        
        public static MethodInfo GetGetMethod(Type fieldType)
        {
            MethodInfo result;
            DefaultGetMethods.TryGetValue(fieldType, out result);
            return result;
        }
    }
}