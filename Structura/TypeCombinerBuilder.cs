using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Structura
{
    /// <summary>
    /// �� �� Ÿ���� �����ϴ� ����
    /// </summary>
    public class TypeCombinerBuilder<T1, T2>
    {
        private string _typeName = string.Empty;
        private readonly List<string> _excludedProperties = new List<string>();
        private readonly List<(string Name, Type Type, object? DefaultValue)> _additionalProperties = new List<(string, Type, object?)>();
        private readonly List<(string PropertyName, Type NewType)> _typeChanges = new List<(string, Type)>();
        private readonly List<object> _anonymousTypes = new List<object>();
        private readonly List<IEnumerable> _projectionResults = new List<IEnumerable>();
        
        // _mode �ʵ� ���� - Generate() �޼��忡�� �ҽ� �����Ⱑ ����
        #pragma warning disable CS0414
        private TypeGenerationMode _mode = TypeGenerationMode.Record;
        #pragma warning restore CS0414

        /// <summary>
        /// ������ Ÿ���� �̸��� �����մϴ�.
        /// </summary>
        public TypeCombinerBuilder<T1, T2> WithName(string typeName)
        {
            _typeName = typeName;
            return this;
        }

        /// <summary>
        /// ���� Ÿ���� �߰��մϴ�.
        /// </summary>
        public TypeCombinerBuilder<T1, T2> With(object anonymousType)
        {
            _anonymousTypes.Add(anonymousType);
            return this;
        }

        /// <summary>
        /// EF Core projection ������� ��Ű���� �����Ͽ� Ÿ���� �����մϴ�.
        /// </summary>
        /// <param name="projectionResult">EF Core Select().ToList() ���</param>
        public TypeCombinerBuilder<T1, T2> WithProjection(IEnumerable projectionResult)
        {
            _projectionResults.Add(projectionResult);
            return this;
        }

        /// <summary>
        /// Ư�� �Ӽ��� �����մϴ�.
        /// </summary>
        public TypeCombinerBuilder<T1, T2> Exclude<T>(Expression<Func<T, object?>> propertyExpression)
        {
            var propertyName = GetPropertyName(propertyExpression);
            _excludedProperties.Add(propertyName);
            return this;
        }

        /// <summary>
        /// ���ο� �Ӽ��� �߰��մϴ�.
        /// </summary>
        public TypeCombinerBuilder<T1, T2> Add(string propertyName, Type propertyType, object? defaultValue = null)
        {
            _additionalProperties.Add((propertyName, propertyType, defaultValue));
            return this;
        }

        /// <summary>
        /// ���� �Ӽ��� Ÿ���� �����մϴ�.
        /// </summary>
        public TypeCombinerBuilder<T1, T2> ChangeType<T>(Expression<Func<T, object?>> propertyExpression, Type newType)
        {
            var propertyName = GetPropertyName(propertyExpression);
            _typeChanges.Add((propertyName, newType));
            return this;
        }

        /// <summary>
        /// ���ڵ� Ÿ������ �����մϴ�.
        /// </summary>
        public TypeCombinerBuilder<T1, T2> AsRecord()
        {
            _mode = TypeGenerationMode.Record;
            return this;
        }

        /// <summary>
        /// Ŭ���� Ÿ������ �����մϴ�.
        /// </summary>
        public TypeCombinerBuilder<T1, T2> AsClass()
        {
            _mode = TypeGenerationMode.Class;
            return this;
        }

        /// <summary>
        /// ����ü Ÿ������ �����մϴ�.
        /// </summary>
        public TypeCombinerBuilder<T1, T2> AsStruct()
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
    /// ���� Ÿ���� �����ϴ� ����
    /// </summary>
    public class TypeCombinerBuilder<T> : TypeCombinerBuilder<T, object>
    {
        /// <summary>
        /// ������ Ÿ���� �̸��� �����մϴ�.
        /// </summary>
        public new TypeCombinerBuilder<T> WithName(string typeName)
        {
            base.WithName(typeName);
            return this;
        }

        /// <summary>
        /// ���� Ÿ���� �߰��մϴ�.
        /// </summary>
        public new TypeCombinerBuilder<T> With(object anonymousType)
        {
            base.With(anonymousType);
            return this;
        }

        /// <summary>
        /// EF Core projection ������� ��Ű���� �����Ͽ� Ÿ���� �����մϴ�.
        /// </summary>
        /// <param name="projectionResult">EF Core Select().ToList() ���</param>
        public new TypeCombinerBuilder<T> WithProjection(IEnumerable projectionResult)
        {
            base.WithProjection(projectionResult);
            return this;
        }

        /// <summary>
        /// ���ڵ� Ÿ������ �����մϴ�.
        /// </summary>
        public new TypeCombinerBuilder<T> AsRecord()
        {
            base.AsRecord();
            return this;
        }

        /// <summary>
        /// Ŭ���� Ÿ������ �����մϴ�.
        /// </summary>
        public new TypeCombinerBuilder<T> AsClass()
        {
            base.AsClass();
            return this;
        }

        /// <summary>
        /// ����ü Ÿ������ �����մϴ�.
        /// </summary>
        public new TypeCombinerBuilder<T> AsStruct()
        {
            base.AsStruct();
            return this;
        }
    }
}