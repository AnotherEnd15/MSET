using System;

namespace ET
{
	public interface IEvent
	{
		Type Type { get; }
		
	}
	
	[Event]
	public abstract class AEvent<E,A>: IEvent where A: struct where E: Entity
	{
		public Type Type
		{
			get
			{
				return typeof (A);
			}
		}

		protected abstract ETTask Run(E entity, A a);

		public async ETTask Handle(E entity, A a)
		{
			try
			{
				await Run(entity, a);
			}
			catch (Exception e)
			{
				Log.GetLogger().Error("{TypeName} {Exception}",
					this.GetType().Name, e);
			}
		}
	}
}