using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NPOI.XSSF.UserModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ET
{
    public static class StartScene
    {
        public static string SrcPath => Options.Instance.InputPath;
        public static string ExportPath => Options.Instance.OutputPath;
        
        public static void Export()
        {
            StartConfig config = new();

            using (var stream = File.Open(SrcPath, FileMode.Open, FileAccess.Read,FileShare.ReadWrite))
            {
                using XSSFWorkbook xssWorkbook = new XSSFWorkbook(stream);
                
                var scene_sheet = xssWorkbook.GetSheet("scene");
                for (int i = 5; i <= scene_sheet.LastRowNum; i++)
                {
                    var row = scene_sheet.GetRow(i);
                    if(row == null)
                        continue;
                    if(row.GetCell(1) == null || string.IsNullOrEmpty(row.GetCell(1).ToString()))
                        continue;
                    
                    if (row.GetCell(0)!= null && !string.IsNullOrEmpty(row.GetCell(0).ToString()) && row.GetCell(0).ToString().Contains("#"))
                    {
                        continue;
                    }

                    var process = int.Parse(row.GetCell(1).ToString());
                    var sceneType = row.GetCell(2).ToString();
                    var port = 0;
                    if (row.GetCell(3) != null)
                    {
                        port = int.Parse(row.GetCell(3).ToString());
                    }
                    List<string> tags = new();
                    if (row.GetCell(4) != null)
                    {
                        var sp =  row.GetCell(4).ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        tags.AddRange(sp);
                    }

                    var zone = int.Parse(row.GetCell(5).ToString());
                    config.SceneConfigs.Add(new StartSceneConfig()
                    {
                        Process = process,
                        SceneType = sceneType,
                        OuterPort = port,
                        Tags = tags,
                        Zone = zone
                    });
                }
                
                var process_sheet = xssWorkbook.GetSheet("process");
                for (int i = 5; i <= process_sheet.LastRowNum; i++)
                {
                    var row = process_sheet.GetRow(i);
                    if(row == null)
                        continue;
                    if(row.GetCell(1) == null || string.IsNullOrEmpty(row.GetCell(1).ToString()))
                        continue;
                    
                    if (row.GetCell(0)!= null && !string.IsNullOrEmpty(row.GetCell(0).ToString()) && row.GetCell(0).ToString().Contains("#"))
                    {
                        continue;
                    }

                    var process = int.Parse(row.GetCell(1).ToString());
                    var ip = row.GetCell(2).ToString() +":"+ row.GetCell(3).ToString();
                    config.ProcessConfigs.Add(process,  new StartProcessConfig()
                    {
                        Process = process,
                        ip = ip
                    });
                }
                
            }

            {
                // 根据configs生成一个yaml的配置文件
                // 创建序列化器
                var serializer = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                    .Build();

                // 序列化为 YAML
                var yaml = serializer.Serialize(config);

                // 添加注释头
                var yamlWithComment = $"# 此文件为自动生成，请勿手动修改\n{yaml}";

                // 写入文件
                File.WriteAllText(ExportPath, yamlWithComment);
            }
        }
    }
}