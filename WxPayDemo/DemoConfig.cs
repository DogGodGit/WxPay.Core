using Microsoft.Extensions.Configuration;
using System;
using WxPayAPI.lib;

namespace WxPayDemo
{
    public class DemoConfig : IConfig
    {
        private readonly IConfiguration configuration;

        public DemoConfig(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        //=======【基本信息设置】=====================================
        /* 微信公众号信息配置
        * APPID：绑定支付的APPID（必须配置）
        * MCHID：商户号（必须配置）
        * KEY：商户支付密钥，参考开户邮件设置（必须配置），请妥善保管，避免密钥泄露
        * APPSECRET：公众帐号secert（仅JSAPI支付的时候需要配置），请妥善保管，避免密钥泄露
        */

        /// <summary>
        /// 绑定支付的APPID
        /// </summary>
        /// <returns></returns>
        public string GetAppID()
        {
            return configuration["WeChat:AppID"];
        }

        /// <summary>
        /// 商户号
        /// </summary>
        /// <returns></returns>
        public string GetMchID()
        {
            return configuration["WeChat:MchID"];
        }

        /// <summary>
        /// 商户支付密钥
        /// </summary>
        /// <returns></returns>
        public string GetKey()
        {
            return configuration["WeChat:Key"];
        }

        /// <summary>
        /// 公众帐号secert
        /// </summary>
        /// <returns></returns>
        public string GetAppSecret()
        {
            return configuration["WeChat:Appsecret"];
        }

        //=======【证书路径设置】=====================================
        /* 证书路径,注意应该填写绝对路径（仅退款、撤销订单时需要）
         * 1.证书文件不能放在web服务器虚拟目录，应放在有访问权限控制的目录中，防止被他人下载；
         * 2.建议将证书文件名改为复杂且不容易猜测的文件
         * 3.商户服务器要做好病毒和木马防护工作，不被非法侵入者窃取证书文件。
        */

        /// <summary>
        /// 证书绝对路径
        /// </summary>
        /// <returns></returns>
        public string GetSSlCertPath()
        {
            return configuration["WeChat:SSlCertPath"];
        }

        /// <summary>
        /// 证书密码
        /// </summary>
        /// <returns></returns>
        public string GetSSlCertPassword()
        {
            return configuration["WeChat:SSlCertPassword"];
        }

        /// <summary>
        /// 支付结果通知url
        /// </summary>
        /// <remarks>支付结果通知回调url，用于商户接收支付结果</remarks>
        /// <returns></returns>
        public string GetNotifyUrl()
        {
            return configuration["WeChat:NotifyUrl"];
        }

        /// <summary>
        /// 商户系统后台机器IP
        /// </summary>
        /// <returns></returns>
        public string GetIp()
        {
            return "0.0.0.0";
        }

        /// <summary>
        /// 代理服务器设置
        /// </summary>
        /// <remarks>默认IP和端口号分别为0.0.0.0和0，此时不开启代理（如有需要才设置）</remarks>
        /// <returns></returns>
        public string GetProxyUrl()
        {
            return "";
        }

        /// <summary>
        /// 测速上报等级
        /// </summary>
        /// <returns>0.关闭上报; 1.仅错误时上报; 2.全量上报</returns>
        public int GetReportLevel()
        {
            return Convert.ToInt32(configuration["WeChat:ReportLevel"]);
        }

        /// <summary>
        /// 日志等级
        /// </summary>
        /// <returns>0.不输出日志；1.只输出错误信息; 2.输出错误和正常信息; 3.输出错误信息、正常信息和调试信息</returns>
        public int GetLogLevel()
        {
            return Convert.ToInt32(configuration["WeChat:LogLevel"]);
        }
    }
}