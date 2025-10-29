using System.Windows.Forms;

namespace market.Forms
{
    partial class SalesStatisticsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelControl = new System.Windows.Forms.Panel();
            this.btnExportExcel = new System.Windows.Forms.Button();
            this.btnExportPDF = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.dtpEndDate = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.dtpStartDate = new System.Windows.Forms.DateTimePicker();
            this.panelSummary = new System.Windows.Forms.Panel();
            this.lblProductCount = new System.Windows.Forms.Label();
            this.lblOrderCount = new System.Windows.Forms.Label();
            this.lblTotalSales = new System.Windows.Forms.Label();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageTrend = new System.Windows.Forms.TabPage();
            this.chartSalesTrend = new System.Windows.Forms.DataGridView();
            this.tabPageTopProducts = new System.Windows.Forms.TabPage();
            this.chartTopProducts = new System.Windows.Forms.DataGridView();
            this.tabPageCategory = new System.Windows.Forms.TabPage();
            this.chartCategoryDistribution = new System.Windows.Forms.DataGridView();
            this.tabPageSlowMoving = new System.Windows.Forms.TabPage();
            this.chartSlowMoving = new System.Windows.Forms.DataGridView();
            this.panelControl.SuspendLayout();
            this.panelSummary.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPageTrend.SuspendLayout();
            this.tabPageTopProducts.SuspendLayout();
            this.tabPageCategory.SuspendLayout();
            this.tabPageSlowMoving.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelControl
            // 
            this.panelControl.BackColor = System.Drawing.SystemColors.Control;
            this.panelControl.Controls.Add(this.btnExportExcel);
            this.panelControl.Controls.Add(this.btnExportPDF);
            this.panelControl.Controls.Add(this.btnRefresh);
            this.panelControl.Controls.Add(this.label2);
            this.panelControl.Controls.Add(this.dtpEndDate);
            this.panelControl.Controls.Add(this.label1);
            this.panelControl.Controls.Add(this.dtpStartDate);
            this.panelControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControl.Location = new System.Drawing.Point(0, 0);
            this.panelControl.Name = "panelControl";
            this.panelControl.Size = new System.Drawing.Size(1057, 60);
            this.panelControl.TabIndex = 0;
            // 
            // btnExportExcel
            // 
            this.btnExportExcel.Location = new System.Drawing.Point(448, 20);
            this.btnExportExcel.Name = "btnExportExcel";
            this.btnExportExcel.Size = new System.Drawing.Size(90, 30);
            this.btnExportExcel.TabIndex = 6;
            this.btnExportExcel.Text = "导出Excel";
            this.btnExportExcel.UseVisualStyleBackColor = true;
            this.btnExportExcel.Click += new System.EventHandler(this.btnExportExcel_Click);
            // 
            // btnExportPDF
            // 
            this.btnExportPDF.Location = new System.Drawing.Point(352, 20);
            this.btnExportPDF.Name = "btnExportPDF";
            this.btnExportPDF.Size = new System.Drawing.Size(90, 30);
            this.btnExportPDF.TabIndex = 5;
            this.btnExportPDF.Text = "导出PDF";
            this.btnExportPDF.UseVisualStyleBackColor = true;
            this.btnExportPDF.Click += new System.EventHandler(this.btnExportPDF_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(256, 20);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(90, 30);
            this.btnRefresh.TabIndex = 4;
            this.btnRefresh.Text = "刷新数据";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(132, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "至";
            // 
            // dtpEndDate
            // 
            this.dtpEndDate.Location = new System.Drawing.Point(155, 20);
            this.dtpEndDate.Name = "dtpEndDate";
            this.dtpEndDate.Size = new System.Drawing.Size(95, 25);
            this.dtpEndDate.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "日期：";
            // 
            // dtpStartDate
            // 
            this.dtpStartDate.Location = new System.Drawing.Point(62, 20);
            this.dtpStartDate.Name = "dtpStartDate";
            this.dtpStartDate.Size = new System.Drawing.Size(95, 25);
            this.dtpStartDate.TabIndex = 0;
            // 
            // panelSummary
            // 
            this.panelSummary.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panelSummary.Controls.Add(this.lblProductCount);
            this.panelSummary.Controls.Add(this.lblOrderCount);
            this.panelSummary.Controls.Add(this.lblTotalSales);
            this.panelSummary.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSummary.Location = new System.Drawing.Point(0, 60);
            this.panelSummary.Name = "panelSummary";
            this.panelSummary.Size = new System.Drawing.Size(1057, 50);
            this.panelSummary.TabIndex = 1;
            // 
            // lblProductCount
            // 
            this.lblProductCount.AutoSize = true;
            this.lblProductCount.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.lblProductCount.Location = new System.Drawing.Point(340, 15);
            this.lblProductCount.Name = "lblProductCount";
            this.lblProductCount.Size = new System.Drawing.Size(124, 20);
            this.lblProductCount.TabIndex = 2;
            this.lblProductCount.Text = "销售商品种类: 0";
            // 
            // lblOrderCount
            // 
            this.lblOrderCount.AutoSize = true;
            this.lblOrderCount.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.lblOrderCount.Location = new System.Drawing.Point(170, 15);
            this.lblOrderCount.Name = "lblOrderCount";
            this.lblOrderCount.Size = new System.Drawing.Size(94, 20);
            this.lblOrderCount.TabIndex = 1;
            this.lblOrderCount.Text = "订单数量: 0";
            // 
            // lblTotalSales
            // 
            this.lblTotalSales.AutoSize = true;
            this.lblTotalSales.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.lblTotalSales.Location = new System.Drawing.Point(12, 15);
            this.lblTotalSales.Name = "lblTotalSales";
            this.lblTotalSales.Size = new System.Drawing.Size(114, 20);
            this.lblTotalSales.TabIndex = 0;
            this.lblTotalSales.Text = "总销售额: ¥0";
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageTrend);
            this.tabControl.Controls.Add(this.tabPageTopProducts);
            this.tabControl.Controls.Add(this.tabPageCategory);
            this.tabControl.Controls.Add(this.tabPageSlowMoving);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 110);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1057, 529);
            this.tabControl.TabIndex = 2;
            // 
            // tabPageTrend
            // 
            this.tabPageTrend.Controls.Add(this.chartSalesTrend);
            this.tabPageTrend.Location = new System.Drawing.Point(4, 25);
            this.tabPageTrend.Name = "tabPageTrend";
            this.tabPageTrend.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTrend.Size = new System.Drawing.Size(1049, 500);
            this.tabPageTrend.TabIndex = 0;
            this.tabPageTrend.Text = "销售趋势";
            this.tabPageTrend.UseVisualStyleBackColor = true;
            // 
            // chartSalesTrend
            // 
            this.chartSalesTrend.AllowUserToAddRows = false;
            this.chartSalesTrend.AllowUserToDeleteRows = false;
            this.chartSalesTrend.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.chartSalesTrend.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.chartSalesTrend.Location = new System.Drawing.Point(6, 6);
            this.chartSalesTrend.Name = "chartSalesTrend";
            this.chartSalesTrend.ReadOnly = true;
            this.chartSalesTrend.RowTemplate.Height = 25;
            this.chartSalesTrend.Size = new System.Drawing.Size(1037, 488);
            this.chartSalesTrend.TabIndex = 0;
            // 
            // tabPageTopProducts
            // 
            this.tabPageTopProducts.Controls.Add(this.chartTopProducts);
            this.tabPageTopProducts.Location = new System.Drawing.Point(4, 25);
            this.tabPageTopProducts.Name = "tabPageTopProducts";
            this.tabPageTopProducts.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTopProducts.Size = new System.Drawing.Size(1049, 500);
            this.tabPageTopProducts.TabIndex = 1;
            this.tabPageTopProducts.Text = "热门商品";
            this.tabPageTopProducts.UseVisualStyleBackColor = true;
            // 
            // chartTopProducts
            // 
            this.chartTopProducts.AllowUserToAddRows = false;
            this.chartTopProducts.AllowUserToDeleteRows = false;
            this.chartTopProducts.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.chartTopProducts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.chartTopProducts.Location = new System.Drawing.Point(6, 6);
            this.chartTopProducts.Name = "chartTopProducts";
            this.chartTopProducts.ReadOnly = true;
            this.chartTopProducts.RowTemplate.Height = 25;
            this.chartTopProducts.Size = new System.Drawing.Size(1037, 488);
            this.chartTopProducts.TabIndex = 0;
            // 
            // tabPageCategory
            // 
            this.tabPageCategory.Controls.Add(this.chartCategoryDistribution);
            this.tabPageCategory.Location = new System.Drawing.Point(4, 25);
            this.tabPageCategory.Name = "tabPageCategory";
            this.tabPageCategory.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageCategory.Size = new System.Drawing.Size(1049, 500);
            this.tabPageCategory.TabIndex = 2;
            this.tabPageCategory.Text = "类别分布";
            this.tabPageCategory.UseVisualStyleBackColor = true;
            // 
            // chartCategoryDistribution
            // 
            this.chartCategoryDistribution.AllowUserToAddRows = false;
            this.chartCategoryDistribution.AllowUserToDeleteRows = false;
            this.chartCategoryDistribution.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.chartCategoryDistribution.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.chartCategoryDistribution.Location = new System.Drawing.Point(6, 6);
            this.chartCategoryDistribution.Name = "chartCategoryDistribution";
            this.chartCategoryDistribution.ReadOnly = true;
            this.chartCategoryDistribution.RowTemplate.Height = 25;
            this.chartCategoryDistribution.Size = new System.Drawing.Size(1037, 488);
            this.chartCategoryDistribution.TabIndex = 0;
            // 
            // tabPageSlowMoving
            // 
            this.tabPageSlowMoving.Controls.Add(this.chartSlowMoving);
            this.tabPageSlowMoving.Location = new System.Drawing.Point(4, 25);
            this.tabPageSlowMoving.Name = "tabPageSlowMoving";
            this.tabPageSlowMoving.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSlowMoving.Size = new System.Drawing.Size(1049, 500);
            this.tabPageSlowMoving.TabIndex = 3;
            this.tabPageSlowMoving.Text = "滞销商品";
            this.tabPageSlowMoving.UseVisualStyleBackColor = true;
            // 
            // chartSlowMoving
            // 
            this.chartSlowMoving.AllowUserToAddRows = false;
            this.chartSlowMoving.AllowUserToDeleteRows = false;
            this.chartSlowMoving.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.chartSlowMoving.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.chartSlowMoving.Location = new System.Drawing.Point(6, 6);
            this.chartSlowMoving.Name = "chartSlowMoving";
            this.chartSlowMoving.ReadOnly = true;
            this.chartSlowMoving.RowTemplate.Height = 25;
            this.chartSlowMoving.Size = new System.Drawing.Size(1037, 488);
            this.chartSlowMoving.TabIndex = 0;
            // 
            // SalesStatisticsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1057, 639);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.panelSummary);
            this.Controls.Add(this.panelControl);
            this.Name = "SalesStatisticsForm";
            this.Text = "销售统计报表";
            this.panelControl.ResumeLayout(false);
            this.panelControl.PerformLayout();
            this.panelSummary.ResumeLayout(false);
            this.panelSummary.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.tabPageTrend.ResumeLayout(false);
            this.tabPageTopProducts.ResumeLayout(false);
            this.tabPageCategory.ResumeLayout(false);
            this.tabPageSlowMoving.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelControl;
        private System.Windows.Forms.Button btnExportExcel;
        private System.Windows.Forms.Button btnExportPDF;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dtpEndDate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dtpStartDate;
        private System.Windows.Forms.Panel panelSummary;
        private System.Windows.Forms.Label lblProductCount;
        private System.Windows.Forms.Label lblOrderCount;
        private System.Windows.Forms.Label lblTotalSales;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageTrend;
        private System.Windows.Forms.DataGridView chartSalesTrend;
        private System.Windows.Forms.TabPage tabPageTopProducts;
        private System.Windows.Forms.DataGridView chartTopProducts;
        private System.Windows.Forms.TabPage tabPageCategory;
        private System.Windows.Forms.DataGridView chartCategoryDistribution;
        private System.Windows.Forms.TabPage tabPageSlowMoving;
        private System.Windows.Forms.DataGridView chartSlowMoving;
    }
}