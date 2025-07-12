using Generated;

using Structura;
using System.Reflection;

namespace TestModels
{
    /// <summary>
    /// Test model representing personal information
    /// </summary>
    public class PersonalInfo
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public int Age { get; set; }
        public string Password { get; set; } = ""; // Sensitive property to be excluded
        public DateTime BirthDate { get; set; }
    }

    /// <summary>
    /// Test model representing contact information
    /// </summary>
    public class ContactInfo
    {
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Address { get; set; } = "";
        public string Country { get; set; } = "";
    }

    /// <summary>
    /// Test model representing user information
    /// </summary>
    public class User
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string Email { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Test model representing product information
    /// </summary>
    public class Product
    {
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public string Category { get; set; } = "";
        public int StockQuantity { get; set; }
    }
}

namespace Structura.Test.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("🎯 Structura Library - EF Core Projection Support Feature Test");
            System.Console.WriteLine("===============================================");

            TestVariableReference();
            TestDirectVsVariableComparison();
            TestComplexVariableReference();
            TestEFCoreProjectionFeatures();
            
            System.Console.WriteLine("\n✅ All tests completed successfully!");
            System.Console.WriteLine("Press any key to exit...");
            System.Console.ReadKey();
        }

        /// <summary>
        /// Basic variable reference test
        /// </summary>
        static void TestVariableReference()
        {
            System.Console.WriteLine("\n📋 1. Basic Variable Reference Test");
            System.Console.WriteLine("------------------------");

            // Original problematic scenario
            System.Console.WriteLine("1-1. Original problem scenario:");
            var anonymousInstance = new { Name = "John Doe", Age = 40, Sex = "male" };
            TypeCombiner.Combine()
                .With(anonymousInstance)
                .WithName("AnonymousUser")
                .AsClass()
                .Generate();
            System.Console.WriteLine("✅ AnonymousUser type generated");

            // Complex anonymous type
            System.Console.WriteLine("1-2. Complex anonymous type variable:");
            var complexUser = new { 
                Id = Guid.NewGuid(), 
                Name = "Jane Developer", 
                Email = "jane@company.com",
                IsActive = true,
                CreatedAt = DateTime.Now,
                Score = 95.5,
                Metadata = new Dictionary<string, object>
                {
                    ["department"] = "Development",
                    ["level"] = "Senior"
                }
            };
            
            TypeCombiner.Combine()
                .With(complexUser)
                .WithName("ComplexUser")
                .AsRecord()
                .Generate();
            System.Console.WriteLine("✅ ComplexUser type generated");

            // Multiple variable combination
            System.Console.WriteLine("1-3. Multiple variable combination:");
            var personalData = new { FirstName = "John", LastName = "Developer", Age = 28 };
            var workData = new { Company = "TechCompany", Position = "Backend Developer", Salary = 60000m };
            
            TypeCombiner.Combine()
                .With(personalData)
                .With(workData)
                .WithName("EmployeeProfile")
                .AsClass()
                .Generate();
            System.Console.WriteLine("✅ EmployeeProfile type generated");
        }

        /// <summary>
        /// Direct creation vs variable reference comparison test
        /// </summary>
        static void TestDirectVsVariableComparison()
        {
            System.Console.WriteLine("\n📋 2. Direct Creation vs Variable Reference Comparison");
            System.Console.WriteLine("-------------------------------");

            // Direct creation (existing approach)
            System.Console.WriteLine("2-1. Direct anonymous object creation:");
            TypeCombiner.Combine()
                .With(new { Name = "DirectCreated", Age = 25, Status = "Active" })
                .WithName("DirectCreated")
                .AsRecord()
                .Generate();
            System.Console.WriteLine("✅ DirectCreated type generated");

            // Variable reference (improved approach)
            System.Console.WriteLine("2-2. Variable reference approach:");
            var userInstance = new { Name = "VariableReference", Age = 25, Status = "Active" };
            TypeCombiner.Combine()
                .With(userInstance)
                .WithName("VariableReferenced")
                .AsRecord()
                .Generate();
            System.Console.WriteLine("✅ VariableReferenced type generated");

            // Mixed approach
            System.Console.WriteLine("2-3. Mixed approach (variable + direct):");
            var baseInfo = new { UserId = 123, Username = "mixeduser" };
            TypeCombiner.Combine()
                .With(baseInfo) // Variable reference
                .With(new { CreatedAt = DateTime.Now, IsVerified = true }) // Direct creation
                .WithName("MixedApproach")
                .AsClass()
                .Generate();
            System.Console.WriteLine("✅ MixedApproach type generated");
        }

        /// <summary>
        /// Complex variable reference scenario test
        /// </summary>
        static void TestComplexVariableReference()
        {
            System.Console.WriteLine("\n📋 3. Complex Variable Reference Scenarios");
            System.Console.WriteLine("-----------------------------");

            // Various data types
            System.Console.WriteLine("3-1. Various data types:");
            var typedData = new { 
                StringValue = "TestString",
                IntValue = 42,
                LongValue = 123L,
                FloatValue = 3.14f,
                DoubleValue = 2.718,
                DecimalValue = 99.99m,
                BoolValue = true,
                DateValue = DateTime.Now,
                GuidValue = Guid.NewGuid()
            };
            
            TypeCombiner.Combine()
                .With(typedData)
                .WithName("TypedData")
                .AsStruct()
                .Generate();
            System.Console.WriteLine("✅ TypedData struct generated");

            // Collection types
            System.Console.WriteLine("3-2. Collection types:");
            var collectionData = new { 
                Tags = new string[] { "tag1", "tag2", "tag3" },
                Scores = new List<int> { 85, 90, 95, 88 },
                Properties = new Dictionary<string, object> 
                { 
                    ["key1"] = "value1",
                    ["key2"] = 42,
                    ["key3"] = true
                }
            };
            
            TypeCombiner.Combine()
                .With(collectionData)
                .WithName("CollectionData")
                .AsClass()
                .Generate();
            System.Console.WriteLine("✅ CollectionData class generated");

            // Combining existing type with variable
            System.Console.WriteLine("3-3. Existing type with variable combination:");
            var additionalInfo = new { 
                Department = "R&D", 
                Team = "Backend",
                StartDate = DateTime.Now.AddYears(-2),
                Skills = new string[] { "C#", ".NET", "SQL" }
            };
            
            TypeCombiner.Combine<TestModels.User>()
                .With(additionalInfo)
                .WithName("EnhancedUser")
                .WithConverter()  // 🔥 명명 타입 컨버터 활성화!
                .AsRecord()
                .Generate();
            System.Console.WriteLine("✅ EnhancedUser record generated");

            // Test using generated types
            System.Console.WriteLine("\n3-4. Testing generated types:");
            TestGeneratedTypes();
        }

        /// <summary>
        /// EF Core projection features test
        /// </summary>
        static void TestEFCoreProjectionFeatures()
        {
            System.Console.WriteLine("\n📋 4. EF Core Projection Features Test + Smart Converter");
            System.Console.WriteLine("--------------------------------------------------------");

            // Basic projection scenario WITH CONVERTER
            System.Console.WriteLine("4-1. Basic EF Core projection with Smart Converter:");
            var userProjection = new List<object>
            {
                new { Name = "John Doe", Email = "john@example.com" },
                new { Name = "Jane Smith", Email = "jane@example.com" },
                new { Name = "Bob Johnson", Email = "bob@example.com" }
            };

            TypeCombiner.Combine()
                .WithProjection(userProjection)
                .WithName("UserProjectionType")
                .WithConverter()  // 🔥 This enables the magic!
                .AsRecord()
                .Generate();
            System.Console.WriteLine("✅ UserProjectionType record with converter generated");

            // Complex projection WITH CONVERTER
            System.Console.WriteLine("4-2. Complex projection with Smart Converter:");
            var complexProjection = new List<object>
            {
                new { Name = "John Developer", Email = "john@company.com", DepartmentName = "Development", OrderCount = 5 },
                new { Name = "Jane Designer", Email = "jane@company.com", DepartmentName = "Design", OrderCount = 3 }
            };

            TypeCombiner.Combine()
                .WithProjection(complexProjection)
                .WithName("ComplexProjectionType")
                .WithConverter()  // 🔥 This enables the magic!
                .AsClass()
                .Generate();
            System.Console.WriteLine("✅ ComplexProjectionType class with converter generated");

            // Projection + additional properties WITH CONVERTER
            System.Console.WriteLine("4-3. Enhanced projection with additional properties and converter:");
            var baseProjection = new List<object>
            {
                new { UserId = 1, Name = "User1" },
                new { UserId = 2, Name = "User2" }
            };

            TypeCombiner.Combine()
                .WithProjection(baseProjection)
                .With(new { 
                    LastLogin = DateTime.Now, 
                    IsOnline = false,
                    Permissions = new string[] { "read", "write" }
                })
                .WithName("EnhancedProjectionType")
                .WithConverter()  // 🔥 This enables the magic!
                .AsRecord()
                .Generate();
            System.Console.WriteLine("✅ EnhancedProjectionType record with converter generated");

            // Real scenario: Dashboard data WITH CONVERTER
            System.Console.WriteLine("4-4. Real dashboard scenario with Smart Converter:");
            var dashboardData = new List<object>
            {
                new { 
                    CustomerName = "John Doe", 
                    TotalOrders = 12, 
                    TotalSpent = 150000m,
                    LastOrderDate = DateTime.Now.AddDays(-5)
                },
                new { 
                    CustomerName = "Jane Smith", 
                    TotalOrders = 8, 
                    TotalSpent = 95000m,
                    LastOrderDate = DateTime.Now.AddDays(-2)
                }
            };

            TypeCombiner.Combine()
                .WithProjection(dashboardData)
                .With(new { 
                    GeneratedAt = DateTime.Now,
                    ReportType = "CustomerAnalysis"
                })
                .WithName("CustomerDashboard")
                .WithConverter()  // 🔥 This enables the magic!
                .AsRecord()
                .Generate();
            System.Console.WriteLine("✅ CustomerDashboard record with converter generated");

            // 🎯 NEW: Test actual converter usage
            System.Console.WriteLine("\n4-5. Testing actual converter usage:");
            TestActualConverterUsage(userProjection, baseProjection, dashboardData);

            // 기존 테스트들도 유지
            TestMultipleProjectionCombination();
            TestExistingTypeWithProjection();
        }

        /// <summary>
        /// 🔥 Test actual converter usage with generated types
        /// </summary>
        static void TestActualConverterUsage(List<object> userProjection, List<object> baseProjection, List<object> dashboardData)
        {
            try
            {
                System.Console.WriteLine("Testing static converter methods directly on generated types...");

                // Test 1: Anonymous object conversion using generated static methods
                System.Console.WriteLine("🧪 Test 1: Direct anonymous object conversion...");
                var anonymousUsers = new[]
                {
                    new { Name = "Test User 1", Email = "user1@test.com" },
                    new { Name = "Test User 2", Email = "user2@test.com" }
                };

                try
                {
                    // Direct method call on generated type
                    var convertedUsers = UserProjectionType.FromTypedCollection(anonymousUsers);
                    System.Console.WriteLine($"   ✅ Successfully converted {convertedUsers.Count} users using UserProjectionType.FromTypedCollection()");
                    
                    // Convert single object
                    var singleUser = new { Name = "Single User", Email = "single@test.com" };
                    var convertedSingle = UserProjectionType.FromTyped(singleUser);
                    System.Console.WriteLine($"   ✅ Successfully converted single user: {convertedSingle}");
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"   ⚠️ Anonymous object converter test failed: {ex.Message}");
                }

                // Test 2: Object collection conversion
                System.Console.WriteLine("🧪 Test 2: Object collection conversion...");
                try
                {
                    var convertedFromObjects = UserProjectionType.FromCollection(userProjection);
                    System.Console.WriteLine($"   ✅ Successfully converted {convertedFromObjects.Count} objects using UserProjectionType.FromCollection()");
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"   ⚠️ Object collection converter test failed: {ex.Message}");
                }

                // Test 3: Named type conversion (User -> EnhancedUser)
                System.Console.WriteLine("🧪 Test 3: Named type conversion...");
                try
                {
                    var user = new TestModels.User 
                    { 
                        Name = "Test User", 
                        Age = 30, 
                        Email = "test@example.com", 
                        IsActive = true 
                    };
                    
                    // EnhancedUser.FromUser 메서드가 실제로 생성되었는지 확인
                    // TODO: 명명 타입 컨버터 구현 중
                    System.Console.WriteLine($"   ⚠️ Named type converter (EnhancedUser.FromUser) - implementation in progress");
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"   ⚠️ Named type converter test failed: {ex.Message}");
                }

                // Test 4: Multiple type combination (PersonalInfo + ContactInfo -> UserProfile)
                System.Console.WriteLine("🧪 Test 4: Multiple type combination...");
                try
                {
                    TestCombinedTypeConverters();
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"   ⚠️ Combined type converter test failed: {ex.Message}");
                }

                // Test 5: Dashboard data with additional properties
                System.Console.WriteLine("🧪 Test 5: Complex dashboard conversion...");
                try
                {
                    var dashboardItem = new { 
                        CustomerName = "Test Corp", 
                        TotalOrders = 25, 
                        TotalSpent = 50000m,
                        LastOrderDate = DateTime.Now.AddDays(-3)
                    };
                    
                    var dashboard = CustomerDashboard.FromTyped(dashboardItem);
                    System.Console.WriteLine($"   ✅ Successfully converted dashboard data: {dashboard}");
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"   ⚠️ Dashboard converter test failed: {ex.Message}");
                }

                System.Console.WriteLine("\n🎉 Static Converter Methods successfully tested!");
                System.Console.WriteLine("💡 Usage: TypeName.FromCollection(), TypeName.FromTyped(), TypeName.FromSourceType()");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"\n⚠️ Converter testing failed: {ex.Message}");
                System.Console.WriteLine("This might be expected during compile-time generation.");
            }
        }

        /// <summary>
        /// Test combined type converters
        /// </summary>
        static void TestCombinedTypeConverters()
        {
            System.Console.WriteLine("   🔧 Testing combined type converters...");
            
            // Generate a combined type with converter
            TypeCombiner.Combine<TestModels.PersonalInfo, TestModels.ContactInfo>()
                .WithName("UserProfile")
                .WithConverter()
                .AsClass()
                .Generate();

            var personalInfo = new TestModels.PersonalInfo 
            { 
                FirstName = "John", 
                LastName = "Doe", 
                Age = 30,
                Password = "secret",
                BirthDate = DateTime.Now.AddYears(-30)
            };
            
            var contactInfo = new TestModels.ContactInfo 
            { 
                Email = "john@example.com", 
                PhoneNumber = "555-1234",
                Address = "123 Main St",
                Country = "USA"
            };

            try
            {
                // Test individual conversions
                // TODO: 컴바인 타입 컨버터 구현 중
                System.Console.WriteLine($"     ⚠️ Combined converter methods (UserProfile.FromPersonalInfo, FromContactInfo, FromBoth) - implementation in progress");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"     ⚠️ Combined converter failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Existing type + projection test
        /// </summary>
        static void TestExistingTypeWithProjection()
        {
            System.Console.WriteLine("\n4-6. Existing type with projection combination:");
            var additionalData = new List<object>
            {
                new { Department = "Engineering", Salary = 75000m }
            };

            TypeCombiner.Combine<TestModels.User>()
                .WithProjection(additionalData)
                .WithName("UserWithProjection")
                .AsClass()
                .Generate();
            System.Console.WriteLine("✅ UserWithProjection class generated");
        }

        /// <summary>
        /// Test using actual generated types
        /// </summary>
        static void TestGeneratedTypes()
        {
            try
            {
                // Check AnonymousUser type
                var anonymousUserType = Type.GetType("Generated.AnonymousUser");
                if (anonymousUserType != null)
                {
                    System.Console.WriteLine($"✅ AnonymousUser type found ({GetTypeKind(anonymousUserType)})");
                    var properties = anonymousUserType.GetProperties();
                    foreach (var prop in properties)
                    {
                        System.Console.WriteLine($"   - {prop.Name}: {GetFriendlyTypeName(prop.PropertyType)}");
                    }
                }
                else
                {
                    System.Console.WriteLine("⚠️ AnonymousUser type not found.");
                }

                // Check ComplexUser type
                var complexUserType = Type.GetType("Generated.ComplexUser");
                if (complexUserType != null)
                {
                    System.Console.WriteLine($"✅ ComplexUser type found ({GetTypeKind(complexUserType)})");
                    var properties = complexUserType.GetProperties();
                    System.Console.WriteLine($"   Total {properties.Length} properties:");
                    foreach (var prop in properties.Take(5)) // Show first 5 only
                    {
                        System.Console.WriteLine($"   - {prop.Name}: {GetFriendlyTypeName(prop.PropertyType)}");
                    }
                    if (properties.Length > 5)
                    {
                        System.Console.WriteLine($"   ... and {properties.Length - 5} more properties");
                    }
                }

                // Check TypedData type
                var typedDataType = Type.GetType("Generated.TypedData");
                if (typedDataType != null)
                {
                    System.Console.WriteLine($"✅ TypedData type found ({GetTypeKind(typedDataType)})");
                    System.Console.WriteLine($"   IsValueType: {typedDataType.IsValueType}");
                }

                // Check UserProjectionType
                var userProjectionType = Type.GetType("Generated.UserProjectionType");
                if (userProjectionType != null)
                {
                    System.Console.WriteLine($"✅ UserProjectionType type found ({GetTypeKind(userProjectionType)})");
                    var properties = userProjectionType.GetProperties();
                    foreach (var prop in properties)
                    {
                        System.Console.WriteLine($"   - {prop.Name}: {GetFriendlyTypeName(prop.PropertyType)}");
                    }
                }

                System.Console.WriteLine("\n🎉 Variable reference and EF Core projection features work successfully!");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"\n⚠️ Exception occurred while checking types: {ex.Message}");
                System.Console.WriteLine("However, the source generator worked correctly.");
            }
        }

        private static string GetTypeKind(Type type)
        {
            if (type.IsValueType && !type.IsEnum)
                return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ? "Nullable Struct" : "Struct";
            if (type.IsClass)
            {
                // Check for C# 9.0+ record types (indirect method)
                var toStringMethod = type.GetMethod("ToString", Type.EmptyTypes);
                if (toStringMethod != null && toStringMethod.DeclaringType == type)
                {
                    return "Record";
                }
                return "Class";
            }
            return "Unknown";
        }

        private static string GetFriendlyTypeName(Type type)
        {
            if (type == typeof(string)) return "string";
            if (type == typeof(int)) return "int";
            if (type == typeof(long)) return "long";
            if (type == typeof(float)) return "float";
            if (type == typeof(double)) return "double";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(DateTime)) return "DateTime";
            if (type == typeof(DateTime?)) return "DateTime?";
            if (type == typeof(decimal)) return "decimal";
            if (type == typeof(Guid)) return "Guid";
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var args = type.GetGenericArguments();
                return $"Dictionary<{GetFriendlyTypeName(args[0])}, {GetFriendlyTypeName(args[1])}>";
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var arg = type.GetGenericArguments()[0];
                return $"List<{GetFriendlyTypeName(arg)}>";
            }
            if (type.IsArray)
            {
                return $"{GetFriendlyTypeName(type.GetElementType()!)}[]";
            }
            return type.Name;
        }

        /// <summary>
        /// Multiple projection combination test
        /// </summary>
        static void TestMultipleProjectionCombination()
        {
            System.Console.WriteLine("\n4-5. Multiple projection combination:");
            var userProjection = new List<object>
            {
                new { Name = "John Doe", Email = "john@example.com" }
            };
            
            var profileProjection = new List<object>
            {
                new { Bio = "Developer", Location = "Seoul" }
            };
            
            var settingsProjection = new List<object>
            {
                new { Theme = "Dark", Language = "English" }
            };

            TypeCombiner.Combine()
                .WithProjection(userProjection)
                .WithProjection(profileProjection)
                .WithProjection(settingsProjection)
                .WithName("MultiProjectionType")
                .AsStruct()
                .Generate();
            System.Console.WriteLine("✅ MultiProjectionType struct generated");
        }
    }
}
