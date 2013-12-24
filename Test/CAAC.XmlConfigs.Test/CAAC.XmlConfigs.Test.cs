using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UniCloud.Fleet.XmlConfigs;

namespace CAAC.XmlConfigs.Test
{
    [TestClass]
    public class XmlConfigsTest
    {
        private string conn = "Server=192.168.18.30;Database=CAFM;Persist Security Info=True;User ID=UniCloud;Password=fleet@XMZZ";

        private bool UpdateXmlConfigContent()
        {
            try
            {
                XmlConfigService _XmlService = new XmlConfigService(conn);
                _XmlService.UpdateAllXmlConfigContent();
                return true;
            }
            catch(Exception  ex)
            {
                throw ex;
                return false;
            }
        }

        private bool UpdateXmlConfigFlag()
        {
            try
            {
                XmlConfigService _XmlService = new XmlConfigService(conn);
                _XmlService.UpdateAllXmlConfigFlag();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
                return false;
            }
        }


        [TestMethod]
        public void TestUpdateXmlConfigContent()
        {
            Assert.AreEqual(UpdateXmlConfigContent(), true);
        }

        [TestMethod]
        public void TestUpdateXmlConfigFlag()
        {
            Assert.AreEqual(UpdateXmlConfigFlag(), true);
        }

    }
}
