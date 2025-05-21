using Sirenix.OdinInspector;

namespace ET
{
    public enum CompareType
    {
        /// <summary>
        /// 等于
        /// </summary>
        [LabelText("==")] EQ = 0,
        /// <summary>
        /// 大于等于
        /// </summary>
        [LabelText(">=")] GE = 1,
        /// <summary>
        /// 大于
        /// </summary>
        [LabelText(">")] GT = 2,
        /// <summary>
        /// 小于等于
        /// </summary>
        [LabelText("<=")] LE = 3,
        /// <summary>
        /// 小于
        /// </summary>
        [LabelText("<")] LT = 4,
        /// <summary>
        /// 不等于
        /// </summary>
        [LabelText("!=")] NE = 5,
    }
}