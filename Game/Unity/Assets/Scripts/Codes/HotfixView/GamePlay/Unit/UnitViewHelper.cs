using UnityEngine;

namespace ET
{
    public static class UnitViewHelper
    {
        public static async ETTask<bool> Wait2Valid(this Unit unit)
        {
            while (!unit.IsDisposed && unit.GetComponent<GameObjectComponent>() == null)
            {
                await TimerComponent.Instance.WaitAsync(100);
            }
            if (unit.IsDisposed)
            {
                return false;
            }

            return true;
        }

        public static void SetUnitRotation(this Unit unit)
        {
            var com = unit.GetComponent<GameObjectComponent>();

            if (unit.Rotation == UnitRotationType.Left)
            {
                com.GameObject.transform.localScale = new Vector3(-1,1,1);
            }
            else
            {
                com.GameObject.transform.localScale = new Vector3(1, 1, 1);
            }
        }

        public static long GetTimeScaleTime(this int origin)
        {
            return (long)(origin / Time.timeScale);
        }
        
        public static long GetTimeScaleTime(this long origin)
        {
            return (long)(origin / Time.timeScale);
        }

        public static float GetTimeScaleTime(this float origin)
        {
            return origin / Time.timeScale;
        }
    }
}