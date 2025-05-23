namespace ET
{
	[ComponentOf(typeof(Session))]
	public class SessionPlayerComponent : Entity, IAwake, IDestroy
	{
		// 只读数据
		public Account Account;
		public long GameSceneInstanceId;
		public long PlayerActorId;
		
		// 为true时表示被主动T掉的
		public bool IsKicked = false;
	}
}