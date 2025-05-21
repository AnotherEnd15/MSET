using ET.Proto;
using ET.Server;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            //TODO 如果数据库没这个账号, 创建新账号, 否则读取账号数据信息
            // 根据生成的uid, 查询mongo中此人登录了哪个game, 如果没有, 那就尝试锁定一个game
            // 如果锁定失败, 那就重新查询, 接着前往对应的game登录
            var dbComp = DBManagerComponent.Instance.GetZoneDB(1);
            var database = dbComp.database;
            var userName = request.Account;
            try
            {
                // 1. 尝试读取Account表中id为userName的记录
                var accountCollection = database.GetCollection<BsonDocument>("Account");
                var filter = Builders<BsonDocument>.Filter.Eq("_id", userName);
                var existingAccount = await dbComp.QueryOneNotEntity<Account>(v => v.UserName == userName);

                // 如果记录已存在，直接结束
                if (existingAccount != null)
                {
                    EnterGame(session, existingAccount).Coroutine();
                    return;
                }

                // 2. 尝试对UidGenerator表中id为1的记录的index值+1
                var uidCollection = database.GetCollection<BsonDocument>("UidGenerator");
                var uidFilter = Builders<BsonDocument>.Filter.Eq("_id", 1);
                var update = Builders<BsonDocument>.Update.Inc("index", 1);
                var options = new FindOneAndUpdateOptions<BsonDocument>
                {
                    IsUpsert = true, // 如果记录不存在则创建
                    ReturnDocument = ReturnDocument.After // 返回更新后的文档
                };

                var result = await uidCollection.FindOneAndUpdateAsync(uidFilter, update, options);
                if (result == null)
                {
                    // TODO 服务器内部异常 数据库G了
                    response.Error = ErrorCode.ERR_AccountDataError;
                    return;
                }

                // 提取生成的uid
                var newUid = result["index"].AsInt32;

                // 3. 往Account表中插入新记录
                var newAccount = new Account
                {
                    UserName = userName,
                    Uid = newUid,
                };

                await accountCollection.InsertOneAsync(newAccount.ToBsonDocument<Account>());
                EnterGame(session, newAccount).Coroutine();
            }
            catch (Exception ex)
            {
                Log.GetLogger().Error(ex);
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
            var reqeust = new G2Game_EnterGameRequest()
            {
                Uid = account.Uid, GateActorId= gatePlayer.InstanceId, ClientSessionId = session.InstanceId
            };
            var response =(Game2G_EnterGameResponse) await MessageHelper.CallActor(targetGame.InstanceId, reqeust);
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
