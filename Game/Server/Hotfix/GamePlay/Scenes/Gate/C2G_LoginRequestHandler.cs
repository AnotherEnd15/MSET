using ET.Proto;
using ET;
using ET.DBProxy;
using MongoDB.Bson;
using System;
using System.Threading.Tasks;

namespace ET.GamePlay.Scenes.Gate
{
    [MessageHandler(SceneType.Gate)]
    internal class C2G_LoginRequestHandler : AMRpcHandler<C2G_LoginRequest, G2C_LoginResponse>
    {
        protected override async ETTask Run(Session session, C2G_LoginRequest request, G2C_LoginResponse response)
        {
            session.RemoveComponent<SessionAcceptTimeoutComponent>();

            if (string.IsNullOrEmpty(request.Account))
            {
                response.Error = ErrorCode.ERR_AccountDataError;
                return;
            }

            var userName = request.Account;
            
            try
            {
                // 1. 查询账号是否已存在 - 使用高级Lambda表达式API
                var existingAccount = await DBHelper.FindOneAsync<Account>(x => x.UserName == userName);

                // 如果账号存在，直接进入游戏
                if (existingAccount != null)
                {
                    await EnterGame(session, existingAccount);
                    return;
                }

                // 2. 生成新的UID - 使用原子增量操作
                var uidFilter = new BsonDocument(DBFieldNames.Id, 1);
                var incrementResult = await DBHelper.Increment("UidGenerator", uidFilter, "index", 1);
                
                // 如果增量操作失败，说明记录不存在，先初始化
                if (!incrementResult.Success)
                {
                    // 插入初始的UID生成器记录
                    var initialUidGen = new { _id = 1, index = 1 };
                    await DBHelper.InsertDocument("UidGenerator", initialUidGen.ToBsonDocument());
                    
                    // 再次尝试增量操作
                    incrementResult = await DBHelper.Increment("UidGenerator", uidFilter, "index", 1);
                    
                    if (!incrementResult.Success)
                    {
                        Log.GetLogger().Error($"UID生成失败: {userName}");
                        response.Error = ErrorCode.ERR_AccountDataError;
                        return;
                    }
                }

                // 3. 查询生成的UID值
                var uidGenerator = await DBHelper.FindById<BsonDocument>("UidGenerator", 1);
                if (uidGenerator == null)
                {
                    Log.GetLogger().Error($"查询UID生成器失败: {userName}");
                    response.Error = ErrorCode.ERR_AccountDataError;
                    return;
                }

                var newUid = uidGenerator["index"].AsInt32;

                // 4. 创建新账号 - 使用高级API
                var newAccount = new Account
                {
                    UserName = userName,
                    Uid = newUid,
                };

                var insertResult = await DBHelper.InsertOneAsync(newAccount);
                
                if (!insertResult.Success)
                {
                    Log.GetLogger().Error($"账号创建失败: {userName}, UID: {newUid}");
                    response.Error = ErrorCode.ERR_AccountDataError;
                    return;
                }

                Log.GetLogger().Information($"新账号创建成功: {userName}, UID: {newUid}");
                await EnterGame(session, newAccount);
            }
            catch (Exception ex)
            {
                Log.GetLogger().Error(ex, $"登录处理异常: {userName}");
                response.Error = ErrorCode.ERR_AccountDataError;
                return;
            }
        }

        async ETTask EnterGame(Session session, Account account)
        {
            if (session.IsDisposed)
            {
                return;
            }
            
            // TODO 这里只是简单的固定选择一个Game登录, 请实际业务中使用类似于分布式锁和负载均衡的方式来登录Game
            var zoneKey = (1, SceneType.Game);
            var targetGame = StartSceneService.Instance.ZoneScenes[zoneKey][0];

            var gatePlayerCom = session.DomainScene().GetComponent<GatePlayerComponent>();
            var gatePlayer = gatePlayerCom.AddChildWithId<GatePlayer>(account.Uid);
            gatePlayer.Session = session;
            gatePlayer.AddComponent<MailBoxComponent>();

            session.AddComponent<MailBoxComponent, MailboxType>(MailboxType.GateSession);
            var sessionPlayerCom = session.AddComponent<SessionPlayerComponent>();
            sessionPlayerCom.GameSceneInstanceId = targetGame.InstanceId;
            sessionPlayerCom.Account = account;
            
            var request = new G2Game_EnterGameRequest()
            {
                Uid = account.Uid, 
                GateActorId = gatePlayer.InstanceId, 
                ClientSessionId = session.InstanceId
            };
            
            var response = (Game2G_EnterGameResponse)await MessageHelper.CallActor(targetGame.InstanceId, request);
            
            // TODO 可能还要携带一些方便客户端加载场景/初始化的数据?
            var clientMsg = new G2C_EnterGameResult()
            {
                Error = response.Error
            };
            
            if (session.IsDisposed)
            {
                return;
            }
            
            if (response.Error != 0)
            {
                Log.GetLogger().Error($"登录Game失败: {account.UserName} {account.Uid} {response.Error}");
            }
            
            session.Send(clientMsg);
            sessionPlayerCom.PlayerActorId = response.PlayerActorId;
        }
    }
}
