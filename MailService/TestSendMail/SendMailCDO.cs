using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace UniCloud.Mail
{
    class SendMailCDO
    {
        public int SendMail(MailAccount Sender, MailAccount Receiver, string mailSubject)
        {
            //获得要处理的邮件信
            try
            {

                string mailUser = Sender.UserName;
                string mailPass = Sender.Password;
                string mailFrom = Sender.UserName;
                string smtpserver = Sender.SmtpHost;

                CDO.Message message = new CDO.Message();

                message.To = Receiver.UserName;
                message.TextBody = "Test Send";
                //邮件标题
                message.Subject = "Test";
                message.From = Sender.UserName;
                message.Sender = Sender.UserName;

                CDO.IConfiguration iConfig = message.Configuration;

                ADODB.Fields fields = iConfig.Fields;

                fields["http://schemas.microsoft.com/cdo/configuration/sendusing"].Value = 2;

                fields["http://schemas.microsoft.com/cdo/configuration/sendemailaddress"].Value = mailFrom;

                fields["http://schemas.microsoft.com/cdo/configuration/sendusername"].Value = mailFrom;

                fields["http://schemas.microsoft.com/cdo/configuration/sendpassword"].Value = mailPass;

                fields["http://schemas.microsoft.com/cdo/configuration/smtpauthenticate"].Value = 1;

                fields["http://schemas.microsoft.com/cdo/configuration/smtpserver"].Value = smtpserver;

                fields.Update();

                message.Send();

                message = null;

                return 0;

            }
            catch (Exception ex)
            {
                return -1;
            }

        }

    }
}
