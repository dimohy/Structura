using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Structura
{
    /// <summary>
    /// Builder for combining two types
    /// </summary>
    public class TypeCombinerBuilder<T1, T2>
    {
        private string _typeName = string.Empty;
        private readonly List<string> _excludedProperties = new List<string>();
        private readonly List<(string Name, Type Type, object? DefaultValue)> _additionalProperties = new List<(string, Type, object?)>();
        private readonly List<(string PropertyName, Type NewType)> _typeChanges = new List<(string, Type)>();
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
        public TypeCombinerBuilder<T1, T2> WithName(string typeName)
        {
            _typeName = typeName;
            return this;
        }

        /// <summary>
        /// Adds an anonymous type.
        /// </summary>
        public TypeCombinerBuilder<T1, T2> With(object anonymousType)
        {
            _anonymousTypes.Add(anonymousType);
            return this;
        }

        /// <summary>
        /// Creates type using EF Core projection results.
        /// </summary>
        /// <param name="projectionResult">Result of EF Core Select().ToList()</param>
        public TypeCombinerBuilder<T1, T2> WithProjection(IEnumerable projectionResult)
        {
            _projectionResults.Add(projectionResult);
            return this;
        }

        /// <summary>
        /// Excludes a specific property.
        /// </summary>
        public TypeCombinerBuilder<T1, T2> Exclude<T>(Expression<Func<T, object?>> propertyExpression)
        {
            var propertyName = GetPropertyName(propertyExpression);
            _excludedProperties.Add(propertyName);
            return this;
        }

        /// <summary>
        /// Adds a new property.
        /// </summary>
        public TypeCombinerBuilder<T1, T2> Add(string propertyName, Type propertyType, object? defaultValue = null)
        {
            _additionalProperties.Add((propertyName, propertyType, defaultValue));
            return this;
        }

        /// <summary>
        /// Changes the type of an existing property.
        /// </summary>
        public TypeCombinerBuilder<T1, T2> ChangeType<T>(Expression<Func<T, object?>> propertyExpression, Type newType)
        {
            var propertyName = GetPropertyName(propertyExpression);
            _typeChanges.Add((propertyName, newType));
            return this;
        }

        /// <summary>
        /// 🎯 **Enables Smart Converter** - Generates extension methods to convert anonymous objects to strong types
        /// When enabled, generates methods like: anonymousCollection.ToTypeName() and anonymousObject.ToTypeName()
        /// </summary>
        /// <example>
        /// <code>
        /// TypeCombiner.Combine&lt;PersonalInfo, ContactInfo&gt;()
        ///     .WithName("UserProfile")
        ///     .WithConverter()  // 🔥 This enables the magic!
        ///     .Generate();
        ///     
        /// // Now you can use:
        /// var anonymousData = new { FirstName = "John", Email = "john@example.com" };
        /// Generated.UserProfile typed = anonymousData.ToUserProfile();
        /// </code>
        /// </example>
        public TypeCombinerBuilder<T1, T2> WithConverter()
        {
            _enableConverter = true;
            return this;
        }

        /// <summary>
        /// Generates as record type.
        /// </summary>
        public TypeCombinerBuilder<T1, T2> AsRecord()
        {
            _mode = TypeGenerationMode.Record;
            return this;
        }

        /// <summary>
        /// Generates as class type.
        /// </summary>
        public TypeCombinerBuilder<T1, T2> AsClass()
        {
            _mode = TypeGenerationMode.Class;
            return this;
        }

        /// <summary>
        /// Generates as struct type.
        /// </summary>
        public TypeCombinerBuilder<T1, T2> AsStruct()
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

        private static string GetPropertyName<T>(Expression<Func<T, object?>> expression)
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

    /// <summary>
    /// Builder for combining a single type
    /// </summary>
    public class TypeCombinerBuilder<T> : TypeCombinerBuilder<T, object>
    {
        /// <summary>
        /// Sets the name of the generated type.
        /// </summary>
        public new TypeCombinerBuilder<T> WithName(string typeName)
        {
            base.WithName(typeName);
            return this;
        }

        /// <summary>
        /// Adds an anonymous type.
        /// </summary>
        public new TypeCombinerBuilder<T> With(object anonymousType)
        {
            base.With(anonymousType);
            return this;
        }

        /// <summary>
        /// Creates type using EF Core projection results.
        /// </summary>
        /// <param name="projectionResult">Result of EF Core Select().ToList()</param>
        public new TypeCombinerBuilder<T> WithProjection(IEnumerable projectionResult)
        {
            base.WithProjection(projectionResult);
            return this;
        }

        /// <summary>
        /// 🎯 **Enables Smart Converter** - Generates extension methods to convert anonymous objects to strong types
        /// </summary>
        public new TypeCombinerBuilder<T> WithConverter()
        {
            base.WithConverter();
            return this;
        }

        /// <summary>
        /// Generates as record type.
        /// </summary>
        public new TypeCombinerBuilder<T> AsRecord()
        {
            base.AsRecord();
            return this;
        }

        /// <summary>
        /// Generates as class type.
        /// </summary>
        public new TypeCombinerBuilder<T> AsClass()
        {
            base.AsClass();
            return this;
        }

        /// <summary>
        /// Generates as struct type.
        /// </summary>
        public new TypeCombinerBuilder<T> AsStruct()
        {
            base.AsStruct();
            return this;
        }
    }
}