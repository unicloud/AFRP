using System.Linq;
using UniCloud.Cryptography;
namespace UniCloud.Fleet.Models
{
    public static class DB
    {
        public const string Schema = "Fleet";
    }

    public static class Conn
    {
        public const string LocalAfrp = "AFRP_Local";
        public const string TestAfrp = "AFRP_Test";
        public const string ProductAfrp = "AFRP_Product";
        public const string LocalCafm = "CAFM_Local";
        public const string TestCafm = "CAFM_Test";
        public const string ProductCafm = "CAFM_Product";
        public const string Afrp = LocalAfrp;
        public const string Cafm = LocalCafm;
        public const string Default = "Initial";
    }

    public static class ConnectionStringCryptography
    {
        public static string DecryptConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) return null;
            //分解连接字符串
            var splitConnection = connectionString.Split(';');
            //获取含加密密码字符串
            var encryptedPwd = splitConnection.FirstOrDefault(p => p.StartsWith("Password="));
            //解密后密码
            var decryptPwd = DESCryptography.DecryptString(encryptedPwd.Substring(encryptedPwd.LastIndexOf('=') + 1));
            //含解密后密码的字符串
            var cryptographyPwd = "Password=" + decryptPwd;
            var connection = splitConnection.Where(p => !p.Contains("Password="));
            //重新组合解密后的连接字符串
            string decryptConnectionString = null;
            if (connection.Count() != 0)
            {
                foreach (var item in connection)
                {
                    decryptConnectionString += item + ";";
                }
            }
            decryptConnectionString += cryptographyPwd;
            return decryptConnectionString;
        }
    }

}
