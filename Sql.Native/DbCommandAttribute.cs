using System;
using System.Reflection;

namespace Sql.Native
{
    /// <summary>
    /// Attribute used to specify command methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class DbCommandAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DbCommandAttribute"/> class.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        public DbCommandAttribute(string commandText)
        {
            this.CommandText = commandText;
        }

        /// <summary>
        /// Gets or sets the command text.
        /// </summary>
        /// <value>The command text.</value>
        public string CommandText { get; set; }

        /// <summary>
        /// Gets the <see cref="DbCommandAttribute"/> associated with the specified member.
        /// </summary>
        /// <param name="member">The member whose attributes should be searched.</param>
        /// <returns>The <see cref="DbCommandAttribute "/> associated with the member.</returns>
        public static DbCommandAttribute From(MemberInfo member)
        {
            return (DbCommandAttribute)GetCustomAttribute(member, typeof(DbCommandAttribute), false);
        }
    }
}