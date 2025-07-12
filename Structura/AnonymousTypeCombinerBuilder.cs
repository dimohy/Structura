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
        private readonly List<object> _anonymousTypes = new List<object>();
        private readonly List<IEnumerable> _projectionResults = new List<IEnumerable>();
        private bool _enableConverter = false;
        
        // _mode field usage - used by source generator in Generate() method
        #pragma warning disable CS0414 
        private TypeGenerationMode _mode = TypeGenerationMode.Record;
        #pragma warning restore CS0414

        /// <summary>
        /// Sets the name of the generated type.
        /// </summary>
        public AnonymousTypeCombinerBuilder WithName(string typeName)
        {
            _typeName = typeName;
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

    /// <summary>
    /// Builder for creating new types based on existing types
    /// </summary>
    public class SingleTypeCombinerBuilder<T>
    {
        private string _typeName = string.Empty;
        private readonly List<string> _excludedProperties = new List<string>();
        private readonly List<(string Name, Type Type, object? DefaultValue)> _additionalProperties = new List<(string, Type, object?)>();
        private readonly List<(string PropertyName, Type NewType)> _typeChanges = new List<(string, Type)>();
        private readonly List<object> _anonymousTypes = new List<object>();
        private readonly List<IEnumerable> _projectionResults = new List<IEnumerable>();
        private readonly List<(string PropertyName, bool Condition)> _conditionalExclusions = new List<(string, bool)>();
        private bool _enableConverter = false;
        
        // _mode field usage - used by source generator in Generate() method
        #pragma warning disable CS0414
        private TypeGenerationMode _mode = TypeGenerationMode.Record;
        #pragma warning restore CS0414

        /// <summary>
        /// Sets the name of the generated type.
        /// </summary>
        public SingleTypeCombinerBuilder<T> WithName(string typeName)
        {
            _typeName = typeName;
            return this;
        }

        /// <summary>
        /// Excludes a specific property.
        /// </summary>
        public SingleTypeCombinerBuilder<T> Exclude(Expression<Func<T, object?>> propertyExpression)
        {
            var propertyName = GetPropertyName(propertyExpression);
            _excludedProperties.Add(propertyName);
            return this;
        }

        /// <summary>
        /// Conditionally excludes a property.
        /// </summary>
        public SingleTypeCombinerBuilder<T> ExcludeIf(Expression<Func<T, object?>> propertyExpression, bool condition)
        {
            var propertyName = GetPropertyName(propertyExpression);
            _conditionalExclusions.Add((propertyName, condition));
            return this;
        }

        /// <summary>
        /// Adds a new property.
        /// </summary>
        public SingleTypeCombinerBuilder<T> Add(string propertyName, Type propertyType, object? defaultValue = null)
        {
            _additionalProperties.Add((propertyName, propertyType, defaultValue));
            return this;
        }

        /// <summary>
        /// Changes the type of an existing property.
        /// </summary>
        public SingleTypeCombinerBuilder<T> ChangeType(Expression<Func<T, object?>> propertyExpression, Type newType)
        {
            var propertyName = GetPropertyName(propertyExpression);
            _typeChanges.Add((propertyName, newType));
            return this;
        }

        /// <summary>
        /// Adds an anonymous type.
        /// </summary>
        public SingleTypeCombinerBuilder<T> With(object anonymousType)
        {
            _anonymousTypes.Add(anonymousType);
            return this;
        }

        /// <summary>
        /// Creates type using EF Core projection results.
        /// </summary>
        /// <param name="projectionResult">Result of EF Core Select().ToList()</param>
        public SingleTypeCombinerBuilder<T> WithProjection(IEnumerable projectionResult)
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
        /// TypeCombiner.From&lt;User&gt;()
        ///     .WithProjection(efCoreResult)
        ///     .WithName("EnhancedUser")
        ///     .WithConverter()  // ?? This enables the magic!
        ///     .Generate();
        ///     
        /// // Now you can use:
        /// List&lt;Generated.EnhancedUser&gt; typed = efCoreResult.ToEnhancedUser();
        /// </code>
        /// </example>
        public SingleTypeCombinerBuilder<T> WithConverter()
        {
            _enableConverter = true;
            return this;
        }

        /// <summary>
        /// Generates as record type.
        /// </summary>
        public SingleTypeCombinerBuilder<T> AsRecord()
        {
            _mode = TypeGenerationMode.Record;
            return this;
        }

        /// <summary>
        /// Generates as class type.
        /// </summary>
        public SingleTypeCombinerBuilder<T> AsClass()
        {
            _mode = TypeGenerationMode.Class;
            return this;
        }

        /// <summary>
        /// Generates as struct type.
        /// </summary>
        public SingleTypeCombinerBuilder<T> AsStruct()
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

        private static string GetPropertyName(Expression<Func<T, object?>> expression)
        {
            switch (expression.Body)
            {
                case MemberExpression member:
                    return member.Member.Name;
                case UnaryExpression unary when unary.Operand is MemberExpression member:
                    return member.Member.Name;
                default:
                    throw new ArgumentException("Invalid property expression", nameof(expression));
            }
        }
    }
}