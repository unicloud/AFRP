using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UniCloud.Mail;
using UniCloud.Fleet.XmlConfigs;

namespace TestMailService
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            timer1.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                ReceiveEmail();
                ShowMessage("Receive Email OK");
            }
            catch(Exception ex)
            {
                ShowMessage("Receive Email Error: " + ex.Message);
            };
        }

        private static void UpdateXmlConfigFlag()
        {
            XmlConfigService _XmlService = new XmlConfigService();
            _XmlService.UpdateAllXmlConfigFlag();
        }


        private static void UpdateXmlConfigContent()
        {
            XmlConfigService _XmlService = new XmlConfigService(false);
            _XmlService.UpdateAllXmlConfigContent();
        }

        private static void ReceiveEmail()
        {
            Guid Receiver = Guid.Parse("31A9DE51-C207-4A73-919C-21521F17FEF9");
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            try
            {
                //UpdateXmlConfigContent();
            }
            catch
            {
            };
            timer1.Enabled = true;
        }

        private void UpdateXml_Click(object sender, EventArgs e)
        {
            DateTime dtBegin = DateTime.Now;
            UpdateXmlConfigContent();
            DateTime dtEnd = DateTime.Now;
            TimeSpan dt = dtEnd - dtBegin;
            this.label1.Text = dt.ToString();
        }

        private void btnUpdateXmlFlag_Click(object sender, EventArgs e)
        {
            UpdateXmlConfigFlag();
        }

        private void btnSaveObject_Click(object sender, EventArgs e)
        {
       //     TestSaveAircraftBusiness();
            TestSaveOperationHistory();
       //     TestSaveOwnershipHistory();
            ShowMessage("Test Save Object OK");
        }

        private void TestSaveAircraftBusiness()
        {
            string conn = "Server=.;Database=AFRP;Persist Security Info=True;User ID=UniCloud;Password=uni!cloud";

            TestData td = new TestData(conn);
            Guid gd = td.GetLastAircraftBusiness();

            EncodeModel _EModel = new EncodeModel(conn);
            var obj = _EModel.EncodeAircraftBusiness(gd);

            List<ReceiverObject> objList = new List<ReceiverObject>();
            ReceiverObject ro = new ReceiverObject();
            ro.Objects.Add(obj);
            objList.Add(ro);
            DecodeModel _Model = new DecodeModel();
            if (_Model.SaveObjects(objList))
            {
                if (_Model.DataChanged)
                {
               //     UpdateXmlConfigFlag();
                }
            }
        }


        private void TestSaveOperationHistory()
        {
            string conn = "Server=.;Database=AFRP;Persist Security Info=True;User ID=UniCloud;Password=uni!cloud";

            TestData td = new TestData(conn);
            Guid gd = td.GetLastOperationHistory();

            EncodeModel _EModel = new EncodeModel(conn);
            var obj = _EModel.EncodeOperationHistory(gd);

            List<ReceiverObject> objList = new List<ReceiverObject>();
            ReceiverObject ro = new ReceiverObject();
            ro.Objects.Add(obj);
            objList.Add(ro);
            DecodeModel _Model = new DecodeModel();
            if (_Model.SaveObjects(objList))
            {
                if (_Model.DataChanged)
                {
               //     UpdateXmlConfigFlag();
                }
            }
        }


        private void TestSaveOwnershipHistory()
        {
            string conn = "Server=.;Database=AFRP;Persist Security Info=True;User ID=UniCloud;Password=uni!cloud";

            TestData td = new TestData(conn);
            Guid gd = td.GetLastOwnershipHistory();

            EncodeModel _EModel = new EncodeModel(conn);
            var obj = _EModel.EncodeOwnershipHistory(gd);

            List<ReceiverObject> objList = new List<ReceiverObject>();
            ReceiverObject ro = new ReceiverObject();
            ro.Objects.Add(obj);
            objList.Add(ro);
            DecodeModel _Model = new DecodeModel();
            if (_Model.SaveObjects(objList))
            {
                if (_Model.DataChanged)
                {
               //     UpdateXmlConfigFlag();
                }
            }
        }

        private static int SendEmail()
        {
            Guid Sender = Guid.Parse("49A84FC4-0718-4C17-89F9-E7CCA762B1F2"); // ("004b37c1-70b8-4071-98ab-0bb73c466d00");
            Guid Receiver = Guid.Parse("31A9DE51-C207-4A73-919C-21521F17FEF9");
            SendMail _SendMail = new SendMail();

            string conn = "Server=.;Database=AFRP;Persist Security Info=True;User ID=UniCloud;Password=fleet@XMZZ";

            TestData td = new TestData(conn);
            Guid gd = td.GetLastAircraftPlan();

            EncodeModel _EModel = new EncodeModel(conn);
            var obj = _EModel.EncodePlan(gd);

            if (obj != null)
            {
                return _SendMail.SendEntity(conn, Sender, Receiver, obj, "Plan", "Plan");
            }
            else
                return -1;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (SendEmail()==0)
                {
                ShowMessage("Send Email OK");
                }
                else
                {
                    ShowMessage("Send Email Error");
                };
            }
            catch
            {
                ShowMessage("Send Email Error");
            };
        }

        private void ShowMessage(string msg)
        {
            MessageBox.Show(msg);
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            byte[] testData = { 1, 2, 3 };
            byte[] _temp;
            byte[] DesData;
           _temp = UniCloud.Cryptography.DESCryptography.EncryptByte(testData);
           DesData = UniCloud.Cryptography.DESCryptography.DecryptByte(_temp);
           if (!DesData.SequenceEqual(testData))
           {
               ShowMessage("error");
           }
           else
           {
               ShowMessage("DESCryptography OK");
           }
        }

        private void button1_Click_3(object sender, EventArgs e)
        {
            string testData = "Soft123";
            string  _temp;
            string DesData;
            
            _temp = UniCloud.Cryptography.DESCryptography.EncryptString(testData);
            DesData = UniCloud.Cryptography.DESCryptography.DecryptString(_temp);
            if (DesData != testData)
            {
                ShowMessage("error");
            }
            else
            {
                ShowMessage("DESCryptography OK");
            }
        }
      
    }
}
