using System;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace Sql.Native
{
    /// <summary>
    /// Base class for database command objects that use lambda expressions to bind SQL commands.
    /// </summary>
    /// <typeparam name="T">The type of the object containing the command methods.</typeparam>
    public class DbCommandObject<T>
    {
        #region Fields

        /// <summary>
        /// Create delegate method.
        /// </summary>
        private static readonly MethodInfo CreateDelegateMethod = typeof(Delegate).GetMethod("CreateDelegate",
            BindingFlags.Static | BindingFlags.Public, null, new[] {typeof(Type), typeof(Type), typeof(MethodInfo)}, null);

        /// <summary>
        /// The connection.
        /// </summary>
        private readonly IDbConnection connection;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DbCommandObject&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public DbCommandObject(IDbConnection connection)
        {
            this.connection = connection;
            this.CommandTimeout = 30;
        }

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <value>The connection.</value>
        protected IDbConnection Connection
        {
            get
            {
                return this.connection;
            }
        }

        public int CommandTimeout { get; set; }

        /// <summary>
        /// The execute scalar method.
        /// </summary>
        /// <typeparam name="TReturn">The return type.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns>A scalar of the TReturn type.</returns>
        protected internal TReturn ExecuteScalar<TReturn>(Expression<Func<T, Func<TReturn>>> expression)
        {
            return ExecuteScalar<TReturn>(ExtractMethodInfo(expression.Body));
        }

        /// <summary>
        /// The execute scalar method.
        /// </summary>
        /// <typeparam name="TArg1">The argument type.</typeparam>
        /// <typeparam name="TReturn">The return type.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <param name="arg1">The argument.</param>
        /// <returns>A scalar of the TReturn type.</returns>
        protected internal TReturn ExecuteScalar<TArg1, TReturn>(Expression<Func<T, Func<TArg1, TReturn>>> expression, TArg1 arg1)
        {
            return ExecuteScalar<TReturn>(ExtractMethodInfo(expression.Body), arg1);
        }

        /// <summary>
        /// The extract method info method.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The method info.</returns>
        private static MethodBase ExtractMethodInfo(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    return ExtractMethodInfo(((UnaryExpression)expression).Operand);
                case ExpressionType.Call:
                    var callExpression = (MethodCallExpression)expression;
                    if (!callExpression.Method.Equals(CreateDelegateMethod))
                    {
                        throw CreateInvalidExpressionException();
                    }

                    var methodInfoConstant = callExpression.Arguments[2] as ConstantExpression;
                    if (methodInfoConstant == null)
                    {
                        throw CreateInvalidExpressionException();
                    }

                    return (MethodInfo)methodInfoConstant.Value;
            }

            throw CreateInvalidExpressionException();
        }

        /// <summary>
        /// The create invalid expression exception method.
        /// </summary>
        /// <returns>The invalid expression exception.</returns>
        private static ArgumentException CreateInvalidExpressionException()
        {
            return new ArgumentException(@"Invalid expression", "expression");
        }

        /// <summary>
        /// The prepare command method.
        /// </summary>
        /// <param name="template">The mthod info.</param>
        /// <param name="args">The method arguments.</param>
        /// <returns>The db command.</returns>
        protected IDbCommand PrepareCommand(MethodBase template, object[] args)
        {
            var compiler = new DbMethod(template, args);
            return compiler.PrepareCommand(this.connection, args);
        }

        protected IDbCommand PrepareCommand(MethodBase template, string commandText, object[] args)
        {
            var compiler = new DbMethod(commandText, template, args);
            return compiler.PrepareCommand(this.connection, args);
        }

        protected IDbCommand PrepareCommand(string commandText, Type[] argTypes, object[] args)
        {
            var compiler = new DbMethod(commandText, argTypes, args);
            return compiler.PrepareCommand(this.connection, args);
        }

        /// <summary>
        /// The execute scalar method (two arguments).
        /// </summary>
        /// <typeparam name="TArg1">The first argument type.</typeparam>
        /// <typeparam name="TArg2">The second argument type.</typeparam>
        /// <typeparam name="TReturn">The return type.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <returns>A scalar of the TReturn type.</returns>
        protected internal TReturn ExecuteScalar<TArg1, TArg2, TReturn>(Expression<Func<T, Func<TArg1, TArg2, TReturn>>> expression,
            TArg1 arg1, TArg2 arg2)
        {
            return ExecuteScalar<TReturn>(ExtractMethodInfo(expression.Body), arg1, arg2);
        }

        /// <summary>
        /// The execute scalar method (two arguments).
        /// </summary>
        /// <typeparam name="TArg1">The first argument type.</typeparam>
        /// <typeparam name="TArg2">The second argument type.</typeparam>
        /// <typeparam name="TArg3">The third argument type.</typeparam>
        /// <typeparam name="TReturn">The return type.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="arg3">The third argument.</param>
        /// <returns>A scalar of the TReturn type.</returns>
        protected internal TReturn ExecuteScalar<TArg1, TArg2, TArg3, TReturn>(
            Expression<Func<T, Func<TArg1, TArg2, TArg3, TReturn>>> expression, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            return ExecuteScalar<TReturn>(ExtractMethodInfo(expression.Body), arg1, arg2, arg3);
        }

        /// <summary>
        /// The execute scalar method (four arguments).
        /// </summary>
        /// <typeparam name="TArg1">The first argument type.</typeparam>
        /// <typeparam name="TArg2">The second argument type.</typeparam>
        /// <typeparam name="TArg3">The third argument type.</typeparam>
        /// <typeparam name="TArg4">The fourth argument type.</typeparam>
        /// <typeparam name="TReturn">The return type.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="arg3">The third argument.</param>
        /// <param name="arg4">The fourth argument.</param>
        /// <returns>A scalar of the TReturn type.</returns>
        protected internal TReturn ExecuteScalar<TArg1, TArg2, TArg3, TArg4, TReturn>(
            Expression<Func<T, Func<TArg1, TArg2, TArg3, TArg4, TReturn>>> expression, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            return ExecuteScalar<TReturn>(ExtractMethodInfo(expression.Body), arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// The execute scalar method.
        /// </summary>
        /// <typeparam name="TReturn">The return type.</typeparam>
        /// <param name="methodInfo">The method info.</param>
        /// <param name="args">The arguments/</param>
        /// <returns>A scalar of the TReturn type.</returns>
        protected internal TReturn ExecuteScalar<TReturn>(MethodBase methodInfo, params object[] args)
        {
            return new DbMethod(methodInfo, args) { CommandTimeout = CommandTimeout }.ExecuteScalar<TReturn>(Connection, args);
        }

        /// <summary>
        /// The execute reader method
        /// </summary>
        /// <typeparam name="TReturn">The return type.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <param name="transform">The transform.</param>
        /// <returns>The reader results.</returns>
        protected TReturn ExecuteReader<TReturn>(Expression<Func<T, Func<TReturn>>> expression, Func<IDataReader, TReturn> transform)
        {
            return ExecuteReader(ExtractMethodInfo(expression.Body), transform);
        }

        /// <summary>
        /// The execute reader method
        /// </summary>
        /// <typeparam name="TArg1">The firts argument type</typeparam>
        /// <typeparam name="TReturn">The return type.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <param name="transform">The transform.</param>
        /// <param name="arg1">The first argument.</param>
        /// <returns>The reader results.</returns>
        protected TReturn ExecuteReader<TArg1, TReturn>(Expression<Func<T, Func<TArg1, TReturn>>> expression,
            Func<IDataReader, TReturn> transform, TArg1 arg1)
        {
            return ExecuteReader(ExtractMethodInfo(expression.Body), transform, arg1);
        }

        /// <summary>
        /// The execute reader method
        /// </summary>
        /// <typeparam name="TArg1">The firts argument type.</typeparam>
        /// <typeparam name="TArg2">The second argument type.</typeparam>
        /// <typeparam name="TReturn">The return type.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <param name="transform">The transform.</param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <returns>The reader results.</returns>
        protected TReturn ExecuteReader<TArg1, TArg2, TReturn>(Expression<Func<T, Func<TArg1, TArg2, TReturn>>> expression,
            Func<IDataReader, TReturn> transform, TArg1 arg1, TArg2 arg2)
        {
            return ExecuteReader(ExtractMethodInfo(expression.Body), transform, arg1, arg2);
        }

        /// <summary>
        /// The execute reader method
        /// </summary>
        /// <typeparam name="TArg1">The firts argument type</typeparam>
        /// <typeparam name="TArg2">The second argument type.</typeparam>
        /// <typeparam name="TArg3">The third argument type.</typeparam>
        /// <typeparam name="TReturn">The return type.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <param name="transform">The transform.</param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="arg3">The third argument.</param>
        /// <returns>The reader results.</returns>
        protected TReturn ExecuteReader<TArg1, TArg2, TArg3, TReturn>(Expression<Func<T, Func<TArg1, TArg2, TArg3, TReturn>>> expression,
            Func<IDataReader, TReturn> transform, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            return ExecuteReader(ExtractMethodInfo(expression.Body), transform, arg1, arg2, arg3);
        }

        /// <summary>
        /// Executes the reader.
        /// </summary>
        /// <typeparam name="TArg1">The type of the arg1.</typeparam>
        /// <typeparam name="TArg2">The type of the arg2.</typeparam>
        /// <typeparam name="TArg3">The type of the arg3.</typeparam>
        /// <typeparam name="TArg4">The type of the arg4.</typeparam>
        /// <typeparam name="TReturn">The type of the return.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <param name="transform">The transform.</param>
        /// <param name="arg1">The arg1 value.</param>
        /// <param name="arg2">The arg2 value.</param>
        /// <param name="arg3">The arg3 value.</param>
        /// <param name="arg4">The arg4 value.</param>
        /// <returns>The reader reasults.</returns>
        protected TReturn ExecuteReader<TArg1, TArg2, TArg3, TArg4, TReturn>(
            Expression<Func<T, Func<TArg1, TArg2, TArg3, TArg4, TReturn>>> expression, Func<IDataReader, TReturn> transform, TArg1 arg1,
            TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            return ExecuteReader(ExtractMethodInfo(expression.Body), transform, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// The execute reader method
        /// </summary>
        /// <typeparam name="TReturn">The return type.</typeparam>
        /// <param name="methodInfo">The method info.</param>
        /// <param name="transform">The transform.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>The reader results.</returns>
        protected TReturn ExecuteReader<TReturn>(MethodBase methodInfo, Func<IDataReader, TReturn> transform, params object[] args)
        {
            return new DbMethod(methodInfo, args) {CommandTimeout = CommandTimeout}.ExecuteReader(Connection, transform, args);
        }

        /// <summary>
        /// The execute no query method.
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected int ExecuteNonQuery(Expression<Func<T, Action>> expression)
        {
            return this.ExecuteNonQuery(ExtractMethodInfo(expression.Body));
        }

        /// <summary>
        /// The execute no query method.
        /// </summary>
        /// <typeparam name="TArg1">The first argument type.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <param name="arg1">The first argument.</param>
        protected int ExecuteNonQuery<TArg1>(Expression<Func<T, Action<TArg1>>> expression, TArg1 arg1)
        {
            return this.ExecuteNonQuery(ExtractMethodInfo(expression.Body), arg1);
        }

        /// <summary>
        /// The execute no query method.
        /// </summary>
        /// <typeparam name="TArg1">The first argument type.</typeparam>
        /// <typeparam name="TArg2">The second argument type.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        protected int ExecuteNonQuery<TArg1, TArg2>(Expression<Func<T, Action<TArg1, TArg2>>> expression, TArg1 arg1, TArg2 arg2)
        {
            return this.ExecuteNonQuery(ExtractMethodInfo(expression.Body), arg1, arg2);
        }

        /// <summary>
        /// The execute no query method.
        /// </summary>
        /// <typeparam name="TArg1">The first argument type.</typeparam>
        /// <typeparam name="TArg2">The second argument type.</typeparam>
        /// <typeparam name="TArg3">The third argument type.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="arg3">The third argument.</param>
        protected int ExecuteNonQuery<TArg1, TArg2, TArg3>(Expression<Func<T, Action<TArg1, TArg2, TArg3>>> expression, TArg1 arg1,
            TArg2 arg2, TArg3 arg3)
        {
            return this.ExecuteNonQuery(ExtractMethodInfo(expression.Body), arg1, arg2, arg3);
        }

        /// <summary>
        /// The execute no query method.
        /// </summary>
        /// <typeparam name="TArg1">The first argument type.</typeparam>
        /// <typeparam name="TArg2">The second argument type.</typeparam>
        /// <typeparam name="TArg3">The third argument type.</typeparam>
        /// <typeparam name="TArg4">The fourth argument type.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="arg3">The third argument.</param>
        /// <param name="arg4">The fourth argument.</param>
        protected int ExecuteNonQuery<TArg1, TArg2, TArg3, TArg4>(Expression<Func<T, Action<TArg1, TArg2, TArg3, TArg4>>> expression,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            return this.ExecuteNonQuery(ExtractMethodInfo(expression.Body), arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// The execute no query method.
        /// </summary>
        /// <param name="methodInfo">The method info.</param>
        /// <param name="args">The arguments.</param>
        protected int ExecuteNonQuery(MethodBase methodInfo, params object[] args)
        {
            return new DbMethod(methodInfo, args) {CommandTimeout = CommandTimeout}.ExecuteNonQuery(Connection, args);
        }
    }
}