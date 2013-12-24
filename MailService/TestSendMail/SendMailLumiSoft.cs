using System;
using System.Text;
using System.IO;
using LumiSoft.Net.Mail;
using LumiSoft.Net.MIME;

namespace UniCloud.Mail
{


    public class SendMailLumiSoft
    {

       
        public string ErrorMessage { get; set; }


        public SendMailLumiSoft()
        {
         
        }


        private Mail_Message Create_PlainText_Html_Attachment_Image(string mailTo, string mailFrom, string mailFromDisplay)
        {
            Mail_Message msg = new Mail_Message();
            msg.MimeVersion = "1.0";
            msg.MessageID = MIME_Utils.CreateMessageID();
            msg.Date = DateTime.Now;
            msg.From = new Mail_t_MailboxList();
            msg.From.Add(new Mail_t_Mailbox(mailFromDisplay, mailFrom));
            msg.To = new Mail_t_AddressList();
            msg.To.Add(new Mail_t_Mailbox(mailTo, mailTo));
            msg.Subject = "Test Send Mail";
            //设置回执通知
            //string notifyEmail = SystemConfig.Default.DispositionNotificationTo;
            //if (!string.IsNullOrEmpty(notifyEmail) && ValidateUtil.IsEmail(notifyEmail))
            //{
            //    msg.DispositionNotificationTo = new Mail_t_Mailbox(notifyEmail, notifyEmail);
            //}

            #region MyRegion
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

            //普通文本邮件内容，如果对方的收件客户端不支持HTML，这是必需的
            string plainTextBody = "如果你邮件客户端不支持HTML格式，或者你切换到“普通文本”视图，将看到此内容";
            //if (!string.IsNullOrEmpty(SystemConfig.Default.PlaintTextTips))
            //{
            //    plainTextBody = SystemConfig.Default.PlaintTextTips;
            //}

            text_plain.SetText(MIME_TransferEncodings.QuotedPrintable, Encoding.UTF8, plainTextBody);
            multipartAlternative.BodyParts.Add(entity_text_plain);

            //--- text/html -----------------------------------------
            string htmlText = "<html>这是一份测试邮件，来自<font color=red><b>厦门至正测试程序</b></font></html>";
            MIME_Entity entity_text_html = new MIME_Entity();
            MIME_b_Text text_html = new MIME_b_Text(MIME_MediaTypes.Text.html);
            entity_text_html.Body = text_html;
            text_html.SetText(MIME_TransferEncodings.QuotedPrintable, Encoding.UTF8, htmlText);
            multipartAlternative.BodyParts.Add(entity_text_html);

            //--- application/octet-stream -------------------------
            //foreach (string attach in mailInfo.Attachments)
            //{
            //    multipartMixed.BodyParts.Add(Mail_Message.CreateAttachment(attach));
            //}

            //foreach (string imageFile in mailInfo.EmbedImages)
            //{
            //MIME_Entity entity_image = new MIME_Entity();
            //entity_image.ContentDisposition = new MIME_h_ContentDisposition(MIME_DispositionTypes.Inline);
            //string fileName = "D:\\test.txt"; // DirectoryUtil.GetFileName(imageFile, true);
            //entity_image.ContentID = (new Guid()).ToString().Replace('-', '.'); //BytesTools.BytesToHex(Encoding.Default.GetBytes(fileName));
            //MIME_b_Image body_image = new MIME_b_Image(MIME_MediaTypes.Image.jpeg);
            //entity_image.Body = body_image;
            //body_image.SetDataFromFile("D:\\test.txt", MIME_TransferEncodings.Base64);
            //multipartMixed.BodyParts.Add(entity_image);
            //}

            #endregion

            return msg;
        }

        //public MailMessage GenMail(string mailSubject)
        //{
        //    // 生成邮件
        //    MailMessage message = new MailMessage("Jackyzhang2001@163.com", "Jackyzhang2001@163.com");
        //    message.Sender = new MailAddress("Jackyzhang2001@163.com");
        //    message.Subject = mailSubject;
        //    message.Body = "Jackyzhang2001@163.com" + " Send " + mailSubject;
        //    message.Priority = MailPriority.High;
        //    return message;
        //}

        public int SendMail(MailAccount Sender, MailAccount Receiver, string mailSubject)
        {
            Mail_Message msg = Create_PlainText_Html_Attachment_Image(Receiver.UserName, Sender.MailAddress, Sender.UserName);
            MemoryStream m = new MemoryStream();
            MIME_Encoding_EncodedWord ew = new MIME_Encoding_EncodedWord(MIME_EncodedWordEncoding.B, Encoding.UTF8);
            msg.ToStream(m, ew, Encoding.UTF8, false);
            m.Position = 0;
           
            
            LumiSoft.Net.SMTP.Client.SMTP_Client smtpClient;

            smtpClient = new LumiSoft.Net.SMTP.Client.SMTP_Client();

            try
            {
                smtpClient.Connect(Sender.SmtpHost, Sender.SendPort, Sender.SendSsl);
                try
                {
                    smtpClient.EhloHelo(Sender.SmtpHost);
                    smtpClient.Authenticate(Sender.UserName, Sender.Password);
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
            return 0;

        }
    }
}
