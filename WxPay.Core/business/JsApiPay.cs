using LitJson;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Web;

namespace WxPayAPI
{
    public class JsApiPay
    {
        /// <summary>
        /// 获取 HTTP 请求上下文
        /// </summary>
        private readonly IHttpContextAccessor _httpContext;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="httpContextAccessor"></param>
        public JsApiPay(IHttpContextAccessor httpContextAccessor)
        {
            _httpContext = httpContextAccessor;
        }

        /// <summary>
        /// openid用于调用统一下单接口
        /// </summary>
        public string Openid { get; set; }

        /// <summary>
        /// access_token用于获取收货地址js函数入口参数
        /// </summary>
        public string Access_Token { get; set; }

        /// <summary>
        /// 商品金额，用于统一下单
        /// </summary>
        public int Total_Fee { get; set; }

        /// <summary>
        /// 统一下单接口返回结果
        /// </summary>
        public WxPayData UnifiedOrderResult { get; set; }

        /// <summary>
        /// 网页授权获取用户基本信息的全部过程
        /// </summary>
        /// <remarks>
        /// 详情请参看网页授权获取用户基本信息：http://mp.weixin.qq.com/wiki/17/c0f37d5704f0b64713d5d2c37b468d75.html
        /// 第一步：利用url跳转获取code
        /// 第二步：利用code去获取openid和access_token
        /// </remarks>
        public void GetOpenidAndAccessToken()
        {
            //获取code码，以获取openid和access_token
            string code = _httpContext.HttpContext.Request.Query["code"].FirstOrDefault();

            if (!string.IsNullOrEmpty(code))
            {
                Log.Debug(this.GetType().ToString(), "Get code : " + code);
                GetOpenidAndAccessTokenFromCode(code);
            }
            else
            {
                //构造网页授权获取code的URL
                string host = _httpContext.HttpContext.Request.Host.Host;
                string path = _httpContext.HttpContext.Request.Path;
                string redirect_uri = HttpUtility.UrlEncode("http://" + host + path);
                WxPayData data = new WxPayData();
                data.SetValue("appid", WxPayConfig.GetConfig().GetAppID());
                data.SetValue("redirect_uri", redirect_uri);
                data.SetValue("response_type", "code");
                data.SetValue("scope", "snsapi_base");
                data.SetValue("state", "STATE" + "#wechat_redirect");
                string url = "https://open.weixin.qq.com/connect/oauth2/authorize?" + data.ToUrl();
                Log.Debug(this.GetType().ToString(), "Will Redirect to URL : " + url);
                try
                {
                    //触发微信返回code码
                    _httpContext.HttpContext.Response.Redirect(url);//Redirect函数会抛出ThreadAbortException异常，不用处理这个异常
                }
                catch (System.Threading.ThreadAbortException ex)
                {
                }
            }
        }

        /// <summary>
        /// 通过code换取网页授权access_token和openid的返回数据
        /// </summary>
        /// <remarks>
        /// access_token可用于获取共享收货地址
        /// openid是微信支付jsapi支付接口统一下单时必须的参数
        /// 正确时返回的JSON数据包如下：
        /// {
        ///  "access_token":"ACCESS_TOKEN",
        ///  "expires_in":7200,
        ///  "refresh_token":"REFRESH_TOKEN",
        ///  "openid":"OPENID",
        ///  "scope":"SCOPE",
        ///  "unionid": "o6_bmasdasdsad6_2sgVt7hMZOPfL"
        /// }
        /// 更详细的说明请参考网页授权获取用户基本信息：http://mp.weixin.qq.com/wiki/17/c0f37d5704f0b64713d5d2c37b468d75.html
        /// </remarks>
        /// <param name="code"></param>
        /// <returns></returns>
        public JsonData GetOpenidAndAccessTokenFromCode(string code)
        {
            try
            {
                //构造获取openid及access_token的url
                WxPayData data = new WxPayData();
                data.SetValue("appid", WxPayConfig.GetConfig().GetAppID());
                data.SetValue("secret", WxPayConfig.GetConfig().GetAppSecret());
                data.SetValue("code", code);
                data.SetValue("grant_type", "authorization_code");
                string url = "https://api.weixin.qq.com/sns/oauth2/access_token?" + data.ToUrl();

                //请求url以获取数据
                string result = HttpService.Get(url);

                Log.Debug(this.GetType().ToString(), "GetOpenidAndAccessTokenFromCode response : " + result);

                //保存access_token，用于收货地址获取
                JsonData jd = JsonMapper.ToObject(result);
                Access_Token = (string)jd["access_token"];

                //获取用户openid
                Openid = (string)jd["openid"];

                Log.Debug(this.GetType().ToString(), "Get openid : " + Openid);
                Log.Debug(this.GetType().ToString(), "Get access_token : " + Access_Token);

                return jd;
            }
            catch (Exception ex)
            {
                Log.Error(this.GetType().ToString(), ex.ToString());
                throw new WxPayException(ex.ToString());
            }
        }

        /// <summary>
        /// 调用统一下单
        /// </summary>
        /// <returns>统一下单结果</returns>
        public WxPayData GetUnifiedOrderResult()
        {
            //统一下单
            WxPayData data = new WxPayData();
            data.SetValue("body", "test");
            data.SetValue("attach", "test");
            data.SetValue("out_trade_no", WxPayApi.GenerateOutTradeNo());
            data.SetValue("total_fee", Total_Fee);
            data.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));
            data.SetValue("time_expire", DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss"));
            data.SetValue("goods_tag", "test");
            data.SetValue("trade_type", "JSAPI");
            data.SetValue("openid", Openid);

            WxPayData result = WxPayApi.UnifiedOrder(data);
            if (!result.IsSet("appid") || !result.IsSet("prepay_id") || result.GetValue("prepay_id").ToString() == "")
            {
                Log.Error(this.GetType().ToString(), "UnifiedOrder response error!");
                throw new WxPayException("UnifiedOrder response error!");
            }

            UnifiedOrderResult = result;
            return result;
        }

        /// <summary>
        /// 从统一下单成功返回的数据中获取微信浏览器调起jsapi支付所需的参数
        /// </summary>
        /// <remarks>
        /// 微信浏览器调起JSAPI时的输入参数格式如下：
        /// {
        ///   "appId" : "wx2421b1c4370ec43b",     //公众号名称，由商户传入
        ///   "timeStamp":" 1395712654",         //时间戳，自1970年以来的秒数
        ///   "nonceStr" : "e61463f8efa94090b1f366cccfbbb444", //随机串
        ///   "package" : "prepay_id=u802345jgfjsdfgsdg888",
        ///   "signType" : "MD5",         //微信签名方式:
        ///   "paySign" : "70EA570631E4BB79628FBCA90534C63FF7FADD89" //微信签名
        /// }
        /// 更详细的说明请参考网页端调起支付API：http://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=7_7
        /// </remarks>
        /// <returns>微信浏览器调起JSAPI时的输入参数，json格式可以直接做参数用</returns>
        public string GetJsApiParameters()
        {
            Log.Debug(this.GetType().ToString(), "JsApiPay::GetJsApiParam is processing...");

            WxPayData jsApiParam = new WxPayData();
            jsApiParam.SetValue("appId", UnifiedOrderResult.GetValue("appid"));
            jsApiParam.SetValue("timeStamp", WxPayApi.GenerateTimeStamp());
            jsApiParam.SetValue("nonceStr", WxPayApi.GenerateNonceStr());
            jsApiParam.SetValue("package", "prepay_id=" + UnifiedOrderResult.GetValue("prepay_id"));
            jsApiParam.SetValue("signType", "MD5");
            jsApiParam.SetValue("paySign", jsApiParam.MakeSign());

            string parameters = jsApiParam.ToJson();

            Log.Debug(this.GetType().ToString(), "Get jsApiParam : " + parameters);
            return parameters;
        }

        /// <summary>
        /// 获取收货地址js函数入口参数
        /// </summary>
        /// <remarks>
        /// 详情请参考收货地址共享接口：http://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=7_9
        /// </remarks>
        /// <returns>共享收货地址js函数需要的参数，json格式可以直接做参数使用</returns>
        public string GetEditAddressParameters()
        {
            string parameter = "";
            try
            {
                string host = _httpContext.HttpContext.Request.Host.Host;
                string path = _httpContext.HttpContext.Request.Path;
                string queryString = _httpContext.HttpContext.Request.QueryString.Value;

                //这个地方要注意，参与签名的是网页授权获取用户信息时微信后台回传的完整url
                string url = "http://" + host + path + queryString;

                //构造需要用SHA1算法加密的数据
                WxPayData signData = new WxPayData();
                signData.SetValue("appid", WxPayConfig.GetConfig().GetAppID());
                signData.SetValue("url", url);
                signData.SetValue("timestamp", WxPayApi.GenerateTimeStamp());
                signData.SetValue("noncestr", WxPayApi.GenerateNonceStr());
                signData.SetValue("accesstoken", Access_Token);
                string param = signData.ToUrl();

                Log.Debug(this.GetType().ToString(), "SHA1 encrypt param : " + param);

                //以字节方式存储　　
                byte[] data = System.Text.Encoding.UTF8.GetBytes(param);
                var sha1 = System.Security.Cryptography.SHA1.Create();

                //得到哈希值
                var hash = sha1.ComputeHash(data);

                //SHA1加密
                string addrSign = BitConverter.ToString(hash).Replace("-", "");
                Log.Debug(this.GetType().ToString(), "SHA1 encrypt result : " + addrSign);

                //获取收货地址js函数入口参数
                WxPayData afterData = new WxPayData();
                afterData.SetValue("appId", WxPayConfig.GetConfig().GetAppID());
                afterData.SetValue("scope", "jsapi_address");
                afterData.SetValue("signType", "sha1");
                afterData.SetValue("addrSign", addrSign);
                afterData.SetValue("timeStamp", signData.GetValue("timestamp"));
                afterData.SetValue("nonceStr", signData.GetValue("noncestr"));

                //转为json格式
                parameter = afterData.ToJson();
                Log.Debug(this.GetType().ToString(), "Get EditAddressParam : " + parameter);
            }
            catch (Exception ex)
            {
                Log.Error(this.GetType().ToString(), ex.ToString());
                throw new WxPayException(ex.ToString());
            }

            return parameter;
        }
    }
}