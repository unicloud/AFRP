using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniCloud.Mail
{

    public class MailAccount
    {
        public string MailAddress { get; set; }
        public string SmtpHost { get; set; }
        public string Pop3Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public int SendPort { get; set; }
        public int ReceivePort { get; set; }

        public bool SendSsl { get; set; }
        public bool ReceiveSsl { get; set; }

        public MailAccount()
        {

        }
    }
}
