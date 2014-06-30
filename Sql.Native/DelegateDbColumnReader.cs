using System;
using System.Data;

namespace Sql.Native
{
    public class DelegateDbColumnReader<TProperty> : IDbColumnReader<TProperty>
    {
        #region Fields
        
        private readonly Func<IDataRecord, int, TProperty> readMethod;

        #endregion

        public DelegateDbColumnReader(Func<IDataRecord, int, TProperty> readMethod)
        {
            this.readMethod = readMethod;
        }

        #region IDbColumnReader<TProperty> Members

        public TProperty Read(IDataRecord reader, int index)
        {
            return readMethod(reader, index);
        }

        object IDbColumnReader.Read(IDataRecord reader, int index)
        {
            return Read(reader, index);
        }

        #endregion
    }
}