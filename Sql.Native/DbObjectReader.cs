using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Sql.Native
{
    public class DbObjectReader<T> : IDbObjectReader<T>
    {
        #region Fields

        private readonly IDbClassMap<T> map;
        private readonly DbObjectReaderOptions options;
        private IDataReader reader;
        private readonly IList<T> objects;

        #endregion

        public DbObjectReader(IDataReader reader, IDbClassMap<T> map, DbObjectReaderOptions options = DbObjectReaderOptions.DisposeReader)
        {
            // initialize
            this.reader = reader;
            this.map = map;
            this.options = options;

            // read straight away to reduce risk of leaking reader resources
            this.objects = ReadObjects().ToList();
        }
        
        #region IDisposable Members

        public void Dispose()
        {
            CleanupReader();
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return this.objects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        private IEnumerable<T> ReadObjects()
        {
            if (reader.Read())
            {
                var objectReader = DbMapperFactory.GetObjectReader(reader, map);
                do
                {
                    var instance = map.EntityFactory(reader);
                    objectReader(instance, reader);
                    yield return instance;
                } while (reader.Read());
            }
            CleanupReader();
        }

        private void CleanupReader()
        {
            if (reader != null)
            {
                if (options.HasFlag(DbObjectReaderOptions.DisposeReader))
                {
                    reader.Dispose();
                }
                reader = null;
            }
        }
    }
}