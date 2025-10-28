using System;
using market.Services;

namespace market
{
    /// <summary>
    /// 数据初始化程序
    /// </summary>
    public class DataInitializer
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== 进货管理模块测试数据初始化程序 ===");
            Console.WriteLine();
            
            try
            {
                // 初始化数据库服务
                var databaseService = new DatabaseService();
                
                // 测试数据库连接
                Console.WriteLine("正在测试数据库连接...");
                if (!databaseService.TestConnection())
                {
                    Console.WriteLine("数据库连接失败，请检查MariaDB服务器是否正常运行。");
                    return;
                }
                Console.WriteLine("数据库连接成功！");
                
                // 验证数据库表
                Console.WriteLine("正在验证数据库表...");
                if (!databaseService.ValidateDatabaseTables())
                {
                    Console.WriteLine("数据库表验证失败，可能需要重新创建数据库。");
                    return;
                }
                Console.WriteLine("数据库表验证成功！");
                
                // 创建测试数据服务
                var testDataService = new TestDataService(databaseService);
                
                Console.WriteLine();
                Console.WriteLine("即将创建进货管理模块的测试数据...");
                Console.WriteLine("包括：");
                Console.WriteLine("- 3个测试用户（管理员、仓库管理员、收银员）");
                Console.WriteLine("- 5个供应商");
                Console.WriteLine("- 10个商品");
                Console.WriteLine("- 9个进货单（各种状态）");
                Console.WriteLine("- 对应的进货明细和库存变动记录");
                Console.WriteLine();
                
                Console.Write("是否继续？(Y/N): ");
                var response = Console.ReadLine();
                
                if (response?.ToUpper() != "Y")
                {
                    Console.WriteLine("操作已取消。");
                    return;
                }
                
                Console.WriteLine();
                Console.WriteLine("正在创建测试数据...");
                
                var success = testDataService.CreatePurchaseTestData();
                
                if (success)
                {
                    Console.WriteLine();
                    Console.WriteLine("✅ 测试数据创建成功！");
                    Console.WriteLine();
                    Console.WriteLine("数据使用说明：");
                    Console.WriteLine("1. 用户登录信息：");
                    Console.WriteLine("   - 管理员: admin / admin123");
                    Console.WriteLine("   - 仓库管理员: warehouse / warehouse123");
                    Console.WriteLine("   - 收银员: cashier / cashier123");
                    Console.WriteLine();
                    Console.WriteLine("2. 进货单状态分布：");
                    Console.WriteLine("   - 已完成: 3个订单");
                    Console.WriteLine("   - 已审核: 2个订单");
                    Console.WriteLine("   - 待审核: 2个订单");
                    Console.WriteLine("   - 已到货: 1个订单");
                    Console.WriteLine("   - 已取消: 1个订单");
                    Console.WriteLine();
                    Console.WriteLine("3. 您现在可以正常使用进货管理功能进行测试。");
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("❌ 测试数据创建失败！");
                    Console.WriteLine("请检查数据库连接和表结构。");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 初始化过程中出现错误: {ex.Message}");
                Console.WriteLine($"详细错误信息: {ex.InnerException?.Message}");
            }
            
            Console.WriteLine();
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }
    }
}