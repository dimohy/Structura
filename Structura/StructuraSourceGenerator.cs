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
    /// Structura Ÿ�� ������ ���� �ҽ� ������
    /// </summary>
    [Generator]
    public class StructuraSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // TypeCombiner.Generate() ȣ���� ã�� ���ý� ���ι��̴�
            var generateCalls = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => IsGenerateMethodCall(node),
                    transform: static (ctx, _) => GetGenerateCallInfo(ctx))
                .Where(static info => info != null);

            // ������ ��� ȣ���� �����Ͽ� �ҽ� ����
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

            // �÷��Ʈ ü�̴��� �м��Ͽ� Ÿ�� ���� ����
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
                        // TypeCombiner.Combine<T1, T2>() �Ǵ� TypeCombiner.From<T>() �м�
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
            // Lambda ǥ���Ŀ��� �Ӽ� �̸� ���� (������ ���̽���)
            if (argument?.Expression is SimpleLambdaExpressionSyntax lambda
                && lambda.Body is MemberAccessExpressionSyntax memberAccess)
            {
                return memberAccess.Name.Identifier.ValueText;
            }
            return null;
        }

        private static string? ExtractTypeOfValue(ArgumentSyntax? argument)
        {
            // typeof(Type) ���� Type ����
            if (argument?.Expression is TypeOfExpressionSyntax typeOf)
            {
                return typeOf.Type.ToString();
            }
            return null;
        }

        /// <summary>
        /// EF Core projection ��� �м�
        /// </summary>
        private static List<(string Name, string Type)>? AnalyzeProjection(ArgumentSyntax? argument, SemanticModel semanticModel)
        {
            if (argument?.Expression == null)
                return null;

            switch (argument.Expression)
            {
                case IdentifierNameSyntax identifier:
                    // ���� ����: projectionResult
                    return AnalyzeProjectionVariable(identifier, semanticModel);

                case MemberAccessExpressionSyntax memberAccess:
                    // �Ӽ�/�ʵ� ����: this.ProjectionResult
                    return AnalyzeProjectionMemberAccess(memberAccess, semanticModel);

                default:
                    return null;
            }
        }

        /// <summary>
        /// projection ���� �м� - List<�͸�Ÿ��>���� ��Ű�� ����
        /// </summary>
        private static List<(string Name, string Type)>? AnalyzeProjectionVariable(IdentifierNameSyntax identifier, SemanticModel semanticModel)
        {
            // ������ Ÿ�� ������ ������
            var typeInfo = semanticModel.GetTypeInfo(identifier);
            if (typeInfo.Type is not INamedTypeSymbol namedType)
                return null;

            // IEnumerable<T>, List<T>, ICollection<T> ���� �÷��� Ÿ������ Ȯ��
            var elementType = GetCollectionElementType(namedType);
            if (elementType == null)
                return null;

            // �͸� Ÿ������ Ȯ��
            if (!elementType.IsAnonymousType)
                return null;

            var properties = new List<(string Name, string Type)>();
            
            // �͸� Ÿ���� �Ӽ����� ����
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
        /// projection ��� ���� �м�
        /// </summary>
        private static List<(string Name, string Type)>? AnalyzeProjectionMemberAccess(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel)
        {
            // ��� ������ Ÿ�� ������ ������
            var typeInfo = semanticModel.GetTypeInfo(memberAccess);
            if (typeInfo.Type is not INamedTypeSymbol namedType)
                return null;

            // �÷��� Ÿ���� ��� Ÿ�� ����
            var elementType = GetCollectionElementType(namedType);
            if (elementType == null)
                return null;

            // �͸� Ÿ������ Ȯ��
            if (!elementType.IsAnonymousType)
                return null;

            var properties = new List<(string Name, string Type)>();
            
            // �͸� Ÿ���� �Ӽ����� ����
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
        /// �÷��� Ÿ�Կ��� ��� Ÿ�� ����
        /// </summary>
        private static INamedTypeSymbol? GetCollectionElementType(INamedTypeSymbol collectionType)
        {
            // IEnumerable<T>, List<T>, ICollection<T> ��� T ����
            if (collectionType.IsGenericType && collectionType.TypeArguments.Length == 1)
            {
                var elementType = collectionType.TypeArguments[0];
                if (elementType is INamedTypeSymbol namedElementType)
                {
                    return namedElementType;
                }
            }

            // �������̽� �߿��� IEnumerable<T> ã��
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
        /// ���� Ÿ�� �м� - ���� ������ ���� ���� ��� ����
        /// </summary>
        private static List<(string Name, string Type)>? AnalyzeAnonymousType(ArgumentSyntax? argument, SemanticModel semanticModel)
        {
            if (argument?.Expression == null)
                return null;

            switch (argument.Expression)
            {
                case AnonymousObjectCreationExpressionSyntax anonymousObj:
                    // ���� ���� ��ü ����: new { Name = "", Age = 0 }
                    return AnalyzeDirectAnonymousObject(anonymousObj);

                case IdentifierNameSyntax identifier:
                    // ���� ����: anonymousInstance
                    return AnalyzeVariableReference(identifier, semanticModel);

                case MemberAccessExpressionSyntax memberAccess:
                    // �Ӽ�/�ʵ� ����: this.SomeAnonymousObject
                    return AnalyzeMemberAccess(memberAccess, semanticModel);

                default:
                    return null;
            }
        }

        /// <summary>
        /// ���� ���� ��ü ���� �м�
        /// </summary>
        private static List<(string Name, string Type)> AnalyzeDirectAnonymousObject(AnonymousObjectCreationExpressionSyntax anonymousObj)
        {
            var properties = new List<(string Name, string Type)>();
            
            foreach (var initializer in anonymousObj.Initializers)
            {
                if (initializer is AnonymousObjectMemberDeclaratorSyntax declarator)
                {
                    string name;
                    
                    // �Ӽ� �̸� ����
                    if (declarator.NameEquals != null)
                    {
                        // ����� �̸�: new { Name = "value" }
                        name = declarator.NameEquals.Name.Identifier.ValueText;
                    }
                    else if (declarator.Expression is IdentifierNameSyntax identifierExpr)
                    {
                        // �Ͻ��� �̸�: new { variable }
                        name = identifierExpr.Identifier.ValueText;
                    }
                    else if (declarator.Expression is MemberAccessExpressionSyntax memberExpr)
                    {
                        // ��� ����: new { obj.Property }
                        name = memberExpr.Name.Identifier.ValueText;
                    }
                    else
                    {
                        continue; // �������� �ʴ� ǥ������ ��ŵ
                    }

                    var type = InferTypeFromExpression(declarator.Expression);
                    properties.Add((name, type));
                }
            }
            
            return properties;
        }

        /// <summary>
        /// ���� ���� �м� - SemanticModel�� ����Ͽ� ��Ȯ�� Ÿ�� ���� ����
        /// </summary>
        private static List<(string Name, string Type)>? AnalyzeVariableReference(IdentifierNameSyntax identifier, SemanticModel semanticModel)
        {
            // ������ Ÿ�� ������ ������
            var typeInfo = semanticModel.GetTypeInfo(identifier);
            if (typeInfo.Type is not INamedTypeSymbol namedType)
                return null;

            // �͸� Ÿ������ Ȯ��
            if (!namedType.IsAnonymousType)
                return null;

            var properties = new List<(string Name, string Type)>();
            
            // �͸� Ÿ���� �Ӽ����� ����
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
        /// ��� ���� �м� (this.field, obj.property ��)
        /// </summary>
        private static List<(string Name, string Type)>? AnalyzeMemberAccess(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel)
        {
            // ��� ������ Ÿ�� ������ ������
            var typeInfo = semanticModel.GetTypeInfo(memberAccess);
            if (typeInfo.Type is not INamedTypeSymbol namedType)
                return null;

            // �͸� Ÿ������ Ȯ��
            if (!namedType.IsAnonymousType)
                return null;

            var properties = new List<(string Name, string Type)>();
            
            // �͸� Ÿ���� �Ӽ����� ����
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
        /// ǥ�������κ��� Ÿ�� �߷� (�⺻�� ���)
        /// </summary>
        private static string InferTypeFromExpression(ExpressionSyntax expression)
        {
            switch (expression)
            {
                case LiteralExpressionSyntax literal:
                    return InferTypeFromLiteral(literal);

                case IdentifierNameSyntax identifier:
                    // ���� ������ ��� �⺻������ object ��ȯ
                    return "object";

                case MemberAccessExpressionSyntax memberAccess:
                    // ��� ������ ��� �⺻������ object ��ȯ
                    return "object";

                case InvocationExpressionSyntax invocation:
                    // �޼��� ȣ���� ��� �⺻������ object ��ȯ
                    return "object";

                case ArrayCreationExpressionSyntax arrayCreation:
                    // �迭 ����
                    if (arrayCreation.Type.ElementType != null)
                    {
                        return $"{arrayCreation.Type.ElementType}[]";
                    }
                    return "object[]";

                case ObjectCreationExpressionSyntax objectCreation:
                    // ��ü ����
                    return objectCreation.Type?.ToString() ?? "object";

                default:
                    return "object";
            }
        }

        /// <summary>
        /// ���ͷ��κ��� Ÿ�� �߷�
        /// </summary>
        private static string InferTypeFromLiteral(LiteralExpressionSyntax literal)
        {
            if (literal.Token.IsKind(SyntaxKind.StringLiteralToken))
                return "string";
                
            if (literal.Token.IsKind(SyntaxKind.NumericLiteralToken))
            {
                var text = literal.Token.ValueText;
                
                // decimal ���ͷ�
                if (text.EndsWith("m", System.StringComparison.OrdinalIgnoreCase))
                    return "decimal";
                    
                // float ���ͷ�
                if (text.EndsWith("f", System.StringComparison.OrdinalIgnoreCase))
                    return "float";
                    
                // long ���ͷ�
                if (text.EndsWith("l", System.StringComparison.OrdinalIgnoreCase))
                    return "long";
                    
                // double ���ͷ� (�Ҽ��� ����)
                if (text.Contains('.'))
                    return "double";
                    
                // int ���ͷ�
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
                // ���ڵ� Ÿ�� ����
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
                // Ŭ���� �Ǵ� ����ü Ÿ�� ����
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
                    
                    // ���� Ÿ���� ��� required Ű���� �߰��Ͽ� CS8618 ��� �ذ�
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
        /// Ÿ���� ���� Ÿ������ Ȯ��
        /// </summary>
        private static bool IsReferenceType(string typeName)
        {
            // �� Ÿ�Ե�
            var valueTypes = new HashSet<string>
            {
                "int", "long", "float", "double", "decimal", "bool", "byte", "sbyte",
                "short", "ushort", "uint", "ulong", "char", "DateTime", "Guid",
                "System.Int32", "System.Int64", "System.Single", "System.Double",
                "System.Decimal", "System.Boolean", "System.Byte", "System.SByte",
                "System.Int16", "System.UInt16", "System.UInt32", "System.UInt64",
                "System.Char", "System.DateTime", "System.Guid"
            };

            // Nullable Ÿ���� ��� (�̹� nullable�̹Ƿ� required ���ʿ�)
            if (typeName.EndsWith("?"))
                return false;

            // �� Ÿ���� ���
            if (valueTypes.Contains(typeName))
                return false;

            // ����ü Ÿ���� ��� (struct Ű���尡 ���Ե� Ÿ�Ե��� �� Ÿ��)
            // ���⼭�� ������ �Ϲ����� ��츸 ó��
            return true; // ��κ��� ��� ���� Ÿ������ ����
        }

        private static List<(string Name, string Type)> CollectProperties(GenerateCallInfo callInfo)
        {
            var properties = new List<(string Name, string Type)>();
            // ���� �ҽ� Ÿ�Ե�κ��� �Ӽ� �߰� (TypeSymbol ���)
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

            // Exclude �۾� ó��
            var excludedProperties = new HashSet<string>();
            foreach (var operation in callInfo.Operations)
            {
                if (operation.OperationType == FluentOperationType.Exclude && 
                    operation.PropertyName != null)
                {
                    excludedProperties.Add(operation.PropertyName);
                }
            }

            // ���ܵ� �Ӽ� ����
            properties = properties.Where(p => !excludedProperties.Contains(p.Name)).ToList();

            // �ٸ� �۾��� ó��
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
                            // ���� �Ӽ��� Ÿ�� ����
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
            // C# Ű����� ��ȯ
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

    // ���� Ŭ������
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