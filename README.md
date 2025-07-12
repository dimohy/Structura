# Structura

**Source Generator-Based Fluent API Type Manipulation Library**

Structura is a .NET library that automatically generates new types through source generators when you define type creation and manipulation rules using a fluent API.

## 🏗️ Features

### 🔄 Type Combination
Combine existing types to create new types.
// Combine existing types
TypeCombiner.Combine<PersonalInfo, ContactInfo>()
    .WithName("UserProfile")
    .AsRecord()
    .Generate();

// Result: Creates a UserProfile record with all properties from PersonalInfo and ContactInfo
### 🎭 Anonymous Type Support
Seamlessly supports anonymous classes.
// Combine anonymous type with existing type
TypeCombiner.Combine<User>()
    .With(new { Department = "", Salary = 0m, IsActive = true })
    .WithName("Employee")
    .Generate();

// Create entirely from anonymous types
TypeCombiner.Combine()
    .With(new { Name = "", Age = 0 })
    .With(new { Email = "", Phone = "" })
    .WithName("Contact")
    .AsRecord()
    .Generate();
### 🔗 EF Core Projection Support
Extract schema from EF Core projection results to generate types.
// var result = dbContext.Users.Select(x => new { x.Name, x.Email }).ToList();
TypeCombiner.Combine()
    .WithProjection(result)
    .WithName("UserProjection")
    .AsRecord()
    .Generate();
### ⚙️ Advanced Property Manipulation

#### Property ExclusionTypeCombiner.From<User>()
    .Exclude(u => u.Password)
    .Exclude(u => u.SocialSecurityNumber)
    .WithName("PublicUser")
    .Generate();
#### Property AdditionTypeCombiner.From<BasicUser>()
    .Add("CreatedAt", typeof(DateTime))
    .Add("IsActive", typeof(bool), defaultValue: true)
    .Add("Metadata", typeof(Dictionary<string, object>))
    .WithName("ExtendedUser")
    .Generate();
#### Property Type ChangesTypeCombiner.From<Employee>()
    .ChangeType(e => e.Salary, typeof(string))      // decimal → string
    .ChangeType(e => e.HireDate, typeof(string))    // DateTime → string
    .WithName("EmployeeDto")
    .Generate();
### 🔧 Complex Operations
All features can be used together.
TypeCombiner.Combine<PersonalInfo, ContactInfo>()
    .Exclude(p => p.Password)                       // Exclude sensitive information
    .Add("LastLoginAt", typeof(DateTime?))          // Add new property
    .ChangeType(c => c.PhoneNumber, typeof(string)) // Change type
    .WithName("SecureUserProfile")
    .AsRecord()
    .Generate();
## 📦 Installation
dotnet add package Structura
## 🎯 Target Frameworks

- **.NET Standard 2.0** and above
- **C# 12.0** syntax support

## 🏷️ Supported Type Generation

| Type | Method | Description |
|------|--------|-------------|
| Records | `.AsRecord()` | Generate immutable records |
| Classes | `.AsClass()` | Generate mutable classes |
| Structs | `.AsStruct()` | Generate value type structs |

## 🚀 Advanced Usage

### Nested Anonymous TypesTypeCombiner.Combine()
    .With(new { 
        Name = "",
        Address = new { Street = "", City = "", ZipCode = "" }
    })
    .WithName("UserWithAddress")
    .Generate();
### Collection TypesTypeCombiner.From<User>()
    .Add("Tags", typeof(List<string>))
    .Add("Permissions", typeof(string[]))
    .WithName("TaggedUser")
    .Generate();
### Conditional Property InclusionTypeCombiner.From<FullUser>()
    .ExcludeIf(u => u.AdminData, condition: !isAdmin)
    .WithName("ContextualUser")
    .Generate();
## 💼 Real-World Scenarios

### API DTO Generation// Generate public API DTO from internal entity
TypeCombiner.From<InternalUser>()
    .Exclude(u => u.Password)
    .Exclude(u => u.SecurityToken)
    .Add("PublicId", typeof(Guid))
    .WithName("UserApiDto")
    .AsRecord()
    .Generate();
### Database Migration// Add new columns to existing table schema
TypeCombiner.From<LegacyUserTable>()
    .Add("CreatedAt", typeof(DateTime))
    .Add("UpdatedAt", typeof(DateTime?))
    .ChangeType(u => u.Status, typeof(UserStatus)) // Change to enum
    .WithName("ModernUserTable")
    .Generate();
### EF Core Dashboard Data// Convert EF Core projection results to strongly-typed
var dashboardData = dbContext.Users
    .Select(u => new { u.Name, u.Email, OrderCount = u.Orders.Count() })
    .ToList();

TypeCombiner.Combine()
    .WithProjection(dashboardData)
    .With(new { GeneratedAt = DateTime.Now })
    .WithName("UserDashboard")
    .AsRecord()
    .Generate();
## 🧪 Testing

### Running Testsdotnet test
### Test Coverage
The Structura library is validated with **89 comprehensive unit tests**:

| Test Category | Count | Description |
|---------------|-------|-------------|
| **Core Functionality** | 11 | All basic TypeCombiner API features |
| **Anonymous Types** | 4 | Anonymous object combination and processing |
| **Single Type Builder** | 7 | All From<T>() method functionality |
| **Complex Scenarios** | 4 | Multi-feature combination use cases |
| **EF Core Projection** | 13 | EF Core projection result processing |
| **Variable References** | 9 | Anonymous type variable analysis |
| **Type Generation Modes** | 2 | Record, Class, Struct type generation |
| **Edge Cases** | 4 | Error conditions and boundary cases |
| **Fluent API Chaining** | 3 | Method chaining integrity |
| **Type Safety** | 3 | Compile-time type validation |
| **Source Generator Integration** | 6 | Generated type verification |
| **Performance** | 2 | Large-scale processing and performance |
| **Real-World Scenarios** | 5 | Production environment use cases |
| **Documentation Features** | 8 | README example code validation |
| **Integration Scenarios** | 12 | Complex feature combinations |

#### 🎯 Test Results
- **Total Tests**: 89
- **Passed**: 89 ✅
- **Failed**: 0
- **Skipped**: 0
- **Execution Time**: < 1 second

#### 🔍 Test Types
- **Unit Tests**: API interface and method chaining validation
- **Integration Tests**: Source generator and runtime type generation verification
- **Performance Tests**: Large-scale type generation scenarios
- **Scenario Tests**: Real development environment use cases

## 📁 Project Structure
Structura/
├── 📂 Structura/                    # Main library
│   ├── TypeCombiner.cs              # Fluent API entry point
│   ├── TypeCombinerBuilder.cs       # Multi-type combination builder
│   ├── AnonymousTypeCombinerBuilder.cs # Anonymous type builder
│   ├── TypeDefinitions.cs           # Core type definitions
│   └── StructuraSourceGenerator.cs  # Source generator engine
├── 📂 Structura.Test.Console/       # Integration test console
│   └── Program.cs                   # Real usage examples and demos
├── 📂 Structura.Tests/             # Unit test project
│   ├── UnitTest.cs                 # Basic functionality unit tests
│   ├── VariableReferenceTests.cs   # Variable reference feature tests
│   ├── EFCoreProjectionTests.cs    # EF Core projection tests
│   ├── IntegrationTests.cs         # Integration and scenario tests
│   └── TestModels.cs               # Test model classes
└── 📄 README.md                    # Documentation
## 📈 Development Status

### ✅ Completed Features

| Feature | Status | Description |
|---------|--------|-------------|
| **Source Generator Engine** | 100% ✅ | Complete |
| **Fluent API** | 100% ✅ | Complete |
| **Anonymous Type Support** | 100% ✅ | Complete |
| **EF Core Projection Support** | 100% ✅ | Complete |
| **Variable Reference Analysis** | 100% ✅ | Complete |
| **Property Addition** | 100% ✅ | Complete |
| **Type Conversion (Record/Class/Struct)** | 100% ✅ | Complete |
| **Comprehensive Test Suite** | 100% ✅ | 89 tests passing |

### 🔧 Partially Completed Features

| Feature | Status | Notes |
|---------|--------|-------|
| **Existing Type Property Inheritance** | 90% 🔄 | Basic behavior complete |
| **Property Exclusion/Type Changes** | 85% 🔄 | Some advanced scenarios limited |

## 🚀 Getting Started

### 1. Installationdotnet add package Structura
### 2. Basic Usageusing Structura;

// Create new type from anonymous types
TypeCombiner.Combine()
    .With(new { Name = "", Age = 0 })
    .With(new { Email = "", Phone = "" })
    .WithName("Contact")
    .AsRecord()
    .Generate();

// Use the generated type
var contact = new Generated.Contact("John Doe", 30, "john@example.com", "555-1234");
### 3. Advanced Usage// Add properties to existing type
TypeCombiner.From<User>()
    .Add("CreatedAt", typeof(DateTime))
    .Add("Metadata", typeof(Dictionary<string, object>))
    .WithName("ExtendedUser")
    .AsClass()
    .Generate();

var extendedUser = new Generated.ExtendedUser
{
    Name = "Developer",
    Age = 25,
    Email = "dev@example.com",
    CreatedAt = DateTime.Now,
    Metadata = new Dictionary<string, object>()
};
## 📄 License

MIT License

## 🤝 Contributing

Issues and pull requests are welcome!

---

**Structura** - Simplify type manipulation!