/*任务执行抽象基类 用于实现层的方法约束
 * 
 * 
 */

using ExtractLib;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ICerSpiderTaskLib
{
    public abstract class CerSpiderBase : ICerSpiderTask
    {
        protected String CerType_Str = "基类任务";

        #region 常量声明
        protected const String Base_CerNum_SplitReg = "\\r\\s";
        #endregion

        #region 事件和委托
        public delegate void InitEventHandler(object sender, InitEventArgs ies);
        public event InitEventHandler InitEvent;

        public delegate void StartEventHandler(object sender, StartEventArgs ies);
        public event StartEventHandler StartEvent;

        public delegate void FinishEventHandler(object sender, FinishEventArgs ies);
        public event FinishEventHandler FinishEvent;
        #endregion

        #region 公用部分
        /// <summary>
        /// 证书类型 派生类实例化时必须对此成员赋值
        /// </summary>
        protected int CerType { get; set; } = 0;

        /// <summary>
        /// 待执行的证书号队列
        /// </summary>
        protected ConcurrentQueue<String> CerQueue { get; set; } = new ConcurrentQueue<String>();

        /// <summary>
        /// 上传队列
        /// </summary>
        protected ConcurrentQueue<object> UpLoadQueue { get; set; } = new ConcurrentQueue<object>();

        #endregion

        #region 接口方法
        /// <summary>
        /// 抽象类中默认实现 传入证书号文本的绝对路径   读取证书号加入到队列中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parms">证书号文件绝对路径</param>
        /// <returns></returns>
        public virtual void GetTask(object[] parms)
        {
            String path = String.Empty;

            #region 文件和路径参数校验
            if (parms == null || parms?.Length == 0)
            {
                throw new Exception("传入路径参数错误");
            }
            else
            {
                path = parms[0] as String;
                if (String.IsNullOrEmpty(path))
                {
                    throw new Exception("证书号路径为空");
                }
                else if (!File.Exists(path))
                {
                    throw new Exception($"{path}不存在");
                }
            }
            #endregion

            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    String cernums_str = sr.ReadToEnd();
                    var nums = RegexMethod.RegSplit(Base_CerNum_SplitReg, cernums_str);
                    nums?.ToList().ForEach(num => {
                        if (!String.IsNullOrEmpty(num)) CerQueue.Enqueue(num);
                    });
                    Console.WriteLine("成功装载{0}条证书号", nums.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("读取证书号失败");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }
        /// <summary>
        /// 抽象方法 需要派生类实现
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parms"></param>
        /// <returns></returns>
        public abstract void RunTask(object[] parms);

        /// <summary>
        /// 抽象类中默认实现
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parms"></param>
        /// <returns></returns>
        public virtual void UploadData(object[] parms)
        {
            String path = String.Empty;
            #region 文件和路径参数校验
            if (parms == null || parms?.Length == 0)
            {
                throw new Exception("传入路径参数错误");
            }
            else
            {
                path = parms[0] as String;
                if (String.IsNullOrEmpty(path))
                {
                    throw new Exception("输出路径为空");
                }
                else if (!File.Exists(path))
                {
                    throw new Exception($"{path}不存在");
                }
            }
            #endregion

            String str = JsonConvert.SerializeObject(UpLoadQueue);
            UpLoadQueue = new ConcurrentQueue<object>();
            //同一天的采集会覆盖
            using (StreamWriter sw = new StreamWriter(path, true))
            {
                sw.Write(str);
            }
        }
        #endregion

        #region 事件触发方法
        protected virtual void OnInit(InitEventArgs ies)
        {
            Console.WriteLine($"{ies.SpiderInfo}初始化完毕");
            InitEvent?.Invoke(this, ies);
        }

        protected virtual void OnStart(StartEventArgs ses)
        {
            Console.WriteLine($"{ses.SpiderInfo}初始化完毕");
            StartEvent?.Invoke(this, ses);
        }
        protected virtual void OnInit(FinishEventArgs fes)
        {
            Console.WriteLine($"{fes.SpiderInfo}初始化完毕");
            FinishEvent?.Invoke(this, fes);
        }
        #endregion
    }
}
