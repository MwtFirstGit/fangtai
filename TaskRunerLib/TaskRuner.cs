
using System;
using System.IO;
using TaskEntityLib;
/// <summary>
///任务运行中间操作
/// </summary>
namespace TaskRunerLib
{
   public class TaskRuner
    {
        public static void StartTask(TaskEntity taskEntity)
        {
            var taskType = (TaskType)taskEntity.tasktype;
            //任务地址
            String taskpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Enum.GetName(typeof(TaskType), taskType));

            if(taskType == TaskType.Update)
            {

            }
            else
            {

            }
          
        }
    }
}
