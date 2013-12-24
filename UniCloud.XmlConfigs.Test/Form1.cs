using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UniCloud.Fleet.XmlConfigs;

namespace TestMailService
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private static void UpdateXmlConfigFlag()
        {
            XmlConfigService _XmlService = new XmlConfigService();
            _XmlService.UpdateAllXmlConfigFlag();
        }


        private static void UpdateXmlConfigContent()
        {
            XmlConfigService _XmlService = new XmlConfigService(true);
            _XmlService.UpdateAllXmlConfigContent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
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

        private void button1_Click_1(object sender, EventArgs e)
        {
            UniCloud.DatabaseHelper.Databasehelper dbHelper = new UniCloud.DatabaseHelper.Databasehelper();
            dbHelper.BackupDataBase("AFRP", "D:\\Backup", "Backup" + DateTime.Now.ToString("yyyyMMdd") + ".bak");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            UniCloud.DatabaseHelper.Databasehelper dbHelper = new UniCloud.DatabaseHelper.Databasehelper();
            dbHelper.RestoreDataBase("AFRP", "D:\\Backup", "Backup" + DateTime.Now.ToString("yyyyMMdd") + ".bak");
        }

      
    }
}
