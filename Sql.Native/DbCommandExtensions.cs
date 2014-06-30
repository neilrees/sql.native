using System;
using System.Data;

namespace Sql.Native
{
    /// <summary>
    /// Extension for <see cref="IDbCommand"/>
    /// </summary>
    public static class DbCommandExtensions
    {
        /// <summary>
        /// Adds the parameter.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="instance">The command instance.</param>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The paramter value.</param>
        /// <returns>The added parameter.</returns>
        public static IDbDataParameter AddParameter<T>(this IDbCommand instance, string name, T value)
        {
            var dbparameter = instance.CreateParameter();
            dbparameter.ParameterName = name;
            dbparameter.DbType = DbConverter.ToDbType(typeof(T));
            dbparameter.Direction = ParameterDirection.Input;
            dbparameter.Value = DbConverter.ToDbObject(value, dbparameter.DbType);
            instance.Parameters.Add(dbparameter);
            return dbparameter;
        }

        /// <summary>
        /// Executes the command using <see cref="IDbCommand.ExecuteReader()"/> and transforms the results.
        /// </summary>
        /// <typeparam name="TReturn">The type of the return.</typeparam>
        /// <param name="instance">The command instance.</param>
        /// <param name="transform">The transform function to use to read the results.</param>
        /// <returns>The transformed results.</returns>
        public static TReturn ExecuteReader<TReturn>(this IDbCommand instance, Func<IDataReader, TReturn> transform)
        {
            using (var reader = instance.ExecuteReader())
            {
                return transform(reader);
            }
        }
    }
}
