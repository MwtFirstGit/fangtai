using HttpToolsLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TaskEntityLib;

namespace RePublicLib
{
    public class PublicMethod
    {

        const String Host = "http://118.24.208.90:8012{0}";
        /// <summary>
        /// 上传数据
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        public static bool UpLoadFunc(string str,int certype)
        {
            bool flag = false;
            String url = String.Format(Host, $"/FangTaiData/InsertCerData?CerType={certype}&DataSrc=1");
            HttpInfo info = new HttpInfo(url);
            info.PostData = HttpMethod.URLEncode(str);
            String retstr = "start";
            while (!retstr.ToUpper().Contains("OK") || retstr.Trim().LastIndexOf("}") != retstr.Length - 1)
            {
                retstr = HttpMethod.HttpWork(info);
                Thread.Sleep(1);
                flag = true;
            }
            Console.WriteLine($"{retstr}\r{DateTime.Now}");
            info = null;
            retstr = null;
            //postdata = null;
            return flag;
        }

        //public static void GetTask(ConcurrentQueue<String> runtaskqueue, TaskEntity taskEntity)
        //{
        //    new Task(() =>
        //    {
        //        Console.WriteLine("开启获取任务");
        //        while (true)
        //        {
        //            if (runtaskqueue.Count == 0)
        //            {
        //                taskEntity = PublicMethod.GetTaskPublic(ref runtaskqueue);
        //                if (!String.IsNullOrEmpty(taskEntity.taskid))
        //                {
        //                    Console.WriteLine($"成功获取到taskid为{taskEntity.taskid}的任务");
        //                }
        //            }
        //        }
        //    });
        //}
        /// <summary>
        /// 获取任务
        /// </summary>
        /// <param name="CerQueue"></param>
        public static TaskEntity GetTaskPublic(ref ConcurrentQueue<String> CerQueue)
        {
            TaskEntity taskEntity = new TaskEntity();
            String url = String.Format(Host,"/SpiderTask/GetSpiderTask");
            var json = HttpMethod.FastGetMethod(url);
            if(json.LastIndexOf("}") == json.Length - 1)
            {
                var jobj = JsonConvert.DeserializeObject(json) as JObject;
                if (jobj != null)
                {
                    taskEntity.certype = Convert.ToInt32(jobj["data"]["certype"]);
                    taskEntity.taskid = Convert.ToString(jobj["data"]["taskid"]);
                    taskEntity.threadnum = Convert.ToInt32(jobj["data"]["threadnum"]);
                    if (jobj["data"]["cernums"] is JArray jarr)
                    {
                        if (jarr.Count > 0)
                        {
                            foreach(var item in jarr)
                            {
                                CerQueue.Enqueue(item.ToString());
                            }
                        }
                        jarr = null;
                    }
                }
                jobj = null;
            }
            return taskEntity;
        }
    }
}
