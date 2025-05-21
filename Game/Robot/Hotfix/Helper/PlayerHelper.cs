namespace ET.Helper
{
    public static class PlayerHelper
    {
        public static long GetUnitId(this Scene clientScene)
        {
            return clientScene.ClientScene().GetComponent<PlayerDataComponent>().MyId;
        }
    }
}