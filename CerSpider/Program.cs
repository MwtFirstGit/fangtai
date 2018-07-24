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

namespace CerSpider
{
    class Program
    {
        static void Main(string[] args)
        {
            #region 初始化
            InitFunc();
            #endregion

            #region 更新校验模块
            Task.Run(()=> { UpdateThread(); });
            #endregion

            #region 功能入口
            Start();
            #endregion
        }

        private static void Start()
        {
            DemoSpider demoSpider = new DemoSpider();
            demoSpider.
            throw new NotImplementedException();
        }

        private static void InitFunc()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 检查更新线程
        /// </summary>
        private static void UpdateThread()
        {
            throw new NotImplementedException();
        }
    }
}
