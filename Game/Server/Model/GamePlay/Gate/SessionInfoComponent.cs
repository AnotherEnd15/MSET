namespace ET
{
	[ComponentOf(typeof(Player))]
	public class SessionInfoComponent : Entity,IAwake,IDestroy
	{
		public Session Session;

		public long DisconnectTimer;
	}
}