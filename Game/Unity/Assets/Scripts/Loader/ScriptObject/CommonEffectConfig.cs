using Sirenix.OdinInspector;
using UnityEngine;

namespace ET.ScriptObject
{
    [CreateAssetMenu( menuName = "ET/Common/CommonEffectConfig")]
    public class CommonEffectConfig : ScriptableObject
    {
        public GameObject StunEffect;
        public GameObject DeadEffect;

        [LabelText("击飞曲线(Y坐标)")]
        public AnimationCurve FloatCurve;
    }
}