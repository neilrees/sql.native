using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Sql.Native
{
    public static class DbMapperFactory
    {
        public static Action<T, IDataReader> GetObjectReader<T>(IDataReader reader, IDbClassMap<T> map)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            if (reader.IsClosed)
            {
                throw new InvalidOperationException();
            }

            return GetObjectReaderCore(reader, map);
        }

        private static Action<T, IDataReader> GetObjectReaderCore<T>(IDataReader reader, IDbClassMap<T> map)
        {
            var elementReaders = new List<Action<T, IDataReader>>();

            var columnNames = GetColumnNames(reader).ToList();
            var columnNameMap = columnNames.ToDictionary(x => x,
                StringComparer.OrdinalIgnoreCase);
            var propertiesMap = map.Properties.Where(x => columnNameMap.ContainsKey(x.ColumnName))
                .ToDictionary(x => x.ColumnName, StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < columnNames.Count; i++)
            {
                IDbPropertyMap property;
                if (propertiesMap.TryGetValue(columnNames[i], out property))
                {
                    var columnIndex = i;

                    // get property reader
                    var propertyReader = DbMapperFactory.GetPropertyMapper<T>(
                        property, reader.GetFieldType(columnIndex));

                    // wrap with column mapped reader
                    Action<T, IDataReader> elementReader = (x, r) => propertyReader(x, r, columnIndex);
                    elementReaders.Add(elementReader);
                }
            }

            Action<T, IDataReader> result = (x, r) =>
                {
                    foreach (var elementReader in elementReaders)
                    {
                        elementReader(x, r);
                    }
                };
            return result;
        }

        private static IEnumerable<String> GetColumnNames(IDataReader reader)
        {
            for (var i = 0; i < reader.FieldCount; i ++)
            {
                yield return reader.GetName(i);
            }
        }

        private class ExpressionRewriter : ExpressionVisitor
        {
            private readonly ParameterExpression readerExpression;
            private readonly ParameterExpression indexExpression;

            public ExpressionRewriter(ParameterExpression readerExpression, ParameterExpression indexExpression)
            {
                this.readerExpression = readerExpression;
                this.indexExpression = indexExpression;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (node.Type.Equals(typeof(IDataRecord)))
                {
                    return readerExpression;
                }
                if (node.Type.Equals(typeof(int)))
                {
                    return indexExpression;
                }
                return node;
            }
        }
        
        public static Action<T, IDataReader, int> GetPropertyMapper<T>(IDbPropertyMap property, Type defaultReaderType)
        {
            // define parameters (T target, IDataRecord record, int index)
            var targetParam = Expression.Parameter(typeof(T), "target");
            var readerParam = Expression.Parameter(typeof(IDataRecord), "record");
            var indexParam = Expression.Parameter(typeof(int), "index");

            var propertyInfo = (PropertyInfo)property.Member;
            Expression columnReader = null;
            if (property.ColumnReader != null)
            {
                columnReader = Expression.Call(Expression.Constant(property.ColumnReader, typeof(IDbColumnReader<>)
                    .MakeGenericType(propertyInfo.PropertyType)), "Read", new Type[0], readerParam, indexParam);
            }
            else
            {
                columnReader = GetPropertyReader(defaultReaderType, propertyInfo.PropertyType, readerParam, indexParam);
            }

            if (property.PropertySetter == null)
            {
                return Expression.Lambda<Action<T, IDataRecord, int>>(
                    Expression.Call(targetParam, propertyInfo.GetSetMethod() ?? propertyInfo.GetSetMethod(true), new[] {columnReader}),
                    targetParam, readerParam, indexParam).Compile();
            }

            return Expression.Lambda<Action<T, IDataRecord, int>>(
                Expression.Call(property.PropertySetter.Method.IsStatic 
                    ? null 
                    : Expression.Constant(property.PropertySetter.Target), 
                    property.PropertySetter.Method, new[] { targetParam, columnReader }),
                targetParam, readerParam, indexParam).Compile();
            
        }

        private static Expression GetPropertyReader(Type sourceType, Type targetType, ParameterExpression readerParameter, 
            ParameterExpression indexParameter)
        {
            var defaultDBNull = Expression.Default(targetType);
            var convertIfRequired = ConvertIfRequired(
                Expression.Call(
                    readerParameter, DataRecordAccessors.GetGetMethod(sourceType), new[] { indexParameter }),
                targetType);
            return Expression.Condition(
                Expression.Call(
                    readerParameter, DataRecordAccessors.IsDBNull, new[] { indexParameter }),
                defaultDBNull,
                convertIfRequired);
        }
        
        private static Expression ConvertIfRequired(Expression expression, Type type)
        {
            return type.IsClass && type.IsAssignableFrom(expression.Type) ? expression : Expression.Convert(expression, type);

        }
    }
}