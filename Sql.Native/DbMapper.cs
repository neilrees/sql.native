using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Sql.Native
{
    public class DbClassMapBuilder<T> : IDbClassMap<T>
    {
        #region Fields

        private readonly Dictionary<MemberInfo, object> columnMapBuilderMap = new Dictionary<MemberInfo, object>();
        private readonly List<IDbPropertyMap> properties = new List<IDbPropertyMap>();
        private ReadOnlyCollection<IDbPropertyMap> readerOnlyProperties;
        private Func<IDataRecord, T> entityFactory;

        #endregion

        public DbClassMapBuilder()
        {
            
        }

        public DbClassMapBuilder(IDbClassMap<T> source)
        {
            foreach (var property in source.Properties)
            {
                Property((PropertyInfo)property.Member)
                    .HasColumnName(property.ColumnName)
                    .HasColumnReader(property.ColumnReader)
                    .HasPropertySetter(property.PropertySetter);
            }
        }

        public IDbPropertyMapBuilder Property(PropertyInfo propertyInfo)
        {
            var method = TypeRevealer<DbClassMapBuilder<T>>.Method(x => x.GetOrCreatePropertyMapBuilder<int>(null));
            return (IDbPropertyMapBuilder)method.GetGenericMethodDefinition().MakeGenericMethod(propertyInfo.PropertyType).Invoke(
                this, new object[] {propertyInfo});
        }
        
        public Func<IDataRecord, T> EntityFactory
        {
            get
            {
                if (this.entityFactory == null)
                {
                    var dataRecordParameter = Expression.Parameter(typeof(IDataRecord));
                    var constructorInfo = typeof(T).GetConstructor(Type.EmptyTypes) ?? typeof(T).GetConstructor(
                        BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
                    if (constructorInfo == null)
                    {
                        throw new InvalidOperationException("Type must define a default constructor.");
                    }
                    this.entityFactory = Expression.Lambda<Func<IDataRecord, T>>(
                        Expression.New(constructorInfo), dataRecordParameter).Compile();
                }
                return this.entityFactory;
            }
        }
        
        public IDbPropertyMapBuilder<T, TProperty> Property<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            return GetOrCreatePropertyMapBuilder<TProperty>(GetMemberExpression(expression.Body).Member);
        }

        public IDbClassMap<T> CreateMap()
        {
            return new DbClassMap<T>(this, 
                this.Properties.OfType<IDbPropertyMapBuilder>().Select(x => x.CreateMap()));
        }

        private DbPropertyMapBuilder<T, TProperty> GetOrCreatePropertyMapBuilder<TProperty>(MemberInfo member)
        {
            var propertyBuilder = GetPropertyMapBuilder<TProperty>(member);
            if (propertyBuilder == null)
            {
                propertyBuilder = new DbPropertyMapBuilder<T, TProperty>(member);
                this.columnMapBuilderMap.Add(member, propertyBuilder);
                this.properties.Add(propertyBuilder);
            }
            return propertyBuilder;
        }

        private DbPropertyMapBuilder<T, TProperty> GetPropertyMapBuilder<TProperty>(MemberInfo member)
        {
            object builder;
            this.columnMapBuilderMap.TryGetValue(member, out builder);
            return builder as DbPropertyMapBuilder<T, TProperty>;
        }

        private static MemberExpression GetMemberExpression(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Convert)
            {
                expression = ((UnaryExpression)expression).Operand;
                return GetMemberExpression(expression);
            }
            var memberExpression = expression as MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentException();
            }
            var parameterExpression = memberExpression.Expression as ParameterExpression;
            if (parameterExpression == null)
            {
                throw new ArgumentException();
            }
            if (!parameterExpression.Type.Equals(typeof(T)))
            {
                throw new ArgumentException();
            }
            return memberExpression;
        }
        
        public Type Type
        {
            get
            {
                return typeof(T);
            }
        }

        public ReadOnlyCollection<IDbPropertyMap> Properties
        {
            get
            {
                return readerOnlyProperties ?? (readerOnlyProperties = new ReadOnlyCollection<IDbPropertyMap>(properties));
            }
        }
    }
}
