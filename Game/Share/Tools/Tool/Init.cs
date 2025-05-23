using System;
using System.Collections.Generic;
using System.Threading;
using CommandLine;
using ET.ErrorCode;
using Tool;

namespace ET
{
    internal static class Init
    {
        private static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Console.WriteLine(e.ExceptionObject.ToString());
            };
            
            try
            {

                // 命令行参数
                var result = Parser.Default.ParseArguments<Options>(args)
                        .WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
                        .WithParsed(options => Options.Instance = options);
                

                switch (Options.Instance.ActionType)
                {
                    case "proto":
                        Proto2CS.Export();
                        return 0;
                    case "protoGM":
                        Proto2CS.Export(true);
                        return 0;
                    case "numeric":
                        NumericTypeGen.NumericTypeGen.Export();
                        return 0;
                    case "lang":
                        LanguageGen.Export();
                        return 0;
                    case "errorcode":
                        ErrorCodeGen.Export();
                        return 0;
                    case "constvalue":
                        ConstValueGen.Export();
                        return 0;
                    case "start_scene":
                        StartScene.Export();
                        return 0;
                    default:
                        throw new Exception($"未知ActionType: {Options.Instance.ActionType}");
                }


            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(e.ToString());
                Console.ResetColor();
            }
            return 1;
        }
    }
}