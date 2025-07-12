using Structura.Tests.TestModels;

namespace Structura.Tests
{
    /// <summary>
    /// Basic functionality tests for Structura library
    /// </summary>
    public class BasicTypeCombinerTests
    {
        [Fact]
        public void TypeCombiner_Should_ExistAndNotBeNull()
        {
            // Arrange & Act
            var combiner = TypeCombiner.Combine<PersonalInfo, ContactInfo>();

            // Assert
            Assert.NotNull(combiner);
        }

        [Fact]
        public void TypeCombiner_From_Should_ReturnValidBuilder()
        {
            // Arrange & Act
            var builder = TypeCombiner.From<User>();

            // Assert
            Assert.NotNull(builder);
        }

        [Fact]
        public void TypeCombiner_Combine_WithoutGenerics_Should_ReturnValidBuilder()
        {
            // Arrange & Act
            var builder = TypeCombiner.Combine();

            // Assert
            Assert.NotNull(builder);
        }

        [Fact]
        public void TypeCombiner_Combine_WithSingleGeneric_Should_ReturnValidBuilder()
        {
            // Arrange & Act
            var builder = TypeCombiner.Combine<User>();

            // Assert
            Assert.NotNull(builder);
        }

        [Fact]
        public void TypeCombinerBuilder_WithName_Should_ReturnSameBuilder()
        {
            // Arrange
            var builder = TypeCombiner.Combine<PersonalInfo, ContactInfo>();

            // Act
            var result = builder.WithName("TestType");

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void TypeCombinerBuilder_AsRecord_Should_ReturnSameBuilder()
        {
            // Arrange
            var builder = TypeCombiner.Combine<PersonalInfo, ContactInfo>();

            // Act
            var result = builder.AsRecord();

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void TypeCombinerBuilder_AsClass_Should_ReturnSameBuilder()
        {
            // Arrange
            var builder = TypeCombiner.Combine<PersonalInfo, ContactInfo>();

            // Act
            var result = builder.AsClass();

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void TypeCombinerBuilder_AsStruct_Should_ReturnSameBuilder()
        {
            // Arrange
            var builder = TypeCombiner.Combine<PersonalInfo, ContactInfo>();

            // Act
            var result = builder.AsStruct();

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void TypeCombinerBuilder_Generate_Should_NotThrow()
        {
            // Arrange
            var builder = TypeCombiner.Combine<PersonalInfo, ContactInfo>()
                .WithName("TestUserProfile")
                .AsRecord();

            // Act & Assert
            var exception = Record.Exception(() => builder.Generate());
            Assert.Null(exception);
        }
    }

    /// <summary>
    /// Anonymous type combination functionality tests
    /// </summary>
    public class AnonymousTypeCombinerTests
    {
        [Fact]
        public void AnonymousTypeCombiner_With_Should_ReturnSameBuilder()
        {
            // Arrange
            var builder = TypeCombiner.Combine();

            // Act
            var result = builder.With(new { Name = "", Age = 0 });

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void AnonymousTypeCombiner_MultipleWith_Should_ReturnSameBuilder()
        {
            // Arrange
            var builder = TypeCombiner.Combine();

            // Act
            var result = builder
                .With(new { Name = "", Age = 0 })
                .With(new { Email = "", Phone = "" });

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void AnonymousTypeCombiner_WithName_Should_ReturnSameBuilder()
        {
            // Arrange
            var builder = TypeCombiner.Combine();

            // Act
            var result = builder.WithName("TestContact");

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void AnonymousTypeCombiner_Generate_Should_NotThrow()
        {
            // Arrange
            var builder = TypeCombiner.Combine()
                .With(new { Name = "", Age = 0 })
                .With(new { Email = "", Phone = "" })
                .WithName("TestContact")
                .AsRecord();

            // Act & Assert
            var exception = Record.Exception(() => builder.Generate());
            Assert.Null(exception);
        }
    }

    /// <summary>
    /// Single type builder tests
    /// </summary>
    public class SingleTypeCombinerTests
    {
        [Fact]
        public void SingleTypeCombiner_Add_Should_ReturnSameBuilder()
        {
            // Arrange
            var builder = TypeCombiner.From<User>();

            // Act
            var result = builder.Add("CreatedAt", typeof(DateTime));

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void SingleTypeCombiner_Exclude_Should_ReturnSameBuilder()
        {
            // Arrange
            var builder = TypeCombiner.From<PersonalInfo>();

            // Act
            var result = builder.Exclude(p => p.Password);

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void SingleTypeCombiner_ChangeType_Should_ReturnSameBuilder()
        {
            // Arrange
            var builder = TypeCombiner.From<Product>();

            // Act
            var result = builder.ChangeType(p => p.Price, typeof(string));

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void SingleTypeCombiner_With_Should_ReturnSameBuilder()
        {
            // Arrange
            var builder = TypeCombiner.From<User>();

            // Act
            var result = builder.With(new { Department = "", Salary = 0m });

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void SingleTypeCombiner_ExcludeIf_Should_ReturnSameBuilder()
        {
            // Arrange
            var builder = TypeCombiner.From<PersonalInfo>();

            // Act
            var result = builder.ExcludeIf(p => p.Password, true);

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void SingleTypeCombiner_ChainedCalls_Should_Work()
        {
            // Arrange
            var builder = TypeCombiner.From<User>();

            // Act
            var result = builder
                .Add("CreatedAt", typeof(DateTime))
                .Add("IsVerified", typeof(bool))
                .Exclude(u => u.IsActive)
                .WithName("ModifiedUser")
                .AsClass();

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void SingleTypeCombiner_Generate_Should_NotThrow()
        {
            // Arrange
            var builder = TypeCombiner.From<User>()
                .Add("CreatedAt", typeof(DateTime))
                .WithName("ExtendedUser")
                .AsClass();

            // Act & Assert
            var exception = Record.Exception(() => builder.Generate());
            Assert.Null(exception);
        }
    }

    /// <summary>
    /// Complex scenario tests
    /// </summary>
    public class ComplexScenarioTests
    {
        [Fact]
        public void ComplexScenario_MultipleTypesCombination_Should_NotThrow()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine<PersonalInfo, ContactInfo>()
                    .Exclude<PersonalInfo>(p => p.Password)
                    .Add("CreatedAt", typeof(DateTime))
                    .Add("IsVerified", typeof(bool))
                    .ChangeType<ContactInfo>(c => c.PhoneNumber, typeof(string))
                    .WithName("ComplexUserProfile")
                    .AsRecord()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void ComplexScenario_AnonymousTypesWithExistingType_Should_NotThrow()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine<User>()
                    .With(new { Department = "", Salary = 0m, Position = "" })
                    .Add("LastLogin", typeof(DateTime?))
                    .WithName("ComplexEmployee")
                    .AsRecord()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void ComplexScenario_MultipleAnonymousTypes_Should_NotThrow()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine()
                    .With(new { UserId = Guid.Empty, Username = "" })
                    .With(new { CreatedAt = DateTime.Now, IsActive = true })
                    .With(new { Permissions = new string[] { }, Role = "" })
                    .WithName("ComplexUserAccount")
                    .AsClass()
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void ComplexScenario_ProductWithInventory_Should_NotThrow()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() =>
            {
                TypeCombiner.From<Product>()
                    .Add("LastRestocked", typeof(DateTime?))
                    .Add("MinimumStock", typeof(int))
                    .Add("Supplier", typeof(string))
                    .ChangeType(p => p.Price, typeof(string))
                    .WithName("ProductWithInventory")
                    .AsStruct()
                    .Generate();
            });

            Assert.Null(exception);
        }
    }

    /// <summary>
    /// Type generation mode tests
    /// </summary>
    public class TypeGenerationModeTests
    {
        [Theory]
        [InlineData(TypeGenerationMode.Record)]
        [InlineData(TypeGenerationMode.Class)]
        [InlineData(TypeGenerationMode.Struct)]
        public void TypeGenerationMode_AllModes_Should_BeValid(TypeGenerationMode mode)
        {
            // Assert
            Assert.True(Enum.IsDefined(typeof(TypeGenerationMode), mode));
        }

        [Fact]
        public void TypeGenerationMode_Should_HaveThreeValues()
        {
            // Arrange
            var values = Enum.GetValues<TypeGenerationMode>();

            // Assert
            Assert.Equal(3, values.Length);
            Assert.Contains(TypeGenerationMode.Record, values);
            Assert.Contains(TypeGenerationMode.Class, values);
            Assert.Contains(TypeGenerationMode.Struct, values);
        }
    }

    /// <summary>
    /// Edge cases and error handling tests
    /// </summary>
    public class EdgeCaseTests
    {
        [Fact]
        public void TypeCombiner_WithEmptyName_Should_NotThrow()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() =>
            {
                TypeCombiner.Combine<User>()
                    .WithName("")
                    .Generate();
            });

            Assert.Null(exception);
        }

        [Fact]
        public void TypeCombiner_WithNullObjectInWith_Should_NotThrow()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() =>
            {
                var nullObject = (object)null!;
                TypeCombiner.Combine()
                    .With(nullObject)
                    .WithName("TestType")
                    .Generate();
            });

            // Should not throw during build, may throw during generation
            Assert.Null(exception);
        }

        [Fact]
        public void TypeCombiner_MultipleGenerateCalls_Should_NotThrow()
        {
            // Arrange
            var builder = TypeCombiner.From<SimpleModel>()
                .WithName("MultipleGenerateTest");

            // Act & Assert
            var exception1 = Record.Exception(() => builder.Generate());
            var exception2 = Record.Exception(() => builder.Generate());

            Assert.Null(exception1);
            Assert.Null(exception2);
        }

        [Fact]
        public void TypeCombiner_ExcludeNonExistentProperty_Should_NotThrow()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() =>
            {
                TypeCombiner.From<SimpleModel>()
                    .Add("NonExistent", typeof(string))
                    .WithName("TestType")
                    .Generate();
            });

            Assert.Null(exception);
        }
    }

    /// <summary>
    /// Fluent API chaining tests
    /// </summary>
    public class FluentApiChainTests
    {
        [Fact]
        public void FluentChain_LongChain_Should_Work()
        {
            // Arrange & Act
            var result = TypeCombiner.From<User>()
                .Add("CreatedAt", typeof(DateTime))
                .Add("UpdatedAt", typeof(DateTime?))
                .Add("Metadata", typeof(Dictionary<string, object>))
                .Exclude(u => u.IsActive)
                .ChangeType(u => u.Age, typeof(string))
                .With(new { Department = "", Role = "" })
                .WithName("ChainTestUser")
                .AsClass();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void FluentChain_AllTypeModes_Should_Work()
        {
            // Arrange & Act & Assert
            var recordException = Record.Exception(() =>
                TypeCombiner.From<SimpleModel>()
                    .WithName("RecordTest")
                    .AsRecord()
                    .Generate());

            var classException = Record.Exception(() =>
                TypeCombiner.From<SimpleModel>()
                    .WithName("ClassTest")
                    .AsClass()
                    .Generate());

            var structException = Record.Exception(() =>
                TypeCombiner.From<SimpleModel>()
                    .WithName("StructTest")
                    .AsStruct()
                    .Generate());

            Assert.Null(recordException);
            Assert.Null(classException);
            Assert.Null(structException);
        }

        [Fact]
        public void FluentChain_TypeCombination_AllVariants_Should_Work()
        {
            // Test all TypeCombiner.Combine variants
            var twoTypesException = Record.Exception(() =>
                TypeCombiner.Combine<PersonalInfo, ContactInfo>()
                    .WithName("TwoTypesTest")
                    .Generate());

            var singleTypeException = Record.Exception(() =>
                TypeCombiner.Combine<User>()
                    .WithName("SingleTypeTest")
                    .Generate());

            var noTypesException = Record.Exception(() =>
                TypeCombiner.Combine()
                    .WithName("NoTypesTest")
                    .Generate());

            Assert.Null(twoTypesException);
            Assert.Null(singleTypeException);
            Assert.Null(noTypesException);
        }
    }

    /// <summary>
    /// Type safety tests
    /// </summary>
    public class TypeSafetyTests
    {
        [Fact]
        public void TypeSafety_ExcludeExpression_Should_BeTypeSafe()
        {
            // Arrange
            var builder = TypeCombiner.From<PersonalInfo>();

            // Act & Assert - Compile-time type safety guaranteed
            var result = builder.Exclude(p => p.FirstName);
            Assert.NotNull(result);
        }

        [Fact]
        public void TypeSafety_ChangeTypeExpression_Should_BeTypeSafe()
        {
            // Arrange
            var builder = TypeCombiner.From<Product>();

            // Act & Assert - Compile-time type safety guaranteed
            var result = builder.ChangeType(p => p.Price, typeof(string));
            Assert.NotNull(result);
        }

        [Fact]
        public void TypeSafety_GenericTypeParameters_Should_BePreserved()
        {
            // Arrange & Act
            var builderTwoTypes = TypeCombiner.Combine<PersonalInfo, ContactInfo>();
            var builderSingleType = TypeCombiner.Combine<User>();
            var builderFromType = TypeCombiner.From<Product>();

            // Assert - Compile-time generic type parameter preservation
            Assert.NotNull(builderTwoTypes);
            Assert.NotNull(builderSingleType);
            Assert.NotNull(builderFromType);
        }
    }
}
