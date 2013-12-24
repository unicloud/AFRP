using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using UniCloud.Cryptography;

namespace UniCloud.Security.Models
{
    public class ConfigureAppConfig
    {

        public static string GetSectionInfo(string sectionName)
        {
            string Config = System.Configuration.ConfigurationManager.ConnectionStrings[sectionName].ToString();
            //获取这个config中password对应的字符串
            string a = "fleet@XMZZ";
            string b = DESCryptography.EncryptString(a);
            string connection = Config.Replace(a, b);
            return connection;
        }
    }
}
