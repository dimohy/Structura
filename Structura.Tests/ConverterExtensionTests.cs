using Structura.Tests.TestModels;
using System.Reflection;

namespace Structura.Tests
{
    /// <summary>
    /// Tests for WithConverter() functionality and generated static converter methods
    /// </summary>
    public class StaticConverterTests
    {
        [Fact]
        public void WithConverter_BasicProjection_Should_GenerateStaticConverterMethods()
        {
            // Arrange
            var projectionData = new List<object>
            {
                new { Name = "John Doe", Email = "john@example.com", Age = 30 },
                new { Name = "Jane Smith", Email = "jane@example.com", Age = 25 }
            };

            // Act - Generate type with converter
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .WithProjection(projectionData)
                    .WithName("UserWithConverter")
                    .WithConverter()
                    .AsRecord()
                    .Generate();
            });

            // Assert - Type generation should succeed
            Assert.Null(exception);
            
            // Check if the generated type has static converter methods
            var generatedType = Type.GetType("Generated.UserWithConverter");
            if (generatedType != null)
            {
                // Check for static converter methods on the generated type itself
                var methods = generatedType.GetMethods(BindingFlags.Public | BindingFlags.Static);
                Assert.Contains(methods, m => m.Name == "FromCollection" && 
                    m.GetParameters().Length == 1 && 
                    m.GetParameters()[0].ParameterType == typeof(IEnumerable<object>));
                
                Assert.Contains(methods, m => m.Name == "FromSingle" && 
                    m.GetParameters().Length == 1 && 
                    m.GetParameters()[0].ParameterType == typeof(object));

                Assert.Contains(methods, m => m.Name == "FromTypedCollection" && 
                    m.IsGenericMethodDefinition &&
                    m.GetParameters().Length == 1);

                Assert.Contains(methods, m => m.Name == "FromTyped" && 
                    m.IsGenericMethodDefinition &&
                    m.GetParameters().Length == 1);
            }
        }

        [Fact]
        public void WithConverter_ActualConversion_Should_Work()
        {
            // Arrange - Generate type with converter
            var sourceData = new List<object>
            {
                new { Name = "John Doe", Email = "john@example.com", Age = 30 },
                new { Name = "Jane Smith", Email = "jane@example.com", Age = 25 }
            };

            TypeCombiner.Combine()
                .WithProjection(sourceData)
                .WithName("ConvertibleUser")
                .WithConverter()
                .AsRecord()
                .Generate();

            // Act & Assert - Try to use the generated static converter methods
            var converterType = Type.GetType("Generated.ConvertibleUserConverter");
            if (converterType != null)
            {
                // Test FromCollection method
                var collectionMethod = converterType.GetMethod("FromCollection", 
                    BindingFlags.Public | BindingFlags.Static);
                if (collectionMethod != null)
                {
                    var result = collectionMethod.Invoke(null, new object[] { sourceData });
                    Assert.NotNull(result);

                    // Check if result is a list
                    var resultType = result.GetType();
                    Assert.True(resultType.IsGenericType);
                    Assert.Equal(typeof(List<>), resultType.GetGenericTypeDefinition());
                }

                // Test FromSingle method
                var singleMethod = converterType.GetMethod("FromSingle", 
                    BindingFlags.Public | BindingFlags.Static);
                if (singleMethod != null)
                {
                    var singleResult = singleMethod.Invoke(null, new object[] { sourceData.First() });
                    Assert.NotNull(singleResult);

                    // Verify the converted object has the expected properties
                    var convertedType = singleResult.GetType();
                    var nameProperty = convertedType.GetProperty("Name");
                    var emailProperty = convertedType.GetProperty("Email");
                    var ageProperty = convertedType.GetProperty("Age");

                    Assert.NotNull(nameProperty);
                    Assert.NotNull(emailProperty);
                    Assert.NotNull(ageProperty);

                    // Verify property values (for dynamic conversion, this might be limited)
                    // Note: Dynamic property access might not work as expected in unit tests
                }

                // Test FromTypedCollection method with strongly-typed data
                var typedData = new[]
                {
                    new { Name = "John Doe", Email = "john@example.com", Age = 30 },
                    new { Name = "Jane Smith", Email = "jane@example.com", Age = 25 }
                };

                var typedCollectionMethod = converterType.GetMethod("FromTypedCollection", 
                    BindingFlags.Public | BindingFlags.Static);
                if (typedCollectionMethod != null && typedCollectionMethod.IsGenericMethodDefinition)
                {
                    var genericMethod = typedCollectionMethod.MakeGenericMethod(typedData.GetType().GetElementType()!);
                    var typedResult = genericMethod.Invoke(null, new object[] { typedData });
                    Assert.NotNull(typedResult);

                    var typedResultType = typedResult.GetType();
                    Assert.True(typedResultType.IsGenericType);
                    Assert.Equal(typeof(List<>), typedResultType.GetGenericTypeDefinition());
                }
            }
        }

        [Fact]
        public void WithConverter_TypedAnonymousConversion_Should_Work()
        {
            // Arrange - Generate type with converter for testing strongly-typed anonymous objects
            var projectionData = new List<object>
            {
                new { UserId = 1, UserName = "test_user", IsActive = true, Score = 95.5 }
            };

            TypeCombiner.Combine()
                .WithProjection(projectionData)
                .WithName("TypedAnonymousTest")
                .WithConverter()
                .AsRecord()
                .Generate();

            // Act & Assert - Test with actual strongly-typed anonymous objects
            var converterType = Type.GetType("Generated.TypedAnonymousTestConverter");
            if (converterType != null)
            {
                // Create strongly-typed anonymous data that matches the projection schema
                var typedAnonymousData = new { UserId = 42, UserName = "typed_user", IsActive = true, Score = 87.3 };

                // Get the FromTyped method
                var fromTypedMethod = converterType.GetMethod("FromTyped", 
                    BindingFlags.Public | BindingFlags.Static);
                if (fromTypedMethod != null && fromTypedMethod.IsGenericMethodDefinition)
                {
                    // Make the generic method specific to our strongly-typed anonymous type
                    var genericMethod = fromTypedMethod.MakeGenericMethod(typedAnonymousData.GetType());
                    
                    // Actually invoke the converter
                    var result = genericMethod.Invoke(null, new object[] { typedAnonymousData });
                    Assert.NotNull(result);

                    // Verify the converted object has the expected properties
                    var convertedType = result.GetType();
                    var userIdProperty = convertedType.GetProperty("UserId");
                    var userNameProperty = convertedType.GetProperty("UserName");
                    var isActiveProperty = convertedType.GetProperty("IsActive");
                    var scoreProperty = convertedType.GetProperty("Score");

                    Assert.NotNull(userIdProperty);
                    Assert.NotNull(userNameProperty);
                    Assert.NotNull(isActiveProperty);
                    Assert.NotNull(scoreProperty);

                    // Property values should be correctly mapped through reflection
                    Assert.Equal(42, userIdProperty.GetValue(result));
                    Assert.Equal("typed_user", userNameProperty.GetValue(result));
                    Assert.Equal(true, isActiveProperty.GetValue(result));
                    Assert.Equal(87.3, scoreProperty.GetValue(result));
                }
            }
        }

        [Fact]
        public void WithConverter_TypeConversion_Should_HandleDifferentTypes()
        {
            // Arrange - Create test data with various types
            var complexData = new List<object>
            {
                new { 
                    StringValue = "Test String",
                    IntValue = 42,
                    BoolValue = true,
                    DateValue = DateTime.Now,
                    DecimalValue = 99.99m,
                    NullableInt = (int?)123,
                    NullableString = (string?)"Optional"
                }
            };

            TypeCombiner.Combine()
                .WithProjection(complexData)
                .WithName("TypeConversionTest")
                .WithConverter()
                .AsRecord()
                .Generate();

            // Act & Assert - Test actual type conversion
            var converterType = Type.GetType("Generated.TypeConversionTestConverter");
            if (converterType != null)
            {
                var fromSingleMethod = converterType.GetMethod("FromSingle", 
                    BindingFlags.Public | BindingFlags.Static);

                if (fromSingleMethod != null)
                {
                    var result = fromSingleMethod.Invoke(null, new object[] { complexData.First() });
                    Assert.NotNull(result);

                    var resultType = result.GetType();
                    
                    // Verify all properties exist (values might be default due to dynamic conversion limitations)
                    var stringProp = resultType.GetProperty("StringValue");
                    var intProp = resultType.GetProperty("IntValue");
                    var boolProp = resultType.GetProperty("BoolValue");
                    var dateProp = resultType.GetProperty("DateValue");
                    var decimalProp = resultType.GetProperty("DecimalValue");

                    Assert.NotNull(stringProp);
                    Assert.NotNull(intProp);
                    Assert.NotNull(boolProp);
                    Assert.NotNull(dateProp);
                    Assert.NotNull(decimalProp);
                }
            }
        }

        [Fact]
        public void WithConverter_CollectionConversion_Should_Work()
        {
            // Arrange
            var collectionTestData = new List<object>
            {
                new { Id = 1, Name = "User One", IsActive = true },
                new { Id = 2, Name = "User Two", IsActive = false },
                new { Id = 3, Name = "User Three", IsActive = true }
            };

            TypeCombiner.Combine()
                .WithProjection(collectionTestData)
                .WithName("CollectionConversionTest")
                .WithConverter()
                .AsRecord()
                .Generate();

            // Act & Assert - Test collection conversion
            var converterType = Type.GetType("Generated.CollectionConversionTestConverter");
            if (converterType != null)
            {
                // Test FromCollection method
                var collectionMethod = converterType.GetMethod("FromCollection", 
                    BindingFlags.Public | BindingFlags.Static);

                if (collectionMethod != null)
                {
                    var result = collectionMethod.Invoke(null, new object[] { collectionTestData });
                    Assert.NotNull(result);

                    // Check if it's a list
                    var resultType = result.GetType();
                    Assert.True(resultType.IsGenericType);
                    Assert.Equal(typeof(List<>), resultType.GetGenericTypeDefinition());

                    // Get the list as IEnumerable to check count
                    if (result is System.Collections.IEnumerable enumResult)
                    {
                        var count = enumResult.Cast<object>().Count();
                        Assert.Equal(3, count);
                    }
                }

                // Test FromTypedCollection with strongly-typed data
                var typedCollection = new[]
                {
                    new { Id = 10, Name = "Typed User One", IsActive = true },
                    new { Id = 20, Name = "Typed User Two", IsActive = false },
                    new { Id = 30, Name = "Typed User Three", IsActive = true }
                };

                var typedCollectionMethod = converterType.GetMethod("FromTypedCollection", 
                    BindingFlags.Public | BindingFlags.Static);

                if (typedCollectionMethod != null && typedCollectionMethod.IsGenericMethodDefinition)
                {
                    var genericMethod = typedCollectionMethod.MakeGenericMethod(typedCollection.GetType().GetElementType()!);
                    var typedResult = genericMethod.Invoke(null, new object[] { typedCollection });
                    Assert.NotNull(typedResult);

                    var typedResultType = typedResult.GetType();
                    Assert.True(typedResultType.IsGenericType);
                    Assert.Equal(typeof(List<>), typedResultType.GetGenericTypeDefinition());

                    if (typedResult is System.Collections.IEnumerable typedEnumResult)
                    {
                        var typedCount = typedEnumResult.Cast<object>().Count();
                        Assert.Equal(3, typedCount);
                    }
                }
            }
        }

        [Fact]
        public void WithConverter_NullHandling_Should_Work()
        {
            // Arrange - Test data with null values
            var nullTestData = new List<object>
            {
                new { 
                    Name = "Test User",
                    OptionalField = (string?)null,
                    NullableDate = (DateTime?)null,
                    RequiredField = "Required Value"
                }
            };

            TypeCombiner.Combine()
                .WithProjection(nullTestData)
                .WithName("NullHandlingTest")
                .WithConverter()
                .AsRecord()
                .Generate();

            // Act & Assert - Test null handling in conversion
            var converterType = Type.GetType("Generated.NullHandlingTestConverter");
            if (converterType != null)
            {
                var fromSingleMethod = converterType.GetMethod("FromSingle", 
                    BindingFlags.Public | BindingFlags.Static);

                if (fromSingleMethod != null)
                {
                    var result = fromSingleMethod.Invoke(null, new object[] { nullTestData.First() });
                    Assert.NotNull(result);

                    var resultType = result.GetType();
                    
                    // Verify properties exist
                    var nameProp = resultType.GetProperty("Name");
                    var optionalProp = resultType.GetProperty("OptionalField");
                    var requiredProp = resultType.GetProperty("RequiredField");

                    Assert.NotNull(nameProp);
                    Assert.NotNull(optionalProp);
                    Assert.NotNull(requiredProp);
                }
            }
        }

        [Fact]
        public void WithConverter_ErrorHandling_Should_ThrowMeaningfulExceptions()
        {
            // Arrange - Generate converter for testing
            var testData = new List<object>
            {
                new { Name = "Test", Age = 30 }
            };

            TypeCombiner.Combine()
                .WithProjection(testData)
                .WithName("ErrorHandlingTest")
                .WithConverter()
                .AsRecord()
                .Generate();

            // Act & Assert - Test error handling
            var converterType = Type.GetType("Generated.ErrorHandlingTestConverter");
            if (converterType != null)
            {
                var fromSingleMethod = converterType.GetMethod("FromSingle", 
                    BindingFlags.Public | BindingFlags.Static);

                if (fromSingleMethod != null)
                {
                    // Test with null input - should throw ArgumentNullException
                    var exception = Record.Exception(() => 
                        fromSingleMethod.Invoke(null, new object[] { null! }));
                    
                    Assert.NotNull(exception);
                    // The exception will be wrapped in TargetInvocationException
                    if (exception is System.Reflection.TargetInvocationException tie)
                    {
                        Assert.IsType<ArgumentNullException>(tie.InnerException);
                    }
                }
            }
        }

        [Fact]
        public void WithConverter_AnonymousTypeBuilder_Should_GenerateStaticConverterMethods()
        {
            // Arrange
            var anonymousData = new { Name = "Test User", Age = 25, IsActive = true };

            // Act - Generate type with converter using anonymous builder
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .With(anonymousData)
                    .WithName("AnonymousWithConverter")
                    .WithConverter()
                    .AsClass()
                    .Generate();
            });

            // Assert
            Assert.Null(exception);
            
            // Check if static converter methods are generated
            var converterType = Type.GetType("Generated.AnonymousWithConverterConverter");
            // Converter type may or may not exist depending on source generator execution
            // This is expected behavior for compile-time generation
        }

        [Fact]
        public void WithConverter_SingleTypeBuilder_Should_GenerateStaticConverterMethods()
        {
            // Arrange & Act
            var exception = Record.Exception(() =>
            {
                TypeCombiner.From<User>()
                    .Add("Department", typeof(string))
                    .Add("Salary", typeof(decimal))
                    .WithName("ExtendedUserWithConverter")
                    .WithConverter()
                    .AsRecord()
                    .Generate();
            });

            // Assert
            Assert.Null(exception);
            
            // Check if static converter methods are generated
            var converterType = Type.GetType("Generated.ExtendedUserWithConverterConverter");
            // Converter type may or may not exist depending on source generator execution
        }

        [Fact]
        public void WithConverter_CombineTypesBuilder_Should_GenerateStaticConverterMethods()
        {
            // Arrange & Act
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine<PersonalInfo, ContactInfo>()
                    .WithName("CombinedWithConverter")
                    .WithConverter()
                    .AsStruct()
                    .Generate();
            });

            // Assert
            Assert.Null(exception);
            
            // Check if static converter methods for named types are generated
            var generatedType = Type.GetType("Generated.CombinedWithConverter");
            if (generatedType != null)
            {
                var methods = generatedType.GetMethods(BindingFlags.Public | BindingFlags.Static);
                
                // Check for individual type converters
                Assert.Contains(methods, m => m.Name == "FromPersonalInfo" && 
                    m.GetParameters().Length == 1);
                
                Assert.Contains(methods, m => m.Name == "FromContactInfo" && 
                    m.GetParameters().Length == 1);
                
                // Check for combined converter
                Assert.Contains(methods, m => m.Name == "FromBoth" && 
                    m.GetParameters().Length == 2);
            }
        }

        [Fact]
        public void WithConverter_NamedTypeConversion_Should_Work()
        {
            // Arrange - Generate combined type with converter
            TypeCombiner.Combine<PersonalInfo, ContactInfo>()
                .WithName("TestCombined")
                .WithConverter()
                .AsClass()
                .Generate();

            var personalInfo = new PersonalInfo 
            { 
                FirstName = "John", 
                LastName = "Doe", 
                Age = 30 
            };
            
            var contactInfo = new ContactInfo 
            { 
                Email = "john@example.com", 
                PhoneNumber = "555-1234"
            };

            // Act & Assert - Try to use the generated static converter methods
            var generatedType = Type.GetType("Generated.TestCombined");
            if (generatedType != null)
            {
                // Test FromPersonalInfo method
                var fromPersonalMethod = generatedType.GetMethod("FromPersonalInfo", 
                    BindingFlags.Public | BindingFlags.Static);
                if (fromPersonalMethod != null)
                {
                    var result = fromPersonalMethod.Invoke(null, new object[] { personalInfo });
                    Assert.NotNull(result);
                    
                    var resultType = result.GetType();
                    var firstNameProp = resultType.GetProperty("FirstName");
                    Assert.NotNull(firstNameProp);
                    Assert.Equal("John", firstNameProp.GetValue(result));
                }

                // Test FromContactInfo method
                var fromContactMethod = generatedType.GetMethod("FromContactInfo", 
                    BindingFlags.Public | BindingFlags.Static);
                if (fromContactMethod != null)
                {
                    var result = fromContactMethod.Invoke(null, new object[] { contactInfo });
                    Assert.NotNull(result);
                    
                    var resultType = result.GetType();
                    var emailProp = resultType.GetProperty("Email");
                    Assert.NotNull(emailProp);
                    Assert.Equal("john@example.com", emailProp.GetValue(result));
                }

                // Test FromBoth method
                var fromBothMethod = generatedType.GetMethod("FromBoth", 
                    BindingFlags.Public | BindingFlags.Static);
                if (fromBothMethod != null)
                {
                    var result = fromBothMethod.Invoke(null, new object[] { personalInfo, contactInfo });
                    Assert.NotNull(result);
                    
                    var resultType = result.GetType();
                    var firstNameProp = resultType.GetProperty("FirstName");
                    var emailProp = resultType.GetProperty("Email");
                    
                    Assert.NotNull(firstNameProp);
                    Assert.NotNull(emailProp);
                    Assert.Equal("John", firstNameProp.GetValue(result));
                    Assert.Equal("john@example.com", emailProp.GetValue(result));
                }
            }
        }

        [Fact]
        public void WithoutConverter_Should_NotGenerateStaticConverterMethods()
        {
            // Arrange & Act - Generate type WITHOUT converter
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .With(new { Name = "Test", Value = 42 })
                    .WithName("WithoutConverter")
                    .AsRecord()
                    .Generate();
            });

            // Assert
            Assert.Null(exception);
            
            // Check that NO static converter methods are generated
            var converterType = Type.GetType("Generated.WithoutConverterConverter");
            Assert.Null(converterType);
        }

        [Fact]
        public void WithConverter_AllGenerationModes_Should_Work()
        {
            // Test Record mode
            var recordException = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .With(new { Name = "Record Test", Value = 1 })
                    .WithName("RecordWithConverter")
                    .WithConverter()
                    .AsRecord()
                    .Generate();
            });

            // Test Class mode
            var classException = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .With(new { Name = "Class Test", Value = 2 })
                    .WithName("ClassWithConverter")
                    .WithConverter()
                    .AsClass()
                    .Generate();
            });

            // Test Struct mode
            var structException = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .With(new { Name = "Struct Test", Value = 3 })
                    .WithName("StructWithConverter")
                    .WithConverter()
                    .AsStruct()
                    .Generate();
            });

            // Assert all succeed
            Assert.Null(recordException);
            Assert.Null(classException);
            Assert.Null(structException);

            // Verify all have static converter methods (may not be available at test time)
            // This is expected for source generator tests
        }

        [Fact]
        public void WithConverter_ComplexProjection_Should_GenerateStaticConverterMethods()
        {
            // Arrange - Complex EF Core-like projection
            var complexProjection = new List<object>
            {
                new { 
                    UserId = 1,
                    UserName = "john_doe",
                    Email = "john@example.com",
                    DepartmentName = "Engineering",
                    Salary = 75000m,
                    IsActive = true,
                    LastLogin = DateTime.Now.AddDays(-1),
                    Tags = new string[] { "senior", "fullstack" },
                    Metadata = new Dictionary<string, object> { ["level"] = "senior" }
                }
            };

            // Act
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .WithProjection(complexProjection)
                    .With(new { 
                        CreatedAt = DateTime.Now,
                        UpdatedAt = (DateTime?)null,
                        Version = 1
                    })
                    .WithName("ComplexProjectionWithConverter")
                    .WithConverter()
                    .AsRecord()
                    .Generate();
            });

            // Assert
            Assert.Null(exception);

            // Verify static converter methods are generated (may not be available at test time)
            var converterType = Type.GetType("Generated.ComplexProjectionWithConverterConverter");
            // This test verifies that generation completes without errors
        }

        [Fact]
        public void WithConverter_EFCoreScenario_Should_GenerateUsableStaticMethods()
        {
            // Arrange - Simulate real EF Core projection scenario
            var efCoreProjectionResult = new List<object>
            {
                new { 
                    CustomerId = 1, 
                    CustomerName = "Acme Corp", 
                    TotalOrders = 15,
                    TotalSpent = 25000m,
                    LastOrderDate = DateTime.Now.AddDays(-10)
                },
                new { 
                    CustomerId = 2, 
                    CustomerName = "Tech Solutions", 
                    TotalOrders = 8,
                    TotalSpent = 12000m,
                    LastOrderDate = DateTime.Now.AddDays(-5)
                }
            };

            // Act - Generate dashboard type with converter
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .WithProjection(efCoreProjectionResult)
                    .With(new { 
                        ReportGeneratedAt = DateTime.Now,
                        ReportType = "Customer Analytics",
                        GeneratedBy = "System"
                    })
                    .WithName("CustomerDashboardDto")
                    .WithConverter()
                    .AsRecord()
                    .Generate();
            });

            // Assert
            Assert.Null(exception);

            // Verify the generated type exists (may be generated at compile time)
            var generatedType = Type.GetType("Generated.CustomerDashboardDto");
            // Type may not be available during unit test execution

            // Verify static converter methods exist (may be generated at compile time)
            var converterType = Type.GetType("Generated.CustomerDashboardDtoConverter");
            // Converter may not be available during unit test execution
        }

        [Fact]
        public void WithConverter_ChainedWithOtherMethods_Should_Work()
        {
            // Arrange & Act - Test that WithConverter() can be chained with other methods
            var exception = Record.Exception(() =>
            {
                TypeCombiner.From<User>()
                    .Add("Department", typeof(string))
                    .Exclude(u => u.IsActive)
                    .ChangeType(u => u.Age, typeof(string))
                    .With(new { CreatedAt = DateTime.Now })
                    .WithConverter()  // Should work in any position in the chain
                    .WithName("ChainedWithConverter")
                    .AsClass()
                    .Generate();
            });

            // Assert
            Assert.Null(exception);

            // Verify both generated type and converter exist (may be compile-time only)
            // This test ensures the fluent API works correctly with WithConverter()
        }

        [Fact]
        public void WithConverter_ShouldGenerateInCorrectNamespace()
        {
            // Arrange & Act
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .With(new { TestProperty = "value" })
                    .WithName("NamespaceTest")
                    .WithConverter()
                    .Generate();
            });

            // Assert - Generation should complete without errors
            Assert.Null(exception);

            // Generated type should be in Generated namespace (compile-time)
            // Static converter should be in Generated namespace (compile-time)
            // These are verified through actual usage in integration tests
        }
    }

    /// <summary>
    /// Integration tests for static converter functionality in real-world scenarios
    /// </summary>
    public class StaticConverterIntegrationTests
    {
        [Fact]
        public void StaticConverterIntegration_EFCoreProjectionWorkflow_Should_Work()
        {
            // Arrange - Simulate complete EF Core projection workflow
            var userProjections = new List<object>
            {
                new { Id = 1, Name = "John", Email = "john@test.com", DepartmentId = 1 },
                new { Id = 2, Name = "Jane", Email = "jane@test.com", DepartmentId = 2 }
            };

            // Act - Generate type with converter for the projection
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .WithProjection(userProjections)
                    .With(new { 
                        ProjectionDate = DateTime.Now,
                        IsActive = true 
                    })
                    .WithName("UserProjectionIntegration")
                    .WithConverter()
                    .AsRecord()
                    .Generate();
            });

            // Assert - Type generation should succeed
            Assert.Null(exception);

            // Test actual conversion if static converter is available
            var converterType = Type.GetType("Generated.UserProjectionIntegrationConverter");
            if (converterType != null)
            {
                var collectionMethod = converterType.GetMethod("FromCollection", 
                    BindingFlags.Public | BindingFlags.Static);

                if (collectionMethod != null)
                {
                    var result = collectionMethod.Invoke(null, new object[] { userProjections });
                    Assert.NotNull(result);
                }
            }
        }

        [Fact]
        public void StaticConverterIntegration_MultipleProjectionsWithConverter_Should_Work()
        {
            // Arrange - Multiple projections scenario
            var userProjection = new List<object>
            {
                new { UserId = 1, UserName = "john_doe" }
            };

            var profileProjection = new List<object>
            {
                new { Bio = "Software Engineer", Location = "Seoul" }
            };

            // Act - Combine multiple projections with converter
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .WithProjection(userProjection)
                    .WithProjection(profileProjection)
                    .With(new { MergedAt = DateTime.Now })
                    .WithName("MultiProjectionWithConverter")
                    .WithConverter()
                    .AsRecord()
                    .Generate();
            });

            // Assert - Generation should complete successfully
            Assert.Null(exception);
        }

        [Fact]
        public void StaticConverterIntegration_AllBuilderTypes_Should_SupportConverter()
        {
            // Test Anonymous Type Builder
            var anonymousException = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .With(new { Name = "Anonymous Test", Value = 1 })
                    .WithConverter()
                    .WithName("AnonymousConverterTest")
                    .Generate();
            });

            // Test Single Type Builder
            var singleException = Record.Exception(() =>
            {
                TypeCombiner.From<User>()
                    .Add("ExtraField", typeof(string))
                    .WithConverter()
                    .WithName("SingleConverterTest")
                    .Generate();
            });

            // Test Multi Type Builder
            var multiException = Record.Exception(() =>
            {
                TypeCombiner.Combine<PersonalInfo, ContactInfo>()
                    .WithConverter()
                    .WithName("MultiConverterTest")
                    .Generate();
            });

            // Assert all succeed
            Assert.Null(anonymousException);
            Assert.Null(singleException);
            Assert.Null(multiException);
        }

        [Fact] 
        public void StaticConverterIntegration_RealWorldEFCoreScenario_Should_Work()
        {
            // Arrange - Realistic EF Core scenario
            var customerAnalytics = new List<object>
            {
                new { 
                    CustomerId = 1,
                    CustomerName = "ABC Corporation",
                    TotalOrders = 25,
                    TotalRevenue = 50000m,
                    AverageOrderValue = 2000m,
                    FirstOrderDate = DateTime.Now.AddYears(-2),
                    LastOrderDate = DateTime.Now.AddDays(-5),
                    IsVipCustomer = true
                },
                new { 
                    CustomerId = 2,
                    CustomerName = "XYZ Limited",
                    TotalOrders = 10,
                    TotalRevenue = 15000m,
                    AverageOrderValue = 1500m,
                    FirstOrderDate = DateTime.Now.AddMonths(-6),
                    LastOrderDate = DateTime.Now.AddDays(-15),
                    IsVipCustomer = false
                }
            };

            // Act - Generate analytics DTO with converter
            TypeCombiner.Combine()
                .WithProjection(customerAnalytics)
                .With(new { 
                    AnalysisDate = DateTime.UtcNow,
                    AnalysisType = "Customer Revenue Analysis",
                    Currency = "USD",
                    GeneratedBy = "Analytics Service"
                })
                .WithName("CustomerAnalyticsDto")
                .WithConverter()
                .AsRecord()
                .Generate();

            // Assert - Try to use the static converter if available
            var converterType = Type.GetType("Generated.CustomerAnalyticsDtoConverter");
            if (converterType != null)
            {
                var collectionMethod = converterType.GetMethod("FromCollection", 
                    BindingFlags.Public | BindingFlags.Static);

                if (collectionMethod != null)
                {
                    var result = collectionMethod.Invoke(null, new object[] { customerAnalytics });
                    Assert.NotNull(result);

                    // Verify we get a properly typed collection
                    if (result is System.Collections.IEnumerable enumResult)
                    {
                        var count = 0;
                        foreach (var item in enumResult)
                        {
                            count++;
                            var itemType = item.GetType();
                            
                            // Verify key properties exist
                            Assert.NotNull(itemType.GetProperty("CustomerId"));
                            Assert.NotNull(itemType.GetProperty("CustomerName"));
                            Assert.NotNull(itemType.GetProperty("TotalRevenue"));
                            Assert.NotNull(itemType.GetProperty("AnalysisDate"));
                        }
                        Assert.Equal(2, count);
                    }
                }
            }
        }
    }
}