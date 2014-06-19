namespace GetFeatureInfo
{
    partial class FrmGetFeatureInfo
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmGetFeatureInfo));
            this.gBoxSequence = new System.Windows.Forms.GroupBox();
            this.btnViewSeqFile = new System.Windows.Forms.Button();
            this.rTxtBoxSequence = new System.Windows.Forms.RichTextBox();
            this.btnSequenceUsingFile = new System.Windows.Forms.Button();
            this.gBoxFeature = new System.Windows.Forms.GroupBox();
            this.btnViewFeaFile = new System.Windows.Forms.Button();
            this.btnFeatureUsingFile = new System.Windows.Forms.Button();
            this.cBoxMod4 = new System.Windows.Forms.CheckBox();
            this.cBoxMod3 = new System.Windows.Forms.CheckBox();
            this.cBoxMod2 = new System.Windows.Forms.CheckBox();
            this.cBoxMod1 = new System.Windows.Forms.CheckBox();
            this.rTxtBoxFeature = new System.Windows.Forms.RichTextBox();
            this.gBoxResult = new System.Windows.Forms.GroupBox();
            this.rTxtBoxFeatureName = new System.Windows.Forms.RichTextBox();
            this.rTxtBoxResult = new System.Windows.Forms.RichTextBox();
            this.btnSavaToFile = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.gBoxSequence.SuspendLayout();
            this.gBoxFeature.SuspendLayout();
            this.gBoxResult.SuspendLayout();
            this.SuspendLayout();
            // 
            // gBoxSequence
            // 
            this.gBoxSequence.Controls.Add(this.btnViewSeqFile);
            this.gBoxSequence.Controls.Add(this.rTxtBoxSequence);
            this.gBoxSequence.Controls.Add(this.btnSequenceUsingFile);
            this.gBoxSequence.Location = new System.Drawing.Point(6, 7);
            this.gBoxSequence.Name = "gBoxSequence";
            this.gBoxSequence.Size = new System.Drawing.Size(245, 232);
            this.gBoxSequence.TabIndex = 0;
            this.gBoxSequence.TabStop = false;
            this.gBoxSequence.Text = "序列";
            // 
            // btnViewSeqFile
            // 
            this.btnViewSeqFile.Location = new System.Drawing.Point(40, 200);
            this.btnViewSeqFile.Name = "btnViewSeqFile";
            this.btnViewSeqFile.Size = new System.Drawing.Size(75, 25);
            this.btnViewSeqFile.TabIndex = 2;
            this.btnViewSeqFile.Tag = "sv";
            this.btnViewSeqFile.Text = "查看详情";
            this.btnViewSeqFile.UseVisualStyleBackColor = true;
            this.btnViewSeqFile.Click += new System.EventHandler(this.btnViewFile_Click);
            // 
            // rTxtBoxSequence
            // 
            this.rTxtBoxSequence.AcceptsTab = true;
            this.rTxtBoxSequence.Location = new System.Drawing.Point(6, 18);
            this.rTxtBoxSequence.Name = "rTxtBoxSequence";
            this.rTxtBoxSequence.Size = new System.Drawing.Size(232, 173);
            this.rTxtBoxSequence.TabIndex = 0;
            this.rTxtBoxSequence.Text = "";
            this.rTxtBoxSequence.WordWrap = false;
            // 
            // btnSequenceUsingFile
            // 
            this.btnSequenceUsingFile.Location = new System.Drawing.Point(135, 200);
            this.btnSequenceUsingFile.Name = "btnSequenceUsingFile";
            this.btnSequenceUsingFile.Size = new System.Drawing.Size(75, 25);
            this.btnSequenceUsingFile.TabIndex = 1;
            this.btnSequenceUsingFile.Tag = "su";
            this.btnSequenceUsingFile.Text = "使用文件";
            this.btnSequenceUsingFile.UseVisualStyleBackColor = true;
            this.btnSequenceUsingFile.Click += new System.EventHandler(this.btnUsingFile_Click);
            // 
            // gBoxFeature
            // 
            this.gBoxFeature.Controls.Add(this.btnViewFeaFile);
            this.gBoxFeature.Controls.Add(this.btnFeatureUsingFile);
            this.gBoxFeature.Controls.Add(this.cBoxMod4);
            this.gBoxFeature.Controls.Add(this.cBoxMod3);
            this.gBoxFeature.Controls.Add(this.cBoxMod2);
            this.gBoxFeature.Controls.Add(this.cBoxMod1);
            this.gBoxFeature.Controls.Add(this.rTxtBoxFeature);
            this.gBoxFeature.Location = new System.Drawing.Point(260, 7);
            this.gBoxFeature.Name = "gBoxFeature";
            this.gBoxFeature.Size = new System.Drawing.Size(245, 232);
            this.gBoxFeature.TabIndex = 1;
            this.gBoxFeature.TabStop = false;
            this.gBoxFeature.Text = "特征";
            // 
            // btnViewFeaFile
            // 
            this.btnViewFeaFile.Location = new System.Drawing.Point(43, 200);
            this.btnViewFeaFile.Name = "btnViewFeaFile";
            this.btnViewFeaFile.Size = new System.Drawing.Size(75, 25);
            this.btnViewFeaFile.TabIndex = 7;
            this.btnViewFeaFile.Tag = "fv";
            this.btnViewFeaFile.Text = "查看详情";
            this.btnViewFeaFile.UseVisualStyleBackColor = true;
            this.btnViewFeaFile.Click += new System.EventHandler(this.btnViewFile_Click);
            // 
            // btnFeatureUsingFile
            // 
            this.btnFeatureUsingFile.Location = new System.Drawing.Point(136, 200);
            this.btnFeatureUsingFile.Name = "btnFeatureUsingFile";
            this.btnFeatureUsingFile.Size = new System.Drawing.Size(75, 25);
            this.btnFeatureUsingFile.TabIndex = 6;
            this.btnFeatureUsingFile.Tag = "fu";
            this.btnFeatureUsingFile.Text = "使用文件";
            this.btnFeatureUsingFile.UseVisualStyleBackColor = true;
            this.btnFeatureUsingFile.Click += new System.EventHandler(this.btnUsingFile_Click);
            // 
            // cBoxMod4
            // 
            this.cBoxMod4.AutoSize = true;
            this.cBoxMod4.Location = new System.Drawing.Point(6, 90);
            this.cBoxMod4.Name = "cBoxMod4";
            this.cBoxMod4.Size = new System.Drawing.Size(119, 17);
            this.cBoxMod4.TabIndex = 4;
            this.cBoxMod4.Text = "histone modification";
            this.cBoxMod4.UseVisualStyleBackColor = true;
            // 
            // cBoxMod3
            // 
            this.cBoxMod3.AutoSize = true;
            this.cBoxMod3.Location = new System.Drawing.Point(6, 66);
            this.cBoxMod3.Name = "cBoxMod3";
            this.cBoxMod3.Size = new System.Drawing.Size(124, 17);
            this.cBoxMod3.TabIndex = 3;
            this.cBoxMod3.Text = "repeat element(rmsk)";
            this.cBoxMod3.UseVisualStyleBackColor = true;
            // 
            // cBoxMod2
            // 
            this.cBoxMod2.AutoSize = true;
            this.cBoxMod2.Location = new System.Drawing.Point(6, 42);
            this.cBoxMod2.Name = "cBoxMod2";
            this.cBoxMod2.Size = new System.Drawing.Size(90, 17);
            this.cBoxMod2.TabIndex = 2;
            this.cBoxMod2.Text = "tfbsConsSites";
            this.cBoxMod2.UseVisualStyleBackColor = true;
            // 
            // cBoxMod1
            // 
            this.cBoxMod1.AutoSize = true;
            this.cBoxMod1.Location = new System.Drawing.Point(6, 18);
            this.cBoxMod1.Name = "cBoxMod1";
            this.cBoxMod1.Size = new System.Drawing.Size(137, 17);
            this.cBoxMod1.TabIndex = 1;
            this.cBoxMod1.Text = "sequence pattern(motif)";
            this.cBoxMod1.UseVisualStyleBackColor = true;
            // 
            // rTxtBoxFeature
            // 
            this.rTxtBoxFeature.AcceptsTab = true;
            this.rTxtBoxFeature.Location = new System.Drawing.Point(6, 113);
            this.rTxtBoxFeature.Name = "rTxtBoxFeature";
            this.rTxtBoxFeature.Size = new System.Drawing.Size(232, 78);
            this.rTxtBoxFeature.TabIndex = 5;
            this.rTxtBoxFeature.Text = "";
            this.rTxtBoxFeature.WordWrap = false;
            // 
            // gBoxResult
            // 
            this.gBoxResult.Controls.Add(this.rTxtBoxFeatureName);
            this.gBoxResult.Controls.Add(this.rTxtBoxResult);
            this.gBoxResult.Controls.Add(this.btnSavaToFile);
            this.gBoxResult.Controls.Add(this.btnClear);
            this.gBoxResult.Location = new System.Drawing.Point(6, 276);
            this.gBoxResult.Name = "gBoxResult";
            this.gBoxResult.Size = new System.Drawing.Size(499, 354);
            this.gBoxResult.TabIndex = 2;
            this.gBoxResult.TabStop = false;
            this.gBoxResult.Text = "结果";
            // 
            // rTxtBoxFeatureName
            // 
            this.rTxtBoxFeatureName.Location = new System.Drawing.Point(6, 22);
            this.rTxtBoxFeatureName.Name = "rTxtBoxFeatureName";
            this.rTxtBoxFeatureName.Size = new System.Drawing.Size(486, 87);
            this.rTxtBoxFeatureName.TabIndex = 3;
            this.rTxtBoxFeatureName.Text = "";
            this.rTxtBoxFeatureName.WordWrap = false;
            // 
            // rTxtBoxResult
            // 
            this.rTxtBoxResult.Location = new System.Drawing.Point(6, 116);
            this.rTxtBoxResult.Name = "rTxtBoxResult";
            this.rTxtBoxResult.Size = new System.Drawing.Size(486, 201);
            this.rTxtBoxResult.TabIndex = 0;
            this.rTxtBoxResult.Text = "";
            this.rTxtBoxResult.WordWrap = false;
            // 
            // btnSavaToFile
            // 
            this.btnSavaToFile.Location = new System.Drawing.Point(170, 324);
            this.btnSavaToFile.Name = "btnSavaToFile";
            this.btnSavaToFile.Size = new System.Drawing.Size(75, 25);
            this.btnSavaToFile.TabIndex = 1;
            this.btnSavaToFile.Text = "保存结果";
            this.btnSavaToFile.UseVisualStyleBackColor = true;
            this.btnSavaToFile.Click += new System.EventHandler(this.btnSavaToFile_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(254, 324);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 25);
            this.btnClear.TabIndex = 2;
            this.btnClear.Text = "清空结果";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(141, 245);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 25);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "开始计算";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnExit
            // 
            this.btnExit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnExit.Location = new System.Drawing.Point(222, 245);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(75, 25);
            this.btnExit.TabIndex = 2;
            this.btnExit.Text = "退出程序";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(303, 245);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(75, 25);
            this.btnReset.TabIndex = 3;
            this.btnReset.Text = "复位程序";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // FrmGetFeatureInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnExit;
            this.ClientSize = new System.Drawing.Size(513, 644);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.gBoxResult);
            this.Controls.Add(this.gBoxFeature);
            this.Controls.Add(this.gBoxSequence);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FrmGetFeatureInfo";
            this.Text = "DNA 序列特征提取 V2.1";
            this.Load += new System.EventHandler(this.FrmGetFeatureInfo_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmGetFeatureInfo_FormClosing);
            this.gBoxSequence.ResumeLayout(false);
            this.gBoxFeature.ResumeLayout(false);
            this.gBoxFeature.PerformLayout();
            this.gBoxResult.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gBoxSequence;
        public System.Windows.Forms.RichTextBox rTxtBoxSequence;
        private System.Windows.Forms.GroupBox gBoxFeature;
        public System.Windows.Forms.RichTextBox rTxtBoxFeature;
        private System.Windows.Forms.GroupBox gBoxResult;
        public System.Windows.Forms.RichTextBox rTxtBoxResult;
        private System.Windows.Forms.Button btnSavaToFile;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnSequenceUsingFile;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button btnFeatureUsingFile;
        private System.Windows.Forms.CheckBox cBoxMod4;
        private System.Windows.Forms.CheckBox cBoxMod3;
        private System.Windows.Forms.CheckBox cBoxMod2;
        private System.Windows.Forms.CheckBox cBoxMod1;
        private System.Windows.Forms.Button btnViewSeqFile;
        private System.Windows.Forms.Button btnViewFeaFile;
        private System.Windows.Forms.Button btnReset;
        public System.Windows.Forms.RichTextBox rTxtBoxFeatureName;
    }
}

