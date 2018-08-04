﻿/*公用枚举
 * 
 * 
 * 
 */

namespace CerSpider
{
    /// <summary>
    /// 证书类型
    /// </summary>
    public enum CerType {DEMO,VDE ,UL,TUV}
    /// <summary>
    /// 任务类型 
    /// </summary>
    public enum TaskType {None,Update ,RunTask }
    /// <summary>
    /// 爬虫任务状态
    /// </summary>
    public enum SpiderStatue {Wait,Start,Finish }
}