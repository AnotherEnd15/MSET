using Microsoft.CodeAnalysis;using Microsoft.CodeAnalysis.CSharp.Syntax;using System.Collections.Generic;using System.Linq;

namespace SyncCodeGen
{
	public class SyncSyntaxReceiver : ISyntaxContextReceiver
	{
		public List<SyncClassInfo> SyncClasses { get; } = new List<SyncClassInfo>();

		public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
		{
			if (context.Node is ClassDeclarationSyntax classDeclaration)
			{
				var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
				if (classSymbol == null) return;

				// 检查是否有SyncClass属性
				bool hasSyncClass = classSymbol.GetAttributes()
					.Any(a => a.AttributeClass?.Name == "SyncClassAttribute" || a.AttributeClass?.Name == "SyncClass");

				if (!hasSyncClass) return;

				var classInfo = new SyncClassInfo
				{
					ClassName = classSymbol.Name,
					Namespace = classSymbol.ContainingNamespace.ToDisplayString()
				};

				// 收集同步字段
				foreach (var member in classSymbol.GetMembers())
				{
					if (member is IFieldSymbol field)
					{
						bool hasSyncField = field.GetAttributes()
							.Any(a => a.AttributeClass?.Name == "SyncFieldAttribute" || a.AttributeClass?.Name == "SyncField");

						if (hasSyncField)
						{
							classInfo.SyncFields.Add(new SyncFieldInfo
							{
								Name = field.Name,
								Type = field.Type.ToDisplayString()
							});
						}
					}
					else if (member is IPropertySymbol property)
					{
						bool hasSyncProperty = property.GetAttributes()
							.Any(a => a.AttributeClass?.Name == "SyncPropertyAttribute" || a.AttributeClass?.Name == "SyncProperty");

						if (hasSyncProperty)
						{
							classInfo.SyncProperties.Add(new SyncPropertyInfo
							{
								Name = property.Name,
								Type = property.Type.ToDisplayString()
							});
						}
					}
				}

				if (classInfo.SyncFields.Count > 0 || classInfo.SyncProperties.Count > 0)
				{
					SyncClasses.Add(classInfo);
				}
			}
		}
	}
} 