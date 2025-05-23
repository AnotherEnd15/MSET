using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ET.Proto;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace ET.DBProxy;

/// <summary>
/// DBHelper - 基于BsonDocument的底层数据库操作API
/// </summary>
public static partial class DBHelper
{
    /// <summary>
    /// 生成操作ID
    /// </summary>
    public static long GenerateOperationId()
    {
        return RandomGenerator.RandInt64();
    }

    /// <summary>
    /// 发送数据库操作请求到DBProxy进程
    /// </summary>
    private static async Task<BsonDocument> SendDBRequest(DBOperationRequest request)
    {
        var msgRequest = new Any2DP_DBOperationRequest()
        {
            Request = request
        };
        var sceneId = StartSceneService.Instance.GetOneConfigWithHash(1, SceneType.DBProxy, request.Id);
        var msgResponse = (DP2Any_DBOperationResponse)await MessageHelper.CallActor(sceneId, msgRequest);
        
        // 检查错误
        if (msgResponse.Error != 0)
        {
            throw new Exception($"数据库操作失败: {msgResponse.Message}");
        }
        
        return msgResponse.Response;
    }

    #region 插入操作

    /// <summary>
    /// 插入文档
    /// </summary>
    public static async Task<DBInsertResult> Insert<T>(long id, string collectionName, T entity) where T : class
    {
        var request = new DBOperationRequest
        {
            Id = id,
            CollectionName = collectionName,
            OperationType = MongoOperationType.Insert,
            Document = entity.ToBsonDocument()
        };

        var response = await SendDBRequest(request);
        return DBInsertResult.FromBsonDocument(response);
    }

    /// <summary>
    /// 插入BsonDocument
    /// </summary>
    public static async Task<DBInsertResult> InsertDocument(long id, string collectionName, BsonDocument document)
    {
        var request = new DBOperationRequest
        {
            Id = id,
            CollectionName = collectionName,
            OperationType = MongoOperationType.Insert,
            Document = document
        };

        var response = await SendDBRequest(request);
        return DBInsertResult.FromBsonDocument(response);
    }

    /// <summary>
    /// 插入文档（自动生成ID）
    /// </summary>
    public static async Task<DBInsertResult> Insert<T>(string collectionName, T entity) where T : class
    {
        return await Insert(GenerateOperationId(), collectionName, entity);
    }

    /// <summary>
    /// 插入BsonDocument（自动生成ID）
    /// </summary>
    public static async Task<DBInsertResult> InsertDocument(string collectionName, BsonDocument document)
    {
        return await InsertDocument(GenerateOperationId(), collectionName, document);
    }

    #endregion

    #region 查询操作

    /// <summary>
    /// 根据ID查找文档
    /// </summary>
    public static async Task<T> FindById<T>(long id, string collectionName, object entityId) where T : class
    {
        var request = new DBOperationRequest
        {
            Id = id,
            CollectionName = collectionName,
            OperationType = MongoOperationType.Find,
            Filter = new BsonDocument(DBFieldNames.Id, BsonValue.Create(entityId))
        };

        var response = await SendDBRequest(request);
        if (response == null || response.ElementCount == 0)
            return null;

        return BsonSerializer.Deserialize<T>(response);
    }

    /// <summary>
    /// 根据ID查找文档（自动生成操作ID）
    /// </summary>
    public static async Task<T> FindById<T>(string collectionName, object entityId) where T : class
    {
        return await FindById<T>(GenerateOperationId(), collectionName, entityId);
    }

    /// <summary>
    /// 根据条件查找单个文档
    /// </summary>
    public static async Task<T> FindOne<T>(long id, string collectionName, BsonDocument filter) where T : class
    {
        var request = new DBOperationRequest
        {
            Id = id,
            CollectionName = collectionName,
            OperationType = MongoOperationType.Find,
            Filter = filter
        };

        var response = await SendDBRequest(request);
        if (response == null || response.ElementCount == 0)
            return null;

        return BsonSerializer.Deserialize<T>(response);
    }

    /// <summary>
    /// 根据条件查找单个文档（自动生成操作ID）
    /// </summary>
    public static async Task<T> FindOne<T>(string collectionName, BsonDocument filter) where T : class
    {
        return await FindOne<T>(GenerateOperationId(), collectionName, filter);
    }

    /// <summary>
    /// 根据条件查找多个文档
    /// </summary>
    public static async Task<DBFindManyResult<T>> FindMany<T>(long id, string collectionName, BsonDocument filter = null) where T : class
    {
        var request = new DBOperationRequest
        {
            Id = id,
            CollectionName = collectionName,
            OperationType = MongoOperationType.FindMany,
            Filter = filter ?? new BsonDocument()
        };

        var response = await SendDBRequest(request);
        var findResult = new DBFindManyResult<T>();
        
        if (response != null && response.Contains(DBFieldNames.Documents))
        {
            var documents = response[DBFieldNames.Documents].AsBsonArray;
            foreach (var doc in documents)
            {
                findResult.Documents.Add(BsonSerializer.Deserialize<T>(doc.AsBsonDocument));
            }
        }

        return findResult;
    }

    /// <summary>
    /// 根据条件查找多个文档（自动生成操作ID）
    /// </summary>
    public static async Task<DBFindManyResult<T>> FindMany<T>(string collectionName, BsonDocument filter = null) where T : class
    {
        return await FindMany<T>(GenerateOperationId(), collectionName, filter);
    }

    #endregion

    #region 更新操作

    /// <summary>
    /// 更新文档
    /// </summary>
    public static async Task<DBUpdateResult> Update<T>(long id, string collectionName, BsonDocument filter, T entity) where T : class
    {
        var request = new DBOperationRequest
        {
            Id = id,
            CollectionName = collectionName,
            OperationType = MongoOperationType.Update,
            Filter = filter,
            Document = new BsonDocument(DBFieldNames.Set, entity.ToBsonDocument())
        };

        var response = await SendDBRequest(request);
        return DBUpdateResult.FromBsonDocument(response);
    }

    /// <summary>
    /// 更新文档（自动生成操作ID）
    /// </summary>
    public static async Task<DBUpdateResult> Update<T>(string collectionName, BsonDocument filter, T entity) where T : class
    {
        return await Update(GenerateOperationId(), collectionName, filter, entity);
    }

    /// <summary>
    /// 根据ID更新文档
    /// </summary>
    public static async Task<DBUpdateResult> UpdateById<T>(long id, string collectionName, object entityId, T entity) where T : class
    {
        var request = new DBOperationRequest
        {
            Id = id,
            CollectionName = collectionName,
            OperationType = MongoOperationType.Update,
            Filter = new BsonDocument(DBFieldNames.Id, BsonValue.Create(entityId)),
            Document = new BsonDocument(DBFieldNames.Set, entity.ToBsonDocument())
        };

        var response = await SendDBRequest(request);
        return DBUpdateResult.FromBsonDocument(response);
    }

    /// <summary>
    /// 根据ID更新文档（自动生成操作ID）
    /// </summary>
    public static async Task<DBUpdateResult> UpdateById<T>(string collectionName, object entityId, T entity) where T : class
    {
        return await UpdateById(GenerateOperationId(), collectionName, entityId, entity);
    }

    /// <summary>
    /// 部分更新文档
    /// </summary>
    public static async Task<DBUpdateResult> UpdatePartial(long id, string collectionName, BsonDocument filter, BsonDocument updateFields)
    {
        var request = new DBOperationRequest
        {
            Id = id,
            CollectionName = collectionName,
            OperationType = MongoOperationType.Update,
            Filter = filter,
            Document = new BsonDocument(DBFieldNames.Set, updateFields)
        };

        var response = await SendDBRequest(request);
        return DBUpdateResult.FromBsonDocument(response);
    }

    /// <summary>
    /// 部分更新文档（自动生成操作ID）
    /// </summary>
    public static async Task<DBUpdateResult> UpdatePartial(string collectionName, BsonDocument filter, BsonDocument updateFields)
    {
        return await UpdatePartial(GenerateOperationId(), collectionName, filter, updateFields);
    }

    /// <summary>
    /// 原子增量操作
    /// </summary>
    public static async Task<DBUpdateResult> Increment(long id, string collectionName, BsonDocument filter, string fieldName, long incrementValue)
    {
        var request = new DBOperationRequest
        {
            Id = id,
            CollectionName = collectionName,
            OperationType = MongoOperationType.Update,
            Filter = filter,
            Document = new BsonDocument(DBFieldNames.Inc, new BsonDocument(fieldName, incrementValue))
        };

        var response = await SendDBRequest(request);
        return DBUpdateResult.FromBsonDocument(response);
    }

    /// <summary>
    /// 原子增量操作（自动生成操作ID）
    /// </summary>
    public static async Task<DBUpdateResult> Increment(string collectionName, BsonDocument filter, string fieldName, long incrementValue)
    {
        return await Increment(GenerateOperationId(), collectionName, filter, fieldName, incrementValue);
    }

    #endregion

    #region 删除操作

    /// <summary>
    /// 删除文档
    /// </summary>
    public static async Task<DBDeleteResult> Delete(long id, string collectionName, BsonDocument filter)
    {
        var request = new DBOperationRequest
        {
            Id = id,
            CollectionName = collectionName,
            OperationType = MongoOperationType.Delete,
            Filter = filter
        };

        var response = await SendDBRequest(request);
        return DBDeleteResult.FromBsonDocument(response);
    }

    /// <summary>
    /// 删除文档（自动生成操作ID）
    /// </summary>
    public static async Task<DBDeleteResult> Delete(string collectionName, BsonDocument filter)
    {
        return await Delete(GenerateOperationId(), collectionName, filter);
    }

    /// <summary>
    /// 根据ID删除文档
    /// </summary>
    public static async Task<DBDeleteResult> DeleteById(long id, string collectionName, object entityId)
    {
        var request = new DBOperationRequest
        {
            Id = id,
            CollectionName = collectionName,
            OperationType = MongoOperationType.Delete,
            Filter = new BsonDocument(DBFieldNames.Id, BsonValue.Create(entityId))
        };

        var response = await SendDBRequest(request);
        return DBDeleteResult.FromBsonDocument(response);
    }

    /// <summary>
    /// 根据ID删除文档（自动生成操作ID）
    /// </summary>
    public static async Task<DBDeleteResult> DeleteById(string collectionName, object entityId)
    {
        return await DeleteById(GenerateOperationId(), collectionName, entityId);
    }

    #endregion

    #region 高级操作

    /// <summary>
    /// 查找并更新文档
    /// </summary>
    public static async Task<T> FindOneAndUpdate<T>(long id, string collectionName, BsonDocument filter, T entity, bool returnUpdatedDocument = true) where T : class
    {
        var request = new DBOperationRequest
        {
            Id = id,
            CollectionName = collectionName,
            OperationType = MongoOperationType.FindOneAndUpdate,
            Filter = filter,
            Document = new BsonDocument(DBFieldNames.Set, entity.ToBsonDocument()),
            Options = new BsonDocument(DBFieldNames.ReturnUpdatedDocument, returnUpdatedDocument)
        };

        var response = await SendDBRequest(request);
        if (response == null || response.ElementCount == 0)
            return null;

        return BsonSerializer.Deserialize<T>(response);
    }

    /// <summary>
    /// 查找并更新文档（自动生成操作ID）
    /// </summary>
    public static async Task<T> FindOneAndUpdate<T>(string collectionName, BsonDocument filter, T entity, bool returnUpdatedDocument = true) where T : class
    {
        return await FindOneAndUpdate(GenerateOperationId(), collectionName, filter, entity, returnUpdatedDocument);
    }

    /// <summary>
    /// 插入文档如果不存在
    /// </summary>
    public static async Task<DBInsertResult> InsertIfNotExists<T>(long id, string collectionName, BsonDocument filter, T entity) where T : class
    {
        var request = new DBOperationRequest
        {
            Id = id,
            CollectionName = collectionName,
            OperationType = MongoOperationType.InsertIfNotExists,
            Filter = filter,
            Document = entity.ToBsonDocument()
        };

        var response = await SendDBRequest(request);
        return DBInsertResult.FromBsonDocument(response);
    }

    /// <summary>
    /// 插入文档如果不存在（自动生成操作ID）
    /// </summary>
    public static async Task<DBInsertResult> InsertIfNotExists<T>(string collectionName, BsonDocument filter, T entity) where T : class
    {
        return await InsertIfNotExists(GenerateOperationId(), collectionName, filter, entity);
    }

    /// <summary>
    /// 聚合查询
    /// </summary>
    public static async Task<List<T>> Aggregate<T>(long id, string collectionName, BsonDocument[] pipeline) where T : class
    {
        var request = new DBOperationRequest
        {
            Id = id,
            CollectionName = collectionName,
            OperationType = MongoOperationType.Aggregate,
            Pipeline = pipeline
        };

        var response = await SendDBRequest(request);
        var resultList = new List<T>();
        
        if (response != null && response.Contains(DBFieldNames.Results))
        {
            var documents = response[DBFieldNames.Results].AsBsonArray;
            foreach (var doc in documents)
            {
                resultList.Add(BsonSerializer.Deserialize<T>(doc.AsBsonDocument));
            }
        }

        return resultList;
    }

    /// <summary>
    /// 聚合查询（自动生成操作ID）
    /// </summary>
    public static async Task<List<T>> Aggregate<T>(string collectionName, BsonDocument[] pipeline) where T : class
    {
        return await Aggregate<T>(GenerateOperationId(), collectionName, pipeline);
    }

    /// <summary>
    /// 计数查询
    /// </summary>
    public static async Task<DBCountResult> Count(long id, string collectionName, BsonDocument filter = null)
    {
        var pipeline = new BsonDocument[]
        {
            new BsonDocument(DBFieldNames.Match, filter ?? new BsonDocument()),
            new BsonDocument("$count", DBFieldNames.Count)
        };

        var request = new DBOperationRequest
        {
            Id = id,
            CollectionName = collectionName,
            OperationType = MongoOperationType.Aggregate,
            Pipeline = pipeline
        };

        var response = await SendDBRequest(request);
        return DBCountResult.FromBsonDocument(response);
    }

    /// <summary>
    /// 计数查询（自动生成操作ID）
    /// </summary>
    public static async Task<DBCountResult> Count(string collectionName, BsonDocument filter = null)
    {
        return await Count(GenerateOperationId(), collectionName, filter);
    }

    /// <summary>
    /// 分页查询
    /// </summary>
    public static async Task<DBPagedResult<T>> FindPaged<T>(long id, string collectionName, BsonDocument filter, int skip, int limit, BsonDocument sort = null) where T : class
    {
        var pipelineList = new List<BsonDocument>
        {
            new BsonDocument(DBFieldNames.Match, filter ?? new BsonDocument())
        };
        
        if (sort != null)
        {
            pipelineList.Add(new BsonDocument(DBFieldNames.Sort, sort));
        }
        
        pipelineList.Add(new BsonDocument(DBFieldNames.Skip, skip));
        pipelineList.Add(new BsonDocument(DBFieldNames.Limit, limit));

        var request = new DBOperationRequest
        {
            Id = id,
            CollectionName = collectionName,
            OperationType = MongoOperationType.Aggregate,
            Pipeline = pipelineList.ToArray()
        };

        var response = await SendDBRequest(request);
        var pagedResult = new DBPagedResult<T>
        {
            Skip = skip,
            Limit = limit
        };

        if (response != null && response.Contains(DBFieldNames.Results))
        {
            var documents = response[DBFieldNames.Results].AsBsonArray;
            foreach (var doc in documents)
            {
                pagedResult.Documents.Add(BsonSerializer.Deserialize<T>(doc.AsBsonDocument));
            }
        }

        return pagedResult;
    }

    /// <summary>
    /// 分页查询（自动生成操作ID）
    /// </summary>
    public static async Task<DBPagedResult<T>> FindPaged<T>(string collectionName, BsonDocument filter, int skip, int limit, BsonDocument sort = null) where T : class
    {
        return await FindPaged<T>(GenerateOperationId(), collectionName, filter, skip, limit, sort);
    }

    /// <summary>
    /// 执行命令
    /// </summary>
    public static async Task<BsonDocument> RunCommand(long id, BsonDocument command)
    {
        var request = new DBOperationRequest
        {
            Id = id,
            OperationType = MongoOperationType.RunCommand,
            Document = command
        };

        var response = await SendDBRequest(request);
        return response ?? new BsonDocument();
    }

    /// <summary>
    /// 执行命令（自动生成操作ID）
    /// </summary>
    public static async Task<BsonDocument> RunCommand(BsonDocument command)
    {
        return await RunCommand(GenerateOperationId(), command);
    }

    #endregion

    #region 便利方法

    /// <summary>
    /// 根据条件检查文档是否存在
    /// </summary>
    public static async Task<bool> Exists(string collectionName, BsonDocument filter)
    {
        var result = await FindOne<BsonDocument>(collectionName, filter);
        return result != null;
    }

    /// <summary>
    /// 根据ID检查文档是否存在
    /// </summary>
    public static async Task<bool> ExistsById(string collectionName, object entityId)
    {
        var result = await FindById<BsonDocument>(collectionName, entityId);
        return result != null;
    }

    /// <summary>
    /// 批量插入
    /// </summary>
    public static async Task<List<DBInsertResult>> InsertMany<T>(string collectionName, List<T> entities) where T : class
    {
        var results = new List<DBInsertResult>();
        foreach (var entity in entities)
        {
            var result = await Insert(collectionName, entity);
            results.Add(result);
        }
        return results;
    }

    /// <summary>
    /// 批量删除
    /// </summary>
    public static async Task<List<DBDeleteResult>> DeleteMany(string collectionName, List<BsonDocument> filters)
    {
        var results = new List<DBDeleteResult>();
        foreach (var filter in filters)
        {
            var result = await Delete(collectionName, filter);
            results.Add(result);
        }
        return results;
    }

    #endregion
} 