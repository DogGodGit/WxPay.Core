using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using ThoughtWorks.QRCode.Codec;

namespace WxPay.Core.lib
{
    public class MakeQRCode
    {
        /// <summary>
        /// 二维码生成
        /// </summary>
        /// <param name="data">二维码参数</param>
        /// <returns>二维码</returns>
        public static string GetMakeQRCodeBase64String(string data)
        {
            //初始化二维码生成工具
            QRCodeEncoder qrCodeEncoder = new QRCodeEncoder
            {
                QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE,
                QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M,
                QRCodeVersion = 0,
                QRCodeScale = 4
            };

            //将字符串生成二维码图片
            Bitmap image = qrCodeEncoder.Encode(data, Encoding.Default);

            //保存为PNG到内存流
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);

                //输出二维码图片
                //读图片转为Base64String
                byte[] arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length);

                return Convert.ToBase64String(arr);
            }
        }
    }
}