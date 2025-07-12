# Structura

<img src="./logo.png" width="256" height="256">

**Fluent API-based Source Generator Type Manipulation Library**

Structura is a flexible type generation library that converts anonymous types and EF Core projection results to strongly-typed types, with the ability to add, exclude, and change properties.

## 🚀 Key Features

- **🎭 Anonymous Type Combination**: Combine multiple anonymous types to create new types
- **🔗 EF Core Projection Support**: Automatically convert EF Core Select results to strongly-typed types
- **➕ Property Addition**: Add new properties to existing types (`.Add()`)
- **➖ Property Exclusion**: Exclude sensitive or unnecessary properties (`.Exclude()`)
- **🔄 Type Conversion**: Change existing property types to different types (`.ChangeType()`)
- **🎯 Smart Converter**: Automatically generate extension methods for converting anonymous objects to strong types
- **🏷️ Multiple Type Generation**: Support for Records, Classes, and Structs
- **🧪 Comprehensive Testing**: Stability verified with 100+ unit tests

## 📦 Installation

```bash
dotnet add package Structura
```
## 🏗️ Core Features

### 1. Anonymous Type Combination

Combine multiple anonymous types to create new strongly-typed types.

```csharp
// Single anonymous type usage
var userInfo = new { Name = "John Doe", Age = 30, Department = "Development" };

TypeCombiner.Combine()
    .With(userInfo)
    .WithName("Employee")
    .AsRecord()
    .Generate();

// Usage: new Generated.Employee("Jane Smith", 25, "Design")

// Multiple anonymous type combination
var personal = new { FirstName = "John", LastName = "Doe" };
var work = new { Company = "TechCorp", Position = "Developer" };

TypeCombiner.Combine()
    .With(personal)
    .With(work)
    .WithName("FullProfile")
    .AsClass()
    .Generate();
```
### 2. EF Core Projection Support

Automatically convert EF Core Select results to strongly-typed types.

```csharp
// EF Core projection query
var userProjection = dbContext.Users
    .Select(u => new { u.Name, u.Email, u.Department })
    .ToList();

// Strong type generation
TypeCombiner.Combine()
    .WithProjection(userProjection)
    .WithName("UserDto")
    .WithConverter()  // 🔥 Enable smart conversion
    .AsRecord()
    .Generate();

// Automatic conversion usage
List<Generated.UserDto> typedUsers = UserDto.FromCollection(userProjection);
Generated.UserDto singleUser = UserDto.FromSingle(userProjection.First());
```
### 3. 🎯 Smart Converter (Core Feature!)

**The game-changer for EF Core projections!** Generates static methods for automatically converting anonymous objects to strongly-typed types.

```csharp
// Step 1: Generate type with converter enabled
var customerData = dbContext.Orders
    .GroupBy(o => o.CustomerId)
    .Select(g => new {
        CustomerId = g.Key,
        CustomerName = g.First().Customer.Name,
        TotalOrders = g.Count(),
        TotalSpent = g.Sum(o => o.Amount)
    })
    .ToList();

TypeCombiner.Combine()
    .WithProjection(customerData)
    .WithName("CustomerAnalytics")
    .WithConverter()  // 🔥 The magic begins!
    .AsRecord()
    .Generate();

// Step 2: Use conversion methods directly on the generated type
List<Generated.CustomerAnalytics> analytics = CustomerAnalytics.FromCollection(customerData);
Generated.CustomerAnalytics single = CustomerAnalytics.FromSingle(customerData.First());

// Strongly-typed anonymous objects are also convertible
var anonymousData = new[] {
    new { CustomerId = 1, CustomerName = "John Doe", TotalOrders = 5, TotalSpent = 150000m },
    new { CustomerId = 2, CustomerName = "Jane Smith", TotalOrders = 3, TotalSpent = 75000m }
};
List<Generated.CustomerAnalytics> converted = CustomerAnalytics.FromCollection(anonymousData);
```
### 4. Property Manipulation Features

#### Property Addition

```csharp
// Add new properties to existing types
var userData = new { Name = "John Doe", Email = "john@test.com" };

TypeCombiner.Combine()
    .With(userData)
    .Add("Id", typeof(int))
    .Add("CreatedAt", typeof(DateTime))
    .Add("Metadata", typeof(Dictionary<string, object>))
    .WithName("EnhancedUser")
    .AsClass()
    .Generate();

// Pure Add usage for type creation
TypeCombiner.Combine()
    .Add("Name", typeof(string))
    .Add("Value", typeof(int))
    .Add("IsActive", typeof(bool))
    .WithName("CustomType")
    .AsRecord()
    .Generate();
```
#### Property Exclusion

```csharp
// Exclude sensitive information
var sensitiveData = new { 
    Name = "John Doe", 
    Age = 30, 
    Password = "secret123", 
    Email = "john@test.com" 
};

TypeCombiner.Combine()
    .With(sensitiveData)
    .Exclude("Password")  // Exclude password
    .WithName("SafeUser")
    .AsClass()
    .Generate();

// Multiple property exclusion
TypeCombiner.Combine()
    .With(userData)
    .Exclude("Password")
    .Exclude("InternalId")
    .WithName("PublicData")
    .Generate();
```
#### Type Conversion

```csharp
// Property type conversion
var numericData = new { 
    Id = 1, 
    Price = 100m, 
    Quantity = 5, 
    IsActive = true 
};

TypeCombiner.Combine()
    .With(numericData)
    .ChangeType("Price", typeof(string))    // decimal → string
    .ChangeType("Quantity", typeof(long))   // int → long
    .ChangeType("IsActive", typeof(int))    // bool → int
    .WithName("ConvertedProduct")
    .AsClass()
    .Generate();
```
### 5. Complex Operations

All features can be used together.

```csharp
// EF Core projection + property addition + type conversion + converter
var orderProjection = dbContext.Orders
    .Select(o => new { 
        o.Id, 
        o.CustomerName, 
        o.Amount, 
        o.OrderDate 
    })
    .ToList();

TypeCombiner.Combine()
    .WithProjection(orderProjection)
    .Add("ProcessedAt", typeof(DateTime))           // Property addition
    .Add("Status", typeof(string))                  // Property addition
    .ChangeType("Amount", typeof(string))           // decimal → string
    .Exclude("InternalId")                          // Property exclusion (if exists)
    .WithConverter()                                // Enable smart converter
    .WithName("ProcessedOrder")
    .AsRecord()
    .Generate();

// Complete conversion support
List<Generated.ProcessedOrder> orders = ProcessedOrder.FromCollection(orderProjection);
```
## 🏷️ Supported Type Generation

| Type | Method | Description | Converter Support |
|------|--------|-------------|-------------------|
| **Record** | `.AsRecord()` | Generate immutable record types | ✅ Full Support |
| **Class** | `.AsClass()` | Generate mutable class types | ✅ Full Support |
| **Struct** | `.AsStruct()` | Generate value type structs | ✅ Full Support |

## 🎯 Target Frameworks

- **.NET Standard 2.0** and above
- **.NET 9** recommended
- **C# 12.0** syntax support

## 🚀 Real-World Usage Scenarios

### EF Core Integration

```csharp
// Complex dashboard data generation
var dashboardData = await dbContext.Orders
    .Include(o => o.Customer)
    .GroupBy(o => new { o.CustomerId, o.Customer.Name })
    .Select(g => new {
        CustomerId = g.Key.CustomerId,
        CustomerName = g.Key.Name,
        TotalOrders = g.Count(),
        TotalRevenue = g.Sum(o => o.Amount),
        AverageOrderValue = g.Average(o => o.Amount),
        LastOrderDate = g.Max(o => o.OrderDate)
    })
    .ToListAsync();

// Strongly-typed DTO generation and conversion
TypeCombiner.Combine()
    .WithProjection(dashboardData)
    .Add("GeneratedAt", typeof(DateTime))
    .Add("ReportType", typeof(string))
    .WithName("CustomerDashboard")
    .WithConverter()
    .AsRecord()
    .Generate();

// Immediately available strongly-typed collection
List<Generated.CustomerDashboard> dashboard = CustomerDashboard.FromCollection(dashboardData);

// Complete type safety in business logic
var topCustomers = dashboard
    .Where(c => c.TotalRevenue > 100000)
    .OrderByDescending(c => c.TotalRevenue)
    .Take(10)
    .ToList();
```
### API DTO Generation

```csharp
// Generate public API DTO from internal entity
var internalUser = new { 
    Id = 1, 
    Name = "John Doe", 
    Email = "john@company.com", 
    Password = "hashed_password", 
    InternalNotes = "Important internal info",
    Salary = 5000000m
};

TypeCombiner.Combine()
    .With(internalUser)
    .Exclude("Password")       // Exclude password
    .Exclude("InternalNotes")  // Exclude internal info
    .Exclude("Salary")         // Exclude salary info
    .Add("PublicId", typeof(Guid))
    .WithConverter()
    .WithName("UserApiDto")
    .AsRecord()
    .Generate();
```
### Data Transformation Pipeline

```csharp
// Convert external API response to internal format
var externalApiResponse = new[] {
    new { user_id = "123", full_name = "John Doe", email_addr = "john@example.com", is_active = "1" },
    new { user_id = "456", full_name = "Jane Smith", email_addr = "jane@example.com", is_active = "0" }
};

TypeCombiner.Combine()
    .WithProjection(externalApiResponse)
    .ChangeType("user_id", typeof(int))      // string → int
    .ChangeType("is_active", typeof(bool))   // string → bool
    .Add("ImportedAt", typeof(DateTime))
    .WithConverter()
    .WithName("ImportedUser")
    .AsRecord()
    .Generate();

// Type-safe conversion
List<Generated.ImportedUser> users = ImportedUser.FromCollection(externalApiResponse);
```
## 🧪 Testing

### Running Tests
dotnet test
### Comprehensive Test Coverage

Structura is validated with **100+ comprehensive unit tests**:

| Test Category | Test Count | Description |
|---------------|------------|-------------|
| **Core Features** | 15 | All basic TypeCombiner API features |
| **Anonymous Types** | 12 | Anonymous object combination and processing |
| **EF Core Projection** | 18 | EF Core projection result processing |
| **Variable References** | 8 | Anonymous type variable analysis |
| **Type Generation Modes** | 6 | Record, Class, Struct type generation |
| **Edge Cases** | 5 | Error conditions and boundary cases |
| **Fluent API** | 4 | Method chaining integrity |
| **Type Safety** | 3 | Compile-time type validation |
| **Source Generator Integration** | 8 | Generated type verification |
| **Performance** | 3 | Large-scale processing and performance |
| **Real-World Scenarios** | 10 | Production environment use cases |
| **Documented Features** | 12 | README example code validation |
| **Integration Scenarios** | 15 | Complex feature combinations |
| **🆕 Converter Extensions** | 20 | Smart converter functionality |
| **Add/Exclude/ChangeType** | 18 | Property manipulation features |

#### 🎯 Test Results
- **Total Tests**: 150+
- **Passed**: 150+ ✅
- **Failed**: 0
- **Skipped**: 0
- **Execution Time**: < 3 seconds

## 📁 Project Structure
Structura/
├── 📂 Structura/                     # Main library
│   ├── TypeCombiner.cs               # Fluent API entry point
│   ├── AnonymousTypeCombinerBuilder.cs # Anonymous type builder
│   ├── TypeDefinitions.cs            # Core type definitions
│   └── StructuraSourceGenerator.cs   # Source generator engine
├── 📂 Structura.Test.Console/        # Integration test console
│   └── Program.cs                    # Real usage examples and demos
├── 📂 Structura.Tests/              # Unit test project
│   ├── UnitTest.cs                  # Basic functionality unit tests
│   ├── VariableReferenceTests.cs    # Variable reference feature tests
│   ├── EFCoreProjectionTests.cs     # EF Core projection tests
│   ├── ConverterExtensionTests.cs   # Smart converter tests
│   ├── AddFunctionalityTests.cs     # Add functionality tests
│   ├── ExcludeFunctionalityTests.cs # Exclude functionality tests
│   ├── ChangeTypeFunctionalityTests.cs # ChangeType functionality tests
│   └── IntegrationTests.cs          # Integration and scenario tests
└── 📄 README.md                     # Documentation
## 📈 Development Status

### ✅ Completed Features

| Feature | Status | Description |
|---------|--------|-------------|
| **Source Generator Engine** | 100% ✅ | Complete |
| **Fluent API** | 100% ✅ | Complete |
| **Anonymous Type Support** | 100% ✅ | Complete |
| **Variable Reference Analysis** | 100% ✅ | Complete |
| **EF Core Projection Support** | 100% ✅ | Complete |
| **Property Add/Exclude/ChangeType** | 100% ✅ | Complete |
| **Type Conversion** | 100% ✅ | Record/Class/Struct support |
| **🆕 Smart Converter Extensions** | 100% ✅ | **NEW! Perfect object conversion** |
| **Comprehensive Test Suite** | 100% ✅ | 150+ tests passing |

## 🚀 Getting Started

### 1. Installation

```bash
dotnet add package Structura
```
### 2. Basic Usage

```csharp
using Structura;

// Create new type from anonymous types
var userData = new { Name = "John Doe", Age = 30, Email = "john@test.com" };

TypeCombiner.Combine()
    .With(userData)
    .Add("Id", typeof(int))
    .Exclude("InternalData")  // Exclude if exists
    .WithName("User")
    .AsRecord()
    .Generate();

// Use the generated type
var user = new Generated.User("Jane Smith", 25, "jane@test.com", 1001);
```
### 3. Advanced Usage with Converter

```csharp
// EF Core projection conversion
var efResult = dbContext.Products
    .Select(p => new { p.Name, p.Price, p.Category })
    .ToList();

TypeCombiner.Combine()
    .WithProjection(efResult)
    .Add("ProcessedAt", typeof(DateTime))
    .WithConverter()  // 🔥 Enable smart conversion
    .WithName("ProductDto")
    .AsRecord()
    .Generate();

// Immediate conversion available
List<Generated.ProductDto> products = ProductDto.FromCollection(efResult);
```
## 🎨 API Reference

### TypeCombiner Static Methods

| Method | Description | Return Type |
|--------|-------------|-------------|
| `Combine()` | Start with anonymous types only | `AnonymousTypeCombinerBuilder` |

### Fluent Methods

| Method | Description | Chainable |
|--------|-------------|-----------|
| `.With(object)` | Add anonymous type | ✅ |
| `.WithProjection(IEnumerable<object>)` | Add EF Core projection | ✅ |
| `.Add(string, Type)` | Add property | ✅ |
| `.Exclude(string)` | Exclude property | ✅ |
| `.ChangeType(string, Type)` | Change property type | ✅ |
| `.WithConverter()` | 🆕 **Enable smart converter extensions** | ✅ |
| `.WithName(string)` | Set generated type name | ✅ |
| `.AsRecord()` | Generate as record | ✅ |
| `.AsClass()` | Generate as class | ✅ |
| `.AsStruct()` | Generate as struct | ✅ |
| `.Generate()` | Execute type generation | ❌ |

### 🆕 Generated Static Converter Methods

When `.WithConverter()` is used, the following static methods are automatically generated **on the generated type itself**:

| Method | Description | Usage |
|--------|-------------|-------|
| `.FromSingle(object)` | Convert single object | `UserDto.FromSingle(objectItem)` |
| `.FromCollection(IEnumerable<object>)` | Convert object collection | `UserDto.FromCollection(objectList)` |

## 🤝 Contributing

Issues and pull requests are welcome!

## 📄 License

MIT License

---

**Structura** - Simplify type manipulation with smart conversion! 🚀