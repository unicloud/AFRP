using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using UniCloud.Fleet.XmlConfigs;
using UniCloud.Mail;
using UniCloud.Log;

namespace CAAC.Fleet.MailService
{
    partial class MailService : ServiceBase
    {

        private Thread ReceiveMailThread;
        private bool ReceiveMailFlag = true;
 
        public MailService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // TODO: 在此处添加代码以启动服务。
            StartReceiveMail();
        }

        private void WriteLog(string Content)
        {
            WindowsLog.Write(Content);
        }

        protected override void OnStop()
        {
            // TODO: 在此处添加代码以执行停止服务所需的关闭操作。
            try
            {
                ReceiveMailThread.Abort();

                ReceiveMailFlag = false;

                WriteLog("邮件接收线程停止");
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }

        private void StartReceiveMail()
        {
            try
            {
                ReceiveMailThread = new Thread(new ThreadStart(ReceiveEmail));
                ReceiveMailThread.Start();
                WriteLog("邮件线程任务开始");
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
                WriteLog("线程任务开始失败");
            }
        }

        private static void UpdateXmlConfigFlag()
        {
            XmlConfigService _XmlService = new XmlConfigService();
            _XmlService.UpdateAllXmlConfigFlag();
        }


        private void ReceiveEmail()
        {
            Guid Receiver = Guid.Parse("31A9DE51-C207-4A73-919C-21521F17FEF9");
            while (ReceiveMailFlag)
            {
                try
                {
                    ReceiverMail _ReceiveMail = new ReceiverMail(Receiver);
                    DecodeModel _Model = new DecodeModel();
                    if (_ReceiveMail.Receive())
                    {
                        if (_Model.SaveObjects(_ReceiveMail.ReceiverObjects))
                        {
                            _ReceiveMail.ClearReceiveMails();
                        }
                        if (_Model.DataChanged)
                        {
                            UpdateXmlConfigFlag();
                        }
                    }
                }
                catch (Exception e)
                {
                    WriteLog(e.Message);
                }
                Thread.Sleep(300000);
             }
        }

    }
}
