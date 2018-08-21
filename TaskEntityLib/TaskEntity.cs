using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskEntityLib
{
    public class TaskEntity: BaseTask
    {
        /// <summary>
        /// 任务类型
        /// </summary>
        public int tasktype { get; set; }
        /// <summary>
        /// 任务id
        /// </summary>
        public string taskid { get; set; } 
        /// <summary>
        /// 创建时间
        /// </summary>
        public string createtime { get; set; }
   
    }
}
