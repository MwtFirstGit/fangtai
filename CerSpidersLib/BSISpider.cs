using ExtractLib;
using HttpToolsLib;
using ICerSpiderTaskLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CerSpidersLib
{
    public class BSISpider : CerSpiderBase
    {
        #region 公共变量声明
        /// <summary>
        /// 公共Url
        /// </summary>
        const String Url = "https://www.bsigroup.com{0}";
        //const String PageIndex = "/en-GB/Product-Directory/Search-Results/pagination/?currentPageIndex={0}&pageSize=5";
        /// <summary>
        /// 穷举Url
        /// </summary>
        const String Exhaustion_Url = "https://www.bsigroup.com/en-GB/Product-Directory/Product-Directory---ResultsNew/?license={0}&productid={1}";
        /// <summary>
        /// 请求所需Cookie
        /// </summary>
        const String Cookie = "ASP.NET_SessionId=4azf3sub35akzvun1zya5kzh;";

        #region  正则抽取数据
        /// <summary>
        /// 正则匹配详情页Url
        /// </summary>
        const String Reg_Details_Url = "<li>\\s+<a href=\"(.*?)\">";
        const String Reg_Product = "<div class=\"header selected\">[\\s\\S]+?<h3>(.*?)</h3>";
        //匹配Description
        const String Reg_Description = "<strong>Description<span>:</span></strong>(.*?)</li>";
        //匹配Standard
        const String Reg_Standard = "<strong>Standard<span>:</span></strong>(.*?)</li>";
        //匹配Model No
        const String Reg_ModelNo = "<strong>Model No<span>:</span></strong>(.*?)</li>";
        //匹配Certificate Number
        const String Reg_CertiNum = "<strong>Certificate Number<span>:</span></strong>(.*?)</li>";
        //匹配Brand
        const String Reg_Brand = "<h2 class=\"panel-title\">(.*?)</h2>";
        #endregion

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        public BSISpider()
        {
            this.CerType = 7;
        }
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
        /// <summary>
        /// 重写基类方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parms">预留  可空</param>
        /// <returns></returns>
        public override void RunTask(object[] parms = null)
        {
            String cernum = String.Empty;

            while (CerQueue.TryDequeue(out cernum))
            {
                /*这里写执行任务相关代码
                 *
                 */
                Console.WriteLine($"BSI证书号{cernum}开始");
                ConcurrentQueue<string> productidqueue = new ConcurrentQueue<string>();
                for (int i = 1; i < 60001; i++)
                {
                    productidqueue.Enqueue(i.ToString());
                }
                Thread[] td = new Thread[50];
                for(int i = 0; i < td.Length; i++)
                {
                    td[i] = new Thread(new ThreadStart(delegate 
                    {
                        string productid = string.Empty;
                        while(productidqueue.TryDequeue(out productid))
                        {
                            Dictionary<String, String> updata = BSI_Details_Ex(cernum, productid);
                            //上传数据入队
                            UpLoadQueue.Enqueue(updata);
                        }
                    }));
                }
               
                Console.WriteLine($"BSI证书号{cernum}完毕");


                Thread.Sleep(1);
            }
        }

        #region 请求相关
        /// <summary>
        /// 获取详情页
        /// </summary>
        /// <param name="detailsurl"></param>
        public static Dictionary<String, String> BSI_Details(String detailsurl, String Certi_No)
        {
            Dictionary<String, String> dirs = new Dictionary<string, string>();
            string html = "start";
            try
            {
                HttpInfo info = new HttpInfo();
                info.RequestUrl = String.Format(Url, detailsurl);
                while (!html.Contains("Product Directory - Details"))
                {
                    html = HttpMethod.HttpWork(info);
                    Thread.Sleep(1);
                }
                //处理数据
                string product = RegexMethod.GetSingleResult(Reg_Product, html, 1);
                string description = RegexMethod.GetSingleResult(Reg_Description, html, 1);
                string standard = RegexMethod.GetSingleResult(Reg_Standard, html, 1);
                string modelno = RegexMethod.GetSingleResult(Reg_ModelNo, html, 1);
                string certinum = RegexMethod.GetSingleResult(Reg_CertiNum, html, 1);
                string brand = RegexMethod.GetSingleResult(Reg_Brand, html, 1);
                if (certinum == Certi_No)
                {
                    dirs.Add("CertificateNo", certinum);
                    dirs.Add("Product", product);
                    dirs.Add("Model", modelno);
                    dirs.Add("Description", description);
                    dirs.Add("Brand", brand);
                    dirs.Add("Standard", standard);
                }
            }
            catch
            {

            }
            return dirs;
        }
        /// <summary>
        /// 穷举获取详情页
        /// </summary>
        /// <param name="Certi_No"></param>
        public static Dictionary<string, string> BSI_Details_Ex(String Certi_No, String productid)
        {
            Dictionary<String, String> dirs = new Dictionary<string, string>();
            string html = "start";
            try
            {
                HttpInfo info = new HttpInfo();
                info.RequestUrl = String.Format(Exhaustion_Url, Certi_No, productid);
                info.KeepLive = true;
                info.User_Agent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36";
                while (!html.Contains("Product Directory - Details"))
                {
                    html = HttpMethod.HttpWork(info);
                    Thread.Sleep(1);
                }
                //处理数据
                string product = RegexMethod.GetSingleResult(Reg_Product, html, 1);
                string description = RegexMethod.GetSingleResult(Reg_Description, html, 1);
                string standard = RegexMethod.GetSingleResult(Reg_Standard, html, 1);
                string modelno = RegexMethod.GetSingleResult(Reg_ModelNo, html, 1);
                string certinum = RegexMethod.GetSingleResult(Reg_CertiNum, html, 1);
                string brand = RegexMethod.GetSingleResult(Reg_Brand, html, 1);
                if (certinum == Certi_No)
                {
                    dirs.Add("CertificateNo", certinum);
                    dirs.Add("Product", product);
                    dirs.Add("Model", modelno);
                    dirs.Add("Description", description);
                    dirs.Add("Brand", brand);
                    dirs.Add("Standard", standard);
                }
            }
            catch
            {

            }
            return dirs;
        }
        #endregion
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

        #region 辅助函数
        /// <summary>
        /// 获取详情页Url
        /// </summary>
        /// <param name="Certi_No"></param>
        /// <param name="pageindex"></param>
        /// <returns></returns>
        public static List<String> Result_GetUrl(String Certi_No, int pageindex)
        {
            string html = "start";
            List<String> urllist = new List<string>();
            String str = RegexMethod.GetSingleResult("\\s+", Certi_No);
            Certi_No = Certi_No.Replace(str, "+");
            try
            {
                HttpInfo info = new HttpInfo();
                while (!html.Contains("Product Directory - Search Results") || !html.Contains("No results found"))
                {
                    info.RequestUrl = String.Format(Url, "/en-GB/Product-Directory/Search-Results/?currentPageIndex=" + pageindex + "&pageSize=5");
                    info.Referer = String.Format(Url, "/en-GB/Product-Directory/Search-Results/?license=" + Certi_No);
                    info.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                    info.Cookie = new CookieString(Cookie, true);

                    info.User_Agent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36";
                    info.AllowAutoRedirect = true;
                    html = HttpMethod.HttpWork(info);
                    Thread.Sleep(1);
                }
                var list = RegexMethod.GetMutResult(Reg_Details_Url, html, 1);
                if (list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        urllist.Add(item.Replace("amp;", ""));
                    }
                }
            }
            catch
            {

            }
            return urllist;
        }
        #endregion
    }
}
