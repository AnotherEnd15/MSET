namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class PlayerDataComponent: Entity, IAwake
    {
        public int OriginZone { get; set; }
        public long MyId { get; set; }
        public int Level { get; set; }
        public int Exp { get; set; }
        public long BattleScore { get; set; }

        public int MainlineLevelId { get; set; }

        public string ProfilePic { get; set; }
        public int ProfileFrame { get; set; }
        public string UID { get; set; }
        public string Name { get; set; }
        public bool IsFirstRename { get; set; }
        public string ServerName { get; set; }
        public int PlayerTitle { get; set; }
    }
}