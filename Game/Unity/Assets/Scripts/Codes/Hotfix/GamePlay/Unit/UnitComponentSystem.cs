

using ET.EventType;

namespace ET
{
	[ObjectSystem]
	public class UnitComponentAwakeSystem : AwakeSystem<UnitComponent>
	{
		protected override void Awake(UnitComponent self)
		{
		}
	}
	
	[ObjectSystem]
	public class UnitComponentDestroySystem : DestroySystem<UnitComponent>
	{
		protected override void Destroy(UnitComponent self)
		{
		}
	}
	
	public static class UnitComponentSystem
	{
		public static void Add(this UnitComponent self, Unit unit)
		{
			if (unit.UnitType == UnitType.Player)
			{
				self.Players.Add(unit);
			}
			
			EventSystem.Instance.Publish(unit,new UnitAdd());
		}

		public static Unit Get(this UnitComponent self, long id)
		{
			Unit unit = self.GetChild<Unit>(id);
			return unit;
		}

		public static void Remove(this UnitComponent self, long id, bool withEvent = true)
		{
			Unit unit = self.GetChild<Unit>(id);
			if (unit == null)
				return;

			if (withEvent)
				EventSystem.Instance.Publish(unit, new EventType.UnitRemove());

			if (unit.UnitType == UnitType.Player)
			{
				self.Players.Remove(unit);
			}
			unit?.Dispose();
		}
	}
}