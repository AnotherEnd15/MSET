using UnityEngine.UI;

namespace ET
{
    public class UILogin
    {
        public Slider LoadingProgress;

        public UILogin(Slider slider)
        {
            this.LoadingProgress = slider;
            LoadingProgress.value = 0;
        }
    }
}