
using UniCloud.Cryptography;
using System.Linq;
namespace UniCloud.Fleet.Models
{
    public static class DB
    {
        public const string Schema = "Fleet";
    }
    public static class Conn
    {
        public const string LocalCafm = "CAFM_Local";
        public const string TestCafm = "CAFM_Test";
        public const string ProductCafm = "CAFM_Product";
        public const string Default = LocalCafm;
    }



    public static class  GlobalConst
    {
        public const string CAACID = "31A9DE51-C207-4A73-919C-21521F17FEF9";
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
