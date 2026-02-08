namespace Cyclophiops
{
    partial class SettingsForm
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
            this.groupBoxExport = new System.Windows.Forms.GroupBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtCustomPath = new System.Windows.Forms.TextBox();
            this.radioCustomPath = new System.Windows.Forms.RadioButton();
            this.radioDefaultPath = new System.Windows.Forms.RadioButton();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBoxExport.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxExport
            // 
            this.groupBoxExport.Controls.Add(this.btnBrowse);
            this.groupBoxExport.Controls.Add(this.txtCustomPath);
            this.groupBoxExport.Controls.Add(this.radioCustomPath);
            this.groupBoxExport.Controls.Add(this.radioDefaultPath);
            this.groupBoxExport.Location = new System.Drawing.Point(12, 12);
            this.groupBoxExport.Name = "groupBoxExport";
            this.groupBoxExport.Size = new System.Drawing.Size(580, 120);
            this.groupBoxExport.TabIndex = 0;
            this.groupBoxExport.TabStop = false;
            this.groupBoxExport.Text = "导出文件";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(490, 78);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 25);
            this.btnBrowse.TabIndex = 3;
            this.btnBrowse.Text = "浏览...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.BtnBrowse_Click);
            // 
            // txtCustomPath
            // 
            this.txtCustomPath.Location = new System.Drawing.Point(30, 80);
            this.txtCustomPath.Name = "txtCustomPath";
            this.txtCustomPath.Size = new System.Drawing.Size(450, 25);
            this.txtCustomPath.TabIndex = 2;
            // 
            // radioCustomPath
            // 
            this.radioCustomPath.AutoSize = true;
            this.radioCustomPath.Location = new System.Drawing.Point(15, 55);
            this.radioCustomPath.Name = "radioCustomPath";
            this.radioCustomPath.Size = new System.Drawing.Size(88, 19);
            this.radioCustomPath.TabIndex = 1;
            this.radioCustomPath.Text = "选择路径";
            this.radioCustomPath.UseVisualStyleBackColor = true;
            this.radioCustomPath.CheckedChanged += new System.EventHandler(this.RadioCustomPath_CheckedChanged);
            // 
            // radioDefaultPath
            // 
            this.radioDefaultPath.AutoSize = true;
            this.radioDefaultPath.Checked = true;
            this.radioDefaultPath.Location = new System.Drawing.Point(15, 25);
            this.radioDefaultPath.Name = "radioDefaultPath";
            this.radioDefaultPath.Size = new System.Drawing.Size(173, 19);
            this.radioDefaultPath.TabIndex = 0;
            this.radioDefaultPath.TabStop = true;
            this.radioDefaultPath.Text = "软件运行本体（默认）";
            this.radioDefaultPath.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(416, 415);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(85, 30);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(507, 415);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(85, 30);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(604, 457);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.groupBoxExport);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "设置";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.groupBoxExport.ResumeLayout(false);
            this.groupBoxExport.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxExport;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtCustomPath;
        private System.Windows.Forms.RadioButton radioCustomPath;
        private System.Windows.Forms.RadioButton radioDefaultPath;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
    }
}
