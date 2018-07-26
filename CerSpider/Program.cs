/*宿主壳程序  功能如下:
 * 1 组件更新校验
 * 2 初始化
 * 3 核心功能入口
 * 4 基础逻辑控制
 * 
 */

using CerSpidersLib;
using System;
using System.Threading.Tasks;
using System.Timers;
using TaskEntityLib;

namespace CerSpider
{
    class Program
    {
        /// <summary>
        /// 接受命令定时器   更新 任务执行都通过定时器实现   接受到更新命令时会触发更新事件
        /// </summary>
        static Timer rev_timer = new Timer();

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
            throw new NotImplementedException();
        }

        private static void InitFunc()
        {
            //组件版本读取

            //服务端网络联通校验

            //任务证书号目录校验

            //输出目录校验
            throw new NotImplementedException();
        }
    }
}
