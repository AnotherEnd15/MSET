using StackExchange.Redis;

namespace ET;

public class RedisManager : Singleton<RedisManager>
{
    public ConnectionMultiplexer ConnectionMultiplexer { get; private set; }
    public IDatabase Database { get; private set; }
    
    public ISubscriber Subscriber { get; private set; }
    
    public RedisManager()
    {
        var etcdNode = ServerConfig.Instance.Config["redis"];
        ConnectionMultiplexer = ConnectionMultiplexer.Connect(etcdNode["connect"].ToString());
        Database = ConnectionMultiplexer.GetDatabase();
        Subscriber = ConnectionMultiplexer.GetSubscriber();
        
        // TODO 管理lua脚本的预热
    }
    
}