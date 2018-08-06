using ExtractLib;
using HttpToolsLib;
using ICerSpiderTaskLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CerSpidersLib
{
    public class TUVLYSpider : CerSpiderBase
    {
        #region 公共变量声明
        /// <summary>
        /// TUVLY请求Url
        /// </summary>
        const String TUVLY_Details_Url = "https://www.certipedia.com/certificates/{0}?locale=en";
        /// <summary>
        /// xpath获取数据所在节点
        /// </summary>
        const String xpath_certi = "//*[@class=\"certificate\"]/table/tbody/tr";
        /// <summary>
        /// xpath获取标题
        /// </summary>
        const String xpath_title = "tr/td[1]";
        /// <summary>
        /// xpath获取详情
        /// </summary>
        const String xpath_details = "tr/td[2]";
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        public TUVLYSpider() { this.CerType = 6; }

        /// <summary>
        /// 使用基类方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parms"></param>
        /// <returns></returns>
        public override void GetTask(object[] parms)
        {
            base.GetTask(parms);
        }
        public override void RunTask(object[] parms = null)
        {
            String cernum = String.Empty;

            while (CerQueue.TryDequeue(out cernum))
            {
                /*这里写执行任务相关代码
                 *
                 */
                Console.WriteLine($"TUVLY证书号{cernum}开始");
                //String html = CCC_Details(cernum);
                Dictionary<String, String> updata = TUVLY_Details(cernum);
                Console.WriteLine($"TUVLY证书号{cernum}完毕");

                //上传数据入队
                UpLoadQueue.Enqueue(updata);

                Thread.Sleep(1);
            }
        }
        /// <summary>
        /// 获取证书编号对应详情
        /// </summary>
        /// <param name="Certi_No"></param>
        private Dictionary<String, String> TUVLY_Details(String Certi_No)
        {
            Dictionary<String, String> dirs = new Dictionary<string, string>();
            String html = "start";
            String newcertino = Certi_No.Substring(1, Certi_No.Length - 1);
            try
            {
                HttpInfo info = new HttpInfo();
                info.RequestUrl = String.Format(TUVLY_Details_Url, newcertino);
                while (!html.Contains(newcertino) || !html.Contains("Certificate No"))
                {
                    //info.Ip = 
                    html = HttpMethod.HttpWork(info);
                    Thread.Sleep(1);
                }
                if (html.Contains(newcertino))
                {
                    //处理数据
                    var strs = XpathMethod.GetMutResult(xpath_certi, html, 0);
                    foreach (var str in strs)
                    {
                        dirs.Add(XpathMethod.GetSingleResult(xpath_title, str, 1).Replace(": ", ""), XpathMethod.GetSingleResult(xpath_details, str, 1).Replace("\n", ""));
                    }
                }
            }
            catch
            {

            }
            return dirs;

        }
        /// <summary>
        /// 使用基类方法 服务端接口未完成前先输出到本地
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parms">预留 可空</param>
        /// <returns></returns>
        public override void UploadData(object[] parms = null)
        {
            base.UploadData(parms);
        }
    }
}
