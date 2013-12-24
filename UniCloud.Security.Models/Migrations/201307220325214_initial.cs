namespace UniCloud.Security.Models.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Security.Applications",
                c => new
                    {
                        ApplicationId = c.Int(nullable: false, identity: true),
                        ApplicationName = c.String(maxLength: 256),
                        LoweredApplicationName = c.String(maxLength: 256),
                        ModuleName = c.String(maxLength: 30),
                        ViewNameSpace = c.String(maxLength: 30),
                        Description = c.String(maxLength: 256),
                    })
                .PrimaryKey(t => t.ApplicationId);
            
            CreateTable(
                "Security.Users",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        ApplicationId = c.Int(nullable: false),
                        UserName = c.String(maxLength: 256),
                        LoweredUserName = c.String(maxLength: 256),
                        Password = c.String(maxLength: 128),
                        PasswordFormat = c.Int(nullable: false),
                        PasswordSalt = c.String(maxLength: 128),
                        MobileAlias = c.String(maxLength: 16),
                        Email = c.String(maxLength: 256),
                        LoweredEmail = c.String(maxLength: 256),
                        PasswordQuestion = c.String(maxLength: 256),
                        PasswordAnswer = c.String(maxLength: 128),
                        IsApproved = c.Boolean(nullable: false),
                        IsLockedOut = c.Boolean(nullable: false),
                        IsAnonymous = c.Boolean(nullable: false),
                        CreateDate = c.DateTime(nullable: false, storeType: "datetime2"),
                        LastLoginDate = c.DateTime(nullable: false, storeType: "datetime2"),
                        LastPasswordChangedDate = c.DateTime(nullable: false, storeType: "datetime2"),
                        LastLockoutDate = c.DateTime(nullable: false, storeType: "datetime2"),
                        LastActivityDate = c.DateTime(nullable: false, storeType: "datetime2"),
                        FailedPasswordAttemptCount = c.Int(nullable: false),
                        FailedPasswordAttemptWindowStart = c.DateTime(nullable: false, storeType: "datetime2"),
                        FailedPasswordAnswerAttemptCount = c.Int(nullable: false),
                        FailedPasswordAnswerAttemptWindowStart = c.DateTime(nullable: false, storeType: "datetime2"),
                        Comment = c.String(),
                    })
                .PrimaryKey(t => t.UserId)
                .ForeignKey("Security.Applications", t => t.ApplicationId)
                .Index(t => t.ApplicationId);
            
            CreateTable(
                "Security.Profiles",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        IsAnonymous = c.Int(nullable: false),
                        LastActivityDate = c.DateTime(nullable: false, storeType: "datetime2"),
                        LastUpdatedDate = c.DateTime(nullable: false, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.UserId)
                .ForeignKey("Security.Users", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "Security.ProfileValues",
                c => new
                    {
                        ProfileValuesID = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        PropertyValue = c.String(maxLength: 256),
                    })
                .PrimaryKey(t => t.ProfileValuesID)
                .ForeignKey("Security.Profiles", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "Security.UserInRole",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                        IsValid = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("Security.Users", t => t.UserId, cascadeDelete: true)
                .ForeignKey("Security.Roles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "Security.Roles",
                c => new
                    {
                        RoleId = c.Int(nullable: false, identity: true),
                        ApplicationId = c.Int(nullable: false),
                        RoleName = c.String(maxLength: 256),
                        LoweredRoleName = c.String(maxLength: 256),
                        Description = c.String(maxLength: 256),
                    })
                .PrimaryKey(t => t.RoleId)
                .ForeignKey("Security.Applications", t => t.ApplicationId)
                .Index(t => t.ApplicationId);
            
            CreateTable(
                "Security.FunctionsInRoles",
                c => new
                    {
                        FunctionItemID = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                        IsValid = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => new { t.FunctionItemID, t.RoleId })
                .ForeignKey("Security.Roles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("Security.FunctionItem", t => t.FunctionItemID, cascadeDelete: true)
                .Index(t => t.RoleId)
                .Index(t => t.FunctionItemID);
            
            CreateTable(
                "Security.FunctionItem",
                c => new
                    {
                        FunctionItemID = c.Int(nullable: false, identity: true),
                        ApplicationId = c.Int(nullable: false),
                        ParentItemID = c.Int(),
                        IsLeaf = c.Boolean(nullable: false),
                        IsButton = c.Boolean(nullable: false),
                        LevelCode = c.String(maxLength: 30),
                        Name = c.String(maxLength: 50),
                        ViewName = c.String(maxLength: 100),
                        Description = c.String(maxLength: 256),
                    })
                .PrimaryKey(t => t.FunctionItemID)
                .ForeignKey("Security.Applications", t => t.ApplicationId)
                .ForeignKey("Security.FunctionItem", t => t.ParentItemID)
                .Index(t => t.ApplicationId)
                .Index(t => t.ParentItemID);
            
            CreateTable(
                "Security.PasswordHistory",
                c => new
                    {
                        PasswordHistoryID = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        Password = c.String(maxLength: 128),
                        CreateDate = c.DateTime(nullable: false, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.PasswordHistoryID)
                .ForeignKey("Security.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropIndex("Security.PasswordHistory", new[] { "UserId" });
            DropIndex("Security.FunctionItem", new[] { "ParentItemID" });
            DropIndex("Security.FunctionItem", new[] { "ApplicationId" });
            DropIndex("Security.FunctionsInRoles", new[] { "FunctionItemID" });
            DropIndex("Security.FunctionsInRoles", new[] { "RoleId" });
            DropIndex("Security.Roles", new[] { "ApplicationId" });
            DropIndex("Security.UserInRole", new[] { "RoleId" });
            DropIndex("Security.UserInRole", new[] { "UserId" });
            DropIndex("Security.ProfileValues", new[] { "UserId" });
            DropIndex("Security.Profiles", new[] { "UserId" });
            DropIndex("Security.Users", new[] { "ApplicationId" });
            DropForeignKey("Security.PasswordHistory", "UserId", "Security.Users");
            DropForeignKey("Security.FunctionItem", "ParentItemID", "Security.FunctionItem");
            DropForeignKey("Security.FunctionItem", "ApplicationId", "Security.Applications");
            DropForeignKey("Security.FunctionsInRoles", "FunctionItemID", "Security.FunctionItem");
            DropForeignKey("Security.FunctionsInRoles", "RoleId", "Security.Roles");
            DropForeignKey("Security.Roles", "ApplicationId", "Security.Applications");
            DropForeignKey("Security.UserInRole", "RoleId", "Security.Roles");
            DropForeignKey("Security.UserInRole", "UserId", "Security.Users");
            DropForeignKey("Security.ProfileValues", "UserId", "Security.Profiles");
            DropForeignKey("Security.Profiles", "UserId", "Security.Users");
            DropForeignKey("Security.Users", "ApplicationId", "Security.Applications");
            DropTable("Security.PasswordHistory");
            DropTable("Security.FunctionItem");
            DropTable("Security.FunctionsInRoles");
            DropTable("Security.Roles");
            DropTable("Security.UserInRole");
            DropTable("Security.ProfileValues");
            DropTable("Security.Profiles");
            DropTable("Security.Users");
            DropTable("Security.Applications");
        }
    }
}
