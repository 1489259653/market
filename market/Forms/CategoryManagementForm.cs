using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using market.Models;
using market.Services;

namespace market.Forms
{
    public partial class CategoryManagementForm : Form
    {
        private readonly CategoryService _categoryService;
        private List<Category> _categories;
        private ToolStrip _toolStrip;
        private DataGridView _dataGridView;
        private StatusStrip _statusStrip;
        private TreeView _treeView;
        private SplitContainer _splitContainer;

        public CategoryManagementForm(CategoryService categoryService)
        {
            _categoryService = categoryService;
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "商品分类管理";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // 创建工具栏
            CreateToolStrip();

            // 创建分割容器
            _splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 300
            };

            // 创建树形视图（左侧）
            CreateTreeView();

            // 创建数据网格视图（右侧）
            CreateDataGridView();

            // 创建状态栏
            CreateStatusStrip();

            // 添加控件到窗体
            _splitContainer.Panel1.Controls.Add(_treeView);
            _splitContainer.Panel2.Controls.Add(_dataGridView);
            
            // 设置控件的Dock属性
            _toolStrip.Dock = DockStyle.Top;
            _statusStrip.Dock = DockStyle.Bottom;
            _splitContainer.Dock = DockStyle.Fill;
            
            // 添加控件时按正确顺序（从下到上）
            this.Controls.Add(_statusStrip);
            this.Controls.Add(_splitContainer);
            this.Controls.Add(_toolStrip);
        }

        private void CreateToolStrip()
        {
            _toolStrip = new ToolStrip
            {
                Dock = DockStyle.Top,
                Location = new Point(0, 0)
            };

            var btnAdd = new ToolStripButton("添加分类", null, (s, e) => AddCategory())
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
            };

            var btnEdit = new ToolStripButton("编辑分类", null, (s, e) => EditCategory())
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
            };

            var btnDelete = new ToolStripButton("删除分类", null, (s, e) => DeleteCategory())
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
            };

            var btnRefresh = new ToolStripButton("刷新", null, (s, e) => LoadData())
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
            };

            _toolStrip.Items.AddRange(new ToolStripItem[] { btnAdd, btnEdit, btnDelete, new ToolStripSeparator(), btnRefresh });
        }

        private void CreateTreeView()
        {
            _treeView = new TreeView
            {
                Dock = DockStyle.Fill,
                ShowRootLines = true,
                ShowLines = true,
                ShowPlusMinus = true,
                HideSelection = false,
                Margin = new Padding(0, 5, 0, 0) // 添加上边距避免被遮挡
            };

            _treeView.AfterSelect += (s, e) => LoadCategoryDetails(e.Node?.Tag as string);
        }

        private void CreateDataGridView()
        {
            _dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            _dataGridView.CellDoubleClick += (s, e) => EditCategory();
        }

        private void CreateStatusStrip()
        {
            _statusStrip = new StatusStrip
            {
                Dock = DockStyle.Bottom
            };

            var lblStatus = new ToolStripStatusLabel("就绪");
            var lblCount = new ToolStripStatusLabel("分类数: 0");
            
            _statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus, lblCount });
        }

        private void LoadData()
        {
            try
            {
                _categories = _categoryService.GetAllCategories();
                
                // 加载树形视图
                LoadTreeView();
                
                // 加载数据网格
                LoadDataGridView();
                
                UpdateStatus("数据加载成功", $"分类数: {_categories.Count}");
            }
            catch (Exception ex)
            {
                UpdateStatus($"加载数据失败: {ex.Message}", "分类数: 0");
                MessageBox.Show($"加载数据失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTreeView()
        {
            _treeView.Nodes.Clear();
            
            var topLevelCategories = _categoryService.GetTopLevelCategories();
            foreach (var category in topLevelCategories)
            {
                var node = new TreeNode(category.Name)
                {
                    Tag = category.Id,
                    ForeColor = category.IsActive ? Color.Black : Color.Gray
                };
                
                // 添加子节点
                AddChildNodes(node, category.Id);
                
                _treeView.Nodes.Add(node);
            }
            
            // 展开第一个节点
            if (_treeView.Nodes.Count > 0)
            {
                _treeView.Nodes[0].Expand();
            }
        }

        private void AddChildNodes(TreeNode parentNode, string parentId)
        {
            var childCategories = _categoryService.GetChildCategories(parentId);
            foreach (var child in childCategories)
            {
                var childNode = new TreeNode(child.Name)
                {
                    Tag = child.Id,
                    ForeColor = child.IsActive ? Color.Black : Color.Gray
                };
                
                // 递归添加子节点
                AddChildNodes(childNode, child.Id);
                
                parentNode.Nodes.Add(childNode);
            }
        }

        private void LoadDataGridView()
        {
            _dataGridView.Columns.Clear();
            
            // 添加列
            _dataGridView.Columns.Add("Name", "分类名称");
            _dataGridView.Columns.Add("Description", "描述");
            _dataGridView.Columns.Add("Level", "层级");
            _dataGridView.Columns.Add("ParentName", "父分类");
            _dataGridView.Columns.Add("ProductCount", "商品数量");
            _dataGridView.Columns.Add("IsActive", "状态");
            
            // 清空行
            _dataGridView.Rows.Clear();
            
            foreach (var category in _categories)
            {
                _dataGridView.Rows.Add(
                    category.Name,
                    category.Description ?? "",
                    category.Level,
                    category.ParentName ?? "顶级分类",
                    category.ProductCount,
                    category.IsActive ? "启用" : "禁用"
                );
            }
        }

        private void LoadCategoryDetails(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId)) return;
            
            var category = _categoryService.GetCategoryById(categoryId);
            if (category != null)
            {
                UpdateStatus($"已选择: {category.Name}", $"商品数量: {category.ProductCount}");
            }
        }

        private void AddCategory()
        {
            var form = new CategoryEditForm(_categoryService, null);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void EditCategory()
        {
            var selectedId = GetSelectedCategoryId();
            if (string.IsNullOrEmpty(selectedId))
            {
                MessageBox.Show("请先选择一个分类", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            var form = new CategoryEditForm(_categoryService, selectedId);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void DeleteCategory()
        {
            var selectedId = GetSelectedCategoryId();
            if (string.IsNullOrEmpty(selectedId))
            {
                MessageBox.Show("请先选择一个分类", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            var category = _categoryService.GetCategoryById(selectedId);
            if (category == null) return;
            
            if (MessageBox.Show($"确定要删除分类 '{category.Name}' 吗？\n此操作将禁用该分类，无法恢复。", 
                "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    if (_categoryService.DeleteCategory(selectedId))
                    {
                        MessageBox.Show("删除成功", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData();
                    }
                    else
                    {
                        MessageBox.Show("删除失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"删除失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string GetSelectedCategoryId()
        {
            // 从树形视图获取
            if (_treeView.SelectedNode != null && _treeView.SelectedNode.Tag != null)
            {
                return _treeView.SelectedNode.Tag.ToString();
            }
            
            // 从数据网格获取
            if (_dataGridView.SelectedRows.Count > 0)
            {
                var selectedRow = _dataGridView.SelectedRows[0];
                var categoryName = selectedRow.Cells["Name"].Value.ToString();
                
                var category = _categories.Find(c => c.Name == categoryName);
                return category?.Id;
            }
            
            return null;
        }

        private void UpdateStatus(string status, string count = null)
        {
            if (_statusStrip.Items.Count >= 2)
            {
                _statusStrip.Items[0].Text = status;
                if (count != null)
                {
                    _statusStrip.Items[1].Text = count;
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            
            // 释放资源
            _dataGridView?.Dispose();
            _treeView?.Dispose();
            _splitContainer?.Dispose();
            _toolStrip?.Dispose();
            _statusStrip?.Dispose();
        }
    }
}