using System;
using System.Reflection;
using Xunit;
using Structura;

namespace Structura.Tests
{
    /// <summary>
    /// Tests for verifying ChangeType functionality
    /// </summary>
    public class ChangeTypeFunctionalityTests
    {
        [Fact]
        public void ChangeType_SingleProperty_Should_ChangePropertyType()
        {
            // Arrange
            TypeCombiner.Combine()
                .With(new { Id = 1, Price = 100m, Name = "Product" })
                .ChangeType("Price", typeof(string))
                .WithName("ChangeTypeSingleTest")
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.ChangeTypeSingleTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            // Price should be string type
            var priceProperty = generatedType.GetProperty("Price");
            Assert.NotNull(priceProperty);
            Assert.Equal(typeof(string), priceProperty.PropertyType);
            
            // Other properties should remain unchanged
            var idProperty = generatedType.GetProperty("Id");
            var nameProperty = generatedType.GetProperty("Name");
            
            Assert.NotNull(idProperty);
            Assert.NotNull(nameProperty);
            Assert.Equal(typeof(int), idProperty.PropertyType);
            Assert.Equal(typeof(string), nameProperty.PropertyType);
        }

        [Fact]
        public void ChangeType_MultipleProperties_Should_ChangeAllSpecifiedTypes()
        {
            // Arrange
            TypeCombiner.Combine()
                .With(new { Id = 1, Price = 100m, Quantity = 5, IsActive = true })
                .ChangeType("Price", typeof(string))
                .ChangeType("Quantity", typeof(long))
                .ChangeType("IsActive", typeof(int))
                .WithName("ChangeTypeMultiTest")
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.ChangeTypeMultiTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            // Check changed types
            var priceProperty = generatedType.GetProperty("Price");
            var quantityProperty = generatedType.GetProperty("Quantity");
            var isActiveProperty = generatedType.GetProperty("IsActive");
            
            Assert.NotNull(priceProperty);
            Assert.NotNull(quantityProperty);
            Assert.NotNull(isActiveProperty);
            
            Assert.Equal(typeof(string), priceProperty.PropertyType);
            Assert.Equal(typeof(long), quantityProperty.PropertyType);
            Assert.Equal(typeof(int), isActiveProperty.PropertyType);
            
            // Unchanged property
            var idProperty = generatedType.GetProperty("Id");
            Assert.NotNull(idProperty);
            Assert.Equal(typeof(int), idProperty.PropertyType);
        }

        [Fact]
        public void ChangeType_WithProjection_Should_ChangeProjectionPropertyTypes()
        {
            // Arrange
            var projectionData = new[]
            {
                new { OrderId = 1, Total = 250.50m, CustomerName = "Alice" },
                new { OrderId = 2, Total = 150.25m, CustomerName = "Bob" }
            };

            TypeCombiner.Combine()
                .WithProjection(projectionData)
                .ChangeType("Total", typeof(string))
                .ChangeType("OrderId", typeof(string))
                .WithName("ChangeTypeProjectionTest")
                .AsRecord()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.ChangeTypeProjectionTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            // Check changed types
            var totalProperty = generatedType.GetProperty("Total");
            var orderIdProperty = generatedType.GetProperty("OrderId");
            
            Assert.NotNull(totalProperty);
            Assert.NotNull(orderIdProperty);
            
            Assert.Equal(typeof(string), totalProperty.PropertyType);
            Assert.Equal(typeof(string), orderIdProperty.PropertyType);
            
            // Unchanged property
            var customerNameProperty = generatedType.GetProperty("CustomerName");
            Assert.NotNull(customerNameProperty);
            Assert.Equal(typeof(string), customerNameProperty.PropertyType);
        }

        [Fact]
        public void ChangeType_WithAdd_Should_ChangeAddedPropertyType()
        {
            // Arrange
            TypeCombiner.Combine()
                .With(new { Name = "Product", Category = "Electronics" })
                .Add("Price", typeof(decimal))
                .ChangeType("Price", typeof(string))
                .WithName("ChangeTypeAddTest")
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.ChangeTypeAddTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            // Price should be string (changed from decimal)
            var priceProperty = generatedType.GetProperty("Price");
            Assert.NotNull(priceProperty);
            Assert.Equal(typeof(string), priceProperty.PropertyType);
            
            // Other properties unchanged
            var nameProperty = generatedType.GetProperty("Name");
            var categoryProperty = generatedType.GetProperty("Category");
            
            Assert.NotNull(nameProperty);
            Assert.NotNull(categoryProperty);
            Assert.Equal(typeof(string), nameProperty.PropertyType);
            Assert.Equal(typeof(string), categoryProperty.PropertyType);
        }

        [Fact]
        public void ChangeType_ToComplexTypes_Should_HandleComplexTypeChanges()
        {
            // Arrange
            TypeCombiner.Combine()
                .With(new { Created = DateTime.Now, Count = 100, Status = true })
                .ChangeType("Created", typeof(string))
                .ChangeType("Count", typeof(decimal))
                .ChangeType("Status", typeof(string))
                .WithName("ChangeTypeComplexTest")
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.ChangeTypeComplexTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            var createdProperty = generatedType.GetProperty("Created");
            var countProperty = generatedType.GetProperty("Count");
            var statusProperty = generatedType.GetProperty("Status");
            
            Assert.NotNull(createdProperty);
            Assert.NotNull(countProperty);
            Assert.NotNull(statusProperty);
            
            Assert.Equal(typeof(string), createdProperty.PropertyType);
            Assert.Equal(typeof(decimal), countProperty.PropertyType);
            Assert.Equal(typeof(string), statusProperty.PropertyType);
        }

        [Fact]
        public void ChangeType_NonExistentProperty_Should_NotAffectGeneration()
        {
            // Arrange
            TypeCombiner.Combine()
                .With(new { Name = "Test", Value = 42 })
                .ChangeType("NonExistentProperty", typeof(string))
                .WithName("ChangeTypeNonExistentTest")
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.ChangeTypeNonExistentTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            // Should have all original properties with original types
            var nameProperty = generatedType.GetProperty("Name");
            var valueProperty = generatedType.GetProperty("Value");
            
            Assert.NotNull(nameProperty);
            Assert.NotNull(valueProperty);
            Assert.Equal(typeof(string), nameProperty.PropertyType);
            Assert.Equal(typeof(int), valueProperty.PropertyType);
        }

        [Fact]
        public void ChangeType_CombinedWithExclude_Should_HandleBothOperations()
        {
            // Arrange
            TypeCombiner.Combine()
                .With(new { Id = 1, Name = "Product", Price = 100m, Secret = "confidential" })
                .ChangeType("Price", typeof(string))
                .Exclude("Secret")
                .WithName("ChangeTypeExcludeTest")
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.ChangeTypeExcludeTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            // Should have properties with correct types
            var idProperty = generatedType.GetProperty("Id");
            var nameProperty = generatedType.GetProperty("Name");
            var priceProperty = generatedType.GetProperty("Price");
            
            Assert.NotNull(idProperty);
            Assert.NotNull(nameProperty);
            Assert.NotNull(priceProperty);
            
            Assert.Equal(typeof(int), idProperty.PropertyType);
            Assert.Equal(typeof(string), nameProperty.PropertyType);
            Assert.Equal(typeof(string), priceProperty.PropertyType); // Changed type
            
            // Should NOT have excluded property
            var secretProperty = generatedType.GetProperty("Secret");
            Assert.Null(secretProperty);
        }

        [Fact]
        public void ChangeType_ToNullableTypes_Should_HandleNullableTypeChanges()
        {
            // Arrange
            TypeCombiner.Combine()
                .With(new { Id = 1, Name = "Test", Value = 42 })
                .ChangeType("Id", typeof(int?))
                .ChangeType("Value", typeof(double?))
                .WithName("ChangeTypeNullableTest")
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.ChangeTypeNullableTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            var idProperty = generatedType.GetProperty("Id");
            var valueProperty = generatedType.GetProperty("Value");
            var nameProperty = generatedType.GetProperty("Name");
            
            Assert.NotNull(idProperty);
            Assert.NotNull(valueProperty);
            Assert.NotNull(nameProperty);
            
            Assert.Equal(typeof(int?), idProperty.PropertyType);
            Assert.Equal(typeof(double?), valueProperty.PropertyType);
            Assert.Equal(typeof(string), nameProperty.PropertyType); // Unchanged
        }
    }
}