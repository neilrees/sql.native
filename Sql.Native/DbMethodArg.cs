using System;
using System.Data;

namespace Sql.Native
{
    public class DbMethodArg
    {
        public DbMethodArg(string name, Type type, object value, ParameterDirection direction = ParameterDirection.Input)
        {
            Name = name;
            Type = type;
            Value = value;
            Direction = direction;
        }

        public string Name { get; private set; }
        public Type Type { get; private set; }
        public object Value { get; internal set; }
        public ParameterDirection Direction { get; private set; }

        public bool IsComplexType
        {
            get
            {
                return DbConverter.ToDbType(Type) == DbType.Object;
            }
        }
    }
}