using System;
using System.Net;
using System.Net.Mail;
using System.IO;


namespace UniCloud.Mail
{
    public class NetSelfSend : ISendMail
    {


        private SmtpClient _Client;

        private string ErrorMessage;

        public NetSelfSend()
        {
            _Client = new SmtpClient();
        }

        private void EncodeErrorMessage(Exception ex)
        {
            string CharReturn = "\n";
            if (ex.InnerException != null)
            {
                ErrorMessage = ex.InnerException.Message + CharReturn + ex.Message;
            }
            else
            {
                ErrorMessage = ex.Message;
            }
        }

        private MailMessage CreateTestMail(BaseMailAccount Sender, BaseMailAccount Receiver)
        {
            MailMessage msg = new MailMessage(Sender.MailAddress, Receiver.MailAddress);
            msg.Sender = new MailAddress(Sender.MailAddress, Sender.DisplayName);
            msg.Subject = "Test Send Mail By Net.Mail";
            msg.Body = "这是一份测试邮件，来自厦门至正测试程序";
            msg.Priority = MailPriority.High;

            //测试邮件不发送附件

            return msg;
        }

        #region 发送邮箱设置
        private void InitSmtpClient(BaseMailAccount Sender)
        {
            // 指定 smtp 服务器地址
            _Client.Host = Sender.SendHost;
            // 指定 smtp 服务器的端口，默认是25，如果采用默认端口，可省去
            _Client.Port = Sender.SendPort;
            // 将smtp的出站方式设为 Network
            _Client.DeliveryMethod = SmtpDeliveryMethod.Network;
            // SMTP服务器需要身份认证
            _Client.UseDefaultCredentials = true;
            // smtp服务器是否启用SSL加密
            _Client.EnableSsl = false;
            //设置发信凭据
            _Client.Credentials = new NetworkCredential(Sender.UserName, Sender.Password);
        }
        #endregion

        #region 邮件转换
        public MailMessage TransferMessage(BaseMailMessage message)
        {
            // 生成邮件
            MailMessage msg = new MailMessage(message.From, message.To);
            msg.Sender = new MailAddress(message.Sender, message.DisplayName);
            msg.From = new MailAddress(message.From, message.DisplayName);
            msg.Subject = message.Subject;
            msg.Body = message.Body;
            msg.Priority = message.Priority;

            foreach (BaseAttachment attach in message.Attachments)
            {
                if (attach.AttachStream != null)
                {
                    msg.Attachments.Add(new Attachment(attach.AttachStream, attach.FileName));
                }
            }
            return msg;
        }
        #endregion


        #region 发送邮件
        private int SendMessage(MailMessage message)
        {
            try
            {
                _Client.Send(message);
                return 0;
            }
            catch (Exception ex)
            {
                this.EncodeErrorMessage(ex);
                return -1;
            }
        }
        #endregion

        #region 断开连接
        private void CloseConnection()
        {
            try
            {
                _Client.Dispose();
            }
            catch (Exception ex)
            {
                this.EncodeErrorMessage(ex);
            }
        }
        #endregion


        public bool CanSend(BaseMailAccount Sender)
        {
            return Sender.SendSsl == false;
        }

        public int SendTest(BaseMailAccount Sender, BaseMailAccount Receiver)
        {
            try
            {
                //生成邮件
                MailMessage msg = CreateTestMail(Sender, Receiver);
                //邮箱设置
                InitSmtpClient(Sender);
                //发送邮件
                int intSend = SendMessage(msg);
                //断开连接
                CloseConnection();

                return intSend;
            }
            catch (Exception ex)
            {
                this.EncodeErrorMessage(ex);
                return -3;
            }
        }

        public int SendMail(BaseMailAccount Sender, BaseMailAccount Receiver, BaseMailMessage message)
        {
            try
            {
                //生成邮件
                MailMessage msg = TransferMessage(message);
                //邮箱设置
                InitSmtpClient(Sender);
                //发送邮件
                int intSend = SendMessage(msg);
                //断开连接
                CloseConnection();

                return intSend;
            }
            catch (Exception ex)
            {
                this.EncodeErrorMessage(ex);
                return -3;
            }
        }

        public string GetLastErrorMsg()
        {
            return ErrorMessage;
        }

    }
}
