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
            panelControl = new Panel();
            btnExportExcel = new Button();
            btnExportPDF = new Button();
            btnRefresh = new Button();
            label2 = new Label();
            dtpEndDate = new DateTimePicker();
            label1 = new Label();
            dtpStartDate = new DateTimePicker();
            panelSummary = new Panel();
            lblProductCount = new Label();
            lblOrderCount = new Label();
            lblTotalSales = new Label();
            tabControl = new TabControl();
            tabPageTrend = new TabPage();
            chartSalesTrend = new DataGridView();
            tabPageTopProducts = new TabPage();
            chartTopProducts = new DataGridView();
            tabPageCategory = new TabPage();
            chartCategoryDistribution = new DataGridView();
            tabPageSlowMoving = new TabPage();
            chartSlowMoving = new DataGridView();
            panelControl.SuspendLayout();
            panelSummary.SuspendLayout();
            tabControl.SuspendLayout();
            tabPageTrend.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chartSalesTrend).BeginInit();
            tabPageTopProducts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chartTopProducts).BeginInit();
            tabPageCategory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chartCategoryDistribution).BeginInit();
            tabPageSlowMoving.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chartSlowMoving).BeginInit();
            SuspendLayout();
            // 
            // panelControl
            // 
            panelControl.BackColor = SystemColors.Control;
            panelControl.Controls.Add(btnExportExcel);
            panelControl.Controls.Add(btnExportPDF);
            panelControl.Controls.Add(btnRefresh);
            panelControl.Controls.Add(label2);
            panelControl.Controls.Add(dtpEndDate);
            panelControl.Controls.Add(label1);
            panelControl.Controls.Add(dtpStartDate);
            panelControl.Dock = DockStyle.Top;
            panelControl.Location = new Point(0, 0);
            panelControl.Margin = new Padding(3, 4, 3, 4);
            panelControl.Name = "panelControl";
            panelControl.Size = new Size(1189, 80);
            panelControl.TabIndex = 0;
            // 
            // btnExportExcel
            // 
            btnExportExcel.Location = new Point(538, 30);
            btnExportExcel.Margin = new Padding(3, 4, 3, 4);
            btnExportExcel.Name = "btnExportExcel";
            btnExportExcel.Size = new Size(101, 40);
            btnExportExcel.TabIndex = 6;
            btnExportExcel.Text = "导出Excel";
            btnExportExcel.UseVisualStyleBackColor = true;
            btnExportExcel.Click += btnExportExcel_Click;
            // 
            // btnExportPDF
            // 
            btnExportPDF.Location = new Point(430, 30);
            btnExportPDF.Margin = new Padding(3, 4, 3, 4);
            btnExportPDF.Name = "btnExportPDF";
            btnExportPDF.Size = new Size(101, 40);
            btnExportPDF.TabIndex = 5;
            btnExportPDF.Text = "导出PDF";
            btnExportPDF.UseVisualStyleBackColor = true;
            btnExportPDF.Click += btnExportPDF_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(322, 30);
            btnRefresh.Margin = new Padding(3, 4, 3, 4);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(101, 40);
            btnRefresh.TabIndex = 4;
            btnRefresh.Text = "刷新数据";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(180, 39);
            label2.Name = "label2";
            label2.Size = new Size(24, 20);
            label2.TabIndex = 3;
            label2.Text = "至";
            // 
            // dtpEndDate
            // 
            dtpEndDate.Location = new Point(209, 36);
            dtpEndDate.Margin = new Padding(3, 4, 3, 4);
            dtpEndDate.Name = "dtpEndDate";
            dtpEndDate.Size = new Size(106, 27);
            dtpEndDate.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(14, 40);
            label1.Name = "label1";
            label1.Size = new Size(54, 20);
            label1.TabIndex = 1;
            label1.Text = "日期：";
            // 
            // dtpStartDate
            // 
            dtpStartDate.Location = new Point(70, 36);
            dtpStartDate.Margin = new Padding(3, 4, 3, 4);
            dtpStartDate.Name = "dtpStartDate";
            dtpStartDate.Size = new Size(106, 27);
            dtpStartDate.TabIndex = 0;
            // 
            // panelSummary
            // 
            panelSummary.BackColor = Color.WhiteSmoke;
            panelSummary.Controls.Add(lblProductCount);
            panelSummary.Controls.Add(lblOrderCount);
            panelSummary.Controls.Add(lblTotalSales);
            panelSummary.Dock = DockStyle.Top;
            panelSummary.Location = new Point(0, 80);
            panelSummary.Margin = new Padding(3, 4, 3, 4);
            panelSummary.Name = "panelSummary";
            panelSummary.Size = new Size(1189, 67);
            panelSummary.TabIndex = 1;
            // 
            // lblProductCount
            // 
            lblProductCount.AutoSize = true;
            lblProductCount.Font = new Font("微软雅黑", 10F, FontStyle.Bold, GraphicsUnit.Point);
            lblProductCount.Location = new Point(382, 20);
            lblProductCount.Name = "lblProductCount";
            lblProductCount.Size = new Size(132, 24);
            lblProductCount.TabIndex = 2;
            lblProductCount.Text = "销售商品种类: 0";
            // 
            // lblOrderCount
            // 
            lblOrderCount.AutoSize = true;
            lblOrderCount.Font = new Font("微软雅黑", 10F, FontStyle.Bold, GraphicsUnit.Point);
            lblOrderCount.Location = new Point(191, 20);
            lblOrderCount.Name = "lblOrderCount";
            lblOrderCount.Size = new Size(98, 24);
            lblOrderCount.TabIndex = 1;
            lblOrderCount.Text = "订单数量: 0";
            // 
            // lblTotalSales
            // 
            lblTotalSales.AutoSize = true;
            lblTotalSales.Font = new Font("微软雅黑", 10F, FontStyle.Bold, GraphicsUnit.Point);
            lblTotalSales.Location = new Point(14, 20);
            lblTotalSales.Name = "lblTotalSales";
            lblTotalSales.Size = new Size(108, 24);
            lblTotalSales.TabIndex = 0;
            lblTotalSales.Text = "总销售额: ¥0";
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabPageTrend);
            tabControl.Controls.Add(tabPageTopProducts);
            tabControl.Controls.Add(tabPageCategory);
            tabControl.Controls.Add(tabPageSlowMoving);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 147);
            tabControl.Margin = new Padding(3, 4, 3, 4);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(1189, 705);
            tabControl.TabIndex = 2;
            // 
            // tabPageTrend
            // 
            tabPageTrend.Controls.Add(chartSalesTrend);
            tabPageTrend.Location = new Point(4, 29);
            tabPageTrend.Margin = new Padding(3, 4, 3, 4);
            tabPageTrend.Name = "tabPageTrend";
            tabPageTrend.Padding = new Padding(3, 4, 3, 4);
            tabPageTrend.Size = new Size(1181, 672);
            tabPageTrend.TabIndex = 0;
            tabPageTrend.Text = "销售趋势";
            tabPageTrend.UseVisualStyleBackColor = true;
            // 
            // chartSalesTrend
            // 
            chartSalesTrend.AllowUserToAddRows = false;
            chartSalesTrend.AllowUserToDeleteRows = false;
            chartSalesTrend.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            chartSalesTrend.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            chartSalesTrend.Dock = DockStyle.Fill;
            chartSalesTrend.Location = new Point(3, 4);
            chartSalesTrend.Margin = new Padding(3, 4, 3, 4);
            chartSalesTrend.Name = "chartSalesTrend";
            chartSalesTrend.ReadOnly = true;
            chartSalesTrend.RowHeadersWidth = 51;
            chartSalesTrend.RowTemplate.Height = 25;
            chartSalesTrend.Size = new Size(1175, 664);
            chartSalesTrend.TabIndex = 0;
            // 
            // tabPageTopProducts
            // 
            tabPageTopProducts.Controls.Add(chartTopProducts);
            tabPageTopProducts.Location = new Point(4, 29);
            tabPageTopProducts.Margin = new Padding(3, 4, 3, 4);
            tabPageTopProducts.Name = "tabPageTopProducts";
            tabPageTopProducts.Padding = new Padding(3, 4, 3, 4);
            tabPageTopProducts.Size = new Size(1181, 672);
            tabPageTopProducts.TabIndex = 1;
            tabPageTopProducts.Text = "热门商品";
            tabPageTopProducts.UseVisualStyleBackColor = true;
            // 
            // chartTopProducts
            // 
            chartTopProducts.AllowUserToAddRows = false;
            chartTopProducts.AllowUserToDeleteRows = false;
            chartTopProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            chartTopProducts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            chartTopProducts.Dock = DockStyle.Fill;
            chartTopProducts.Location = new Point(3, 4);
            chartTopProducts.Margin = new Padding(3, 4, 3, 4);
            chartTopProducts.Name = "chartTopProducts";
            chartTopProducts.ReadOnly = true;
            chartTopProducts.RowHeadersWidth = 51;
            chartTopProducts.RowTemplate.Height = 25;
            chartTopProducts.Size = new Size(1175, 664);
            chartTopProducts.TabIndex = 0;
            // 
            // tabPageCategory
            // 
            tabPageCategory.Controls.Add(chartCategoryDistribution);
            tabPageCategory.Location = new Point(4, 29);
            tabPageCategory.Margin = new Padding(3, 4, 3, 4);
            tabPageCategory.Name = "tabPageCategory";
            tabPageCategory.Padding = new Padding(3, 4, 3, 4);
            tabPageCategory.Size = new Size(1181, 672);
            tabPageCategory.TabIndex = 2;
            tabPageCategory.Text = "类别分布";
            tabPageCategory.UseVisualStyleBackColor = true;
            // 
            // chartCategoryDistribution
            // 
            chartCategoryDistribution.AllowUserToAddRows = false;
            chartCategoryDistribution.AllowUserToDeleteRows = false;
            chartCategoryDistribution.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            chartCategoryDistribution.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            chartCategoryDistribution.Dock = DockStyle.Fill;
            chartCategoryDistribution.Location = new Point(3, 4);
            chartCategoryDistribution.Margin = new Padding(3, 4, 3, 4);
            chartCategoryDistribution.Name = "chartCategoryDistribution";
            chartCategoryDistribution.ReadOnly = true;
            chartCategoryDistribution.RowHeadersWidth = 51;
            chartCategoryDistribution.RowTemplate.Height = 25;
            chartCategoryDistribution.Size = new Size(1175, 664);
            chartCategoryDistribution.TabIndex = 0;
            // 
            // tabPageSlowMoving
            // 
            tabPageSlowMoving.Controls.Add(chartSlowMoving);
            tabPageSlowMoving.Location = new Point(4, 29);
            tabPageSlowMoving.Margin = new Padding(3, 4, 3, 4);
            tabPageSlowMoving.Name = "tabPageSlowMoving";
            tabPageSlowMoving.Padding = new Padding(3, 4, 3, 4);
            tabPageSlowMoving.Size = new Size(1181, 672);
            tabPageSlowMoving.TabIndex = 3;
            tabPageSlowMoving.Text = "滞销商品";
            tabPageSlowMoving.UseVisualStyleBackColor = true;
            // 
            // chartSlowMoving
            // 
            chartSlowMoving.AllowUserToAddRows = false;
            chartSlowMoving.AllowUserToDeleteRows = false;
            chartSlowMoving.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            chartSlowMoving.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            chartSlowMoving.Dock = DockStyle.Fill;
            chartSlowMoving.Location = new Point(3, 4);
            chartSlowMoving.Margin = new Padding(3, 4, 3, 4);
            chartSlowMoving.Name = "chartSlowMoving";
            chartSlowMoving.ReadOnly = true;
            chartSlowMoving.RowHeadersWidth = 51;
            chartSlowMoving.RowTemplate.Height = 25;
            chartSlowMoving.Size = new Size(1175, 664);
            chartSlowMoving.TabIndex = 0;
            // 
            // SalesStatisticsForm
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1189, 852);
            Controls.Add(tabControl);
            Controls.Add(panelSummary);
            Controls.Add(panelControl);
            Margin = new Padding(3, 4, 3, 4);
            Name = "SalesStatisticsForm";
            Text = "销售统计报表";
            WindowState = FormWindowState.Maximized;
            panelControl.ResumeLayout(false);
            panelControl.PerformLayout();
            panelSummary.ResumeLayout(false);
            panelSummary.PerformLayout();
            tabControl.ResumeLayout(false);
            tabPageTrend.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)chartSalesTrend).EndInit();
            tabPageTopProducts.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)chartTopProducts).EndInit();
            tabPageCategory.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)chartCategoryDistribution).EndInit();
            tabPageSlowMoving.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)chartSlowMoving).EndInit();
            ResumeLayout(false);

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