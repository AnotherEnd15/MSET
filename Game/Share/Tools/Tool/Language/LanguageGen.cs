using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NPOI.XSSF.UserModel;

namespace ET
{
    public static class LanguageGen
    {
        public static string SrcPath => Options.Instance.InputPath;
        public static string ExportPath => Options.Instance.OutputPath;
        
        public class Config
        {
            public int Id;
            public string Commment;
            public string VarName;
        }

        public static string Template = """
// 本文件自动生成,不要手动修改
namespace ET
{
    [UniqueId]
    public static partial class LanguageKey
    {
#Content#
    }
}
""";
        
        public static void Export()
        {
            List<Config> configs = new();
            using (var stream = File.Open(SrcPath, FileMode.Open, FileAccess.Read,FileShare.ReadWrite))
            {
                using XSSFWorkbook xssWorkbook = new XSSFWorkbook(stream);
                var sheet = xssWorkbook.GetSheetAt(0);

                for (int i = 5; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if(row == null)
                        continue;
                    if(row.GetCell(1) == null || string.IsNullOrEmpty(row.GetCell(1).ToString()))
                        continue;
                    
                    if (row.GetCell(0)!= null && !string.IsNullOrEmpty(row.GetCell(0).ToString()) && row.GetCell(0).ToString().Contains("#"))
                    {
                        continue;
                    }
                    
                    if(row.GetCell(2) == null || string.IsNullOrEmpty(row.GetCell(2).ToString()))
                        continue;

                    var id = int.Parse(row.GetCell(1).ToString());
                    var varName = row.GetCell(2).ToString();
                    var comment = row.GetCell(3).ToString();
                    
                    configs.Add(new Config()
                    {
                        Id = id,
                        Commment = comment,
                        VarName = varName,
                    });
                }
                
            }

            var content = new StringBuilder();
            foreach (var v in configs)
            {
                content.AppendLine($"\t\tpublic const int {v.VarName} = {v.Id} ; // {v.Commment}");
            }

            var result = Template.Replace("#Content#", content.ToString());
            File.WriteAllText(ExportPath,result);
        }
    }
}