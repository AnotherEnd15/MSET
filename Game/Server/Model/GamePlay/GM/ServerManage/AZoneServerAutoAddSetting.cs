using MongoDB.Bson.Serialization.Attributes;

namespace ET.GamePlay
{
    public class ZoneServerAutoAddSetting: Object
    {
        public int Id;
        public string Cron;
    }
}