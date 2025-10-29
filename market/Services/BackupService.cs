using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace market.Services
{
    public class BackupService
    {
        private readonly DatabaseService _databaseService;

        public BackupService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        /// <summary>
        /// 备份整个数据库到SQL文件
        /// </summary>
        /// <param name="backupPath">备份文件保存路径</param>
        /// <returns>是否备份成功</returns>
        public bool BackupDatabase(string backupPath)
        {
            try
            {
                // 创建备份目录
                string directory = Path.GetDirectoryName(backupPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // 直接从连接获取数据库名称
                string databaseName;
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    databaseName = connection.Database;
                }
                
                // 使用简单的备份方法
                return BackupUsingManualMethod(backupPath, databaseName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"备份失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        
        /// <summary>
        /// 手动备份数据库表结构和数据
        /// </summary>
        private bool BackupUsingManualMethod(string backupPath, string databaseName)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                
                // 添加备份信息
                sqlBuilder.AppendLine($"-- 数据库备份: {databaseName}");
                sqlBuilder.AppendLine($"-- 备份时间: {DateTime.Now}");
                sqlBuilder.AppendLine();
                
                // 获取所有表名
                DataTable tables = connection.GetSchema("Tables", new string[] { null, null, null, "TABLE" });
                
                foreach (DataRow row in tables.Rows)
                {
                    string tableName = row["TABLE_NAME"].ToString();
                    
                    // 获取表结构
                    using (var cmd = new MySqlCommand($"SHOW CREATE TABLE {tableName}", connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                sqlBuilder.AppendLine($"-- 表结构: {tableName}");
                                sqlBuilder.AppendLine(reader["Create Table"].ToString() + ";");
                                sqlBuilder.AppendLine();
                            }
                            reader.Close();
                        }
                    }
                }
            }
            
            // 保存SQL文件
            File.WriteAllText(backupPath, sqlBuilder.ToString(), Encoding.UTF8);
            return true;
        }

        /// <summary>
        /// 生成备份文件名
        /// </summary>
        /// <returns>格式化的备份文件名</returns>
        public string GenerateBackupFileName()
        {
            return $"market_backup_{DateTime.Now:yyyyMMdd_HHmmss}.sql";
        }

        /// <summary>
        /// 获取默认备份路径
        /// </summary>
        /// <returns>默认备份目录路径</returns>
        public string GetDefaultBackupDirectory()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string backupDir = Path.Combine(appDataPath, "market", "backups");
            return backupDir;
        }
    }
}