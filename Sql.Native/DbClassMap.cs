using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace Sql.Native
{
    public class DbClassMap<T> : IDbClassMap<T>
    {
        public DbClassMap(IDbClassMap<T> classMap, IEnumerable<IDbPropertyMap> propertyMaps)
        {
            this.Type = classMap.Type;
            this.Properties = new ReadOnlyCollection<IDbPropertyMap>(propertyMaps.ToArray());
            this.EntityFactory = classMap.EntityFactory;
        }

        public Type Type { get; private set; }

        public ReadOnlyCollection<IDbPropertyMap> Properties
        {
            get;
            private set;
        }

        public static readonly DbClassMap<T> Default = CreateDefaultClassMap();

        private static DbClassMap<T> CreateDefaultClassMap()
        {
            var builder = new DbClassMapBuilder<T>();
            foreach (var property in typeof(T).GetProperties())
            {
                builder.Property(property);
            }
            return (DbClassMap<T>)builder.CreateMap();
        }

        public Func<IDataRecord, T> EntityFactory { get; private set; }
    }
}