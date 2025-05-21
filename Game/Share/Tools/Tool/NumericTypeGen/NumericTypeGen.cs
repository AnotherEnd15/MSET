using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NPOI.XSSF.UserModel;

namespace ET.NumericTypeGen
{
    public static class NumericTypeGen
    {
        public static string SrcPath => Options.Instance.InputPath;
        public static string ExportPath => Options.Instance.OutputPath;
        
        public class NumericConfig
        {
            public int Id;
            public string Commment;
            public string VarName;
            public int GenSnd;
        }
        

        public static string Template = """
<!-- 本文件自动生成,不要手动修改 -->
<module name="">
   <enum name="NumericType">
#Content#
    </enum>
</module>
""";
        
        public static void Export()
        {
            List<NumericConfig> configs = new();

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

                    var id = int.Parse(row.GetCell(1).ToString());
                    var comment = "";
                    if (row.GetCell(4) != null)
                    {
                        comment = row.GetCell(4).ToString();
                    }

                    var genAdjust = 0;
                    
                    
                    if (row.GetCell(5) != null)
                    {
                        var str = row.GetCell(5).ToString();
                        if (!string.IsNullOrEmpty(str))
                            genAdjust = int.Parse(str);
                    }

                    var varName = row.GetCell(2).ToString();

                    configs.Add(new NumericConfig()
                    {
                        Id = id,
                        Commment = comment,
                        VarName = varName,
                        GenSnd = genAdjust
                    });
                }
                
            }

            {
                var content = new StringBuilder();
                content.AppendLine($"\t\t<var name=\"None\" value=\"0\" comment=\"特殊用途\" />");
                foreach (var v in configs)
                {
                    content.AppendLine($"\t\t<var name=\"{v.VarName}\" value=\"{v.Id}\" comment=\"{v.Commment}\" />");
                    if (v.GenSnd != 0)
                    {
                        content.AppendLine(
                            $"\t\t<var name=\"{v.VarName}_Base\" value=\"{v.Id * 10 + 1}\" comment=\"{v.Commment}\" />");
                        content.AppendLine(
                            $"\t\t<var name=\"{v.VarName}_Add\" value=\"{v.Id * 10 + 2}\"  />");
                        content.AppendLine(
                            $"\t\t<var name=\"{v.VarName}_Pct\" value=\"{v.Id * 10 + 3}\" />");
                    }
                }

                var result = Template.Replace("#Content#", content.ToString());
                File.WriteAllText(ExportPath, result);
            }
        }
    }
}