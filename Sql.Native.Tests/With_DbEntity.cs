using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sql.Native.Tests
{
    [TestClass]
    public class With_DbEntity
    {
        #region TestEntityEnum enum

        public enum TestEntityEnum
        {
            None,
            One,
            Two,
            Three
        }

        #endregion

        protected IDbClassMap<TestEntity> DefaultMap { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            this.Connection = new SqlConnection("Data Source=(LocalDb)\v11.0;Initial Catalog=tempdb;Integrated Security=SSPI;");
            this.Connection.Open();

            var mapper = new DbClassMapBuilder<TestEntity>();
            mapper.Property(x => x.Int32Value);
            mapper.Property(x => x.StringValue);
            mapper.Property(x => x.NullableInt32Value);
            mapper.Property(x => x.NullableDoubleValue);
            mapper.Property(x => x.IntToEnumValue);
            mapper.Property(x => x.StringToEnumValue)
                  .HasColumnReader(DbColumnReader.StringToEnum<TestEntityEnum>());
            mapper.Property(x => x.ParsedProperty)
                  .HasColumnReader((x, i) => (x.GetStringOrDefault(i) ?? string.Empty).Split(','));
            mapper.Property(x => x.DefaultNameValue);
            DefaultMap = mapper.CreateMap();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Connection.Close();
        }

        public IDbConnection Connection { get; set; }

        [TestMethod]
        public void Should_read_int()
        {
            var result = Connection.Entity(this.DefaultMap).SqlQuery("SELECT @arg1 AS Int32Value ", 1)
                                   .SingleOrDefault(x => x.Int32Value == 1);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Should_read_int_as_default_when_dbull()
        {
            var result = Connection.Entity(this.DefaultMap).SqlQuery("SELECT CAST(NULL AS INT) AS Int32Value ")
                                   .SingleOrDefault(x => x.Int32WriteCount == 1 && x.Int32Value == 0);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Should_read_string()
        {
            var result = Connection.Entity(this.DefaultMap).SqlQuery("SELECT @arg1 AS StringValue", "Test")
                                   .SingleOrDefault(x => x.StringWriteCount == 1 && x.StringValue == "Test");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Should_read_nullable_int_as_null_when_dbnull()
        {
            var result = Connection.Entity(this.DefaultMap).SqlQuery("SELECT CAST(NULL AS INT) AS NullableInt32Value")
                                   .SingleOrDefault(x => x.NullableInt32WriteCount == 1 && x.NullableInt32Value == null);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Should_read_nullable_int()
        {
            var result = Connection.Entity(this.DefaultMap).SqlQuery("SELECT 1 AS NullableInt32Value")
                                   .SingleOrDefault(x => x.NullableInt32WriteCount == 1 && x.NullableInt32Value == 1);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Should_read_using_default_column_name()
        {
            // using default mapper (notice the need for As NullableInt32Value)
            var result = Connection.Entity<TestEntity>().SqlQuery("SELECT @arg1 AS StringValue", "value")
                                   .SingleOrDefault(x => x.StringValue == "value");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Should_read_enum_from_string()
        {
            // get enum mapped from string
            var result = Connection.Entity(DefaultMap).SqlQuery("SELECT 'Two' as StringToEnumValue")
                                   .SingleOrDefault(x => x.StringToEnumValue == TestEntityEnum.Two);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Should_read_using_custom_property_reader()
        {
            var result = Connection.Entity(DefaultMap)
                                   .SqlQuery("SELECT 'One,Two,Three' as ParsedProperty").SingleOrDefault();
            Assert.IsNotNull(result);
            CollectionAssert.AreEqual(new[] {"One", "Two", "Three"}, result.ParsedProperty);
        }

        [TestMethod]
        public void Should_use_default_name()
        {
            var result = Connection.Entity(DefaultMap).SqlQuery("SELECT 'value' AS defaultnamevalue")
                                   .SingleOrDefault(x => x.DefaultNameValue == "value");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Should_read_nullable_double_from_decimal()
        {
            var result = Connection.Entity(DefaultMap)
                                   .SqlQuery("SELECT CAST(101.1 AS DECIMAL(12,1)) AS NullableDoubleValue")
                                   .SingleOrDefault(
                                       x => x.NullableDoubleWriteCount == 1 && x.NullableDoubleValue == 101.1);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Should_read_nullable_double_from_decimal_as_null_when_dbnull()
        {
            var result = Connection.Entity(DefaultMap)
                                   .SqlQuery("SELECT CAST(NULL AS DECIMAL(12,1)) AS NullableDoubleValue")
                                   .SingleOrDefault(
                                       x => x.NullableDoubleWriteCount == 1 && x.NullableDoubleValue == null);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Should_be_selectable()
        {
            // not really a test, just here to demonstrate how you can use these entities to efficiently select data
            var results = Connection.Entity(DefaultMap)
                                    .SqlQuery(
                                        "SELECT @arg1 AS Int32Value, 'Test 1' AS StringValue UNION SELECT @arg2 AS Int32Value, 'Test 2' AS StringValue",
                                        1, 2).Select(
                                            x => new
                                                {
                                                    Id = x.Int32Value,
                                                    Value = x.StringValue
                                                }).ToArray();

            Assert.AreEqual(1, results[0].Id);
            Assert.AreEqual("Test 1", results[0].Value);
            Assert.AreEqual(2, results[1].Id);
            Assert.AreEqual("Test 2", results[1].Value);
        }

        [TestMethod]
        public void Should_be_selectable_using_complex_arg()
        {
            // not really a test, just here to demonstrate how you can use these entities to efficiently select data
            var results = Connection.Entity(DefaultMap)
                                    .SqlQuery("SELECT @Id AS Int32Value, @Value AS StringValue",
                                              new {Id = 1, Value = "Hello World"}).Select(
                                                  x => new
                                                      {
                                                          Id = x.Int32Value,
                                                          Value = x.StringValue
                                                      }).ToArray();

            Assert.AreEqual(1, results[0].Id);
            Assert.AreEqual("Hello World", results[0].Value);
        }

        [TestMethod]
        public void Should_allow_complex_content()
        {
            var mapBuilder = new DbClassMapBuilder<TestEntity>();
            mapBuilder.Property(x => x.Int32Value)
                      .HasColumnName("Id");
            mapBuilder.Property(x => x.StringValue)
                      .HasColumnName("Value");
            var map = mapBuilder.CreateMap();

            const string SQL = @"
                DECLARE @TestTable TABLE (Id INT, Value NVARCHAR(50))
                INSERT INTO @TestTable VALUES (@InputId, @InputValue)
                SELECT * FROM @TestTable";
            var results = Connection.ExecuteReader(SQL, r => new DbObjectReader<TestEntity>(r, map), new
                {
                    InputId = 1,
                    InputValue = "Hello World"
                }).ToArray();
            Assert.AreEqual(1, results[0].Int32Value);
            Assert.AreEqual("Hello World", results[0].StringValue);
        }

        [TestMethod]
        public void Should_allow_static_custom_setter()
        {
            var mapBuilder = new DbClassMapBuilder<TestEntity>();
            mapBuilder.Property(x => x.CustomSetterValue)
                      .HasPropertySetter((x, v) =>
                          {
                              x.CustomSetterValue = v;
                              x.CustomSetterValueSpecified = true;
                          });
            var map = mapBuilder.CreateMap();
            var results = Connection.Entity(map).SqlQuery("SELECT 1 As CustomSetterValue")
                                    .ToArray();
            Assert.AreEqual(1, results[0].CustomSetterValue);
            Assert.IsTrue(results[0].CustomSetterValueSpecified);
        }

        [TestMethod]
        public void Should_allow_non_static_custom_setter()
        {
            var mapBuilder = new DbClassMapBuilder<TestEntity>();
            mapBuilder.Property(x => x.Int32Value);
            mapBuilder.Property(x => x.CustomSetterValue)
                      .HasPropertySetter((x, value) =>
                          {
                              x.CustomSetterValue = value;
                              x.CustomSetterValueSpecified = true;
                          });
            var map = mapBuilder.CreateMap();

            // verify value read
            var result = Connection.Entity(map).SqlQuery("SELECT 1 As CustomSetterValue").Single();
            Assert.AreEqual(1, result.CustomSetterValue);
            Assert.IsTrue(result.CustomSetterValueSpecified);

            // verify value not read
            result = Connection.Entity(map).SqlQuery("SELECT 1 As Int32Value").Single();
            Assert.AreEqual(0, result.CustomSetterValue);
            Assert.IsFalse(result.CustomSetterValueSpecified);
        }

        [TestMethod]
        public void Should_customize_default_map()
        {
            // override just id
            var mapBuilder = new DbClassMapBuilder<TestEntity>(DbClassMap<TestEntity>.Default);
            mapBuilder.Property(x => x.Int32Value)
                      .HasColumnName("Id");
            var map = mapBuilder.CreateMap();
            var result = Connection.Entity(map).SqlQuery("SELECT 1 AS Id, 'Test' AS StringValue").Single();
            Assert.AreEqual(1, result.Int32Value);
            Assert.AreEqual("Test", result.StringValue);
        }

        [TestMethod]
        public void Should_bind_inherited_properties()
        {
            var input = new TestEntityDescendent() {Int32Value = 1, StringValue = "Hello World", DescendentProperty = 2};
            // override just id
            const string SQL = @"
                DECLARE @TestTable TABLE (Int32Value INT, StringValue NVARCHAR(50), DescendentProperty INT)
                INSERT INTO @TestTable VALUES (@Int32Value, @StringValue, @DescendentProperty)
                SELECT * FROM @TestTable";
            var result = Connection.Entity<TestEntityDescendent>()
                                   .SqlQuery(SQL, input).Single();
            Assert.AreEqual(input.Int32Value, result.Int32Value);
            Assert.AreEqual(input.StringValue, result.StringValue);
            Assert.AreEqual(input.DescendentProperty, result.DescendentProperty);
        }

        [TestMethod]
        public void Should_read_case_insensitive()
        {
            var mapBuilder = new DbClassMapBuilder<TestEntity>(DbClassMap<TestEntity>.Default);
            mapBuilder.Property(x => x.StringValue)
                      .HasColumnName("MappedValue");
            var map = mapBuilder.CreateMap();
            var result = Connection.Entity(map)
                                   .SqlQuery("SELECT 1 As INT32VALUE, 'Hello World' As MAPPEDVALUE").Single();
            Assert.AreEqual(1, result.Int32Value);
            Assert.AreEqual("Hello World", result.StringValue);
        }

        #region Nested type: TestEntity

        public class TestEntity
        {
            #region Fields

            private int int32Value;
            private double? nullableDoubleValue;
            private int? nullableInt32Value;
            private string stringValue;

            #endregion

            public int Int32Value
            {
                get { return this.int32Value; }
                set
                {
                    this.Int32WriteCount++;
                    this.int32Value = value;
                }
            }

            public int Int32WriteCount { get; private set; }

            public int? NullableInt32Value
            {
                get { return this.nullableInt32Value; }
                set
                {
                    this.NullableInt32WriteCount++;
                    this.nullableInt32Value = value;
                }
            }

            public int NullableInt32WriteCount { get; private set; }

            public string StringValue
            {
                get { return this.stringValue; }
                set
                {
                    this.StringWriteCount++;
                    this.stringValue = value;
                }
            }

            public int StringWriteCount { get; private set; }

            public double? NullableDoubleValue
            {
                get { return this.nullableDoubleValue; }
                set
                {
                    this.NullableDoubleWriteCount++;
                    this.nullableDoubleValue = value;
                }
            }

            public int NullableDoubleWriteCount { get; private set; }

            public int CustomSetterValue { get; set; }
            public bool CustomSetterValueSpecified { get; set; }

            public TestEntityEnum IntToEnumValue { get; set; }
            public TestEntityEnum StringToEnumValue { get; set; }
            public string DefaultNameValue { get; set; }
            public string[] ParsedProperty { get; set; }
        }

        #endregion

        public class TestEntityDescendent : TestEntity
        {
            public int DescendentProperty { get; set; }
        }
    }
}

