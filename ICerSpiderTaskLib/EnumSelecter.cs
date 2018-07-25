/*此类集中复用简化泛型和switch枚举判断
 * 
 * 
 */


using System;
using System.IO;

namespace ICerSpiderTaskLib
{
  public class EnumSelecter
    {
        /// <summary>
        /// 获得任务路径
        /// </summary>
        /// <param name="folder">根目录</param>
        /// <param name="cerType">证书枚举类型</param>
        /// <returns></returns>
        public static string GetTaskCerPath(String folder, CerType cerType)
        {
            return Path.Combine(folder,Enum.GetName(typeof(CerType),cerType).ToUpper()+".txt");
        }

        /// <summary>
        /// 获得输出路径
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="cerType"></param>
        /// <returns></returns>
        public static String GetOutPutPath(String folder,CerType cerType)
        {
            String second_folder = Path.Combine(folder, Enum.GetName(typeof(CerType), cerType).ToUpper());
            if(!Directory.Exists(second_folder))
            {
                Directory.CreateDirectory(second_folder);
            }
            String path = Path.Combine(second_folder, DateTime.Now.ToString("yyyyMMdd") + ".txt");
            if(!File.Exists(path))
            {
                File.Create(path).Close();
            }
            return path;
        }
    }
}
