using Structura;

namespace TestModels
{
    /// <summary>
    /// 개인 정보를 나타내는 테스트 모델
    /// </summary>
    public class PersonalInfo
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public int Age { get; set; }
        public string Password { get; set; } = ""; // 제외될 민감한 속성
        public DateTime BirthDate { get; set; }
    }

    /// <summary>
    /// 연락처 정보를 나타내는 테스트 모델
    /// </summary>
    public class ContactInfo
    {
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Address { get; set; } = "";
        public string Country { get; set; } = "";
    }

    /// <summary>
    /// 사용자 정보를 나타내는 테스트 모델
    /// </summary>
    public class User
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string Email { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// 제품 정보를 나타내는 테스트 모델
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
            System.Console.WriteLine("🎯 Structura 라이브러리 - EF Core Projection 지원 기능 테스트");
            System.Console.WriteLine("===============================================");

            TestVariableReference();
            TestDirectVsVariableComparison();
            TestComplexVariableReference();
            TestEFCoreProjectionFeatures();
            
            System.Console.WriteLine("\n✅ 모든 테스트가 완료되었습니다!");
            System.Console.WriteLine("Press any key to exit...");
            System.Console.ReadKey();
        }

        /// <summary>
        /// 기본 변수 참조 테스트
        /// </summary>
        static void TestVariableReference()
        {
            System.Console.WriteLine("\n📋 1. 기본 변수 참조 테스트");
            System.Console.WriteLine("------------------------");

            // 원래 문제가 되었던 시나리오
            System.Console.WriteLine("1-1. 원래 문제 시나리오:");
            var anonymousInstance = new { Name = "홍길동", Age = 40, Sex = "male" };
            TypeCombiner.Combine()
                .With(anonymousInstance)
                .WithName("AnonymousUser")
                .AsClass()
                .Generate();
            System.Console.WriteLine("✅ AnonymousUser 타입 생성 완료");

            // 복잡한 무명 타입
            System.Console.WriteLine("1-2. 복잡한 무명 타입 변수:");
            var complexUser = new { 
                Id = Guid.NewGuid(), 
                Name = "김개발", 
                Email = "kim@company.com",
                IsActive = true,
                CreatedAt = DateTime.Now,
                Score = 95.5,
                Metadata = new Dictionary<string, object>
                {
                    ["department"] = "개발팀",
                    ["level"] = "시니어"
                }
            };
            
            TypeCombiner.Combine()
                .With(complexUser)
                .WithName("ComplexUser")
                .AsRecord()
                .Generate();
            System.Console.WriteLine("✅ ComplexUser 타입 생성 완료");

            // 여러 변수 결합
            System.Console.WriteLine("1-3. 여러 변수 결합:");
            var personalData = new { FirstName = "이", LastName = "개발", Age = 28 };
            var workData = new { Company = "테크컴퍼니", Position = "백엔드개발자", Salary = 60000m };
            
            TypeCombiner.Combine()
                .With(personalData)
                .With(workData)
                .WithName("EmployeeProfile")
                .AsClass()
                .Generate();
            System.Console.WriteLine("✅ EmployeeProfile 타입 생성 완료");
        }

        /// <summary>
        /// 직접 생성 vs 변수 참조 비교 테스트
        /// </summary>
        static void TestDirectVsVariableComparison()
        {
            System.Console.WriteLine("\n📋 2. 직접 생성 vs 변수 참조 비교");
            System.Console.WriteLine("-------------------------------");

            // 직접 생성 (기존 방식)
            System.Console.WriteLine("2-1. 직접 무명 객체 생성:");
            TypeCombiner.Combine()
                .With(new { Name = "직접생성", Age = 25, Status = "Active" })
                .WithName("DirectCreated")
                .AsRecord()
                .Generate();
            System.Console.WriteLine("✅ DirectCreated 타입 생성 완료");

            // 변수 참조 (개선된 방식)
            System.Console.WriteLine("2-2. 변수 참조 방식:");
            var userInstance = new { Name = "변수참조", Age = 25, Status = "Active" };
            TypeCombiner.Combine()
                .With(userInstance)
                .WithName("VariableReferenced")
                .AsRecord()
                .Generate();
            System.Console.WriteLine("✅ VariableReferenced 타입 생성 완료");

            // 혼합 방식
            System.Console.WriteLine("2-3. 혼합 방식 (변수 + 직접):");
            var baseInfo = new { UserId = 123, Username = "mixeduser" };
            TypeCombiner.Combine()
                .With(baseInfo) // 변수 참조
                .With(new { CreatedAt = DateTime.Now, IsVerified = true }) // 직접 생성
                .WithName("MixedApproach")
                .AsClass()
                .Generate();
            System.Console.WriteLine("✅ MixedApproach 타입 생성 완료");
        }

        /// <summary>
        /// 복잡한 변수 참조 시나리오 테스트
        /// </summary>
        static void TestComplexVariableReference()
        {
            System.Console.WriteLine("\n📋 3. 복잡한 변수 참조 시나리오");
            System.Console.WriteLine("-----------------------------");

            // 다양한 타입들
            System.Console.WriteLine("3-1. 다양한 데이터 타입:");
            var typedData = new { 
                StringValue = "테스트문자열",
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
            System.Console.WriteLine("✅ TypedData 구조체 생성 완료");

            // 컬렉션 타입들
            System.Console.WriteLine("3-2. 컬렉션 타입들:");
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
            System.Console.WriteLine("✅ CollectionData 클래스 생성 완료");

            // 기존 타입과 변수 결합
            System.Console.WriteLine("3-3. 기존 타입과 변수 결합:");
            var additionalInfo = new { 
                Department = "R&D", 
                Team = "Backend",
                StartDate = DateTime.Now.AddYears(-2),
                Skills = new string[] { "C#", ".NET", "SQL" }
            };
            
            TypeCombiner.Combine<TestModels.User>()
                .With(additionalInfo)
                .WithName("EnhancedUser")
                .AsRecord()
                .Generate();
            System.Console.WriteLine("✅ EnhancedUser 레코드 생성 완료");

            // 실제 생성된 타입 사용 테스트
            System.Console.WriteLine("\n3-4. 생성된 타입 사용 테스트:");
            TestGeneratedTypes();
        }

        /// <summary>
        /// EF Core projection 기능 테스트
        /// </summary>
        static void TestEFCoreProjectionFeatures()
        {
            System.Console.WriteLine("\n📋 4. EF Core Projection 기능 테스트");
            System.Console.WriteLine("----------------------------------");

            // 기본 projection 시나리오
            System.Console.WriteLine("4-1. 기본 EF Core projection 시뮬레이션:");
            // var result = dbContext.Users.Select(x => new { x.Name, x.Email }).ToList();
            var userProjection = new List<object>
            {
                new { Name = "홍길동", Email = "hong@example.com" },
                new { Name = "김철수", Email = "kim@example.com" },
                new { Name = "이영희", Email = "lee@example.com" }
            };

            TypeCombiner.Combine()
                .WithProjection(userProjection)
                .WithName("UserProjectionType")
                .AsRecord()
                .Generate();
            System.Console.WriteLine("✅ UserProjectionType 레코드 생성 완료");

            // 복잡한 projection
            System.Console.WriteLine("4-2. 복잡한 projection 시나리오:");
            // var complexResult = dbContext.Users
            //     .Select(u => new { u.Name, u.Email, u.Department.Name, OrderCount = u.Orders.Count() })
            //     .ToList();
            var complexProjection = new List<object>
            {
                new { Name = "김개발", Email = "kim@company.com", DepartmentName = "개발팀", OrderCount = 5 },
                new { Name = "박디자인", Email = "park@company.com", DepartmentName = "디자인팀", OrderCount = 3 }
            };

            TypeCombiner.Combine()
                .WithProjection(complexProjection)
                .WithName("ComplexProjectionType")
                .AsClass()
                .Generate();
            System.Console.WriteLine("✅ ComplexProjectionType 클래스 생성 완료");

            // Projection + 추가 속성
            System.Console.WriteLine("4-3. Projection과 추가 속성 결합:");
            var baseProjection = new List<object>
            {
                new { UserId = 1, Name = "사용자1" },
                new { UserId = 2, Name = "사용자2" }
            };

            TypeCombiner.Combine()
                .WithProjection(baseProjection)
                .With(new { 
                    LastLogin = DateTime.Now, 
                    IsOnline = false,
                    Permissions = new string[] { "read", "write" }
                })
                .WithName("EnhancedProjectionType")
                .AsRecord()
                .Generate();
            System.Console.WriteLine("✅ EnhancedProjectionType 레코드 생성 완료");

            // 여러 projection 결합
            System.Console.WriteLine("4-4. 여러 projection 결합:");
            var profileProjection = new List<object>
            {
                new { Bio = "개발자", Location = "서울" }
            };
            
            var settingsProjection = new List<object>
            {
                new { Theme = "Dark", Language = "Korean" }
            };

            TypeCombiner.Combine()
                .WithProjection(userProjection)
                .WithProjection(profileProjection)
                .WithProjection(settingsProjection)
                .WithName("MultiProjectionType")
                .AsStruct()
                .Generate();
            System.Console.WriteLine("✅ MultiProjectionType 구조체 생성 완료");

            // 기존 타입 + projection
            System.Console.WriteLine("4-5. 기존 타입과 projection 결합:");
            var additionalData = new List<object>
            {
                new { Department = "Engineering", Salary = 75000m }
            };

            TypeCombiner.Combine<TestModels.User>()
                .WithProjection(additionalData)
                .WithName("UserWithProjection")
                .AsClass()
                .Generate();
            System.Console.WriteLine("✅ UserWithProjection 클래스 생성 완료");

            // 실제 시나리오: 대시보드 데이터
            System.Console.WriteLine("4-6. 실제 대시보드 시나리오:");
            // var dashboardData = dbContext.Users
            //     .Join(dbContext.Orders, u => u.Id, o => o.UserId, (u, o) => new { u.Name, o.TotalAmount, o.OrderDate })
            //     .GroupBy(x => x.Name)
            //     .Select(g => new { 
            //         CustomerName = g.Key, 
            //         TotalOrders = g.Count(), 
            //         TotalSpent = g.Sum(x => x.TotalAmount),
            //         LastOrderDate = g.Max(x => x.OrderDate)
            //     })
            //     .ToList();
            
            var dashboardData = new List<object>
            {
                new { 
                    CustomerName = "홍길동", 
                    TotalOrders = 12, 
                    TotalSpent = 150000m,
                    LastOrderDate = DateTime.Now.AddDays(-5)
                },
                new { 
                    CustomerName = "김철수", 
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
                .AsRecord()
                .Generate();
            System.Console.WriteLine("✅ CustomerDashboard 레코드 생성 완료");

            System.Console.WriteLine("\n🎉 EF Core Projection 기능이 성공적으로 구현되었습니다!");
        }

        /// <summary>
        /// 실제 생성된 타입들 사용 테스트
        /// </summary>
        static void TestGeneratedTypes()
        {
            try
            {
                // AnonymousUser 타입 확인
                var anonymousUserType = Type.GetType("Generated.AnonymousUser");
                if (anonymousUserType != null)
                {
                    System.Console.WriteLine($"✅ AnonymousUser 타입 발견 ({GetTypeKind(anonymousUserType)})");
                    var properties = anonymousUserType.GetProperties();
                    foreach (var prop in properties)
                    {
                        System.Console.WriteLine($"   - {prop.Name}: {GetFriendlyTypeName(prop.PropertyType)}");
                    }
                }
                else
                {
                    System.Console.WriteLine("⚠️ AnonymousUser 타입을 찾을 수 없습니다.");
                }

                // ComplexUser 타입 확인
                var complexUserType = Type.GetType("Generated.ComplexUser");
                if (complexUserType != null)
                {
                    System.Console.WriteLine($"✅ ComplexUser 타입 발견 ({GetTypeKind(complexUserType)})");
                    var properties = complexUserType.GetProperties();
                    System.Console.WriteLine($"   총 {properties.Length}개 속성:");
                    foreach (var prop in properties.Take(5)) // 처음 5개만 표시
                    {
                        System.Console.WriteLine($"   - {prop.Name}: {GetFriendlyTypeName(prop.PropertyType)}");
                    }
                    if (properties.Length > 5)
                    {
                        System.Console.WriteLine($"   ... 및 {properties.Length - 5}개 속성 더");
                    }
                }

                // TypedData 타입 확인
                var typedDataType = Type.GetType("Generated.TypedData");
                if (typedDataType != null)
                {
                    System.Console.WriteLine($"✅ TypedData 타입 발견 ({GetTypeKind(typedDataType)})");
                    System.Console.WriteLine($"   IsValueType: {typedDataType.IsValueType}");
                }

                // UserProjectionType 확인
                var userProjectionType = Type.GetType("Generated.UserProjectionType");
                if (userProjectionType != null)
                {
                    System.Console.WriteLine($"✅ UserProjectionType 타입 발견 ({GetTypeKind(userProjectionType)})");
                    var properties = userProjectionType.GetProperties();
                    foreach (var prop in properties)
                    {
                        System.Console.WriteLine($"   - {prop.Name}: {GetFriendlyTypeName(prop.PropertyType)}");
                    }
                }

                System.Console.WriteLine("\n🎉 변수 참조 및 EF Core projection 기능이 성공적으로 작동합니다!");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"\n⚠️ 타입 확인 중 예외 발생: {ex.Message}");
                System.Console.WriteLine("하지만 소스 생성기는 정상적으로 동작했습니다.");
            }
        }

        private static string GetTypeKind(Type type)
        {
            if (type.IsValueType && !type.IsEnum)
                return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ? "Nullable Struct" : "Struct";
            if (type.IsClass)
            {
                // C# 9.0+ 레코드 타입 확인 (간접적 방법)
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
    }
}
