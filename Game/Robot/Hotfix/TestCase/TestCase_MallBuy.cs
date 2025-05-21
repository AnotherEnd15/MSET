using ET.Helper;
using ET.IAP;
using ET.Robot;
using ET.Shop;

namespace ET.TestCase
{
    [RobotTestCase(TestCaseId.MallBuy)]
    public class TestCase_MallBuy : IRobotTestCase
    {
        public async ETTask Run()
        {
            for (int i = 0; i < 100; i++)
            {
                Scene clientScene = await RobotHelper.CreateRobot(i + 1);
                clientScene.GetOrAdd<ChatComponent>();
                await GMHelper.SendGM(clientScene, "UnlockAllModule");
                HandleChat(clientScene).Coroutine();
            }
        }

        async ETTask HandleChat(Scene clientScene)
        {
            List<ShopConfig> configs = new();
            configs.AddRange(ShopConfigCategory.Instance.DataList);
            while (true)
            {
                await TimerComponent.Instance.WaitAsync(1000);
                try
                {
                    if (clientScene.IsDisposed)
                        return;
                    PlayerShopComponent shopCom=clientScene.GetComponent<PlayerShopComponent>();
                    var config = configs.RandomArray();
                    Init(clientScene,new Proto_IAPProduct { 
                        Category = IAPProductType.Shop
                        , ConfigId = (int)config.Id }).Coroutine();

                    if (shopCom != null && !shopCom.IsDisposed)
                    {
                        if (!shopCom.IsCanBuy(config))
                        {
                            configs.Remove(config);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.GetLogger().Error(e);
                }
            }
        }
        async ETTask Init(Scene clientScene,Proto_IAPProduct product)
        {

            //获得iapconfig
            IAPConfig iAPConfig = IAPHelper.GetIAPConfigByProductTypeAndId(product.Category, product.ConfigId);
            if (iAPConfig == null)
            {
                return;
            }

            //如果iap为免费
            if (iAPConfig.Id == IAPType.Free)
            {
                await IAPHelper.BuyOrder(clientScene.GateSession(), product,1);
                return;
            }



            //从ClientScene获取货币组件
            var currencyComponent = clientScene.GetComponent<CurrencyComponent>();
            //获取当前代币数量
            var star = currencyComponent.GetCurrency(CurrencyType.Star);


            //判断代币是否足够,如果够就用代币直接购买
            if (iAPConfig.StarCost > 0 && star >= iAPConfig.StarCost)
            {
                await IAPHelper.BuyOrder(clientScene.GateSession(), product,1);
                return;
            }
            //代币不够,选择支付方式，先获取支付渠道列表
 
            var iAPChannelType =  IAPChannelType.Editor;
            var iAPOrderRet = await IAPHelper.CreateOrder(clientScene.GateSession(), product, iAPChannelType);
            if (iAPOrderRet != 0)
            {
                return;
            }

            var iAPOrder = clientScene.GetComponent<PlayerIAPOrderComponent>().CurrentOrder;
            if (iAPOrder == null)
            {
                return;
            }

            //拉起sdk支付
            var payret = new ProtoIAPPayResultData_Editor();

            //服务器验证订单
            var response = await IAPHelper.PayOrder(clientScene.GateSession(), iAPOrder, payret);
        }

    }
}