/*与服务端的交互操作
 * 
 * 
 */
using HttpToolsLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using TaskEntityLib;

namespace CerSpider
{
    class ServerMethod
    {
        internal static bool GetTask(out TaskEntity taskEntity)
        {
            bool flag = false;
            TaskEntity _task = new TaskEntity();
            String url = "http://118.242.208.90:8012{0}/SpiderTask/GetSpiderTask";
            var json = HttpMethod.FastGetMethod(url);
            if (json.LastIndexOf("}") == json.Length - 1)
            {
                var jobj = JsonConvert.DeserializeObject(json) as JObject;
                if (jobj != null)
                {
                    _task.certype = Convert.ToInt32(jobj["data"]["certype"]);
                    _task.taskid = Convert.ToString(jobj["data"]["taskid"]);
                    _task.threadnum = Convert.ToInt32(jobj["data"]["threadnum"]);
                    flag = true;
                }
                jobj = null;
            }
            taskEntity = _task;
            return flag;
            //throw new NotImplementedException();
        }
    }
}
