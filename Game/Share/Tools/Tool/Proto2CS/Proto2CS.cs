using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static ProtoParser;

namespace ET
{
    internal class OpcodeInfo
    {
        public string Name;
        public int Opcode;
    }

    public static class Proto2CS
    {
        public static void Export(bool genGM = false)
        {
            // InnerMessage.proto生成cs代码
            InnerProto2CS.Proto2CS(genGM);
            Console.WriteLine("proto2cs succeed!");
        }
    }

    public static class InnerProto2CS
    {
        private static readonly char[] splitChars = { ' ', '\t' };
        private static readonly Dictionary<string, int> msgOpcode = new ();

        private static string clientMessagePath => Options.Instance.Proto_ClientOutputPath;
        private static string serverMessagePath => Options.Instance.Proto_ServerOutputPath;

        private static string protoDir => Environment.CurrentDirectory;
        
        public static void Proto2CS(bool genGM = false)
        {
            msgOpcode.Clear();

            if (Directory.Exists(clientMessagePath))
            {
                //Directory.Delete(clientMessagePath, true);
                //获取目录下所有文件，删除后缀为.cs的文件
                string[] files = Directory.GetFiles(clientMessagePath);
                foreach (string file in files)
                {
                    if (file.EndsWith(".cs"))
                    {
                        File.Delete(file);
                    }
                }

            }

            if (Directory.Exists(serverMessagePath))
            {
                Directory.Delete(serverMessagePath, true);
            }

            
            {
                var dir = Path.Combine(protoDir, "Outer");
                int startOpcode = OpcodeRangeDefine.MinOpcode + 1;
                BuildProto(dir, startOpcode, "C");
            }
            {
                var dir = Path.Combine(protoDir, "Inner");
                int startOpcode = OpcodeRangeDefine.OuterMaxOpcode + 1;
                BuildProto(dir, startOpcode, "S");
            }
            if(genGM)
            {
                var dir = Path.Combine(protoDir, "OuterGMTool");
                int startOpcode = OpcodeRangeDefine.InnerMaxOpcode + 1;
                BuildProto(dir, startOpcode, "C");
            }
        }

        static void BuildProto(string dir, int startOpcode, string cs)
        {
            List<string> list = Directory.GetFiles(dir, "*proto").ToList();
            foreach (string s in list)
            {
                if (!s.EndsWith(".proto"))
                {
                    continue;
                }

                var fileName = Path.GetFileNameWithoutExtension(s);

                ProtoFile2CS(s,fileName, cs, ref startOpcode);
            }
        }

        public static void ProtoFile2CS(string filePath,string proto, string cs,ref int startOpcode)
        {
            string ns = "ET.Proto";
            msgOpcode.Clear();
            string s = File.ReadAllText(filePath);
            

            var protoName = "OuterMessage";
            if (cs == "S")
                protoName = "InnerMessage";

            Dictionary<string, List<string>> interface2Childrens = new();
            
            Dictionary<string, List<string>> interface2Fields = new();

            HashSet<string> MsgNames = new();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using ET;");
            sb.AppendLine("using System;");
            sb.AppendLine("using MemoryPack;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using MongoDB.Bson;");
            sb.AppendLine("using MongoDB.Bson.Serialization.Attributes;");
            sb.AppendLine("using MongoDB.Bson.Serialization.Options;");
            sb.AppendLine($"namespace {ns};");
            sb.AppendLine("// ReSharper disable InconsistentNaming");

            var defs = new ProtoParser().ParseProtoFile(s);

            foreach (var definition in defs.Where(d => d.IsMessage))
            {
                if (definition.Interfaces.Count == 0)
                    continue;
                msgOpcode.Add(definition.Name, startOpcode++);
            }
            
            GenerateMemoryPackClasses(sb, defs, cs != "S");

            sb.Append("public static partial class " + protoName + "\n\t{\n");
            foreach (var v in msgOpcode)
            {
                sb.Append($"\t public const ushort {v.Key} = {v.Value};\n");
            }

            sb.Append("}\n");
            

            if (cs.Contains("C"))
            {
                GenerateCS(sb, serverMessagePath, protoName,proto);
                GenerateCS(sb, clientMessagePath, protoName,proto);
            }
            
            if (cs.Contains("S"))
            {
                GenerateCS(sb, serverMessagePath, protoName,proto);
            }
        }

        /// <summary>
        /// 根据解析出的Proto定义生成对应的MemoryPack可序列化类代码
        /// </summary>
        /// <param name="sb">用于输出代码的StringBuilder</param>
        /// <param name="definitions">解析出的Proto定义列表</param>
        /// <param name="indentLevel">代码缩进级别</param>
        public static void GenerateMemoryPackClasses(StringBuilder sb, List<ProtoDefinition> definitions, bool addMemoryPack,  int indentLevel = 1)
        {
            string indent = new string('\t', indentLevel);

            // 先找出所有继承关系，为每个基类收集其子类
            Dictionary<string, List<ProtoDefinition>> baseClassToSubClasses = new Dictionary<string, List<ProtoDefinition>>();

            foreach (var definition in definitions.Where(d => d.IsMessage && !string.IsNullOrEmpty(d.BaseClass)))
            {
                if (!baseClassToSubClasses.ContainsKey(definition.BaseClass))
                {
                    baseClassToSubClasses[definition.BaseClass] = new List<ProtoDefinition>();
                }

                baseClassToSubClasses[definition.BaseClass].Add(definition);
            }

            // 枚举定义处理
            foreach (var definition in definitions.Where(d => d.IsEnum))
            {
                // 生成枚举定义
                sb.AppendLine($"{indent}public enum {definition.Name}");
                sb.AppendLine($"{indent}{{");

                // 添加枚举成员
                foreach (var value in definition.EnumValues)
                {
                    sb.AppendLine($"{indent}\t{value.Name} = {value.Value},");
                }

                sb.AppendLine($"{indent}}}");
                sb.AppendLine();
            }

            // 消息定义处理
            foreach (var definition in definitions.Where(d => d.IsMessage))
            {
                // 如果有ResponseType，添加标记
                if (!string.IsNullOrEmpty(definition.ResponseType))
                {
                    sb.AppendLine($"{indent}[ResponseType(typeof({definition.ResponseType}))]");
                }

                if (msgOpcode.TryGetValue(definition.Name, out var code))
                {
                    sb.AppendLine($"{indent}[Message({code})]");
                }

                // 添加MemoryPack序列化标记
                if (addMemoryPack)
                {
                    sb.AppendLine($"{indent}[MemoryPackable]");

                    // 如果这个类是其他类的基类，添加MemoryPackUnion特性标记所有子类
                    if (baseClassToSubClasses.ContainsKey(definition.Name))
                    {
                        int unionTag = 0;
                        foreach (var subClass in baseClassToSubClasses[definition.Name])
                        {
                            sb.AppendLine($"{indent}[MemoryPackUnion({unionTag++}, typeof({subClass.Name}))]");
                        }
                    }
                }
                else
                {
                    definition.BaseClass = "Object";
                }

                // 处理类定义，包括继承和接口实现
                string classDefinition = $"public partial class {definition.Name}";
                if (!string.IsNullOrEmpty(definition.BaseClass))
                {
                    classDefinition += $" : {definition.BaseClass}";

                    // 如果有接口，添加到继承列表
                    if (definition.Interfaces.Any())
                        classDefinition += $", {string.Join(", ", definition.Interfaces)}";
                }
                else if (definition.Interfaces.Any())
                {
                    classDefinition += $" : {string.Join(", ", definition.Interfaces)}";
                }

                sb.AppendLine($"{indent}{classDefinition}");
                sb.AppendLine($"{indent}{{");

                // 根据接口类型添加特定字段
                int orderIndex = 0;

                //Console.WriteLine($"def name {definition.Name} {(definition.Interfaces.Count != 0 ? definition.Interfaces[0].ToString() : "")}");

                // IRequest系列接口处理
                if (definition.Interfaces.Any(i => i == "IRequest" || Regex.Match(i, @"^IActor.*Request").Success))
                {
                    if (addMemoryPack) sb.AppendLine($"{indent}\t[MemoryPackOrder({orderIndex++})]");
                    sb.AppendLine($"{indent}\tpublic int RpcId {{ get; set; }}");
                    sb.AppendLine();
                }

                // // IActorRequest系列接口处理（必须放在IRequest后面，因为IActorRequest继承自IRequest）
                // if (definition.Interfaces.Any(i => Regex.Match(i, @"^IActor.*(Request|Message)$").Success))
                // {
                //     sb.AppendLine($"{indent}\t[MemoryPackOrder({orderIndex++})]");
                //     sb.AppendLine($"{indent}\tpublic long SrcActorId {{ get; set; }}");
                //     sb.AppendLine();
                // }

                // IResponse系列接口处理
                if (definition.Interfaces.Any(i => i == "IResponse" || Regex.Match(i, @"^IActor.*Response").Success))
                {
                    if (addMemoryPack) sb.AppendLine($"{indent}\t[MemoryPackOrder({orderIndex++})]");
                    sb.AppendLine($"{indent}\tpublic int RpcId {{ get; set; }}");
                    sb.AppendLine();

                    if (addMemoryPack) sb.AppendLine($"{indent}\t[MemoryPackOrder({orderIndex++})]");
                    sb.AppendLine($"{indent}\tpublic int Error {{ get; set; }}");
                    sb.AppendLine();

                    if (addMemoryPack) sb.AppendLine($"{indent}\t[MemoryPackOrder({orderIndex++})]");
                    sb.AppendLine($"{indent}\tpublic string Message {{ get; set; }}");
                    sb.AppendLine();
                }

                // 添加所有字段的属性
                for (int i = 0; i < definition.Fields.Count; i++)
                {
                    var field = definition.Fields[i];
                    string fieldType = field.Type;
                    string fieldName = char.ToUpper(field.Name[0]) + field.Name.Substring(1);

     

                    // 处理Map类型转换为Dictionary
                    if (fieldType.StartsWith("map<") && fieldType.EndsWith(">"))
                    {
                        // 提取Map中的泛型参数
                        string mapGenericContent = fieldType.Replace("map<", "").Replace(">", "");
                        var comma= mapGenericContent.Split(',');

                        string keyType = comma[0].Trim();
                        string valueType = comma[1].Trim();

                        // 转换为Dictionary类型
                        fieldType = $"Dictionary<{ConvertType(keyType)}, {ConvertType(valueType)}>";

                        // 添加MongoDB的字典序列化标记
                        sb.AppendLine($"{indent}\t[BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]");
                        if (addMemoryPack) sb.AppendLine($"{indent}\t[MemoryPackOrder({orderIndex++})]");
                        sb.Append($"\t\tpublic {fieldType} {fieldName} {{ get; set; }} = new (); \n\n");
                    }
                    else if (fieldType.StartsWith("repeated"))
                    {
                        string[] ss = fieldType.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                        string type = ss[^1];
                        type = ConvertType(type);

                        if (addMemoryPack) sb.AppendLine($"{indent}\t[MemoryPackOrder({orderIndex++})]");
                        sb.Append($"\t\tpublic List<{type}> {fieldName} {{ get; set; }} = new (); \n\n");
                    }
                    else
                    {
                        fieldType = ConvertType(fieldType);
     
                        if (addMemoryPack) sb.AppendLine($"{indent}\t[MemoryPackOrder({orderIndex++})]");
                        sb.AppendLine($"{indent}\tpublic {fieldType} {char.ToUpper(field.Name[0]) + field.Name.Substring(1)} {{ get; set; }}");
                    }

                    // 如果不是最后一个字段，添加空行
                    if (i < definition.Fields.Count - 1)
                        sb.AppendLine();
                }

                sb.AppendLine($"{indent}}}");
                sb.AppendLine();
            }
        }

        /// <summary>
        /// 辅助方法：在泛型参数中查找分隔符位置，考虑嵌套泛型的情况
        /// </summary>
        private static int FindGenericParameterSeparator(string genericContent)
        {
            int depth = 0;
            for (int i = 0; i < genericContent.Length; i++)
            {
                char c = genericContent[i];
                if (c == '<') depth++;
                else if (c == '>') depth--;
                else if (c == ',' && depth == 0) return i;
            }
            return -1;
        }

        private static void GenerateCS(StringBuilder sb, string path, string protoName,string proto)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string csPath = Path.Combine(path, $"{protoName}.{proto}.cs");
            using (FileStream txt = new FileStream(csPath, FileMode.Create, FileAccess.ReadWrite))
            using (StreamWriter sw = new StreamWriter(txt))
            {
                sw.Write(sb.ToString());
            }
            Console.WriteLine($"生成文件 {csPath}");
        }


        private static string ConvertType(string type)
        {
            string typeCs = "";
            switch (type)
            {
                case "int16":
                    typeCs = "short";
                    break;
                case "int32":
                    typeCs = "int";
                    break;
                case "bytes":
                    typeCs = "byte[]";
                    break;
                case "uint32":
                    typeCs = "uint";
                    break;
                case "long":
                    typeCs = "long";
                    break;
                case "int64":
                    typeCs = "long";
                    break;
                case "uint64":
                    typeCs = "ulong";
                    break;
                case "uint16":
                    typeCs = "ushort";
                    break;
                default:
                    typeCs = type;
                    break;
            }

            return typeCs;
        }
    }
}