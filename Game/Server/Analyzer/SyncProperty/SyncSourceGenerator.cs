#nullable enable
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace SyncCodeGen
{
	[Generator]
	public class SyncSourceGenerator : ISourceGenerator
	{
		public void Initialize(GeneratorInitializationContext context)
		{
			context.RegisterForSyntaxNotifications(() => new SyncSyntaxReceiver());
		}

		public void Execute(GeneratorExecutionContext context)
		{
			if (!(context.SyntaxContextReceiver is SyncSyntaxReceiver receiver))
				return;

			// 调试信息：总是生成一个诊断文件
			var debugInfo = $"// 生成器调试信息 - 版本 2.0\n// 找到的同步类数量: {receiver.SyncClasses.Count}\n";
			foreach (var classInfo in receiver.SyncClasses)
			{
				debugInfo += $"// 类: {classInfo.Namespace}.{classInfo.ClassName}, 字段: {classInfo.SyncFields.Count}, 属性: {classInfo.SyncProperties.Count}\n";
				foreach (var field in classInfo.SyncFields)
				{
					debugInfo += $"//   字段: {field.Name} ({field.Type})\n";
				}
				foreach (var prop in classInfo.SyncProperties)
				{
					debugInfo += $"//   属性: {prop.Name} ({prop.Type})\n";
				}
			}
			context.AddSource("SyncDebug.g.cs", SourceText.From(debugInfo, Encoding.UTF8));

			// 总是生成元数据，即使没有同步类
			var metadataSource = SyncMetadataGenerator.GenerateMetadata(receiver.SyncClasses);
			context.AddSource("SyncMetadata.g.cs", SourceText.From(metadataSource, Encoding.UTF8));

			// 生成类实现
			foreach (var classInfo in receiver.SyncClasses)
			{
				var classSource = SyncClassGenerator.GenerateSyncClass(classInfo);
				// 使用命名空间和类名生成唯一的文件名，避免冲突
				var fileName = $"{classInfo.Namespace.Replace(".", "_")}_{classInfo.ClassName}Sync.g.cs";
				context.AddSource(fileName, SourceText.From(classSource, Encoding.UTF8));
			}
		}
	}
} 