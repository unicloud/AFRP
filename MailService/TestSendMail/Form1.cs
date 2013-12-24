using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UniCloud.Mail;

namespace TestSendMail
{
    public partial class Form1 : Form
    {
        private ISendMail _SendMail;

        public Form1()
        {
            InitializeComponent();
            InitData();
        }

        private void InitData()
        {
            //tbAccount.Text = "X02410@shenzhenair.com";
            //tbPassword.Text = "";
            //tbSmtpPort.Text = "25";
            //tbSmtpName.Text = "smtp.shenzhenair.com";
            //tbReceiveAccount.Text = "jackyzhang2001@163.com";
        }

        private BaseMailAccount CreateSender()
        {
            BaseMailAccount _Sender = new BaseMailAccount();
            _Sender.DisplayName = tbMailAddress.Text;
            _Sender.MailAddress = tbMailAddress.Text;
            _Sender.UserName = tbUserName.Text;
            _Sender.Password = tbPassword.Text;

            _Sender.SendPort = int.Parse(tbSmtpPort.Text);
            _Sender.SendHost = this.tbSmtpName.Text;
            _Sender.SendSsl = this.cbSendSSl.Checked;

            return _Sender;
        }

        private BaseMailAccount CreateReceiver()
        {
            BaseMailAccount _Receiver = new BaseMailAccount();
            _Receiver.MailAddress = tbReceiveAccount.Text;
            _Receiver.UserName = tbReceiveAccount.Text;
            return _Receiver;
        }

        private int SendEmail()
        {
            BaseMailAccount _Sender = CreateSender();

            BaseMailAccount _Receiver = CreateReceiver();

            _SendMail = new NetSelfSend();

            return _SendMail.SendTest(_Sender, _Receiver);
        }

        private int SendEmailCDO()
        {
            BaseMailAccount _Sender = CreateSender();

            BaseMailAccount _Receiver = CreateReceiver();

            _SendMail = new NetSelfSend();

            return _SendMail.SendTest(_Sender, _Receiver);
        }

        private int SendEmailLumisoft()
        {
            BaseMailAccount _Sender = CreateSender();

            BaseMailAccount _Receiver = CreateReceiver();

            _SendMail = new LumiSoftSend();

            return _SendMail.SendTest(_Sender, _Receiver);


        }

        private void ShowMessage(string msg)
        {
            this.rtbLoginfo.Text = msg;

            //MessageBox.Show(msg);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.rtbLoginfo.Text = "";
            this.Update();
            try
            {
                if (SendEmail() == 0)
                {
                    ShowMessage("Send Email OK");
                }
                else
                {
                    ShowMessage( _SendMail.GetLastErrorMsg());
                };
            }
            catch(Exception ex )
            {
                ShowMessage(ex.Message);
            };
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.rtbLoginfo.Text = "";
            this.Update();
            try
            {
                if (SendEmailCDO() == 0)
                {
                    ShowMessage("Send Email OK");
                }
                else
                {
                    ShowMessage(_SendMail.GetLastErrorMsg());
                };
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message);
            };
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.rtbLoginfo.Text = "";
            this.Update();
            try
            {
                if (SendEmailLumisoft() == 0)
                {
                    ShowMessage("Send Email OK");
                }
                else
                {
                    ShowMessage(_SendMail.GetLastErrorMsg());
                };
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message);
            };
        }

        private void cbSendSSl_CheckedChanged(object sender, EventArgs e)
        {
            if (cbSendSSl.Checked)
            {
                tbSmtpPort.Text = "465";
            }
            else
            {
                tbSmtpPort.Text = "25";
            }
        }


    }
}
