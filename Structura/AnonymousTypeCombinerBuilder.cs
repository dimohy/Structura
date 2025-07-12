using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Structura
{
    /// <summary>
    /// ���� Ÿ�Ե鸸�� �����ϴ� ����
    /// </summary>
    public class AnonymousTypeCombinerBuilder
    {
        private string _typeName = string.Empty;
        private readonly List<object> _anonymousTypes = new List<object>();
        private readonly List<IEnumerable> _projectionResults = new List<IEnumerable>();
        
        // _mode �ʵ� ���� - Generate() �޼��忡�� �ҽ� �����Ⱑ ����
        #pragma warning disable CS0414 
        private TypeGenerationMode _mode = TypeGenerationMode.Record;
        #pragma warning restore CS0414

        /// <summary>
        /// ������ Ÿ���� �̸��� �����մϴ�.
        /// </summary>
        public AnonymousTypeCombinerBuilder WithName(string typeName)
        {
            _typeName = typeName;
            return this;
        }

        /// <summary>
        /// ���� Ÿ���� �߰��մϴ�.
        /// </summary>
        public AnonymousTypeCombinerBuilder With(object anonymousType)
        {
            _anonymousTypes.Add(anonymousType);
            return this;
        }

        /// <summary>
        /// EF Core projection ������� ��Ű���� �����Ͽ� Ÿ���� �����մϴ�.
        /// </summary>
        /// <param name="projectionResult">EF Core Select().ToList() ���</param>
        public AnonymousTypeCombinerBuilder WithProjection(IEnumerable projectionResult)
        {
            _projectionResults.Add(projectionResult);
            return this;
        }

        /// <summary>
        /// ���ڵ� Ÿ������ �����մϴ�.
        /// </summary>
        public AnonymousTypeCombinerBuilder AsRecord()
        {
            _mode = TypeGenerationMode.Record;
            return this;
        }

        /// <summary>
        /// Ŭ���� Ÿ������ �����մϴ�.
        /// </summary>
        public AnonymousTypeCombinerBuilder AsClass()
        {
            _mode = TypeGenerationMode.Class;
            return this;
        }

        /// <summary>
        /// ����ü Ÿ������ �����մϴ�.
        /// </summary>
        public AnonymousTypeCombinerBuilder AsStruct()
        {
            _mode = TypeGenerationMode.Struct;
            return this;
        }

        /// <summary>
        /// Ÿ�� ������ �����մϴ�.
        /// </summary>
        public void Generate()
        {
            // �ҽ� �����Ⱑ �� �޼��� ȣ���� �����Ͽ� ���� Ÿ���� �����մϴ�.
            // �޼��� ���� ������ �ҽ� �����⿡�� ó���ǹǷ� ����Ӵϴ�.
        }
    }

    /// <summary>
    /// ���� Ÿ���� ������� ���ο� Ÿ���� �����ϴ� ����
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
        
        // _mode �ʵ� ���� - Generate() �޼��忡�� �ҽ� �����Ⱑ ����
        #pragma warning disable CS0414
        private TypeGenerationMode _mode = TypeGenerationMode.Record;
        #pragma warning restore CS0414

        /// <summary>
        /// ������ Ÿ���� �̸��� �����մϴ�.
        /// </summary>
        public SingleTypeCombinerBuilder<T> WithName(string typeName)
        {
            _typeName = typeName;
            return this;
        }

        /// <summary>
        /// Ư�� �Ӽ��� �����մϴ�.
        /// </summary>
        public SingleTypeCombinerBuilder<T> Exclude(Expression<Func<T, object?>> propertyExpression)
        {
            var propertyName = GetPropertyName(propertyExpression);
            _excludedProperties.Add(propertyName);
            return this;
        }

        /// <summary>
        /// ���Ǻη� �Ӽ��� �����մϴ�.
        /// </summary>
        public SingleTypeCombinerBuilder<T> ExcludeIf(Expression<Func<T, object?>> propertyExpression, bool condition)
        {
            var propertyName = GetPropertyName(propertyExpression);
            _conditionalExclusions.Add((propertyName, condition));
            return this;
        }

        /// <summary>
        /// ���ο� �Ӽ��� �߰��մϴ�.
        /// </summary>
        public SingleTypeCombinerBuilder<T> Add(string propertyName, Type propertyType, object? defaultValue = null)
        {
            _additionalProperties.Add((propertyName, propertyType, defaultValue));
            return this;
        }

        /// <summary>
        /// ���� �Ӽ��� Ÿ���� �����մϴ�.
        /// </summary>
        public SingleTypeCombinerBuilder<T> ChangeType(Expression<Func<T, object?>> propertyExpression, Type newType)
        {
            var propertyName = GetPropertyName(propertyExpression);
            _typeChanges.Add((propertyName, newType));
            return this;
        }

        /// <summary>
        /// ���� Ÿ���� �߰��մϴ�.
        /// </summary>
        public SingleTypeCombinerBuilder<T> With(object anonymousType)
        {
            _anonymousTypes.Add(anonymousType);
            return this;
        }

        /// <summary>
        /// EF Core projection ������� ��Ű���� �����Ͽ� Ÿ���� �����մϴ�.
        /// </summary>
        /// <param name="projectionResult">EF Core Select().ToList() ���</param>
        public SingleTypeCombinerBuilder<T> WithProjection(IEnumerable projectionResult)
        {
            _projectionResults.Add(projectionResult);
            return this;
        }

        /// <summary>
        /// ���ڵ� Ÿ������ �����մϴ�.
        /// </summary>
        public SingleTypeCombinerBuilder<T> AsRecord()
        {
            _mode = TypeGenerationMode.Record;
            return this;
        }

        /// <summary>
        /// Ŭ���� Ÿ������ �����մϴ�.
        /// </summary>
        public SingleTypeCombinerBuilder<T> AsClass()
        {
            _mode = TypeGenerationMode.Class;
            return this;
        }

        /// <summary>
        /// ����ü Ÿ������ �����մϴ�.
        /// </summary>
        public SingleTypeCombinerBuilder<T> AsStruct()
        {
            _mode = TypeGenerationMode.Struct;
            return this;
        }

        /// <summary>
        /// Ÿ�� ������ �����մϴ�.
        /// </summary>
        public void Generate()
        {
            // �ҽ� �����Ⱑ �� �޼��� ȣ���� �����Ͽ� ���� Ÿ���� �����մϴ�.
            // �޼��� ���� ������ �ҽ� �����⿡�� ó���ǹǷ� ����Ӵϴ�.
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