namespace ET
{
	public static class RobotLog
	{
#if DOTNET
		public static void Debug(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
		{
			Log.GetLogger().Debug(message.ToStringAndClear());
		}
		
		public static void Console(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
		{
			Log.GetLogger().Debug(message.ToStringAndClear());
		}
#endif
		
		public static void Debug(string msg)
		{
			Log.GetLogger().Debug(msg);
		}

		public static void Console(string msg)
		{
			Log.GetLogger().Debug(msg);
		}
	}
}