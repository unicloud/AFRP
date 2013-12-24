namespace UniCloud.Fleet.Models.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class database : DbMigration
    {
        public override void Up()
        {
            AddColumn("Fleet.MailAddress", "Address", c => c.String(maxLength: 100));
            AddColumn("Fleet.MailAddress", "DisplayName", c => c.String(maxLength: 100));
            AddColumn("Fleet.MailAddress", "SendSSL", c => c.Boolean(nullable: false));
            AddColumn("Fleet.MailAddress", "StartTLS", c => c.Boolean(nullable: false));
            AddColumn("Fleet.MailAddress", "ReceiveSSL", c => c.Boolean(nullable: false));
            AddColumn("Fleet.MailAddress", "ServerType", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("Fleet.MailAddress", "ServerType");
            DropColumn("Fleet.MailAddress", "ReceiveSSL");
            DropColumn("Fleet.MailAddress", "StartTLS");
            DropColumn("Fleet.MailAddress", "SendSSL");
            DropColumn("Fleet.MailAddress", "DisplayName");
            DropColumn("Fleet.MailAddress", "Address");
        }
    }
}
