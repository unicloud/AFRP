using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace UniCloud.Mail
{
    public static class ObjAttachHelper
    {
        public static Stream ModelObjToAttachmentStream(object obj)
        {
            if (obj != null)
            {
                var ms = new MemoryStream();
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;
                //加密
                UniCloud.Cryptography.DESCryptography.EncryptStream(ref ms);
                return ms;
            }
            return null;
        }
    }
}
