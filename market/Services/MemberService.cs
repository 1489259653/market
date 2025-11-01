using MySql.Data.MySqlClient;
using market.Models;
using System.Data;

namespace market.Services
{
    /// <summary>
    /// 会员管理服务类
    /// </summary>
    public class MemberService
    {
        private readonly DatabaseService _databaseService;

        public MemberService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        /// <summary>
        /// 初始化会员表
        /// </summary>
        public void InitializeMemberTable()
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                string sql = @"CREATE TABLE IF NOT EXISTS members (
                                id VARCHAR(50) PRIMARY KEY,
                                name VARCHAR(100) NOT NULL,
                                phone_number VARCHAR(20),
                                email VARCHAR(100),
                                registration_date DATETIME NOT NULL,
                                points DECIMAL(10,2) DEFAULT 0,
                                level INT NOT NULL DEFAULT 0,
                                discount DECIMAL(5,2) NOT NULL DEFAULT 1.00
                              )";
                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.ExecuteNonQuery();
                }
                
                // 如果表已存在但缺少discount字段，则添加该字段
                sql = @"ALTER TABLE members ADD COLUMN IF NOT EXISTS discount DECIMAL(5,2) NOT NULL DEFAULT 1.00";
                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 添加会员
        /// </summary>
        public void AddMember(Member member)
        {
            // 生成会员ID：哈尔滨区号(0451) + 年月日 + 6位随机数
            string datePart = DateTime.Now.ToString("yyyyMMdd");
            string randomPart = new Random().Next(100000, 999999).ToString();
            member.Id = "0451" + datePart + randomPart;
            
            // 设置默认折扣率
            if (member.Discount == 0)
            {
                member.Discount = GetDiscountByLevel(member.Level);
            }
            
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                string sql = @"INSERT INTO members (id, name, phone_number, email, registration_date, points, level, discount) 
                              VALUES (@Id, @Name, @PhoneNumber, @Email, @RegistrationDate, @Points, @Level, @Discount)";
                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", member.Id);
                    cmd.Parameters.AddWithValue("@Name", member.Name);
                    cmd.Parameters.AddWithValue("@PhoneNumber", member.PhoneNumber);
                    cmd.Parameters.AddWithValue("@Email", member.Email);
                    cmd.Parameters.AddWithValue("@RegistrationDate", member.RegistrationDate);
                    cmd.Parameters.AddWithValue("@Points", member.Points);
                    cmd.Parameters.AddWithValue("@Level", (int)member.Level);
                    cmd.Parameters.AddWithValue("@Discount", member.Discount);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 更新会员信息
        /// </summary>
        public void UpdateMember(Member member)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                string sql = @"UPDATE members SET name = @Name, phone_number = @PhoneNumber, email = @Email, 
                              points = @Points, level = @Level, discount = @Discount WHERE id = @Id";
                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", member.Id);
                    cmd.Parameters.AddWithValue("@Name", member.Name);
                    cmd.Parameters.AddWithValue("@PhoneNumber", member.PhoneNumber);
                    cmd.Parameters.AddWithValue("@Email", member.Email);
                    cmd.Parameters.AddWithValue("@Points", member.Points);
                    cmd.Parameters.AddWithValue("@Level", (int)member.Level);
                    cmd.Parameters.AddWithValue("@Discount", member.Discount);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 删除会员
        /// </summary>
        public void DeleteMember(string id)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                string sql = "DELETE FROM members WHERE id = @Id";
                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 根据ID获取会员
        /// </summary>
        public Member GetMemberById(string id)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                string sql = "SELECT * FROM members WHERE id = @Id";
                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Member
                            {
                                Id = reader["id"].ToString(),
                                Name = reader["name"].ToString(),
                                PhoneNumber = reader["phone_number"].ToString(),
                                Email = reader["email"].ToString(),
                                RegistrationDate = Convert.ToDateTime(reader["registration_date"]),
                                Points = Convert.ToDecimal(reader["points"]),
                                Level = (MemberLevel)Convert.ToInt32(reader["level"]),
                                Discount = Convert.ToDecimal(reader["discount"])
                            };
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 获取所有会员
        /// </summary>
        public List<Member> GetAllMembers()
        {
            var members = new List<Member>();
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                string sql = "SELECT * FROM members ORDER BY registration_date DESC";
                using (var cmd = new MySqlCommand(sql, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            members.Add(new Member
                            {
                                Id = reader["id"].ToString(),
                                Name = reader["name"].ToString(),
                                PhoneNumber = reader["phone_number"].ToString(),
                                Email = reader["email"].ToString(),
                                RegistrationDate = Convert.ToDateTime(reader["registration_date"]),
                                Points = Convert.ToDecimal(reader["points"]),
                                Level = (MemberLevel)Convert.ToInt32(reader["level"]),
                                Discount = Convert.ToDecimal(reader["discount"])
                            });
                        }
                    }
                }
            }
            return members;
        }

        /// <summary>
        /// 根据手机号查找会员
        /// </summary>
        public Member GetMemberByPhone(string phoneNumber)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                string sql = "SELECT * FROM members WHERE phone_number = @PhoneNumber";
                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Member
                            {
                                Id = reader["id"].ToString(),
                                Name = reader["name"].ToString(),
                                PhoneNumber = reader["phone_number"].ToString(),
                                Email = reader["email"].ToString(),
                                RegistrationDate = Convert.ToDateTime(reader["registration_date"]),
                                Points = Convert.ToDecimal(reader["points"]),
                                Level = (MemberLevel)Convert.ToInt32(reader["level"]),
                                Discount = Convert.ToDecimal(reader["discount"])
                            };
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 更新会员积分
        /// </summary>
        public void UpdatePoints(string memberId, decimal pointsToAdd)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                string sql = "UPDATE members SET points = points + @PointsToAdd WHERE id = @MemberId";
                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@MemberId", memberId);
                    cmd.Parameters.AddWithValue("@PointsToAdd", pointsToAdd);
                    cmd.ExecuteNonQuery();
                }
                
                // 更新会员等级
                UpdateMemberLevel(memberId);
            }
        }

        /// <summary>
        /// 更新会员等级和折扣率
        /// </summary>
        private void UpdateMemberLevel(string memberId)
        {
            // 根据积分更新等级和折扣率：0-1000铜，1001-3000银，3001-5000金，5000+铂金
            string sql = @"UPDATE members SET 
                          level = CASE 
                              WHEN points >= 5000 THEN 3
                              WHEN points >= 3000 THEN 2
                              WHEN points >= 1000 THEN 1
                              ELSE 0
                          END,
                          discount = CASE
                              WHEN points >= 5000 THEN 0.85  -- 铂金会员 85折
                              WHEN points >= 3000 THEN 0.90  -- 金牌会员 9折
                              WHEN points >= 1000 THEN 0.95  -- 银牌会员 95折
                              ELSE 0.98  -- 铜牌会员 98折
                          END
                          WHERE id = @MemberId";
            
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@MemberId", memberId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        
        /// <summary>
        /// 根据会员等级获取折扣率
        /// </summary>
        private decimal GetDiscountByLevel(MemberLevel level)
        {
            switch (level)
            {
                case MemberLevel.Bronze:
                    return 0.98m; // 铜牌会员 98折
                case MemberLevel.Silver:
                    return 0.95m; // 银牌会员 95折
                case MemberLevel.Gold:
                    return 0.90m; // 金牌会员 9折
                case MemberLevel.Platinum:
                    return 0.85m; // 铂金会员 85折
                default:
                    return 1.00m; // 默认不打折
            }
        }
        
        /// <summary>
        /// 根据手机号或姓名搜索会员
        /// </summary>
        public List<Member> SearchMembers(string keyword)
        {
            var members = new List<Member>();
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                string sql = @"SELECT * FROM members WHERE phone_number LIKE @Keyword OR name LIKE @Keyword ORDER BY registration_date DESC";
                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            members.Add(new Member
                            {
                                Id = reader["id"].ToString(),
                                Name = reader["name"].ToString(),
                                PhoneNumber = reader["phone_number"].ToString(),
                                Email = reader["email"].ToString(),
                                RegistrationDate = Convert.ToDateTime(reader["registration_date"]),
                                Points = Convert.ToDecimal(reader["points"]),
                                Level = (MemberLevel)Convert.ToInt32(reader["level"]),
                                Discount = Convert.ToDecimal(reader["discount"])
                            });
                        }
                    }
                }
            }
            return members;
        }
    }
}