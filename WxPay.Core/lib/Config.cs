using WxPayAPI.lib;

namespace WxPayAPI
{
    /// <summary>
    /// 配置账号信息
    /// </summary>
    public class WxPayConfig
    {
        public static volatile IConfig config;

        private static readonly object syncRoot = new object();

        public static IConfig GetConfig()
        {
            if (config == null)
            {
                lock (syncRoot)
                {
                    if (config == null)
                        config = new DemoConfig();
                }
            }
            return config;
        }
    }
}