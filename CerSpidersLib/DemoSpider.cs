/*派生类Demo
 * 
 * >>>>>>>>>>>>>>>>>>>>>>>至少需要重写基类RunTask方法<<<<<<<<<<<<<<<<<<<<<<<<<<<
 * 
 */
using ICerSpiderTaskLib;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System;
using System.Threading;
using CerInfoEntityLib;

namespace CerSpidersLib
{
  public class DemoSpider: CerSpiderBase
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public DemoSpider() {
            this.CerType =0;
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
            throw new Exception("调用了基类执行任务方法");
            #region 执行任务结构示例
            String cernum = String.Empty;

            while (CerQueue.TryDequeue(out cernum))
            {
                DemoEntity updata;
                /*这里写执行任务相关代码
                 *
                 */

                //上传数据入队
                UpLoadQueue.Enqueue(updata);

                Thread.Sleep(1);
            }

            #endregion

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
