using System;
using System.Reflection;

namespace Sql.Native
{
    public class DbPropertyMapBuilder<T, TProperty> : IDbPropertyMapBuilder<T, TProperty>, IDbPropertyMapBuilder
    {
        private string columnName;

        public DbPropertyMapBuilder(MemberInfo member)
        {
            this.Member = member;
        }

        #region IDbPropertyMapBuilder Members

        IDbPropertyMapBuilder IDbPropertyMapBuilder.HasColumnReader(IDbColumnReader columnReader)
        {
            return (IDbPropertyMapBuilder)HasColumnReader((IDbColumnReader<TProperty>)columnReader);
        }

        IDbPropertyMapBuilder IDbPropertyMapBuilder.HasColumnName(string columnName)
        {
            return (IDbPropertyMapBuilder)HasColumnName(columnName);
        }

        
        IDbPropertyMap IDbPropertyMapBuilder.CreateMap()
        {
            return CreateMap();
        }

        void IDbPropertyMapBuilder.HasPropertySetter(Delegate propertySetter)
        {
            HasPropertySetter((Action<T, TProperty>)propertySetter);
        }

        #endregion

        #region IDbPropertyMapBuilder<T,TProperty> Members

        IDbColumnReader IDbPropertyMap.ColumnReader
        {
            get
            {
                return ColumnReader;
            }
        }

        Delegate IDbPropertyMap.PropertySetter
        {
            get
            {
                return PropertySetter;
            }
        }

        public MemberInfo Member { get; private set; }

        public string ColumnName
        {
            get
            {
                return this.columnName ?? Member.Name;
            }
            private set
            {
                this.columnName = this.Member.Name == value ? null : value;
            }
        }

        public IDbColumnReader<TProperty> ColumnReader { get; protected set; }

        public IDbPropertyMapBuilder<T, TProperty> HasColumnName(string columnName)
        {
            this.ColumnName = columnName;
            return this;
        }

        public IDbPropertyMapBuilder<T, TProperty> HasColumnReader(IDbColumnReader<TProperty> columnReader)
        {
            this.ColumnReader = columnReader;
            return this;
        }

        public IDbPropertyMapBuilder<T, TProperty> HasPropertySetter(Action<T, TProperty> propertySetter)
        {
            this.PropertySetter = propertySetter;
            return this;
        }

        public Action<T, TProperty> PropertySetter { get; private set; }

        public IDbPropertyMap<T, TProperty> CreateMap()
        {
            return new DbPropertyMap<T, TProperty>(this);
        }

        #endregion
    }
}