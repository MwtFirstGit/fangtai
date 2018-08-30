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
    public class ENECSpider : CerSpiderBase
    {
        #region 公共变量声明
        /// <summary>
        /// 请求Url
        /// </summary>
        const String Url = "http://www.eepca.org/trait_recherche.php?s=1";
        /// <summary>
        /// 请求PostData
        /// </summary>
        const String PostData = "soc=ENEC&certif={0}&product=&tradem1=&product_description=&product_web_code_2=&artser=&cb=ALL&validation=&soc=ENEC";

        #region 正则表达式匹配结果
        /// <summary>
        /// 正则匹配标题与内容所在字段
        /// </summary>
        const String reg_all = "<tr>([\\s\\S]+?)</tr>";
        /// <summary>
        /// 正则匹配标题
        /// </summary>
        const String reg_title = "<th.*?>(.+?)</th>";
        /// <summary>
        /// 正则匹配内容
        /// </summary>
        const String reg_details = "<td.*?>(.+?)</td>";
        /// <summary>
        /// 正则匹配More的链接
        /// </summary>
        const String reg_moreurl = "<a href=\"(.*?)\">";
        #endregion

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        public ENECSpider()
        {
            this.CerType = 3;
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
                Console.WriteLine($"ENEC证书号{cernum}开始");
                Dictionary<String, String> updata = ENEC_Details(cernum);
                Console.WriteLine($"ENEC证书号{cernum}完毕");

                //上传数据入队
                UpLoadQueue.Enqueue(updata);

                Thread.Sleep(1);
            }
        }
        /// <summary>
        /// 获取详情页
        /// </summary>
        /// <param name="Certi_No"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ENEC_Details(string Certi_No)
        {
            Dictionary<string, string> dirs = new Dictionary<string, string>();
            String html = "start";
            try
            {
                HttpInfo info = new HttpInfo();
                info.RequestUrl = Url;
                info.User_Agent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36";
                info.PostData = String.Format(PostData, Certi_No);
                info.AllowAutoRedirect = true;
                info.KeepLive = true;
                info.CC = new System.Net.CookieContainer();
                while (!html.Contains("Consultation of") && !html.Contains("licenced products"))
                {
                    html = HttpMethod.HttpWork(info);
                    Thread.Sleep(1);
                }
                //处理数据
                var list = RegexMethod.GetMutResult(reg_all, html, 1);
                if (list.Count > 1)
                {
                    var titlelist = RegexMethod.GetMutResult(reg_title, list[0], 1);
                    var detaillist = RegexMethod.GetMutResult(reg_details, list[1], 1);
                    if (titlelist.Count == detaillist.Count)
                    {
                        for (int i = 0; i < titlelist.Count; i++)
                        {
                            if (i == titlelist.Count - 1)
                            {
                                if (detaillist[i].Contains("a href"))
                                {
                                    String moreall = RegexMethod.GetSingleResult(reg_moreurl, detaillist[i], 1);
                                    dirs.Add(titlelist[i], moreall);
                                }
                                else
                                {
                                    dirs.Add(titlelist[i], detaillist[i]);
                                }
                            }
                            else
                            {
                                dirs.Add(titlelist[i], detaillist[i]);
                            }
                        }
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
