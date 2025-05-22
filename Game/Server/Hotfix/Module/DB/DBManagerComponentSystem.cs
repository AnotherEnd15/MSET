using System;

namespace ET.Server
{
    [FriendOf(typeof(DBManagerComponent))]
    public static class DBManagerComponentSystem
    {
        [ObjectSystem]
        public class DBManagerComponentAwakeSystem: AwakeSystem<DBManagerComponent>
        {
            protected override void Awake(DBManagerComponent self)
            {
                DBManagerComponent.Instance = self;

                foreach (var v in EventSystem.Instance.GetTypes())
                {
                    if (v.Value.IsAbstract || !v.Value.IsClass)
                    {
                        continue;
                    }

                    if (typeof (IPlayerDB).IsAssignableFrom(v.Value))
                    {
                        self.AllDBTransferCom.Add(v.Value);
                    }
                 
                }
            }
        }

        [ObjectSystem]
        public class DBManagerComponentDestroySystem: DestroySystem<DBManagerComponent>
        {
            protected override void Destroy(DBManagerComponent self)
            {
                DBManagerComponent.Instance = null;
            }
        }
        
        public static DBComponent GetZoneDB(this DBManagerComponent self, int zone)
        {
            var mongoUrl = ET.ServerConfig.Instance.Config["mongo_url"].ToString();
            
            if(self.DBComponents.TryGetValue(zone,out var dbComponent))
            {
                return dbComponent;
            }

            dbComponent = self.AddChildWithId<DBComponent, string, string>(zone,mongoUrl, $"{ServerConfig.Instance.HostName}_{zone}");
            self.DBComponents[zone] = dbComponent;
            return dbComponent;
        }

        // public static RedisComponent GetZoneRedis(this DBManagerComponent self, int zone)
        // {
        //     StartZoneConfig startZoneConfig = StartZoneConfigCategory.Instance.Get(zone);
        //     var dbConfig = StartDBConfigCategory.Instance.Get(startZoneConfig.DBId);
        //     if (dbConfig.RedisConnect == null)
        //     {
        //         throw new Exception($"zone: {zone} not found redis connect string");
        //     }
        //     
        //     if(self.RedisComponents.TryGetValue(zone,out var dbComponent))
        //     {
        //         return dbComponent;
        //     }
        //
        //     dbComponent = self.AddChild<RedisComponent, RedisConfig>(dbConfig.RedisConnect);
        //     self.RedisComponents[zone] = dbComponent;
        //     return dbComponent;
        // }
    }
}