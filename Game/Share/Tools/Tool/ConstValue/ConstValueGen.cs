using System.Text;
using ET;
using NPOI.XSSF.UserModel;

namespace Tool
{
    public class ConstValueGen
    {
         public static string SrcPath => Options.Instance.InputPath;
        public static string ExportPath => Options.Instance.OutputPath;
        
        public class Config
        {
            public string VarName;
            public string Type;
            public string Value;
            public string Comment;
        }

        public static string Template = """
// 本文件自动生成,不要手动修改
namespace ET
{
    public static partial class ConstValue
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

                for (int i = 3; i <= sheet.LastRowNum; i++)
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
                    
                    var varName = row.GetCell(1).ToString();
                    var type = row.GetCell(2).ToString();
                    var value = row.GetCell(3).ToString();
                    var comment = row.GetCell(4).ToString();
                    
                    configs.Add(new Config()
                    {
                        VarName =  varName,
                        Type = type,
                        Value = value,
                        Comment = comment
                    });
                }
                
            }

            var content = new StringBuilder();
            foreach (var v in configs)
            {
                if (v.Type.Contains("[]"))
                {
                    content.AppendLine("\t\t[StaticField]");
                    content.AppendLine($"\t\tpublic static {v.Type} {v.VarName} = {{ {v.Value} }} ; // {v.Comment}");
                }
                else
                {
                    content.AppendLine($"\t\tpublic const {v.Type} {v.VarName} = {v.Value} ; // {v.Comment}");   
                }
            }

            var result = Template.Replace("#Content#", content.ToString());
            File.WriteAllText(ExportPath,result);
        }
    }
}