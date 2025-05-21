using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Proto文件解析器，用于解析proto定义并提取消息结构信息
/// </summary>
public class ProtoParser
{
    /// <summary>
    /// 表示Proto文件中定义的字段
    /// </summary>
    public class ProtoField
    {
        /// <summary>
        /// 字段类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 字段名称
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// 表示Proto文件中定义的枚举值
    /// </summary>
    public class ProtoEnumValue
    {
        /// <summary>
        /// 枚举名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 枚举值
        /// </summary>
        public int Value { get; set; }
    }

    /// <summary>
    /// 表示Proto文件中定义的消息或枚举类型
    /// </summary>
    public class ProtoDefinition
    {
        /// <summary>
        /// 消息或枚举的名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 消息继承的基类（如果有）
        /// </summary>
        public string BaseClass { get; set; }

        /// <summary>
        /// 消息实现的接口列表
        /// </summary>
        public List<string> Interfaces { get; set; } = new List<string>();

        /// <summary>
        /// 如果这是一个请求消息，则对应的响应类型
        /// </summary>
        public string ResponseType { get; set; }

        /// <summary>
        /// 是否为消息类型
        /// </summary>
        public bool IsMessage { get; set; }

        /// <summary>
        /// 是否为枚举类型
        /// </summary>
        public bool IsEnum { get; set; }

        /// <summary>
        /// 消息中定义的字段列表
        /// </summary>
        public List<ProtoField> Fields { get; set; } = new List<ProtoField>();

        /// <summary>
        /// 枚举中定义的值列表
        /// </summary>
        public List<ProtoEnumValue> EnumValues { get; set; } = new List<ProtoEnumValue>();

        /// <summary>
        /// 返回消息或枚举的详细信息字符串表示
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"名称: {Name}");

            if (!string.IsNullOrEmpty(BaseClass))
                sb.AppendLine($"基类: {BaseClass}");

            if (Interfaces.Any())
                sb.AppendLine($"实现接口: {string.Join(", ", Interfaces)}");

            if (!string.IsNullOrEmpty(ResponseType))
                sb.AppendLine($"响应类型: {ResponseType}");

            sb.AppendLine($"类型: {(IsMessage ? "消息" : "枚举")}");

            if (IsMessage)
            {
                sb.AppendLine("字段列表:");
                foreach (var field in Fields)
                    sb.AppendLine($"  {field.Type} {field.Name}");
            }
            else
            {
                sb.AppendLine("枚举值:");
                foreach (var value in EnumValues)
                    sb.AppendLine($"  {value.Name} = {value.Value}");
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// 解析Proto文件内容，返回所有消息和枚举定义的列表
    /// </summary>
    /// <param name="protoContent">Proto文件的内容</param>
    /// <returns>消息和枚举定义的有序列表</returns>
    public List<ProtoDefinition> ParseProtoFile(string protoContent)
    {
        var definitions = new List<ProtoDefinition>();
        var lines = protoContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        string lastComment = null;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            // 跳过模块声明和空行
            if (line.StartsWith("module") || line.StartsWith("package") || string.IsNullOrWhiteSpace(line))
                continue;

            // 捕获可能的ResponseType注释信息
            if (line.StartsWith("//"))
            {
                lastComment = line;
                continue;
            }

            // 检查是否为消息或枚举定义
            if (line.StartsWith("message") || line.StartsWith("enum"))
            {
                bool isMessage = line.StartsWith("message");
                string definitionType = isMessage ? "message" : "enum";

                // 提取定义的名称和可能的继承/接口信息
                string nameAndInterfaces = line.Substring(definitionType.Length).Trim();
                string name;
                string baseClass = null;
                List<string> interfaces = new List<string>();

                // 检查是否在注释中定义了继承关系和接口
                int commentIndex = nameAndInterfaces.IndexOf("//");
                if (commentIndex >= 0)
                {
                    string inheritancePart = nameAndInterfaces.Substring(commentIndex + 2).Trim();
                    var inheritanceItems = inheritancePart.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                        .Select(s => s.Trim())
                                                        .ToList();

                    // 第一个项目为基类（如果有），其余为接口
                    if (inheritanceItems.Count > 0)
                    {
                        if (!inheritanceItems[0].StartsWith("I"))
                        {
                            baseClass = inheritanceItems[0];
                            interfaces = inheritanceItems.Skip(1).ToList();
                        }
                        else 
                        {
                            interfaces = inheritanceItems;
                        }
                    }

                    name = nameAndInterfaces.Substring(0, commentIndex).Trim();
                }
                else
                {
                    // 处理名称后可能跟着'{'的情况
                    int braceIndex = nameAndInterfaces.IndexOf('{');
                    if (braceIndex >= 0)
                        name = nameAndInterfaces.Substring(0, braceIndex).Trim();
                    else
                        name = nameAndInterfaces.Trim();
                }

                var definition = new ProtoDefinition
                {
                    Name = name,
                    BaseClass = baseClass,
                    Interfaces = interfaces,
                    IsMessage = isMessage,
                    IsEnum = !isMessage
                };

                // 检查是否有ResponseType注释
                if (lastComment != null && lastComment.Contains("ResponseType"))
                {
                    var match = Regex.Match(lastComment, @"//ResponseType\s+(.+)");
                    if (match.Success)
                    {
                        definition.ResponseType = match.Groups[1].Value.Trim();
                    }
                }

                // 重置最后的注释
                lastComment = null;

                // 如果当前行没有找到开始的大括号，则查找下一行
                if (!line.Contains("{"))
                {
                    while (++i < lines.Length)
                    {
                        if (lines[i].Contains("{"))
                            break;
                    }
                }

                // 解析消息或枚举的内容
                while (++i < lines.Length)
                {
                    line = lines[i].Trim();

                    // 定义结束
                    if (line.Contains("}"))
                        break;

                    // 跳过空行和注释
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
                        continue;

                    if (isMessage)
                    {
                        // 解析消息字段
                        var field = ParseMessageField(line);
                        if (field != null)
                            definition.Fields.Add(field);
                    }
                    else
                    {
                        // 解析枚举值
                        var enumValue = ParseEnumValue(line);
                        if (enumValue != null)
                            definition.EnumValues.Add(enumValue);
                    }
                }

                definitions.Add(definition);
            }
        }

        return definitions;
    }

    /// <summary>
    /// 解析消息中的字段定义
    /// </summary>
    private ProtoField ParseMessageField(string line)
    {
        // 移除行尾注释
        int commentIndex = line.IndexOf("//");
        if (commentIndex >= 0)
            line = line.Substring(0, commentIndex);

        // 移除字段编号（'='之后的内容）
        int equalsIndex = line.IndexOf('=');
        if (equalsIndex >= 0)
            line = line.Substring(0, equalsIndex);

        line = line.Trim();
        if (string.IsNullOrWhiteSpace(line) || line.EndsWith(";"))
            line = line.TrimEnd(';');

        
        var ret = ParseProtoField(line);
        //if (line.Contains("Map") || line.Contains("repeated"))
        //{
        //    Console.WriteLine($"解析 {line}: {ret.Type} : {ret.FieldName}");
        //}

        return new ProtoField
        {
            Type = ret.Type,
            Name = ret.FieldName
        };
    }

    public (string Type, string FieldName) ParseProtoField(string fieldDefinition)
    {
        // 移除末尾可能的分号和空格
        fieldDefinition = fieldDefinition.TrimEnd(';', ' ');

        // 使用正则表达式匹配类型和字段名
        // 模式解释:
        // - (map<[^>]+>|repeated [a-zA-Z0-9\[\]]+|[a-zA-Z0-9\[\]]+) - 捕获类型部分(map或repeated或普通类型)
        // - \s+ - 匹配中间的空格
        // - ([a-zA-Z0-9_]+) - 捕获字段名部分
        var match = Regex.Match(fieldDefinition, @"(map<[^>]+>|repeated\s+[a-zA-Z0-9\[\]]+|[a-zA-Z0-9\[\]]+)\s+([a-zA-Z0-9_]+)");

        if (match.Success)
        {
            string type = match.Groups[1].Value.Trim();
            string fieldName = match.Groups[2].Value.Trim();
            return (type, fieldName);
        }

        throw new FormatException($"无法解析字段定义: {fieldDefinition}");
    }

    /// <summary>
    /// 解析枚举中的值定义
    /// </summary>
    private ProtoEnumValue ParseEnumValue(string line)
    {
        // 移除行尾注释
        int commentIndex = line.IndexOf("//");
        if (commentIndex >= 0)
            line = line.Substring(0, commentIndex);

        line = line.Trim();
        if (line.EndsWith(";"))
            line = line.TrimEnd(';');

        // 按等号分割
        string[] parts = line.Split(new[] { '=' }, 2);
        if (parts.Length != 2)
            return null;

        string name = parts[0].Trim();
        if (int.TryParse(parts[1].Trim().Split(';')[0], out int value))
        {
            return new ProtoEnumValue
            {
                Name = name,
                Value = value
            };
        }

        return null;
    }
}

// 使用示例:
// var parser = new ProtoParser();
// var definitions = parser.ParseProtoFile(protoContent);
// foreach(var def in definitions)
// {
//     Console.WriteLine(def.ToString());
// }