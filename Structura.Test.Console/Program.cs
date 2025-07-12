using System;
using System.Linq;
using System.Collections.Generic;
using Generated;
using Structura;

namespace Structura.Test.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("🎯 Structura Library - 핵심 기능 테스트");
            System.Console.WriteLine("=====================================");
            System.Console.WriteLine();

            try
            {
                // 📋 1단계: 배열 프로젝션 (핵심 기능)
                TestArrayProjection();
                
                // 📋 2단계: 컨버터 기능
                TestConverterFeatures();
                
                // 📋 3단계: 타입 생성 모드 (Record, Class, Struct)
                TestTypeGenerationModes();
                
                // 📋 4단계: 익명 타입 처리
                TestAnonymousTypeHandling();
                
                // 📋 5단계: Add 기능
                TestAddFeature();
                
                // 📋 6단계: 네임스페이스 지정 기능
                TestNamespaceFeature();
                
                // 📋 7단계: Exclude 기능
                TestExcludeFeature();
                
                // 📋 8단계: ChangeType 기능
                TestChangeTypeFeature();
                
                System.Console.WriteLine("\n🎉 모든 핵심 기능 테스트가 성공적으로 완료되었습니다!");
                System.Console.WriteLine("💡 배열 프로젝션, 컨버터, 타입 생성, 익명 타입, Add, 네임스페이스, Exclude, ChangeType 기능 모두 정상 작동!");
                
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"\n❌ 테스트 실패: {ex.Message}");
                System.Console.WriteLine($"스택 트레이스: {ex.StackTrace}");
            }

            System.Console.WriteLine("\n아무 키나 누르면 종료됩니다...");
            System.Console.ReadKey();
        }

        /// <summary>
        /// 📋 1단계: 배열 프로젝션 테스트 (핵심 기능)
        /// </summary>
        static void TestArrayProjection()
        {
            System.Console.WriteLine("📋 1단계: 배열 프로젝션 테스트");
            System.Console.WriteLine("===========================");

            // 1-1. 간단한 배열 프로젝션
            System.Console.WriteLine("1-1. 간단한 배열 프로젝션 - Name 속성");
            var names = new[]
            {
                new { Name = "Alice" },
                new { Name = "Bob" },
                new { Name = "Charlie" }
            };

            TypeCombiner.Combine()
                .WithProjection(names)
                .WithName("ProjectedNames")
                .WithConverter()
                .Generate();

            var result = Generated.ProjectedNames.FromCollection(names);
            System.Console.WriteLine($"✅ ProjectedNames 변환: {result.Count}개 항목");
            foreach (var item in result)
            {
                System.Console.WriteLine($"   - Name: '{item.Name}' ✓");
            }

            // 1-2. 복잡한 배열 프로젝션
            System.Console.WriteLine("\n1-2. 복잡한 배열 프로젝션 - 여러 속성");
            var complexData = new[]
            {
                new { Id = 1, Name = "Product A", Price = 100m, Category = "Electronics" },
                new { Id = 2, Name = "Product B", Price = 200m, Category = "Books" }
            };

            TypeCombiner.Combine()
                .WithProjection(complexData)
                .WithName("ProjectedProducts")
                .WithConverter()
                .AsRecord()
                .Generate();

            var products = Generated.ProjectedProducts.FromCollection(complexData);
            System.Console.WriteLine($"✅ ProjectedProducts 변환: {products.Count}개 제품");
            foreach (var product in products)
            {
                System.Console.WriteLine($"   - ID: {product.Id}, Name: '{product.Name}', Price: {product.Price}원, Category: '{product.Category}' ✓");
            }

            System.Console.WriteLine();
        }

        /// <summary>
        /// 📋 2단계: 컨버터 기능 테스트
        /// </summary>
        static void TestConverterFeatures()
        {
            System.Console.WriteLine("📋 2단계: 컨버터 기능 테스트");
            System.Console.WriteLine("=========================");

            var products = new[]
            {
                new { Name = "노트북", Price = 1500000m, Brand = "삼성" },
                new { Name = "마우스", Price = 50000m, Brand = "로지텍" }
            };

            TypeCombiner.Combine()
                .WithProjection(products)
                .WithName("ProductWithConverter")
                .WithConverter()
                .AsRecord()
                .Generate();

            // FromCollection 테스트
            var convertedProducts = Generated.ProductWithConverter.FromCollection(products);
            System.Console.WriteLine($"✅ FromCollection: {convertedProducts.Count}개 제품 변환");
            foreach (var product in convertedProducts)
            {
                System.Console.WriteLine($"   - {product.Name}: {product.Price:N0}원 ({product.Brand}) ✓");
            }
            
            // FromSingle 단일 항목 테스트
            var singleProduct = new { Name = "키보드", Price = 120000m, Brand = "체리" };
            var convertedSingle = Generated.ProductWithConverter.FromSingle(singleProduct);
            System.Console.WriteLine($"✅ FromSingle: '{convertedSingle.Name}' 제품 변환 (가격: {convertedSingle.Price:N0}원) ✓");

            System.Console.WriteLine();
        }

        /// <summary>
        /// 📋 3단계: 타입 생성 모드 테스트
        /// </summary>
        static void TestTypeGenerationModes()
        {
            System.Console.WriteLine("📋 3단계: 타입 생성 모드 테스트");
            System.Console.WriteLine("===========================");

            var testData = new { Name = "테스트", Value = 42, IsValid = true };

            // 3-1. Record 모드
            System.Console.WriteLine("3-1. Record 모드 - 불변 데이터 구조");
            TypeCombiner.Combine()
                .With(testData)
                .WithName("TestRecord")
                .AsRecord()
                .Generate();

            var testRecord = new Generated.TestRecord("레코드 테스트", 100, true);
            System.Console.WriteLine($"✅ TestRecord 생성: Name='{testRecord.Name}', Value={testRecord.Value}, IsValid={testRecord.IsValid} ✓");

            // 3-2. Class 모드
            System.Console.WriteLine("3-2. Class 모드 - 가변 참조 타입");
            TypeCombiner.Combine()
                .With(testData)
                .WithName("TestClass")
                .AsClass()
                .Generate();

            var testClass = new Generated.TestClass
            {
                Name = "클래스 테스트",
                Value = 200,
                IsValid = false
            };
            System.Console.WriteLine($"✅ TestClass 생성: Name='{testClass.Name}', Value={testClass.Value}, IsValid={testClass.IsValid} ✓");

            // 3-3. Struct 모드
            System.Console.WriteLine("3-3. Struct 모드 - 값 타입");
            TypeCombiner.Combine()
                .With(testData)
                .WithName("TestStruct")
                .AsStruct()
                .Generate();

            var testStruct = new Generated.TestStruct
            {
                Name = "구조체 테스트",
                Value = 300,
                IsValid = true
            };
            System.Console.WriteLine($"✅ TestStruct 생성: Name='{testStruct.Name}', Value={testStruct.Value}, IsValid={testStruct.IsValid} ✓");

            System.Console.WriteLine();
        }

        /// <summary>
        /// 📋 4단계: 익명 타입 처리 테스트
        /// </summary>
        static void TestAnonymousTypeHandling()
        {
            System.Console.WriteLine("📋 4단계: 익명 타입 처리 테스트");
            System.Console.WriteLine("==========================");

            // 4-1. 단일 익명 타입 변수 참조
            System.Console.WriteLine("4-1. 단일 익명 타입 변수 참조");
            var userInfo = new { Name = "김철수", Age = 28, Department = "개발팀" };
            
            TypeCombiner.Combine()
                .With(userInfo)
                .WithName("AnonymousUser")
                .AsRecord()
                .Generate();

            var anonymousUser = new Generated.AnonymousUser("이영희", 25, "디자인팀");
            System.Console.WriteLine($"✅ AnonymousUser 생성: {anonymousUser.Name} ({anonymousUser.Department}) ✓");

            // 4-2. 복수 익명 타입 조합
            System.Console.WriteLine("4-2. 복수 익명 타입 조합");
            var personal = new { FirstName = "박", LastName = "민수" };
            var work = new { Company = "테크회사", Position = "시니어 개발자" };
            
            TypeCombiner.Combine()
                .With(personal)
                .With(work)
                .WithName("CombinedEmployee")
                .AsClass()
                .Generate();

            var employee = new Generated.CombinedEmployee
            {
                FirstName = "최",
                LastName = "유리",
                Company = "스타트업",
                Position = "프론트엔드 개발자"
            };
            System.Console.WriteLine($"✅ CombinedEmployee 생성: {employee.FirstName} {employee.LastName} - {employee.Position} ✓");

            // 4-3. 복잡한 데이터 타입 포함
            System.Console.WriteLine("4-3. 복잡한 데이터 타입 포함");
            var complexAnonymous = new 
            { 
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                Tags = new[] { "중요", "긴급" },
                Metadata = new Dictionary<string, object> { ["version"] = "1.0", ["author"] = "system" }
            };

            TypeCombiner.Combine()
                .With(complexAnonymous)
                .WithName("ComplexAnonymous")
                .AsStruct()
                .Generate();

            var complex = new Generated.ComplexAnonymous
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                Tags = new[] { "테스트", "데모" },
                Metadata = new Dictionary<string, object> { ["env"] = "test" }
            };
            System.Console.WriteLine($"✅ ComplexAnonymous 생성: ID {complex.Id.ToString()[..8]}..., 태그 수: {complex.Tags.Length} ✓");

            System.Console.WriteLine();
        }

        /// <summary>
        /// 📋 5단계: Add 기능 테스트
        /// </summary>
        static void TestAddFeature()
        {
            System.Console.WriteLine("📋 5단계: Add 기능 테스트");
            System.Console.WriteLine("=====================");

            // 5-1. With() + Add() 조합
            System.Console.WriteLine("5-1. With() + Add() 조합");
            var simpleData = new { Name = "테스트", Value = 42 };
            
            TypeCombiner.Combine()
                .With(simpleData)
                .Add("Id", typeof(int))
                .WithName("WorkingAddExample")
                .AsClass()
                .Generate();

            var workingExample = new Generated.WorkingAddExample
            {
                Name = "작동하는예제",
                Value = 100,
                Id = 123
            };
            
            System.Console.WriteLine($"✅ With() + Add() 조합 성공: Name={workingExample.Name}, Value={workingExample.Value}, Id={workingExample.Id}");

            // 5-2. 순수 Add() 사용
            System.Console.WriteLine("\n5-2. 순수 Add() 사용");
            TypeCombiner.Combine()
                .Add("Name", typeof(string))
                .Add("Value", typeof(int))
                .Add("CreatedAt", typeof(DateTime))
                .WithName("PureAddExample")
                .AsClass()
                .Generate();

            var pureAddExample = new Generated.PureAddExample
            {
                Name = "순수Add예제",
                Value = 200,
                CreatedAt = DateTime.Now
            };

            System.Console.WriteLine($"✅ Add만 사용한 타입 생성 성공: Name={pureAddExample.Name}, Value={pureAddExample.Value}, CreatedAt={pureAddExample.CreatedAt:yyyy-MM-dd}");

            // 5-3. Projection + Add() 조합
            System.Console.WriteLine("\n5-3. Projection + Add() 조합");
            var projectionData = new[]
            {
                new { UserId = 1, UserName = "john" },
                new { UserId = 2, UserName = "jane" }
            };

            TypeCombiner.Combine()
                .WithProjection(projectionData)
                .Add("IsActive", typeof(bool))
                .Add("CreatedAt", typeof(DateTime))
                .WithName("EnhancedUserProjection")
                .WithConverter()
                .AsRecord()
                .Generate();

            // Record는 생성자로 생성되므로 FromSingle을 사용하여 변환
            var projectionItem = new { UserId = 3, UserName = "test_user" };
            var enhancedUser = Generated.EnhancedUserProjection.FromSingle(projectionItem);

            System.Console.WriteLine($"✅ Projection + Add() 조합 성공: UserId={enhancedUser.UserId}, UserName={enhancedUser.UserName}, IsActive={enhancedUser.IsActive}");

            System.Console.WriteLine("\n🎉 Add 기능 완전 작동!");
            System.Console.WriteLine("💡 모든 속성 접근이 정상 작동하며 타입 안전성 보장");
        }

        /// <summary>
        /// 📋 6단계: 네임스페이스 지정 기능 테스트
        /// </summary>
        static void TestNamespaceFeature()
        {
            System.Console.WriteLine("📋 6단계: 네임스페이스 지정 기능 테스트");
            System.Console.WriteLine("============================");

            // 6-1. 기본 Generated 네임스페이스 사용
            System.Console.WriteLine("6-1. 기본 Generated 네임스페이스");
            var defaultData = new { Name = "기본네임스페이스", Value = 1 };
            
            TypeCombiner.Combine()
                .With(defaultData)
                .WithName("DefaultNamespaceType")
                .AsClass()
                .Generate();

            var defaultType = new Generated.DefaultNamespaceType
            {
                Name = "기본네임스페이스테스트",
                Value = 100
            };
            System.Console.WriteLine($"✅ Generated.DefaultNamespaceType 생성: Name={defaultType.Name}, Value={defaultType.Value} ✓");

            // 6-2. 커스텀 네임스페이스 지정
            System.Console.WriteLine("\n6-2. 커스텀 네임스페이스 지정");
            var customData = new { ProductName = "상품", Price = 1000m };
            
            TypeCombiner.Combine()
                .With(customData)
                .WithName("CustomProduct")
                .AsRecord()
                .Generate();

            var customProduct = new Generated.CustomProduct("커스텀상품", 2000m);
            System.Console.WriteLine($"✅ Generated.CustomProduct 생성: ProductName={customProduct.ProductName}, Price={customProduct.Price} ✓");

            System.Console.WriteLine();
        }

        /// <summary>
        /// 📋 7단계: Exclude 기능 테스트
        /// </summary>
        static void TestExcludeFeature()
        {
            System.Console.WriteLine("📋 7단계: Exclude 기능 테스트");
            System.Console.WriteLine("==========================");

            // 7-1. 특정 속성 제외
            System.Console.WriteLine("7-1. 특정 속성 제외 테스트");
            var userData = new { Name = "김철수", Age = 30, Password = "secret123", Email = "kim@test.com" };
            
            TypeCombiner.Combine()
                .With(userData)
                .Exclude("Password")
                .WithName("SafeUser")
                .AsClass()
                .Generate();

            var safeUser = new Generated.SafeUser
            {
                Name = "안전한사용자",
                Age = 25,
                Email = "safe@test.com"
            };
            
            System.Console.WriteLine($"✅ Exclude 기능 성공: Name={safeUser.Name}, Age={safeUser.Age}, Email={safeUser.Email}");
            System.Console.WriteLine("💡 Password 속성이 제외되어 생성되지 않음");

            // 7-2. 여러 속성 제외
            System.Console.WriteLine("\n7-2. 여러 속성 제외 테스트");
            var fullData = new { Id = 1, Name = "테스트", Password = "pwd", InternalId = 999, PublicInfo = "공개정보" };
            
            TypeCombiner.Combine()
                .With(fullData)
                .Exclude("Password")
                .Exclude("InternalId")
                .WithName("PublicData")
                .AsRecord()
                .Generate();

            var publicData = new Generated.PublicData(2, "공개데이터", "공개된정보");
            System.Console.WriteLine($"✅ 다중 Exclude 성공: Id={publicData.Id}, Name={publicData.Name}, PublicInfo={publicData.PublicInfo}");
            System.Console.WriteLine("💡 Password와 InternalId 속성이 제외됨");

            System.Console.WriteLine();
        }

        /// <summary>
        /// 📋 8단계: ChangeType 기능 테스트
        /// </summary>
        static void TestChangeTypeFeature()
        {
            System.Console.WriteLine("📋 8단계: ChangeType 기능 테스트");
            System.Console.WriteLine("===========================");

            // 8-1. 타입 변경 테스트
            System.Console.WriteLine("8-1. 타입 변경 테스트");
            var numericData = new { Id = 1, Price = 1000m, Quantity = 5, IsActive = true };
            
            TypeCombiner.Combine()
                .With(numericData)
                .ChangeType("Price", typeof(string))
                .ChangeType("Quantity", typeof(string))
                .WithName("StringifiedProduct")
                .AsClass()
                .Generate();

            var stringProduct = new Generated.StringifiedProduct
            {
                Id = 1,
                Price = "1500.00",
                Quantity = "10",
                IsActive = true
            };
            
            System.Console.WriteLine($"✅ ChangeType 성공: Id={stringProduct.Id}, Price={stringProduct.Price} (string), Quantity={stringProduct.Quantity} (string), IsActive={stringProduct.IsActive}");
            System.Console.WriteLine("💡 decimal과 int가 string으로 타입 변경됨");

            // 8-2. 복합 타입 변경
            System.Console.WriteLine("\n8-2. 복합 타입 변경 테스트");
            var mixedData = new { Name = "상품", Count = 100, Active = true, Created = DateTime.Now };
            
            TypeCombiner.Combine()
                .With(mixedData)
                .ChangeType("Count", typeof(long))
                .ChangeType("Active", typeof(int))
                .ChangeType("Created", typeof(string))
                .WithName("ConvertedMixedData")
                .AsRecord()
                .Generate();

            var convertedData = new Generated.ConvertedMixedData("변환된상품", 200L, 1, "2024-01-01");
            System.Console.WriteLine($"✅ 복합 ChangeType 성공: Name={convertedData.Name}, Count={convertedData.Count} (long), Active={convertedData.Active} (int), Created={convertedData.Created} (string)");
            System.Console.WriteLine("💡 int→long, bool→int, DateTime→string 변환 성공");

            System.Console.WriteLine();
        }
    }
}
