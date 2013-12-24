using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LumiSoft.Net;
using LumiSoft.Net.POP3;
using LumiSoft.Net.POP3.Client;
using LumiSoft.Net.Mime;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UniCloud.Fleet.Models;
using LumiSoft.Net.Mail;
using LumiSoft.Net.MIME;

namespace UniCloud.Mail
{
    public class ReceiverMail
    {
        private POP3_Client _Pop3Client;
        private BaseMailAccount _Account;
        private ManageMail _InvalidMails;
        private List<POP3_ClientMessage> _ValidMails;
       // private FleetEntities _FE;
        private List<ReceiverObject> _ReceiverObjects;

        public ReceiverMail(Guid Receiver)
        {
            _Pop3Client = new POP3_Client();
            _Account = MailAccountHelper.GetMailAccount(Receiver);
            _InvalidMails = new ManageMail();
           // _FE = new FleetEntities();
            _ReceiverObjects = new List<ReceiverObject>();
        }

        public List<ReceiverObject> ReceiverObjects { get { return _ReceiverObjects; } }
        private bool IsReceived(string MessageId)
        {
            return _InvalidMails.Contains(MessageId);
        }

        private bool AddInValidMailID(string MessageId)
        {
            return _InvalidMails.Add(MessageId);
        }

        public void ClearReceiveMails()
        {
            try
            {
                foreach (var ro in _ReceiverObjects)
                {
                    if (ro.Saved && ro.Message!= null)
                    {
                        ro.Message.MarkForDeletion();
                    }
                }
            }
            catch(Exception ex)
            {
               // UniCloud.Log.WindowsLog.Write(ex.Message);
            }
            try
            {
                _Pop3Client.Disconnect();
            }
            catch
            {
            }
            try
            {
                this._ReceiverObjects.Clear();
            }
            catch
            {
            }
        }

        private bool IsValidAttachment(string FileName)
        {
            return FileName != null &&( FileName.Contains("Request") ||
                FileName.Contains("Plan") ||
                FileName.Contains("ApprovalDoc")||
                FileName.Contains("OperationHistory")||
                FileName.Contains("AircraftBusiness")||
                FileName.Contains("OwnershipHistory"));
        }

        private object DeSerialObj(byte[] data)
        {
            try
            {
                //解密
                byte[] _DeData = UniCloud.Cryptography.DESCryptography.DecryptByte(data);

                MemoryStream ms = new MemoryStream(_DeData, 0, _DeData.Length);
                IFormatter formatter = new BinaryFormatter();

                object obj = formatter.Deserialize(ms);
                return obj;
            }
            catch(Exception ex)
            {
                //UniCloud.Log.WindowsLog.Write(ex.Message);
                return null;
            }
        }

        private bool DecodeMessageAttachment(POP3_ClientMessage message)
        {
            bool _IsValid = false;
            ReceiverObject ro = new ReceiverObject();
            //获取这封邮件的内容
            byte[] bytes = message.MessageToByte();
            //解析从Pop3服务器发送过来的邮件附件
            Mime m = Mime.Parse(bytes);

            //二个月前的不接收了
            if (m.MainEntity.Date < DateTime.Now.AddDays(-60))
            {
                return false;
            }
            //遍历所有附件
            foreach (MimeEntity me in m.Attachments)
            {
                //附件有效
                if (!IsValidAttachment(me.ContentType_Name))
                {
                    continue;
                }
                //解压邮件
                object obj = DeSerialObj(me.Data);
                if (obj != null)
                {
                    ro.Objects.Add(obj);
                    _IsValid = true;
                }
            }
            if (_IsValid)
            {
                this._ReceiverObjects.Add(ro);
                ro.Message = message;
            }
            return _IsValid;
        }

        public bool Receive()
        {
            //与Pop3服务器建立连接
            _Pop3Client.Connect(_Account.ReceiveHost, _Account.ReceivePort);
            //验证身份
            _Pop3Client.Authenticate(_Account.UserName, _Account.Password, true);
            //获取邮件信息列表
            POP3_ClientMessageCollection infos = _Pop3Client.Messages;

            //POP3_ClientMessage info;
            //for (int i = infos.Count-1; i>=0; i--)
           foreach (POP3_ClientMessage info in infos)
            {
               // info = infos[i];
                //每封Email会有一个在Pop3服务器范围内唯一的Id,检查这个Id是否存在就可以知道以前有没有接收过这封邮件
                if(!this.IsReceived(info.UID))
                {
                    if (DecodeMessageAttachment(info))
                    {
                        if (_ReceiverObjects.Count() >= 20)
                        {
                            break;
                        }
                    }
                    else
                    {
                        this.AddInValidMailID(info.UID);
                    }
                }
            }
            return this._ReceiverObjects.Count() > 0;
        }
    }
}
