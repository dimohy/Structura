using System;
using System.Reflection;
using Xunit;
using Structura;

namespace Structura.Tests
{
    /// <summary>
    /// Tests for verifying projection functionality and property extraction
    /// </summary>
    public class ProjectionFunctionalityTests
    {
        [Fact]
        public void WithProjection_Should_ExtractPropertiesFromArray()
        {
            // Arrange
            var projectionData = new[]
            {
                new { Name = "Alice", Age = 25 },
                new { Name = "Bob", Age = 30 }
            };

            TypeCombiner.Combine()
                .WithProjection(projectionData)
                .WithName("ArrayProjectionTest")
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.ArrayProjectionTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            // Should have properties extracted from the anonymous type in the array
            var nameProperty = generatedType.GetProperty("Name");
            var ageProperty = generatedType.GetProperty("Age");

            Assert.NotNull(nameProperty);
            Assert.NotNull(ageProperty);
            Assert.Equal(typeof(string), nameProperty.PropertyType);
            Assert.Equal(typeof(int), ageProperty.PropertyType);
        }

        [Fact]
        public void WithProjection_ComplexTypes_Should_ExtractAllProperties()
        {
            // Arrange
            var complexProjection = new[]
            {
                new { 
                    Id = 1, 
                    Name = "Product", 
                    Price = 100.50m, 
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }
            };

            TypeCombiner.Combine()
                .WithProjection(complexProjection)
                .WithName("ComplexProjectionTest")
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.ComplexProjectionTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            var idProperty = generatedType.GetProperty("Id");
            var nameProperty = generatedType.GetProperty("Name");
            var priceProperty = generatedType.GetProperty("Price");
            var isActiveProperty = generatedType.GetProperty("IsActive");
            var createdAtProperty = generatedType.GetProperty("CreatedAt");

            Assert.NotNull(idProperty);
            Assert.NotNull(nameProperty);
            Assert.NotNull(priceProperty);
            Assert.NotNull(isActiveProperty);
            Assert.NotNull(createdAtProperty);

            Assert.Equal(typeof(int), idProperty.PropertyType);
            Assert.Equal(typeof(string), nameProperty.PropertyType);
            Assert.Equal(typeof(decimal), priceProperty.PropertyType);
            Assert.Equal(typeof(bool), isActiveProperty.PropertyType);
            Assert.Equal(typeof(DateTime), createdAtProperty.PropertyType);
        }

        [Fact]
        public void WithProjection_AndAdd_Should_CombineProperties()
        {
            // Arrange
            var projectionData = new[]
            {
                new { UserId = 1, UserName = "john" }
            };

            TypeCombiner.Combine()
                .WithProjection(projectionData)
                .Add("IsActive", typeof(bool))
                .Add("CreatedAt", typeof(DateTime))
                .WithName("ProjectionWithAddTest")
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.ProjectionWithAddTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            // Properties from projection
            var userIdProperty = generatedType.GetProperty("UserId");
            var userNameProperty = generatedType.GetProperty("UserName");
            
            // Properties from Add
            var isActiveProperty = generatedType.GetProperty("IsActive");
            var createdAtProperty = generatedType.GetProperty("CreatedAt");

            Assert.NotNull(userIdProperty);
            Assert.NotNull(userNameProperty);
            Assert.NotNull(isActiveProperty);
            Assert.NotNull(createdAtProperty);

            Assert.Equal(typeof(int), userIdProperty.PropertyType);
            Assert.Equal(typeof(string), userNameProperty.PropertyType);
            Assert.Equal(typeof(bool), isActiveProperty.PropertyType);
            Assert.Equal(typeof(DateTime), createdAtProperty.PropertyType);
        }

        [Fact]
        public void WithProjection_AndConverter_Should_GenerateConverterMethods()
        {
            // Arrange
            var projectionData = new[]
            {
                new { ProductId = 1, ProductName = "Test Product" }
            };

            TypeCombiner.Combine()
                .WithProjection(projectionData)
                .WithName("ProjectionConverterTest")
                .WithConverter()
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.ProjectionConverterTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            // Should have both properties and converter methods
            var productIdProperty = generatedType.GetProperty("ProductId");
            var productNameProperty = generatedType.GetProperty("ProductName");
            var fromCollectionMethod = generatedType.GetMethod("FromCollection", BindingFlags.Public | BindingFlags.Static);

            Assert.NotNull(productIdProperty);
            Assert.NotNull(productNameProperty);
            Assert.NotNull(fromCollectionMethod);
        }

        [Fact]
        public void WithProjection_Record_Should_GenerateRecordWithProjectedProperties()
        {
            // Arrange
            var projectionData = new[]
            {
                new { OrderId = 1, OrderTotal = 150.75m, CustomerName = "John Doe" }
            };

            TypeCombiner.Combine()
                .WithProjection(projectionData)
                .WithName("ProjectionRecordTest")
                .AsRecord()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.ProjectionRecordTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            // Should have properties from projection
            var orderIdProperty = generatedType.GetProperty("OrderId");
            var orderTotalProperty = generatedType.GetProperty("OrderTotal");
            var customerNameProperty = generatedType.GetProperty("CustomerName");

            Assert.NotNull(orderIdProperty);
            Assert.NotNull(orderTotalProperty);
            Assert.NotNull(customerNameProperty);

            // Should have record constructor
            var constructors = generatedType.GetConstructors();
            var primaryConstructor = Array.Find(constructors, c => c.GetParameters().Length == 3);
            Assert.NotNull(primaryConstructor);
        }

        [Fact]
        public void WithProjection_Struct_Should_GenerateStructWithProjectedProperties()
        {
            // Arrange
            var projectionData = new[]
            {
                new { X = 10, Y = 20, Z = 30 }
            };

            TypeCombiner.Combine()
                .WithProjection(projectionData)
                .WithName("ProjectionStructTest")
                .AsStruct()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.ProjectionStructTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            Assert.True(generatedType.IsValueType);
            
            var xProperty = generatedType.GetProperty("X");
            var yProperty = generatedType.GetProperty("Y");
            var zProperty = generatedType.GetProperty("Z");

            Assert.NotNull(xProperty);
            Assert.NotNull(yProperty);
            Assert.NotNull(zProperty);

            Assert.Equal(typeof(int), xProperty.PropertyType);
            Assert.Equal(typeof(int), yProperty.PropertyType);
            Assert.Equal(typeof(int), zProperty.PropertyType);
        }

        [Fact]
        public void WithProjection_EmptyArray_Should_HandleGracefully()
        {
            // Arrange - This might not generate anything, but shouldn't crash
            var emptyProjection = new object[0];

            // Act & Assert - Should not throw
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .WithProjection(emptyProjection)
                    .WithName("EmptyProjectionTest")
                    .AsClass()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void WithProjection_DuplicatePropertyNames_Should_HandleCorrectly()
        {
            // Arrange
            var projectionData = new[]
            {
                new { Name = "Test", Value = 42 }
            };

            TypeCombiner.Combine()
                .WithProjection(projectionData)
                .Add("Name", typeof(string)) // Same property name as in projection
                .WithName("DuplicatePropertyTest")
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.DuplicatePropertyTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            // Should have the property (no duplicates)
            var nameProperty = generatedType.GetProperty("Name");
            var valueProperty = generatedType.GetProperty("Value");

            Assert.NotNull(nameProperty);
            Assert.NotNull(valueProperty);
            
            // Should not have multiple Name properties
            var nameProperties = generatedType.GetProperties().Where(p => p.Name == "Name").ToArray();
            Assert.Single(nameProperties);
        }
    }
}