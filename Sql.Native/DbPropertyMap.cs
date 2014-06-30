using System;
using System.Reflection;

namespace Sql.Native
{
    public class DbPropertyMap<T, TProperty> : IDbPropertyMap<T, TProperty>
    {
        internal DbPropertyMap(IDbPropertyMap<T, TProperty> source)
        {
            this.ColumnName = source.ColumnName;
            this.Member = source.Member;
            this.ColumnReader = source.ColumnReader;
            this.PropertySetter = source.PropertySetter;
        }

        #region IDbPropertyMap<T,TProperty> Members

        public Action<T, TProperty> PropertySetter { get; private set; }

        public MemberInfo Member { get; private set; }

        public string ColumnName { get; private set; }

        public IDbColumnReader<TProperty> ColumnReader
        {
            get;
            private set;
        }

        Delegate IDbPropertyMap.PropertySetter
        {
            get
            {
                return PropertySetter;
            }
        }

        IDbColumnReader IDbPropertyMap.ColumnReader
        {
            get
            {
                return ColumnReader;
            }
        }
        
        #endregion
    }
}