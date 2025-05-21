using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using MemoryPack;
using UnityEngine;
using YooAsset;

namespace ET
{
	[MemoryPackable]
	public partial class AOTDll
	{
		[MemoryPackOrder(0)]
		public Dictionary<string,byte[]> Dlls = new();
	}

	public class CodeLoader: Singleton<CodeLoader>
	{
		private List<Assembly> assemblies =new();
		private Assembly model;

		public async ETTask Start()
		{
// #if !UNITY_EDITOR
//
// 			var loadList = new List<string>()
// 			{
// 				"Unity.Model",
// 				"Unity.ModelView",
// 				"Unity.Hotfix",
// 				"Unity.HotfixView"
// 			};
//
// 			Stopwatch sw = new();
// 			sw.Start();
//
// 			foreach (var v in loadList)
// 			{
// 				var path = $"Assets/Bundles/Code/{v}.dll.bytes";
// 				using var handle = YooAssets.LoadAssetAsync(path);
// 				await handle.Task;
// 				var b = (handle.AssetObject as TextAsset).bytes;
// 				var asm = Assembly.Load(b);
// 				if (v == "Unity.Model")
// 				{
// 					this.model = asm;
// 				}
// 				this.assemblies.Add(asm);
// 			}
// 			sw.Stop();
// 			Log.Information($"加载dll 总耗时: {sw.ElapsedMilliseconds} ms");
// 			sw.Restart();
// 			
// 			// 补充元数据
// 			{
// 				using var handle = YooAssets.LoadAssetAsync("Assets/Bundles/AOTDll/CombinedAOTDll.bytes");
// 				await handle.Task;
// 				var b = (handle.AssetObject as TextAsset).bytes;
// 				var aotDll = MemoryPackHelper.Deserialize(typeof(AOTDll), b, 0, b.Length) as AOTDll;
// 				foreach (var v in aotDll.Dlls)
// 				{
// 					var err = RuntimeApi.LoadMetadataForAOTAssembly(v.Value, HomologousImageMode.SuperSet);
// 					if (err != LoadImageErrorCode.OK)
// 					{
// 						Log.Error($"元数据注入失败: {v.Key}  {err}");
// 					}
// 				}
// 			}
// 			sw.Stop();
// 			Log.Information($" 补充元数据总耗时: {sw.ElapsedMilliseconds} ms");
//
// #else
			var set = new HashSet<string>()
			{
				"Unity.Model",
				"Unity.ModelView",
				"Unity.Hotfix",
				"Unity.HotfixView"
			};
			foreach (var v in AppDomain.CurrentDomain.GetAssemblies())
			{
				var name = v.GetName().Name;
				if (!set.Contains(name))
				{
					continue;
				}

				if (name == "Unity.Model")
				{
					this.model = v;
				}

				Log.Debug($"加载Dll: {name}");
				this.assemblies.Add(v);
			}

			await ETTask.CompletedTask;
//#endif


			IStaticMethod init = new StaticMethod(this.model, "ET.Entry", "Run");
			init.Run(assemblies);
		}
	}
}