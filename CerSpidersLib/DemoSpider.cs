/*派生类Demo
 * 
 * >>>>>>>>>>>>>>>>>>>>>>>至少需要重写基类RunTask方法<<<<<<<<<<<<<<<<<<<<<<<<<<<
 * 
 */
using ICerSpiderTaskLib;
using System.Collections.Generic;

namespace CerSpidersLib
{
  public class DemoSpider: CerSpiderBase
    {
        /// <summary>
        /// 使用基类方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parms"></param>
        /// <returns></returns>
        public override List<T> GetTask<T>(object[] parms)
        {
            return base.GetTask<T>(parms);
        }

        /// <summary>
        /// 重写基类方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parms"></param>
        /// <returns></returns>
        public override List<T> RunTask<T>(object[] parms)
        {
            return null;
        }
        /// <summary>
        /// 使用基类方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parms"></param>
        /// <returns></returns>
        public override List<T> UploadData<T>(object[] parms)
        {
            return base.UploadData<T>(parms);
        }
    }
}
