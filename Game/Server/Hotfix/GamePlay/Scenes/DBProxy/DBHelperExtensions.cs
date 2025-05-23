using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace ET.DBProxy;

/// <summary>
/// DBHelper的高级泛型API - 提供类似MongoDB官方驱动的易用接口
/// 基于Lambda表达式，自动处理集合名和类型转换
/// </summary>
public static partial class DBHelper
{
    /// <summary>
    /// 获取实体的集合名（默认使用类名）
    /// </summary>
    private static string GetCollectionName<T>()
    {
        return typeof(T).Name;
    }

    /// <summary>
    /// 将Lambda表达式转换为BsonDocument过滤器
    /// </summary>
    private static BsonDocument ConvertExpressionToBson<T>(Expression<Func<T, bool>> expression)
    {
        try
        {
            // 使用MongoDB官方驱动的表达式转换
            var filterDefinition = Builders<T>.Filter.Where(expression);
            var serializer = BsonSerializer.SerializerRegistry.GetSerializer<T>();
            var renderArgs = new RenderArgs<T>(serializer, BsonSerializer.SerializerRegistry);
            return filterDefinition.Render(renderArgs);
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"无法转换表达式为BSON过滤器: {ex.Message}", nameof(expression), ex);
        }
    }

    #region 高级插入操作

    /// <summary>
    /// 插入单个实体（自动推断集合名）
    /// </summary>
    public static async Task<DBInsertResult> InsertOneAsync<T>(T entity) where T : class
    {
        var collectionName = GetCollectionName<T>();
        return await Insert(collectionName, entity);
    }

    /// <summary>
    /// 插入单个实体（指定集合名）
    /// </summary>
    public static async Task<DBInsertResult> InsertOneAsync<T>(T entity, string collectionName) where T : class
    {
        return await Insert(collectionName, entity);
    }

    /// <summary>
    /// 批量插入实体（自动推断集合名）
    /// </summary>
    public static async Task<List<DBInsertResult>> InsertManyAsync<T>(IEnumerable<T> entities) where T : class
    {
        var collectionName = GetCollectionName<T>();
        var results = new List<DBInsertResult>();
        foreach (var entity in entities)
        {
            var result = await Insert(collectionName, entity);
            results.Add(result);
        }
        return results;
    }

    /// <summary>
    /// 批量插入实体（指定集合名）
    /// </summary>
    public static async Task<List<DBInsertResult>> InsertManyAsync<T>(IEnumerable<T> entities, string collectionName) where T : class
    {
        var results = new List<DBInsertResult>();
        foreach (var entity in entities)
        {
            var result = await Insert(collectionName, entity);
            results.Add(result);
        }
        return results;
    }

    #endregion

    #region 高级查询操作

    /// <summary>
    /// 根据ID查找实体（自动推断集合名）
    /// </summary>
    public static async Task<T> FindByIdAsync<T>(object id) where T : class
    {
        var collectionName = GetCollectionName<T>();
        return await FindById<T>(collectionName, id);
    }

    /// <summary>
    /// 根据ID查找实体（指定集合名）
    /// </summary>
    public static async Task<T> FindByIdAsync<T>(object id, string collectionName) where T : class
    {
        return await FindById<T>(collectionName, id);
    }

    /// <summary>
    /// 根据Lambda表达式查找单个实体（自动推断集合名）
    /// </summary>
    public static async Task<T> FindOneAsync<T>(Expression<Func<T, bool>> filter) where T : class
    {
        var collectionName = GetCollectionName<T>();
        var bsonFilter = ConvertExpressionToBson(filter);
        return await FindOne<T>(collectionName, bsonFilter);
    }

    /// <summary>
    /// 根据Lambda表达式查找单个实体（指定集合名）
    /// </summary>
    public static async Task<T> FindOneAsync<T>(Expression<Func<T, bool>> filter, string collectionName) where T : class
    {
        var bsonFilter = ConvertExpressionToBson(filter);
        return await FindOne<T>(collectionName, bsonFilter);
    }

    /// <summary>
    /// 根据BsonDocument条件查找单个实体（自动推断集合名）
    /// </summary>
    public static async Task<T> FindOneAsync<T>(BsonDocument filter) where T : class
    {
        var collectionName = GetCollectionName<T>();
        return await FindOne<T>(collectionName, filter);
    }

    /// <summary>
    /// 根据Lambda表达式查找多个实体（自动推断集合名）
    /// </summary>
    public static async Task<List<T>> FindAsync<T>(Expression<Func<T, bool>> filter) where T : class
    {
        var collectionName = GetCollectionName<T>();
        var bsonFilter = ConvertExpressionToBson(filter);
        var result = await FindMany<T>(collectionName, bsonFilter);
        return result.Documents;
    }

    /// <summary>
    /// 根据Lambda表达式查找多个实体（指定集合名）
    /// </summary>
    public static async Task<List<T>> FindAsync<T>(Expression<Func<T, bool>> filter, string collectionName) where T : class
    {
        var bsonFilter = ConvertExpressionToBson(filter);
        var result = await FindMany<T>(collectionName, bsonFilter);
        return result.Documents;
    }

    /// <summary>
    /// 根据BsonDocument条件查找多个实体（自动推断集合名）
    /// </summary>
    public static async Task<List<T>> FindAsync<T>(BsonDocument filter = null) where T : class
    {
        var collectionName = GetCollectionName<T>();
        var result = await FindMany<T>(collectionName, filter);
        return result.Documents;
    }

    /// <summary>
    /// 查找所有实体（自动推断集合名）
    /// </summary>
    public static async Task<List<T>> FindAllAsync<T>() where T : class
    {
        return await FindAsync<T>(new BsonDocument());
    }

    /// <summary>
    /// 分页查询（Lambda表达式，自动推断集合名）
    /// </summary>
    public static async Task<List<T>> FindAsync<T>(Expression<Func<T, bool>> filter, int skip, int limit, BsonDocument sort = null) where T : class
    {
        var collectionName = GetCollectionName<T>();
        var bsonFilter = ConvertExpressionToBson(filter);
        var result = await FindPaged<T>(collectionName, bsonFilter, skip, limit, sort);
        return result.Documents;
    }

    /// <summary>
    /// 分页查询（BsonDocument，自动推断集合名）
    /// </summary>
    public static async Task<List<T>> FindAsync<T>(BsonDocument filter, int skip, int limit, BsonDocument sort = null) where T : class
    {
        var collectionName = GetCollectionName<T>();
        var result = await FindPaged<T>(collectionName, filter, skip, limit, sort);
        return result.Documents;
    }

    #endregion

    #region 高级更新操作

    /// <summary>
    /// 根据ID更新实体（自动推断集合名）
    /// </summary>
    public static async Task<DBUpdateResult> UpdateByIdAsync<T>(object id, T entity) where T : class
    {
        var collectionName = GetCollectionName<T>();
        return await UpdateById(collectionName, id, entity);
    }

    /// <summary>
    /// 根据ID更新实体（指定集合名）
    /// </summary>
    public static async Task<DBUpdateResult> UpdateByIdAsync<T>(object id, T entity, string collectionName) where T : class
    {
        return await UpdateById(collectionName, id, entity);
    }

    /// <summary>
    /// 根据Lambda表达式更新单个实体（自动推断集合名）
    /// </summary>
    public static async Task<DBUpdateResult> UpdateOneAsync<T>(Expression<Func<T, bool>> filter, T entity) where T : class
    {
        var collectionName = GetCollectionName<T>();
        var bsonFilter = ConvertExpressionToBson(filter);
        return await Update(collectionName, bsonFilter, entity);
    }

    /// <summary>
    /// 根据Lambda表达式更新单个实体（指定集合名）
    /// </summary>
    public static async Task<DBUpdateResult> UpdateOneAsync<T>(Expression<Func<T, bool>> filter, T entity, string collectionName) where T : class
    {
        var bsonFilter = ConvertExpressionToBson(filter);
        return await Update(collectionName, bsonFilter, entity);
    }

    /// <summary>
    /// 根据Lambda表达式部分更新实体（自动推断集合名）
    /// </summary>
    public static async Task<DBUpdateResult> UpdateOneAsync<T>(Expression<Func<T, bool>> filter, BsonDocument updateFields) where T : class
    {
        var collectionName = GetCollectionName<T>();
        var bsonFilter = ConvertExpressionToBson(filter);
        return await UpdatePartial(collectionName, bsonFilter, updateFields);
    }

    /// <summary>
    /// 根据Lambda表达式部分更新实体（指定集合名）
    /// </summary>
    public static async Task<DBUpdateResult> UpdateOneAsync<T>(Expression<Func<T, bool>> filter, BsonDocument updateFields, string collectionName) where T : class
    {
        var bsonFilter = ConvertExpressionToBson(filter);
        return await UpdatePartial(collectionName, bsonFilter, updateFields);
    }

    /// <summary>
    /// 查找并更新实体（Lambda表达式，自动推断集合名）
    /// </summary>
    public static async Task<T> FindOneAndUpdateAsync<T>(Expression<Func<T, bool>> filter, T entity, bool returnUpdatedDocument = true) where T : class
    {
        var collectionName = GetCollectionName<T>();
        var bsonFilter = ConvertExpressionToBson(filter);
        return await FindOneAndUpdate(collectionName, bsonFilter, entity, returnUpdatedDocument);
    }

    /// <summary>
    /// 查找并更新实体（Lambda表达式，指定集合名）
    /// </summary>
    public static async Task<T> FindOneAndUpdateAsync<T>(Expression<Func<T, bool>> filter, T entity, string collectionName, bool returnUpdatedDocument = true) where T : class
    {
        var bsonFilter = ConvertExpressionToBson(filter);
        return await FindOneAndUpdate(collectionName, bsonFilter, entity, returnUpdatedDocument);
    }

    #endregion

    #region 高级删除操作

    /// <summary>
    /// 根据ID删除实体（自动推断集合名）
    /// </summary>
    public static async Task<DBDeleteResult> DeleteByIdAsync<T>(object id) where T : class
    {
        var collectionName = GetCollectionName<T>();
        return await DeleteById(collectionName, id);
    }

    /// <summary>
    /// 根据ID删除实体（指定集合名）
    /// </summary>
    public static async Task<DBDeleteResult> DeleteByIdAsync<T>(object id, string collectionName) where T : class
    {
        return await DeleteById(collectionName, id);
    }

    /// <summary>
    /// 根据Lambda表达式删除单个实体（自动推断集合名）
    /// </summary>
    public static async Task<DBDeleteResult> DeleteOneAsync<T>(Expression<Func<T, bool>> filter) where T : class
    {
        var collectionName = GetCollectionName<T>();
        var bsonFilter = ConvertExpressionToBson(filter);
        return await Delete(collectionName, bsonFilter);
    }

    /// <summary>
    /// 根据Lambda表达式删除单个实体（指定集合名）
    /// </summary>
    public static async Task<DBDeleteResult> DeleteOneAsync<T>(Expression<Func<T, bool>> filter, string collectionName) where T : class
    {
        var bsonFilter = ConvertExpressionToBson(filter);
        return await Delete(collectionName, bsonFilter);
    }

    #endregion

    #region 高级聚合和统计

    /// <summary>
    /// 统计符合Lambda表达式条件的文档数量（自动推断集合名）
    /// </summary>
    public static async Task<long> CountDocumentsAsync<T>(Expression<Func<T, bool>> filter) where T : class
    {
        var collectionName = GetCollectionName<T>();
        var bsonFilter = ConvertExpressionToBson(filter);
        var result = await Count(collectionName, bsonFilter);
        return result.Count;
    }

    /// <summary>
    /// 统计符合Lambda表达式条件的文档数量（指定集合名）
    /// </summary>
    public static async Task<long> CountDocumentsAsync<T>(Expression<Func<T, bool>> filter, string collectionName) where T : class
    {
        var bsonFilter = ConvertExpressionToBson(filter);
        var result = await Count(collectionName, bsonFilter);
        return result.Count;
    }

    /// <summary>
    /// 统计所有文档数量（自动推断集合名）
    /// </summary>
    public static async Task<long> CountDocumentsAsync<T>() where T : class
    {
        var collectionName = GetCollectionName<T>();
        var result = await Count(collectionName);
        return result.Count;
    }

    /// <summary>
    /// 检查实体是否存在（Lambda表达式，自动推断集合名）
    /// </summary>
    public static async Task<bool> ExistsAsync<T>(Expression<Func<T, bool>> filter) where T : class
    {
        var collectionName = GetCollectionName<T>();
        var bsonFilter = ConvertExpressionToBson(filter);
        return await Exists(collectionName, bsonFilter);
    }

    /// <summary>
    /// 检查实体是否存在（Lambda表达式，指定集合名）
    /// </summary>
    public static async Task<bool> ExistsAsync<T>(Expression<Func<T, bool>> filter, string collectionName) where T : class
    {
        var bsonFilter = ConvertExpressionToBson(filter);
        return await Exists(collectionName, bsonFilter);
    }

    /// <summary>
    /// 根据ID检查实体是否存在（自动推断集合名）
    /// </summary>
    public static async Task<bool> ExistsByIdAsync<T>(object id) where T : class
    {
        var collectionName = GetCollectionName<T>();
        return await ExistsById(collectionName, id);
    }

    /// <summary>
    /// 根据ID检查实体是否存在（指定集合名）
    /// </summary>
    public static async Task<bool> ExistsByIdAsync<T>(object id, string collectionName) where T : class
    {
        return await ExistsById(collectionName, id);
    }

    #endregion

    #region 高级条件插入

    /// <summary>
    /// 插入实体如果不存在（Lambda表达式，自动推断集合名）
    /// </summary>
    public static async Task<DBInsertResult> InsertIfNotExistsAsync<T>(Expression<Func<T, bool>> filter, T entity) where T : class
    {
        var collectionName = GetCollectionName<T>();
        var bsonFilter = ConvertExpressionToBson(filter);
        return await InsertIfNotExists(collectionName, bsonFilter, entity);
    }

    /// <summary>
    /// 插入实体如果不存在（Lambda表达式，指定集合名）
    /// </summary>
    public static async Task<DBInsertResult> InsertIfNotExistsAsync<T>(Expression<Func<T, bool>> filter, T entity, string collectionName) where T : class
    {
        var bsonFilter = ConvertExpressionToBson(filter);
        return await InsertIfNotExists(collectionName, bsonFilter, entity);
    }

    /// <summary>
    /// 插入实体如果不存在（BsonDocument，自动推断集合名）
    /// </summary>
    public static async Task<DBInsertResult> InsertIfNotExistsAsync<T>(BsonDocument filter, T entity) where T : class
    {
        var collectionName = GetCollectionName<T>();
        return await InsertIfNotExists(collectionName, filter, entity);
    }

    #endregion
} 