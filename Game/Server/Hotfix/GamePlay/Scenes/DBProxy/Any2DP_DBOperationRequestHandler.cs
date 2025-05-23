using System.Collections.Generic;
using ET.Proto;
using MongoDB.Bson;

namespace ET.DBProxy;

[ActorMessageHandler]
public class Any2DP_DBOperationRequestHandler : AMActorRpcHandler<Scene, Any2DP_DBOperationRequest, DP2Any_DBOperationResponse>
{
    protected override async ETTask Run(Scene actor, Any2DP_DBOperationRequest request, DP2Any_DBOperationResponse response)
    {
        var dbProxy = DBProxyComponent.Instance;
        
        try
        {
            // 直接构建MongoTask，所有字段在DBHelper中已经预处理
            var mongoTask = new MongoTask
            {
                Id = request.Request.Id,
                OperationType = request.Request.OperationType,
                CollectionName = request.Request.CollectionName,
                Document = request.Request.Document,
                Filter = request.Request.Filter,
                Pipeline = request.Request.Pipeline,
                Options = request.Request.Options
            };

            var result = await dbProxy.ExecuteMongoTaskAsync(mongoTask);

            // 正确设置响应
            if (result.Success)
            {
                response.Error = 0;
                response.Message = "";
                response.Response = result.Data ?? new BsonDocument();
            }
            else
            {
                response.Error = 1;
                response.Message = result.ErrorMessage ?? "操作失败";
                response.Response = new BsonDocument(DBFieldNames.Error, result.ErrorMessage ?? "未知错误");
            }
        }
        catch (System.Exception ex)
        {
            response.Error = 1;
            response.Message = ex.Message;
            response.Response = new BsonDocument(DBFieldNames.Error, ex.Message);
        }
    }
}