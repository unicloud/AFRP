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
            this.components = new System.ComponentModel.Container();
            this.Receive = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.UpdateXml = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btnUpdateXmlFlag = new System.Windows.Forms.Button();
            this.btnSaveObject = new System.Windows.Forms.Button();
            this.btnSendMail = new System.Windows.Forms.Button();
            this.Encryption = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Receive
            // 
            this.Receive.Location = new System.Drawing.Point(23, 12);
            this.Receive.Name = "Receive";
            this.Receive.Size = new System.Drawing.Size(114, 23);
            this.Receive.TabIndex = 0;
            this.Receive.Text = "Receive Mail";
            this.Receive.UseVisualStyleBackColor = true;
            this.Receive.Click += new System.EventHandler(this.button1_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 10000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // UpdateXml
            // 
            this.UpdateXml.Location = new System.Drawing.Point(169, 12);
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
            this.btnUpdateXmlFlag.Location = new System.Drawing.Point(320, 12);
            this.btnUpdateXmlFlag.Name = "btnUpdateXmlFlag";
            this.btnUpdateXmlFlag.Size = new System.Drawing.Size(75, 23);
            this.btnUpdateXmlFlag.TabIndex = 3;
            this.btnUpdateXmlFlag.Text = "Update XmlFlag";
            this.btnUpdateXmlFlag.UseVisualStyleBackColor = true;
            this.btnUpdateXmlFlag.Click += new System.EventHandler(this.btnUpdateXmlFlag_Click);
            // 
            // btnSaveObject
            // 
            this.btnSaveObject.Location = new System.Drawing.Point(169, 69);
            this.btnSaveObject.Name = "btnSaveObject";
            this.btnSaveObject.Size = new System.Drawing.Size(75, 23);
            this.btnSaveObject.TabIndex = 4;
            this.btnSaveObject.Text = "SaveObject";
            this.btnSaveObject.UseVisualStyleBackColor = true;
            this.btnSaveObject.Click += new System.EventHandler(this.btnSaveObject_Click);
            // 
            // btnSendMail
            // 
            this.btnSendMail.Location = new System.Drawing.Point(35, 69);
            this.btnSendMail.Name = "btnSendMail";
            this.btnSendMail.Size = new System.Drawing.Size(95, 23);
            this.btnSendMail.TabIndex = 5;
            this.btnSendMail.Text = "Send Mail";
            this.btnSendMail.UseVisualStyleBackColor = true;
            this.btnSendMail.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // Encryption
            // 
            this.Encryption.Location = new System.Drawing.Point(46, 122);
            this.Encryption.Name = "Encryption";
            this.Encryption.Size = new System.Drawing.Size(75, 23);
            this.Encryption.TabIndex = 6;
            this.Encryption.Text = "Encryption";
            this.Encryption.UseVisualStyleBackColor = true;
            this.Encryption.Click += new System.EventHandler(this.button1_Click_2);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(46, 168);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "Test DesC";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_3);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(665, 325);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.Encryption);
            this.Controls.Add(this.btnSendMail);
            this.Controls.Add(this.btnSaveObject);
            this.Controls.Add(this.btnUpdateXmlFlag);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.UpdateXml);
            this.Controls.Add(this.Receive);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Receive;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button UpdateXml;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnUpdateXmlFlag;
        private System.Windows.Forms.Button btnSaveObject;
        private System.Windows.Forms.Button btnSendMail;
        private System.Windows.Forms.Button Encryption;
        private System.Windows.Forms.Button button1;
    }
}

