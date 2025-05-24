using System.Collections.Generic;
using SyncFramework;

namespace TestConsole
{
    /// <summary>
    /// 测试用的Player类
    /// 使用SyncClass属性标记，源生成器将为此类生成同步代码
    /// </summary>
    [SyncClass]
    public sealed partial class TestPlayer
    {
        // 基本类型属性
        [SyncProperty]
        public partial string Name { get; set; }

        [SyncProperty]
        public partial int Level { get; set; }

        // 普通集合属性（整体替换同步）
        [SyncProperty]
        public partial List<string> NameList { get; set; }

        [SyncProperty]
        public partial Dictionary<string, string> NameDict { get; set; }

        // 同步集合属性（增量同步）
        [SyncProperty]
        public partial SyncList<string> SyncNameList { get; set; }

        [SyncProperty]
        public partial SyncDictionary<string, int> SyncStats { get; set; }

        // 构造函数 - 不要在这里给属性赋值，避免触发脏状态
        public TestPlayer()
        {
            // 不在构造函数中设置属性值，保持初始的非脏状态
            // 属性会自动初始化为null或默认值
            
            // 如果需要初始化集合，应该在使用前按需初始化
            // 或者提供专门的初始化方法
        }

        /// <summary>
        /// 初始化方法 - 需要时手动调用来初始化集合
        /// </summary>
        public void InitializeCollections()
        {
            if (NameList == null)
                NameList = new List<string>();
            if (NameDict == null)
                NameDict = new Dictionary<string, string>();
            if (SyncNameList == null)
                SyncNameList = new SyncList<string>();
            if (SyncStats == null)
                SyncStats = new SyncDictionary<string, int>();
        }
    }

    /// <summary>
    /// 简单的测试实体类，用于测试基本同步功能
    /// </summary>
    [SyncClass]
    public sealed partial class SimpleEntity
    {
        [SyncProperty]
        public partial string Id { get; set; }

        [SyncProperty]
        public partial string Data { get; set; }

        public SimpleEntity()
        {
            // 不在构造函数中设置属性值
        }

        public SimpleEntity(string id, string data)
        {
            Id = id;
            Data = data;
        }
    }

    /// <summary>
    /// 集合测试实体类
    /// </summary>
    [SyncClass]
    public sealed partial class CollectionEntity
    {
        [SyncProperty]
        public partial List<string> Items { get; set; }

        [SyncProperty]
        public partial Dictionary<string, int> Scores { get; set; }

        [SyncProperty]
        public partial SyncList<string> SyncItems { get; set; }

        [SyncProperty]
        public partial SyncDictionary<string, int> SyncScores { get; set; }

        public CollectionEntity()
        {
            // 不在构造函数中设置属性值
        }

        /// <summary>
        /// 初始化集合
        /// </summary>
        public void InitializeCollections()
        {
            if (Items == null)
                Items = new List<string>();
            if (Scores == null)
                Scores = new Dictionary<string, int>();
            if (SyncItems == null)
                SyncItems = new SyncList<string>();
            if (SyncScores == null)
                SyncScores = new SyncDictionary<string, int>();
        }
    }
} 