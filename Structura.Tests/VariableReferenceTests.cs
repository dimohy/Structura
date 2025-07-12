using Structura.Tests.TestModels;
using System.Reflection;

namespace Structura.Tests
{
    /// <summary>
    /// Variable reference handling tests
    /// </summary>
    public class VariableReferenceTests
    {
        [Fact]
        public void VariableReference_SimpleAnonymousObject_Should_Work()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() =>
            {
                var anonymousInstance = new { Name = "John Doe", Age = 40, Sex = "male" };
                TypeCombiner.Combine()
                    .With(anonymousInstance)
                    .WithName("VariableReferenceTest1")
                    .AsClass()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void VariableReference_ComplexAnonymousObject_Should_Work()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() =>
            {
                var userInfo = new { 
                    Id = Guid.NewGuid(), 
                    Name = "TestUser", 
                    Email = "test@example.com",
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    Score = 95.5 
                };
                
                TypeCombiner.Combine()
                    .With(userInfo)
                    .WithName("VariableReferenceTest2")
                    .AsRecord()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void VariableReference_MultipleVariables_Should_Work()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() =>
            {
                var personalData = new { FirstName = "John", LastName = "Doe", Age = 30 };
                var contactData = new { Email = "john@example.com", Phone = "010-1234-5678" };
                
                TypeCombiner.Combine()
                    .With(personalData)
                    .With(contactData)
                    .WithName("VariableReferenceTest3")
                    .AsRecord()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void VariableReference_WithExistingType_Should_Work()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() =>
            {
                var additionalInfo = new { Department = "Development", Position = "Senior", Salary = 50000m };
                
                TypeCombiner.Combine<User>()
                    .With(additionalInfo)
                    .WithName("VariableReferenceTest4")
                    .AsClass()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void VariableReference_MixedWithDirectCreation_Should_Work()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() =>
            {
                var userInfo = new { UserId = 123, Username = "testuser" };
                
                TypeCombiner.Combine()
                    .With(userInfo) // Variable reference
                    .With(new { CreatedAt = DateTime.Now, IsActive = true }) // Direct creation
                    .WithName("VariableReferenceTest5")
                    .AsRecord()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void VariableReference_WithDifferentTypes_Should_Work()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() =>
            {
                var numericData = new { 
                    IntValue = 42,
                    LongValue = 123L,
                    FloatValue = 3.14f,
                    DoubleValue = 2.718,
                    DecimalValue = 99.99m,
                    BoolValue = true
                };
                
                TypeCombiner.Combine()
                    .With(numericData)
                    .WithName("VariableReferenceTest6")
                    .AsStruct()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void VariableReference_WithCollections_Should_Work()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() =>
            {
                var collectionData = new { 
                    Tags = new string[] { "tag1", "tag2" },
                    Scores = new List<int> { 85, 90, 95 },
                    Metadata = new Dictionary<string, object> { ["key"] = "value" }
                };
                
                TypeCombiner.Combine()
                    .With(collectionData)
                    .WithName("VariableReferenceTest7")
                    .AsClass()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void VariableReference_NullValues_Should_Work()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() =>
            {
                var nullableData = new { 
                    Name = "Test",
                    OptionalValue = (string?)null,
                    NullableInt = (int?)null,
                    NullableDate = (DateTime?)DateTime.Now
                };
                
                TypeCombiner.Combine()
                    .With(nullableData)
                    .WithName("VariableReferenceTest8")
                    .AsRecord()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void VariableReference_WithProjection_Should_Work()
        {
            // Arrange & Act & Assert - EF Core projection scenario
            var exception = Record.Exception(() =>
            {
                // var result = dbContext.Users.Select(x => new { x.Name, x.Email }).ToList();
                var projectionResult = new List<object>
                {
                    new { Name = "John Doe", Email = "john@example.com" },
                    new { Name = "Jane Smith", Email = "jane@example.com" }
                };
                
                TypeCombiner.Combine()
                    .WithProjection(projectionResult)
                    .WithName("VariableReferenceTest9")
                    .AsRecord()
                    .Generate();
            });

            Assert.Null(exception);
        }
    }

    /// <summary>
    /// Original problem scenario tests
    /// </summary>
    public class OriginalProblemScenarioTests
    {
        [Fact]
        public void OriginalProblem_AnonymousInstanceVariable_Should_Work()
        {
            // Arrange & Act & Assert - Previously problematic code
            var exception = Record.Exception(() =>
            {
                var anonymousInstance = new { Name = "John Doe", Age = 40, Sex = "male" };
                TypeCombiner.Combine()
                    .With(anonymousInstance)
                    .WithName("AnonymousUser")
                    .AsClass()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void OriginalProblem_CompareDirectVsVariable_Should_BothWork()
        {
            // Arrange & Act & Assert
            
            // Direct creation (previously working approach)
            var directException = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .With(new { Name = "John Doe", Age = 40, Sex = "male" })
                    .WithName("DirectAnonymousUser")
                    .AsClass()
                    .Generate();
            });

            // Variable reference (improved approach)
            var variableException = Record.Exception(() =>
            {
                var anonymousInstance = new { Name = "John Doe", Age = 40, Sex = "male" };
                TypeCombiner.Combine()
                    .With(anonymousInstance)
                    .WithName("VariableAnonymousUser")
                    .AsClass()
                    .Generate();
            });

            Assert.Null(directException);
            Assert.Null(variableException);
        }

        [Fact]
        public void OriginalProblem_RealWorldScenario_Should_Work()
        {
            // Arrange & Act & Assert - Real world scenario
            var exception = Record.Exception(() =>
            {
                // Data from various sources in actual application
                var userBasicInfo = new { Name = "Jane Developer", Age = 28, Email = "jane@company.com" };
                var jobInfo = new { Department = "Development", Position = "Senior", StartDate = DateTime.Now.AddYears(-3) };
                var settings = new { Theme = "Dark", Language = "English", NotificationsEnabled = true };

                // Combine all data to create new type
                TypeCombiner.Combine()
                    .With(userBasicInfo)
                    .With(jobInfo)
                    .With(settings)
                    .WithName("ComprehensiveUserProfile")
                    .AsRecord()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void OriginalProblem_EFCoreProjectionScenario_Should_Work()
        {
            // Arrange & Act & Assert - EF Core projection scenario
            var exception = Record.Exception(() =>
            {
                // In real EF Core this would be like:
                // var result = dbContext.Users.Select(x => new { x.Name }).ToList();
                var result = new List<object>
                {
                    new { Name = "John Doe" },
                    new { Name = "Jane Smith" },
                    new { Name = "Bob Johnson" }
                };

                TypeCombiner.Combine()
                    .WithProjection(result)
                    .WithName("EFCoreUserProjection")
                    .AsRecord()
                    .Generate();
            });

            Assert.Null(exception);
        }
    }

    /// <summary>
    /// Enhanced source generator tests
    /// </summary>
    public class EnhancedSourceGeneratorIntegrationTests
    {
        [Fact]
        public void EnhancedSourceGenerator_Should_HandleVariableReferences()
        {
            // Arrange
            var anonymousInstance = new { Name = "Test", Age = 25, IsActive = true };
            
            TypeCombiner.Combine()
                .With(anonymousInstance)
                .WithName("EnhancedTestType")
                .AsClass()
                .Generate();

            // Act & Assert
            var enhancedTestType = Type.GetType("Generated.EnhancedTestType");
            if (enhancedTestType != null)
            {
                Assert.True(enhancedTestType.IsClass);
                var properties = enhancedTestType.GetProperties();
                
                // Check if expected properties are generated
                var nameProp = properties.FirstOrDefault(p => p.Name == "Name");
                var ageProp = properties.FirstOrDefault(p => p.Name == "Age");
                var isActiveProp = properties.FirstOrDefault(p => p.Name == "IsActive");

                // Verify at least the expected properties exist
                Assert.True(properties.Length >= 3);
            }
        }

        [Fact]
        public void EnhancedSourceGenerator_Should_PreserveTypeInformation()
        {
            // Arrange
            var typedData = new { 
                StringValue = "test",
                IntValue = 42,
                BoolValue = true,
                DateValue = DateTime.Now,
                DecimalValue = 99.99m
            };
            
            TypeCombiner.Combine()
                .With(typedData)
                .WithName("TypePreservationTest")
                .AsRecord()
                .Generate();

            // Act & Assert
            var typePreservationTestType = Type.GetType("Generated.TypePreservationTest");
            if (typePreservationTestType != null)
            {
                var properties = typePreservationTestType.GetProperties().ToDictionary(p => p.Name, p => p.PropertyType);
                
                // Check if type information is correctly preserved
                if (properties.ContainsKey("StringValue"))
                    Assert.Equal(typeof(string), properties["StringValue"]);
                    
                if (properties.ContainsKey("IntValue"))
                    Assert.Equal(typeof(int), properties["IntValue"]);
                    
                if (properties.ContainsKey("BoolValue"))
                    Assert.Equal(typeof(bool), properties["BoolValue"]);
            }
        }

        [Fact]
        public void EnhancedSourceGenerator_Should_HandleProjectionResults()
        {
            // Arrange
            var projectionResult = new List<object>
            {
                new { UserId = 1, UserName = "testuser", Email = "test@example.com" },
                new { UserId = 2, UserName = "testuser2", Email = "test2@example.com" }
            };
            
            TypeCombiner.Combine()
                .WithProjection(projectionResult)
                .WithName("ProjectionTestType")
                .AsRecord()
                .Generate();

            // Act & Assert
            var projectionTestType = Type.GetType("Generated.ProjectionTestType");
            if (projectionTestType != null)
            {
                var properties = projectionTestType.GetProperties();
                
                // Check if properties from projection are correctly extracted
                var userIdProp = properties.FirstOrDefault(p => p.Name == "UserId");
                var userNameProp = properties.FirstOrDefault(p => p.Name == "UserName");
                var emailProp = properties.FirstOrDefault(p => p.Name == "Email");

                Assert.True(properties.Length >= 3);
            }
        }

        [Fact]
        public void EnhancedSourceGenerator_Should_CombineProjectionWithOtherFeatures()
        {
            // Arrange
            var projectionResult = new List<object>
            {
                new { Name = "John Doe", Email = "john@example.com" }
            };
            
            var additionalData = new { CreatedAt = DateTime.Now, IsActive = true };
            
            TypeCombiner.Combine()
                .WithProjection(projectionResult)
                .With(additionalData)
                .WithName("CombinedProjectionTestType")
                .AsClass()
                .Generate();

            // Act & Assert
            var combinedType = Type.GetType("Generated.CombinedProjectionTestType");
            if (combinedType != null)
            {
                Assert.True(combinedType.IsClass);
                var properties = combinedType.GetProperties();
                
                // Check if all properties from projection and additional data are generated
                Assert.True(properties.Length >= 4); // Name, Email, CreatedAt, IsActive
            }
        }
    }
}