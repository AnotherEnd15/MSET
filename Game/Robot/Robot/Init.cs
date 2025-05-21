using System;
using CommandLine;

namespace ET
{
	public static class Init
	{
		public static void Start()
		{
			try
			{
				AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
				{
					Log.GetLogger().Error(e.ExceptionObject.ToString());
				};
				
				// 异步方法全部会回掉到主线程
				Game.AddSingleton<MainThreadSynchronizationContext>();

				// 命令行参数
				Parser.Default.ParseArguments<Options>(System.Environment.GetCommandLineArgs())
					.WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
					.WithParsed(Game.AddSingleton);

				// ProcessId从5W起
				Options.Instance.Process += 60001;
				
				Game.AddSingleton<TimeInfo>();
				Serilog.Log.Logger = LoggerConfigure.Build()
						.CreateLogger();
				Log.Init();
				Game.AddSingleton<ObjectPool>();
				Game.AddSingleton<IdGenerater>();

				ETTask.ExceptionHandler += Log.GetLogger().Error;
				
				Log.GetLogger().Information($"{Parser.Default.FormatCommandLine(Options.Instance)}");

				Game.AddSingleton<CodeLoader>().Start();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}

		public static void Update()
		{
			Game.Update();
		}

		public static void LateUpdate()
		{
			Game.LateUpdate();
		}

		public static void FrameFinishUpdate()
		{
			Game.FrameFinishUpdate();
		}
	}
}
