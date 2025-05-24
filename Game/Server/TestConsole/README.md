# TestConsole 同步框架测试套件

本项目是一个**同步框架功能测试工程**，使用MSTest框架对Model项目中的SyncFramework进行全面测试。该项目引用Analyzer项目来测试源生成器功能，并引用Model项目来使用其中现有的同步框架代码。

## 项目特点

- ✅ **框架测试**：直接测试Model项目中的SyncFramework
- ✅ **源生成器测试**：引用Analyzer项目，测试源生成器功能
- ✅ **多实体类型**：包含不同复杂度的测试实体
- ✅ **全面覆盖**：基础功能、集合同步、复合场景、性能测试

## 项目结构

```
TestConsole/
├── TestConsole.csproj          # 测试项目配置文件（引用Analyzer和Model）
├── TestPlayer.cs              # 测试实体类定义
├── SyncFrameworkTests.cs      # 基础同步功能测试
├── CollectionSyncTests.cs     # 集合同步测试
├── ComplexSyncTests.cs        # 复合场景测试
├── PerformanceTests.cs        # 性能测试
├── TestSuite.cs              # 测试套件主类
└── README.md                  # 本文档
```

## 核心组件说明

### 1. Model项目中的SyncFramework - 被测试的同步框架
- **同步属性标记**：`SyncClassAttribute`, `SyncPropertyAttribute`
- **同步接口**：`ISyncable`, `ISyncCollection`
- **同步集合**：`SyncList<T>`, `SyncDictionary<TKey, TValue>`
- **序列化器**：`TypeSerializers`
- **辅助工具**：`SyncHelper`

### 2. TestPlayer.cs - 测试实体类
- **TestPlayer**：主要测试实体，包含基本类型和集合属性
- **SimpleEntity**：简单实体，用于基础功能测试
- **CollectionEntity**：集合实体，专门测试集合同步功能

### 3. 测试类说明

#### SyncFrameworkTests.cs - 基础同步功能测试
- 基本类型同步测试（string, int）
- 简单实体同步测试
- TestPlayer脏状态测试
- 序列化基础测试
- 清除脏状态测试

#### CollectionSyncTests.cs - 集合同步测试
- 普通集合同步测试（List, Dictionary）
- 同步集合测试（SyncList, SyncDictionary）
- CollectionEntity同步测试
- Dictionary序列化详细测试
- 空集合序列化测试

#### ComplexSyncTests.cs - 复合场景测试
- 复合场景测试：多种类型混合修改
- 多轮同步测试：连续多次同步操作
- 双向同步测试：两个对象相互同步
- 边界情况测试：空值、空集合等
- 不同实体类型测试

#### PerformanceTests.cs - 性能测试
- 大量数据同步性能测试
- 频繁同步性能测试
- 内存使用测试
- 压力测试：极限数据量
- 不同实体类型性能测试

#### TestSuite.cs - 测试套件主类
- 统一测试入口
- 完整测试套件
- 快速烟雾测试
- 源生成器功能验证
- 测试统计信息

## 测试实体说明

### TestPlayer 类
```csharp
[SyncClass]
public sealed partial class TestPlayer : ISyncable
{
    [SyncProperty] public partial string Name { get; set; }
    [SyncProperty] public partial int Level { get; set; }
    [SyncProperty] public partial List<string> NameList { get; set; }
    [SyncProperty] public partial Dictionary<string, string> NameDict { get; set; }
    [SyncProperty] public partial SyncList<string> SyncNameList { get; set; }
    [SyncProperty] public partial SyncDictionary<string, int> SyncStats { get; set; }
}
```

### SimpleEntity 类
```csharp
[SyncClass]
public sealed partial class SimpleEntity : ISyncable
{
    [SyncProperty] public partial string Id { get; set; }
    [SyncProperty] public partial string Data { get; set; }
}
```

### CollectionEntity 类
```csharp
[SyncClass]
public sealed partial class CollectionEntity : ISyncable
{
    [SyncProperty] public partial List<string> Items { get; set; }
    [SyncProperty] public partial Dictionary<string, int> Scores { get; set; }
    [SyncProperty] public partial SyncList<string> SyncItems { get; set; }
    [SyncProperty] public partial SyncDictionary<string, int> SyncScores { get; set; }
}
```

## 运行测试

### 方式一：Visual Studio
1. 在Visual Studio中打开解决方案
2. 确保Model和Analyzer项目已编译
3. 右键点击TestConsole项目，选择"运行测试"
4. 在测试资源管理器中查看测试结果

### 方式二：命令行
```bash
# 进入TestConsole目录
cd TestConsole

# 先构建依赖项目（确保Model和源生成器可用）
cd ../Model && dotnet build && cd ../Analyzer && dotnet build && cd ../TestConsole

# 运行所有测试
dotnet test

# 运行特定类别的测试
dotnet test --filter "TestCategory=Smoke"          # 运行烟雾测试
dotnet test --filter "TestCategory=Performance"   # 运行性能测试
dotnet test --filter "TestCategory=FullSuite"     # 运行完整测试套件

# 运行特定测试方法
dotnet test --filter "TestMethod=QuickSmokeTest"

# 运行测试并显示详细输出
dotnet test --logger:console;verbosity=detailed
```

### 方式三：指定特定测试类
```bash
# 运行基础功能测试
dotnet test --filter "ClassName=SyncFrameworkTests"

# 运行集合同步测试
dotnet test --filter "ClassName=CollectionSyncTests"

# 运行复合场景测试
dotnet test --filter "ClassName=ComplexSyncTests"

# 运行性能测试
dotnet test --filter "ClassName=PerformanceTests"

# 运行测试套件
dotnet test --filter "ClassName=TestSuite"
```

## 测试分类

### 测试类别标签
- **Smoke**：快速烟雾测试，验证基本功能
- **Performance**：性能测试，可能耗时较长
- **FullSuite**：完整测试套件

### 推荐的测试执行顺序

1. **源生成器验证**：确保源生成器正常工作
   ```bash
   dotnet test --filter "TestMethod=TestSourceGeneratorVerification"
   ```

2. **快速验证**：先运行烟雾测试确保基本功能正常
   ```bash
   dotnet test --filter "TestCategory=Smoke"
   ```

3. **功能测试**：运行具体功能测试
   ```bash
   dotnet test --filter "ClassName=SyncFrameworkTests"
   dotnet test --filter "ClassName=CollectionSyncTests"
   dotnet test --filter "ClassName=ComplexSyncTests"
   ```

4. **性能测试**：最后运行性能测试（可选）
   ```bash
   dotnet test --filter "TestCategory=Performance"
   ```

5. **完整测试**：运行所有测试
   ```bash
   dotnet test --filter "TestCategory=FullSuite"
   ```

## 项目依赖

### 引用的项目
- **Analyzer**：源生成器项目，用于生成同步代码
- **Model**：包含被测试的SyncFramework

### NuGet 包
- Microsoft.NET.Test.Sdk
- MSTest.TestAdapter
- MSTest.TestFramework
- coverlet.collector

## 源生成器集成

本项目引用Analyzer项目作为分析器，引用Model项目获取SyncFramework：

```xml
<ItemGroup>
    <!-- 引用源生成器分析器项目 -->
    <ProjectReference Include="../Analyzer/Share.Analyzer.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
    
    <!-- 引用Model项目以使用现有的同步框架 -->
    <ProjectReference Include="../Model/Model.csproj" />
</ItemGroup>
```

源生成器将为标记了`[SyncClass]`的类自动生成：
- ISyncable接口的实现
- 属性的脏状态跟踪
- 序列化和反序列化代码
- 同步集合的回调设置

## 测试结果解读

### 成功标识
- ✓ 表示测试通过
- 🎉 表示完整测试套件成功

### 失败标识
- ❌ 表示测试失败
- ⚠ 表示警告信息

### 性能指标
- 序列化/反序列化时间
- 内存使用量
- 数据传输大小
- 平均操作时间

## 扩展测试

如需添加新的测试用例：

### 1. 添加新的测试实体
```csharp
[SyncClass]
public sealed partial class YourTestEntity : ISyncable
{
    [SyncProperty]
    public partial string YourProperty { get; set; }
}
```

### 2. 创建对应的测试类
```csharp
[TestClass]
public class YourCustomTests
{
    [TestMethod]
    public void YourTestMethod()
    {
        // 测试代码
    }
}
```

### 3. 在TestSuite中集成
在TestSuite.cs的相应方法中调用新的测试

### 4. 扩展Model中的SyncFramework
如需支持新的数据类型，在Model项目的`TypeSerializers`中添加相应的序列化逻辑。

## 故障排除

### 常见问题

1. **源生成器未工作**
   - 确保Model和Analyzer项目已正确编译
   - 检查分析器引用路径是否正确
   - 重新构建整个解决方案

2. **编译错误**
   - 检查Model和Analyzer项目是否正确构建
   - 确保源生成器生成了必要的代码

3. **测试失败**
   - 查看详细错误信息
   - 确认Model项目中的SyncFramework实现是否正确
   - 检查测试实体的属性标记

4. **性能问题**
   - 调整测试数据量
   - 检查系统资源
   - 分析Model项目中的序列化逻辑

### 调试技巧
- 使用`Console.WriteLine`输出调试信息
- 在Visual Studio中设置断点调试
- 查看测试输出窗口的详细信息
- 使用`GetTestStatistics`方法查看测试覆盖情况

## 版本历史

- **v3.0**：重构为测试Model项目的SyncFramework
  - 移除自实现的同步框架代码
  - 引用Model项目使用其中的SyncFramework
  - 专注于测试现有框架的功能
  - 增强对源生成器的测试覆盖

- **v2.0**：重构为独立测试项目
  - 移除对Model项目的依赖
  - 引用Analyzer项目进行源生成器测试
  - 自实现完整的同步框架基础设施
  - 添加多种测试实体类型
  - 增强测试覆盖范围

- **v1.0**：将原TestConsole控制台项目改写为标准MSTest测试项目

## 贡献

欢迎提交测试用例和改进建议！请确保：
1. 新测试覆盖了重要的功能点
2. 测试命名清晰，易于理解
3. 包含必要的文档说明
4. 遵循现有的代码风格 