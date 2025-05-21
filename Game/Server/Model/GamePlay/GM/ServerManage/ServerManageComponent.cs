using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using Quartz;

namespace ET.GamePlay
{
    [ComponentOf(typeof (Scene))]
    public class ServerManageComponent: Entity, IAwake,IDestroy
    {
        public List<ZoneServerAutoAddSetting> AllAutoAddSettings = new();
        public int LastAddSettingId = 1;
        public int CurrNewZone; // 当前新服

        public bool UseEmergency = true; // 是否使用应急策略 todo
        public int EmergencyLineOnPct = 9500; // 新服在线人数达到多少(百分比 10000 = 100%)的时候 触发应急线 自动再开新服


        [BsonIgnore]
        public Dictionary<ZoneServerAutoAddSetting,long> ScheduleTimerIds = new();
        [BsonIgnore]
        public long CheckEmergencyTimer;
    }
    
    
}