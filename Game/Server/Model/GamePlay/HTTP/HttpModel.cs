/// <summary>
/// 双方服务器的通用返回结构,没有特殊约定的,返回值统一都是这个
/// </summary>
public class CommonHttpResponse
{
    public string ErrorMsg { get; set; } // 如果为空,表示操作OK,否则就是有问题

    public static CommonHttpResponse Error(string content)
    {
        return new CommonHttpResponse()
        {
            ErrorMsg = content
        };
    }
}

public class PlatformHttpConst
{
    public const string PlatformCallbackPath = "/gunkingh5/platform";
}

public class IAP_WechatH5OrderSuccessInfo
{
    public string OrderId { get; set; }
}