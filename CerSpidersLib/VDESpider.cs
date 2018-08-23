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
  public  class VDESpider: CerSpiderBase
    {
        #region 公共变量声明
        /// <summary>
        /// 查询页Url
        /// </summary>
        const String Url = "https://www2.vde.com/en/institute/onlineservice/vde-approved-products/pages/online-search.aspx";
        /// <summary>
        /// 详情页Url
        /// </summary>
        const String Details_Url = "https://www2.vde.com/en/institute/onlineservice/vde-approved-products/pages/SearchResult.aspx?cid={0}";
        /// <summary>
        /// 查询页postdata
        /// </summary>
        const String PostData = "{0}&{1}&InputKeywords=&ctl00%24SPWebPartManager1%24g_0e8aa12b_f819_403f_8243_19f81274454c%24ctl02={2}&ctl00%24SPWebPartManager1%24g_0e8aa12b_f819_403f_8243_19f81274454c%24ctl03=Search";
        /// <summary>
        /// 详情页postdata
        /// </summary>
        const String Details_PostData = "{0}&{1}&{2}&{3}&__VIEWSTATEENCRYPTED=";

        #region 正则或抽取
        //匹配 VIEWSTATE
        const String reg_view = "id=\"__VIEWSTATE\" value=\"(.*?)\"";
        //匹配 EVENTVALIDATION
        const String reg_event = "id=\"__EVENTVALIDATION\" value=\"(.*?)\"";
        /// <summary>
        /// 获取cid
        /// </summary>
        const String reg_cid = "\\?cid=(.*?)\"";
        /// <summary>
        /// 获取详情页VIEWSTATE和EVENTVALIDATION
        /// </summary>
        const String reg_newenent = "onclick=\"__doPostBack\\('(.*?)','(.*?)'\\)";
        /// <summary>
        /// Xpath抽取标题
        /// </summary>
        const String xpath_title = "//*[@id=\"ctl00_SPWebPartManager1_g_ec5813e5_5aec_4729_8621_c2bea12c22a6\"]/table/tr";
        /// <summary>
        /// Xpath抽取详情
        /// </summary>
        const String xpath_details = "tr/td";
        /// <summary>
        /// Xpath抽取Standard
        /// </summary>
        const String xpath_stand = "//*[@id=\"ctl00_SPWebPartManager1_g_081d1951_335c_4dda_8b51_9122acc9521f\"]/table/tr/td";
        /// <summary>
        /// Xpath抽取Type
        /// </summary>
        const String xpath_Type = "//*[@id=\"ctl00_SPWebPartManager1_g_dc2b080c_fc15_4e80_867e_75ce3f457e14_DataGrid\"]/tr";
        #endregion

        #endregion
        /// <summary>
        /// 构造函数
        /// </summary>
        public VDESpider() { this.CerType = 4; }


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
            //throw new Exception("调用了基类执行任务方法");
            #region 执行任务结构示例
            String cernum = String.Empty;

            while (CerQueue.TryDequeue(out cernum))
            {

                /*这里写执行任务相关代码
                 *
                 */
                Console.WriteLine($"VDE证书号{cernum}开始");
                Dictionary<String, String> updata = VDE_Details(cernum);
                Console.WriteLine($"VDE证书号{cernum}完毕");
                //上传数据入队
                UpLoadQueue.Enqueue(updata);

                Thread.Sleep(1);
            }

            #endregion

        }

        /// <summary>
        /// 请求获取详细信息
        /// </summary>
        /// <param name="Certi_No"></param>
        public static Dictionary<string,string> VDE_Details(String Certi_No)
        {
            Dictionary<String, String> dirs = new Dictionary<string, string>();
            String Html = String.Empty;
            String newpostdata = String.Empty;
            String typedata = String.Empty;
            try
            {
                String postdata = GetPostData(Certi_No);
                String cid = GetCid(postdata, ref newpostdata);
                HttpInfo info = new HttpInfo();
                info.RequestUrl = String.Format(Details_Url, cid);
                info.PostData = newpostdata;
                info.AllowAutoRedirect = true;
                while (!Html.Contains("Details"))
                {
                    Html = HttpMethod.HttpWork(info);
                    Thread.Sleep(1);
                }
                dirs.Add("CertificateNo", Certi_No);
                var trs = XpathMethod.GetMutResult(xpath_title, Html, 0);
                foreach (var tr in trs)
                {
                    if (XpathMethod.GetSingleResult(xpath_details + "[1]", tr, 1) != "Certification mark")
                    {
                        dirs.Add(XpathMethod.GetSingleResult(xpath_details + "[1]", tr, 1), XpathMethod.GetSingleResult(xpath_details + "[2]", tr, 1));
                    }
                }
                if (Html.Contains("Additional information"))
                {
                    dirs.Add("Additional", "Additional information");
                }
                else
                {
                    dirs.Add("Additional", "");
                }
                dirs.Add(XpathMethod.GetSingleResult(xpath_stand + "[1]", Html, 1).Replace("<.*>", ""), XpathMethod.GetSingleResult(xpath_stand + "[2]", Html, 1).Replace("<.*>\\s+", "").Replace("\\s+<.*>", "").Replace("<.*>", ""));
                var typetrs = XpathMethod.GetMutResult(xpath_Type, Html, 0);
                for (int i = 1; i < typetrs.Count; i++)
                {
                    if (i == typetrs.Count - 1)
                    {
                        typedata = typedata + XpathMethod.GetSingleResult("tr/td[1]", typetrs[i], 1).Replace("&#178;", "") + ";" + XpathMethod.GetSingleResult("tr/td[2]", typetrs[i], 1).Replace("&nbsp;", "").Replace("&#178;", "");
                    }
                    else
                    {
                        typedata = typedata + XpathMethod.GetSingleResult("tr/td[1]", typetrs[i], 1).Replace("&#178;", "") + ";" + XpathMethod.GetSingleResult("tr/td[2]", typetrs[i], 1).Replace("&nbsp;", "").Replace("&#178;", "");
                    }
                }
                dirs.Add("Type-Techical Data", "Type and Technical Data" + ";" + typedata);
                dirs.Add("Type", typedata);
                foreach (var item in dirs.Keys)
                {
                    Console.Write(item + "，");
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

        #region 辅助函数
        /// <summary>
        /// 获取PostData
        /// </summary>
        /// <param name="Certi_No"></param>
        /// <returns></returns>
        private static string GetPostData(String Certi_No)
        {
            String postdata = String.Empty;
            String html = "start";
            try
            {
                while (!html.Contains("Online Search"))
                {
                    html = HttpMethod.FastGetMethod(Url);
                    Thread.Sleep(1);
                }
                String ViewState = "__VIEWSTATE=" + RegexMethod.GetSingleResult(reg_view, html, 1).Replace("+", "%2B").Replace("/", "%2F").Replace("=", "%3D");
                String EventValidation = "__EVENTVALIDATION=" + RegexMethod.GetSingleResult(reg_event, html, 1).Replace("+", "%2B").Replace("/", "%2F").Replace("=", "%3D");
                postdata = String.Format(PostData, ViewState, EventValidation, Certi_No);
            }
            catch
            {

            }
            return postdata;
        }
        /// <summary>
        /// 获取cid
        /// </summary>
        /// <param name="postdata"></param>
        /// <returns></returns>
        private static string GetCid(string postdata, ref string newpostdata)
        {
            String cid = String.Empty;
            String html = "start";
            try
            {
                HttpInfo info = new HttpInfo();
                info.RequestUrl = Url;
                info.PostData = postdata;
                info.AllowAutoRedirect = true;
                info.KeepLive = true;
                while (!html.Contains("Search Result"))
                {
                    html = HttpMethod.HttpWork(info);
                    Thread.Sleep(1);
                }
                cid = RegexMethod.GetSingleResult(reg_cid, html, 1);
                String eventarget = "__EVENTTARGET=" + RegexMethod.GetSingleResult(reg_newenent, html, 1).Replace("$", "%24");
                String eventargument = "__EVENTARGUMENT=" + RegexMethod.GetSingleResult(reg_newenent, html, 2).Replace("$", "%24");
                String ViewState = "__VIEWSTATE=" + RegexMethod.GetSingleResult(reg_view, html, 1).Replace("+", "%2B").Replace("/", "%2F").Replace("=", "%3D");
                String EventValidation = "__EVENTVALIDATION=" + RegexMethod.GetSingleResult(reg_event, html, 1).Replace("+", "%2B").Replace("/", "%2F").Replace("=", "%3D");
                newpostdata = String.Format(Details_PostData, eventarget, eventargument, ViewState, EventValidation);
            }
            catch
            {

            }
            return cid;
        }
        #endregion
    }
}
