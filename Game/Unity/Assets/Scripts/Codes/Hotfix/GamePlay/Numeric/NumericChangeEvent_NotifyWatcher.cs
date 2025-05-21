namespace ET
{
	// 分发数值监听
	[Event]  // 服务端Map需要分发, 客户端CurrentScene也要分发
	public class NumericChangeEvent_NotifyWatcher: AEvent<Unit,EventType.NumbericChange>
	{
		protected override async ETTask Run(Unit unit, EventType.NumbericChange args)
		{
			NumericWatcherComponent.Instance.Run(unit, args);
			await ETTask.CompletedTask;
		}
	}
}
