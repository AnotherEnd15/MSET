namespace ET
{
    public static partial class UnitHelper
    {
        // 禁止战斗模块直接用这个
        public static Unit GetMyPlayer(this Entity entity)
        {
            var currScene = entity.CurrentScene();
            if (currScene == null)
                return null;
            var unitCom = currScene.GetComponent<UnitComponent>();
            if (unitCom == null)
            {
                return null;
            }
            var masterPlayerId = unitCom.MasterPlayerId;
            if (masterPlayerId == 0)
                return null;
            return unitCom.Get(masterPlayerId);
        }
    }
}