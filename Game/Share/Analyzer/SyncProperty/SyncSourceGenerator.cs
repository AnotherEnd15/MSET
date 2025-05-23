// SyncSourceGenerator.cs
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
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

            // 生成元数据
            var metadataSource = GenerateMetadata(receiver);
            context.AddSource("SyncMetadata.g.cs", SourceText.From(metadataSource, Encoding.UTF8));

            // 生成类实现
            foreach (var classInfo in receiver.SyncClasses)
            {
                var classSource = GenerateSyncClass(classInfo);
                context.AddSource($"{classInfo.ClassName}Sync.g.cs", SourceText.From(classSource, Encoding.UTF8));
            }
        }

        private string GenerateMetadata(SyncSyntaxReceiver receiver)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendLine("namespace SyncFramework {");
            sb.AppendLine("    public static class SyncMetadata {");

            // 类ID与名称映射
            sb.AppendLine("        // 类ID映射");
            sb.AppendLine("        public static readonly Dictionary<int, string> ClassNames = new Dictionary<int, string> {");
            int nextClassId = 1;
            foreach (var classInfo in receiver.SyncClasses)
            {
                sb.AppendLine($"            {{ {nextClassId}, \"{classInfo.Namespace}.{classInfo.ClassName}\" }},");
                nextClassId++;
            }
            sb.AppendLine("        };");
            sb.AppendLine();

            // 字段ID常量
            sb.AppendLine("        // 字段ID常量");
            sb.AppendLine("        public static class Ids {");
            nextClassId = 1;
            foreach (var classInfo in receiver.SyncClasses)
            {
                sb.AppendLine($"            // {classInfo.ClassName}的ID和字段ID");
                sb.AppendLine($"            public const int {classInfo.ClassName}_ClassId = {nextClassId++};");

                int nextFieldId = 1;
                foreach (var field in classInfo.SyncFields)
                {
                    sb.AppendLine($"            public const int {classInfo.ClassName}_{field.Name}_Id = {nextFieldId++};");
                }

                foreach (var prop in classInfo.SyncProperties)
                {
                    sb.AppendLine($"            public const int {classInfo.ClassName}_{prop.Name}_Id = {nextFieldId++};");
                }
                sb.AppendLine();
            }
            sb.AppendLine("        }");
            sb.AppendLine();

            // 字段元数据
            sb.AppendLine("        // 字段元数据");
            sb.AppendLine("        public static readonly Dictionary<int, FieldMetadata> Fields = new Dictionary<int, FieldMetadata> {");
            nextClassId = 1;
            foreach (var classInfo in receiver.SyncClasses)
            {
                int nextFieldId = 1;
                foreach (var field in classInfo.SyncFields)
                {
                    string memberName = GetPropertyNameFromField(field.Name);
                    string typeString = GetTypeString(field.Type);
                    sb.AppendLine($"            {{ Ids.{classInfo.ClassName}_{field.Name}_Id, new FieldMetadata(Ids.{classInfo.ClassName}_ClassId, \"{memberName}\", typeof({typeString})) }},");
                    nextFieldId++;
                }

                foreach (var prop in classInfo.SyncProperties)
                {
                    string typeString = GetTypeString(prop.Type);
                    sb.AppendLine($"            {{ Ids.{classInfo.ClassName}_{prop.Name}_Id, new FieldMetadata(Ids.{classInfo.ClassName}_ClassId, \"{prop.Name}\", typeof({typeString})) }},");
                    nextFieldId++;
                }
                nextClassId++;
            }
            sb.AppendLine("        };");

            // 元数据类
            sb.AppendLine();
            sb.AppendLine("        public class FieldMetadata {");
            sb.AppendLine("            public int ClassId { get; }");
            sb.AppendLine("            public string FieldName { get; }");
            sb.AppendLine("            public Type FieldType { get; }");
            sb.AppendLine();
            sb.AppendLine("            public FieldMetadata(int classId, string fieldName, Type fieldType) {");
            sb.AppendLine("                ClassId = classId;");
            sb.AppendLine("                FieldName = fieldName;");
            sb.AppendLine("                FieldType = fieldType;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GenerateSyncClass(SyncClassInfo classInfo)
        {
            var sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.IO;");
            sb.AppendLine();
            sb.AppendLine($"namespace {classInfo.Namespace} {{");
            sb.AppendLine($"    public partial class {classInfo.ClassName} : SyncFramework.ISyncable {{");

            // 脏状态跟踪
            sb.AppendLine("        private HashSet<int> _dirtyState = new HashSet<int>();");
            sb.AppendLine("        public bool IsDirty => _dirtyState.Count > 0;");
            sb.AppendLine("        public int DirtyCount => _dirtyState.Count;");

            // 字段的属性
            foreach (var field in classInfo.SyncFields)
            {
                string propertyName = GetPropertyNameFromField(field.Name);

                sb.AppendLine();
                sb.AppendLine($"        public {field.Type} {propertyName} {{");
                sb.AppendLine($"            get => {field.Name};");
                sb.AppendLine($"            set {{");
                sb.AppendLine($"                if(!EqualityComparer<{field.Type}>.Default.Equals({field.Name}, value)) {{");
                sb.AppendLine($"                    {field.Name} = value;");
                sb.AppendLine($"                    _dirtyState.Add(SyncFramework.SyncMetadata.Ids.{classInfo.ClassName}_{field.Name}_Id);");
                sb.AppendLine("                }");
                sb.AppendLine("            }");
                sb.AppendLine("        }");
            }

            // partial property实现
            foreach (var prop in classInfo.SyncProperties)
            {
                sb.AppendLine();
                sb.AppendLine($"        partial {prop.Type} {prop.Name} {{");
                sb.AppendLine($"            get;");
                sb.AppendLine($"            set {{");
                sb.AppendLine($"                if(!EqualityComparer<{prop.Type}>.Default.Equals(this.{prop.Name}, value)) {{");
                sb.AppendLine($"                    _dirtyState.Add(SyncFramework.SyncMetadata.Ids.{classInfo.ClassName}_{prop.Name}_Id);");
                sb.AppendLine("                }");
                sb.AppendLine("            }");
                sb.AppendLine("        }");
            }

            // 直接序列化脏数据（避免装箱）
            sb.AppendLine();
            sb.AppendLine("        public void SerializeDirtyValues(BinaryWriter writer) {");
            sb.AppendLine("            // 写入脏字段数量");
            sb.AppendLine("            writer.Write(_dirtyState.Count);");
            sb.AppendLine();
            sb.AppendLine("            // 写入每个脏字段");
            sb.AppendLine("            foreach (int fieldId in _dirtyState) {");
            sb.AppendLine("                writer.Write(fieldId);");
            sb.AppendLine("                switch (fieldId) {");

            foreach (var field in classInfo.SyncFields)
            {
                sb.AppendLine($"                    case SyncFramework.SyncMetadata.Ids.{classInfo.ClassName}_{field.Name}_Id:");
                // 直接写入值，无需装箱
                GenerateSerializeField(sb, field.Name, field.Type);
                sb.AppendLine("                        break;");
            }

            foreach (var prop in classInfo.SyncProperties)
            {
                sb.AppendLine($"                    case SyncFramework.SyncMetadata.Ids.{classInfo.ClassName}_{prop.Name}_Id:");
                // 直接写入值，无需装箱
                GenerateSerializeProperty(sb, prop.Name, prop.Type);
                sb.AppendLine("                        break;");
            }

            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine("        }");

            // 反序列化方法
            sb.AppendLine();
            sb.AppendLine("        public void DeserializeValues(BinaryReader reader) {");
            sb.AppendLine("            int fieldCount = reader.ReadInt32();");
            sb.AppendLine();
            sb.AppendLine("            for (int i = 0; i < fieldCount; i++) {");
            sb.AppendLine("                int fieldId = reader.ReadInt32();");
            sb.AppendLine("                switch (fieldId) {");

            foreach (var field in classInfo.SyncFields)
            {
                sb.AppendLine($"                    case SyncFramework.SyncMetadata.Ids.{classInfo.ClassName}_{field.Name}_Id:");
                // 直接读取值
                GenerateDeserializeField(sb, field.Name, field.Type);
                sb.AppendLine("                        break;");
            }

            foreach (var prop in classInfo.SyncProperties)
            {
                sb.AppendLine($"                    case SyncFramework.SyncMetadata.Ids.{classInfo.ClassName}_{prop.Name}_Id:");
                // 直接读取值
                GenerateDeserializeProperty(sb, prop.Name, prop.Type);
                sb.AppendLine("                        break;");
            }

            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine("        }");

            // 清除脏状态
            sb.AppendLine();
            sb.AppendLine("        public void ClearDirtyState() {");
            sb.AppendLine("            _dirtyState.Clear();");
            sb.AppendLine("        }");

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        // 为不同类型生成序列化代码（避免装箱）
        private void GenerateSerializeField(StringBuilder sb, string fieldName, string typeName)
        {
            switch (GetTypeCategory(typeName))
            {
                case TypeCategory.Int:
                    sb.AppendLine($"                        writer.Write({fieldName});");
                    break;
                case TypeCategory.Float:
                    sb.AppendLine($"                        writer.Write({fieldName});");
                    break;
                case TypeCategory.Double:
                    sb.AppendLine($"                        writer.Write({fieldName});");
                    break;
                case TypeCategory.Bool:
                    sb.AppendLine($"                        writer.Write({fieldName});");
                    break;
                case TypeCategory.String:
                    sb.AppendLine($"                        writer.Write({fieldName} ?? string.Empty);");
                    break;
                default:
                    sb.AppendLine($"                        SyncFramework.TypeSerializers.Serialize(writer, {fieldName});");
                    break;
            }
        }

        private void GenerateSerializeProperty(StringBuilder sb, string propertyName, string typeName)
        {
            switch (GetTypeCategory(typeName))
            {
                case TypeCategory.Int:
                    sb.AppendLine($"                        writer.Write(this.{propertyName});");
                    break;
                case TypeCategory.Float:
                    sb.AppendLine($"                        writer.Write(this.{propertyName});");
                    break;
                case TypeCategory.Double:
                    sb.AppendLine($"                        writer.Write(this.{propertyName});");
                    break;
                case TypeCategory.Bool:
                    sb.AppendLine($"                        writer.Write(this.{propertyName});");
                    break;
                case TypeCategory.String:
                    sb.AppendLine($"                        writer.Write(this.{propertyName} ?? string.Empty);");
                    break;
                default:
                    sb.AppendLine($"                        SyncFramework.TypeSerializers.Serialize(writer, this.{propertyName});");
                    break;
            }
        }

        private void GenerateDeserializeField(StringBuilder sb, string fieldName, string typeName)
        {
            switch (GetTypeCategory(typeName))
            {
                case TypeCategory.Int:
                    sb.AppendLine($"                        {fieldName} = reader.ReadInt32();");
                    break;
                case TypeCategory.Float:
                    sb.AppendLine($"                        {fieldName} = reader.ReadSingle();");
                    break;
                case TypeCategory.Double:
                    sb.AppendLine($"                        {fieldName} = reader.ReadDouble();");
                    break;
                case TypeCategory.Bool:
                    sb.AppendLine($"                        {fieldName} = reader.ReadBoolean();");
                    break;
                case TypeCategory.String:
                    sb.AppendLine($"                        {fieldName} = reader.ReadString();");
                    break;
                default:
                    sb.AppendLine($"                        {fieldName} = SyncFramework.TypeSerializers.Deserialize<{typeName}>(reader);");
                    break;
            }
        }

        private void GenerateDeserializeProperty(StringBuilder sb, string propertyName, string typeName)
        {
            switch (GetTypeCategory(typeName))
            {
                case TypeCategory.Int:
                    sb.AppendLine($"                        this.{propertyName} = reader.ReadInt32();");
                    break;
                case TypeCategory.Float:
                    sb.AppendLine($"                        this.{propertyName} = reader.ReadSingle();");
                    break;
                case TypeCategory.Double:
                    sb.AppendLine($"                        this.{propertyName} = reader.ReadDouble();");
                    break;
                case TypeCategory.Bool:
                    sb.AppendLine($"                        this.{propertyName} = reader.ReadBoolean();");
                    break;
                case TypeCategory.String:
                    sb.AppendLine($"                        this.{propertyName} = reader.ReadString();");
                    break;
                default:
                    sb.AppendLine($"                        this.{propertyName} = SyncFramework.TypeSerializers.Deserialize<{typeName}>(reader);");
                    break;
            }
        }

        private string GetPropertyNameFromField(string fieldName)
        {
            if (fieldName.StartsWith("_") && fieldName.Length > 1)
                return char.ToUpper(fieldName[1]) + fieldName.Substring(2);
            else
                return char.ToUpper(fieldName[0]) + fieldName.Substring(1);
        }

        private string GetTypeString(string type)
        {
            // 简化一些常用类型表示，避免使用全限定名称
            if (type == "System.Int32") return "int";
            if (type == "System.Single") return "float";
            if (type == "System.Double") return "double";
            if (type == "System.Boolean") return "bool";
            if (type == "System.String") return "string";
            return type;
        }

        private enum TypeCategory
        {
            Int,
            Float,
            Double,
            Bool,
            String,
            Complex
        }

        private TypeCategory GetTypeCategory(string typeName)
        {
            if (typeName == "int" || typeName == "System.Int32") return TypeCategory.Int;
            if (typeName == "float" || typeName == "System.Single") return TypeCategory.Float;
            if (typeName == "double" || typeName == "System.Double") return TypeCategory.Double;
            if (typeName == "bool" || typeName == "System.Boolean") return TypeCategory.Bool;
            if (typeName == "string" || typeName == "System.String") return TypeCategory.String;
            return TypeCategory.Complex;
        }
    }

    // 语法接收器保持不变
    public class SyncSyntaxReceiver : ISyntaxContextReceiver
    {
        public List<SyncClassInfo> SyncClasses { get; } = new List<SyncClassInfo>();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is ClassDeclarationSyntax classDecl)
            {
                var semanticModel = context.SemanticModel;
                var classSymbol = semanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;

                if (classSymbol != null && classSymbol.GetAttributes().Any(attr =>
                    attr.AttributeClass.ToDisplayString() == "SyncFramework.SyncClassAttribute"))
                {
                    var classInfo = new SyncClassInfo
                    {
                        ClassName = classSymbol.Name,
                        Namespace = classSymbol.ContainingNamespace.ToDisplayString()
                    };

                    // 查找标记的字段和属性
                    foreach (var member in classSymbol.GetMembers())
                    {
                        if (member is IFieldSymbol field && field.GetAttributes().Any(attr =>
                            attr.AttributeClass.ToDisplayString() == "SyncFramework.SyncFieldAttribute"))
                        {
                            classInfo.SyncFields.Add(new SyncMemberInfo
                            {
                                Name = field.Name,
                                Type = field.Type.ToDisplayString()
                            });
                        }
                        else if (member is IPropertySymbol property && property.GetAttributes().Any(attr =>
                            attr.AttributeClass.ToDisplayString() == "SyncFramework.SyncPropertyAttribute"))
                        {
                            classInfo.SyncProperties.Add(new SyncMemberInfo
                            {
                                Name = property.Name,
                                Type = property.Type.ToDisplayString()
                            });
                        }
                    }

                    SyncClasses.Add(classInfo);
                }
            }
        }
    }

    // 类信息结构保持不变
    public class SyncClassInfo
    {
        public string ClassName { get; set; }
        public string Namespace { get; set; }
        public List<SyncMemberInfo> SyncFields { get; } = new List<SyncMemberInfo>();
        public List<SyncMemberInfo> SyncProperties { get; } = new List<SyncMemberInfo>();
    }

    public class SyncMemberInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
}