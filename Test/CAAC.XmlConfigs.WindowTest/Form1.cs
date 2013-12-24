using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UniCloud.Fleet.XmlConfigs;

namespace CAAC.XmlConfigs.WindowTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void UpdateXmlConfig()
        {
            string conn = "Server=.;Database=CAFM;Persist Security Info=True;User ID=UniCloud;Password=26344DB3960DD7C73A93F6EDFA95BEC0";
            XmlConfigService _XmlService = new XmlConfigService();
            _XmlService.UpdateAllXmlConfigContent();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DateTime dtBegin = DateTime.Now;
            UpdateXmlConfig();
            DateTime dtEnd = DateTime.Now;
            TimeSpan dt = dtEnd - dtBegin;
            this.label1.Text = dt.ToString();
        }

        protected string GetPercent(int Num, int Amount)
        {
            return Num == 0 || Amount == 0 ? "0%" : (Num * 100.0 / Amount).ToString("##0.##") + "%";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show(GetPercent(5000, 5000));
        }
    }
}
