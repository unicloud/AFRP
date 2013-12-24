namespace TestMailService
{
    partial class Form1
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
            this.UpdateXml = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btnUpdateXmlFlag = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // UpdateXml
            // 
            this.UpdateXml.Location = new System.Drawing.Point(36, 12);
            this.UpdateXml.Name = "UpdateXml";
            this.UpdateXml.Size = new System.Drawing.Size(75, 23);
            this.UpdateXml.TabIndex = 1;
            this.UpdateXml.Text = "Update Xml";
            this.UpdateXml.UseVisualStyleBackColor = true;
            this.UpdateXml.Click += new System.EventHandler(this.UpdateXml_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(401, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "label1";
            // 
            // btnUpdateXmlFlag
            // 
            this.btnUpdateXmlFlag.Location = new System.Drawing.Point(155, 12);
            this.btnUpdateXmlFlag.Name = "btnUpdateXmlFlag";
            this.btnUpdateXmlFlag.Size = new System.Drawing.Size(75, 23);
            this.btnUpdateXmlFlag.TabIndex = 3;
            this.btnUpdateXmlFlag.Text = "Update XmlFlag";
            this.btnUpdateXmlFlag.UseVisualStyleBackColor = true;
            this.btnUpdateXmlFlag.Click += new System.EventHandler(this.btnUpdateXmlFlag_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(336, 75);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(106, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "BackupDatabase";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(482, 74);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(138, 23);
            this.button2.TabIndex = 5;
            this.button2.Text = "RestoreDatabase";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(665, 325);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnUpdateXmlFlag);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.UpdateXml);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button UpdateXml;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnUpdateXmlFlag;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}

