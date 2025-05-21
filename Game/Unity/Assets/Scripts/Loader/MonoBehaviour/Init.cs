using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace ET
{
	[Preserve]
	public class Init: MonoBehaviour
	{
		public static bool GameStartFromInit;

		public static Init Instance;

		public GameObject StartUI;

		public long ClientStartTime;
		
		private void Start()
		{
			Instance = this;
			GameStartFromInit = true;

			Physics.autoSimulation = false;

			Application.targetFrameRate = 60;
			
			DontDestroyOnLoad(gameObject);

			AppDomain.CurrentDomain.UnhandledException += (sender, e) => { Log.Error(e.ExceptionObject.ToString()); };

#if !UNITY_WEBGL
			Game.AddSingleton<MainThreadSynchronizationContext>();
#endif
            // 命令行参数
            Game.AddSingleton<Options>();
			Options.Instance.Process = 50000;

			Game.AddSingleton<TimeInfo>();
			Log.Init();
			Game.AddSingleton<ObjectPool>();
			Game.AddSingleton<IdGenerater>();

			Serilog.Log.Logger = LoggerConfigure.BuildLogger().CreateLogger();
			ETTask.ExceptionHandler += Log.Error;

			InitAsync().Coroutine();
			Log.ImportantInfo("渲染分辨率：" + Screen.currentResolution.width + "X" + Screen.currentResolution.height);
		}

		private async ETTask InitAsync()
		{
			ClientStartTime = TimeHelper.ClientNow();
			Game.AddSingleton<CodeLoader>().Start().Coroutine();
		}
		
		private void Update()
		{
            Game.Update();
		}

		private void LateUpdate()
		{
            Game.LateUpdate();
			Game.FrameFinishUpdate();
		}

		private void OnApplicationQuit()
		{
			Game.Close();
		}
	}
}