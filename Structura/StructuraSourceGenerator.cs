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
    /// Source generator for Structura type combination
    /// </summary>
    [Generator]
    public class StructuraSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // ?? **Generate() 호출 감지** - AnonymousTypeCombinerBuilder.Generate() 호출만 감지
            var generateCalls = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => IsGenerateMethodCall(node),
                    transform: static (ctx, _) => GetGenerateCallInfo(ctx))
                .Where(static info => info != null);

            // Generate source for all found calls
            context.RegisterSourceOutput(
                generateCalls.Collect(),
                static (spc, generateCalls) => GenerateTypes(spc, generateCalls));
        }

        private static bool IsGenerateMethodCall(SyntaxNode node)
        {
            // ?? **Generate() 메서드 호출 감지**
            if (node is InvocationExpressionSyntax invocation
                && invocation.Expression is MemberAccessExpressionSyntax memberAccess
                && memberAccess.Name.Identifier.ValueText == "Generate")
            {
                return true;
            }
            return false;
        }

        private static GenerateCallInfo? GetGenerateCallInfo(GeneratorSyntaxContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

            // ?? **플루언트 체인 분석** - TypeCombiner.Combine() 체인을 처리
            var chainInfo = AnalyzeFluentChain(memberAccess.Expression, context.SemanticModel);
            if (chainInfo == null)
            {
                return null;
            }

            return new GenerateCallInfo
            {
                TargetTypeName = chainInfo.TypeName,
                TargetNamespace = chainInfo.Namespace,
                Operations = chainInfo.Operations,
                GenerationMode = chainInfo.GenerationMode,
                EnableConverter = chainInfo.EnableConverter
            };
        }

        private static FluentChainInfo? AnalyzeFluentChain(SyntaxNode expression, SemanticModel semanticModel)
        {
            var operations = new List<FluentOperation>();
            string typeName = "";
            string namespaceName = "Generated";
            var generationMode = TypeGenerationMode.Record;
            bool enableConverter = false;

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
                                if (!string.IsNullOrEmpty(operation.NamespaceValue))
                                {
                                    namespaceName = operation.NamespaceValue;
                                }
                            }
                            else if (operation.OperationType == FluentOperationType.WithConverter)
                            {
                                enableConverter = true;
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
                        // ?? **TypeCombiner.Combine() 호출만 처리**
                        if (memberAccessExpr.Expression is IdentifierNameSyntax identifier 
                            && identifier.Identifier.ValueText == "TypeCombiner"
                            && memberAccessExpr.Name.Identifier.ValueText == "Combine")
                        {
                            // TypeCombiner.Combine() 호출 감지됨
                            // AnonymousTypeCombinerBuilder로 반환되므로 여기서는 추가 작업 없음
                        }
                        current = null;
                        break;

                    default:
                        current = null;
                        break;
                }
            }

            // 타입 이름이 없으면 무효
            if (string.IsNullOrEmpty(typeName))
                return null;

            return new FluentChainInfo
            {
                TypeName = typeName,
                Namespace = namespaceName,
                Operations = operations,
                GenerationMode = generationMode,
                EnableConverter = enableConverter
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
                    var args = invocation.ArgumentList.Arguments;
                    return new FluentOperation
                    {
                        OperationType = FluentOperationType.WithName,
                        StringValue = GetStringLiteralValue(args.FirstOrDefault()),
                        NamespaceValue = args.Count > 1 ? GetStringLiteralValue(args[1]) : null
                    };
                case "WithConverter":
                    return new FluentOperation { OperationType = FluentOperationType.WithConverter };
                case "Add":
                    return AnalyzeAddOperation(invocation);
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
                case "Exclude":
                    return new FluentOperation
                    {
                        OperationType = FluentOperationType.Exclude,
                        ExcludedPropertyName = GetStringLiteralValue(invocation.ArgumentList.Arguments.FirstOrDefault())
                    };
                case "ChangeType":
                    return AnalyzeChangeTypeOperation(invocation);
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

        private static FluentOperation? AnalyzeChangeTypeOperation(InvocationExpressionSyntax invocation)
        {
            var args = invocation.ArgumentList.Arguments;
            if (args.Count < 2)
                return null;

            return new FluentOperation
            {
                OperationType = FluentOperationType.ChangeType,
                ChangeTypePropertyName = GetStringLiteralValue(args[0]),
                NewType = ExtractTypeOfValue(args[1])
            };
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

        private static string? GetStringLiteralValue(ArgumentSyntax? argument)
        {
            if (argument?.Expression is LiteralExpressionSyntax literal
                && literal.Token.IsKind(SyntaxKind.StringLiteralToken))
            {
                return literal.Token.ValueText;
            }
            return null;
        }

        private static string? ExtractTypeOfValue(ArgumentSyntax? argument)
        {
            // Extract Type from typeof(Type) expression
            if (argument?.Expression is TypeOfExpressionSyntax typeOf)
            {
                return typeOf.Type.ToString();
            }
            return null;
        }

        /// <summary>
        /// Analyze EF Core projection results
        /// </summary>
        private static List<(string Name, string Type)>? AnalyzeProjection(ArgumentSyntax? argument, SemanticModel semanticModel)
        {
            if (argument?.Expression == null)
                return null;

            switch (argument.Expression)
            {
                case IdentifierNameSyntax identifier:
                    // Variable reference: projectionResult
                    return AnalyzeProjectionVariable(identifier, semanticModel);

                case MemberAccessExpressionSyntax memberAccess:
                    // Property/field reference: this.ProjectionResult
                    return AnalyzeProjectionMemberAccess(memberAccess, semanticModel);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Analyze projection variable - extract properties from List&lt;AnonymousType&gt;, arrays, etc.
        /// </summary>
        private static List<(string Name, string Type)>? AnalyzeProjectionVariable(IdentifierNameSyntax identifier, SemanticModel semanticModel)
        {
            // Get type information for the variable
            var typeInfo = semanticModel.GetTypeInfo(identifier);
            
            // 배열 타입 처리
            if (typeInfo.Type is IArrayTypeSymbol arrayType)
            {
                var elementType = arrayType.ElementType;
                
                // Check if it's an anonymous type
                if (elementType.IsAnonymousType)
                {
                    var properties = new List<(string Name, string Type)>();
                    
                    // Extract properties from the anonymous type
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
            }
            
            // 기존 Named Type 처리
            if (typeInfo.Type is not INamedTypeSymbol namedType)
                return null;

            // Check for collection types like IEnumerable<T>, List<T>, ICollection<T>
            var elementTypeFromCollection = GetCollectionElementType(namedType);
            if (elementTypeFromCollection == null)
                return null;

            // Check if it's an anonymous type
            if (!elementTypeFromCollection.IsAnonymousType)
                return null;

            var collectionProperties = new List<(string Name, string Type)>();
            
            // Extract properties from the anonymous type
            foreach (var member in elementTypeFromCollection.GetMembers())
            {
                if (member is IPropertySymbol property && 
                    property.DeclaredAccessibility == Accessibility.Public)
                {
                    var propertyName = property.Name;
                    var propertyType = FormatTypeName(property.Type);
                    collectionProperties.Add((propertyName, propertyType));
                }
            }

            return collectionProperties.Count > 0 ? collectionProperties : null;
        }

        /// <summary>
        /// Analyze projection member access
        /// </summary>
        private static List<(string Name, string Type)>? AnalyzeProjectionMemberAccess(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel)
        {
            // Get type information for the member access
            var typeInfo = semanticModel.GetTypeInfo(memberAccess);
            if (typeInfo.Type is not INamedTypeSymbol namedType)
                return null;

            // Extract element type from collection type
            var elementType = GetCollectionElementType(namedType);
            if (elementType == null)
                return null;

            // Check if it's an anonymous type
            if (!elementType.IsAnonymousType)
                return null;

            var properties = new List<(string Name, string Type)>();
            
            // Extract properties from the anonymous type
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
        /// Extract element type from collection type
        /// </summary>
        private static INamedTypeSymbol? GetCollectionElementType(INamedTypeSymbol collectionType)
        {
            // 배열 타입 처리 (new[] { ... } 형태)
            if (collectionType is IArrayTypeSymbol arrayType)
            {
                if (arrayType.ElementType is INamedTypeSymbol namedElementType)
                {
                    return namedElementType;
                }
            }

            // Extract T from IEnumerable<T>, List<T>, ICollection<T>
            if (collectionType.IsGenericType && collectionType.TypeArguments.Length == 1)
            {
                var elementType = collectionType.TypeArguments[0];
                if (elementType is INamedTypeSymbol namedElementType)
                {
                    return namedElementType;
                }
            }

            // Search for IEnumerable<T> in interfaces
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
        /// Analyze anonymous type - handle both direct creation and variable references
        /// </summary>
        private static List<(string Name, string Type)>? AnalyzeAnonymousType(ArgumentSyntax? argument, SemanticModel semanticModel)
        {
            if (argument?.Expression == null)
                return null;

            switch (argument.Expression)
            {
                case AnonymousObjectCreationExpressionSyntax anonymousObj:
                    // Direct anonymous object creation: new { Name = "", Age = 0 }
                    return AnalyzeDirectAnonymousObject(anonymousObj);

                case IdentifierNameSyntax identifier:
                    // Variable reference: anonymousInstance
                    return AnalyzeVariableReference(identifier, semanticModel);

                case MemberAccessExpressionSyntax memberAccess:
                    // Property/field reference: this.SomeAnonymousObject
                    return AnalyzeMemberAccess(memberAccess, semanticModel);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Analyze direct anonymous object creation
        /// </summary>
        private static List<(string Name, string Type)> AnalyzeDirectAnonymousObject(AnonymousObjectCreationExpressionSyntax anonymousObj)
        {
            var properties = new List<(string Name, string Type)>();
            
            foreach (var initializer in anonymousObj.Initializers)
            {
                if (initializer is AnonymousObjectMemberDeclaratorSyntax declarator)
                {
                    string name;
                    
                    // Determine property name
                    if (declarator.NameEquals != null)
                    {
                        // Explicit name: new { Name = "value" }
                        name = declarator.NameEquals.Name.Identifier.ValueText;
                    }
                    else if (declarator.Expression is IdentifierNameSyntax identifierExpr)
                    {
                        // Implicit name: new { variable }
                        name = identifierExpr.Identifier.ValueText;
                    }
                    else if (declarator.Expression is MemberAccessExpressionSyntax memberExpr)
                    {
                        // Member access: new { obj.Property }
                        name = memberExpr.Name.Identifier.ValueText;
                    }
                    else
                    {
                        continue; // Skip unhandled expressions
                    }

                    var type = InferTypeFromExpression(declarator.Expression);
                    properties.Add((name, type));
                }
            }
            
            return properties;
        }

        /// <summary>
        /// Analyze variable reference - use SemanticModel for accurate type information
        /// </summary>
        private static List<(string Name, string Type)>? AnalyzeVariableReference(IdentifierNameSyntax identifier, SemanticModel semanticModel)
        {
            // Get type information for the variable
            var typeInfo = semanticModel.GetTypeInfo(identifier);
            if (typeInfo.Type is not INamedTypeSymbol namedType)
                return null;

            // Check if it's an anonymous type
            if (!namedType.IsAnonymousType)
                return null;

            var properties = new List<(string Name, string Type)>();
            
            // Extract properties from the anonymous type
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
        /// Analyze member access (this.field, obj.property, etc.)
        /// </summary>
        private static List<(string Name, string Type)>? AnalyzeMemberAccess(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel)
        {
            // Get type information for the member access
            var typeInfo = semanticModel.GetTypeInfo(memberAccess);
            if (typeInfo.Type is not INamedTypeSymbol namedType)
                return null;

            // Check if it's an anonymous type
            if (!namedType.IsAnonymousType)
                return null;

            var properties = new List<(string Name, string Type)>();
            
            // Extract properties from the anonymous type
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
        /// Infer type from expression (basic implementation)
        /// </summary>
        private static string InferTypeFromExpression(ExpressionSyntax expression)
        {
            switch (expression)
            {
                case LiteralExpressionSyntax literal:
                    return InferTypeFromLiteral(literal);

                case IdentifierNameSyntax identifier:
                    // For variable references, default to object
                    return "object";

                case MemberAccessExpressionSyntax memberAccess:
                    // For member access, default to object
                    return "object";

                case InvocationExpressionSyntax invocation:
                    // For method calls, default to object
                    return "object";

                case ArrayCreationExpressionSyntax arrayCreation:
                    // Array creation
                    if (arrayCreation.Type.ElementType != null)
                    {
                        return $"{arrayCreation.Type.ElementType}[]";
                    }
                    return "object[]";

                case ObjectCreationExpressionSyntax objectCreation:
                    // Object creation
                    return objectCreation.Type?.ToString() ?? "object";

                default:
                    return "object";
            }
        }

        /// <summary>
        /// Infer type from literal
        /// </summary>
        private static string InferTypeFromLiteral(LiteralExpressionSyntax literal)
        {
            if (literal.Token.IsKind(SyntaxKind.StringLiteralToken))
                return "string";
                
            if (literal.Token.IsKind(SyntaxKind.NumericLiteralToken))
            {
                var text = literal.Token.ValueText;
                
                // decimal literal
                if (text.EndsWith("m", System.StringComparison.OrdinalIgnoreCase))
                    return "decimal";
                    
                // float literal
                if (text.EndsWith("f", System.StringComparison.OrdinalIgnoreCase))
                    return "float";
                    
                // long literal
                if (text.EndsWith("l", System.StringComparison.OrdinalIgnoreCase))
                    return "long";
                    
                // double literal (contains decimal point)
                if (text.Contains('.'))
                    return "double";
                
                // int literal
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

                var properties = CollectProperties(callInfo);

                var sourceCode = GenerateTypeSource(callInfo);
                context.AddSource($"{callInfo.TargetTypeName}.g.cs", SourceText.From(sourceCode, Encoding.UTF8));

                // ?? **컨버터 생성**: WithConverter()가 활성화된 경우
                if (callInfo.EnableConverter)
                {
                    var converterCode = GenerateConverterSource(callInfo);
                    context.AddSource($"{callInfo.TargetTypeName}Converter.g.cs", SourceText.From(converterCode, Encoding.UTF8));
                }
            }
        }

        private static string GenerateTypeSource(GenerateCallInfo callInfo)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("// <auto-generated />");
            sb.AppendLine("// This code was generated by Structura Source Generator");
            sb.AppendLine("// Supports anonymous types, EF Core projections, and Add functionality");
            sb.AppendLine("#nullable enable");
            sb.AppendLine();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine();
            sb.AppendLine($"namespace {callInfo.TargetNamespace}");
            sb.AppendLine("{");

            var properties = CollectProperties(callInfo);

            if (callInfo.GenerationMode == TypeGenerationMode.Record)
            {
                // Generate record type
                sb.AppendLine($"    /// <summary>");
                sb.AppendLine($"    /// Generated record type: {callInfo.TargetTypeName}");
                sb.AppendLine($"    /// Properties from anonymous types, EF Core projections, and Add operations");
                sb.AppendLine($"    /// Namespace: {callInfo.TargetNamespace}");
                if (callInfo.EnableConverter)
                {
                    sb.AppendLine($"    /// Static converter methods: {callInfo.TargetTypeName}.FromCollection(), {callInfo.TargetTypeName}.FromSingle()");
                }
                sb.AppendLine($"    /// </summary>");
                sb.AppendLine($"    public partial record {callInfo.TargetTypeName}(");
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
                // Generate class or struct type
                var typeKeyword = callInfo.GenerationMode == TypeGenerationMode.Class ? "class" : "struct";
                sb.AppendLine($"    /// <summary>");
                sb.AppendLine($"    /// Generated {typeKeyword} type: {callInfo.TargetTypeName}");
                sb.AppendLine($"    /// Properties from anonymous types, EF Core projections, and Add operations");
                sb.AppendLine($"    /// Namespace: {callInfo.TargetNamespace}");
                if (callInfo.EnableConverter)
                {
                    sb.AppendLine($"    /// Static converter methods: {callInfo.TargetTypeName}.FromCollection(), {callInfo.TargetTypeName}.FromSingle()");
                }
                sb.AppendLine($"    /// </summary>");
                sb.AppendLine($"    public partial {typeKeyword} {callInfo.TargetTypeName}");
                sb.AppendLine("    {");

                foreach (var (name, type) in properties)
                {
                    sb.AppendLine($"        /// <summary>Property: {name} (Type: {type})</summary>");
                    
                    // Remove required keyword to avoid CS9035 compilation errors
                    sb.AppendLine($"        public {type} {name} {{ get; set; }}");
                    sb.AppendLine();
                }

                sb.AppendLine("    }");
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        /// <summary>
        /// ?? **Generate Static Converter Methods** - Creates static methods on the generated type itself
        /// </summary>
        private static string GenerateConverterSource(GenerateCallInfo callInfo)
        {
            var sb = new StringBuilder();
            var properties = CollectProperties(callInfo);
            var typeName = callInfo.TargetTypeName;
            
            sb.AppendLine("// <auto-generated />");
            sb.AppendLine("// This code was generated by Structura Source Generator - Static Converter Methods");
            sb.AppendLine("// Enables seamless conversion from anonymous objects to strongly-typed instances");
            sb.AppendLine("#nullable enable");
            sb.AppendLine();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine();
            sb.AppendLine($"namespace {callInfo.TargetNamespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// ?? **Static Converter Methods** for {typeName}");
            sb.AppendLine($"    /// Automatically generated when .WithConverter() is used");
            sb.AppendLine($"    /// Namespace: {callInfo.TargetNamespace}");
            sb.AppendLine($"    /// </summary>");
            sb.AppendLine($"    public partial {GetTypeKeyword(callInfo.GenerationMode)} {typeName}");
            sb.AppendLine("    {");

            // Generate collection converter for anonymous objects
            sb.AppendLine($"        /// <summary>");
            sb.AppendLine($"        /// ?? **Converts** anonymous object collection to {typeName} list");
            sb.AppendLine($"        /// </summary>");
            sb.AppendLine($"        public static List<{typeName}> FromCollection(IEnumerable<object> source)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (source == null)");
            sb.AppendLine($"                throw new ArgumentNullException(nameof(source));");
            sb.AppendLine();
            sb.AppendLine($"            return source.Select(FromSingle).ToList();");
            sb.AppendLine("        }");
            sb.AppendLine();

            // Generate single object converter for anonymous objects
            sb.AppendLine($"        /// <summary>");
            sb.AppendLine($"        /// ?? **Converts** single anonymous object to {typeName}");
            sb.AppendLine($"        /// </summary>");
            sb.AppendLine($"        public static {typeName} FromSingle(object source)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (source == null)");
            sb.AppendLine($"                throw new ArgumentNullException(nameof(source));");
            sb.AppendLine();
            sb.AppendLine("            dynamic dynamicSource = source;");
            sb.AppendLine();

            // Generate property extraction and mapping code based on generation mode
            if (callInfo.GenerationMode == TypeGenerationMode.Record)
            {
                // Record constructor parameters
                sb.AppendLine($"            return new {typeName}(");
                for (int i = 0; i < properties.Count; i++)
                {
                    var property = properties[i];
                    var name = property.Name;
                    var type = property.Type;
                    sb.Append($"                GetPropertyValueOrDefault(dynamicSource, \"{name}\", typeof({type})) is {type} {name.ToLowerInvariant()}Value ? {name.ToLowerInvariant()}Value : default({type})!");
                    if (i < properties.Count - 1)
                        sb.AppendLine(",");
                    else
                        sb.AppendLine();
                }
                sb.AppendLine("            );");
            }
            else
            {
                // Class/Struct property assignment - using a simple approach
                sb.AppendLine($"            var target = new {typeName}();");
                sb.AppendLine();
                
                foreach (var property in properties)
                {
                    var name = property.Name;
                    var type = property.Type;
                    sb.AppendLine($"            try");
                    sb.AppendLine("            {");
                    sb.AppendLine($"                var {name.ToLowerInvariant()}Value = GetPropertyValueOrDefault(dynamicSource, \"{name}\", typeof({type}));");
                    sb.AppendLine($"                if ({name.ToLowerInvariant()}Value is {type} converted{name})");
                    sb.AppendLine($"                    target.{name} = converted{name};");
                    sb.AppendLine($"                else");
                    sb.AppendLine($"                    target.{name} = default({type})!;");
                    sb.AppendLine("            }");
                    sb.AppendLine($"            catch");
                    sb.AppendLine("            {");
                    sb.AppendLine($"                target.{name} = default({type})!;");
                    sb.AppendLine("            }");
                    sb.AppendLine();
                }
                
                sb.AppendLine("            return target;");
            }

            sb.AppendLine("        }");
            sb.AppendLine();

            // Generate smart value converter helper method
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// ?? **Smart Value Converter** - Handles intelligent type conversion");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        private static object? ConvertValue(object? sourceValue, Type targetType)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (sourceValue == null)");
            sb.AppendLine("            {");
            sb.AppendLine("                var underlyingType = Nullable.GetUnderlyingType(targetType);");
            sb.AppendLine("                if (underlyingType != null)");
            sb.AppendLine("                    return null;");
            sb.AppendLine("                ");
            sb.AppendLine("                if (targetType.IsValueType)");
            sb.AppendLine("                {");
            sb.AppendLine("                    if (targetType == typeof(int)) return 0;");
            sb.AppendLine("                    if (targetType == typeof(long)) return 0L;");
            sb.AppendLine("                    if (targetType == typeof(float)) return 0f;");
            sb.AppendLine("                    if (targetType == typeof(double)) return 0.0;");
            sb.AppendLine("                    if (targetType == typeof(decimal)) return 0m;");
            sb.AppendLine("                    if (targetType == typeof(bool)) return false;");
            sb.AppendLine("                    if (targetType == typeof(DateTime)) return DateTime.MinValue;");
            sb.AppendLine("                    if (targetType == typeof(Guid)) return Guid.Empty;");
            sb.AppendLine("                }");
            sb.AppendLine("                ");
            sb.AppendLine("                return null;");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            var sourceType = sourceValue.GetType();");
            sb.AppendLine("            ");
            sb.AppendLine("            if (targetType.IsAssignableFrom(sourceType))");
            sb.AppendLine("                return sourceValue;");
            sb.AppendLine();
            sb.AppendLine("            var underlyingTargetType = Nullable.GetUnderlyingType(targetType) ?? targetType;");
            sb.AppendLine("            ");
            sb.AppendLine("            if (underlyingTargetType == typeof(string))");
            sb.AppendLine("                return sourceValue.ToString();");
            sb.AppendLine("            ");
            sb.AppendLine("            if (underlyingTargetType == typeof(int))");
            sb.AppendLine("                return Convert.ToInt32(sourceValue);");
            sb.AppendLine("            ");
            sb.AppendLine("            if (underlyingTargetType == typeof(long))");
            sb.AppendLine("                return Convert.ToInt64(sourceValue);");
            sb.AppendLine("            ");
            sb.AppendLine("            if (underlyingTargetType == typeof(float))");
            sb.AppendLine("                return Convert.ToSingle(sourceValue);");
            sb.AppendLine("            ");
            sb.AppendLine("            if (underlyingTargetType == typeof(double))");
            sb.AppendLine("                return Convert.ToDouble(sourceValue);");
            sb.AppendLine("            ");
            sb.AppendLine("            if (underlyingTargetType == typeof(decimal))");
            sb.AppendLine("                return Convert.ToDecimal(sourceValue);");
            sb.AppendLine("            ");
            sb.AppendLine("            if (underlyingTargetType == typeof(bool))");
            sb.AppendLine("                return Convert.ToBoolean(sourceValue);");
            sb.AppendLine("            ");
            sb.AppendLine("            if (underlyingTargetType == typeof(DateTime))");
            sb.AppendLine("                return Convert.ToDateTime(sourceValue);");
            sb.AppendLine("            ");
            sb.AppendLine("            if (underlyingTargetType == typeof(Guid))");
            sb.AppendLine("            {");
            sb.AppendLine("                if (sourceValue is string guidString)");
            sb.AppendLine("                    return Guid.Parse(guidString);");
            sb.AppendLine("                return (Guid)sourceValue;");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            if (underlyingTargetType.IsEnum)");
            sb.AppendLine("            {");
            sb.AppendLine("                if (sourceValue is string stringValue)");
            sb.AppendLine("                    return Enum.Parse(underlyingTargetType, stringValue, true);");
            sb.AppendLine("                return Enum.ToObject(underlyingTargetType, sourceValue);");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine("                return Convert.ChangeType(sourceValue, underlyingTargetType);");
            sb.AppendLine("            }");
            sb.AppendLine("            catch");
            sb.AppendLine("            {");
            sb.AppendLine("                return sourceValue;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();

            // Generate GetPropertyValueOrDefault helper method
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// ?? **Get Property Value or Default** - Safely gets property value or returns default");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        private static object? GetPropertyValueOrDefault(dynamic source, string propertyName, Type targetType)");
            sb.AppendLine("        {");
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine("                var sourceType = source.GetType();");
            sb.AppendLine("                var property = sourceType.GetProperty(propertyName);");
            sb.AppendLine("                if (property != null)");
            sb.AppendLine("                {");
            sb.AppendLine("                    var value = property.GetValue(source);");
            sb.AppendLine("                    return ConvertValue(value, targetType);");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine("            catch");
            sb.AppendLine("            {");
            sb.AppendLine("                // Property doesn't exist, return default");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            // Return default value for the target type");
            sb.AppendLine("            return ConvertValue(null, targetType);");
            sb.AppendLine("        }");
            sb.AppendLine();

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        /// <summary>
        /// Get type keyword for the generation mode
        /// </summary>
        private static string GetTypeKeyword(TypeGenerationMode mode)
        {
            return mode switch
            {
                TypeGenerationMode.Record => "record",
                TypeGenerationMode.Class => "class",
                TypeGenerationMode.Struct => "struct",
                _ => "class"
            };
        }

        private static List<(string Name, string Type)> CollectProperties(GenerateCallInfo callInfo)
        {
            var properties = new List<(string Name, string Type)>();
            var excludedProperties = new HashSet<string>();
            var typeChanges = new Dictionary<string, string>();
            
            // First pass: collect excluded properties and type changes
            foreach (var operation in callInfo.Operations)
            {
                switch (operation.OperationType)
                {
                    case FluentOperationType.Exclude:
                        if (operation.ExcludedPropertyName != null)
                        {
                            excludedProperties.Add(operation.ExcludedPropertyName);
                        }
                        break;
                        
                    case FluentOperationType.ChangeType:
                        if (operation.ChangeTypePropertyName != null && operation.NewType != null)
                        {
                            typeChanges[operation.ChangeTypePropertyName] = operation.NewType;
                        }
                        break;
                }
            }
            
            // Second pass: collect properties and apply filters/changes
            foreach (var operation in callInfo.Operations)
            {
                switch (operation.OperationType)
                {
                    case FluentOperationType.Add:
                        // ?? Add new properties (this is the core Add functionality)
                        if (operation.PropertyName != null && operation.PropertyType != null)
                        {
                            // Skip if excluded
                            if (excludedProperties.Contains(operation.PropertyName))
                                continue;
                                
                            // Apply type change if specified
                            var finalType = typeChanges.ContainsKey(operation.PropertyName) 
                                ? typeChanges[operation.PropertyName] 
                                : operation.PropertyType;
                            
                            // Check if property already exists (avoid duplicates)
                            var existingPropertyIndex = properties.FindIndex(p => p.Name == operation.PropertyName);
                            if (existingPropertyIndex >= 0)
                            {
                                // Replace existing property with new type
                                properties[existingPropertyIndex] = (operation.PropertyName, finalType);
                            }
                            else
                            {
                                // Add new property
                                properties.Add((operation.PropertyName, finalType));
                            }
                        }
                        break;

                    case FluentOperationType.With:
                        // Add properties from anonymous types
                        if (operation.AnonymousTypeInfo != null)
                        {
                            foreach (var anonymousProperty in operation.AnonymousTypeInfo)
                            {
                                // Skip if excluded
                                if (excludedProperties.Contains(anonymousProperty.Name))
                                    continue;
                                    
                                // Apply type change if specified
                                var finalType = typeChanges.ContainsKey(anonymousProperty.Name) 
                                    ? typeChanges[anonymousProperty.Name] 
                                    : anonymousProperty.Type;
                                
                                // Avoid duplicates
                                if (!properties.Any(p => p.Name == anonymousProperty.Name))
                                {
                                    properties.Add((anonymousProperty.Name, finalType));
                                }
                            }
                        }
                        break;

                    case FluentOperationType.WithProjection:
                        // Add properties from EF Core projections
                        if (operation.ProjectionInfo != null)
                        {
                            foreach (var projectionProperty in operation.ProjectionInfo)
                            {
                                // Skip if excluded
                                if (excludedProperties.Contains(projectionProperty.Name))
                                    continue;
                                    
                                // Apply type change if specified
                                var finalType = typeChanges.ContainsKey(projectionProperty.Name) 
                                    ? typeChanges[projectionProperty.Name] 
                                    : projectionProperty.Type;
                                
                                // Avoid duplicates
                                if (!properties.Any(p => p.Name == projectionProperty.Name))
                                {
                                    properties.Add((projectionProperty.Name, finalType));
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
            // Convert to C# keywords
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

    // Helper classes
    public class GenerateCallInfo
    {
        public string TargetTypeName { get; set; } = "";
        public string TargetNamespace { get; set; } = "Generated";
        public List<FluentOperation> Operations { get; set; } = new List<FluentOperation>();
        public TypeGenerationMode GenerationMode { get; set; } = TypeGenerationMode.Record;
        public bool EnableConverter { get; set; } = false;
    }

    public class FluentChainInfo
    {
        public string TypeName { get; set; } = "";
        public string Namespace { get; set; } = "Generated";
        public List<FluentOperation> Operations { get; set; } = new List<FluentOperation>();
        public TypeGenerationMode GenerationMode { get; set; } = TypeGenerationMode.Record;
        public bool EnableConverter { get; set; } = false;
    }

    public class FluentOperation
    {
        public FluentOperationType OperationType { get; set; }
        public string? PropertyName { get; set; }
        public string? PropertyType { get; set; }
        public string? StringValue { get; set; }
        public string? NamespaceValue { get; set; }
        public string? DefaultValue { get; set; }
        public List<(string Name, string Type)>? AnonymousTypeInfo { get; set; }
        public List<(string Name, string Type)>? ProjectionInfo { get; set; }
        public string? ExcludedPropertyName { get; set; }
        public string? ChangeTypePropertyName { get; set; }
        public string? NewType { get; set; }
    }

    public enum FluentOperationType
    {
        WithName,
        Add,
        With,
        WithProjection,
        WithConverter,
        AsRecord,
        AsClass,
        AsStruct,
        Exclude,
        ChangeType
    }
}