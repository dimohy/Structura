using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Structura;

namespace Structura.Tests
{
    /// <summary>
    /// Tests for verifying converter functionality generation and behavior
    /// </summary>
    public class ConverterFunctionalityTests
    {
        [Fact]
        public void WithConverter_Should_GenerateConverterMethods()
        {
            // Arrange
            TypeCombiner.Combine()
                .With(new { Name = "Test", Value = 42 })
                .WithName("ConverterMethodTest")
                .WithConverter()
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.ConverterMethodTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            // Check for converter methods - only FromCollection and FromSingle
            var fromCollectionMethod = generatedType.GetMethod("FromCollection", BindingFlags.Public | BindingFlags.Static);
            var fromSingleMethod = generatedType.GetMethod("FromSingle", BindingFlags.Public | BindingFlags.Static);

            Assert.NotNull(fromCollectionMethod);
            Assert.NotNull(fromSingleMethod);
        }

        [Fact]
        public void WithoutConverter_Should_NotGenerateConverterMethods()
        {
            // Arrange
            TypeCombiner.Combine()
                .With(new { Name = "Test", Value = 42 })
                .WithName("NoConverterTest")
                .AsClass()
                .Generate();

            // Act
            var generatedType = Type.GetType("Generated.NoConverterTest, Structura.Tests");

            // Assert
            Assert.NotNull(generatedType);
            
            // Should NOT have converter methods
            var fromCollectionMethod = generatedType.GetMethod("FromCollection", BindingFlags.Public | BindingFlags.Static);
            var fromSingleMethod = generatedType.GetMethod("FromSingle", BindingFlags.Public | BindingFlags.Static);

            Assert.Null(fromCollectionMethod);
            Assert.Null(fromSingleMethod);
        }

        [Fact]
        public void FromSingle_Should_ConvertAnonymousObjectCorrectly()
        {
            // Arrange
            TypeCombiner.Combine()
                .With(new { Name = "Test", Value = 42 })
                .WithName("FromSingleTest")
                .WithConverter()
                .AsClass()
                .Generate();

            var generatedType = Type.GetType("Generated.FromSingleTest, Structura.Tests");
            Assert.NotNull(generatedType);

            var fromSingleMethod = generatedType.GetMethod("FromSingle", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(fromSingleMethod);

            // Act
            var anonymousObject = new { Name = "Test Name", Value = 123 };
            var result = fromSingleMethod.Invoke(null, new object[] { anonymousObject });

            // Assert
            Assert.NotNull(result);
            Assert.IsType(generatedType, result);

            var nameProperty = generatedType.GetProperty("Name");
            var valueProperty = generatedType.GetProperty("Value");

            Assert.Equal("Test Name", nameProperty.GetValue(result));
            Assert.Equal(123, valueProperty.GetValue(result));
        }

        [Fact]
        public void FromCollection_Should_ConvertAnonymousObjectCollection()
        {
            // Arrange
            TypeCombiner.Combine()
                .With(new { Name = "Test", Value = 42 })
                .WithName("FromCollectionTest")
                .WithConverter()
                .AsClass()
                .Generate();

            var generatedType = Type.GetType("Generated.FromCollectionTest, Structura.Tests");
            Assert.NotNull(generatedType);

            var fromCollectionMethod = generatedType.GetMethod("FromCollection", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(fromCollectionMethod);

            // Act
            var anonymousObjects = new object[]
            {
                new { Name = "Item 1", Value = 100 },
                new { Name = "Item 2", Value = 200 }
            };
            
            var result = fromCollectionMethod.Invoke(null, new object[] { anonymousObjects });

            // Assert
            Assert.NotNull(result);
            
            // Result should be a List<GeneratedType>
            var listType = typeof(List<>).MakeGenericType(generatedType);
            Assert.IsType(listType, result);

            var resultList = (System.Collections.IList)result;
            Assert.Equal(2, resultList.Count);

            // Check first item
            var firstItem = resultList[0];
            var nameProperty = generatedType.GetProperty("Name");
            var valueProperty = generatedType.GetProperty("Value");

            Assert.Equal("Item 1", nameProperty.GetValue(firstItem));
            Assert.Equal(100, valueProperty.GetValue(firstItem));
        }

        [Fact]
        public void Converter_Should_HandleNullValues()
        {
            // Arrange
            TypeCombiner.Combine()
                .With(new { Name = "Test", Value = 42 })
                .WithName("NullHandlingTest")
                .WithConverter()
                .AsClass()
                .Generate();

            var generatedType = Type.GetType("Generated.NullHandlingTest, Structura.Tests");
            Assert.NotNull(generatedType);

            var fromSingleMethod = generatedType.GetMethod("FromSingle", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(fromSingleMethod);

            // Act & Assert - Should handle null input appropriately
            Assert.Throws<TargetInvocationException>(() =>
            {
                fromSingleMethod.Invoke(null, new object[] { null });
            });
        }

        [Fact]
        public void Converter_Should_HandleTypeConversion()
        {
            // Arrange
            TypeCombiner.Combine()
                .With(new { StringValue = "123", IntValue = 456 })
                .WithName("TypeConversionTest")
                .WithConverter()
                .AsClass()
                .Generate();

            var generatedType = Type.GetType("Generated.TypeConversionTest, Structura.Tests");
            Assert.NotNull(generatedType);

            var fromSingleMethod = generatedType.GetMethod("FromSingle", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(fromSingleMethod);

            // Act - Convert with different types (string to string, int to int)
            var testObject = new { StringValue = "converted", IntValue = 789 };
            var result = fromSingleMethod.Invoke(null, new object[] { testObject });

            // Assert
            Assert.NotNull(result);
            
            var stringProperty = generatedType.GetProperty("StringValue");
            var intProperty = generatedType.GetProperty("IntValue");

            Assert.Equal("converted", stringProperty.GetValue(result));
            Assert.Equal(789, intProperty.GetValue(result));
        }
    }
}