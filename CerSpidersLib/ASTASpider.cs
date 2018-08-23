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
    public class ASTASpider : CerSpiderBase
    {
        const String PostUrl = "http://www.astabeab.com/buyers-by-number.asp";
        const String PostData_Temp = "reply=yes&LicNo={0}&submit=SubmitLic";
        /// <summary>
        /// 构造函数
        /// </summary>
        public ASTASpider()
        {
            this.CerType = 9;
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
            #region 执行任务结构示例
            String cernum = String.Empty;

            while (CerQueue.TryDequeue(out cernum))
            {
                /*这里写执行任务相关代码
                 *
                 */
                Console.WriteLine($"ASTA证书号{cernum}开始");
                String html = GetHtml(cernum);
                Dictionary<String, String> updata = GetData(html);
                Console.WriteLine($"ASTA证书号{cernum}完毕");

                //上传数据入队
                UpLoadQueue.Enqueue(updata);

                Thread.Sleep(1);
            }

            #endregion

        }

        private Dictionary<String,String> GetData(string html)
        {
            Dictionary<String, String> dirs = new Dictionary<string, string>();
            var trs = XpathMethod.GetMutResult("//*[@id=\"certificatedetails\"]/table/tr", html, 0);
            foreach (var tr in trs)
            {
                var valign = XpathMethod.GetSingleResult("tr", tr, "valign");
                if (valign.Trim().ToUpper().Equals("TOP"))
                {
                    var name = XpathMethod.GetSingleResult("tr/td[1]", tr);
                    var value = XpathMethod.GetSingleResult("tr/td[2]", tr);
                    if (!name.Contains("ASTA BEAB Ref") && !name.Contains("Characteristics") && !name.Contains("Licenced Mark") && !name.Contains("Additional Info"))
                    {
                        dirs.Add(Fitlter(name, ":"), Fitlter(value));
                    }
                }
            }

            return dirs;
        }

        private String Fitlter(string v,String repstr =null)
        {
            String str = v.Replace("&nbsp;",String.Empty);
            if(!String.IsNullOrEmpty(repstr))
            {
                str = str.Replace(repstr, String.Empty);
            }
            return str;
        }

        private string GetHtml(string cernum)
        {
            String html = String.Empty;
            while (!html.Contains("Search by Number"))
            {
                html = HttpMethod.FastPostMethod(PostUrl, String.Format(PostData_Temp, cernum));
                Thread.Sleep(1);
            }
            return html;
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
