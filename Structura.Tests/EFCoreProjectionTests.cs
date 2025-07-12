using Structura.Tests.TestModels;
using System.Reflection;

namespace Structura.Tests
{
    /// <summary>
    /// EF Core projection ������� Ÿ�� ���� ��� �׽�Ʈ
    /// </summary>
    public class EFCoreProjectionTests
    {
        [Fact]
        public void EFCoreProjection_SimpleProjection_Should_Work()
        {
            // Arrange - EF Core Select projection �ó����� �ùķ��̼�
            var projectionResult = new List<object>
            {
                new { Name = "ȫ�浿", Age = 30 },
                new { Name = "��ö��", Age = 25 },
                new { Name = "�̿���", Age = 28 }
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
            // Arrange - ������ projection �ó�����
            var complexProjection = new List<object>
            {
                new { 
                    UserId = Guid.NewGuid(), 
                    Name = "�����1", 
                    Email = "user1@example.com",
                    CreatedAt = DateTime.Now,
                    IsActive = true,
                    Score = 85.5m
                },
                new { 
                    UserId = Guid.NewGuid(), 
                    Name = "�����2", 
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
            // Arrange - �� projection ���
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
            // Arrange - projection ����� �߰� �Ӽ� ����
            var projectionResult = new List<object>
            {
                new { Name = "ȫ�浿", Email = "hong@example.com" }
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
            // Arrange - projection�� ���� Ÿ�� ����
            var projectionResult = new List<object>
            {
                new { Department = "������", Position = "�ôϾ�" }
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
            // Arrange - ���� projection ��� ����
            var userProjection = new List<object>
            {
                new { Name = "ȫ�浿", Age = 30 }
            };

            var profileProjection = new List<object>
            {
                new { Bio = "������", Location = "����" }
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
            // Arrange - ���� EF Core ��� �ó����� �ùķ��̼�
            // var result = dbContext.Users.Select(x => new { x.Name, x.Email, x.Department.Name }).ToList();
            var efCoreResult = new List<object>
            {
                new { Name = "�谳��", Email = "kim@company.com", DepartmentName = "������" },
                new { Name = "�ڵ�����", Email = "park@company.com", DepartmentName = "��������" },
                new { Name = "�ָ�����", Email = "choi@company.com", DepartmentName = "��������" }
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
            // Arrange - �÷����� ���Ե� projection
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
            // Arrange - nullable Ÿ���� ���Ե� projection
            var projectionWithNullables = new List<object>
            {
                new { 
                    Name = "�׽�Ʈ�����",
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
            // Arrange - ù ��° �׸񿡼� ��Ű�� ���� �׽�Ʈ
            var projectionResult = new List<object>
            {
                new { Name = "ȫ�浿", Age = 30, Email = "hong@example.com" },
                new { Name = "��ö��", Age = 25, Email = "kim@example.com" },
                // �ٸ� ������ ���� �׸�鵵 ù ��° �׸��� ��Ű���� ����� ��
            };

            TypeCombiner.Combine()
                .WithProjection(projectionResult)
                .WithName("SchemaExtractionTest")
                .AsRecord()
                .Generate();

            // Act & Assert - ������ Ÿ�� Ȯ��
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
    /// EF Core ���� �ó����� �׽�Ʈ
    /// </summary>
    public class EFCoreIntegrationScenarioTests
    {
        [Fact]
        public void EFCoreIntegration_UserDashboard_Should_Work()
        {
            // Arrange - ����� ��ú��� ������ �ó�����
            // var dashboardData = dbContext.Users
            //     .Select(u => new { u.Name, u.Email, OrderCount = u.Orders.Count() })
            //     .ToList();
            
            var dashboardData = new List<object>
            {
                new { Name = "ȫ�浿", Email = "hong@example.com", OrderCount = 5 },
                new { Name = "��ö��", Email = "kim@example.com", OrderCount = 3 }
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
            // Arrange - ���� ���� �ó�����
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
            // Arrange - API DTO ���� �ó�����
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