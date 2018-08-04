/*任务执行抽象基类 用于实现层的方法约束
 * 
 * 
 */

namespace ICerSpiderTaskLib
{
    public class SpiderEventArgs
    {
        public string SpiderInfo { get; set; } = string.Empty;
        public string TaskId = string.Empty;
        public SpiderEventArgs() { }
        public  SpiderEventArgs(string id,string spiderinfo)
        {
            TaskId = id;
            this.SpiderInfo = spiderinfo;
        }
    }

    public class InitEventArgs: SpiderEventArgs
    {
    }

    public class StartEventArgs : SpiderEventArgs
    {

    }
    public class FinishEventArgs : SpiderEventArgs
    {

    }
}