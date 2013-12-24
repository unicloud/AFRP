using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LumiSoft.Net.Mail;
using LumiSoft.Net.MIME;

namespace UniCloud.Mail
{
    public class LumiSoftSend : ISendMail
    {
        private string ErrorMessage;

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

        private Mail_Message CreateTestMail(BaseMailAccount Sender, BaseMailAccount Receiver)
        {
            Mail_Message msg = new Mail_Message();
            msg.MimeVersion = "1.0";
            msg.MessageID = MIME_Utils.CreateMessageID();
            msg.Date = DateTime.Now;
            msg.From = new Mail_t_MailboxList();
            msg.From.Add(new Mail_t_Mailbox(Sender.DisplayName, Sender.MailAddress));
            msg.To = new Mail_t_AddressList();
            msg.To.Add(new Mail_t_Mailbox(Receiver.MailAddress, Receiver.MailAddress));
            msg.Subject = "Test Send Mail";
            //设置回执通知
            //string notifyEmail = SystemConfig.Default.DispositionNotificationTo;
            //if (!string.IsNullOrEmpty(notifyEmail) && ValidateUtil.IsEmail(notifyEmail))
            //{
            //    msg.DispositionNotificationTo = new Mail_t_Mailbox(notifyEmail, notifyEmail);
            //}

            #region MyRegion

            string BodyText = "这是一份测试邮件，来自<font color=red><b>厦门至正测试程序</b></font>";
            CreateBodyPart(msg, BodyText);

            //测试邮件不发送附件

            #endregion

            return msg;
        }

        private static void CreateBodyPart(Mail_Message msg, string BodyText)
        {
            //--- multipart/mixed -----------------------------------
            MIME_h_ContentType contentType_multipartMixed = new MIME_h_ContentType(MIME_MediaTypes.Multipart.mixed);
            contentType_multipartMixed.Param_Boundary = Guid.NewGuid().ToString().Replace('-', '.');
            MIME_b_MultipartMixed multipartMixed = new MIME_b_MultipartMixed(contentType_multipartMixed);
            msg.Body = multipartMixed;

            //--- multipart/alternative -----------------------------
            MIME_Entity entity_multipartAlternative = new MIME_Entity();
            MIME_h_ContentType contentType_multipartAlternative = new MIME_h_ContentType(MIME_MediaTypes.Multipart.alternative);
            contentType_multipartAlternative.Param_Boundary = Guid.NewGuid().ToString().Replace('-', '.');
            MIME_b_MultipartAlternative multipartAlternative = new MIME_b_MultipartAlternative(contentType_multipartAlternative);
            entity_multipartAlternative.Body = multipartAlternative;
            multipartMixed.BodyParts.Add(entity_multipartAlternative);

             //--- text/plain ----------------------------------------
            MIME_Entity entity_text_plain = new MIME_Entity();
            MIME_b_Text text_plain = new MIME_b_Text(MIME_MediaTypes.Text.plain);
            entity_text_plain.Body = text_plain;
            entity_text_plain.ContentTransferEncoding = MIME_TransferEncodings.Base64;
            //普通文本邮件内容，如果对方的收件客户端不支持HTML，这是必需的
            string plainTextBody = "如果你邮件客户端不支持HTML格式，或者你切换到“普通文本”视图，将看到此内容";
            //if (!string.IsNullOrEmpty(SystemConfig.Default.PlaintTextTips))
            //{
            //    plainTextBody = SystemConfig.Default.PlaintTextTips;
            //}

            text_plain.SetText(MIME_TransferEncodings.Base64, Encoding.UTF8, plainTextBody);
            multipartAlternative.BodyParts.Add(entity_text_plain);

            //--- text/html -----------------------------------------
            string htmlText = "<html>" + BodyText.Replace("\r\n", "<br>") + "</html>"; 
            MIME_Entity entity_text_html = new MIME_Entity();
            MIME_b_Text text_html = new MIME_b_Text(MIME_MediaTypes.Text.html);
            entity_text_html.Body = text_html;
            text_html.SetText(MIME_TransferEncodings.QuotedPrintable, Encoding.UTF8, htmlText);
            multipartAlternative.BodyParts.Add(entity_text_html);
        }

        private static void CreateAttachmentPart(Mail_Message msg, List<BaseAttachment> Attachments)
        {
            if (Attachments != null)
            {
                MIME_b_MultipartMixed multipartMixed = (MIME_b_MultipartMixed)msg.Body;

                foreach (BaseAttachment attach in Attachments)
                {
                    if (attach.AttachStream != null)
                    {
                        multipartMixed.BodyParts.Add(Mail_Message.CreateAttachment(attach.AttachStream, attach.FileName));
                    }
                }
            }
        }

        #region 邮件转换
        public Mail_Message TransferMessage(BaseMailMessage message)
        {
            // 生成邮件
            Mail_Message msg = new Mail_Message();
            msg.MimeVersion = "1.0";
            msg.MessageID = MIME_Utils.CreateMessageID();
            msg.Date = DateTime.Now;
            msg.From = new Mail_t_MailboxList();
            msg.From.Add(new Mail_t_Mailbox(message.DisplayName, message.From));
            msg.To = new Mail_t_AddressList();
            msg.To.Add(new Mail_t_Mailbox(message.To, message.To));
            msg.Subject = message.Subject;
           // msg.Priority = message.Priority;
            CreateBodyPart(msg, message.Body);
            CreateAttachmentPart(msg, message.Attachments);
            return msg;
        }
        #endregion

        private static MemoryStream MessageToStream(Mail_Message msg)
        {
            MemoryStream m = new MemoryStream();
            MIME_Encoding_EncodedWord ew = new MIME_Encoding_EncodedWord(MIME_EncodedWordEncoding.B, Encoding.UTF8);
            msg.ToStream(m, ew, Encoding.UTF8, false);
            m.Position = 0;
            return m;
        }

        private static void SendMessage(BaseMailAccount Sender, BaseMailAccount Receiver, MemoryStream m)
        {
            LumiSoft.Net.SMTP.Client.SMTP_Client smtpClient;

            smtpClient = new LumiSoft.Net.SMTP.Client.SMTP_Client();

            try
            {
                smtpClient.Connect(Sender.SendHost, Sender.SendPort, Sender.SendSsl);
                try
                {
                    smtpClient.EhloHelo(Sender.SendHost);
                    smtpClient.Auth(new LumiSoft.Net.AUTH.AUTH_SASL_Client_Login(Sender.UserName, Sender.Password)); 
                   // smtpClient.Authenticate(Sender.UserName, Sender.Password);
                    smtpClient.MailFrom(Sender.UserName, -1);
                    smtpClient.RcptTo(Receiver.UserName);
                    smtpClient.SendMessage(m);
                }
                finally
                {
                    smtpClient.Disconnect();
                }
            }
            finally
            {
                smtpClient = null;
            }
        }

        public bool CanSend(BaseMailAccount Sender)
        {
            return true;
        }

        public int SendTest(BaseMailAccount Sender, BaseMailAccount Receiver)
        {
            Mail_Message msg = CreateTestMail(Sender, Receiver);
            MemoryStream m = MessageToStream(msg);
            try
            {
                SendMessage(Sender, Receiver, m);
                return 0;
            }
            catch (Exception ex)
            {
                this.EncodeErrorMessage(ex);
                return -1;
                throw ex;
            }
        }

        public int SendMail(BaseMailAccount Sender, BaseMailAccount Receiver, BaseMailMessage message)
        {
            Mail_Message msg = TransferMessage(message);
            MemoryStream m = MessageToStream(msg);
            try
            {
                SendMessage(Sender, Receiver, m);
                return 0;
            }
            catch (Exception ex)
            {
                this.EncodeErrorMessage(ex);
                return -1;
                throw ex;
            }
        }

        public string GetLastErrorMsg()
        {
            return ErrorMessage;
        }

    }
}
