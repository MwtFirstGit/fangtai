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
using TaskEntityLib;

namespace CerSpidersLib
{
    public class ERACSpider : CerSpiderBase
    {
        #region 公共变量声明
        /// <summary>
        /// 任务队列
        /// </summary>
        static ConcurrentQueue<string> taskqueue = new ConcurrentQueue<string>();
        /// <summary>
        /// 详情页postdata队列
        /// </summary>
        static ConcurrentQueue<string> postdataqueue = new ConcurrentQueue<string>();
        /// <summary>
        /// 访问Url
        /// </summary>
        const String Url = "https://equipment.erac.gov.au/Public/";
        /// <summary>
        /// 查询页面PostData
        /// </summary>
        const String Certi_PostData = "{0}&{1}&ctl00%24ContentPlaceHolder1%24cmdSearch=Search&ctl00%24ContentPlaceHolder1%24txtApprovalNumber={2}&ctl00%24ContentPlaceHolder1%24txtApplicant=%25";
        /// <summary>
        /// 详情页PostData
        /// </summary>
        const String Details_PostData = "{0}&{1}&{2}&ctl00%24ContentPlaceHolder1%24txtApprovalNumber={3}&ctl00%24ContentPlaceHolder1%24txtApplicant=%25";
        #region 匹配或抽取
        //匹配 VIEWSTATE
        const String reg_view = "id=\"__VIEWSTATE\" value=\"(.*?)\"";
        //匹配 EVENTVALIDATION
        const String reg_event = "id=\"__EVENTVALIDATION\" value=\"(.*?)\"";
        //匹配 EVENTARGET
        const String reg_arget = "__doPostBack\\((.*?cmdLoad).*\\)";
        /// <summary>
        /// 正则匹配Brand
        /// </summary>
        const String reg_brand = "<span id=\"ContentPlaceHolder1_lblApplicant\">(.*?)</span>";
        /// <summary>
        /// 正则匹配EquipmentClass
        /// </summary>
        const String reg_equi = "<span id=\"ContentPlaceHolder1_lblEquipmentClass\">(.*?)</span>";
        /// <summary>
        /// 正则匹配Description
        /// </summary>
        const String reg_des = "<span id=\"ContentPlaceHolder1_lblDescription\">(.*?)</span>";
        /// <summary>
        /// 正则匹配CertifiedDate
        /// </summary>
        const String reg_cerdate = "<span id=\"ContentPlaceHolder1_lblCertifiedDate\">(.*?)</span>";
        /// <summary>
        /// 正则匹配ExpiryDate
        /// </summary>
        const String reg_expidate = "<span id=\"ContentPlaceHolder1_lblExpiryDate\">(.*?)</span>";
        /// <summary>
        /// 正则匹配Standard
        /// </summary>
        const String reg_stand = "<span id=\"ContentPlaceHolder1_lblStandard\">(.*?)</span>";
        /// <summary>
        /// 正则匹配Renewal_of
        /// </summary>
        const String reg_conditions = "<span id=\"ContentPlaceHolder1_lblConditions\">.* (.*?)</span>";
        /// <summary>
        /// 正则匹配Model
        /// </summary>
        const String reg_model = "<span id=\"ContentPlaceHolder1_rProfiles_lblModelList_.*?\">(.*?)</span>";
        /// <summary>
        /// 正则匹配Rating
        /// </summary>
        const String reg_rating = "<span id=\"ContentPlaceHolder1_rProfiles_lblInputs_.*?\">(.*?)</span>";
        #endregion

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        public ERACSpider()
        {
            this.CerType = 9;
        }
        /// <summary>
        /// 使用基类方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parms"></param>
        /// <returns></returns>
        //public override void GetTask(object[] parms)
        //{
        //    base.GetTask(parms);
        //}
        public void GetTask(TaskEntity taskEntity)
        {
            //base.GetTask(taskqueue, taskEntity);
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
            taskqueue = (ConcurrentQueue<string>)parms[0];
            while (taskqueue.TryDequeue(out cernum))
            {
                /*这里写执行任务相关代码
                 *
                 */
                Console.WriteLine($"ERAC证书号{cernum}开始");
                //String html = CCC_Details(cernum);
                string postdata = GetPostData(cernum);
                GetDetailsPostData(cernum, postdata, ref postdataqueue);
                string detailspostdata = string.Empty;
                while (postdataqueue.TryDequeue(out detailspostdata))
                {
                    Dictionary<String, String> updata = ERAC_Details(detailspostdata, cernum);
                    //上传数据入队
                    UpLoadQueue.Enqueue(updata);
                    Thread.Sleep(1);
                }
                Console.WriteLine($"ERAC证书号{cernum}完毕");
                Thread.Sleep(1);
            }
        }
        /// <summary>
        /// 获取详情页
        /// </summary>
        /// <param name="Certi_No"></param>
        public static Dictionary<string, string> ERAC_Details(String detailspost, String Certi_No)
        {
            Dictionary<string, string> dirs = new Dictionary<string, string>();
            String html = String.Empty;
            try
            {
                HttpInfo info = new HttpInfo();
                info.PostData = detailspost;
                info.AllowAutoRedirect = true;
                info.RequestUrl = Url;
                while (!html.Contains("ERAC National Certification Database - "))
                {
                    html = HttpMethod.HttpWork(info);
                    Thread.Sleep(1);
                }
                //处理数据
                dirs.Add("CertNo", Certi_No);
                dirs.Add("Renewal_of", RegexMethod.GetSingleResult(reg_conditions, html, 1));
                dirs.Add("Description", RegexMethod.GetSingleResult(reg_des, html, 1));
                dirs.Add("Brand", RegexMethod.GetSingleResult(reg_brand, html, 1));
                dirs.Add("EquipmentClass", RegexMethod.GetSingleResult(reg_equi, html, 1));
                dirs.Add("Standard", RegexMethod.GetSingleResult(reg_stand, html, 1));
                dirs.Add("CertifiedDate", RegexMethod.GetSingleResult(reg_cerdate, html, 1));
                dirs.Add("ExpiryDate", RegexMethod.GetSingleResult(reg_expidate, html, 1));
                var modellist = RegexMethod.GetMutResult(reg_model, html, 1);
                String model = string.Empty;
                if (modellist.Count > 0)
                {
                    foreach (var modelitem in modellist)
                    {
                        model = model + modelitem + ";";
                    }
                }
                dirs.Add("Model", model);
                var ratinglist = RegexMethod.GetMutResult(reg_rating, html, 1);
                String rating = string.Empty;
                if (ratinglist.Count > 0)
                {
                    foreach (var ratingitem in ratinglist)
                    {
                        rating = rating + ratingitem + ";";
                    }
                }
                dirs.Add("Rating", rating);
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
        /// 获取查询页PostData
        /// </summary>
        /// <param name="Certi_No"></param>
        /// <returns></returns>
        public static string GetPostData(String Certi_No)
        {
            String postdata = String.Empty;
            String html = "start";
            try
            {
                while (!html.Contains("ERAC National Certification Database"))
                {
                    html = HttpMethod.FastGetMethod(Url);
                    Thread.Sleep(1);
                }
                String ViewState = "__VIEWSTATE=" + RegexMethod.GetSingleResult(reg_view, html, 1).Replace("+", "%2B").Replace("/", "%2F").Replace("=", "%3D");
                String EventValidation = "__EVENTVALIDATION=" + RegexMethod.GetSingleResult(reg_event, html, 1).Replace("+", "%2B").Replace("/", "%2F").Replace("=", "%3D");
                postdata = String.Format(Certi_PostData, ViewState, EventValidation, Certi_No);
            }
            catch
            {

            }
            return postdata;
        }
        /// <summary>
        /// 获取详情页PostData
        /// </summary>
        /// <param name="Certi_No"></param>
        /// <param name="postdata"></param>
        /// <returns></returns>
        public static void GetDetailsPostData(string Certi_No, string postdata, ref ConcurrentQueue<string> postqueue)
        {
            String html = "start";
            try
            {
                while (!html.Contains("ERAC National Certification Database"))
                {
                    HttpInfo info = new HttpInfo();
                    info.RequestUrl = Url;
                    info.PostData = postdata;
                    info.KeepLive = true;
                    html = HttpMethod.HttpWork(info);
                    Thread.Sleep(1);
                }
                List<string> list = RegexMethod.GetMutResult(reg_arget, html, 1);
                String ViewState = "__VIEWSTATE=" + RegexMethod.GetSingleResult(reg_view, html, 1).Replace("+", "%2B").Replace("/", "%2F").Replace("=", "%3D");
                String EventValidation = "__EVENTVALIDATION=" + RegexMethod.GetSingleResult(reg_event, html, 1).Replace("+", "%2B").Replace("/", "%2F").Replace("=", "%3D");
                if (list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        String detailpost = String.Format(Details_PostData, "__EVENTTARGET=" + item.Replace("&#39;", "").Replace("$", "%24"), ViewState, EventValidation, Certi_No);
                        postqueue.Enqueue(detailpost);
                    }
                }
            }
            catch
            {

            }
        }
        #endregion
    }
}
