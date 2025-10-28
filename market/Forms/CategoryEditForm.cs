using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using market.Models;
using market.Services;

namespace market.Forms
{
    public partial class CategoryEditForm : Form
    {
        private readonly CategoryService _categoryService;
        private readonly string _categoryId;
        private Category _category;
        
        private TextBox _txtName;
        private TextBox _txtDescription;
        private ComboBox _cmbParent;
        private NumericUpDown _numLevel;
        private NumericUpDown _numSortOrder;
        private CheckBox _chkIsActive;
        private TextBox _txtColor;
        private Button _btnSelectColor;
        private Button _btnSave;
        private Button _btnCancel;
        private ColorDialog _colorDialog;

        public CategoryEditForm(CategoryService categoryService, string categoryId = null)
        {
            _categoryService = categoryService;
            _categoryId = categoryId;
            _category = categoryId != null ? categoryService.GetCategoryById(categoryId) : new Category();
            
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = _categoryId == null ? "添加分类" : "编辑分类";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // 创建控件
            CreateControls();
            
            // 设置布局
            SetLayout();
            
            // 绑定事件
            BindEvents();
        }

        private void CreateControls()
        {
            // 分类名称
            var lblName = new Label { Text = "分类名称:", AutoSize = true, Location = new Point(20, 20) };
            _txtName = new TextBox { Location = new Point(100, 17), Size = new Size(300, 23), MaxLength = 255 };

            // 分类描述
            var lblDescription = new Label { Text = "描述:", AutoSize = true, Location = new Point(20, 60) };
            _txtDescription = new TextBox { Location = new Point(100, 57), Size = new Size(300, 60), Multiline = true, ScrollBars = ScrollBars.Vertical, MaxLength = 1000 };

            // 父分类
            var lblParent = new Label { Text = "父分类:", AutoSize = true, Location = new Point(20, 140) };
            _cmbParent = new ComboBox { Location = new Point(100, 137), Size = new Size(300, 23), DropDownStyle = ComboBoxStyle.DropDownList };

            // 层级
            var lblLevel = new Label { Text = "层级:", AutoSize = true, Location = new Point(20, 180) };
            _numLevel = new NumericUpDown { Location = new Point(100, 177), Size = new Size(100, 23), Minimum = 1, Maximum = 5, Value = 1 };

            // 排序序号
            var lblSortOrder = new Label { Text = "排序:", AutoSize = true, Location = new Point(220, 180) };
            _numSortOrder = new NumericUpDown { Location = new Point(270, 177), Size = new Size(100, 23), Minimum = 0, Maximum = 999, Value = 0 };

            // 是否启用
            _chkIsActive = new CheckBox { Text = "启用分类", Location = new Point(100, 217), Size = new Size(100, 23), Checked = true };

            // 颜色选择
            var lblColor = new Label { Text = "颜色:", AutoSize = true, Location = new Point(20, 260) };
            _txtColor = new TextBox { Location = new Point(100, 257), Size = new Size(200, 23), ReadOnly = true };
            _btnSelectColor = new Button { Text = "选择", Location = new Point(310, 257), Size = new Size(60, 23) };
            
            // 按钮
            _btnSave = new Button { Text = "保存", Location = new Point(200, 310), Size = new Size(80, 30), DialogResult = DialogResult.OK };
            _btnCancel = new Button { Text = "取消", Location = new Point(300, 310), Size = new Size(80, 30), DialogResult = DialogResult.Cancel };

            // 颜色对话框
            _colorDialog = new ColorDialog { AllowFullOpen = true, AnyColor = true };

            // 添加控件到窗体
            this.Controls.AddRange(new Control[] {
                lblName, _txtName,
                lblDescription, _txtDescription,
                lblParent, _cmbParent,
                lblLevel, _numLevel,
                lblSortOrder, _numSortOrder,
                _chkIsActive,
                lblColor, _txtColor, _btnSelectColor,
                _btnSave, _btnCancel
            });
        }

        private void SetLayout()
        {
            // 设置Tab顺序
            _txtName.TabIndex = 0;
            _txtDescription.TabIndex = 1;
            _cmbParent.TabIndex = 2;
            _numLevel.TabIndex = 3;
            _numSortOrder.TabIndex = 4;
            _chkIsActive.TabIndex = 5;
            _btnSelectColor.TabIndex = 6;
            _btnSave.TabIndex = 7;
            _btnCancel.TabIndex = 8;
            
            // 设置接受按钮
            this.AcceptButton = _btnSave;
            this.CancelButton = _btnCancel;
        }

        private void BindEvents()
        {
            _btnSave.Click += (s, e) => SaveCategory();
            _btnCancel.Click += (s, e) => this.Close();
            _btnSelectColor.Click += (s, e) => SelectColor();
            _cmbParent.SelectedIndexChanged += (s, e) => UpdateLevelFromParent();
        }

        private void LoadData()
        {
            try
            {
                // 加载父分类选项
                LoadParentCategories();
                
                // 如果是编辑模式，加载分类数据
                if (_categoryId != null)
                {
                    _txtName.Text = _category.Name;
                    _txtDescription.Text = _category.Description ?? "";
                    _numLevel.Value = _category.Level;
                    _numSortOrder.Value = _category.SortOrder;
                    _chkIsActive.Checked = _category.IsActive;
                    _txtColor.Text = _category.Color ?? "#000000";
                    
                    // 设置父分类
                    if (!string.IsNullOrEmpty(_category.ParentId))
                    {
                        var parentCategory = _categoryService.GetCategoryById(_category.ParentId);
                        if (parentCategory != null)
                        {
                            var item = _cmbParent.Items.Cast<ComboBoxItem>().FirstOrDefault(x => x.Value != null && parentCategory.Id != null && x.Value.ToString() == parentCategory.Id.ToString());
                            if (item != null)
                            {
                                _cmbParent.SelectedItem = item;
                            }
                        }
                    }
                }
                else
                {
                    // 新建模式，设置默认值
                    _txtColor.Text = "#000000";
                    _numLevel.Value = 1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载数据失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadParentCategories()
        {
            _cmbParent.Items.Clear();
            
            // 添加"无父分类"选项
            _cmbParent.Items.Add(new ComboBoxItem("无父分类（顶级分类）", null));
            
            // 加载所有活跃分类（排除当前编辑的分类）
            var allCategories = _categoryService.GetAllCategories()
                .Where(c => c.IsActive && c.Id != _categoryId)
                .ToList();
            
            foreach (var category in allCategories)
            {
                _cmbParent.Items.Add(new ComboBoxItem($"{new string(' ', (category.Level - 1) * 2)}{category.Name}", category.Id));
            }
            
            _cmbParent.SelectedIndex = 0;
        }

        private void UpdateLevelFromParent()
        {
            var selectedItem = _cmbParent.SelectedItem as ComboBoxItem;
            if (selectedItem != null && selectedItem.Value != null)
            {
                var parentCategory = _categoryService.GetCategoryById(selectedItem.Value.ToString());
                if (parentCategory != null)
                {
                    _numLevel.Value = parentCategory.Level + 1;
                }
            }
            else
            {
                _numLevel.Value = 1;
            }
        }

        private void SelectColor()
        {
            if (!string.IsNullOrEmpty(_txtColor.Text))
            {
                try
                {
                    var color = ColorTranslator.FromHtml(_txtColor.Text);
                    _colorDialog.Color = color;
                }
                catch
                {
                    _colorDialog.Color = Color.Black;
                }
            }
            
            if (_colorDialog.ShowDialog() == DialogResult.OK)
            {
                _txtColor.Text = ColorTranslator.ToHtml(_colorDialog.Color);
            }
        }

        private void SaveCategory()
        {
            try
            {
                // 验证数据
                if (!ValidateData())
                {
                    return;
                }

                // 更新分类对象
                _category.Name = _txtName.Text.Trim();
                _category.Description = string.IsNullOrEmpty(_txtDescription.Text) ? null : _txtDescription.Text.Trim();
                _category.Level = (int)_numLevel.Value;
                _category.SortOrder = (int)_numSortOrder.Value;
                _category.IsActive = _chkIsActive.Checked;
                _category.Color = string.IsNullOrEmpty(_txtColor.Text) ? null : _txtColor.Text.Trim();
                
                // 设置父分类ID
                var selectedItem = _cmbParent.SelectedItem as ComboBoxItem;
                _category.ParentId = selectedItem?.Value?.ToString();

                bool success;
                if (_categoryId == null)
                {
                    // 新建分类
                    _category.Id = Guid.NewGuid().ToString();
                    success = _categoryService.AddCategory(_category);
                }
                else
                {
                    // 更新分类
                    success = _categoryService.UpdateCategory(_category);
                }

                if (success)
                {
                    MessageBox.Show("保存成功", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("保存失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateData()
        {
            // 验证分类名称
            if (string.IsNullOrEmpty(_txtName.Text.Trim()))
            {
                MessageBox.Show("请输入分类名称", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtName.Focus();
                return false;
            }

            // 检查分类名称是否已存在
            var name = _txtName.Text.Trim();
            if (_categoryService.CategoryNameExists(name, _categoryId))
            {
                MessageBox.Show("分类名称已存在，请使用其他名称", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtName.Focus();
                return false;
            }

            // 验证层级
            if (_numLevel.Value < 1 || _numLevel.Value > 5)
            {
                MessageBox.Show("层级必须在1-5之间", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _numLevel.Focus();
                return false;
            }

            // 验证颜色格式
            if (!string.IsNullOrEmpty(_txtColor.Text))
            {
                try
                {
                    ColorTranslator.FromHtml(_txtColor.Text);
                }
                catch
                {
                    MessageBox.Show("颜色格式不正确，请使用十六进制格式（如#FF0000）", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtColor.Focus();
                    return false;
                }
            }

            return true;
        }

        // 辅助类用于ComboBox项
        private class ComboBoxItem
        {
            public string Text { get; set; }
            public object Value { get; set; }

            public ComboBoxItem(string text, object value)
            {
                Text = text;
                Value = value;
            }

            public override string ToString()
            {
                return Text;
            }
        }
    }
}