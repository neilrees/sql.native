using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Sql.Native
{
    public static class TypeRevealer<T>
    {
        public static MethodInfo Method(Expression<Action<T>> expression)
        {
            var callexpression = expression.Body as MethodCallExpression;
            if (callexpression != null)
            {
                return callexpression.Method;
            }
            throw new ArgumentException();
        }
    }
}