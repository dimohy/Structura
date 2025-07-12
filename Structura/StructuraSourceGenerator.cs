using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Structura.SourceGenerators
{
    /// <summary>
    /// Structura 타입 조합을 위한 소스 생성기
    /// </summary>
    [Generator]
    public class StructuraSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // TypeCombiner.Generate() 호출을 찾는 신택스 프로바이더
            var generateCalls = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => IsGenerateMethodCall(node),
                    transform: static (ctx, _) => GetGenerateCallInfo(ctx))
                .Where(static info => info != null);

            // 수집된 모든 호출을 결합하여 소스 생성
            context.RegisterSourceOutput(
                generateCalls.Collect(),
                static (spc, generateCalls) => GenerateTypes(spc, generateCalls));
        }

        private static bool IsGenerateMethodCall(SyntaxNode node)
        {
            return node is InvocationExpressionSyntax invocation
                && invocation.Expression is MemberAccessExpressionSyntax memberAccess
                && memberAccess.Name.Identifier.ValueText == "Generate";
        }

        private static GenerateCallInfo? GetGenerateCallInfo(GeneratorSyntaxContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

            // 플루언트 체이닝을 분석하여 타입 생성 정보
            var chainInfo = AnalyzeFluentChain(memberAccess.Expression, context.SemanticModel);
            if (chainInfo == null)
                return null;

            return new GenerateCallInfo
            {
                TargetTypeName = chainInfo.TypeName,
                SourceTypes = chainInfo.SourceTypes,
                Operations = chainInfo.Operations,
                GenerationMode = chainInfo.GenerationMode
            };
        }

        private static FluentChainInfo? AnalyzeFluentChain(SyntaxNode expression, SemanticModel semanticModel)
        {
            var operations = new List<FluentOperation>();
            var sourceTypes = new List<INamedTypeSymbol>();
            string typeName = "";
            var generationMode = TypeGenerationMode.Record;

            var current = expression;
            while (current != null)
            {
                switch (current)
                {
                    case InvocationExpressionSyntax invocation:
                        var operation = AnalyzeInvocation(invocation, semanticModel);
                        if (operation != null)
                        {
                            operations.Insert(0, operation);
                            if (operation.OperationType == FluentOperationType.WithName)
                            {
                                typeName = operation.StringValue ?? "";
                            }
                            else if (operation.OperationType == FluentOperationType.AsRecord)
                            {
                                generationMode = TypeGenerationMode.Record;
                            }
                            else if (operation.OperationType == FluentOperationType.AsClass)
                            {
                                generationMode = TypeGenerationMode.Class;
                            }
                            else if (operation.OperationType == FluentOperationType.AsStruct)
                            {
                                generationMode = TypeGenerationMode.Struct;
                            }
                        }

                        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                        {
                            current = memberAccess.Expression;
                        }
                        else
                        {
                            current = null;
                        }
                        break;

                    case MemberAccessExpressionSyntax memberAccessExpr:
                        // TypeCombiner.Combine<T1, T2>() 또는 TypeCombiner.From<T>() 분석
                        if (memberAccessExpr.Expression is IdentifierNameSyntax identifier 
                            && identifier.Identifier.ValueText == "TypeCombiner")
                        {
                            var methodName = memberAccessExpr.Name;
                            if (methodName is GenericNameSyntax genericName)
                            {
                                foreach (var typeArg in genericName.TypeArgumentList.Arguments)
                                {
                                    var typeInfo = semanticModel.GetTypeInfo(typeArg);
                                    if (typeInfo.Type is INamedTypeSymbol namedType)
                                    {
                                        sourceTypes.Add(namedType);
                                    }
                                }
                            }
                        }
                        current = null;
                        break;

                    default:
                        current = null;
                        break;
                }
            }

            if (string.IsNullOrEmpty(typeName))
                return null;

            return new FluentChainInfo
            {
                TypeName = typeName,
                SourceTypes = sourceTypes,
                Operations = operations,
                GenerationMode = generationMode
            };
        }

        private static FluentOperation? AnalyzeInvocation(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            if (!(invocation.Expression is MemberAccessExpressionSyntax memberAccess))
                return null;

            var methodName = memberAccess.Name.Identifier.ValueText;

            switch (methodName)
            {
                case "WithName":
                    return new FluentOperation
                    {
                        OperationType = FluentOperationType.WithName,
                        StringValue = GetStringLiteralValue(invocation.ArgumentList.Arguments.FirstOrDefault())
                    };
                case "Exclude":
                    return new FluentOperation
                    {
                        OperationType = FluentOperationType.Exclude,
                        PropertyName = ExtractPropertyName(invocation.ArgumentList.Arguments.FirstOrDefault())
                    };
                case "Add":
                    return AnalyzeAddOperation(invocation);
                case "ChangeType":
                    return AnalyzeChangeTypeOperation(invocation);
                case "With":
                    return new FluentOperation
                    {
                        OperationType = FluentOperationType.With,
                        AnonymousTypeInfo = AnalyzeAnonymousType(invocation.ArgumentList.Arguments.FirstOrDefault(), semanticModel)
                    };
                case "WithProjection":
                    return new FluentOperation
                    {
                        OperationType = FluentOperationType.WithProjection,
                        ProjectionInfo = AnalyzeProjection(invocation.ArgumentList.Arguments.FirstOrDefault(), semanticModel)
                    };
                case "AsRecord":
                    return new FluentOperation { OperationType = FluentOperationType.AsRecord };
                case "AsClass":
                    return new FluentOperation { OperationType = FluentOperationType.AsClass };
                case "AsStruct":
                    return new FluentOperation { OperationType = FluentOperationType.AsStruct };
                default:
                    return null;
            }
        }

        private static FluentOperation? AnalyzeAddOperation(InvocationExpressionSyntax invocation)
        {
            var args = invocation.ArgumentList.Arguments;
            if (args.Count < 2)
                return null;

            return new FluentOperation
            {
                OperationType = FluentOperationType.Add,
                PropertyName = GetStringLiteralValue(args[0]),
                PropertyType = ExtractTypeOfValue(args[1]),
                DefaultValue = args.Count > 2 ? args[2].Expression.ToString() : null
            };
        }

        private static FluentOperation? AnalyzeChangeTypeOperation(InvocationExpressionSyntax invocation)
        {
            var args = invocation.ArgumentList.Arguments;
            if (args.Count < 2)
                return null;

            return new FluentOperation
            {
                OperationType = FluentOperationType.ChangeType,
                PropertyName = ExtractPropertyName(args[0]),
                PropertyType = ExtractTypeOfValue(args[1])
            };
        }

        private static string? GetStringLiteralValue(ArgumentSyntax? argument)
        {
            if (argument?.Expression is LiteralExpressionSyntax literal
                && literal.Token.IsKind(SyntaxKind.StringLiteralToken))
            {
                return literal.Token.ValueText;
            }
            return null;
        }

        private static string? ExtractPropertyName(ArgumentSyntax? argument)
        {
            // Lambda 표현식에서 속성 이름 추출 (간단한 케이스만)
            if (argument?.Expression is SimpleLambdaExpressionSyntax lambda
                && lambda.Body is MemberAccessExpressionSyntax memberAccess)
            {
                return memberAccess.Name.Identifier.ValueText;
            }
            return null;
        }

        private static string? ExtractTypeOfValue(ArgumentSyntax? argument)
        {
            // typeof(Type) 에서 Type 추출
            if (argument?.Expression is TypeOfExpressionSyntax typeOf)
            {
                return typeOf.Type.ToString();
            }
            return null;
        }

        /// <summary>
        /// EF Core projection 결과 분석
        /// </summary>
        private static List<(string Name, string Type)>? AnalyzeProjection(ArgumentSyntax? argument, SemanticModel semanticModel)
        {
            if (argument?.Expression == null)
                return null;

            switch (argument.Expression)
            {
                case IdentifierNameSyntax identifier:
                    // 변수 참조: projectionResult
                    return AnalyzeProjectionVariable(identifier, semanticModel);

                case MemberAccessExpressionSyntax memberAccess:
                    // 속성/필드 접근: this.ProjectionResult
                    return AnalyzeProjectionMemberAccess(memberAccess, semanticModel);

                default:
                    return null;
            }
        }

        /// <summary>
        /// projection 변수 분석 - List<익명타입>에서 스키마 추출
        /// </summary>
        private static List<(string Name, string Type)>? AnalyzeProjectionVariable(IdentifierNameSyntax identifier, SemanticModel semanticModel)
        {
            // 변수의 타입 정보를 가져옴
            var typeInfo = semanticModel.GetTypeInfo(identifier);
            if (typeInfo.Type is not INamedTypeSymbol namedType)
                return null;

            // IEnumerable<T>, List<T>, ICollection<T> 등의 컬렉션 타입인지 확인
            var elementType = GetCollectionElementType(namedType);
            if (elementType == null)
                return null;

            // 익명 타입인지 확인
            if (!elementType.IsAnonymousType)
                return null;

            var properties = new List<(string Name, string Type)>();
            
            // 익명 타입의 속성들을 추출
            foreach (var member in elementType.GetMembers())
            {
                if (member is IPropertySymbol property && 
                    property.DeclaredAccessibility == Accessibility.Public)
                {
                    var propertyName = property.Name;
                    var propertyType = FormatTypeName(property.Type);
                    properties.Add((propertyName, propertyType));
                }
            }

            return properties.Count > 0 ? properties : null;
        }

        /// <summary>
        /// projection 멤버 접근 분석
        /// </summary>
        private static List<(string Name, string Type)>? AnalyzeProjectionMemberAccess(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel)
        {
            // 멤버 접근의 타입 정보를 가져옴
            var typeInfo = semanticModel.GetTypeInfo(memberAccess);
            if (typeInfo.Type is not INamedTypeSymbol namedType)
                return null;

            // 컬렉션 타입의 요소 타입 추출
            var elementType = GetCollectionElementType(namedType);
            if (elementType == null)
                return null;

            // 익명 타입인지 확인
            if (!elementType.IsAnonymousType)
                return null;

            var properties = new List<(string Name, string Type)>();
            
            // 익명 타입의 속성들을 추출
            foreach (var member in elementType.GetMembers())
            {
                if (member is IPropertySymbol property && 
                    property.DeclaredAccessibility == Accessibility.Public)
                {
                    var propertyName = property.Name;
                    var propertyType = FormatTypeName(property.Type);
                    properties.Add((propertyName, propertyType));
                }
            }

            return properties.Count > 0 ? properties : null;
        }

        /// <summary>
        /// 컬렉션 타입에서 요소 타입 추출
        /// </summary>
        private static INamedTypeSymbol? GetCollectionElementType(INamedTypeSymbol collectionType)
        {
            // IEnumerable<T>, List<T>, ICollection<T> 등에서 T 추출
            if (collectionType.IsGenericType && collectionType.TypeArguments.Length == 1)
            {
                var elementType = collectionType.TypeArguments[0];
                if (elementType is INamedTypeSymbol namedElementType)
                {
                    return namedElementType;
                }
            }

            // 인터페이스 중에서 IEnumerable<T> 찾기
            foreach (var interfaceType in collectionType.AllInterfaces)
            {
                if (interfaceType.IsGenericType && 
                    interfaceType.ConstructedFrom.ToDisplayString() == "System.Collections.Generic.IEnumerable<T>" &&
                    interfaceType.TypeArguments.Length == 1)
                {
                    var elementType = interfaceType.TypeArguments[0];
                    if (elementType is INamedTypeSymbol namedElementType)
                    {
                        return namedElementType;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 무명 타입 분석 - 기존 변수나 직접 생성 모두 지원
        /// </summary>
        private static List<(string Name, string Type)>? AnalyzeAnonymousType(ArgumentSyntax? argument, SemanticModel semanticModel)
        {
            if (argument?.Expression == null)
                return null;

            switch (argument.Expression)
            {
                case AnonymousObjectCreationExpressionSyntax anonymousObj:
                    // 직접 무명 객체 생성: new { Name = "", Age = 0 }
                    return AnalyzeDirectAnonymousObject(anonymousObj);

                case IdentifierNameSyntax identifier:
                    // 변수 참조: anonymousInstance
                    return AnalyzeVariableReference(identifier, semanticModel);

                case MemberAccessExpressionSyntax memberAccess:
                    // 속성/필드 접근: this.SomeAnonymousObject
                    return AnalyzeMemberAccess(memberAccess, semanticModel);

                default:
                    return null;
            }
        }

        /// <summary>
        /// 직접 무명 객체 생성 분석
        /// </summary>
        private static List<(string Name, string Type)> AnalyzeDirectAnonymousObject(AnonymousObjectCreationExpressionSyntax anonymousObj)
        {
            var properties = new List<(string Name, string Type)>();
            
            foreach (var initializer in anonymousObj.Initializers)
            {
                if (initializer is AnonymousObjectMemberDeclaratorSyntax declarator)
                {
                    string name;
                    
                    // 속성 이름 결정
                    if (declarator.NameEquals != null)
                    {
                        // 명시적 이름: new { Name = "value" }
                        name = declarator.NameEquals.Name.Identifier.ValueText;
                    }
                    else if (declarator.Expression is IdentifierNameSyntax identifierExpr)
                    {
                        // 암시적 이름: new { variable }
                        name = identifierExpr.Identifier.ValueText;
                    }
                    else if (declarator.Expression is MemberAccessExpressionSyntax memberExpr)
                    {
                        // 멤버 접근: new { obj.Property }
                        name = memberExpr.Name.Identifier.ValueText;
                    }
                    else
                    {
                        continue; // 지원하지 않는 표현식은 스킵
                    }

                    var type = InferTypeFromExpression(declarator.Expression);
                    properties.Add((name, type));
                }
            }
            
            return properties;
        }

        /// <summary>
        /// 변수 참조 분석 - SemanticModel을 사용하여 정확한 타입 정보 추출
        /// </summary>
        private static List<(string Name, string Type)>? AnalyzeVariableReference(IdentifierNameSyntax identifier, SemanticModel semanticModel)
        {
            // 변수의 타입 정보를 가져옴
            var typeInfo = semanticModel.GetTypeInfo(identifier);
            if (typeInfo.Type is not INamedTypeSymbol namedType)
                return null;

            // 익명 타입인지 확인
            if (!namedType.IsAnonymousType)
                return null;

            var properties = new List<(string Name, string Type)>();
            
            // 익명 타입의 속성들을 추출
            foreach (var member in namedType.GetMembers())
            {
                if (member is IPropertySymbol property && 
                    property.DeclaredAccessibility == Accessibility.Public)
                {
                    var propertyName = property.Name;
                    var propertyType = FormatTypeName(property.Type);
                    properties.Add((propertyName, propertyType));
                }
            }

            return properties.Count > 0 ? properties : null;
        }

        /// <summary>
        /// 멤버 접근 분석 (this.field, obj.property 등)
        /// </summary>
        private static List<(string Name, string Type)>? AnalyzeMemberAccess(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel)
        {
            // 멤버 접근의 타입 정보를 가져옴
            var typeInfo = semanticModel.GetTypeInfo(memberAccess);
            if (typeInfo.Type is not INamedTypeSymbol namedType)
                return null;

            // 익명 타입인지 확인
            if (!namedType.IsAnonymousType)
                return null;

            var properties = new List<(string Name, string Type)>();
            
            // 익명 타입의 속성들을 추출
            foreach (var member in namedType.GetMembers())
            {
                if (member is IPropertySymbol property && 
                    property.DeclaredAccessibility == Accessibility.Public)
                {
                    var propertyName = property.Name;
                    var propertyType = FormatTypeName(property.Type);
                    properties.Add((propertyName, propertyType));
                }
            }

            return properties.Count > 0 ? properties : null;
        }

        /// <summary>
        /// 표현식으로부터 타입 추론 (기본적 방법)
        /// </summary>
        private static string InferTypeFromExpression(ExpressionSyntax expression)
        {
            switch (expression)
            {
                case LiteralExpressionSyntax literal:
                    return InferTypeFromLiteral(literal);

                case IdentifierNameSyntax identifier:
                    // 변수 참조의 경우 기본적으로 object 반환
                    return "object";

                case MemberAccessExpressionSyntax memberAccess:
                    // 멤버 접근의 경우 기본적으로 object 반환
                    return "object";

                case InvocationExpressionSyntax invocation:
                    // 메서드 호출의 경우 기본적으로 object 반환
                    return "object";

                case ArrayCreationExpressionSyntax arrayCreation:
                    // 배열 생성
                    if (arrayCreation.Type.ElementType != null)
                    {
                        return $"{arrayCreation.Type.ElementType}[]";
                    }
                    return "object[]";

                case ObjectCreationExpressionSyntax objectCreation:
                    // 객체 생성
                    return objectCreation.Type?.ToString() ?? "object";

                default:
                    return "object";
            }
        }

        /// <summary>
        /// 리터럴로부터 타입 추론
        /// </summary>
        private static string InferTypeFromLiteral(LiteralExpressionSyntax literal)
        {
            if (literal.Token.IsKind(SyntaxKind.StringLiteralToken))
                return "string";
                
            if (literal.Token.IsKind(SyntaxKind.NumericLiteralToken))
            {
                var text = literal.Token.ValueText;
                
                // decimal 리터럴
                if (text.EndsWith("m", System.StringComparison.OrdinalIgnoreCase))
                    return "decimal";
                    
                // float 리터럴
                if (text.EndsWith("f", System.StringComparison.OrdinalIgnoreCase))
                    return "float";
                    
                // long 리터럴
                if (text.EndsWith("l", System.StringComparison.OrdinalIgnoreCase))
                    return "long";
                    
                // double 리터럴 (소수점 포함)
                if (text.Contains('.'))
                    return "double";
                    
                // int 리터럴
                return "int";
            }
            
            if (literal.Token.IsKind(SyntaxKind.TrueKeyword) || literal.Token.IsKind(SyntaxKind.FalseKeyword))
                return "bool";
                
            if (literal.Token.IsKind(SyntaxKind.NullKeyword))
                return "object?";
                
            return "object";
        }

        private static void GenerateTypes(SourceProductionContext context, ImmutableArray<GenerateCallInfo?> generateCalls)
        {
            foreach (var callInfo in generateCalls)
            {
                if (callInfo == null)
                    continue;

                var sourceCode = GenerateTypeSource(callInfo);
                context.AddSource($"{callInfo.TargetTypeName}.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
            }
        }

        private static string GenerateTypeSource(GenerateCallInfo callInfo)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("// <auto-generated />");
            sb.AppendLine("// This code was generated by Structura Source Generator");
            sb.AppendLine("// Supports variable reference analysis and EF Core projection results");
            sb.AppendLine("#nullable enable");
            sb.AppendLine();
            sb.AppendLine("namespace Generated");
            sb.AppendLine("{");

            var properties = CollectProperties(callInfo);

            if (callInfo.GenerationMode == TypeGenerationMode.Record)
            {
                // 레코드 타입 생성
                sb.AppendLine($"    /// <summary>");
                sb.AppendLine($"    /// Generated record type: {callInfo.TargetTypeName}");
                sb.AppendLine($"    /// Properties analyzed from anonymous types, variable references, and EF Core projections");
                sb.AppendLine($"    /// </summary>");
                sb.AppendLine($"    public record {callInfo.TargetTypeName}(");
                for (int i = 0; i < properties.Count; i++)
                {
                    var (name, type) = properties[i];
                    sb.Append($"        {type} {name}");
                    if (i < properties.Count - 1)
                        sb.AppendLine(",");
                    else
                        sb.AppendLine();
                }
                sb.AppendLine("    );");
            }
            else
            {
                // 클래스 또는 구조체 타입 생성
                var typeKeyword = callInfo.GenerationMode == TypeGenerationMode.Class ? "class" : "struct";
                sb.AppendLine($"    /// <summary>");
                sb.AppendLine($"    /// Generated {typeKeyword} type: {callInfo.TargetTypeName}");
                sb.AppendLine($"    /// Properties analyzed from anonymous types, variable references, and EF Core projections");
                sb.AppendLine($"    /// </summary>");
                sb.AppendLine($"    public {typeKeyword} {callInfo.TargetTypeName}");
                sb.AppendLine("    {");

                foreach (var (name, type) in properties)
                {
                    sb.AppendLine($"        /// <summary>Property: {name} (Type: {type})</summary>");
                    
                    // 참조 타입인 경우 required 키워드 추가하여 CS8618 경고 해결
                    var isReferenceType = IsReferenceType(type);
                    var requiredKeyword = isReferenceType ? "required " : "";
                    
                    sb.AppendLine($"        public {requiredKeyword}{type} {name} {{ get; set; }}");
                    sb.AppendLine();
                }

                sb.AppendLine("    }");
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        /// <summary>
        /// 타입이 참조 타입인지 확인
        /// </summary>
        private static bool IsReferenceType(string typeName)
        {
            // 값 타입들
            var valueTypes = new HashSet<string>
            {
                "int", "long", "float", "double", "decimal", "bool", "byte", "sbyte",
                "short", "ushort", "uint", "ulong", "char", "DateTime", "Guid",
                "System.Int32", "System.Int64", "System.Single", "System.Double",
                "System.Decimal", "System.Boolean", "System.Byte", "System.SByte",
                "System.Int16", "System.UInt16", "System.UInt32", "System.UInt64",
                "System.Char", "System.DateTime", "System.Guid"
            };

            // Nullable 타입인 경우 (이미 nullable이므로 required 불필요)
            if (typeName.EndsWith("?"))
                return false;

            // 값 타입인 경우
            if (valueTypes.Contains(typeName))
                return false;

            // 구조체 타입인 경우 (struct 키워드가 포함된 타입들은 값 타입)
            // 여기서는 간단히 일반적인 경우만 처리
            return true; // 대부분의 경우 참조 타입으로 가정
        }

        private static List<(string Name, string Type)> CollectProperties(GenerateCallInfo callInfo)
        {
            var properties = new List<(string Name, string Type)>();
            // 기존 소스 타입들로부터 속성 추가 (TypeSymbol 기반)
            foreach (var sourceType in callInfo.SourceTypes)
            {
                foreach (var member in sourceType.GetMembers())
                {
                    if (member is IPropertySymbol property && 
                        property.DeclaredAccessibility == Accessibility.Public)
                    {
                        properties.Add((property.Name, FormatTypeName(property.Type)));
                    }
                }
            }

            // Exclude 작업 처리
            var excludedProperties = new HashSet<string>();
            foreach (var operation in callInfo.Operations)
            {
                if (operation.OperationType == FluentOperationType.Exclude && 
                    operation.PropertyName != null)
                {
                    excludedProperties.Add(operation.PropertyName);
                }
            }

            // 제외된 속성 제거
            properties = properties.Where(p => !excludedProperties.Contains(p.Name)).ToList();

            // 다른 작업들 처리
            foreach (var operation in callInfo.Operations)
            {
                switch (operation.OperationType)
                {
                    case FluentOperationType.Add:
                        if (operation.PropertyName != null && operation.PropertyType != null)
                        {
                            properties.Add((operation.PropertyName, operation.PropertyType));
                        }
                        break;

                    case FluentOperationType.With:
                        if (operation.AnonymousTypeInfo != null)
                        {
                            properties.AddRange(operation.AnonymousTypeInfo);
                        }
                        break;

                    case FluentOperationType.WithProjection:
                        if (operation.ProjectionInfo != null)
                        {
                            properties.AddRange(operation.ProjectionInfo);
                        }
                        break;

                    case FluentOperationType.ChangeType:
                        if (operation.PropertyName != null && operation.PropertyType != null)
                        {
                            // 기존 속성의 타입 변경
                            for (int i = 0; i < properties.Count; i++)
                            {
                                if (properties[i].Name == operation.PropertyName)
                                {
                                    properties[i] = (operation.PropertyName, operation.PropertyType);
                                    break;
                                }
                            }
                        }
                        break;
                }
            }

            return properties;
        }

        private static string FormatTypeName(ITypeSymbol typeSymbol)
        {
            // C# 키워드로 변환
            return typeSymbol.SpecialType switch
            {
                SpecialType.System_String => "string",
                SpecialType.System_Int32 => "int",
                SpecialType.System_Int64 => "long",
                SpecialType.System_Double => "double",
                SpecialType.System_Single => "float",
                SpecialType.System_Boolean => "bool",
                SpecialType.System_Decimal => "decimal",
                SpecialType.System_DateTime => "DateTime",
                _ => typeSymbol.ToDisplayString()
            };
        }
    }

    // 지원 클래스들
    public class GenerateCallInfo
    {
        public string TargetTypeName { get; set; } = "";
        public List<INamedTypeSymbol> SourceTypes { get; set; } = new List<INamedTypeSymbol>();
        public List<FluentOperation> Operations { get; set; } = new List<FluentOperation>();
        public TypeGenerationMode GenerationMode { get; set; } = TypeGenerationMode.Record;
    }

    public class FluentChainInfo
    {
        public string TypeName { get; set; } = "";
        public List<INamedTypeSymbol> SourceTypes { get; set; } = new List<INamedTypeSymbol>();
        public List<FluentOperation> Operations { get; set; } = new List<FluentOperation>();
        public TypeGenerationMode GenerationMode { get; set; } = TypeGenerationMode.Record;
    }

    public class FluentOperation
    {
        public FluentOperationType OperationType { get; set; }
        public string? PropertyName { get; set; }
        public string? PropertyType { get; set; }
        public string? StringValue { get; set; }
        public string? DefaultValue { get; set; }
        public List<(string Name, string Type)>? AnonymousTypeInfo { get; set; }
        public List<(string Name, string Type)>? ProjectionInfo { get; set; }
    }

    public enum FluentOperationType
    {
        WithName,
        Exclude,
        Add,
        ChangeType,
        With,
        WithProjection,
        AsRecord,
        AsClass,
        AsStruct
    }
}