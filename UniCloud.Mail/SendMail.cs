using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using UniCloud.Fleet.Models;
using UniCloud.Cryptography;

namespace UniCloud.Mail
{
    public class SendMail
    {

        public SendMail()
        {
        }


        #region 生成邮件

        private BaseAttachment CreateMessageAttachment(object obj, string attName)
        {
            BaseAttachment attach = new BaseAttachment();
            attach.AttachStream = ObjAttachHelper.ModelObjToAttachmentStream(obj);
            attach.FileName = attName;
            return attach;
        }

        public BaseMailMessage GenMail(BaseMailAccount Sender, BaseMailAccount Receiver, object obj, string mailSubject, string attName)
        {
            // 生成邮件
            BaseMailMessage message = new BaseMailMessage();
            message.Sender = Sender.MailAddress;
            message.DisplayName = Sender.DisplayName;
            message.From = Sender.MailAddress;
            message.To = Receiver.MailAddress;
            message.Subject = mailSubject;
            message.Body = mailSubject + "\r\n" + "来自" + Sender.UserName;
            message.Priority = MailPriority.High;
            message.Attachments.Add(CreateMessageAttachment(obj, attName));
            return message;
        }

        #endregion

        #region 发送邮件
        private int SendMessage(BaseMailAccount Sender, BaseMailAccount Receiver, BaseMailMessage message)
        {
            try
            {
                SendContainer sc = new SendContainer();
                ISendMail ISM = sc.GetSendClassBySender(Sender);
                if(ISM != null)
                {
                   return ISM.SendMail(Sender, Receiver, message);
                }
                return -1;
            }
            catch (Exception ex) 
            {
                return -2;
            }
        }
        #endregion

        #region 数据传输

        /// <summary>
        /// 通过邮件发送数据文件的方法
        /// </summary>
        /// <param name="obj">发送的对象</param>
        /// <param name="mailSubject">发送邮件的主题</param>
        /// <param name="attName">附件的名称</param>
        public int SendEntity(string Conn, Guid Sender, Guid Receiver, object obj, string mailSubject, string attName)
        {
            try
            {
                FleetEntities FE = new FleetEntities(Conn);
                //接收账号
                BaseMailAccount _ReceiverAccount = MailAccountHelper.GetMailAccount(Receiver, FE);
                //发送账号
                BaseMailAccount _SenderAccount = MailAccountHelper.GetMailAccount(Sender, FE);
                //发送对象
                return SendEntity(_SenderAccount, _ReceiverAccount, obj, mailSubject, attName);
            }
            catch( Exception ex)
            {
                return -4;
            }
        }

        /// <summary>
        /// 通过邮件发送数据文件的方法
        /// </summary>
        /// <param name="Sender">发送的账号</param>
        /// <param name="Receiver">接收的账号</param>
        /// <param name="obj">发送的对象</param>
        /// <param name="mailSubject">发送邮件的主题</param>
        /// <param name="attName">附件的名称</param>
        public int SendEntity(BaseMailAccount Sender, BaseMailAccount Receiver, object obj, string mailSubject, string attName)
        {
            try
            {
                //生成邮件
                BaseMailMessage message = this.GenMail(Sender, Receiver, obj, mailSubject, attName);
                //发送邮件
                return this.SendMessage(Sender, Receiver, message);
            }
            catch (Exception ex)
            {
                return -3;
            }
        }

        #endregion

        #region 发送测试邮件
        /// <summary>
        /// 测试邮件发送
        /// </summary>
        /// <param name="Sender">发送的账号</param>
        /// <param name="Receiver">接收的账号</param>
        public int SendTestMail(BaseMailAccount Sender, BaseMailAccount Receiver)
        {
            try
            {
                SendContainer sc = new SendContainer();
                ISendMail ISM = sc.GetSendClassBySender(Sender);
                if (ISM != null)
                {
                    return ISM.SendTest(Sender, Receiver);
                }
                return -1;
            }
            catch (Exception ex)
            {
                return -2;
            }
        }
        #endregion
    }
}
