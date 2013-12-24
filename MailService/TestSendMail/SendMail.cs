using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using System.Threading.Tasks;


namespace UniCloud.Mail
{
    public class SendMail
    {
        private SmtpClient _Client;

        public string ErrorMessage{get; set;}


        public SendMail()
        {
            _Client = new SmtpClient();
        }

        #region 发送邮箱设置
        private void InitSmtpClient(MailAccount _Sender)
        {
            // 指定 smtp 服务器地址
            _Client.Host = _Sender.SmtpHost;
            // 指定 smtp 服务器的端口，默认是25，如果采用默认端口，可省去
            _Client.Port = _Sender.SendPort;
            // 将smtp的出站方式设为 Network
            _Client.DeliveryMethod = SmtpDeliveryMethod.Network;
            // SMTP服务器需要身份认证
            _Client.UseDefaultCredentials = true;
            // smtp服务器是否启用SSL加密
            _Client.EnableSsl = _Sender.SendSsl;
            _Client.Credentials = new NetworkCredential(_Sender.UserName, _Sender.Password);
        }
        #endregion

        #region 生成邮件


        public MailMessage GenMail(MailAccount Sender, MailAccount Receiver, string mailSubject)
        {
            // 生成邮件
            MailMessage message = new MailMessage(Sender.UserName, Receiver.UserName);
            MailAddress _Sender = new MailAddress(Sender.UserName);
            message.Sender = _Sender;
            message.Subject = mailSubject;
            message.Body = Sender.UserName + " Send " + mailSubject;
            message.Priority = MailPriority.High; 
            return message;
        }

        #endregion

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
                EncodeErrorMessage(ex);
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
                EncodeErrorMessage(ex);
            }
        }


        #endregion


        #region 数据传输

        /// <summary>
        /// 通过邮件发送数据文件的方法
        /// </summary>
        /// <param name="Sender">发送的账号</param>
        /// <param name="Receiver">接收的账号</param>
        /// <param name="obj">发送的对象</param>
        /// <param name="mailSubject">发送邮件的主题</param>
        /// <param name="attName">附件的名称</param>
        public int SendEntity(MailAccount Sender, MailAccount Receiver, string mailSubject)
        {
            try
            {
                //生成邮件
                MailMessage message = this.GenMail(Sender, Receiver, mailSubject);
                //邮箱设置
                InitSmtpClient(Sender);
                //发送邮件
                return this.SendMessage(message);
            }
            catch (Exception ex)
            {
                EncodeErrorMessage(ex);
                return -2;
            }
        }

        #endregion

    }
}
