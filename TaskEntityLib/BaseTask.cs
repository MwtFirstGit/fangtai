using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskEntityLib
{
    public class BaseTask
    {
        /// <summary>
        /// 证书类型
        /// </summary>
        public int certype { get; set; }

        /// <summary>
        /// 线程数
        /// </summary>
        public int threadnum { get; set; } = 1;
    }
}
