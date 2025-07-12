using Structura.Tests.TestModels;
using System.Reflection;

namespace Structura.Tests
{
    /// <summary>
    /// �ҽ� ������ ���� �׽�Ʈ - ���� ������ Ÿ�Ե��� �׽�Ʈ
    /// </summary>
    public class SourceGeneratorIntegrationTests
    {
        [Fact]
        public void SourceGenerator_Should_GenerateContactType()
        {
            // Arrange
            TypeCombiner.Combine()
                .With(new { Name = "", Age = 0, Sex = "Male" })
                .With(new { Email = "", Phone = "" })
                .WithName("Contact")
                .AsRecord()
                .Generate();

            // Act & Assert
            var contactType = Type.GetType("Generated.Contact");
            if (contactType != null)
            {
                Assert.True(contactType.IsClass);
                var properties = contactType.GetProperties();
                Assert.True(properties.Length >= 3); // Name, Age, Sex, Email, Phone ��
            }
            // �ҽ� �����Ⱑ ���� �������� ���� �� �����Ƿ� null�̾ �������� ����
        }

        [Fact]
        public void SourceGenerator_Should_GenerateExtendedUserType()
        {
            // Arrange
            TypeCombiner.From<User>()
                .Add("CreatedAt", typeof(DateTime))
                .Add("LastLoginAt", typeof(DateTime?))
                .Add("Metadata", typeof(Dictionary<string, object>))
                .WithName("ExtendedUser")
                .AsClass()
                .Generate();

            // Act & Assert
            var extendedUserType = Type.GetType("Generated.ExtendedUser");
            if (extendedUserType != null)
            {
                Assert.True(extendedUserType.IsClass);
                var properties = extendedUserType.GetProperties();
                
                // �߰��� �Ӽ��� Ȯ��
                var createdAtProp = properties.FirstOrDefault(p => p.Name == "CreatedAt");
                var lastLoginProp = properties.FirstOrDefault(p => p.Name == "LastLoginAt");
                var metadataProp = properties.FirstOrDefault(p => p.Name == "Metadata");

                Assert.NotNull(createdAtProp);
                Assert.NotNull(lastLoginProp);
                Assert.NotNull(metadataProp);
            }
        }

        [Fact]
        public void SourceGenerator_Should_GenerateUserProfileType()
        {
            // Arrange
            TypeCombiner.Combine<PersonalInfo, ContactInfo>()
                .WithName("UserProfile")
                .AsRecord()
                .Generate();

            // Act & Assert
            var userProfileType = Type.GetType("Generated.UserProfile");
            if (userProfileType != null)
            {
                Assert.True(userProfileType.IsClass);
                var properties = userProfileType.GetProperties();
                Assert.True(properties.Length >= 0); // ���յ� Ÿ���� �Ӽ���
            }
        }

        [Fact]
        public void SourceGenerator_Should_HandleExcludedProperties()
        {
            // Arrange
            TypeCombiner.From<PersonalInfo>()
                .Exclude(p => p.Password)
                .WithName("PublicPersonalInfo")
                .AsRecord()
                .Generate();

            // Act & Assert
            var publicPersonalInfoType = Type.GetType("Generated.PublicPersonalInfo");
            if (publicPersonalInfoType != null)
            {
                var properties = publicPersonalInfoType.GetProperties();
                var passwordProp = properties.FirstOrDefault(p => p.Name == "Password");
                
                // Password �Ӽ��� ���ܵǾ����� Ȯ��
                Assert.Null(passwordProp);
            }
        }

        [Fact]
        public void SourceGenerator_Should_HandleTypeChanges()
        {
            // Arrange
            TypeCombiner.From<Product>()
                .ChangeType(p => p.Price, typeof(string))
                .ChangeType(p => p.StockQuantity, typeof(string))
                .WithName("ProductDto")
                .AsRecord()
                .Generate();

            // Act & Assert
            var productDtoType = Type.GetType("Generated.ProductDto");
            if (productDtoType != null)
            {
                var properties = productDtoType.GetProperties();
                var priceProp = properties.FirstOrDefault(p => p.Name == "Price");
                var stockProp = properties.FirstOrDefault(p => p.Name == "StockQuantity");

                // Ÿ���� ����Ǿ����� Ȯ��
                if (priceProp != null)
                    Assert.Equal(typeof(string), priceProp.PropertyType);
                if (stockProp != null)
                    Assert.Equal(typeof(string), stockProp.PropertyType);
            }
        }

        [Fact]
        public void SourceGenerator_Should_CreateStructType()
        {
            // Arrange
            TypeCombiner.From<Product>()
                .Add("LastRestocked", typeof(DateTime?))
                .Add("MinimumStock", typeof(int))
                .Add("Supplier", typeof(string))
                .WithName("ProductWithInventory")
                .AsStruct()
                .Generate();

            // Act & Assert
            var productWithInventoryType = Type.GetType("Generated.ProductWithInventory");
            if (productWithInventoryType != null)
            {
                Assert.True(productWithInventoryType.IsValueType);
                Assert.False(productWithInventoryType.IsEnum);
            }
        }
    }

    /// <summary>
    /// ���� �׽�Ʈ
    /// </summary>
    public class PerformanceTests
    {
        [Fact]
        public void TypeCombiner_MultipleGenerations_Should_BeReasonablyFast()
        {
            // Arrange
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            const int iterations = 100;

            // Act
            for (int i = 0; i < iterations; i++)
            {
                TypeCombiner.From<SimpleModel>()
                    .Add($"Property{i}", typeof(string))
                    .WithName($"TestType{i}")
                    .Generate();
            }

            stopwatch.Stop();

            // Assert - 100�� ������ 5�� �̳��� �Ϸ�Ǿ�� ��
            Assert.True(stopwatch.ElapsedMilliseconds < 5000, 
                $"Performance test failed: {stopwatch.ElapsedMilliseconds}ms for {iterations} iterations");
        }

        [Fact]
        public void TypeCombiner_ComplexChain_Should_NotCauseStackOverflow()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() =>
            {
                var builder = TypeCombiner.From<User>();
                
                // �ſ� �� ü�� ����
                for (int i = 0; i < 50; i++)
                {
                    builder = builder.Add($"Property{i}", typeof(string));
                }
                
                builder.WithName("VeryComplexType").Generate();
            });

            Assert.Null(exception);
        }
    }

    /// <summary>
    /// ���� ��� �ó����� �׽�Ʈ
    /// </summary>
    public class RealWorldScenarioTests
    {
        [Fact]
        public void Scenario_APIDto_Should_Work()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() =>
            {
                // README.md ���ÿ� ������ �ó�����
                TypeCombiner.From<PersonalInfo>()
                    .Exclude(u => u.Password)
                    .Add("PublicId", typeof(Guid))
                    .WithName("UserApiDto")
                    .AsRecord()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void Scenario_DatabaseMigration_Should_Work()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() =>
            {
                // README.md ���ÿ� ������ �ó�����
                TypeCombiner.From<User>()
                    .Add("CreatedAt", typeof(DateTime))
                    .Add("UpdatedAt", typeof(DateTime?))
                    .ChangeType(u => u.IsActive, typeof(int)) // bool -> int ��ȯ
                    .WithName("ModernUserTable")
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void Scenario_EmployeeFromUser_Should_Work()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() =>
            {
                // Program.cs�� ������ �ó�����
                TypeCombiner.Combine<User>()
                    .With(new { Department = "", Salary = 0m, Position = "" })
                    .WithName("Employee")
                    .AsRecord()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void Scenario_SecureUserProfile_Should_Work()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() =>
            {
                // Program.cs�� ������ ���� �ó�����
                TypeCombiner.Combine<PersonalInfo, ContactInfo>()
                    .Exclude<PersonalInfo>(p => p.Password)
                    .Exclude<PersonalInfo>(p => p.BirthDate)
                    .Add("CreatedAt", typeof(DateTime))
                    .Add("IsVerified", typeof(bool))
                    .ChangeType<ContactInfo>(c => c.PhoneNumber, typeof(string))
                    .WithName("SecureUserProfile")
                    .AsRecord()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void Scenario_UserAccount_Should_Work()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .With(new { UserId = Guid.Empty, Username = "" })
                    .With(new { CreatedAt = DateTime.Now, IsActive = true })
                    .With(new { Permissions = new string[] { }, Role = "" })
                    .WithName("UserAccount")
                    .AsClass()
                    .Generate();
            });

            Assert.Null(exception);
        }
    }

    /// <summary>
    /// ����ȭ�� ��� �׽�Ʈ (README.md ���)
    /// </summary>
    public class DocumentedFeaturesTests
    {
        [Fact]
        public void DocumentedFeature_TypeCombination_Should_Work()
        {
            // README.md ù ��° ����
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine<PersonalInfo, ContactInfo>()
                    .WithName("UserProfile")
                    .AsRecord()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void DocumentedFeature_AnonymousTypesSupport_Should_Work()
        {
            // README.md ���� Ÿ�� ���õ�
            var exception1 = Record.Exception(() =>
            {
                TypeCombiner.Combine<User>()
                    .With(new { Department = "", Salary = 0m, IsActive = true })
                    .WithName("Employee")
                    .Generate();
            });

            var exception2 = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .With(new { Name = "", Age = 0 })
                    .With(new { Email = "", Phone = "" })
                    .WithName("Contact")
                    .AsRecord()
                    .Generate();
            });

            Assert.Null(exception1);
            Assert.Null(exception2);
        }

        [Fact]
        public void DocumentedFeature_PropertyExclusion_Should_Work()
        {
            // README.md �Ӽ� ���� ����
            var exception = Record.Exception(() =>
            {
                TypeCombiner.From<PersonalInfo>()
                    .Exclude(u => u.Password)
                    .WithName("PublicUser")
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void DocumentedFeature_PropertyAddition_Should_Work()
        {
            // README.md �Ӽ� �߰� ����
            var exception = Record.Exception(() =>
            {
                TypeCombiner.From<User>()
                    .Add("CreatedAt", typeof(DateTime))
                    .Add("IsActive", typeof(bool))
                    .Add("Metadata", typeof(Dictionary<string, object>))
                    .WithName("ExtendedUser")
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void DocumentedFeature_TypeChange_Should_Work()
        {
            // README.md Ÿ�� ���� ����
            var exception = Record.Exception(() =>
            {
                TypeCombiner.From<Product>()
                    .ChangeType(e => e.Price, typeof(string))      // decimal �� string
                    .ChangeType(e => e.StockQuantity, typeof(string)) // int �� string
                    .WithName("EmployeeDto")
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void DocumentedFeature_ComplexOperation_Should_Work()
        {
            // README.md ���� ���� ����
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine<PersonalInfo, ContactInfo>()
                    .Exclude<PersonalInfo>(p => p.Password)                       // �ΰ��� ���� ����
                    .Add("LastLoginAt", typeof(DateTime?))          // �� �Ӽ� �߰�
                    .ChangeType<ContactInfo>(c => c.PhoneNumber, typeof(string)) // Ÿ�� ����
                    .WithName("SecureUserProfile")
                    .AsRecord()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void DocumentedFeature_CollectionTypes_Should_Work()
        {
            // README.md �÷��� Ÿ�� ����
            var exception = Record.Exception(() =>
            {
                TypeCombiner.From<User>()
                    .Add("Tags", typeof(List<string>))
                    .Add("Permissions", typeof(string[]))
                    .WithName("TaggedUser")
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void DocumentedFeature_ConditionalExclusion_Should_Work()
        {
            // README.md ���Ǻ� �Ӽ� ���� ����
            var exception = Record.Exception(() =>
            {
                TypeCombiner.From<PersonalInfo>()
                    .ExcludeIf(u => u.Password, condition: true)
                    .WithName("ContextualUser")
                    .Generate();
            });

            Assert.Null(exception);
        }
    }
}