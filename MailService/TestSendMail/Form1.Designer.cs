namespace TestSendMail
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
            this.tbMailAddress = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.rtbLoginfo = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbSmtpName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbSmtpPort = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbReceiveAccount = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.cbSendSSl = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tbUserName = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // tbMailAddress
            // 
            this.tbMailAddress.Location = new System.Drawing.Point(102, 15);
            this.tbMailAddress.Name = "tbMailAddress";
            this.tbMailAddress.Size = new System.Drawing.Size(373, 21);
            this.tbMailAddress.TabIndex = 0;
            this.tbMailAddress.Text = "Jackyzhang2001@163.com";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(31, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "邮箱账号";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(603, 430);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "测试发送";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // rtbLoginfo
            // 
            this.rtbLoginfo.Location = new System.Drawing.Point(21, 244);
            this.rtbLoginfo.Name = "rtbLoginfo";
            this.rtbLoginfo.Size = new System.Drawing.Size(601, 184);
            this.rtbLoginfo.TabIndex = 3;
            this.rtbLoginfo.Text = "";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(55, 83);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "密码";
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(102, 80);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.PasswordChar = '*';
            this.tbPassword.Size = new System.Drawing.Size(373, 21);
            this.tbPassword.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 120);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "发送服务器";
            // 
            // tbSmtpName
            // 
            this.tbSmtpName.Location = new System.Drawing.Point(102, 117);
            this.tbSmtpName.Name = "tbSmtpName";
            this.tbSmtpName.Size = new System.Drawing.Size(373, 21);
            this.tbSmtpName.TabIndex = 6;
            this.tbSmtpName.Text = "smtp.163.com";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(31, 151);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 9;
            this.label4.Text = "发送端口";
            // 
            // tbSmtpPort
            // 
            this.tbSmtpPort.Location = new System.Drawing.Point(102, 148);
            this.tbSmtpPort.Name = "tbSmtpPort";
            this.tbSmtpPort.Size = new System.Drawing.Size(373, 21);
            this.tbSmtpPort.TabIndex = 8;
            this.tbSmtpPort.Text = "25";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(31, 217);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 11;
            this.label5.Text = "接收账号";
            // 
            // tbReceiveAccount
            // 
            this.tbReceiveAccount.Location = new System.Drawing.Point(102, 217);
            this.tbReceiveAccount.Name = "tbReceiveAccount";
            this.tbReceiveAccount.Size = new System.Drawing.Size(373, 21);
            this.tbReceiveAccount.TabIndex = 10;
            this.tbReceiveAccount.Text = "8816202@sina.com";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(392, 430);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 12;
            this.button2.Text = "Exchange";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(238, 430);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 13;
            this.button3.Text = "测试发送2";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // cbSendSSl
            // 
            this.cbSendSSl.AutoSize = true;
            this.cbSendSSl.Location = new System.Drawing.Point(33, 181);
            this.cbSendSSl.Name = "cbSendSSl";
            this.cbSendSSl.Size = new System.Drawing.Size(144, 16);
            this.cbSendSSl.TabIndex = 14;
            this.cbSendSSl.Text = "此服务器要求安全连接";
            this.cbSendSSl.UseVisualStyleBackColor = true;
            this.cbSendSSl.CheckedChanged += new System.EventHandler(this.cbSendSSl_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(31, 56);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 12);
            this.label6.TabIndex = 16;
            this.label6.Text = "登录用户名";
            // 
            // tbUserName
            // 
            this.tbUserName.Location = new System.Drawing.Point(102, 53);
            this.tbUserName.Name = "tbUserName";
            this.tbUserName.Size = new System.Drawing.Size(373, 21);
            this.tbUserName.TabIndex = 15;
            this.tbUserName.Text = "Jackyzhang2001@163.com";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(723, 465);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tbUserName);
            this.Controls.Add(this.cbSendSSl);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbReceiveAccount);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbSmtpPort);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbSmtpName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbPassword);
            this.Controls.Add(this.rtbLoginfo);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbMailAddress);
            this.Name = "Form1";
            this.Text = "邮件发送测试";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbMailAddress;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.RichTextBox rtbLoginfo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbSmtpName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbSmtpPort;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbReceiveAccount;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.CheckBox cbSendSSl;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbUserName;
    }
}

