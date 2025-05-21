using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;

namespace ET
{
    public static class BuildAssembliesHelper
    {
        public const string CodeDir = "Assets/Bundles/Code/";

        public static bool BuildModel(CodeOptimization codeOptimization)
        {
            List<string> codes  = new List<string>()
            {
                "Unity.Model",
                "Unity.ModelView",
            };

            if (!BuildAssembliesHelper.BuildMuteAssembly("Model", codes, codeOptimization))
                return false;

            File.Copy(Path.Combine(Define.BuildOutputDir, $"Model.dll"), Path.Combine(CodeDir, $"Model.dll.bytes"), true);
            File.Copy(Path.Combine(Define.BuildOutputDir, $"Model.pdb"), Path.Combine(CodeDir, $"Model.pdb.bytes"), true);
            Debug.Log("copy Model.dll to Bundles/Code success!");
            return true;
        }

        public static bool BuildHotfix(CodeOptimization codeOptimization)
        {
            string[] logicFiles = Directory.GetFiles(Define.BuildOutputDir, "Hotfix_*");
            foreach (string file in logicFiles)
            {
                File.Delete(file);
            }

            int random = RandomGenerator.RandomNumber(100000000, 999999999);
            string logicFile = $"Hotfix_{random}";

            List<string> codes = new List<string>()
            {
                "Unity.Hotfix",
                "Unity.HotfixView",
            };

            var analyzerDll = Path.Combine("../Share/Analyzer/bin/Debug/Share.Analyzer.dll");
            if (!File.Exists(analyzerDll))
            {
                Debug.LogError("分析器Dll不存在 请先编译ET.dll");
                return false;
            }

            if (!BuildAssembliesHelper.BuildMuteAssembly("Hotfix", codes, codeOptimization))
            {
                return false;
            }

            File.Copy(Path.Combine(Define.BuildOutputDir, "Hotfix.dll"), Path.Combine(CodeDir, $"Hotfix.dll.bytes"), true);
            File.Copy(Path.Combine(Define.BuildOutputDir, "Hotfix.pdb"), Path.Combine(CodeDir, $"Hotfix.pdb.bytes"), true);
            File.Copy(Path.Combine(Define.BuildOutputDir, "Hotfix.dll"), Path.Combine(Define.BuildOutputDir, $"{logicFile}.dll"), true);
            File.Copy(Path.Combine(Define.BuildOutputDir, "Hotfix.pdb"), Path.Combine(Define.BuildOutputDir, $"{logicFile}.pdb"), true);
            Debug.Log("copy Hotfix.dll to Bundles/Code success!");

            return true;
        }

        private static bool BuildMuteAssembly(
        string assemblyName, List<string> asmNames, CodeOptimization codeOptimization)
        {
            if (!Directory.Exists(Define.BuildOutputDir))
            {
                Directory.CreateDirectory(Define.BuildOutputDir);
            }

            string dllPath = Path.Combine(Define.BuildOutputDir, $"{assemblyName}.dll");
            string pdbPath = Path.Combine(Define.BuildOutputDir, $"{assemblyName}.pdb");
            File.Delete(dllPath);
            File.Delete(pdbPath);

            Directory.CreateDirectory(Define.BuildOutputDir);

            HashSet<string> files = new HashSet<string>();
            HashSet<string> refs = new HashSet<string>();
            HashSet<string> defs = new HashSet<string>();

            foreach (var asm in asmNames)
            {

                var assembly = CompilationPipeline.GetAssemblies().FirstOrDefault(v => v.name == asm);
                if (null == assembly)
                {
                    Debug.LogError($"找不到编译Src: {asm}");
                    return false;
                }

                foreach (var v in assembly.sourceFiles)
                {
                    files.Add(v);
                }

                foreach (var v in assembly.allReferences)
                {
                    refs.Add(v);
                }

                foreach (var v in assembly.defines)
                {
                    defs.Add(v);
                }

            }


            // Compiles scripts outside the Assets folder into a managed assembly that can be used inside the Assets folder.
            AssemblyBuilder builder = new AssemblyBuilder(dllPath, files.ToArray());
            builder.compilerOptions.AllowUnsafeCode = true;
            builder.compilerOptions.CodeOptimization = codeOptimization;
            BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            builder.compilerOptions.ApiCompatibilityLevel = PlayerSettings.GetApiCompatibilityLevel(buildTargetGroup);
            builder.additionalReferences = refs.ToArray();
            builder.flags = AssemblyBuilderFlags.None;
            // todo: 以下动作会导致程序集完成编译后IDE 报错，待确认：减少编译频率，变成 aa build 时编译，同时看能不能将 csproj 回到原点
            builder.referencesOptions = ReferencesOptions.UseEngineModules;
            builder.buildTarget = EditorUserBuildSettings.activeBuildTarget;
            builder.buildTargetGroup = buildTargetGroup;
            //builder.excludeReferences = new string[] { assembly.outputPath };
            builder.additionalDefines = defs.ToArray();
            
            bool noErr = true;
            builder.buildFinished += (arg1, arg2) =>
            {
              
                foreach (var msg in arg2)
                {
                    if (msg.type == CompilerMessageType.Error)
                    {
                        Debug.LogError(msg.message);
                        noErr = false;
                    }
                    else if (msg.type == CompilerMessageType.Warning)
                    {
                        Debug.LogWarning(msg.message);
                    }
                    else
                    {
                        Debug.Log(msg.message);
                    }
                }

            };
            if (!builder.Build())
            {
                Debug.LogError($"Assembly Build Fail！");
            }
            else
            {
                Debug.Log($"程序集： <color=yellow>{dllPath} </color> 完成构建！");
            }

            while (EditorApplication.isCompiling)
            {
                // 主线程sleep并不影响编译线程
                Thread.Sleep(1);
            }

            return noErr;
        }
    }
}