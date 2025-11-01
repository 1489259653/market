using market.Models;
using market.Services;
using System;
using System.Windows.Forms;

namespace market.Forms
{
    public partial class MemberManagementForm : Form
    {
        private readonly MemberService _memberService;

        public MemberManagementForm(MemberService memberService)
        {
            InitializeComponent();
            _memberService = memberService;
            InitializeForm();
        }

        private void InitializeForm()
        {
            // 初始化会员表
            _memberService.InitializeMemberTable();
            // 加载会员数据
            LoadMembers();
            // 初始化会员等级下拉框
            cmbLevel.DataSource = Enum.GetValues(typeof(MemberLevel));
        }

        private void LoadMembers()
        {            
            dgvMembers.Rows.Clear();
            var members = _memberService.GetAllMembers();
            foreach (var member in members)
            {
                dgvMembers.Rows.Add(
                    member.Id,
                    member.Name,
                    member.PhoneNumber,
                    member.Email,
                    member.RegistrationDate.ToString("yyyy-MM-dd"),
                    member.Points,
                    member.Level.ToString(),
                    (member.Discount * 10).ToString("0.#") + "折"
                );
            }
        }

        private void btnAddMember_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("请输入会员姓名和手机号码", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 检查手机号是否已存在
            if (_memberService.GetMemberByPhone(txtPhone.Text) != null)
            {
                MessageBox.Show("该手机号已存在会员", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {                
                var selectedLevel = (MemberLevel)cmbLevel.SelectedItem;
                var member = new Member
                {                    
                    Id = Guid.NewGuid().ToString(),
                    Name = txtName.Text,
                    PhoneNumber = txtPhone.Text,
                    Email = txtEmail.Text,
                    RegistrationDate = DateTime.Now,
                    Points = 0,
                    Level = selectedLevel,
                    Discount = GetDiscountByLevel(selectedLevel) // 根据等级设置默认折扣
                };

                _memberService.AddMember(member);
                ClearInputs();
                LoadMembers();
                MessageBox.Show("会员添加成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("添加失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdateMember_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtId.Text))
            {
                MessageBox.Show("请选择要修改的会员", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var member = new Member
                {
                    Id = txtId.Text,
                    Name = txtName.Text,
                    PhoneNumber = txtPhone.Text,
                    Email = txtEmail.Text,
                    RegistrationDate = Convert.ToDateTime(dtpRegistrationDate.Value),
                    Points = Convert.ToDecimal(txtPoints.Text),
                    Level = (MemberLevel)cmbLevel.SelectedItem,
                    Discount = Convert.ToDecimal(txtDiscount.Text)
                };

                _memberService.UpdateMember(member);
                LoadMembers();
                MessageBox.Show("会员信息更新成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("更新失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDeleteMember_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtId.Text))
            {
                MessageBox.Show("请选择要删除的会员", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("确定要删除该会员吗？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    _memberService.DeleteMember(txtId.Text);
                    ClearInputs();
                    LoadMembers();
                    MessageBox.Show("会员删除成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("删除失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dgvMembers_CellClick(object sender, DataGridViewCellEventArgs e)
        {            
            if (e.RowIndex >= 0)
            {                
                var row = dgvMembers.Rows[e.RowIndex];
                txtId.Text = row.Cells["Id"].Value.ToString();
                txtName.Text = row.Cells["Name"].Value.ToString();
                txtPhone.Text = row.Cells["PhoneNumber"].Value.ToString();
                txtEmail.Text = row.Cells["Email"].Value.ToString();
                dtpRegistrationDate.Value = Convert.ToDateTime(row.Cells["RegistrationDate"].Value);
                txtPoints.Text = row.Cells["Points"].Value.ToString();
                cmbLevel.SelectedItem = Enum.Parse(typeof(MemberLevel), row.Cells["Level"].Value.ToString());
                
                // 从表格中的折扣字符串解析回折扣率
                string discountText = row.Cells["Discount"].Value.ToString();
                if (discountText.EndsWith("折"))
                {
                    discountText = discountText.Substring(0, discountText.Length - 1);
                    txtDiscount.Text = (Convert.ToDecimal(discountText) / 10).ToString("0.##");
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearInputs();
        }

        private void ClearInputs()
        {            
            txtId.Text = string.Empty;
            txtName.Text = string.Empty;
            txtPhone.Text = string.Empty;
            txtEmail.Text = string.Empty;
            dtpRegistrationDate.Value = DateTime.Now;
            txtPoints.Text = "0";
            cmbLevel.SelectedItem = MemberLevel.Bronze;
            txtDiscount.Text = GetDiscountByLevel(MemberLevel.Bronze).ToString("0.##");
        }
        
        private decimal GetDiscountByLevel(MemberLevel level)
        {
            switch (level)
            {
                case MemberLevel.Bronze: return 0.99m; // 99折
                case MemberLevel.Silver: return 0.90m;  // 9折
                case MemberLevel.Gold: return 0.85m;    // 85折
                case MemberLevel.Platinum: return 0.75m; // 75折
                default: return 1.0m;                   // 无折扣
            }
        }
        
        private void cmbLevel_SelectedIndexChanged(object sender, EventArgs e)
        {            
            // 无论新增还是编辑会员，当选择不同等级时自动更新折扣
            var selectedLevel = (MemberLevel)cmbLevel.SelectedItem;
            txtDiscount.Text = GetDiscountByLevel(selectedLevel).ToString("0.##");
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtSearchPhone.Text))
            {
                var member = _memberService.GetMemberByPhone(txtSearchPhone.Text);
                if (member != null)
                {
                    dgvMembers.Rows.Clear();
                    dgvMembers.Rows.Add(
                        member.Id,
                        member.Name,
                        member.PhoneNumber,
                        member.Email,
                        member.RegistrationDate.ToString("yyyy-MM-dd"),
                        member.Points,
                        member.Level.ToString()
                    );
                }
                else
                {
                    MessageBox.Show("未找到该手机号的会员", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {                
                LoadMembers();
            }
        }
        


        private void btnResetSearch_Click(object sender, EventArgs e)
        {
            txtSearchPhone.Text = string.Empty;
            LoadMembers();
        }

        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.dgvMembers = new System.Windows.Forms.DataGridView();
            this.Id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PhoneNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Email = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RegistrationDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Points = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Level = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Discount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.txtId = new System.Windows.Forms.TextBox();
            this.txtName = new System.Windows.Forms.TextBox();
            this.txtPhone = new System.Windows.Forms.TextBox();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.txtPoints = new System.Windows.Forms.TextBox();
            this.txtDiscount = new System.Windows.Forms.TextBox();
            this.cmbLevel = new System.Windows.Forms.ComboBox();
            this.dtpRegistrationDate = new System.Windows.Forms.DateTimePicker();
            this.btnAddMember = new System.Windows.Forms.Button();
            this.btnUpdateMember = new System.Windows.Forms.Button();
            this.btnDeleteMember = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.txtSearchPhone = new System.Windows.Forms.TextBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnResetSearch = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMembers)).BeginInit();
            this.SuspendLayout();
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(385, 374);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(53, 12);
            this.label9.TabIndex = 23;
            this.label9.Text = "折扣率：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 14F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "会员管理";
            // 
            // dgvMembers
            // 
            this.dgvMembers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMembers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Id,
            this.ColumnName,
            this.PhoneNumber,
            this.Email,
            this.RegistrationDate,
            this.Points,
            this.Level,
            this.Discount});
            this.dgvMembers.Location = new System.Drawing.Point(12, 45);
            this.dgvMembers.Name = "dgvMembers";
            this.dgvMembers.RowTemplate.Height = 23;
            this.dgvMembers.Size = new System.Drawing.Size(776, 250);
            this.dgvMembers.TabIndex = 1;
            this.dgvMembers.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvMembers_CellClick);
            // 
            // Id
            // 
            this.Id.HeaderText = "会员ID";
            this.Id.Name = "Id";
            this.Id.Width = 150;
            // 
            // ColumnName
            // 
            this.ColumnName.HeaderText = "姓名";
            this.ColumnName.Name = "Name";
            this.ColumnName.Width = 100;
            // 
            // PhoneNumber
            // 
            this.PhoneNumber.HeaderText = "手机号码";
            this.PhoneNumber.Name = "PhoneNumber";
            this.PhoneNumber.Width = 120;
            // 
            // Email
            // 
            this.Email.HeaderText = "电子邮箱";
            this.Email.Name = "Email";
            this.Email.Width = 150;
            // 
            // RegistrationDate
            // 
            this.RegistrationDate.HeaderText = "注册日期";
            this.RegistrationDate.Name = "RegistrationDate";
            this.RegistrationDate.Width = 100;
            // 
            // Points
            // 
            this.Points.HeaderText = "积分";
            this.Points.Name = "Points";
            this.Points.Width = 60;
            // 
            // Level
            // 
            this.Level.HeaderText = "会员等级";
            this.Level.Name = "Level";
            this.Level.Width = 80;
            // 
            // Discount
            // 
            this.Discount.HeaderText = "折扣";
            this.Discount.Name = "Discount";
            this.Discount.Width = 60;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 314);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "会员ID：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 344);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 3;
            this.label3.Text = "姓  名：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 374);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "手机号：";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 404);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 5;
            this.label5.Text = "邮箱：";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(385, 314);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 6;
            this.label6.Text = "注册日期：";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(385, 344);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 12);
            this.label7.TabIndex = 7;
            this.label7.Text = "积分：";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(385, 374);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(53, 12);
            this.label9.TabIndex = 8;
            this.label9.Text = "折扣率：";
            // 
            // txtId
            // 
            this.txtId.Location = new System.Drawing.Point(71, 311);
            this.txtId.Name = "txtId";
            this.txtId.ReadOnly = true;
            this.txtId.Size = new System.Drawing.Size(290, 21);
            this.txtId.TabIndex = 8;
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(71, 341);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(290, 21);
            this.txtName.TabIndex = 9;
            // 
            // txtPhone
            // 
            this.txtPhone.Location = new System.Drawing.Point(71, 371);
            this.txtPhone.Name = "txtPhone";
            this.txtPhone.Size = new System.Drawing.Size(290, 21);
            this.txtPhone.TabIndex = 10;
            // 
            // txtEmail
            // 
            this.txtEmail.Location = new System.Drawing.Point(71, 399);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(290, 21);
            this.txtEmail.TabIndex = 11;
            // 
            // txtPoints
            // 
            this.txtPoints.Location = new System.Drawing.Point(444, 341);
            this.txtPoints.Name = "txtPoints";
            this.txtPoints.Size = new System.Drawing.Size(120, 21);
            this.txtPoints.TabIndex = 12;
            // 
            // txtDiscount
            // 
            this.txtDiscount.Location = new System.Drawing.Point(444, 371);
            this.txtDiscount.Name = "txtDiscount";
            this.txtDiscount.Size = new System.Drawing.Size(120, 21);
            this.txtDiscount.TabIndex = 24;
            // 
            // cmbLevel
            // 
            this.cmbLevel.FormattingEnabled = true;
            this.cmbLevel.Location = new System.Drawing.Point(444, 399);
            this.cmbLevel.Name = "cmbLevel";
            this.cmbLevel.Size = new System.Drawing.Size(120, 20);
            this.cmbLevel.TabIndex = 13;
            this.cmbLevel.SelectedIndexChanged += new System.EventHandler(this.cmbLevel_SelectedIndexChanged);
            // 
            // dtpRegistrationDate
            // 
            this.dtpRegistrationDate.Location = new System.Drawing.Point(444, 311);
            this.dtpRegistrationDate.Name = "dtpRegistrationDate";
            this.dtpRegistrationDate.Size = new System.Drawing.Size(180, 21);
            this.dtpRegistrationDate.TabIndex = 14;
            // 
            // btnAddMember
            // 
            this.btnAddMember.Location = new System.Drawing.Point(12, 442);
            this.btnAddMember.Name = "btnAddMember";
            this.btnAddMember.Size = new System.Drawing.Size(90, 30);
            this.btnAddMember.TabIndex = 15;
            this.btnAddMember.Text = "添加会员";
            this.btnAddMember.UseVisualStyleBackColor = true;
            this.btnAddMember.Click += new System.EventHandler(this.btnAddMember_Click);
            // 
            // btnUpdateMember
            // 
            this.btnUpdateMember.Location = new System.Drawing.Point(118, 442);
            this.btnUpdateMember.Name = "btnUpdateMember";
            this.btnUpdateMember.Size = new System.Drawing.Size(90, 30);
            this.btnUpdateMember.TabIndex = 16;
            this.btnUpdateMember.Text = "更新会员";
            this.btnUpdateMember.UseVisualStyleBackColor = true;
            this.btnUpdateMember.Click += new System.EventHandler(this.btnUpdateMember_Click);
            // 
            // btnDeleteMember
            // 
            this.btnDeleteMember.Location = new System.Drawing.Point(224, 442);
            this.btnDeleteMember.Name = "btnDeleteMember";
            this.btnDeleteMember.Size = new System.Drawing.Size(90, 30);
            this.btnDeleteMember.TabIndex = 17;
            this.btnDeleteMember.Text = "删除会员";
            this.btnDeleteMember.UseVisualStyleBackColor = true;
            this.btnDeleteMember.Click += new System.EventHandler(this.btnDeleteMember_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(330, 442);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(90, 30);
            this.btnClear.TabIndex = 18;
            this.btnClear.Text = "清空输入";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(441, 447);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(113, 12);
            this.label8.TabIndex = 19;
            this.label8.Text = "按手机号搜索：";
            // 
            // txtSearchPhone
            // 
            this.txtSearchPhone.Location = new System.Drawing.Point(560, 444);
            this.txtSearchPhone.Name = "txtSearchPhone";
            this.txtSearchPhone.Size = new System.Drawing.Size(120, 21);
            this.txtSearchPhone.TabIndex = 20;
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(686, 442);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(50, 23);
            this.btnSearch.TabIndex = 21;
            this.btnSearch.Text = "搜索";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // btnResetSearch
            // 
            this.btnResetSearch.Location = new System.Drawing.Point(742, 442);
            this.btnResetSearch.Name = "btnResetSearch";
            this.btnResetSearch.Size = new System.Drawing.Size(46, 23);
            this.btnResetSearch.TabIndex = 22;
            this.btnResetSearch.Text = "重置";
            this.btnResetSearch.UseVisualStyleBackColor = true;
            this.btnResetSearch.Click += new System.EventHandler(this.btnResetSearch_Click);
            // 
            // MemberManagementForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 500);
            this.Controls.Add(this.btnResetSearch);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.txtSearchPhone);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnDeleteMember);
            this.Controls.Add(this.btnUpdateMember);
            this.Controls.Add(this.btnAddMember);
            this.Controls.Add(this.dtpRegistrationDate);
            this.Controls.Add(this.cmbLevel);
            this.Controls.Add(this.txtPoints);
            this.Controls.Add(this.txtEmail);
            this.Controls.Add(this.txtPhone);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.txtId);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.dgvMembers);
            this.Controls.Add(this.label1);
            this.Name = "MemberManagementForm";
            this.Text = "会员管理";
            ((System.ComponentModel.ISupportInitialize)(this.dgvMembers)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dgvMembers;
        private System.Windows.Forms.DataGridViewTextBoxColumn Id;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn PhoneNumber;
        private System.Windows.Forms.DataGridViewTextBoxColumn Email;
        private System.Windows.Forms.DataGridViewTextBoxColumn RegistrationDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn Points;
        private System.Windows.Forms.DataGridViewTextBoxColumn Level;
        private System.Windows.Forms.DataGridViewTextBoxColumn Discount;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtDiscount;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtId;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.TextBox txtPhone;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.TextBox txtPoints;
        private System.Windows.Forms.ComboBox cmbLevel;
        private System.Windows.Forms.DateTimePicker dtpRegistrationDate;
        private System.Windows.Forms.Button btnAddMember;
        private System.Windows.Forms.Button btnUpdateMember;
        private System.Windows.Forms.Button btnDeleteMember;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtSearchPhone;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnResetSearch;
    }
}