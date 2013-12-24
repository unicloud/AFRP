
using UniCloud.Cryptography;
using System.Linq;
namespace UniCloud.Security.Models
{
    public static class DB
    {
        public const string Schema = "Security";
    }

    public static class Conn
    {
        public const string LocalSec = "Sec_Local";
        public const string TestSec = "Sec_Test";
        public const string ProductSec = "Sec_Product";
        public const string Default = LocalSec;
    }

    public static class ConnectionStringCryptography
    {
        public static string DecryptConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) return null;
            //分解连接字符串
            var splitConnection = connectionString.Split(';');
            //获取含加密密码字符串
            var encryptedPwd = splitConnection.FirstOrDefault(p => p.Contains("Password="));
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