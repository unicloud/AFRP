using System;
using System.Linq;
using UniCloud.Fleet.Models;

namespace UniCloud.Mail
{

    public class MailAccountHelper
    {

        public static BaseMailAccount GetMailAccount(Guid account)
        {
          return InitAccountSetting(account);
        }

        public static BaseMailAccount GetMailAccount(Guid account, string conn)
        {
            return InitAccountSetting(account, conn);
        }

        public static BaseMailAccount GetMailAccount(Guid account, FleetEntities fe)
        {
            return InitAccountSetting(account, fe);
        }

        public static BaseMailAccount GetMailAccount(MailAddress addr)
        {
            return InitAccountSetting(addr);
        }

        private static BaseMailAccount SetPropertyFromAddr(MailAddress addr)
        {

            var account = new BaseMailAccount
                {
                    MailAddress = addr.Address,
                    DisplayName = addr.DisplayName,
                    UserName = addr.LoginUser,
                    Password = addr.OriginPassword,
                    ReceiveHost = addr.Pop3Host,
                    ReceivePort = addr.ReceivePort,
                    ReceiveSsl = addr.ReceiveSSL,
                    SendHost = addr.SmtpHost,
                    SendPort = addr.SendPort,
                    SendSsl = addr.SendSSL,
                    SendStartTLS = addr.StartTLS
                };

            return account;
        }

        private static BaseMailAccount SetDefaultProperty()
        {
            var account = new BaseMailAccount
                {
                    MailAddress = "",
                    DisplayName = "",
                    UserName = "",
                    Password = "",
                    ReceiveHost = "",
                    ReceivePort = 110,
                    ReceiveSsl = false,
                    SendHost = "",
                    SendPort = 25,
                    SendSsl = false,
                    SendStartTLS = false
                };

            return account;
        }

        private static BaseMailAccount InitAccountSetting(Guid account)
        {
            var fe = new FleetEntities();
            return InitAccountSetting(account, fe);
        }

        private static BaseMailAccount InitAccountSetting(Guid account, string conn)
        {
            var fe = new FleetEntities(conn);
            return InitAccountSetting(account, fe);
        }

        private static BaseMailAccount InitAccountSetting(Guid account, FleetEntities fe)
        {
            if (fe != null && fe.MailAddresses.Any(p => p.OwnerID == account))
            {
                var addr = fe.MailAddresses.FirstOrDefault(p => p.OwnerID == account);
                return InitAccountSetting(addr);  
            }
            return InitAccountSetting(null);
        }

        private static BaseMailAccount InitAccountSetting(MailAddress addr)
        {
            return addr != null ? SetPropertyFromAddr(addr) : SetDefaultProperty();
        }
    }
}
