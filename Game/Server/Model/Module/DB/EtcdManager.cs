using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using dotnet_etcd;
using Etcdserverpb;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Mvccpb;
using Sirenix.OdinInspector;
using YamlDotNet.RepresentationModel;

namespace ET;

// 文档: https://github.com/shubhamranjan/dotnet-etcd?tab=readme-ov-file#documentation
public class EtcdManager : Singleton<EtcdManager>
{
    private EtcdClient EtcdClient;
    
    public long HealthTimer;
    public long LeaseId;
    public CancellationTokenSource CancellationToken;

    public EtcdManager()
    {
        CancellationToken = new CancellationTokenSource();
    }

    public async ETTask InitAsync()
    {
        var etcdNode = ServerConfig.Instance.Config["etcd"];
        var etcdPath = etcdNode["endpoints"].ToString();

        var handler = new SocketsHttpHandler
        {
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
            KeepAlivePingDelay = TimeSpan.FromSeconds(20),
            KeepAlivePingTimeout = TimeSpan.FromSeconds(5),
            EnableMultipleHttp2Connections = true,
        };
        EtcdClient = new EtcdClient(etcdPath, configureChannelOptions: options =>
        {
            options.Credentials = ChannelCredentials.Insecure;
            options.HttpHandler = handler;
        });

        // 本地服的情况下 清理一下旧的Key
        if (Options.Instance.StartConfig.Contains("Localhost") && Options.Instance.Process == 1)
        {
            await ClearKeys();
        }

        var response = await EtcdClient.LeaseGrantAsync(new LeaseGrantRequest()
        {
            TTL = 20
        });
        if (!string.IsNullOrEmpty(response.Error))
        {
            Log.GetLogger().Error("etcd lease failed: {Error}", response.Error);
            await TimerComponent.Instance.WaitAsync(2000);
            return;
        }
        Log.GetLogger().Information($"lease success: {response.ID}");
        this.LeaseId = response.ID;
        this.HealthTimer = TimerComponent.Instance.NewRepeatedTimer(6000, TimerInvokeType.EtcdHealth, this);
        await this.WatchRangeAsync(EtcdHelper.EtcdKeyPrefix, this.OnWatchSceneChanged);
        var existScenes = await this.GetRangeValAsync(EtcdHelper.EtcdKeyPrefix);
        foreach (var v in existScenes)
        {
            StartSceneService.Instance.AddConfig(v.Key, v.Value);
        }
    }
    
    public async ETTask EtcdHealthCheck()
    {
        await this.EtcdClient.LeaseKeepAlive(this.LeaseId, this.CancellationToken.Token);
    }

    // TODO 正常关服流程一定要有个地方触发这个
    public async Task ClearKeys()
    {
        await EtcdClient.DeleteRangeAsync(EtcdHelper.EtcdKeyPrefix);
    }

    public override void Dispose()
    {
        TimerComponent.Instance.Remove(ref HealthTimer);
    }

    public async ETTask PutAsync(string key, string value)
    {
        key = EtcdHelper.ConvertKey(key);
        var putResp = await EtcdClient.PutAsync(new PutRequest()
        {
            Key = ByteString.CopyFromUtf8(key),
            Value = ByteString.CopyFromUtf8(value),
            Lease = this.LeaseId
        });
        Log.GetLogger().Information("etcd put response {Key}: {Value} {Response}", key, value, putResp);
    }

    public async ETTask<IDictionary<string, string>> GetRangeValAsync(string prefix)
    {
        prefix = EtcdHelper.ConvertKey(prefix);
        return await EtcdClient.GetRangeValAsync(prefix);
    }

    public async ETTask<string> GetValAsync(string key)
    {
        key = EtcdHelper.ConvertKey(key);
        return await EtcdClient.GetValAsync(key);
    }

    public async ETTask WatchRangeAsync(string path, Action<WatchResponse> callback)
    {
        path = EtcdHelper.ConvertKey(path);
        await EtcdClient.WatchRangeAsync(path, callback);
    }
    
    
    #region 为scene监听特殊写
    
    private void OnWatchSceneChanged(WatchResponse response)
    {
        foreach (var evt in response.Events)
        {
            Log.GetLogger().Information($"Watch Scene {evt.Kv.Key.ToStringUtf8()} {evt.Type}");
            string key = evt.Kv.Key.ToStringUtf8();
            if (evt.Type == Event.Types.EventType.Put)
            {
                string value = evt.Kv.Value.ToStringUtf8();
                StartSceneService.Instance.AddConfig(key, value);
            }
            else if (evt.Type == Event.Types.EventType.Delete)
            {
                StartSceneService.Instance.RemoveConfig(key);
            }
        }
    }

    #endregion
}


[Invoke(TimerInvokeType.EtcdHealth)]
public class EtcdHealthCheckTimer : ATimer<EtcdManager>
{
    protected override void Run(EtcdManager t)
    {
       t.EtcdHealthCheck().Coroutine();
    }
}
