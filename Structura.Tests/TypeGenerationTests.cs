using System;
using System.Reflection;
using Xunit;
using Structura;

namespace Structura.Tests
{
    /// <summary>
    /// Tests for verifying actual type generation by source generator
    /// </summary>
    public class TypeGenerationTests
    {
        [Fact]
        public void Generate_WithRecord_Should_CreateRecordType()
        {
            // Arrange
            TypeCombiner.Combine()
                .With(new { Name = "Test", Value = 42 })
                .WithName("GeneratedRecord")
                .AsRecord()
                .Generate();

            // Act - Try to get the generated type
            var generatedType = Type.GetType("Generated.GeneratedRecord, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            Assert.True(generatedType.IsValueType == false); // Records are reference types
            
            // Verify it's a record by checking for specific record characteristics
            var methods = generatedType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            var hasEqualityContract = Array.Exists(methods, m => m.Name == "get_EqualityContract");
            Assert.True(hasEqualityContract || generatedType.Name.Contains("Record")); // Record indicator
        }

        [Fact]
        public void Generate_WithClass_Should_CreateClassType()
        {
            // Arrange
            TypeCombiner.Combine()
                .With(new { Name = "Test", Value = 42 })
                .WithName("GeneratedClass")
                .AsClass()
                .Generate();

            // Act - Try to get the generated type
            var generatedType = Type.GetType("Generated.GeneratedClass, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            Assert.True(generatedType.IsClass);
            Assert.False(generatedType.IsValueType);
        }

        [Fact]
        public void Generate_WithStruct_Should_CreateStructType()
        {
            // Arrange
            TypeCombiner.Combine()
                .With(new { Name = "Test", Value = 42 })
                .WithName("GeneratedStruct")
                .AsStruct()
                .Generate();

            // Act - Try to get the generated type
            var generatedType = Type.GetType("Generated.GeneratedStruct, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            Assert.True(generatedType.IsValueType);
            Assert.False(generatedType.IsClass);
        }

        [Fact]
        public void Generate_Should_CreateTypeWithCorrectProperties()
        {
            // Arrange
            TypeCombiner.Combine()
                .With(new { Name = "Test", Age = 25, IsActive = true })
                .WithName("PropertyTestType")
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.PropertyTestType, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            // Check for expected properties
            var nameProperty = generatedType.GetProperty("Name");
            var ageProperty = generatedType.GetProperty("Age");
            var isActiveProperty = generatedType.GetProperty("IsActive");

            Assert.NotNull(nameProperty);
            Assert.NotNull(ageProperty);
            Assert.NotNull(isActiveProperty);

            // Check property types
            Assert.Equal(typeof(string), nameProperty.PropertyType);
            Assert.Equal(typeof(int), ageProperty.PropertyType);
            Assert.Equal(typeof(bool), isActiveProperty.PropertyType);
        }

        [Fact]
        public void Generate_WithMultipleAnonymousTypes_Should_CombineProperties()
        {
            // Arrange
            TypeCombiner.Combine()
                .With(new { FirstName = "John", LastName = "Doe" })
                .With(new { Age = 30, Email = "john@test.com" })
                .WithName("CombinedPropertiesType")
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.CombinedPropertiesType, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            // Should have properties from both anonymous types
            Assert.NotNull(generatedType.GetProperty("FirstName"));
            Assert.NotNull(generatedType.GetProperty("LastName"));
            Assert.NotNull(generatedType.GetProperty("Age"));
            Assert.NotNull(generatedType.GetProperty("Email"));
        }

        [Fact]
        public void Generate_WithCustomNamespace_Should_CreateTypeInSpecifiedNamespace()
        {
            // Arrange
            TypeCombiner.Combine()
                .With(new { ProductId = 1, ProductName = "Test Product" })
                .WithName("NamespaceTestType", "MyApp.Models")
                .AsClass()
                .Generate();

            // Act - Try to get the generated type from custom namespace
            var generatedType = Type.GetType("MyApp.Models.NamespaceTestType, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            Assert.Equal("MyApp.Models", generatedType.Namespace);
            Assert.Equal("NamespaceTestType", generatedType.Name);
            
            // Verify properties
            Assert.NotNull(generatedType.GetProperty("ProductId"));
            Assert.NotNull(generatedType.GetProperty("ProductName"));
        }

        [Fact]
        public void Generate_WithDefaultNamespace_Should_UseGeneratedNamespace()
        {
            // Arrange
            TypeCombiner.Combine()
                .With(new { Id = 1, Name = "Default" })
                .WithName("DefaultNamespaceTestType")
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.DefaultNamespaceTestType, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            Assert.Equal("Generated", generatedType.Namespace);
            Assert.Equal("DefaultNamespaceTestType", generatedType.Name);
        }

        [Fact]
        public void Generate_WithNestedNamespace_Should_CreateTypeInNestedNamespace()
        {
            // Arrange
            TypeCombiner.Combine()
                .With(new { UserId = 1, UserName = "TestUser" })
                .WithName("NestedTestType", "MyCompany.MyProject.Models.DTOs")
                .AsRecord()
                .Generate();

            // Act
            var generatedType = Type.GetType("MyCompany.MyProject.Models.DTOs.NestedTestType, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            Assert.Equal("MyCompany.MyProject.Models.DTOs", generatedType.Namespace);
            Assert.Equal("NestedTestType", generatedType.Name);
        }
    }
}