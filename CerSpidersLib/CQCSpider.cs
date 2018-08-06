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
    public class CQCSpider : CerSpiderBase
    {
        #region 公共变量声明
        /// <summary>
        /// CQC详情Url
        /// </summary>
        const String CQC_Details_Url = "http://webdata.cqccms.com.cn/webdata/query/ZYCerti.do";
        /// <summary>
        /// CQCPost请求参数
        /// </summary>
        const String CQC_PostData = "keyword={0}&_h_select_chaxuntype=certinum&chaxuntype=certinum&imagePassword={1}";
        /// <summary>
        /// xpath获取标题
        /// </summary>
        const String xpath_title = "/html/body/div/div/table[5]/thead/tr/th";
        /// <summary>
        /// xpath获取详情
        /// </summary>
        const String xpath_certi = "/html/body/div/div/table[5]/tbody/tr/td";
        /// <summary>
        /// 正则匹配下载附件Url
        /// </summary>
        const String reg_fjurl = "<a href=\"(.*?)\">";
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        public CQCSpider() { this.CerType = 2; }

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
                Console.WriteLine($"CQC证书号{cernum}开始");
                //String html = CCC_Details(cernum);
                Dictionary<String, String> updata = CQC_Details(cernum);
                Console.WriteLine($"CQC证书号{cernum}完毕");

                //上传数据入队
                UpLoadQueue.Enqueue(updata);

                Thread.Sleep(1);
            }
        }
        /// <summary>
        /// 获取证书编号对应详情
        /// </summary>
        /// <param name="Certi_No"></param>
        private Dictionary<String, String> CQC_Details(String Certi_No)
        {
            Dictionary<String, String> dirs = new Dictionary<string, string>();
            HttpInfo info = new HttpInfo();
            String html = "start";
            String code = String.Empty;
            String cookie = Get_Cookie(CQC_Details_Url);
            try
            {
                while (!html.Contains(Certi_No) || html.Contains("验证码输入有误") || html.Contains("请输入验证码"))
                {
                    code = WmCodeHelper.Get_Code(cookie);
                    info.RequestUrl = CQC_Details_Url;
                    info.PostData = String.Format(CQC_PostData, Certi_No, code);
                    info.Cookie = new CookieString(cookie, true);
                    //info.Ip = "";
                    html = HttpMethod.HttpWork(info);
                    Thread.Sleep(1);
                }
                if (html.Contains(Certi_No))
                {
                    //数据处理
                    var ths = XpathMethod.GetMutResult(xpath_title, html, 1);
                    var strs = XpathMethod.GetMutResult(xpath_certi, html, 1);
                    string fjurl = XpathMethod.GetSingleResult(xpath_certi + "[14]", html, 0);
                    fjurl = RegexMethod.GetSingleResult(reg_fjurl, fjurl, 1);
                    for (int i = 1; i < strs.Count; i++)
                    {
                        if(i == strs.Count - 1 && !String.IsNullOrEmpty(fjurl))
                        {
                            dirs.Add(ths[i], fjurl);
                        }
                        else
                        {
                            dirs.Add(ths[i], strs[i]);
                        }
                    }
                    //CCCInfo cccinfo = new CCCInfo();
                    //cccinfo.CertiNo = XpathMethod.GetSingleResult(xpath_certino, html);
                    //cccinfo.Applicant = XpathMethod.GetSingleResult(xpath_appli1, html) + "\r\n" + XpathMethod.GetSingleResult(xpath_appli2, html);
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
        /// 获取CQC验证码cookie
        /// </summary>
        /// <returns></returns>
        private static string Get_Cookie(String Url)
        {
            String cookie = String.Empty;
            String html = "start";
            try
            {
                HttpInfo info = new HttpInfo(Url);
                while (!html.Contains("产品认证证书查询"))
                {
                    info.Encoding = Encoding.GetEncoding("gb2312");
                    html = HttpMethod.HttpWork(ref info);
                    Thread.Sleep(1);
                }
                cookie = info.Cookie.ConventToString();
            }
            catch { }
            return cookie;
        }
        #endregion
    }
}
