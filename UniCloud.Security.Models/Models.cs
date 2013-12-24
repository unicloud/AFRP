using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using System.ServiceModel.DomainServices.Server;

namespace UniCloud.Security.Models
{
    public class Applications
    {
        public Applications()
        {
            this.Users = new HashSet<Users>();
            this.Roles = new HashSet<Roles>();
            this.FunctionItems = new HashSet<FunctionItem>();

        }

        public int ApplicationId { get; set; }
        [StringLength(256)]
        public string ApplicationName { get; set; }
        [StringLength(256)]
        public string LoweredApplicationName { get; set; }
        [StringLength(30)]
        public string ModuleName { get; set; }
        [StringLength(30)]
        public string ViewNameSpace { get; set; }
        [StringLength(256)]
        public string Description { get; set; }

        [Include]
        public virtual ICollection<Users> Users { get; set; }
        [Include]
        public virtual ICollection<Roles> Roles { get; set; }
        [Include]
        public virtual ICollection<FunctionItem> FunctionItems { get; set; }
    }
    public class ApplicationsConfiguration : EntityTypeConfiguration<Applications>
    {
        public ApplicationsConfiguration()
        {
            ToTable("Applications", DB.Schema);
            HasKey(k => k.ApplicationId);
            Property(p => p.ApplicationId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }


    public class FunctionItem
    {
        public FunctionItem()
        {
            this.FunctionsInRoles = new HashSet<FunctionsInRoles>();
        }

        public int FunctionItemID { get; set; }
        public int ApplicationId { get; set; }
        public int? ParentItemID { get; set; } // 父节点的ID
        public bool IsLeaf { get; set; } // 是否叶子节点
        public bool IsButton { get; set; } // 是否按钮
        [StringLength(30)]
        public string LevelCode { get; set; }
        [StringLength(50)]
        public string Name { get; set; }
        [StringLength(100)]
        public string ViewName { get; set; }
        [StringLength(256)]
        public string Description { get; set; }

        [Include]
        public virtual Applications Application { get; set; }
        [Include]
        public virtual ICollection<FunctionItem> SubItems { get; set; }
        [Include]
        public virtual ICollection<FunctionsInRoles> FunctionsInRoles { get; set; }
    }
    public class FunctionItemConfiguration : EntityTypeConfiguration<FunctionItem>
    {
        public FunctionItemConfiguration()
        {
            ToTable("FunctionItem", DB.Schema);
            HasKey(k => k.FunctionItemID);
            Property(p => p.FunctionItemID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            HasRequired(f => f.Application).WithMany(a => a.FunctionItems).HasForeignKey(f => f.ApplicationId).WillCascadeOnDelete(false);
            HasMany(f => f.SubItems).WithOptional().HasForeignKey(f => f.ParentItemID);
        }
    }


    public class FunctionsInRoles
    {
        [Required]
        public int FunctionItemID { get; set; }
        [Required]
        public int RoleId { get; set; }
        public bool IsValid { get; set; }

        [Include]
        public virtual Roles Role { get; set; }
        [Include]
        public virtual FunctionItem FunctionItem { get; set; }
    }
    public class FunctionsInRolesConfiguration : EntityTypeConfiguration<FunctionsInRoles>
    {
        public FunctionsInRolesConfiguration()
        {
            ToTable("FunctionsInRoles", DB.Schema);
            HasKey(k => new { k.FunctionItemID, k.RoleId });

            HasRequired(f => f.FunctionItem).WithMany(f => f.FunctionsInRoles).HasForeignKey(f => f.FunctionItemID);
            HasRequired(f => f.Role).WithMany(r => r.FunctionsInRoles).HasForeignKey(u => u.RoleId);
        }
    }


    public class Users
    {
        public Users()
        {
            this.UserInRoles = new HashSet<UserInRole>();
            this.PasswordHistories = new HashSet<PasswordHistory>();
        }

        public int UserId { get; set; }
        public int ApplicationId { get; set; }
        [StringLength(256)]
        public string UserName { get; set; }
        [StringLength(256)]
        public string LoweredUserName { get; set; }
        [StringLength(128)]
        public string Password { get; set; }
        public int PasswordFormat { get; set; }
        [StringLength(128)]
        public string PasswordSalt { get; set; }
        [StringLength(16)]
        public string MobileAlias { get; set; }
        [StringLength(256)]
        public string Email { get; set; }
        [StringLength(256)]
        public string LoweredEmail { get; set; }
        [StringLength(256)]
        public string PasswordQuestion { get; set; }
        [StringLength(128)]
        public string PasswordAnswer { get; set; }
        public bool IsApproved { get; set; }
        public bool IsLockedOut { get; set; }
        public bool IsAnonymous { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        public DateTime LastPasswordChangedDate { get; set; }
        public DateTime LastLockoutDate { get; set; }
        public DateTime LastActivityDate { get; set; }
        public int FailedPasswordAttemptCount { get; set; }
        public DateTime FailedPasswordAttemptWindowStart { get; set; }
        public int FailedPasswordAnswerAttemptCount { get; set; }
        public DateTime FailedPasswordAnswerAttemptWindowStart { get; set; }
        public string Comment { get; set; }

        [Include]
        public virtual Applications Application { get; set; }
        [Include]
        public virtual Profiles Profiles { get; set; }
        [Include]
        public ICollection<UserInRole> UserInRoles { get; set; }
        [Include]
        public ICollection<PasswordHistory> PasswordHistories { get; set; }
    }
    public class UsersConfiguration : EntityTypeConfiguration<Users>
    {
        public UsersConfiguration()
        {
            ToTable("Users", DB.Schema);
            HasKey(k => k.UserId);
            Property(p => p.UserId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(p => p.CreateDate).HasColumnType("datetime2");
            Property(p => p.LastLoginDate).HasColumnType("datetime2");
            Property(p => p.LastPasswordChangedDate).HasColumnType("datetime2");
            Property(p => p.LastLockoutDate).HasColumnType("datetime2");
            Property(p => p.LastActivityDate).HasColumnType("datetime2");
            Property(p => p.FailedPasswordAttemptWindowStart).HasColumnType("datetime2");
            Property(p => p.FailedPasswordAnswerAttemptWindowStart).HasColumnType("datetime2");

            HasRequired(u => u.Application).WithMany(a => a.Users).HasForeignKey(u => u.ApplicationId).WillCascadeOnDelete(false);
        }
    }


    /// <summary>
    /// 用户历史密码
    /// </summary>
    public class PasswordHistory
    {
        public int PasswordHistoryID { get; set; }
        public int UserId { get; set; }
        [StringLength(128)]
        public string Password { get; set; } //历史密码
        public DateTime CreateDate { get; set; }

        [Include]
        public virtual Users Users { get; set; }
    }
    public class PasswordHistoryConfiguration : EntityTypeConfiguration<PasswordHistory>
    {
        public PasswordHistoryConfiguration()
        {
            ToTable("PasswordHistory", DB.Schema);
            HasKey(k => k.PasswordHistoryID);
            Property(p => p.PasswordHistoryID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(p => p.CreateDate).HasColumnType("datetime2");

            HasRequired(p => p.Users).WithMany(u => u.PasswordHistories).HasForeignKey(p => p.UserId);
        }
    }


    public class Roles
    {
        public Roles()
        {
            this.UserInRoles = new HashSet<UserInRole>();
            this.FunctionsInRoles = new HashSet<FunctionsInRoles>();
        }

        public int RoleId { get; set; }
        public int ApplicationId { get; set; }
        [StringLength(256)]
        public string RoleName { get; set; }
        [StringLength(256)]
        public string LoweredRoleName { get; set; }
        [StringLength(256)]
        public string Description { get; set; }

        [Include]
        public virtual Applications Application { get; set; }
        [Include]
        public virtual ICollection<FunctionsInRoles> FunctionsInRoles { get; set; }
        [Include]
        public ICollection<UserInRole> UserInRoles { get; set; }
    }
    public class RolesConfiguration : EntityTypeConfiguration<Roles>
    {
        public RolesConfiguration()
        {
            ToTable("Roles", DB.Schema);
            HasKey(k => k.RoleId);
            Property(p => p.RoleId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            HasRequired(u => u.Application).WithMany(a => a.Roles).HasForeignKey(u => u.ApplicationId).WillCascadeOnDelete(false);
        }
    }


    public class UserInRole
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public int RoleId { get; set; }
        public bool IsValid { get; set; }

        [Include]
        public virtual Users Users { get; set; }
        [Include]
        public virtual Roles Roles { get; set; }
    }
    public class UserInRoleConfiguration : EntityTypeConfiguration<UserInRole>
    {
        public UserInRoleConfiguration()
        {
            ToTable("UserInRole", DB.Schema);
            HasKey(k => new { k.UserId, k.RoleId });

            HasRequired(u => u.Users).WithMany(u => u.UserInRoles).HasForeignKey(u => u.UserId);
            HasRequired(u => u.Roles).WithMany(r => r.UserInRoles).HasForeignKey(u => u.RoleId);
        }
    }


    public class Profiles
    {
        public Profiles()
        {
            this.ProfileValues = new HashSet<ProfileValues>();
        }

        public int UserId { get; set; }
        public int IsAnonymous { get; set; }
        public DateTime LastActivityDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }

        [Include]
        public virtual Users User { get; set; }
        [Include]
        public virtual ICollection<ProfileValues> ProfileValues { get; set; }
    }
    public class ProfilesConfiguration : EntityTypeConfiguration<Profiles>
    {
        public ProfilesConfiguration()
        {
            ToTable("Profiles", DB.Schema);
            HasKey(k => k.UserId);
            Property(p => p.UserId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            Property(p => p.LastActivityDate).HasColumnType("datetime2");
            Property(p => p.LastUpdatedDate).HasColumnType("datetime2");

            HasRequired(p => p.User).WithOptional(u => u.Profiles);
        }
    }


    [KnownType(typeof(FriendlyName))]
    public abstract class ProfileValues
    {
        public int ProfileValuesID { get; set; }
        public int UserId { get; set; }
        [StringLength(256)]
        public string PropertyValue { get; set; }

        [Include]
        public virtual Profiles Profiles { get; set; }
    }
    public class ProfileValuesConfiguration : EntityTypeConfiguration<ProfileValues>
    {
        public ProfileValuesConfiguration()
        {
            ToTable("ProfileValues", DB.Schema);
            HasKey(k => k.ProfileValuesID);
            Property(p => p.ProfileValuesID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            HasRequired(p => p.Profiles).WithMany(p => p.ProfileValues).HasForeignKey(p => p.UserId);
        }
    }


    public class FriendlyName : ProfileValues
    {
    }
    public class FriendlyNameConfiguration : EntityTypeConfiguration<FriendlyName>
    {
        public FriendlyNameConfiguration()
        {
        }
    }


}
