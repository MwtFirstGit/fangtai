/*宿主壳程序  功能如下:
 * 1 组件更新校验
 * 2 初始化
 * 3 核心功能入口
 * 4 基础逻辑控制
 * 
 */

using System;
using System.Timers;
using TaskEntityLib;
using CerSpidersLib;
using System.IO;
using System.Collections.Generic;
using PartEntityLib;
using HttpToolsLib;

namespace CerSpider
{
    class Program
    {
        #region 常量
        const String CerFolder = "CerNum";
        const String Host = "Host";
        const String UpdateAddress = "{0}/{1}";
        #endregion

        #region 全局变量

        #endregion

        static List<PartEntity> PartList = new List<PartEntity>();

        /// <summary>
        /// 是否需要更新
        /// </summary>
        static bool NeedUpdate = false;
        /// <summary>
        /// 接受命令定时器   更新 任务执行都通过定时器实现   接受到更新命令时会触发更新事件
        /// </summary>
        static Timer rev_timer = new Timer();
        /// <summary>
        /// 爬虫状态
        /// </summary>
        static Dictionary<String, SpiderStatue> SpiderDics { get; set; } = new Dictionary<string, SpiderStatue>();

        static void Main(string[] args)
        {
            #region 测试
            Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory);
            Console.ReadLine();
            #endregion

            #region 初始化
            InitFunc();
            #endregion

            rev_timer.AutoReset = true;
            rev_timer.Enabled = true;
            rev_timer.Interval = 5000;
            rev_timer.Elapsed += new ElapsedEventHandler(TimerFunc);
            rev_timer.Start();
        }

        private static void TimerFunc(object sender, ElapsedEventArgs e)
        {
            TaskEntity taskEntity = null;
            if(ServerMethod.GetTask(out taskEntity))
            {
                RunTask(taskEntity);
            }
        }
        /// <summary>
        /// 执行任务入口
        /// </summary>
        /// <param name="taskEntity"></param>
        private static void RunTask(TaskEntity taskEntity)
        {
            var tasktype = (TaskType)taskEntity.tasktype;
            switch (tasktype)
            {
                case TaskType.None:
                    break;
                case TaskType.Update:
                    break;
                case TaskType.RunTask:
                    SpiderDics.Add(taskEntity.taskid, SpiderStatue.Wait);
                    RunSpider(taskEntity);
                    SpiderDics[taskEntity.taskid] = SpiderStatue.Finish;
                    break;
                default:
                    break;
            }
        }

        private static void RunSpider(TaskEntity taskEntity)
        {
            SpiderDics[taskEntity.taskid] =  SpiderStatue.Start;
            var runtasktype = (CerType)taskEntity.runtype;
            object ins = EnumSelecter.Ins_Dic[runtasktype].Invoke();
            var type_ins = ins?.GetType();
            var gettaskmethod = type_ins.GetMethod("GetTask");
            var runtaskmethod = type_ins.GetMethod("RunTask");
            var uploadtaskmethod = type_ins.GetMethod("UploadData");
            object[] path = { EnumSelecter.GetTaskCerPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CerFolder), runtasktype) };
            gettaskmethod.Invoke(ins, path);
            runtaskmethod.Invoke(ins, null);
            uploadtaskmethod.Invoke(ins, null);
        }

        private static void InitFunc()
        {
            //组件版本读取
            InItPart();
            //服务端网络联通校验 同时进行更新校验
            NeedUpdate = CheckUpdate();
            //任务证书号目录校验

            //输出目录校验
            throw new NotImplementedException();
        }

        private static bool CheckUpdate()
        {
            String url = String.Format(UpdateAddress, Host);
            String html = HttpMethod.FastGetMethod(url);
            return CheckUpdateChar(html);
        }

        private static bool CheckUpdateChar(string html)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 组件版本读取
        /// </summary>
        private static void InItPart()
        {
          var files=  Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory);
           foreach(String path in files)
            {
                PartList.Add(new PartEntity(path));
            }
        }
    }
}
