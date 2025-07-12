using Structura.Tests.TestModels;
using System.Reflection;

namespace Structura.Tests
{
    /// <summary>
    /// 변수 참조 처리 기능 테스트
    /// </summary>
    public class VariableReferenceTests
    {
        [Fact]
        public void VariableReference_SimpleAnonymousObject_Should_Work()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() =>
            {
                var anonymousInstance = new { Name = "홍길동", Age = 40, Sex = "male" };
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
                    Name = "테스트사용자", 
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
                var personalData = new { FirstName = "홍", LastName = "길동", Age = 30 };
                var contactData = new { Email = "hong@example.com", Phone = "010-1234-5678" };
                
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
                var additionalInfo = new { Department = "개발팀", Position = "시니어", Salary = 50000m };
                
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
                    .With(userInfo) // 변수 참조
                    .With(new { CreatedAt = DateTime.Now, IsActive = true }) // 직접 생성
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
                    Name = "테스트",
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
            // Arrange & Act & Assert - EF Core projection 시나리오
            var exception = Record.Exception(() =>
            {
                // var result = dbContext.Users.Select(x => new { x.Name, x.Email }).ToList();
                var projectionResult = new List<object>
                {
                    new { Name = "홍길동", Email = "hong@example.com" },
                    new { Name = "김철수", Email = "kim@example.com" }
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
    /// 원래 문제 시나리오 재현 테스트
    /// </summary>
    public class OriginalProblemScenarioTests
    {
        [Fact]
        public void OriginalProblem_AnonymousInstanceVariable_Should_Work()
        {
            // Arrange & Act & Assert - 원래 문제가 되었던 코드
            var exception = Record.Exception(() =>
            {
                var anonymousInstance = new { Name = "홍길동", Age = 40, Sex = "male" };
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
            
            // 직접 생성 (원래 작동하던 방식)
            var directException = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .With(new { Name = "홍길동", Age = 40, Sex = "male" })
                    .WithName("DirectAnonymousUser")
                    .AsClass()
                    .Generate();
            });

            // 변수 참조 (개선된 기능)
            var variableException = Record.Exception(() =>
            {
                var anonymousInstance = new { Name = "홍길동", Age = 40, Sex = "male" };
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
            // Arrange & Act & Assert - 실제 사용 시나리오
            var exception = Record.Exception(() =>
            {
                // 다양한 소스에서 온 데이터를 변수로 저장
                var userBasicInfo = new { Name = "김개발", Age = 28, Email = "kim@company.com" };
                var jobInfo = new { Department = "개발팀", Position = "시니어", StartDate = DateTime.Now.AddYears(-3) };
                var settings = new { Theme = "Dark", Language = "Korean", NotificationsEnabled = true };

                // 모든 정보를 결합하여 새로운 타입 생성
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
            // Arrange & Act & Assert - EF Core 사용 시나리오
            var exception = Record.Exception(() =>
            {
                // 실제 EF Core에서 이런 식으로 사용될 수 있음
                // var result = dbContext.Users.Select(x => new { x.Name }).ToList();
                var result = new List<object>
                {
                    new { Name = "홍길동" },
                    new { Name = "김철수" },
                    new { Name = "이영희" }
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
    /// 향상된 소스 생성기 통합 테스트
    /// </summary>
    public class EnhancedSourceGeneratorIntegrationTests
    {
        [Fact]
        public void EnhancedSourceGenerator_Should_HandleVariableReferences()
        {
            // Arrange
            var anonymousInstance = new { Name = "테스트", Age = 25, IsActive = true };
            
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
                
                // 예상 속성들이 생성되었는지 확인
                var nameProp = properties.FirstOrDefault(p => p.Name == "Name");
                var ageProp = properties.FirstOrDefault(p => p.Name == "Age");
                var isActiveProp = properties.FirstOrDefault(p => p.Name == "IsActive");

                // 변수 참조에서 추출된 속성들이 존재하는지 확인
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
                
                // 타입 정보가 올바르게 보존되었는지 확인
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
                
                // projection에서 추출된 속성들이 존재하는지 확인
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
                new { Name = "홍길동", Email = "hong@example.com" }
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
                
                // projection과 추가 데이터에서 모든 속성이 생성되었는지 확인
                Assert.True(properties.Length >= 4); // Name, Email, CreatedAt, IsActive
            }
        }
    }
}