# Structura NuGet Package Build Instructions

## Building the Package

### Prerequisites
- .NET SDK 8.0 or later
- Visual Studio 2022 or VS Code with C# extension

### Build Commands

#### 1. Clean and Builddotnet clean
dotnet build --configuration Release
#### 2. Run Testsdotnet test --configuration Release
#### 3. Create NuGet Packagedotnet pack --configuration Release --output ./nupkg
#### 4. Publish to NuGet (when ready)# Test publish (optional)
dotnet nuget push ./nupkg/Structura.0.9.0-beta.nupkg --source https://api.nuget.org/v3/index.json --api-key YOUR_API_KEY --skip-duplicate

# Or publish to local feed for testing
dotnet nuget push ./nupkg/Structura.0.9.0-beta.nupkg --source ./local-feed
## Package Information

- **Package ID**: Structura
- **Version**: 0.9.0-beta
- **Target Framework**: .NET Standard 2.0
- **Package Type**: Source Generator / Analyzer
- **Author**: dimohy
- **Repository**: https://github.com/dimohy/structura

## Included Files

- Main library assembly
- Source generator
- README.md
- LICENSE
- Symbol files (.pdb)

## Beta Release Notes

This is a beta release of Structura with the following features:

### ? Completed Features
- Source generator engine (100%)
- Fluent API (100%)
- Anonymous type support (100%)
- Variable reference analysis (100%)
- EF Core projection support (100%)
- Property manipulation (100%)
- Multiple type generation modes (100%)
- **?? Smart Converter Extensions (100%)**
- Comprehensive test suite (100+ tests)

### ?? Smart Converter Extensions

The revolutionary `.WithConverter()` method enables seamless conversion from anonymous objects to strongly-typed instances:
// EF Core projection with smart conversion
var efResult = dbContext.Users.Select(x => new { x.Name, x.Email }).ToList();

TypeCombiner.Combine()
    .WithProjection(efResult)
    .WithName("UserDto")
    .WithConverter()  // ?? Enable smart conversion
    .AsRecord()
    .Generate();

// One-liner conversion with full type safety
List<Generated.UserDto> typed = efResult.ToUserDto();
### ?? In Development
- Advanced property manipulation scenarios (90%)
- Existing type property inheritance (95%)

### ?? Beta Limitations
- Some complex nested type scenarios may have limitations
- Advanced property manipulation edge cases

## Installation
dotnet add package Structura --version 0.9.0-beta
## Usage Example
using Structura;

// Basic type combination
TypeCombiner.Combine<PersonalInfo, ContactInfo>()
    .WithName("UserProfile")
    .AsRecord()
    .Generate();

// Anonymous type support with converter
var userInfo = new { Name = "John", Age = 30 };
TypeCombiner.Combine()
    .With(userInfo)
    .WithName("User")
    .WithConverter()  // ?? Enable smart conversion
    .AsClass()
    .Generate();

// Convert anonymous object to strongly-typed instance
Generated.User typedUser = userInfo.ToUser();
## Test Coverage

The package includes comprehensive test coverage:

| Test Category | Test Count | Description |
|---------------|------------|-------------|
| **Core Functionality** | 11 tests | Basic TypeCombiner API features |
| **Anonymous Types** | 4 tests | Anonymous object combination |
| **Single Type Builder** | 7 tests | From<T>() method functionality |
| **EF Core Projection** | 13 tests | EF Core projection processing |
| **Variable References** | 9 tests | Anonymous type variable analysis |
| **?? Converter Extensions** | 15 tests | Smart converter functionality |
| **?? Converter Integration** | 8 tests | Integration scenarios |
| **Other Categories** | 35+ tests | Additional comprehensive coverage |

**Total: 100+ tests - All passing ?**

## Performance Characteristics

- **Compile-time Generation**: Zero runtime overhead
- **Type Safety**: Full compile-time type checking
- **Smart Conversion**: Intelligent property mapping with type conversion
- **Memory Efficient**: No reflection-based conversion at runtime
- **IntelliSense Support**: Full IDE support for generated types and converters

## Support

For issues, questions, or contributions:
- GitHub: https://github.com/dimohy/structura
- Issues: https://github.com/dimohy/structura/issues

## Advanced Features

### Smart Converter Features

- **Automatic Property Matching**: Case-insensitive name-based mapping
- **Intelligent Type Conversion**: Handles primitives, nullables, enums, collections
- **Null Safety**: Automatic null handling for nullable types
- **Error Handling**: Meaningful error messages for conversion failures
- **Universal Support**: Works with Records, Classes, and Structs
- **Performance Optimized**: Generated code optimized for maximum efficiency

### Real-World Scenarios

1. **EF Core Projections**: Convert query results to DTOs
2. **API Response Mapping**: Transform internal models to public DTOs
3. **Data Transformation**: Combine data from multiple sources
4. **Migration Support**: Add properties for schema evolution
5. **Testing**: Generate test models with specific property combinations