using System;


namespace ET
{
	public abstract class AChannel: IDisposable
	{
		public long Id;
		
		public ChannelType ChannelType { get; protected set; }

		public int Error { get; set; }

		public bool IsDisposed
		{
			get
			{
				return this.Id == 0;	
			}
		}

		public abstract void Dispose();
	}
}