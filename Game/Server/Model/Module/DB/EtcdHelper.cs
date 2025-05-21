using System.Diagnostics;
using System.Threading;
using System;

namespace ET;

public static class EtcdHelper
{
    public static string EtcdKeyPrefix => $"{ServerConfig.Instance.HostName}/";
    public static string ConvertKey(string key)
    {
        return $"{EtcdKeyPrefix}{key}";
    }

}