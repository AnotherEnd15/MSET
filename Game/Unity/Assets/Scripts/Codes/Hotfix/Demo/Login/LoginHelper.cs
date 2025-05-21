using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using ET.EventType;
using ET.Login;

namespace ET
{
    public static class LoginHelper
    {
        static string GetRealmAddress()
        {
            return "127.0.0.1:10000";
        }

        static string GetGateAddress(string gateConnect)
        {
            return "127.0.0.1"+ gateConnect;
        }


        public static async ETTask<(C2R_LoginResponse,int)> LoginRealm(Scene clientScene, string account, bool isWx,string ticket)
        {
            try
            {
                var loginInfo = clientScene.GetOrAdd<ClientLoginInfoComponent>();
                loginInfo.Account = account;
                loginInfo.IsWX = isWx;
                
                clientScene.RemoveComponent<NetClientComponent>();
                clientScene.RemoveComponent<SessionComponent>();

                var realmAdd = GetRealmAddress();

                var netCom = clientScene.AddComponent<NetClientComponent, string>(realmAdd);
                
                C2R_LoginResponse result;
                Session session = netCom.Session;
                {
                    session.AddComponent<PingComponent>();
                    clientScene.AddComponent<SessionComponent>().Session = session;
                    var r2CLogin = (C2R_LoginResponse)await session.Call(new C2R_LoginRequest()
                    {
                        Account = account,
                        InviteQueryInfo = ticket,
                        IsWx = isWx,
                        GameVersion = GameConst.GameVersion
                    });
                    if (r2CLogin.Error != 0)
                    {
                        Log.Debug($"登录失败: {r2CLogin.Error}");
                        return (null,r2CLogin.Error);
                    }

                    result = r2CLogin;
                }
              
                
                return (result,0);
            }
            catch (Exception e)
            {
                Log.Error(e);
                return (null,-1);
            }
        }

        public static async ETTask<int> LoginGate(Scene clientScene, int zone,Proto_DeviceInfo deviceInfo)
        {
            try
            {
                var loginInfo = clientScene.GetComponent<ClientLoginInfoComponent>();
                loginInfo.TargetZone = zone;
                
                var session = clientScene.GateSession();
                if (session == null || session.IsDisposed)
                {
                    return -2;
                }

                var r2c_enterGate = (C2R_EnterGateResponse)await session.Call(new C2R_EnterGateRequest() { Id = zone });
                if (r2c_enterGate.Error != 0)
                {
                    return r2c_enterGate.Error;
                }

                session.Dispose();

                clientScene.RemoveComponent<SessionComponent>();
                clientScene.RemoveComponent<NetClientComponent>(); // ios上需要重新创建socket,所以这里移除

                var gateAdd = GetGateAddress(r2c_enterGate.WSConnect);

                var netCom = clientScene.AddComponent<NetClientComponent, string>(gateAdd);
                // 创建一个gate Session,并且保存到SessionComponent中
                Session gateSession = netCom.Session;
                gateSession.AddComponent<PingComponent>();
                clientScene.AddComponent<SessionComponent>().Session = gateSession;

                G2C_LoginGate g2CLoginGate = (G2C_LoginGate)await gateSession.Call(
                    new C2G_LoginGate()
                    {
                        Key = r2c_enterGate.Key, GateId = r2c_enterGate.GateId,
                        DeviceData = deviceInfo
                    });

                if (g2CLoginGate.Error != 0)
                {
                    Log.Debug($"登陆gate失败 {g2CLoginGate.Error}!");
                    HandleLoginGateError(clientScene, g2CLoginGate.Error);
                    return g2CLoginGate.Error;
                }

                Log.Debug("登陆gate成功!");
                var connectCom = clientScene.GetOrAdd<ClientConnectionComponent>();
                connectCom.ConnectStr = r2c_enterGate.WSConnect;
                connectCom.GateId = r2c_enterGate.GateId;
                connectCom.OpenId = r2c_enterGate.OpenId;
                
                
                var enterMap = await gateSession.Call(new C2G_EnterMap() {});
                if (enterMap.Error != 0)
                {
                    Log.Debug($"进入游戏失败 {enterMap.Error}!");
                    HandleLoginGateError(clientScene, g2CLoginGate.Error);
                    return enterMap.Error;
                }

                clientScene.GetComponent<ClientLoginInfoComponent>().LoginSuccess = true;
                await EventSystem.Instance.PublishAsync(clientScene, new EventType.LoginFinish());

                return 0;
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                return -1;
            }
        }

        public static async ETTask<bool> Reconnect(Scene clientScene)
        {
            Log.ImportantInfo("重新登录");

            var connectCom = clientScene.GetComponent<ClientConnectionComponent>();
            if (connectCom == null)
            {
                SceneChangeHelper.DisposeCurrScene(clientScene);
                EventSystem.Instance.Publish(clientScene, new EventType.ReLoginRealm());
                return true;
            }

            EventSystem.Instance.Publish(clientScene,new ReconnectStart());

            clientScene.RemoveComponent<SessionComponent>();


            var accountInfo = clientScene.GetComponent<ClientLoginInfoComponent>();
            var gateAdd = GetGateAddress(connectCom.ConnectStr);

            var netCom = clientScene.GetComponent<NetClientComponent>();
            netCom.CreateSession(gateAdd);
            // 创建一个gate Session,并且保存到SessionComponent中
            Session gateSession = netCom.Session;
            gateSession.AddComponent<PingComponent>();
            clientScene.AddComponent<SessionComponent>().Session = gateSession;
            C2G_ReconnectResponse response = (C2G_ReconnectResponse)await gateSession.Call(
                new C2G_ReconnectRequest()
                {
                    OpenId = connectCom.OpenId,
                    Code = accountInfo.Account
                });

            if (response.Error != 0)
            {
                SceneChangeHelper.DisposeCurrScene(clientScene);
                await EventSystem.Instance.PublishAsync(clientScene, new EventType.ReLoginRealm());
                return false;
            }

            var resp = await gateSession.Call(new C2G_EnterMap() { MapInfo = connectCom.MapInfo });
            if (resp.Error != 0)
            {
                SceneChangeHelper.DisposeCurrScene(clientScene);
                await EventSystem.Instance.PublishAsync(clientScene, new EventType.ReLoginRealm());
                return false;
            }
            
            // 使用之前的缓存数据
            Log.Debug($"使用缓存数据包 {netCom.Sendbuffer.Count}");
            netCom.SetClientCache();
            
            EventSystem.Instance.Publish(clientScene,new ReconnectEnd());

            await EventSystem.Instance.PublishAsync(clientScene, new EventType.LoginFinish());
            return true;
        }

        static void HandleLoginGateError(Scene clientScene, int error)
        {
            if (error == ErrorCode.ERR_ServerClosed)
            {
                return;
            }
        }
    }
}