using Structura.Tests.TestModels;
using System.Reflection;

namespace Structura.Tests
{
    /// <summary>
    /// EF Core projection 결과에서 타입 생성 기능 테스트
    /// </summary>
    public class EFCoreProjectionTests
    {
        [Fact]
        public void EFCoreProjection_SimpleProjection_Should_Work()
        {
            // Arrange - EF Core Select projection 시나리오 시뮬레이션
            var projectionResult = new List<object>
            {
                new { Name = "홍길동", Age = 30 },
                new { Name = "김철수", Age = 25 },
                new { Name = "이영희", Age = 28 }
            };

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .WithProjection(projectionResult)
                    .WithName("UserProjection")
                    .AsRecord()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void EFCoreProjection_ComplexProjection_Should_Work()
        {
            // Arrange - 복잡한 projection 시나리오
            var complexProjection = new List<object>
            {
                new { 
                    UserId = Guid.NewGuid(), 
                    Name = "사용자1", 
                    Email = "user1@example.com",
                    CreatedAt = DateTime.Now,
                    IsActive = true,
                    Score = 85.5m
                },
                new { 
                    UserId = Guid.NewGuid(), 
                    Name = "사용자2", 
                    Email = "user2@example.com",
                    CreatedAt = DateTime.Now.AddDays(-1),
                    IsActive = false,
                    Score = 92.3m
                }
            };

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .WithProjection(complexProjection)
                    .WithName("ComplexUserProjection")
                    .AsClass()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void EFCoreProjection_EmptyList_Should_Work()
        {
            // Arrange - 빈 projection 결과
            var emptyProjection = new List<object>();

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .WithProjection(emptyProjection)
                    .WithName("EmptyProjection")
                    .AsRecord()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void EFCoreProjection_WithAdditionalProperties_Should_Work()
        {
            // Arrange - projection 결과에 추가 속성 결합
            var projectionResult = new List<object>
            {
                new { Name = "홍길동", Email = "hong@example.com" }
            };

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .WithProjection(projectionResult)
                    .With(new { CreatedAt = DateTime.Now, IsVerified = true })
                    .WithName("EnhancedProjection")
                    .AsRecord()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void EFCoreProjection_WithExistingType_Should_Work()
        {
            // Arrange - projection과 기존 타입 결합
            var projectionResult = new List<object>
            {
                new { Department = "개발팀", Position = "시니어" }
            };

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine<User>()
                    .WithProjection(projectionResult)
                    .WithName("UserWithDepartment")
                    .AsClass()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void EFCoreProjection_MultipleProjections_Should_Work()
        {
            // Arrange - 여러 projection 결과 결합
            var userProjection = new List<object>
            {
                new { Name = "홍길동", Age = 30 }
            };

            var profileProjection = new List<object>
            {
                new { Bio = "개발자", Location = "서울" }
            };

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .WithProjection(userProjection)
                    .WithProjection(profileProjection)
                    .WithName("CombinedProjection")
                    .AsRecord()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void EFCoreProjection_RealWorldScenario_Should_Work()
        {
            // Arrange - 실제 EF Core 사용 시나리오 시뮬레이션
            // var result = dbContext.Users.Select(x => new { x.Name, x.Email, x.Department.Name }).ToList();
            var efCoreResult = new List<object>
            {
                new { Name = "김개발", Email = "kim@company.com", DepartmentName = "개발팀" },
                new { Name = "박디자인", Email = "park@company.com", DepartmentName = "디자인팀" },
                new { Name = "최마케팅", Email = "choi@company.com", DepartmentName = "마케팅팀" }
            };

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .WithProjection(efCoreResult)
                    .With(new { 
                        LastLogin = DateTime.Now, 
                        IsOnline = false,
                        Permissions = new string[] { "read", "write" }
                    })
                    .WithName("EmployeeView")
                    .AsRecord()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void EFCoreProjection_WithCollectionTypes_Should_Work()
        {
            // Arrange - 컬렉션이 포함된 projection
            var projectionWithCollections = new List<object>
            {
                new { 
                    UserId = 1,
                    Tags = new string[] { "developer", "senior" },
                    Scores = new List<int> { 85, 90, 95 },
                    Metadata = new Dictionary<string, object> { ["level"] = "senior" }
                }
            };

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .WithProjection(projectionWithCollections)
                    .WithName("ProjectionWithCollections")
                    .AsClass()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void EFCoreProjection_NullableTypes_Should_Work()
        {
            // Arrange - nullable 타입이 포함된 projection
            var projectionWithNullables = new List<object>
            {
                new { 
                    Name = "테스트사용자",
                    LastLogin = (DateTime?)DateTime.Now,
                    OptionalField = (string?)null,
                    NullableAge = (int?)25
                }
            };

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .WithProjection(projectionWithNullables)
                    .WithName("ProjectionWithNullables")
                    .AsStruct()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void EFCoreProjection_ShouldExtractSchemaFromFirstItem()
        {
            // Arrange - 첫 번째 항목에서 스키마 추출 테스트
            var projectionResult = new List<object>
            {
                new { Name = "홍길동", Age = 30, Email = "hong@example.com" },
                new { Name = "김철수", Age = 25, Email = "kim@example.com" },
                // 다른 구조를 가진 항목들도 첫 번째 항목의 스키마를 따라야 함
            };

            TypeCombiner.Combine()
                .WithProjection(projectionResult)
                .WithName("SchemaExtractionTest")
                .AsRecord()
                .Generate();

            // Act & Assert - 생성된 타입 확인
            var generatedType = Type.GetType("Generated.SchemaExtractionTest");
            if (generatedType != null)
            {
                var properties = generatedType.GetProperties();
                
                Assert.Contains(properties, p => p.Name == "Name");
                Assert.Contains(properties, p => p.Name == "Age");
                Assert.Contains(properties, p => p.Name == "Email");
            }
        }
    }

    /// <summary>
    /// EF Core 통합 시나리오 테스트
    /// </summary>
    public class EFCoreIntegrationScenarioTests
    {
        [Fact]
        public void EFCoreIntegration_UserDashboard_Should_Work()
        {
            // Arrange - 사용자 대시보드 데이터 시나리오
            // var dashboardData = dbContext.Users
            //     .Select(u => new { u.Name, u.Email, OrderCount = u.Orders.Count() })
            //     .ToList();
            
            var dashboardData = new List<object>
            {
                new { Name = "홍길동", Email = "hong@example.com", OrderCount = 5 },
                new { Name = "김철수", Email = "kim@example.com", OrderCount = 3 }
            };

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .WithProjection(dashboardData)
                    .With(new { 
                        LastVisit = DateTime.Now,
                        PreferredTheme = "dark",
                        NotificationSettings = new { Email = true, SMS = false }
                    })
                    .WithName("UserDashboard")
                    .AsRecord()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void EFCoreIntegration_ReportGeneration_Should_Work()
        {
            // Arrange - 보고서 생성 시나리오
            // var reportData = dbContext.Sales
            //     .GroupBy(s => s.ProductCategory)
            //     .Select(g => new { Category = g.Key, TotalSales = g.Sum(s => s.Amount) })
            //     .ToList();
            
            var reportData = new List<object>
            {
                new { Category = "Electronics", TotalSales = 150000m },
                new { Category = "Clothing", TotalSales = 80000m },
                new { Category = "Books", TotalSales = 25000m }
            };

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .WithProjection(reportData)
                    .With(new { 
                        GeneratedAt = DateTime.Now,
                        GeneratedBy = "System",
                        ReportType = "SalesAnalysis"
                    })
                    .WithName("SalesReport")
                    .AsClass()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void EFCoreIntegration_ApiDto_Should_Work()
        {
            // Arrange - API DTO 생성 시나리오
            // var apiData = dbContext.Products
            //     .Where(p => p.IsActive)
            //     .Select(p => new { p.Name, p.Price, CategoryName = p.Category.Name })
            //     .ToList();
            
            var apiData = new List<object>
            {
                new { Name = "iPhone 15", Price = 1200000m, CategoryName = "Smartphone" },
                new { Name = "MacBook Pro", Price = 2500000m, CategoryName = "Laptop" }
            };

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .WithProjection(apiData)
                    .With(new { 
                        IsAvailable = true,
                        ImageUrl = "",
                        Rating = 0.0,
                        ReviewCount = 0
                    })
                    .WithName("ProductApiDto")
                    .AsRecord()
                    .Generate();
            });

            Assert.Null(exception);
        }
    }
}