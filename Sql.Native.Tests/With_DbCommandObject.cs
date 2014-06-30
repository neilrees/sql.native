using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sql.Native.Tests
{
    [TestClass]
    public class With_DbCommandObject
    {
        [TestMethod]
        public void Should_bind_single_complex_parameter()
        {
            var testDbObject = new TestDbObject
                {
                    Property1Parameter = 1,
                    Property2Parameter = Guid.NewGuid()
                };

            var command = new TestDbCommandObject(
                new SqlConnection()).PrepareWithSingleComplexParameter(testDbObject);

            // verify parameter bindings
            Assert.AreEqual(2, command.Parameters.Count);
            AssertParameter(command.Parameters[0], testDbObject.Property1Parameter, DbType.Int32);
            AssertParameter(command.Parameters[1], testDbObject.Property2Parameter, DbType.Guid);
        }

        [TestMethod]
        public void Should_bind_multiple_complex_parameters()
        {
            var testDbObject1 = new TestDbObject
                {
                    Property1Parameter = 1,
                    Property2Parameter = Guid.NewGuid()
                };

            var testDbObject2 = new TestDbObject
                {
                    Property1Parameter = 2,
                    Property2Parameter = Guid.NewGuid()
                };

            var command = new TestDbCommandObject(
                new SqlConnection()).PrepareWithMultipleComplexParameters(testDbObject1, testDbObject2);

            // verify parameter bindings
            Assert.AreEqual(2, command.Parameters.Count);
            AssertParameter(command.Parameters[0], testDbObject1.Property1Parameter, DbType.Int32);
            AssertParameter(command.Parameters[1], testDbObject2.Property2Parameter, DbType.Guid);
        }

        [TestMethod]
        public void Should_bind_multiple_complex_and_non_complex_parameters()
        {
            var testDbObject1 = new TestDbObject
                {
                    Property1Parameter = 1,
                    Property2Parameter = Guid.NewGuid()
                };

            var testDbObject2 = new TestDbObject
                {
                    Property1Parameter = 2,
                    Property2Parameter = Guid.NewGuid()
                };

            var command = new TestDbCommandObject(
                new SqlConnection()).PrepareWithMultipleComplexAndNonComplexParameters(testDbObject1, "Test",
                                                                                       testDbObject2);

            // verify parameter bindings
            Assert.AreEqual(3, command.Parameters.Count);
            AssertParameter(command.Parameters[0], testDbObject1.Property1Parameter, DbType.Int32);
            AssertParameter(command.Parameters[1], "Test", DbType.String);
            AssertParameter(command.Parameters[2], testDbObject2.Property2Parameter, DbType.Guid);
        }

        private void AssertParameter(object parameter, object value, DbType dbType)
        {
            var dbParameter = (IDataParameter) parameter;
            Assert.AreEqual(value, dbParameter.Value);
            Assert.AreEqual(dbType, dbParameter.DbType);
        }

        [TestMethod]
        public void Should_ignore_parameters_in_strings()
        {
            Assert.AreEqual(0, DbParameterParser.ParseParameters("'''@InString2  '").Count());
        }

        [TestMethod]
        public void Should_parse_invalid_expression()
        {
            var actual = DbParameterParser.ParseParameters("@Param1 '@InString1''").ToArray();
            CollectionAssert.AreEquivalent(new[] {"Param1"}, actual);
        }

        [TestMethod]
        public void Should_parse_complex_expression()
        {
            var actual =
                DbParameterParser.ParseParameters("@Param1 '' '@InString1' '''@InString2  '' @Instring3' @Param2")
                                 .ToArray();
            CollectionAssert.AreEquivalent(new[] {"Param1", "Param2"}, actual);
        }

        #region Nested type: TestDbCommandObject

        public class TestDbCommandObject : DbCommandObject<TestDbCommandObject>
        {
            public TestDbCommandObject(IDbConnection connection)
                : base(connection)
            {
            }

            [DbCommand("INSERT INTO TEST VALUES (@value_Property1Parameter, @value_Property2Parameter)")]
            public IDbCommand PrepareWithSingleComplexParameter(TestDbObject value)
            {
                return this.PrepareCommand(MethodBase.GetCurrentMethod(), new object[] {value});
            }

            [DbCommand("INSERT INTO TEST VALUES (@value1_Property1Parameter, @value2_Property2Parameter)")]
            public IDbCommand PrepareWithMultipleComplexParameters(TestDbObject value1, TestDbObject value2)
            {
                return this.PrepareCommand(MethodBase.GetCurrentMethod(), new object[] {value1, value2});
            }

            [DbCommand("INSERT INTO TEST VALUES (@value1_Property1Parameter, @stringValue, @value2_Property2Parameter)")
            ]
            public IDbCommand PrepareWithMultipleComplexAndNonComplexParameters(TestDbObject value1, string stringValue,
                                                                                TestDbObject value2)
            {
                return this.PrepareCommand(MethodBase.GetCurrentMethod(), new object[] {value1, stringValue, value2});
            }

            [DbCommand("INSERT INTO TEST VALUES (@value1, @value2)")]
            public IDbCommand PrepareWithParameters(int value1, int value2)
            {
                return this.PrepareCommand(MethodBase.GetCurrentMethod(), new object[] {value1, value2});
            }
        }

        #endregion

        #region Nested type: TestDbObject

        public class TestDbObject
        {
            public int Property1Parameter { get; set; }
            public Guid Property2Parameter { get; set; }
        }

        #endregion
    }
}