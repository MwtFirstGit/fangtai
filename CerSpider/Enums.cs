/*公用枚举
 * 
 * 
 * 
 */

namespace CerSpider
{
    /// <summary>
    /// 证书类型
    /// </summary>
    public enum CerType { CCC, CQC, UL, ENEC, VDE, TUVLY, TUVND, BSI, ASTA, ERAC }
    /// <summary>
    /// 任务类型 临时任务和定时任务
    /// </summary>
    public enum TaskType { Temp,Timing}
    /// <summary>
    /// 爬虫任务状态
    /// </summary>
    public enum SpiderStatue {Wait,Start,Finish }
}
