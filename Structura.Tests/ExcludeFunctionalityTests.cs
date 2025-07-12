using System;
using System.Reflection;
using Xunit;
using Structura;

namespace Structura.Tests
{
    /// <summary>
    /// Tests for verifying Exclude functionality
    /// </summary>
    public class ExcludeFunctionalityTests
    {
        [Fact]
        public void Exclude_SingleProperty_Should_RemovePropertyFromGeneratedType()
        {
            // Arrange
            TypeCombiner.Combine()
                .With(new { Name = "John", Password = "secret", Age = 30 })
                .Exclude("Password")
                .WithName("SafeUserTest")
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.SafeUserTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            // Should have Name and Age properties
            var nameProperty = generatedType.GetProperty("Name");
            var ageProperty = generatedType.GetProperty("Age");
            
            Assert.NotNull(nameProperty);
            Assert.NotNull(ageProperty);
            
            // Should NOT have Password property
            var passwordProperty = generatedType.GetProperty("Password");
            Assert.Null(passwordProperty);
        }

        [Fact]
        public void Exclude_MultipleProperties_Should_RemoveAllSpecifiedProperties()
        {
            // Arrange
            TypeCombiner.Combine()
                .With(new { Id = 1, Name = "Test", Password = "pwd", InternalId = 999, PublicInfo = "public" })
                .Exclude("Password")
                .Exclude("InternalId")
                .WithName("MultiExcludeTest")
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.MultiExcludeTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            // Should have remaining properties
            var idProperty = generatedType.GetProperty("Id");
            var nameProperty = generatedType.GetProperty("Name");
            var publicInfoProperty = generatedType.GetProperty("PublicInfo");
            
            Assert.NotNull(idProperty);
            Assert.NotNull(nameProperty);
            Assert.NotNull(publicInfoProperty);
            
            // Should NOT have excluded properties
            var passwordProperty = generatedType.GetProperty("Password");
            var internalIdProperty = generatedType.GetProperty("InternalId");
            
            Assert.Null(passwordProperty);
            Assert.Null(internalIdProperty);
        }

        [Fact]
        public void Exclude_WithProjection_Should_ExcludePropertiesFromProjection()
        {
            // Arrange
            var projectionData = new[]
            {
                new { Id = 1, Name = "Product A", Price = 100m, Secret = "confidential" },
                new { Id = 2, Name = "Product B", Price = 200m, Secret = "classified" }
            };

            TypeCombiner.Combine()
                .WithProjection(projectionData)
                .Exclude("Secret")
                .WithName("PublicProjectionTest")
                .AsRecord()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.PublicProjectionTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            // Should have public properties
            var idProperty = generatedType.GetProperty("Id");
            var nameProperty = generatedType.GetProperty("Name");
            var priceProperty = generatedType.GetProperty("Price");
            
            Assert.NotNull(idProperty);
            Assert.NotNull(nameProperty);
            Assert.NotNull(priceProperty);
            
            // Should NOT have excluded property
            var secretProperty = generatedType.GetProperty("Secret");
            Assert.Null(secretProperty);
        }

        [Fact]
        public void Exclude_WithAdd_Should_HandleBothExcludeAndAdd()
        {
            // Arrange
            TypeCombiner.Combine()
                .With(new { Name = "User", Password = "secret", Email = "user@test.com" })
                .Exclude("Password")
                .Add("IsVerified", typeof(bool))
                .WithName("EnhancedSafeUserTest")
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.EnhancedSafeUserTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            // Should have original properties (except excluded)
            var nameProperty = generatedType.GetProperty("Name");
            var emailProperty = generatedType.GetProperty("Email");
            
            Assert.NotNull(nameProperty);
            Assert.NotNull(emailProperty);
            
            // Should have added property
            var isVerifiedProperty = generatedType.GetProperty("IsVerified");
            Assert.NotNull(isVerifiedProperty);
            Assert.Equal(typeof(bool), isVerifiedProperty.PropertyType);
            
            // Should NOT have excluded property
            var passwordProperty = generatedType.GetProperty("Password");
            Assert.Null(passwordProperty);
        }

        [Fact]
        public void Exclude_NonExistentProperty_Should_NotAffectGeneration()
        {
            // Arrange
            TypeCombiner.Combine()
                .With(new { Name = "Test", Value = 42 })
                .Exclude("NonExistentProperty")
                .WithName("ExcludeNonExistentTest")
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.ExcludeNonExistentTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            // Should have all original properties
            var nameProperty = generatedType.GetProperty("Name");
            var valueProperty = generatedType.GetProperty("Value");
            
            Assert.NotNull(nameProperty);
            Assert.NotNull(valueProperty);
        }

        [Fact]
        public void Exclude_AllProperties_Should_GenerateEmptyType()
        {
            // Arrange
            TypeCombiner.Combine()
                .With(new { Name = "Test", Value = 42 })
                .Exclude("Name")
                .Exclude("Value")
                .WithName("EmptyExcludeTest")
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.EmptyExcludeTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            // Should have no properties (only inherited ones)
            var properties = generatedType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            Assert.Empty(properties);
        }
    }
}