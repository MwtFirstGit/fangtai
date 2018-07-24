/*任务执行抽象基类 用于实现层的方法约束
 * 
 * 
 */

using System;
using System.Collections.Generic;

namespace ICerSpiderTaskLib
{
    public abstract class CerSpiderBase : ICerSpiderTask
    {
        /// <summary>
        /// 抽象类中默认实现
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parms"></param>
        /// <returns></returns>
        public virtual List<T> GetTask<T>(object[] parms)
        {
            throw new System.NotImplementedException();
        }
        /// <summary>
        /// 抽象方法 需要派生类实现
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parms"></param>
        /// <returns></returns>
        public abstract List<T> RunTask<T>(object[] parms);

        /// <summary>
        /// 抽象类中默认实现
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parms"></param>
        /// <returns></returns>
        public virtual List<T> UploadData<T>(object[] parms)
        {
            throw new System.NotImplementedException();
        }
    }
}
