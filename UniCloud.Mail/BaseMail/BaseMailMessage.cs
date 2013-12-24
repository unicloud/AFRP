using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Mail;

namespace UniCloud.Mail
{
    public class BaseAttachment
    {
        public Stream AttachStream { get; set; }
        public string FileName { get; set; }
    }

    public class BaseMailMessage
    {
        public string DisplayName { get; set; }
        public string Sender { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public MailPriority Priority;
        public List<BaseAttachment> Attachments { get; set; }

        public BaseMailMessage()
        {
            Attachments = new List<BaseAttachment>();
        }
    }
}
