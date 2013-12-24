using System.Data.Entity;

namespace UniCloud.Mail.DecodeClass
{
    interface IDecodeObject
    {
        void Decode(object obj);
        bool CanDecode(string objTypeName);
        void SetDbContext(DbContext context);
    }
}
