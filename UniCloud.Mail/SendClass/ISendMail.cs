
namespace UniCloud.Mail
{
    public interface ISendMail
    {
        int SendTest(BaseMailAccount Sender, BaseMailAccount Receiver);
        int SendMail(BaseMailAccount Sender, BaseMailAccount Receiver, BaseMailMessage message);
        bool CanSend(BaseMailAccount Sender);
        string GetLastErrorMsg();
    }
}
