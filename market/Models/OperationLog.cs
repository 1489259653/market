using System;

namespace market.Models
{
    /// <summary>
    /// 操作日志模型
    /// </summary>
    public class OperationLog
    {
        /// <summary>
        /// 日志ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        public string OperationType { get; set; }

        /// <summary>
        /// 操作用户ID
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 操作时间
        /// </summary>
        public DateTime OperationTime { get; set; }

        /// <summary>
        /// 操作详情
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// 操作用户名称（非数据库字段，用于显示）
        /// </summary>
        public string Username { get; set; }
    }
}