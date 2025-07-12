using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Structura
{
    /// <summary>
    /// 두 개 타입을 결합하는 빌더
    /// </summary>
    public class TypeCombinerBuilder<T1, T2>
    {
        private string _typeName = string.Empty;
        private readonly List<string> _excludedProperties = new List<string>();
        private readonly List<(string Name, Type Type, object? DefaultValue)> _additionalProperties = new List<(string, Type, object?)>();
        private readonly List<(string PropertyName, Type NewType)> _typeChanges = new List<(string, Type)>();
        private readonly List<object> _anonymousTypes = new List<object>();
        private readonly List<IEnumerable> _projectionResults = new List<IEnumerable>();
        
        // _mode 필드 사용됨 - Generate() 메서드에서 소스 생성기가 참조
        #pragma warning disable CS0414
        private TypeGenerationMode _mode = TypeGenerationMode.Record;
        #pragma warning restore CS0414

        /// <summary>
        /// 생성될 타입의 이름을 지정합니다.
        /// </summary>
        public TypeCombinerBuilder<T1, T2> WithName(string typeName)
        {
            _typeName = typeName;
            return this;
        }

        /// <summary>
        /// 무명 타입을 추가합니다.
        /// </summary>
        public TypeCombinerBuilder<T1, T2> With(object anonymousType)
        {
            _anonymousTypes.Add(anonymousType);
            return this;
        }

        /// <summary>
        /// EF Core projection 결과에서 스키마를 추출하여 타입을 생성합니다.
        /// </summary>
        /// <param name="projectionResult">EF Core Select().ToList() 결과</param>
        public TypeCombinerBuilder<T1, T2> WithProjection(IEnumerable projectionResult)
        {
            _projectionResults.Add(projectionResult);
            return this;
        }

        /// <summary>
        /// 특정 속성을 제외합니다.
        /// </summary>
        public TypeCombinerBuilder<T1, T2> Exclude<T>(Expression<Func<T, object?>> propertyExpression)
        {
            var propertyName = GetPropertyName(propertyExpression);
            _excludedProperties.Add(propertyName);
            return this;
        }

        /// <summary>
        /// 새로운 속성을 추가합니다.
        /// </summary>
        public TypeCombinerBuilder<T1, T2> Add(string propertyName, Type propertyType, object? defaultValue = null)
        {
            _additionalProperties.Add((propertyName, propertyType, defaultValue));
            return this;
        }

        /// <summary>
        /// 기존 속성의 타입을 변경합니다.
        /// </summary>
        public TypeCombinerBuilder<T1, T2> ChangeType<T>(Expression<Func<T, object?>> propertyExpression, Type newType)
        {
            var propertyName = GetPropertyName(propertyExpression);
            _typeChanges.Add((propertyName, newType));
            return this;
        }

        /// <summary>
        /// 레코드 타입으로 생성합니다.
        /// </summary>
        public TypeCombinerBuilder<T1, T2> AsRecord()
        {
            _mode = TypeGenerationMode.Record;
            return this;
        }

        /// <summary>
        /// 클래스 타입으로 생성합니다.
        /// </summary>
        public TypeCombinerBuilder<T1, T2> AsClass()
        {
            _mode = TypeGenerationMode.Class;
            return this;
        }

        /// <summary>
        /// 구조체 타입으로 생성합니다.
        /// </summary>
        public TypeCombinerBuilder<T1, T2> AsStruct()
        {
            _mode = TypeGenerationMode.Struct;
            return this;
        }

        /// <summary>
        /// 타입 생성을 실행합니다.
        /// </summary>
        public void Generate()
        {
            // 소스 생성기가 이 메서드 호출을 감지하여 실제 타입을 생성합니다.
            // 메서드 내부 구현은 소스 생성기에서 처리되므로 비워둡니다.
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
    /// 단일 타입을 결합하는 빌더
    /// </summary>
    public class TypeCombinerBuilder<T> : TypeCombinerBuilder<T, object>
    {
        /// <summary>
        /// 생성될 타입의 이름을 지정합니다.
        /// </summary>
        public new TypeCombinerBuilder<T> WithName(string typeName)
        {
            base.WithName(typeName);
            return this;
        }

        /// <summary>
        /// 무명 타입을 추가합니다.
        /// </summary>
        public new TypeCombinerBuilder<T> With(object anonymousType)
        {
            base.With(anonymousType);
            return this;
        }

        /// <summary>
        /// EF Core projection 결과에서 스키마를 추출하여 타입을 생성합니다.
        /// </summary>
        /// <param name="projectionResult">EF Core Select().ToList() 결과</param>
        public new TypeCombinerBuilder<T> WithProjection(IEnumerable projectionResult)
        {
            base.WithProjection(projectionResult);
            return this;
        }

        /// <summary>
        /// 레코드 타입으로 생성합니다.
        /// </summary>
        public new TypeCombinerBuilder<T> AsRecord()
        {
            base.AsRecord();
            return this;
        }

        /// <summary>
        /// 클래스 타입으로 생성합니다.
        /// </summary>
        public new TypeCombinerBuilder<T> AsClass()
        {
            base.AsClass();
            return this;
        }

        /// <summary>
        /// 구조체 타입으로 생성합니다.
        /// </summary>
        public new TypeCombinerBuilder<T> AsStruct()
        {
            base.AsStruct();
            return this;
        }
    }
}