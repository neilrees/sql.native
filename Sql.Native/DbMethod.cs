using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Sql.Native
{
    public class DbMethod
    {
        public DbMethod(string commandText) : this(commandText, new DbMethodArg[0])
        {
        }
        
        public DbMethod(MethodBase template, IList<object> argValues)
            : this(GetCommandText(template), template, argValues)
        {
        }

        public DbMethod(string commandText, MethodBase template, IList<object> argValues)
        {
            var parameters = template.GetParameters();
            if (parameters == null || parameters.Length != argValues.Count)
            {
                throw new ArgumentException(@"Invalid number of arguments passed.", "argValues");
            }

            Initialize(commandText, parameters.Select((x, i) => 
                new DbMethodArg(parameters[i].Name, parameters[i].ParameterType, argValues[i],
                    parameters[i].IsOut ? ParameterDirection.Output : ParameterDirection.Input)));
        }

        public DbMethod(string commandText, IEnumerable<DbMethodArg> args)
        {
            Initialize(commandText, args);
        }

        public DbMethod(string commandText, string[] argNames, Type[] argTypes, object[] argValues)
        {
            argTypes = argTypes ?? Type.EmptyTypes;
            if (argTypes.Length != (argValues == null ? 0 : argValues.Length) ||
                argTypes.Length != (argNames == null ? 0 : argNames.Length))
            {
                throw new ArgumentException(@"Argument array lengths are not equal.", "argTypes");
            }

            Initialize(commandText,
                argTypes.Select((x, i) => new DbMethodArg(argNames[i], argTypes[i], argValues[i], ParameterDirection.Input)));
        }

        public DbMethod(string commandText, Type[] argTypes, object[] argValues)
        {
            argTypes = argTypes ?? Type.EmptyTypes;
            if (argTypes.Length != (argValues == null ? 0 : argValues.Length))
            {
                throw new ArgumentException(@"Argument array lengths are not equal.", "argTypes");
            }

            Initialize(commandText, argTypes.Select((x, i) =>
                {
                    var argName = "arg" + (i + 1).ToString(CultureInfo.InvariantCulture);
                    return new DbMethodArg(argName, argTypes[i], argValues[i], ParameterDirection.Input);
                }));
        }

        protected HashSet<string> ParameterMap { get; set; }

        public ReadOnlyCollection<string> Parameters { get; private set; }

        public string CommandText { get; private set; }

        public int CommandTimeout { get; set; }

        public ReadOnlyCollection<DbMethodArg> Args { get; private set; }

        /// <summary>
        /// The get command text method.
        /// </summary>
        /// <param name="methodInfo">The method info.</param>
        /// <returns>The command text.</returns>
        protected static string GetCommandText(MemberInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException("methodInfo");
            }

            var commandAttribute = DbCommandAttribute.From(methodInfo);
            if (commandAttribute == null)
            {
                // TODO: Determine exception type and message
                throw new Exception();
            }

            return commandAttribute.CommandText;
        }

        private void Initialize(string commandText, IEnumerable<DbMethodArg> args)
        {
            this.CommandText = commandText;
            this.Args = new ReadOnlyCollection<DbMethodArg>(args.ToArray());
            this.Parameters = new ReadOnlyCollection<string>(
                DbParameterParser.ParseParameters(CommandText).Distinct().ToArray());
            this.ParameterMap = new HashSet<string>();
            foreach (var parameter in Parameters)
            {
                this.ParameterMap.Add(parameter);
            }
        }

        public IDbCommand PrepareCommand(IDbConnection connection, params object[] args)
        {
            UpdateArgs(args);

            var command = connection.CreateCommand();
            command.CommandText = CommandText;
            command.CommandTimeout = this.CommandTimeout;
            foreach (var arg in Args)
            {
                if (arg.IsComplexType)
                {
                    AddComplexParameters(command, arg);
                }
                else
                {
                    var dbParameter = CreateParameter(command, arg);
                    command.Parameters.Add(dbParameter);
                }
            }

            return command;
        }

        private static IDbDataParameter CreateParameter(IDbCommand command, DbMethodArg methodArg)
        {
            var dbparameter = command.CreateParameter();
            dbparameter.ParameterName = "@" + methodArg.Name;
            dbparameter.DbType = DbConverter.ToDbType(methodArg.Type);
            dbparameter.Direction = methodArg.Direction;
            dbparameter.Value = DbConverter.ToDbObject(methodArg.Value, dbparameter.DbType);
            return dbparameter;
        }

        private void AddComplexParameters(IDbCommand command, DbMethodArg methodArg)
        {
            // use actual object type if we haven't been more specific
            var type = methodArg.Type;
            if (type == typeof(object))
            {
                type = methodArg.Value.GetType();
            }

            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in properties)
            {
                var parameterName = methodArg.Name + "_" + property.Name;
                var found = HasParameter(parameterName);
                if (!found && Args.Count == 1)
                {
                    parameterName = property.Name;
                    found = HasParameter(parameterName);
                }

                if (found)
                {
                    var dbParameter = CreateParameter(command,
                        new DbMethodArg(parameterName, property.PropertyType, property.GetValue(methodArg.Value, null)));
                    command.Parameters.Add(dbParameter);
                }
            }
        }

        private bool HasParameter(string parameterName)
        {
            return ParameterMap.Contains(parameterName);
        }

        public TReturn ExecuteReader<TReturn>(IDbConnection connection, Func<IDataReader, TReturn> transform, object[] args = null)
        {
            using (var command = this.PrepareCommand(connection, args))
            {
                using (var reader = new UtcDateDataReader(command.ExecuteReader()))
                {
                    UpdateOutputParameters(command, args);
                    return transform(reader);
                }
            }
        }

        public int ExecuteNonQuery(IDbConnection connection, object[] args = null)
        {
            using (var command = this.PrepareCommand(connection, args))
            {
                var result = command.ExecuteNonQuery();
                UpdateOutputParameters(command, args);
                return result;
            }
        }

        public TReturn ExecuteScalar<TReturn>(IDbConnection connection, object[] args = null)
        {
            var command = this.PrepareCommand(connection, args);
            var result = FromDbValue<TReturn>(command.ExecuteScalar());
            UpdateOutputParameters(command, args);
            return result;
        }

        private void UpdateArgs(object[] args)
        {
            if (args != null)
            {
                if (args.Length != Args.Count)
                {
                    throw new ArgumentException();
                }
                
                for (var i = 0; i < Args.Count; i++)
                {
                    Args[i].Value = args[i];
                }
            }
        }

        /// <summary>
        /// The bind out parameters method.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="args"></param>
        private void UpdateOutputParameters(IDbCommand command, object[] args)
        {
            for (var i = 0; i < Args.Count; i++)
            {
                var arg = Args[i];
                if (arg.Direction.HasFlag(ParameterDirection.Output))
                {
                    var dataParameter = command.Parameters[i] as IDataParameter;
                    if (dataParameter != null)
                    {
                        arg.Value = FromDbValue(dataParameter.Value, arg.Type);
                    }
                }
            }
            
            if (args != null)
            {
                CopyOutputArgsTo(args);
            }
        }

        /// <summary>
        /// Converts the specified database provided value to the specified type.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="type">The type to convert to.</param>
        /// <returns>The converted value.</returns>
        private static object FromDbValue(object value, Type type)
        {
            if (type.IsClass && type.HasElementType)
            {
                type = type.GetElementType();
            }

            if (value != null && type.IsInstanceOfType(value))
            {
                return value;
            }

            if ((value == null || value is DBNull) && (type.IsClass || Nullable.GetUnderlyingType(type) != null))
            {
                return null;
            }

            // User converter to ensure other types are coerced correctly
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Empty:
                    return value;
                case TypeCode.Boolean:
                    return Convert.ToBoolean(value);
                case TypeCode.Char:
                    return Convert.ToChar(value);
                case TypeCode.SByte:
                    return Convert.ToSByte(value);
                case TypeCode.Byte:
                    return Convert.ToByte(value);
                case TypeCode.Int16:
                    return Convert.ToInt16(value);
                case TypeCode.UInt16:
                    return Convert.ToUInt16(value);
                case TypeCode.Int32:
                    return Convert.ToInt32(value);
                case TypeCode.UInt32:
                    return Convert.ToUInt32(value);
                case TypeCode.Int64:
                    return Convert.ToInt64(value);
                case TypeCode.UInt64:
                    return Convert.ToUInt64(value);
                case TypeCode.Single:
                    return Convert.ToSingle(value);
                case TypeCode.Double:
                    return Convert.ToDouble(value);
                case TypeCode.Decimal:
                    return Convert.ToDecimal(value);
                case TypeCode.DateTime:
                    return Convert.ToDateTime(value);
                case TypeCode.String:
                    return Convert.ToString(value);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// The from db value method.
        /// </summary>
        /// <typeparam name="TReturn">The return type.</typeparam>
        /// <param name="value">The value/</param>
        /// <returns>The converted value.</returns>
        private static TReturn FromDbValue<TReturn>(object value)
        {
            return (TReturn)FromDbValue(value, typeof(TReturn));
        }

        public void CopyOutputArgsTo(object[] args)
        {
            if (args.Length != Args.Count)
            {
                throw new ArgumentException();
            }
            for (var i = 0;i<Args.Count; i++)
            {
                if (Args[i].Direction .HasFlag( ParameterDirection.Output))
                {
                    args[i] = Args[i].Value;
                }
            }
        }
    }
}