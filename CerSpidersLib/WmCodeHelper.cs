using HttpToolsLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CerSpidersLib
{
    class WmCodeHelper
    {
        /// <summary>
        /// 验证码图片Url
        /// </summary>
        const String Code_Url = "http://webdata.cqccms.com.cn/webdata/ImagepassController/imagePass.do";

        /// <summary>
        /// 载入识别库
        /// </summary>
        /// <param name="FilePath">识别库文件所在路径</param>
        /// <param name="Password">识别库调用密码</param>
        /// <returns></returns>
        [DllImport("WmCode.dll")]
        public static extern bool LoadWmFromFile(string FilePath, string Password);

        [DllImport("WmCode.dll")]
        public static extern bool LoadWmFromBuffer(byte[] FileBuffer, int FileBufLen, string Password);

        [DllImport("WmCode.dll")]
        public static extern bool GetImageFromFile(string FilePath, StringBuilder Vcode);

        [DllImport("WmCode.dll")]
        public static extern bool GetImageFromBuffer(byte[] FileBuffer, int ImgBufLen, StringBuilder Vcode);

        [DllImport("WmCode.dll")]
        public static extern bool SetWmOption(int OptionIndex, int OptionValue);

        /// <summary>
        /// 识别验证码
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static String WmCodeAndGetCode(String Url, String cookie)
        {
            HttpInfo info = new HttpInfo(Url);
            info.Cookie = new CookieString(cookie, true);
            Image image = HttpMethod.DownPic(info);
            byte[] imagebyte = ImageToBytes(image);
            String vcode = String.Empty;
            bool flag = false;
            try
            {
                StringBuilder result = new StringBuilder('\0', 256);
                do
                {
                    flag = GetImageFromBuffer(imagebyte, imagebyte.Length, result);
                }
                while (!flag);
                vcode = result.ToString();
            }
            catch { }
            return vcode;
        }
        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <returns></returns>
        public static string Get_Code(string cookie)
        {
            String code = String.Empty;
            try
            {
                while (code.Length != 4)
                {
                    code = WmCodeHelper.WmCodeAndGetCode(Code_Url, cookie);
                    Thread.Sleep(1);
                }
            }
            catch
            {

            }
            return code;
        }
        /// <summary>
        /// 图片转化为byte
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static byte[] ImageToBytes(Image image)
        {
            ImageFormat format = image.RawFormat;
            byte[] buffer = null;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    if (format.Equals(ImageFormat.Jpeg))
                    {
                        Bitmap t = new Bitmap(image);
                        t.Save(ms, ImageFormat.Jpeg);
                    }
                    else if (format.Equals(ImageFormat.Png))
                    {
                        image.Save(ms, ImageFormat.Png);
                    }
                    else if (format.Equals(ImageFormat.Bmp))
                    {
                        image.Save(ms, ImageFormat.Bmp);
                    }
                    else if (format.Equals(ImageFormat.Gif))
                    {
                        image.Save(ms, ImageFormat.Gif);
                    }
                    else if (format.Equals(ImageFormat.Icon))
                    {
                        image.Save(ms, ImageFormat.Icon);
                    }
                    buffer = new byte[ms.Length];
                    //Image.Save()会改变MemoryStream的Position，需要重新Seek到Begin
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.Read(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return buffer;
        }
    }
}
