using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using System.Linq;
using market.Models;

namespace market.Services
{
    /// <summary>
    /// 操作日志服务，负责日志的记录、查询和管理
    /// </summary>
    public class LogService
    {
        private readonly DatabaseService _databaseService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="databaseService">数据库服务依赖</param>
        public LogService(DatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        }

        /// <summary>
        /// 获取操作日志列表（分页）
        /// </summary>
        /// <param name="operationType">操作类型（可选）</param>
        /// <param name="userId">用户ID（可选）</param>
        /// <param name="startTime">开始时间（可选）</param>
        /// <param name="endTime">结束时间（可选）</param>
        /// <param name="pageIndex">页码（从1开始）</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>日志列表和总记录数</returns>
        public Tuple<List<OperationLog>, int> GetOperationLogs(string operationType = null, string userId = null, 
            DateTime? startTime = null, DateTime? endTime = null, int pageIndex = 1, int pageSize = 20)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();

                    // 构建查询条件
                    var conditions = new List<string>();
                    var parameters = new List<MySqlParameter>();

                    if (!string.IsNullOrEmpty(operationType))
                    {
                        conditions.Add("OperationType LIKE @OperationType");
                        parameters.Add(new MySqlParameter("@OperationType", "%" + operationType + "%"));
                    }

                    if (!string.IsNullOrEmpty(userId))
                    {
                        conditions.Add("UserId LIKE @UserId");
                        parameters.Add(new MySqlParameter("@UserId", "%" + userId + "%"));
                    }

                    if (startTime.HasValue)
                    {
                        conditions.Add("OperationTime >= @StartTime");
                        parameters.Add(new MySqlParameter("@StartTime", startTime.Value));
                    }

                    if (endTime.HasValue)
                    {
                        conditions.Add("OperationTime <= @EndTime");
                        parameters.Add(new MySqlParameter("@EndTime", endTime.Value));
                    }

                    var whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";

                    // 查询总记录数
                    var countQuery = $"SELECT COUNT(*) FROM OperationLogs {whereClause}";
                    using (var countCommand = new MySqlCommand(countQuery, connection))
                    {
                        countCommand.Parameters.AddRange(parameters.ToArray());
                        var totalCount = Convert.ToInt32(countCommand.ExecuteScalar());

                        // 查询分页数据，关联用户表获取用户名
                        var query = $@"SELECT ol.*, u.Username 
                                       FROM OperationLogs ol
                                       LEFT JOIN Users u ON ol.UserId = u.Id
                                       {whereClause}
                                       ORDER BY ol.OperationTime DESC
                                       LIMIT @Limit OFFSET @Offset";

                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddRange(parameters.ToArray());
                            command.Parameters.Add(new MySqlParameter("@Limit", pageSize));
                        command.Parameters.Add(new MySqlParameter("@Offset", (pageIndex - 1) * pageSize));

                            var logs = new List<OperationLog>();
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    logs.Add(new OperationLog
                                    {
                                        Id = Convert.ToInt32(reader["Id"]),
                                        OperationType = reader["OperationType"].ToString(),
                                        UserId = reader["UserId"].ToString(),
                                        OperationTime = Convert.ToDateTime(reader["OperationTime"]),
                                        Details = reader["Details"]?.ToString(),
                                        Username = reader["Username"]?.ToString() ?? "系统"
                                    });
                                }
                            }

                            return Tuple.Create(logs, totalCount);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取操作日志失败: {ex.Message}");
                return Tuple.Create(new List<OperationLog>(), 0);
            }
        }

        /// <summary>
        /// 获取所有操作类型
        /// </summary>
        /// <returns>操作类型列表</returns>
        public List<string> GetAllOperationTypes()
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    var query = "SELECT DISTINCT OperationType FROM OperationLogs ORDER BY OperationType";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        var types = new List<string>();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                types.Add(reader["OperationType"].ToString());
                            }
                        }
                        return types;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取操作类型失败: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// 记录操作日志
        /// </summary>
        /// <param name="operationType">操作类型</param>
        /// <param name="userId">用户ID</param>
        /// <param name="details">操作详情</param>
        public void LogOperation(string operationType, string userId, string details)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    var query = @"
                        INSERT INTO OperationLogs (OperationType, UserId, OperationTime, Details) 
                        VALUES (@OperationType, @UserId, @OperationTime, @Details)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OperationType", operationType);
                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@OperationTime", DateTime.Now);
                        command.Parameters.AddWithValue("@Details", details);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // 日志记录失败不影响主要功能
                System.Diagnostics.Debug.WriteLine($"日志记录失败: {ex.Message}");
            }
        }
    }
}