using System;
using System.Reflection;
using Xunit;
using Structura;

namespace Structura.Tests
{
    /// <summary>
    /// Tests for verifying Add functionality and property generation
    /// </summary>
    public class AddFunctionalityTests
    {
        [Fact]
        public void Add_Should_CreatePropertyWithCorrectType()
        {
            // Arrange
            TypeCombiner.Combine()
                .Add("TestProperty", typeof(string))
                .WithName("AddPropertyTest")
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.AddPropertyTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            var property = generatedType.GetProperty("TestProperty");
            Assert.NotNull(property);
            Assert.Equal(typeof(string), property.PropertyType);
        }

        [Fact]
        public void Add_WithMultipleProperties_Should_CreateAllProperties()
        {
            // Arrange
            TypeCombiner.Combine()
                .Add("Name", typeof(string))
                .Add("Age", typeof(int))
                .Add("CreatedAt", typeof(DateTime))
                .Add("IsActive", typeof(bool))
                .WithName("MultipleAddTest")
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.MultipleAddTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            // Verify all added properties exist with correct types
            var nameProperty = generatedType.GetProperty("Name");
            var ageProperty = generatedType.GetProperty("Age");
            var createdAtProperty = generatedType.GetProperty("CreatedAt");
            var isActiveProperty = generatedType.GetProperty("IsActive");

            Assert.NotNull(nameProperty);
            Assert.NotNull(ageProperty);
            Assert.NotNull(createdAtProperty);
            Assert.NotNull(isActiveProperty);

            Assert.Equal(typeof(string), nameProperty.PropertyType);
            Assert.Equal(typeof(int), ageProperty.PropertyType);
            Assert.Equal(typeof(DateTime), createdAtProperty.PropertyType);
            Assert.Equal(typeof(bool), isActiveProperty.PropertyType);
        }

        [Fact]
        public void Add_CombinedWithAnonymousType_Should_CreateAllProperties()
        {
            // Arrange
            TypeCombiner.Combine()
                .With(new { Name = "Test", Value = 42 })
                .Add("Id", typeof(int))
                .Add("CreatedAt", typeof(DateTime))
                .WithName("CombinedWithAddTest")
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.CombinedWithAddTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            // Properties from anonymous type
            var nameProperty = generatedType.GetProperty("Name");
            var valueProperty = generatedType.GetProperty("Value");
            
            // Properties from Add operations
            var idProperty = generatedType.GetProperty("Id");
            var createdAtProperty = generatedType.GetProperty("CreatedAt");

            Assert.NotNull(nameProperty);
            Assert.NotNull(valueProperty);
            Assert.NotNull(idProperty);
            Assert.NotNull(createdAtProperty);

            Assert.Equal(typeof(string), nameProperty.PropertyType);
            Assert.Equal(typeof(int), valueProperty.PropertyType);
            Assert.Equal(typeof(int), idProperty.PropertyType);
            Assert.Equal(typeof(DateTime), createdAtProperty.PropertyType);
        }

        [Fact]
        public void Add_WithComplexTypes_Should_CreatePropertiesWithCorrectTypes()
        {
            // Arrange
            TypeCombiner.Combine()
                .Add("Id", typeof(Guid))
                .Add("Tags", typeof(string[]))
                .Add("Metadata", typeof(System.Collections.Generic.Dictionary<string, object>))
                .Add("NullableDate", typeof(DateTime?))
                .WithName("ComplexTypesAddTest")
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.ComplexTypesAddTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            var idProperty = generatedType.GetProperty("Id");
            var tagsProperty = generatedType.GetProperty("Tags");
            var metadataProperty = generatedType.GetProperty("Metadata");
            var nullableDateProperty = generatedType.GetProperty("NullableDate");

            Assert.NotNull(idProperty);
            Assert.NotNull(tagsProperty);
            Assert.NotNull(metadataProperty);
            Assert.NotNull(nullableDateProperty);

            Assert.Equal(typeof(Guid), idProperty.PropertyType);
            Assert.Equal(typeof(string[]), tagsProperty.PropertyType);
            Assert.Equal(typeof(System.Collections.Generic.Dictionary<string, object>), metadataProperty.PropertyType);
            Assert.Equal(typeof(DateTime?), nullableDateProperty.PropertyType);
        }

        [Fact]
        public void Add_WithRecordType_Should_CreateRecordWithConstructor()
        {
            // Arrange
            TypeCombiner.Combine()
                .Add("Name", typeof(string))
                .Add("Value", typeof(int))
                .WithName("AddRecordTest")
                .AsRecord()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.AddRecordTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            // Records should have a constructor with all properties as parameters
            var constructors = generatedType.GetConstructors();
            var primaryConstructor = Array.Find(constructors, c => c.GetParameters().Length == 2);
            
            Assert.NotNull(primaryConstructor);
            
            var parameters = primaryConstructor.GetParameters();
            Assert.Equal(2, parameters.Length);
            
            // Check parameter types match the added properties
            Assert.Contains(parameters, p => p.ParameterType == typeof(string));
            Assert.Contains(parameters, p => p.ParameterType == typeof(int));
        }

        [Fact]
        public void Add_PropertyCanBeAccessedAndSet()
        {
            // Arrange
            TypeCombiner.Combine()
                .Add("TestName", typeof(string))
                .Add("TestValue", typeof(int))
                .WithName("PropertyAccessTest")
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.PropertyAccessTest, Structura.Tests");
            Assert.NotNull(generatedType);

            var instance = Activator.CreateInstance(generatedType);
            Assert.NotNull(instance);

            var nameProperty = generatedType.GetProperty("TestName");
            var valueProperty = generatedType.GetProperty("TestValue");

            // Set values
            nameProperty.SetValue(instance, "Test Name");
            valueProperty.SetValue(instance, 123);

            // Get values
            var nameValue = nameProperty.GetValue(instance);
            var valueValue = valueProperty.GetValue(instance);

            // Assert
            Assert.Equal("Test Name", nameValue);
            Assert.Equal(123, valueValue);
        }
    }
}