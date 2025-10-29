using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace market.Services
{
    /// <summary>
    /// 机器码获取服务
    /// </summary>
    public class MachineCodeService
    {
        /// <summary>
        /// 获取机器唯一标识码（主板信息的后四位）
        /// </summary>
        /// <returns>机器码后四位</returns>
        public static string GetMachineCode()
        {
            try
            {
                // 获取主板序列号
                string motherboardId = GetMotherboardId();
                
                if (string.IsNullOrEmpty(motherboardId))
                {
                    // 如果无法获取主板信息，使用处理器ID作为备选
                    motherboardId = GetProcessorId();
                }
                
                if (string.IsNullOrEmpty(motherboardId))
                {
                    // 如果还是无法获取，使用系统卷序列号
                    motherboardId = GetVolumeSerialNumber();
                }
                
                // 如果所有方法都失败，生成一个基于机器名的哈希
                if (string.IsNullOrEmpty(motherboardId))
                {
                    motherboardId = Environment.MachineName;
                }
                
                // 计算MD5哈希并取后四位
                return GetHashLastFour(motherboardId);
            }
            catch (Exception)
            {
                // 如果出错，返回默认机器码
                return "D001";
            }
        }
        
        /// <summary>
        /// 获取主板序列号
        /// </summary>
        private static string GetMotherboardId()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard"))
                using (var collection = searcher.Get())
                {
                    foreach (ManagementObject obj in collection)
                    {
                        string serial = obj["SerialNumber"]?.ToString();
                        if (!string.IsNullOrEmpty(serial))
                        {
                            return serial.Trim();
                        }
                    }
                }
            }
            catch
            {
                // 忽略异常，返回空字符串
            }
            
            return string.Empty;
        }
        
        /// <summary>
        /// 获取处理器ID
        /// </summary>
        private static string GetProcessorId()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                using (var collection = searcher.Get())
                {
                    foreach (ManagementObject obj in collection)
                    {
                        string processorId = obj["ProcessorId"]?.ToString();
                        if (!string.IsNullOrEmpty(processorId))
                        {
                            return processorId.Trim();
                        }
                    }
                }
            }
            catch
            {
                // 忽略异常，返回空字符串
            }
            
            return string.Empty;
        }
        
        /// <summary>
        /// 获取系统卷序列号
        /// </summary>
        private static string GetVolumeSerialNumber()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk WHERE DeviceID='C:'"))
                using (var collection = searcher.Get())
                {
                    foreach (ManagementObject obj in collection)
                    {
                        string volumeSerial = obj["VolumeSerialNumber"]?.ToString();
                        if (!string.IsNullOrEmpty(volumeSerial))
                        {
                            return volumeSerial.Trim();
                        }
                    }
                }
            }
            catch
            {
                // 忽略异常，返回空字符串
            }
            
            return string.Empty;
        }
        
        /// <summary>
        /// 获取字符串的MD5哈希后四位
        /// </summary>
        private static string GetHashLastFour(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                
                // 将哈希字节转换为十六进制字符串
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                
                string hashString = sb.ToString();
                
                // 取后四位，如果不足四位则补零
                if (hashString.Length >= 4)
                {
                    return hashString.Substring(hashString.Length - 4).ToUpper();
                }
                else
                {
                    return hashString.PadLeft(4, '0').ToUpper();
                }
            }
        }
    }
}