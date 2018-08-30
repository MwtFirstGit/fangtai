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
    public class TUVndSpider : CerSpiderBase
    {

        #region 公共变量声明
        /// <summary>
        /// 访问Url
        /// </summary>
        const String Url = "https://certificateexplorer2.tuev-sued.de/web/ig-tuvs/certificate?lang=en&q={0}";

        #region 正则表达式匹配结果
        /// <summary>
        /// 正则匹配标题与内容所在字段
        /// </summary>
        const String reg_all = "<tr>([\\s\\S]+?)</tr>";
        /// <summary>
        /// 正则匹配标题
        /// </summary>
        const String reg_title = "<span>(.*?)</span>";
        /// <summary>
        /// 正则匹配内容
        /// </summary>
        const String reg_details = "<td>([\\s\\S]+?)</td>";
        #endregion

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        public TUVndSpider()
        {
            this.CerType = 6;
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
                Console.WriteLine($"TUVnd证书号{cernum}开始");
                Dictionary<String, String> updata = TUVnd_Details(cernum);
                Console.WriteLine($"TUVnd证书号{cernum}完毕");

                //上传数据入队
                UpLoadQueue.Enqueue(updata);

                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// 请求获取详细信息
        /// </summary>
        /// <param name="Certi_No"></param>
        public static Dictionary<string, string> TUVnd_Details(String Certi_No)
        {
            Dictionary<string, string> dirs = new Dictionary<string, string>();
            String html = "start";
            try
            {
                HttpInfo info = new HttpInfo();
                info.RequestUrl = String.Format(Url, Certi_No);
                while (!html.Contains(Certi_No))
                {
                    html = HttpMethod.HttpWork(info);
                    Thread.Sleep(1);
                }
                //处理数据
                var list = RegexMethod.GetMutResult(reg_all, html, 1);
                int i = 0;
                foreach (var item in list)
                {
                    if (item.Contains("cert_details_th"))
                    {
                        String title = RegexMethod.GetSingleResult(reg_title, item, 1);
                        String details = RegexMethod.GetSingleResult(reg_details, item, 1);
                        if (title != "&#160;" && details != "&#160;")
                        {
                            if (String.IsNullOrEmpty(title))
                            {
                                i++;
                                title = "Custom" + i;
                            }
                            dirs.Add(title, details);
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
