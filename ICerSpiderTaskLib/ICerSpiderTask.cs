using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICerSpiderTaskLib
{
    public interface ICerSpiderTask
    {
        /// <summary>
        /// 获取任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parms"></param>
        /// <returns></returns>
        void GetTask(object []parms);
        /// <summary>
        /// 执行任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parms"></param>
        /// <returns></returns>
        void RunTask(object[] parms);
        /// <summary>
        /// 上传数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parms"></param>
        /// <returns></returns>
        void UploadData(object[] parms);

    }
}
