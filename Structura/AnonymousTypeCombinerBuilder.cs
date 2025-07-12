using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Structura
{
    /// <summary>
    /// Builder for combining anonymous types only
    /// </summary>
    public class AnonymousTypeCombinerBuilder
    {
        private string _typeName = string.Empty;
        private string _namespace = "Generated"; // 기본 네임스페이스
        private readonly List<object> _anonymousTypes = new List<object>();
        private readonly List<IEnumerable> _projectionResults = new List<IEnumerable>();
        private readonly List<(string Name, Type Type, object? DefaultValue)> _additionalProperties = new List<(string, Type, object?)>();
        private readonly List<string> _excludedProperties = new List<string>();
        private readonly List<(string PropertyName, Type NewType)> _typeChanges = new List<(string, Type)>();
        private bool _enableConverter = false;
        
        // _mode field usage - used by source generator in Generate() method
        #pragma warning disable CS0414 
        private TypeGenerationMode _mode = TypeGenerationMode.Record;
        #pragma warning restore CS0414

        /// <summary>
        /// Sets the name of the generated type with optional namespace specification.
        /// </summary>
        /// <param name="typeName">The name of the generated type</param>
        /// <param name="namespaceName">Optional: The namespace for the generated type (default: "Generated")</param>
        /// <example>
        /// <code>
        /// // 기본 Generated 네임스페이스 사용
        /// .WithName("UserDto")
        /// 
        /// // 커스텀 네임스페이스 지정
        /// .WithName("UserDto", "MyProject.Models")
        /// </code>
        /// </example>
        public AnonymousTypeCombinerBuilder WithName(string typeName, string? namespaceName = null)
        {
            _typeName = typeName;
            if (namespaceName != null)
            {
                _namespace = namespaceName;
            }
            return this;
        }

        /// <summary>
        /// Adds an anonymous type.
        /// </summary>
        public AnonymousTypeCombinerBuilder With(object anonymousType)
        {
            _anonymousTypes.Add(anonymousType);
            return this;
        }

        /// <summary>
        /// ?? **Adds a new property** - Core Add functionality for anonymous builders
        /// </summary>
        public AnonymousTypeCombinerBuilder Add(string propertyName, Type propertyType, object? defaultValue = null)
        {
            _additionalProperties.Add((propertyName, propertyType, defaultValue));
            return this;
        }

        /// <summary>
        /// ?? **Excludes a property** - Removes specified property from the generated type
        /// </summary>
        /// <param name="propertyName">Name of the property to exclude</param>
        /// <example>
        /// <code>
        /// TypeCombiner.Combine()
        ///     .With(new { Name = "John", Password = "secret", Age = 30 })
        ///     .Exclude("Password")  // Password 속성 제외
        ///     .WithName("SafeUser")
        ///     .Generate();
        /// </code>
        /// </example>
        public AnonymousTypeCombinerBuilder Exclude(string propertyName)
        {
            _excludedProperties.Add(propertyName);
            return this;
        }

        /// <summary>
        /// ?? **Changes property type** - Changes the type of an existing property
        /// </summary>
        /// <param name="propertyName">Name of the property to change</param>
        /// <param name="newType">New type for the property</param>
        /// <example>
        /// <code>
        /// TypeCombiner.Combine()
        ///     .With(new { Price = 100m, Quantity = 5 })
        ///     .ChangeType("Price", typeof(string))     // decimal → string
        ///     .ChangeType("Quantity", typeof(long))    // int → long
        ///     .WithName("ModifiedProduct")
        ///     .Generate();
        /// </code>
        /// </example>
        public AnonymousTypeCombinerBuilder ChangeType(string propertyName, Type newType)
        {
            _typeChanges.Add((propertyName, newType));
            return this;
        }

        /// <summary>
        /// Creates type using EF Core projection results.
        /// </summary>
        /// <param name="projectionResult">Result of EF Core Select().ToList()</param>
        public AnonymousTypeCombinerBuilder WithProjection(IEnumerable projectionResult)
        {
            _projectionResults.Add(projectionResult);
            return this;
        }

        /// <summary>
        /// ?? **Enables Smart Converter** - Generates extension methods to convert anonymous objects to strong types
        /// When enabled, generates methods like: anonymousCollection.ToTypeName() and anonymousObject.ToTypeName()
        /// </summary>
        /// <example>
        /// <code>
        /// TypeCombiner.Combine()
        ///     .WithProjection(efCoreResult)
        ///     .WithName("UserDto")
        ///     .WithConverter()  // ?? This enables the magic!
        ///     .Generate();
        ///     
        /// // Now you can use:
        /// List&lt;Generated.UserDto&gt; typed = efCoreResult.ToUserDto();
        /// </code>
        /// </example>
        public AnonymousTypeCombinerBuilder WithConverter()
        {
            _enableConverter = true;
            return this;
        }

        /// <summary>
        /// Generates as record type.
        /// </summary>
        public AnonymousTypeCombinerBuilder AsRecord()
        {
            _mode = TypeGenerationMode.Record;
            return this;
        }

        /// <summary>
        /// Generates as class type.
        /// </summary>
        public AnonymousTypeCombinerBuilder AsClass()
        {
            _mode = TypeGenerationMode.Class;
            return this;
        }

        /// <summary>
        /// Generates as struct type.
        /// </summary>
        public AnonymousTypeCombinerBuilder AsStruct()
        {
            _mode = TypeGenerationMode.Struct;
            return this;
        }

        /// <summary>
        /// Generates the type.
        /// </summary>
        public void Generate()
        {
            // Source generator will analyze this method call and generate the type based on the configuration.
            // Method body content is processed by the source generator so it remains empty.
        }
    }
}