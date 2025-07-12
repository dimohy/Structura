using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Structura;

namespace Structura.Tests
{
    /// <summary>
    /// Integration tests for complete functionality scenarios
    /// </summary>
    public class IntegrationTests
    {
        [Fact]
        public void CompleteScenario_AnonymousTypeWithAddAndConverter_Should_Work()
        {
            // Arrange - Simulate a real-world scenario
            var userData = new { FirstName = "John", LastName = "Doe", Email = "john@test.com" };

            TypeCombiner.Combine()
                .With(userData)
                .Add("UserId", typeof(Guid))
                .Add("CreatedAt", typeof(DateTime))
                .Add("IsActive", typeof(bool))
                .WithName("CompleteUserProfile")
                .WithConverter()
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.CompleteUserProfile, Structura.Tests");

            // Assert Type Generation
            Assert.NotNull(generatedType);
            Assert.True(generatedType.IsClass);

            // Assert Properties from Anonymous Type
            var firstNameProperty = generatedType.GetProperty("FirstName");
            var lastNameProperty = generatedType.GetProperty("LastName");
            var emailProperty = generatedType.GetProperty("Email");

            Assert.NotNull(firstNameProperty);
            Assert.NotNull(lastNameProperty);
            Assert.NotNull(emailProperty);

            // Assert Properties from Add
            var userIdProperty = generatedType.GetProperty("UserId");
            var createdAtProperty = generatedType.GetProperty("CreatedAt");
            var isActiveProperty = generatedType.GetProperty("IsActive");

            Assert.NotNull(userIdProperty);
            Assert.NotNull(createdAtProperty);
            Assert.NotNull(isActiveProperty);

            // Assert Converter Methods
            var fromSingleMethod = generatedType.GetMethod("FromSingle", BindingFlags.Public | BindingFlags.Static);
            var fromCollectionMethod = generatedType.GetMethod("FromCollection", BindingFlags.Public | BindingFlags.Static);

            Assert.NotNull(fromSingleMethod);
            Assert.NotNull(fromCollectionMethod);

            // Test actual conversion
            var testUser = new { FirstName = "Jane", LastName = "Smith", Email = "jane@test.com" };
            var convertedUser = fromSingleMethod.Invoke(null, new object[] { testUser });

            Assert.NotNull(convertedUser);
            Assert.Equal("Jane", firstNameProperty.GetValue(convertedUser));
            Assert.Equal("Smith", lastNameProperty.GetValue(convertedUser));
            Assert.Equal("jane@test.com", emailProperty.GetValue(convertedUser));
        }

        [Fact]
        public void CompleteScenario_ProjectionWithAddAndConverterAsRecord_Should_Work()
        {
            // Arrange - EF Core-like projection scenario
            var efProjection = new[]
            {
                new { OrderId = 1, CustomerName = "Alice", Total = 250.50m },
                new { OrderId = 2, CustomerName = "Bob", Total = 150.25m }
            };

            TypeCombiner.Combine()
                .WithProjection(efProjection)
                .Add("ProcessedAt", typeof(DateTime))
                .Add("Status", typeof(string))
                .WithName("EnhancedOrder")
                .WithConverter()
                .AsRecord()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.EnhancedOrder, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);

            // Check all properties exist
            var orderIdProperty = generatedType.GetProperty("OrderId");
            var customerNameProperty = generatedType.GetProperty("CustomerName");
            var totalProperty = generatedType.GetProperty("Total");
            var processedAtProperty = generatedType.GetProperty("ProcessedAt");
            var statusProperty = generatedType.GetProperty("Status");

            Assert.NotNull(orderIdProperty);
            Assert.NotNull(customerNameProperty);
            Assert.NotNull(totalProperty);
            Assert.NotNull(processedAtProperty);
            Assert.NotNull(statusProperty);

            // Check record constructor
            var constructors = generatedType.GetConstructors();
            var primaryConstructor = Array.Find(constructors, c => c.GetParameters().Length == 5);
            Assert.NotNull(primaryConstructor);

            // Check converter methods
            var fromCollectionMethod = generatedType.GetMethod("FromCollection", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(fromCollectionMethod);

            // Test actual conversion - use the generated method directly without MakeGenericMethod
            // The generated FromCollection method takes IEnumerable<object>, so cast the array
            var result = fromCollectionMethod.Invoke(null, new object[] { efProjection.Cast<object>() });

            Assert.NotNull(result);
            var resultList = (System.Collections.IList)result;
            Assert.Equal(2, resultList.Count);

            // Verify the converted objects have the correct property values
            var firstOrder = resultList[0];
            var secondOrder = resultList[1];

            Assert.Equal(1, orderIdProperty.GetValue(firstOrder));
            Assert.Equal("Alice", customerNameProperty.GetValue(firstOrder));
            Assert.Equal(250.50m, totalProperty.GetValue(firstOrder));

            Assert.Equal(2, orderIdProperty.GetValue(secondOrder));
            Assert.Equal("Bob", customerNameProperty.GetValue(secondOrder));
            Assert.Equal(150.25m, totalProperty.GetValue(secondOrder));
        }

        [Fact]
        public void CompleteScenario_MultipleAnonymousTypesWithAdd_Should_Work()
        {
            // Arrange - Multiple data sources scenario
            var personalInfo = new { FirstName = "John", LastName = "Doe", Age = 30 };
            var contactInfo = new { Email = "john@example.com", Phone = "123-456-7890" };
            var workInfo = new { Company = "Tech Corp", Position = "Developer" };

            TypeCombiner.Combine()
                .With(personalInfo)
                .With(contactInfo)
                .With(workInfo)
                .Add("EmployeeId", typeof(int))
                .Add("HireDate", typeof(DateTime))
                .Add("Salary", typeof(decimal))
                .WithName("CompleteEmployee")
                .WithConverter()
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.CompleteEmployee, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);

            // Properties from first anonymous type
            Assert.NotNull(generatedType.GetProperty("FirstName"));
            Assert.NotNull(generatedType.GetProperty("LastName"));
            Assert.NotNull(generatedType.GetProperty("Age"));

            // Properties from second anonymous type
            Assert.NotNull(generatedType.GetProperty("Email"));
            Assert.NotNull(generatedType.GetProperty("Phone"));

            // Properties from third anonymous type
            Assert.NotNull(generatedType.GetProperty("Company"));
            Assert.NotNull(generatedType.GetProperty("Position"));

            // Properties from Add operations
            Assert.NotNull(generatedType.GetProperty("EmployeeId"));
            Assert.NotNull(generatedType.GetProperty("HireDate"));
            Assert.NotNull(generatedType.GetProperty("Salary"));

            // Test instance creation and property access
            var instance = Activator.CreateInstance(generatedType);
            Assert.NotNull(instance);

            var firstNameProperty = generatedType.GetProperty("FirstName");
            var employeeIdProperty = generatedType.GetProperty("EmployeeId");

            firstNameProperty.SetValue(instance, "Test Name");
            employeeIdProperty.SetValue(instance, 12345);

            Assert.Equal("Test Name", firstNameProperty.GetValue(instance));
            Assert.Equal(12345, employeeIdProperty.GetValue(instance));
        }

        [Fact]
        public void CompleteScenario_StructWithComplexTypes_Should_Work()
        {
            // Arrange - Complex data types scenario
            var complexData = new 
            { 
                Id = Guid.NewGuid(),
                Tags = new[] { "tag1", "tag2" },
                Metadata = new Dictionary<string, object> { ["key"] = "value" },
                NullableDate = DateTime.Now as DateTime?
            };

            TypeCombiner.Combine()
                .With(complexData)
                .Add("ProcessingTime", typeof(TimeSpan))
                .Add("Flags", typeof(int[]))
                .WithName("ComplexDataStruct")
                .AsStruct()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.ComplexDataStruct, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            Assert.True(generatedType.IsValueType);

            // Check complex type properties
            var idProperty = generatedType.GetProperty("Id");
            var tagsProperty = generatedType.GetProperty("Tags");
            var metadataProperty = generatedType.GetProperty("Metadata");
            var nullableDateProperty = generatedType.GetProperty("NullableDate");
            var processingTimeProperty = generatedType.GetProperty("ProcessingTime");
            var flagsProperty = generatedType.GetProperty("Flags");

            Assert.NotNull(idProperty);
            Assert.NotNull(tagsProperty);
            Assert.NotNull(metadataProperty);
            Assert.NotNull(nullableDateProperty);
            Assert.NotNull(processingTimeProperty);
            Assert.NotNull(flagsProperty);

            Assert.Equal(typeof(Guid), idProperty.PropertyType);
            Assert.Equal(typeof(string[]), tagsProperty.PropertyType);
            Assert.Equal(typeof(Dictionary<string, object>), metadataProperty.PropertyType);
            Assert.Equal(typeof(DateTime?), nullableDateProperty.PropertyType);
            Assert.Equal(typeof(TimeSpan), processingTimeProperty.PropertyType);
            Assert.Equal(typeof(int[]), flagsProperty.PropertyType);
        }

        [Fact]
        public void PerformanceScenario_LargeNumberOfProperties_Should_Work()
        {
            // Arrange - Stress test with many properties
            var largeAnonymous = new 
            { 
                Prop01 = "Value01", Prop02 = "Value02", Prop03 = "Value03", Prop04 = "Value04", Prop05 = "Value05",
                Prop06 = "Value06", Prop07 = "Value07", Prop08 = "Value08", Prop09 = "Value09", Prop10 = "Value10",
                Prop11 = 11, Prop12 = 12, Prop13 = 13, Prop14 = 14, Prop15 = 15,
                Prop16 = true, Prop17 = false, Prop18 = true, Prop19 = false, Prop20 = true
            };

            TypeCombiner.Combine()
                .With(largeAnonymous)
                .Add("AddedProp01", typeof(DateTime))
                .Add("AddedProp02", typeof(Guid))
                .Add("AddedProp03", typeof(decimal))
                .Add("AddedProp04", typeof(double))
                .Add("AddedProp05", typeof(float))
                .WithName("LargePropertyTest")
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.LargePropertyTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);

            // Check that all properties were generated
            var properties = generatedType.GetProperties();
            Assert.True(properties.Length >= 25); // 20 from anonymous + 5 from Add

            // Spot check some properties
            Assert.NotNull(generatedType.GetProperty("Prop01"));
            Assert.NotNull(generatedType.GetProperty("Prop10"));
            Assert.NotNull(generatedType.GetProperty("Prop20"));
            Assert.NotNull(generatedType.GetProperty("AddedProp01"));
            Assert.NotNull(generatedType.GetProperty("AddedProp05"));
        }
    }
}