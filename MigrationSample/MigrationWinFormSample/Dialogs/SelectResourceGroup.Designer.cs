namespace MigrationSample.Dialogs
{
    partial class SelectResourceGroup
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
            this.lblEnvironement = new System.Windows.Forms.Label();
            this.comboEnvironment = new System.Windows.Forms.ComboBox();
            this.subscriptinsLbl = new System.Windows.Forms.Label();
            this.SubscriptionsGridView = new System.Windows.Forms.DataGridView();
            this.resourceGroupLbl = new System.Windows.Forms.Label();
            this.comboResourceGroup = new System.Windows.Forms.ComboBox();
            this.selectBtn = new System.Windows.Forms.Button();
            this.mainResoureceGroupTlp = new System.Windows.Forms.TableLayoutPanel();
            this.environementFlp = new System.Windows.Forms.FlowLayoutPanel();
            this.resourceGroupFlp = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.SubscriptionsGridView)).BeginInit();
            this.mainResoureceGroupTlp.SuspendLayout();
            this.environementFlp.SuspendLayout();
            this.resourceGroupFlp.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblEnvironement
            // 
            this.lblEnvironement.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEnvironement.Location = new System.Drawing.Point(3, 0);
            this.lblEnvironement.Name = "lblEnvironement";
            this.lblEnvironement.Size = new System.Drawing.Size(114, 23);
            this.lblEnvironement.TabIndex = 0;
            this.lblEnvironement.Text = "Environment:";
            // 
            // comboEnvironment
            // 
            this.comboEnvironment.Dock = System.Windows.Forms.DockStyle.Right;
            this.comboEnvironment.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.environementFlp.SetFlowBreak(this.comboEnvironment, true);
            this.comboEnvironment.FormattingEnabled = true;
            this.comboEnvironment.Location = new System.Drawing.Point(123, 3);
            this.comboEnvironment.Name = "comboEnvironment";
            this.comboEnvironment.Size = new System.Drawing.Size(248, 21);
            this.comboEnvironment.TabIndex = 1;
            this.comboEnvironment.SelectedIndexChanged += new System.EventHandler(this.comboEnvironment_SelectedIndexChanged);
            // 
            // subscriptinsLbl
            // 
            this.subscriptinsLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.subscriptinsLbl.Location = new System.Drawing.Point(3, 30);
            this.subscriptinsLbl.Name = "subscriptinsLbl";
            this.subscriptinsLbl.Size = new System.Drawing.Size(91, 24);
            this.subscriptinsLbl.TabIndex = 2;
            this.subscriptinsLbl.Text = "Subscription:";
            // 
            // SubscriptionsGridView
            // 
            this.SubscriptionsGridView.AllowUserToAddRows = false;
            this.SubscriptionsGridView.AllowUserToDeleteRows = false;
            this.SubscriptionsGridView.AllowUserToOrderColumns = true;
            this.SubscriptionsGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.SubscriptionsGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.SubscriptionsGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SubscriptionsGridView.Location = new System.Drawing.Point(3, 63);
            this.SubscriptionsGridView.Name = "SubscriptionsGridView";
            this.SubscriptionsGridView.ReadOnly = true;
            this.SubscriptionsGridView.Size = new System.Drawing.Size(468, 410);
            this.SubscriptionsGridView.TabIndex = 7;
            this.SubscriptionsGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            this.SubscriptionsGridView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.SubscriptionsGridView_MouseUp);
            // 
            // resourceGroupLbl
            // 
            this.resourceGroupLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resourceGroupLbl.Location = new System.Drawing.Point(3, 0);
            this.resourceGroupLbl.Name = "resourceGroupLbl";
            this.resourceGroupLbl.Size = new System.Drawing.Size(125, 24);
            this.resourceGroupLbl.TabIndex = 4;
            this.resourceGroupLbl.Text = "Resource Group:";
            // 
            // comboResourceGroup
            // 
            this.comboResourceGroup.Dock = System.Windows.Forms.DockStyle.Right;
            this.comboResourceGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.resourceGroupFlp.SetFlowBreak(this.comboResourceGroup, true);
            this.comboResourceGroup.FormattingEnabled = true;
            this.comboResourceGroup.Location = new System.Drawing.Point(134, 3);
            this.comboResourceGroup.Name = "comboResourceGroup";
            this.comboResourceGroup.Size = new System.Drawing.Size(325, 21);
            this.comboResourceGroup.TabIndex = 5;
            // 
            // selectBtn
            // 
            this.selectBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.selectBtn.Location = new System.Drawing.Point(3, 509);
            this.selectBtn.Name = "selectBtn";
            this.selectBtn.Size = new System.Drawing.Size(286, 23);
            this.selectBtn.TabIndex = 6;
            this.selectBtn.Text = "Select";
            this.selectBtn.UseVisualStyleBackColor = true;
            this.selectBtn.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // mainResoureceGroupTlp
            // 
            this.mainResoureceGroupTlp.ColumnCount = 1;
            this.mainResoureceGroupTlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainResoureceGroupTlp.Controls.Add(this.subscriptinsLbl, 0, 1);
            this.mainResoureceGroupTlp.Controls.Add(this.SubscriptionsGridView, 0, 2);
            this.mainResoureceGroupTlp.Controls.Add(this.selectBtn, 0, 4);
            this.mainResoureceGroupTlp.Controls.Add(this.environementFlp, 0, 0);
            this.mainResoureceGroupTlp.Controls.Add(this.resourceGroupFlp, 0, 3);
            this.mainResoureceGroupTlp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainResoureceGroupTlp.Location = new System.Drawing.Point(0, 0);
            this.mainResoureceGroupTlp.Name = "mainResoureceGroupTlp";
            this.mainResoureceGroupTlp.RowCount = 5;
            this.mainResoureceGroupTlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.mainResoureceGroupTlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.mainResoureceGroupTlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainResoureceGroupTlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.mainResoureceGroupTlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.mainResoureceGroupTlp.Size = new System.Drawing.Size(474, 536);
            this.mainResoureceGroupTlp.TabIndex = 1;
            // 
            // environementFlp
            // 
            this.environementFlp.Controls.Add(this.lblEnvironement);
            this.environementFlp.Controls.Add(this.comboEnvironment);
            this.environementFlp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.environementFlp.Location = new System.Drawing.Point(3, 3);
            this.environementFlp.Name = "environementFlp";
            this.environementFlp.Size = new System.Drawing.Size(468, 24);
            this.environementFlp.TabIndex = 8;
            // 
            // resourceGroupFlp
            // 
            this.resourceGroupFlp.Controls.Add(this.resourceGroupLbl);
            this.resourceGroupFlp.Controls.Add(this.comboResourceGroup);
            this.resourceGroupFlp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resourceGroupFlp.Location = new System.Drawing.Point(3, 479);
            this.resourceGroupFlp.Name = "resourceGroupFlp";
            this.resourceGroupFlp.Size = new System.Drawing.Size(468, 24);
            this.resourceGroupFlp.TabIndex = 9;
            // 
            // SelectResourceGroup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(474, 536);
            this.Controls.Add(this.mainResoureceGroupTlp);
            this.Name = "SelectResourceGroup";
            this.Text = "Select Power BI Workspace Collections Resource Group";
            this.Load += new System.EventHandler(this.SelectResourceGroup_Load);
            ((System.ComponentModel.ISupportInitialize)(this.SubscriptionsGridView)).EndInit();
            this.mainResoureceGroupTlp.ResumeLayout(false);
            this.environementFlp.ResumeLayout(false);
            this.resourceGroupFlp.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label lblEnvironement;
        private System.Windows.Forms.ComboBox comboEnvironment;
        private System.Windows.Forms.Label subscriptinsLbl;
        private System.Windows.Forms.Label resourceGroupLbl;
        private System.Windows.Forms.ComboBox comboResourceGroup;
        private System.Windows.Forms.Button selectBtn;
        private System.Windows.Forms.DataGridView SubscriptionsGridView;
        private System.Windows.Forms.FlowLayoutPanel environementFlp;
        private System.Windows.Forms.FlowLayoutPanel resourceGroupFlp;
        private System.Windows.Forms.TableLayoutPanel mainResoureceGroupTlp;
    }
}