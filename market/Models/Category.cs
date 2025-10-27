using System;

namespace market.Models
{
    /// <summary>
    /// 商品分类信息模型
    /// </summary>
    public class Category
    {
        /// <summary>
        /// 分类ID（主键）
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// 分类名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 分类描述
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 父分类ID（用于层级结构，为空表示顶级分类）
        /// </summary>
        public string ParentId { get; set; }
        
        /// <summary>
        /// 分类层级
        /// </summary>
        public int Level { get; set; }
        
        /// <summary>
        /// 排序序号
        /// </summary>
        public int SortOrder { get; set; }
        
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; }
        
        /// <summary>
        /// 该分类下的商品数量
        /// </summary>
        public int ProductCount { get; set; }
        
        /// <summary>
        /// 子分类列表
        /// </summary>
        public List<Category> Children { get; set; } = new List<Category>();
        
        /// <summary>
        /// 父分类名称（显示用）
        /// </summary>
        public string ParentName { get; set; }
        
        /// <summary>
        /// 是否叶子节点（没有子分类）
        /// </summary>
        public bool IsLeaf { get; set; }
        
        /// <summary>
        /// 分类图标或图片路径
        /// </summary>
        public string IconPath { get; set; }
        
        /// <summary>
        /// 分类颜色（用于UI显示）
        /// </summary>
        public string Color { get; set; }
    }
    
    /// <summary>
    /// 商品分类列表分页结果
    /// </summary>
    public class CategoryListResult
    {
        /// <summary>
        /// 分类列表
        /// </summary>
        public List<Category> Categories { get; set; }
        
        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalCount { get; set; }
        
        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages { get; set; }
        
        /// <summary>
        /// 当前页码
        /// </summary>
        public int CurrentPage { get; set; }
        
        /// <summary>
        /// 每页记录数
        /// </summary>
        public int PageSize { get; set; }
    }
    
    /// <summary>
    /// 分类树形结构节点
    /// </summary>
    public class CategoryTreeNode
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public bool IsActive { get; set; }
        public int ProductCount { get; set; }
        public List<CategoryTreeNode> Children { get; set; } = new List<CategoryTreeNode>();
    }
    
    /// <summary>
    /// 分类统计信息
    /// </summary>
    public class CategoryStatistics
    {
        /// <summary>
        /// 分类总数
        /// </summary>
        public int TotalCategories { get; set; }
        
        /// <summary>
        /// 活跃分类数
        /// </summary>
        public int ActiveCategories { get; set; }
        
        /// <summary>
        /// 包含商品的分类数
        /// </summary>
        public int CategoriesWithProducts { get; set; }
        
        /// <summary>
        /// 各分类的商品数量分布
        /// </summary>
        public Dictionary<string, int> ProductDistribution { get; set; } = new Dictionary<string, int>();
    }
}