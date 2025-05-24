using System;
using System.Collections.Generic;
using System.Text;
using ET.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ET.Analyzer.PlatformLog
{
    [Generator(LanguageNames.CSharp)]
    public class PlatformLogGenerator : ISourceGenerator
    {
        public static string PlatformLogInterfaceName = "ET.IPlatformLog";
        
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(()=> new PlatformLogSyntaxContextReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not PlatformLogSyntaxContextReceiver receiver || receiver.ClassDeclaration.Count == 0)
            {
                return;
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("namespace ET;");
            stringBuilder.AppendLine("using System.Collections.Generic;");
            stringBuilder.AppendLine("using System;");

            int count = 0;
            
            foreach (var classDeclaration in receiver.ClassDeclaration)
            {
                GenerateCSFiles(classDeclaration,context,stringBuilder,ref count);
            }

            if (count == 0)
                return;

            var fileName = "PlatformLog.g.cs";
            context.AddSource(fileName,stringBuilder.ToString());
        }
        
        /// <summary>
        /// 每个静态类生成一个cs文件
        /// </summary>
        private void GenerateCSFiles(StructDeclarationSyntax classDeclarationSyntax, GeneratorExecutionContext context,StringBuilder stringBuilder,ref int count)
        {
            SemanticModel semanticModel = context.Compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
            INamedTypeSymbol? classTypeSymbol = ModelExtensions.GetDeclaredSymbol(semanticModel, classDeclarationSyntax) as INamedTypeSymbol;
            if (classTypeSymbol == null)
            {
                return;
            }
            
            if (!classTypeSymbol.HasInterface(PlatformLogInterfaceName))
            {
                return;
            }

            count++;
            
            var className = classTypeSymbol.Name;

            stringBuilder.AppendLine($"public partial struct {className}");
            stringBuilder.AppendLine("{");
            
            stringBuilder.AppendLine($"\tpublic void WriteLog(Dictionary<string,object> props)");
            stringBuilder.AppendLine("\t{");

            foreach (var member in classTypeSymbol.GetMembers())
            {
                
                if(member.IsStatic)
                    continue;
                
                if(member is not IPropertySymbol propertySymbol)
                    continue;

                stringBuilder.AppendLine($"\t\tprops.Add(\"{propertySymbol.Name}\", {propertySymbol.Name});");
            }

            stringBuilder.AppendLine("\t}");
            
            stringBuilder.AppendLine("}");


        }
    }
    
    class PlatformLogSyntaxContextReceiver: ISyntaxContextReceiver
    {
        public HashSet<StructDeclarationSyntax> ClassDeclaration { get; } = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            SyntaxNode node = context.Node;
            if (node is not StructDeclarationSyntax classDeclarationSyntax)
            {
                return;
            }

            ClassDeclaration.Add(classDeclarationSyntax);
        }
    }
}