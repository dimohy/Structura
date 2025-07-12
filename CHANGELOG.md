# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.9.0-beta] - 2024-12-19

### ?? Added
- **Source Generator Engine**: Complete implementation of incremental source generator
- **Fluent API**: Comprehensive fluent API with method chaining support
- **Type Combination**: Ability to combine multiple existing types into new types
- **Anonymous Type Support**: Full support for anonymous types with variable reference analysis
- **EF Core Projection Support**: Convert EF Core projection results to strongly-typed types
- **?? Smart Converter Extensions**: Revolutionary `.WithConverter()` method that generates extension methods for seamless object conversion
- **Property Manipulation**: Advanced property manipulation capabilities
  - Add new properties with `Add(name, type)`
  - Exclude properties with `Exclude(expression)`
  - Conditional exclusion with `ExcludeIf(expression, condition)`
  - Change property types with `ChangeType(expression, newType)`
- **Multiple Generation Modes**: Support for Records, Classes, and Structs
- **Variable Reference Analysis**: Intelligent analysis of anonymous type variables
- **Collection Type Support**: Full support for Lists, Arrays, and Dictionaries
- **Comprehensive Test Suite**: 100+ unit tests covering all functionality

### ?? Technical Features
- **.NET Standard 2.0** compatibility for broad framework support
- **C# 12.0** syntax support with latest language features
- **Incremental Source Generator** for optimal performance
- **Compile-time Type Safety** with expression-based property selection
- **Rich Type Inference** for anonymous types and projections
- **?? Conditional Code Generation**: Converter extensions only generated when `.WithConverter()` is used

### ?? API Reference

#### Core Entry PointsTypeCombiner.Combine<T1, T2>()     // Combine two types
TypeCombiner.Combine<T>()          // Start with single type  
TypeCombiner.Combine()             // Anonymous types only
TypeCombiner.From<T>()             // Extend existing type
#### Fluent Methods.With(anonymousObject)             // Add anonymous type
.WithProjection(efCoreResult)      // Add EF projection
.Add(name, type)                   // Add property
.Exclude(x => x.Property)          // Exclude property
.ChangeType(x => x.Prop, newType)  // Change property type
.WithConverter()                   // ?? Enable smart converter extensions
.WithName("TypeName")              // Set generated type name
.AsRecord() / .AsClass() / .AsStruct() // Set generation mode
.Generate()                        // Execute generation
#### ?? Smart Converter Extensions

When `.WithConverter()` is enabled, the following extension methods are automatically generated:
// Collection conversion
List<Generated.TypeName> typed = anonymousCollection.ToTypeName();

// Single object conversion
Generated.TypeName typed = anonymousObject.ToTypeName();
**Features of Smart Converter:**
- **Intelligent Type Conversion**: Handles primitives, nullables, enums, and collections
- **Case-Insensitive Mapping**: Properties matched by name regardless of case
- **Null Safety**: Automatic handling of nullable types and null values
- **Error Handling**: Meaningful error messages for conversion failures
- **Universal Support**: Works with all generation modes (Record, Class, Struct)

### ?? Test Coverage
- **Basic Functionality**: 11 tests
- **Anonymous Types**: 4 tests  
- **Single Type Builder**: 7 tests
- **Complex Scenarios**: 4 tests
- **EF Core Projection**: 13 tests
- **Variable References**: 9 tests
- **Type Generation Modes**: 2 tests
- **Edge Cases**: 4 tests
- **Fluent API**: 3 tests
- **Type Safety**: 3 tests
- **Integration**: 18 tests
- **Performance**: 2 tests
- **Real-World Scenarios**: 5 tests
- **Documentation Examples**: 8 tests
- **?? Converter Extensions**: 15 tests
- **?? Converter Integration**: 8 tests

**Total: 100+ tests - All passing ?**

### ?? Real-World Use Cases

#### ?? EF Core Projection with Smart Conversion// 1. EF Core projection
var customerData = await dbContext.Customers
    .Select(c => new { c.Id, c.Name, c.Email, OrderCount = c.Orders.Count() })
    .ToListAsync();

// 2. Generate strongly-typed DTO with converter
TypeCombiner.Combine()
    .WithProjection(customerData)
    .With(new { ReportDate = DateTime.Now })
    .WithName("CustomerReport")
    .WithConverter()  // ?? Enable smart conversion
    .AsRecord()
    .Generate();

// 3. Seamlessly convert to strongly-typed collection
List<Generated.CustomerReport> reports = customerData.ToCustomerReport();
#### API DTO GenerationTypeCombiner.From<PersonalInfo>()
    .Exclude(u => u.Password)
    .Add("PublicId", typeof(Guid))
    .WithConverter()
    .WithName("UserApiDto")
    .AsRecord()
    .Generate();

// Convert internal entity to public DTO
Generated.UserApiDto dto = internalUser.ToUserApiDto();
#### Database MigrationTypeCombiner.From<User>()
    .Add("CreatedAt", typeof(DateTime))
    .Add("UpdatedAt", typeof(DateTime?))
    .ChangeType(u => u.IsActive, typeof(int)) // bool ¡æ int conversion
    .WithName("ModernUserTable")
    .Generate();
#### Complex Business Logicvar personalData = new { FirstName = "John", LastName = "Developer", Age = 28 };
var workData = new { Company = "TechCompany", Position = "Backend Developer", Salary = 60000m };

TypeCombiner.Combine()
    .With(personalData)
    .With(workData)
    .WithConverter()
    .WithName("EmployeeProfile")
    .AsClass()
    .Generate();

// Convert combined anonymous data to strongly-typed object
var combined = new { /* ... combined properties ... */ };
Generated.EmployeeProfile employee = combined.ToEmployeeProfile();
### ?? Documentation
- Complete README with examples and API reference
- Comprehensive test suite demonstrating all features
- Build instructions and deployment guide
- Real-world usage scenarios and best practices
- ?? Smart converter usage examples and integration patterns

### ?? Beta Status
This is a beta release with the following completion status:
- **Core Features**: 100% complete
- **?? Smart Converter Extensions**: 100% complete
- **Advanced Property Manipulation**: 90% complete
- **Existing Type Property Inheritance**: 95% complete

### ?? Known Issues
- Some complex nested type scenarios may have limitations
- Advanced property manipulation edge cases need refinement
- Generated type discovery in some IDE scenarios

### ??? Breaking Changes
None - This is the initial public release with backward compatibility maintained.

### ?? Package Information
- **Package ID**: Structura
- **Target Framework**: .NET Standard 2.0
- **Dependencies**: Microsoft.CodeAnalysis.CSharp 4.5.0
- **Package Type**: Source Generator/Analyzer
- **Author**: dimohy
- **Repository**: https://github.com/dimohy/structura

### ?? What's New in 0.9.0-beta

#### ?? Game-Changing Feature: Smart Converter Extensions

The `.WithConverter()` method revolutionizes how you work with EF Core projections and anonymous objects:

**Before (Manual Conversion):**var efResult = dbContext.Users.Select(x => new { x.Name, x.Email }).ToList();

// Manual, error-prone conversion
var typed = efResult.Select(x => new UserDto 
{ 
    Name = x.Name, 
    Email = x.Email 
}).ToList();
**After (Smart Conversion):**var efResult = dbContext.Users.Select(x => new { x.Name, x.Email }).ToList();

TypeCombiner.Combine()
    .WithProjection(efResult)
    .WithName("UserDto")
    .WithConverter()  // ?? Enable magic
    .Generate();

// One-liner, type-safe conversion with IntelliSense
List<Generated.UserDto> typed = efResult.ToUserDto();
#### ?? Key Benefits of Smart Converter

1. **Zero Runtime Overhead**: All conversion logic generated at compile-time
2. **Type Safety**: Full compile-time type checking with IntelliSense
3. **Intelligent Mapping**: Automatic property matching with smart type conversion
4. **Universal Support**: Works with all generation modes and builder types
5. **Error Resilience**: Meaningful error messages for conversion issues
6. **Performance**: Optimized generated code for maximum efficiency

---

## [Unreleased]

### Planned for 1.0.0
- Complete advanced property manipulation scenarios
- Enhanced nested type support
- Performance optimizations for large projections
- Additional IDE tooling support
- More comprehensive error messages
- Documentation improvements
- Advanced converter configuration options
- Custom conversion logic support

### Future Enhancements
- **Custom Converter Logic**: Allow custom conversion functions
- **Validation Integration**: Built-in validation support for converted objects
- **Async Conversion**: Support for async conversion scenarios
- **Batch Conversion Optimization**: Performance optimizations for large datasets
- **IDE Extensions**: Visual Studio and VS Code extensions for better developer experience

---

*For the latest updates and detailed documentation, visit [GitHub Repository](https://github.com/dimohy/structura)*