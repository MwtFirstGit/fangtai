using ICerSpiderTaskLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CerSpidersLib
{
    public class BSISpider : CerSpiderBase
    {
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
                //String html = CCC_Details(cernum);
                //Dictionary<String, String> updata = ;
                Console.WriteLine($"BSI证书号{cernum}完毕");

                //上传数据入队
                //UpLoadQueue.Enqueue(updata);

                Thread.Sleep(1);
            }
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
