using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace UniCloud.Security.Models
{
    public partial class SecurityEntities : DbContext
    {
        public SecurityEntities()
            : base(ConnectionStringCryptography.DecryptConnectionString(System.Configuration.ConfigurationManager.ConnectionStrings[Conn.Default].ToString()))
        {
        }

        public SecurityEntities(string conn)
            : base(conn)
        {
            Database.SetInitializer<SecurityEntities>(null);
        }

        #region 映射到数据库的实体集

        public DbSet<Applications> Applications { get; set; }
        public DbSet<FunctionItem> FunctionItems { get; set; }
        public DbSet<FunctionsInRoles> FunctionsInRoles { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<PasswordHistory> PasswordHistories { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<UserInRole> UserInRoles { get; set; }
        public DbSet<Profiles> Profiles { get; set; }
        public DbSet<ProfileValues> ProfileValues { get; set; }

        #endregion

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // 移除EF的表名公约
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            //移除多对多级联删除公约
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            modelBuilder.Configurations
                .Add(new ApplicationsConfiguration())
                .Add(new FunctionItemConfiguration())
                .Add(new FunctionsInRolesConfiguration())
                .Add(new UsersConfiguration())
                .Add(new PasswordHistoryConfiguration())
                .Add(new RolesConfiguration())
                .Add(new UserInRoleConfiguration())
                .Add(new ProfilesConfiguration())
                .Add(new ProfileValuesConfiguration())
                .Add(new FriendlyNameConfiguration());

            #region Oracle 表、属性映射

            //// DbSet<Applications> Applications 
            //modelBuilder.Entity<Applications>().ToTable("APPLICATIONS", "SECURITY");
            //modelBuilder.Entity<Applications>().HasKey(k => k.ApplicationId);
            //modelBuilder.Entity<Applications>().Property(p => p.ApplicationId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            //modelBuilder.Entity<Applications>().Property(p => p.ApplicationId).HasColumnName("APPLICATION_ID").HasColumnType("int");
            //modelBuilder.Entity<Applications>().Property(p => p.ApplicationName).HasColumnName("APPLICATION_NAME");
            //modelBuilder.Entity<Applications>().Property(p => p.LoweredApplicationName).HasColumnName("LOWERED_APPLICATION_NAME");
            //modelBuilder.Entity<Applications>().Property(p => p.Description).HasColumnName("DESCRIPTION");

            //// DbSet<Users> Users 
            //modelBuilder.Entity<Users>().ToTable("USERS", "SECURITY");
            //modelBuilder.Entity<Users>().HasKey(k => k.UserId);
            //modelBuilder.Entity<Users>().Property(p => p.UserId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            //modelBuilder.Entity<Users>().Property(p => p.UserId).HasColumnName("USER_ID").HasColumnType("int");
            //modelBuilder.Entity<Users>().Property(p => p.ApplicationId).HasColumnName("APPLICATION_ID").HasColumnType("int");
            //modelBuilder.Entity<Users>().Property(p => p.UserName).HasColumnName("USER_NAME");
            //modelBuilder.Entity<Users>().Property(p => p.LoweredUserName).HasColumnName("LOWERED_USER_NAME");
            //modelBuilder.Entity<Users>().Property(p => p.Password).HasColumnName("PASSWORD");
            //modelBuilder.Entity<Users>().Property(p => p.PasswordFormat).HasColumnName("PASSWORD_FORMAT").HasColumnType("int");
            //modelBuilder.Entity<Users>().Property(p => p.PasswordSalt).HasColumnName("PASSWORD_SALT");
            //modelBuilder.Entity<Users>().Property(p => p.MobileAlias).HasColumnName("MOBILE_ALIAS");
            //modelBuilder.Entity<Users>().Property(p => p.Email).HasColumnName("EMAIL");
            //modelBuilder.Entity<Users>().Property(p => p.LoweredEmail).HasColumnName("LOWERED_EMAIL");
            //modelBuilder.Entity<Users>().Property(p => p.PasswordQuestion).HasColumnName("PASSWORD_QUESTION");
            //modelBuilder.Entity<Users>().Property(p => p.PasswordAnswer).HasColumnName("PASSWORD_ANSWER");
            //modelBuilder.Entity<Users>().Property(p => p.IsApproved).HasColumnName("IS_APPROVED").HasColumnType("int");
            //modelBuilder.Entity<Users>().Property(p => p.IsLockedOut).HasColumnName("IS_LOCKEDOUT").HasColumnType("int");
            //modelBuilder.Entity<Users>().Property(p => p.IsAnonymous).HasColumnName("IS_ANONYMOUS").HasColumnType("int");
            //modelBuilder.Entity<Users>().Property(p => p.CreateDate).HasColumnName("CREATE_DATE");
            //modelBuilder.Entity<Users>().Property(p => p.LastLoginDate).HasColumnName("LAST_LOGIN_DATE");
            //modelBuilder.Entity<Users>().Property(p => p.LastPasswordChangedDate).HasColumnName("LAST_PASSWORD_CHANGED_DATE");
            //modelBuilder.Entity<Users>().Property(p => p.LastLockoutDate).HasColumnName("LAST_LOCKOUT_DATE");
            //modelBuilder.Entity<Users>().Property(p => p.LastActivityDate).HasColumnName("LAST_ACTIVITY_DATE");
            //modelBuilder.Entity<Users>().Property(p => p.FailedPasswordAttemptCount).HasColumnName("FAILED_PSW_ATTEMPT_COUNT").HasColumnType("int");
            //modelBuilder.Entity<Users>().Property(p => p.FailedPasswordAttemptWindowStart).HasColumnName("FAILED_PSW_ATTEMPTWND_START");
            //modelBuilder.Entity<Users>().Property(p => p.FailedPasswordAnswerAttemptCount).HasColumnName("FAILED_PSW_ANSATTEMPT_COUNT").HasColumnType("int");
            //modelBuilder.Entity<Users>().Property(p => p.FailedPasswordAnswerAttemptWindowStart).HasColumnName("FAILED_PSW_ANSATTEMPTWND_START");
            //modelBuilder.Entity<Users>().Property(p => p.Comment).HasColumnName("USERS_COMMENT");

            //// DbSet<Roles> Roles 
            //modelBuilder.Entity<Roles>().ToTable("ROLES", "SECURITY");
            //modelBuilder.Entity<Roles>().HasKey(k => k.RoleId);
            //modelBuilder.Entity<Roles>().Property(p => p.RoleId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            //modelBuilder.Entity<Roles>().Property(p => p.RoleId).HasColumnName("ROLE_ID").HasColumnType("int");
            //modelBuilder.Entity<Roles>().Property(p => p.ApplicationId).HasColumnName("APPLICATION_ID").HasColumnType("int");
            //modelBuilder.Entity<Roles>().Property(p => p.RoleName).HasColumnName("ROLE_NAME");
            //modelBuilder.Entity<Roles>().Property(p => p.LoweredRoleName).HasColumnName("LOWERED_ROLE_NAME");
            //modelBuilder.Entity<Roles>().Property(p => p.Description).HasColumnName("DESCRIPTION");

            //// DbSet<UsersInRoles> UsersInRoles 
            //modelBuilder.Entity<UsersInRoles>().ToTable("USERS_IN_ROLES", "SECURITY");
            //modelBuilder.Entity<UsersInRoles>().HasKey(k => new { k.UserId, k.RoleId });

            //modelBuilder.Entity<UsersInRoles>().Property(p => p.UserId).HasColumnName("USER_ID").HasColumnType("int");
            //modelBuilder.Entity<UsersInRoles>().Property(p => p.RoleId).HasColumnName("ROLE_ID").HasColumnType("int");

            ////DbSet<Profiles> Profiles
            //modelBuilder.Entity<Profiles>().ToTable("PROFILES", "SECURITY");
            //modelBuilder.Entity<Profiles>().HasKey(k => k.UserId);
            //modelBuilder.Entity<Profiles>().Property(p => p.UserId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            //modelBuilder.Entity<Profiles>().Property(p => p.UserId).HasColumnName("USER_ID").HasColumnType("int");
            //modelBuilder.Entity<Profiles>().Property(p => p.IsAnonymous).HasColumnName("IS_ANONYMOUS").HasColumnType("int");
            //modelBuilder.Entity<Profiles>().Property(p => p.LastActivityDate).HasColumnName("LAST_ACTIVITY_DATE");
            //modelBuilder.Entity<Profiles>().Property(p => p.LastUpdatedDate).HasColumnName("LAST_UPDATED_DATE");

            ////DbSet<ProfileValues> ProfileValues
            //modelBuilder.Entity<ProfileValues>()
            //    .Map<FriendlyName>(m => m.Requires("PROPERTY_NAME").HasValue("FriendlyName".ToUpper()))
            //    .ToTable("PROFILE_VALUES", "SECURITY");
            //modelBuilder.Entity<ProfileValues>().HasKey(k => k.ProfileValuesID);
            //modelBuilder.Entity<ProfileValues>().Property(p => p.ProfileValuesID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            //modelBuilder.Entity<ProfileValues>().Property(p => p.ProfileValuesID).HasColumnName("PROFILE_VALUES_ID").HasColumnType("int");
            //modelBuilder.Entity<ProfileValues>().Property(p => p.UserId).HasColumnName("USER_ID").HasColumnType("int");
            //modelBuilder.Entity<ProfileValues>().Property(p => p.PropertyValue).HasColumnName("PROPERTY_VALUE");

            ////DbSet<FunctionFrame> FunctionFrames
            //modelBuilder.Entity<FunctionFrame>().ToTable("FUNCTION_FRAME", "SECURITY");
            //modelBuilder.Entity<FunctionFrame>().HasKey(k => k.FunctionFrameID);
            //modelBuilder.Entity<FunctionFrame>().Property(p => p.FunctionFrameID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            //modelBuilder.Entity<FunctionFrame>().Property(p => p.FunctionFrameID).HasColumnName("FUNCTION_FRAME_ID").HasColumnType("int");
            //modelBuilder.Entity<FunctionFrame>().Property(p => p.ApplicationId).HasColumnName("APPLICATION_ID").HasColumnType("int");
            //modelBuilder.Entity<FunctionFrame>().Property(p => p.Name).HasColumnName("FUNCTION_FRAME_NAME");

            ////DbSet<FunctionDetail> FunctionDetails
            //modelBuilder.Entity<FunctionDetail>().ToTable("FUNCTION_DETAIL", "SECURITY");
            //modelBuilder.Entity<FunctionDetail>().HasKey(k => k.FunctionDetailID);
            //modelBuilder.Entity<FunctionDetail>().Property(p => p.FunctionDetailID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            //modelBuilder.Entity<FunctionDetail>().Property(p => p.FunctionDetailID).HasColumnName("FUNCTION_DETAIL_ID").HasColumnType("int");
            //modelBuilder.Entity<FunctionDetail>().Property(p => p.FunctionFrameID).HasColumnName("FUNCTION_FRAME_ID").HasColumnType("int");
            //modelBuilder.Entity<FunctionDetail>().Property(p => p.Name).HasColumnName("FUNCTION_DETAIL_NAME");
            //modelBuilder.Entity<FunctionDetail>().Property(p => p.ViewName).HasColumnName("VIEW_NAME");
            //modelBuilder.Entity<FunctionDetail>().Property(p => p.Description).HasColumnName("DESCRIPTION");

            ////DbSet<FunctionsInRoles> FunctionsInRoles
            //modelBuilder.Entity<FunctionsInRoles>().ToTable("FUNCTIONS_IN_ROLES", "SECURITY");
            //modelBuilder.Entity<FunctionsInRoles>().HasKey(k => new { k.FunctionDetailID, k.RoleId });

            //modelBuilder.Entity<FunctionsInRoles>().Property(p => p.FunctionDetailID).HasColumnName("FUNCTION_DETAIL_ID").HasColumnType("int");
            //modelBuilder.Entity<FunctionsInRoles>().Property(p => p.RoleId).HasColumnName("ROLE_ID").HasColumnType("int");
            //modelBuilder.Entity<FunctionsInRoles>().Property(p => p.IsValid).HasColumnName("IS_VALID").HasColumnType("int");

            #endregion

            base.OnModelCreating(modelBuilder);
        }

    }
}
