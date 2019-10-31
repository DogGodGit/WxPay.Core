using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Web;
using WxPay.Core.lib;
using WxPayAPI;

namespace WxPayDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DemoController : ControllerBase
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
        public DemoController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContext = httpContextAccessor;
        }

        /// <summary>
        /// 获取用户的openid和access_token
        /// 第一步：利用url跳转获取code
        /// 第二步：利用code去获取openid和access_token
        /// </summary>
        /// <returns>openid和access_token</returns>
        [HttpGet("OpenidAndAccessToken")]
        public ActionResult<dynamic> GetOpenidAndAccessToken(string code = "")
        {
            JsApiPay jsApiPay = new JsApiPay(_httpContext);
            try
            {
                //调用【网页授权获取用户信息】接口获取用户的openid和access_token
                string result = jsApiPay.GetOpenidAndAccessToken(code);

                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }

                return new { jsApiPay.Openid, jsApiPay.Access_Token };
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// JSAPI支付预处理
        /// </summary>
        /// <param name="openid">下单单号</param>
        /// <param name="total_fee">商品金额</param>
        /// <returns></returns>
        [HttpGet("JsApiPay")]
        public ActionResult<string> GetJsApiPay(string openid, string total_fee)
        {
            //检测是否给当前页面传递了相关参数
            if (string.IsNullOrEmpty(openid) || string.IsNullOrEmpty(total_fee))
            {
                return "页面传参出错,请返回重试";
            }

            //若传递了相关参数，则调统一下单接口，获得后续相关接口的入口参数
            JsApiPay jsApiPay = new JsApiPay(_httpContext)
            {
                Openid = openid,
                Total_Fee = int.Parse(total_fee)
            };

            //JSAPI支付预处理
            try
            {
                WxPayData unifiedOrderResult = jsApiPay.GetUnifiedOrderResult();

                //获取H5调起JS API参数
                string wxJsApiParam = jsApiPay.GetJsApiParameters();
                Log.Debug(this.GetType().ToString(), "wxJsApiParam : " + wxJsApiParam);

                //在页面上显示订单信息
                return unifiedOrderResult.ToPrintStr();
            }
            catch (Exception ex)
            {
                return "下单失败，请返回重试" + ex.Message;
            }
        }

        /// <summary>
        /// 支付结果通知回调
        /// </summary>
        /// <returns></returns>
        [HttpGet("ResultNotify")]
        public ActionResult<string> GetResultNotify()
        {
            ResultNotify resultNotify = new ResultNotify(_httpContext);
            return resultNotify.ProcessNotify();
        }

        /// <summary>
        /// 二维码生成
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="type">1模式一,2模式二</param>
        /// <returns>二维码图片</returns>
        [HttpGet("MakeQRCode")]
        public ActionResult<string> GetMakeQRCode(string productId, int type)
        {
            NativePay nativePay = new NativePay();
            string url = string.Empty;
            if (type == 1)
            {
                //生成扫码支付模式一url
                url = nativePay.GetPrePayUrl(productId);
            }
            else if (type == 2)
            {
                //生成扫码支付模式二url
                url = nativePay.GetPayUrl(productId);
            }

            //将url生成二维码图片
            return MakeQRCode.GetMakeQRCodeBase64String(HttpUtility.UrlEncode(url));
        }

        /// <summary>
        /// 扫码支付模式回调
        /// </summary>
        /// <returns></returns>
        [HttpGet("NativeNotify")]
        public ActionResult<string> GetNativeNotify()
        {
            NativeNotify nativeNatify = new NativeNotify(_httpContext);
            return nativeNatify.ProcessNotify();
        }

        /// <summary>
        /// 刷卡支付
        /// </summary>
        /// <param name="auth_code">授权码</param>
        /// <param name="body">商品描述</param>
        /// <param name="fee">商品总金额</param>
        /// <returns></returns>
        [HttpGet("MicroPay")]
        public ActionResult<string> GetMicroPay(string auth_code, string body, string fee)
        {
            //调用刷卡支付,如果内部出现异常则在页面上显示异常原因
            try
            {
                string result = MicroPay.Run(body, fee, auth_code);
                return result;
            }
            catch (WxPayException ex)
            {
                return ex.ToString();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        /// <summary>
        /// 订单查询
        /// </summary>
        /// <param name="transaction_id">微信订单号</param>
        /// <param name="out_trade_no">商户订单号</param>
        /// <returns></returns>
        [HttpGet("OrderQuery")]
        public ActionResult<string> GetOrderQuery(string transaction_id, string out_trade_no)
        {
            if (string.IsNullOrEmpty(transaction_id) && string.IsNullOrEmpty(out_trade_no))
            {
                return "微信订单号和商户订单号至少填写一个,微信订单号优先！";
            }

            //调用订单查询接口,如果内部出现异常则在页面上显示异常原因
            try
            {
                string result = OrderQuery.Run(transaction_id, out_trade_no);//调用订单查询业务逻辑
                return result;
            }
            catch (WxPayException ex)
            {
                return ex.Message;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 获取收货地址
        /// </summary>
        /// <returns>返回Openid</returns>
        [HttpGet("GetAddress")]
        public ActionResult<string> GetAddress(string accesstoken)
        {
            JsApiPay jsApiPay = new JsApiPay(_httpContext);
            try
            {
                //获取收货地址js函数入口参数
                return jsApiPay.GetEditAddressParameters(accesstoken);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 订单退款
        /// </summary>
        /// <param name="transaction_id">微信订单号</param>
        /// <param name="out_trade_no">商户订单号</param>
        /// <param name="total_fee">订单总金额</param>
        /// <param name="refund_fee">退款金额</param>
        /// <returns></returns>
        [HttpGet("Refund")]
        public ActionResult<string> GetRefund(string transaction_id, string out_trade_no, string total_fee, string refund_fee)
        {
            if (string.IsNullOrEmpty(transaction_id) && string.IsNullOrEmpty(out_trade_no))
            {
                return "微信订单号和商户订单号至少填一个！";
            }
            if (string.IsNullOrEmpty(total_fee))
            {
                return "订单总金额必填！";
            }
            if (string.IsNullOrEmpty(refund_fee))
            {
                return "退款金额必填！";
            }

            //调用订单退款接口,如果内部出现异常则在页面上显示异常原因
            try
            {
                string result = Refund.Run(transaction_id, out_trade_no, total_fee, refund_fee);
                return result;
            }
            catch (WxPayException ex)
            {
                return ex.ToString();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        /// <summary>
        /// 退款查询
        /// </summary>
        /// <param name="refund_id">商户订单号</param>
        /// <param name="out_refund_no">商户退款单号</param>
        /// <param name="transaction_id">微信订单号</param>
        /// <param name="out_trade_no">微信退款单号</param>
        /// <returns></returns>
        [HttpGet("RefundQuery")]
        public ActionResult<string> GetRefundQuery(string refund_id, string out_refund_no, string transaction_id, string out_trade_no)
        {
            if (string.IsNullOrEmpty(refund_id) && string.IsNullOrEmpty(out_refund_no) &&
                string.IsNullOrEmpty(transaction_id) && string.IsNullOrEmpty(out_trade_no))
            {
                return "微信订单号、商户订单号、商户退款单号、微信退款单号选填至少一个，微信退款单号优先！";
            }

            //调用退款查询接口,如果内部出现异常则在页面上显示异常原因
            try
            {
                string result = RefundQuery.Run(refund_id, out_refund_no, transaction_id, out_trade_no);
                return result;
            }
            catch (WxPayException ex)
            {
                return ex.ToString();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        /// <summary>
        /// 下载对账单完整业务流程逻辑,一次只能下载一天的对账单
        /// </summary>
        /// <param name="bill_date">下载对账单的日期（格式：20140603）</param>
        /// <param name="bill_type">
        /// 账单类型
        /// ALL，返回当日所有订单信息，默认值
        /// SUCCESS，返回当日成功支付的订单
        /// REFUND，返回当日退款订单
        /// REVOKED，已撤销的订单
        /// </param>
        /// <returns>对账单结果（xml格式</returns>
        [HttpGet("DownloadBill")]
        public ActionResult<string> GetDownloadBill(string bill_date, string bill_type)
        {
            //调用下载对账单接口,如果内部出现异常则在页面上显示异常原因
            try
            {
                return DownloadBill.Run(bill_date, bill_type);
            }
            catch (WxPayException ex)
            {
                return ex.ToString();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}