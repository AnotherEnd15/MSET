using System.IO;
using YooAsset.Editor;

namespace ETEditor.YooAssets
{
    public class CollectBytes : IFilterRule
    {
        public bool IsCollectAsset(FilterRuleData data)
        {
            return Path.GetExtension(data.AssetPath) == ".bytes";
        }
    }
    
    public class CollectNotBytes : IFilterRule
    {
        public bool IsCollectAsset(FilterRuleData data)
        {
            return Path.GetExtension(data.AssetPath) != ".bytes";
        }
    }
}