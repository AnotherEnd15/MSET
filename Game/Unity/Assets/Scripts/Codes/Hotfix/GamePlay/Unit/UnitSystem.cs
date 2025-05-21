namespace ET
{
    [ObjectSystem]
    public class UnitSystem: AwakeSystem<Unit, int>
    {
        protected override void Awake(Unit self, int configId)
        {
            self.ConfigId = configId;
            self.UnitType = (UnitType)UnitConfigCategory.Instance.Get(configId).UnitType;
        }
    }
    //
    // [ObjectSystem]
    // public class UnitDisposeSystem: DestroySystem<Unit>
    // {
    //     protected override void Destroy(Unit self)
    //     {
    //         var unitCom = self.DomainScene().GetComponent<UnitComponent>();
    //         unitCom.Remove(self.Id);
    //     }
    // }
}