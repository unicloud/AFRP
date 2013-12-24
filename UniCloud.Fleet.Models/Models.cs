#region 更新日志

/// 2012-9-29，丁志浩
/// 1、Airlines实体增加IsCurrent属性，表示层通过这个属性管理当前航空公司。
/// 2、Aircraft和PlanAircraft的关系改为一对多。
/// 3、Aircraft增加SerialNumber（飞机序列号）属性。
/// 4、OperationHistory增加RegNumber属性，用于管理运营历史中可能发生的机号变更。

///2012-11-30,陈春勇
/// 1、Ower实体增加IsValid属性，表示层通过这个属性获取当前有效的Ower。
/// 2、PlanHistory实体增加AirlinesID属性，用户可选择当前计划历史从属于哪家公司，默认情况下为当前编辑的航空公司。
/// 3、ApprovalHistory实体增加AirlinesID属性，用户可选择当前申请历史从属于哪家公司，默认情况下为当前编辑的航空公司。
/// 4、MailAddress实体LoginUser属性长度由30改为100。
/// 5、Owner实体Name属性长度由50改为200,ShortName属性长度由10改为100,Description属性长度由100改为200。
/// 6、AircraftType实体Description属性长度由100改为200。
/// 7、DeliveryRisk实体Description 属性长度由100改为200。
/// 8、OperationHistory实体RegNumber属性长度由6改为10。
/// 9、Programming实体Name 属性长度由16改为20。
/// 10、Aircraft实体RegNumber属性长度由6改为10。
/// 11、Plan实体DocNumber属性长度由30改为100,AttachDocFileName属性由长度30改为100
/// 12、Request实体DocNumber属性长度由30改为100,AttachDocFileName属性由长度30改为100
/// 13、ApprovalDoc实体DocNumber属性长度由30改为100,AttachDocFileName属性由长度30改为100
/// 14、Plan实体增加评审备注属性ManageNote

///2013-02-01 zhangnx
///邮件账号属性修改增加Address、DisplayName、SendSSL、StartTLS、ReceiveSSL、ReceiveSSL、ServerType



#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using System.ServiceModel.DomainServices.Server;
using System.Xml.Linq;
using UniCloud.Cryptography;

namespace UniCloud.Fleet.Models
{

    #region 基础配置

    /// <summary>
    /// 应用基础配置
    /// </summary>
    [Serializable]
    public class XmlConfig
    {
        public Guid XmlConfigID { get; set; }
        public string ConfigType { get; set; }
        public string ConfigContent { get; set; }
        public int VersionNumber { get; set; }

        public XElement XmlContent
        {
            get
            {
                if (!string.IsNullOrEmpty(this.ConfigContent))
                { return XElement.Parse(this.ConfigContent); }
                else
                {
                    return null;
                }
            }
            set { this.ConfigContent = value.ToString(); }
        }
    }
    public class XmlConfigConfiguration : EntityTypeConfiguration<XmlConfig>
    {
        public XmlConfigConfiguration()
        {
            ToTable("XmlConfig", DB.Schema);
            HasKey(x => x.XmlConfigID);
            Property(p => p.XmlConfigID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            Ignore(x => x.XmlContent);

            Property(x => x.ConfigContent).HasColumnType("xml");
        }
    }

    [Serializable]
    public class XmlSetting
    {
        public Guid XmlSettingID { get; set; }
        public string SettingType { get; set; }
        public string SettingContent { get; set; }

        public XElement XmlContent
        {
            get { return XElement.Parse(this.SettingContent); }
            set { this.SettingContent = value.ToString(); }
        }
    }
    public class XmlSettingConfiguration : EntityTypeConfiguration<XmlSetting>
    {
        public XmlSettingConfiguration()
        {
            ToTable("XmlSetting", DB.Schema);
            HasKey(x => x.XmlSettingID);
            Property(p => p.XmlSettingID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            Ignore(x => x.XmlContent);

            Property(x => x.SettingContent).HasColumnType("xml");
        }
    }

    /// <summary>
    /// 邮箱地址
    /// </summary>
    [Serializable]
    public class MailAddress
    {
        public Guid MailAddressID { get; set; }
        public Guid OwnerID { get; set; }
        [StringLength(30)]
        public string SmtpHost { get; set; }
        [StringLength(30)]
        public string Pop3Host { get; set; }
        public int SendPort { get; set; }
        public int ReceivePort { get; set; }
        [StringLength(100)]
        public string LoginUser { get; set; }
        [StringLength(50)]
        public string LoginPassword { get; set; }
        //add by zhangnx 2013-02-01
        [StringLength(100)]
        //电子邮件账号
        public string Address { get; set; }
        [StringLength(100)]
        //账号名称
        public string DisplayName { get; set; }
        //发送要求安全连接
        public bool SendSSL { get; set; }
        //使用 StartTLS加密传输
        public bool StartTLS { get; set; }
        //接收要求安全连接
        public bool ReceiveSSL { get; set; }
        //服务器类型
        public int ServerType { get; set; }
        //end add

        [Include]
        public virtual Owner Owner { get; set; }

        public string OriginPassword
        {
            //get { return this.LoginPassword; }
            //set { this.LoginPassword = value; }

            get { return DESCryptography.DecryptString(this.LoginPassword); }
            set { this.LoginPassword = DESCryptography.EncryptString(value.ToString()); }
        }

    }
    public class MailAddressConfiguration : EntityTypeConfiguration<MailAddress>
    {
        public MailAddressConfiguration()
        {
            ToTable("MailAddress", DB.Schema);
            HasKey(m => m.MailAddressID);
            Property(m => m.MailAddressID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            Ignore(x => x.OriginPassword);
            HasRequired(m => m.Owner).WithMany(o => o.MailAddresses).HasForeignKey(m => m.OwnerID);
        }
    }


    /// <summary>
    /// 增加管理数据同步实体
    /// </summary>
    [Serializable]
    public class DataSynchronous
    {
        public Guid DataSynchronousID { get; set; }
        public string OperationCode { get; set; } //需要同步到数据库的SQL 语句
        public int Version { get; set; } //版本号，以版本号管理需要同步的航空公司
    }
    public class DataSynchronousConfiguration : EntityTypeConfiguration<DataSynchronous>
    {
        public DataSynchronousConfiguration()
        {
            ToTable("DataSynchronous", DB.Schema);
            HasKey(x => x.DataSynchronousID);
            Property(p => p.DataSynchronousID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
        }
    }

    #endregion

    #region 参与者

    /// <summary>
    /// 所有权人
    /// </summary>
    [KnownType(typeof(Manager))]
    [KnownType(typeof(Airlines))]
    [KnownType(typeof(Manufacturer))]
    [Serializable]
    public class Owner
    {
        public Owner()
        {
            this.OwnershipHistorys = new HashSet<OwnershipHistory>();
            this.MailAddresses = new HashSet<MailAddress>();
        }
        public Guid OwnerID { get; set; }
        [StringLength(200)]
        public string Name { get; set; }
        [StringLength(100)]
        public string ShortName { get; set; }
        [StringLength(200)]
        public string Description { get; set; }
        public bool IsValid { get; set; } //是否有效
        public int SupplierType { get; set; } //供应商类型 0-非供应商 1- 国内 2- 国外

        [Include]
        public virtual ICollection<OwnershipHistory> OwnershipHistorys { get; set; }
        [Include]
        public virtual ICollection<MailAddress> MailAddresses { get; set; }
    }
    public class OwnerConfiguration : EntityTypeConfiguration<Owner>
    {
        public OwnerConfiguration()
        {
            ToTable("Owner", DB.Schema);
            HasKey(k => k.OwnerID);
            Property(p => p.OwnerID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
        }
    }


    /// <summary>
    /// 管理者
    /// 包括民航局、发改委等
    /// </summary>
    [Serializable]
    public class Manager : Owner
    { }
    public class ManagerConfiguration : EntityTypeConfiguration<Manager>
    {
        public ManagerConfiguration()
        {
            ToTable("Manager", DB.Schema);
        }
    }


    /// <summary>
    /// 航空公司
    /// 即是运营人，也通常是所有权人
    /// </summary>
    [Serializable]
    public class Airlines : Owner
    {
        public Airlines()
        {
            this.OperationHistories = new HashSet<OperationHistory>();
            this.SubOperationHistories = new HashSet<SubOperationHistory>();
            this.Plans = new HashSet<Plan>();
            this.Requests = new HashSet<Request>();
            this.SubAirlines = new HashSet<Airlines>();
        }

        public Guid? MasterID { get; set; } // 所属航空公司ID
        [StringLength(3)]
        public string ICAOCode { get; set; } // 三字码
        [StringLength(2)]
        public string IATACode { get; set; } // 二字码
        [StringLength(30)]
        public string LevelCode { get; set; } //用于排序，由航空公司自定规则
        public bool IsShareData { get; set; } //航空公司之间是否共享数据
        public bool IsCurrent { get; set; } // 是否当前航空公司
        public DateTime? CreateDate { get; set; } // 创建日期
        public DateTime? LogoutDate { get; set; } // 注销日期
        public DateTime? OperationDate { get; set; } // 运营日期
        public DateTime? ExportDate { get; set; } //   退出运营日期
        public int SubType { get; set; } // 航空公司类型，0-代表分公司，1-子公司，2-分子公司(但不上报计划、申请)

        public int Type { get; set; } // 航空公司类型，包括运输航空公司、通用航空公司等
        public AirlinesType AirlinesType
        {
            get { return (AirlinesType)Type; }
            set { Type = (int)value; }
        }

        public int Status { get; set; }  //主要适用于分公司编辑，分公司允许删除；0-在用，1-删除
        public FilialeStatus FilialeStatus
        {
            get { return (FilialeStatus)Status; }
            set { Status = (int)value; }
        }

        [Include]
        public virtual ICollection<OperationHistory> OperationHistories { get; set; }
        [Include]
        public virtual ICollection<Plan> Plans { get; set; }
        [Include]
        public virtual ICollection<Request> Requests { get; set; }
        [Include]
        public virtual ICollection<Airlines> SubAirlines { get; set; }
        [Include]
        public virtual ICollection<SubOperationHistory> SubOperationHistories { get; set; }
    }
    public class AirlinesConfiguration : EntityTypeConfiguration<Airlines>
    {
        public AirlinesConfiguration()
        {
            ToTable("Airlines", DB.Schema);
            Property(p => p.CreateDate).HasColumnType("datetime2");
            Property(p => p.LogoutDate).HasColumnType("datetime2");
            Property(p => p.OperationDate).HasColumnType("datetime2");
            Property(p => p.ExportDate).HasColumnType("datetime2");

            HasMany(a => a.SubAirlines).WithOptional().HasForeignKey(a => a.MasterID);
        }
    }


    /// <summary>
    /// 制造商
    /// </summary>
    [Serializable]
    public class Manufacturer : Owner
    {
        public Manufacturer()
        {
            this.AircraftTypes = new HashSet<AircraftType>();
        }

        [Include]
        public virtual ICollection<AircraftType> AircraftTypes { get; set; }
    }
    public class ManufacturerConfiguration : EntityTypeConfiguration<Manufacturer>
    {
        public ManufacturerConfiguration()
        {
            ToTable("Manufacturer", DB.Schema);
        }
    }

    #endregion

    #region 飞机

    /// <summary>
    /// 飞机类别
    /// </summary>
    public class AircraftCategory
    {
        public AircraftCategory()
        {
            this.AircraftTypes = new HashSet<AircraftType>();
        }

        public Guid AircraftCategoryID { get; set; }
        [StringLength(6), Display(Name = "飞机类别")]
        public string Category { get; set; } // 客机、货机
        [StringLength(30), Display(Name = "座级")]
        public string Regional { get; set; } //座级：250座以上客机、100-200座客机、100座以下客机；大型货机、中型货机

        [Include]
        public virtual ICollection<AircraftType> AircraftTypes { get; set; }
    }
    public class AircraftCategoryConfiguration : EntityTypeConfiguration<AircraftCategory>
    {
        public AircraftCategoryConfiguration()
        {
            ToTable("AircraftCategory", DB.Schema);
            HasKey(k => k.AircraftCategoryID);
            Property(p => p.AircraftCategoryID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
        }
    }


    /// <summary>
    /// 机型
    /// </summary>
    [Serializable]
    public class AircraftType
    {
        public AircraftType()
        {
            this.Aircrafts = new HashSet<Aircraft>();
        }

        public Guid AircraftTypeID { get; set; }
        public Guid ManufacturerID { get; set; }
        public Guid AircraftCategoryID { get; set; }
        [StringLength(16), Display(Name = "机型")]
        public string Name { get; set; }
        [StringLength(200)]
        public string Description { get; set; }

        [Include]
        public virtual Manufacturer Manufacturer { get; set; }
        [Include]
        public virtual AircraftCategory AircraftCategory { get; set; }
        [Include]
        public virtual ICollection<Aircraft> Aircrafts { get; set; }
    }
    public class AircraftTypeConfiguration : EntityTypeConfiguration<AircraftType>
    {
        public AircraftTypeConfiguration()
        {
            ToTable("AircraftType", DB.Schema);
            HasKey(k => k.AircraftTypeID);
            Property(p => p.AircraftTypeID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            HasRequired(a => a.Manufacturer).WithMany(m => m.AircraftTypes).HasForeignKey(a => a.ManufacturerID);
            HasRequired(a => a.AircraftCategory).WithMany(ac => ac.AircraftTypes).HasForeignKey(a => a.AircraftCategoryID);
        }
    }


    /// <summary>
    /// 运营飞机
    /// 所有在中国的运营中的飞机
    /// </summary>
    [Serializable]
    public class Aircraft
    {
        public Aircraft()
        {
            this.OwnershipHistorys = new HashSet<OwnershipHistory>();
            this.OperationHistories = new HashSet<OperationHistory>();
            this.AircraftBusinesses = new HashSet<AircraftBusiness>();
            this.PlanAircrafts = new HashSet<PlanAircraft>();
        }

        public Guid AircraftID { get; set; }
        public Guid AircraftTypeID { get; set; }
        public Guid? OwnerID { get; set; }
        public Guid AirlinesID { get; set; }
        public Guid ImportCategoryID { get; set; } // 引进方式
        [StringLength(10), Display(Name = "机号")]
        public string RegNumber { get; set; } // 飞机注册号
        [StringLength(20), Display(Name = "序列号")]
        [Required(ErrorMessage = "序列号不能为空")]
        public string SerialNumber { get; set; } // 飞机序列号
        [Display(Name = "创建日期")]
        public DateTime? CreateDate { get; set; } // 开始日期
        [Display(Name = "出厂日期")]
        public DateTime? FactoryDate { get; set; } // 出厂日期
        [Display(Name = "引进日期")]
        public DateTime? ImportDate { get; set; } // 引进日期
        [Display(Name = "注销日期")]
        public DateTime? ExportDate { get; set; } // 注销日期
        [Display(Name = "运营状态")]
        public bool IsOperation { get; set; } // 是否在运营

        [Display(Name = "座位数")]
        public int SeatingCapacity { get; set; } // 当前座位数
        [Display(Name = "商载(吨)")]
        public decimal CarryingCapacity { get; set; } // 当前商载，单位吨

        [Include]
        public virtual AircraftType AircraftType { get; set; }
        [Include]
        public virtual Owner Owner { get; set; }
        [Include]
        public virtual Airlines Airlines { get; set; }
        [Include]
        public virtual ActionCategory ImportCategory { get; set; }
        [Include]
        public virtual ICollection<PlanAircraft> PlanAircrafts { get; set; }
        [Include]
        public virtual ICollection<OwnershipHistory> OwnershipHistorys { get; set; }
        [Include]
        public virtual ICollection<OperationHistory> OperationHistories { get; set; }
        [Include]
        public virtual ICollection<AircraftBusiness> AircraftBusinesses { get; set; }
    }
    public class AircraftConfiguration : EntityTypeConfiguration<Aircraft>
    {
        public AircraftConfiguration()
        {
            ToTable("Aircraft", DB.Schema);
            HasKey(k => k.AircraftID);
            Property(p => p.AircraftID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            Property(p => p.CreateDate).HasColumnType("datetime2");
            Property(p => p.FactoryDate).HasColumnType("datetime2");
            Property(p => p.ImportDate).HasColumnType("datetime2");
            Property(p => p.ExportDate).HasColumnType("datetime2");

            HasRequired(a => a.AircraftType).WithMany(a => a.Aircrafts).HasForeignKey(a => a.AircraftTypeID);
            HasOptional(a => a.Owner).WithMany().HasForeignKey(a => a.OwnerID);
            HasRequired(a => a.Airlines).WithMany().HasForeignKey(a => a.AirlinesID);
            HasRequired(a => a.ImportCategory).WithMany().HasForeignKey(a => a.ImportCategoryID);
        }
    }


    /// <summary>
    /// 计划飞机
    /// 处于计划阶段的飞机，提交计划时创建
    /// </summary>
    [Serializable]
    public class PlanAircraft
    {
        public PlanAircraft()
        {
            this.PlanHistories = new HashSet<PlanHistory>();
            this.ApprovalHistories = new HashSet<ApprovalHistory>();
            this.AgreementDetails = new HashSet<AgreementDetail>();
        }

        public Guid PlanAircraftID { get; set; }
        public Guid? AircraftID { get; set; }
        public Guid AircraftTypeID { get; set; }
        public Guid AirlinesID { get; set; }
        public bool IsLock { get; set; } // 是否锁定，确定计划时锁定相关飞机。一旦锁定，对应的引进计划明细不能修改机型。
        public bool IsOwn { get; set; } // 是否自有，用以区分PlanAircraft，民航局均为False。

        [Display(Name = "管理状态")]
        public int Status { get; set; } // 管理状态：包括预备、计划、申请、批文、签约、运营、停场待退、退役，申请未批准的回到预备阶段
        [Display(Name = "管理状态")]
        public ManageStatus ManageStatus
        {
            get { return (ManageStatus)Status; }
            set { Status = (int)value; }
        }

        [Include]
        public virtual Aircraft Aircraft { get; set; }
        [Include]
        public virtual Airlines Airlines { get; set; }
        [Include]
        public virtual AircraftType AircraftType { get; set; }
        [Include]
        public virtual ICollection<PlanHistory> PlanHistories { get; set; }
        [Include]
        public virtual ICollection<ApprovalHistory> ApprovalHistories { get; set; }
        [Include]
        public virtual ICollection<AgreementDetail> AgreementDetails { get; set; }
    }
    public class PlanAircraftConfiguration : EntityTypeConfiguration<PlanAircraft>
    {
        public PlanAircraftConfiguration()
        {
            ToTable("PlanAircraft", DB.Schema);
            HasKey(k => k.PlanAircraftID);
            Property(p => p.PlanAircraftID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            HasRequired(p => p.Airlines).WithMany().HasForeignKey(p => p.AirlinesID);
            HasRequired(p => p.AircraftType).WithMany().HasForeignKey(p => p.AircraftTypeID);
            HasOptional(p => p.Aircraft).WithMany(a => a.PlanAircrafts).HasForeignKey(p => p.AircraftID);
        }
    }

    /// <summary>
    /// 所有权历史
    /// </summary>
    [Serializable]
    public class OwnershipHistory
    {
        public Guid OwnershipHistoryID { get; set; }
        public Guid AircraftID { get; set; }
        [Display(Name = "所有权人")]
        public Guid OwnerID { get; set; }
        [Display(Name = "开始日期")]
        public DateTime? StartDate { get; set; } // 开始日期
        [Display(Name = "结束日期")]
        public DateTime? EndDate { get; set; } // 结束日期

        [Display(Name = "处理状态")]
        public int Status { get; set; } // 处理状态：包括草稿、待审核、已审核、已提交。
        [Display(Name = "处理状态")]
        public OpStatus OpStatus
        {
            get { return (OpStatus)Status; }
            set { Status = (int)value; }
        }

        [Include]
        public virtual Aircraft Aircraft { get; set; }
        [Include]
        public virtual Owner Owner { get; set; }
    }
    public class OwnershipHistoryConfiguration : EntityTypeConfiguration<OwnershipHistory>
    {
        public OwnershipHistoryConfiguration()
        {
            ToTable("OwnershipHistory", DB.Schema);
            HasKey(k => k.OwnershipHistoryID);
            Property(p => p.OwnershipHistoryID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            Property(p => p.StartDate).HasColumnType("datetime2");
            Property(p => p.EndDate).HasColumnType("datetime2");

            HasRequired(o => o.Aircraft).WithMany(a => a.OwnershipHistorys).HasForeignKey(o => o.AircraftID);
            HasRequired(o => o.Owner).WithMany(o => o.OwnershipHistorys).HasForeignKey(o => o.OwnerID);
        }
    }


    /// <summary>
    /// 计划历史
    /// </summary>
    [KnownType(typeof(OperationPlan))]
    [KnownType(typeof(ChangePlan))]
    [Serializable]
    public abstract class PlanHistory
    {
        public Guid PlanHistoryID { get; set; }
        public Guid? PlanAircraftID { get; set; }
        public Guid PlanID { get; set; }
        public Guid? ApprovalHistoryID { get; set; }
        public Guid ActionCategoryID { get; set; } // 活动类别：包括引进、退出、变更
        public Guid TargetCategoryID { get; set; } // 目标类别：具体的引进、退出方式
        public Guid AircraftTypeID { get; set; } // 计划机型
        public Guid AirlinesID { get; set; }
        [Display(Name = "执行年度")]
        public Guid PerformAnnualID { get; set; } // 执行年度
        [Display(Name = "执行月份")]
        public int PerformMonth { get; set; }
        [Display(Name = "净增座位")]
        public int SeatingCapacity { get; set; }
        [Display(Name = "净增商载(吨)")]
        public decimal CarryingCapacity { get; set; }
        public bool IsValid { get; set; } // 是否有效，确认计划时将计划相关条目置为有效，只有有效的条目才能执行。已有申请、批文的始终有效。
        public bool IsAdjust { get; set; } // 是否调整项
        [StringLength(200)]
        public string Note { get; set; }
        public bool IsSubmit { get; set; } // 是否上报

        [Include]
        public virtual PlanAircraft PlanAircraft { get; set; }
        [Include]
        public virtual Plan Plan { get; set; }
        [Include]
        public virtual ActionCategory ActionCategory { get; set; }
        [Include]
        public virtual ActionCategory TargetCategory { get; set; }
        [Include]
        public virtual AircraftType AircraftType { get; set; }
        [Include]
        public virtual ApprovalHistory ApprovalHistory { get; set; }
        [Include]
        public virtual Annual Annual { get; set; }
        [Include]
        public virtual Airlines Airlines { get; set; }

    }
    public class PlanHistoryConfiguration : EntityTypeConfiguration<PlanHistory>
    {
        public PlanHistoryConfiguration()
        {
            ToTable("PlanHistory", DB.Schema);
            HasKey(k => k.PlanHistoryID);
            Property(p => p.PlanHistoryID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            HasOptional(p => p.PlanAircraft).WithMany(p => p.PlanHistories).HasForeignKey(p => p.PlanAircraftID);
            HasRequired(p => p.Plan).WithMany(p => p.PlanHistories).HasForeignKey(p => p.PlanID);
            HasRequired(p => p.ActionCategory).WithMany().HasForeignKey(p => p.ActionCategoryID);
            HasRequired(p => p.TargetCategory).WithMany().HasForeignKey(p => p.TargetCategoryID);
            HasRequired(p => p.AircraftType).WithMany().HasForeignKey(p => p.AircraftTypeID);
            HasOptional(p => p.ApprovalHistory).WithMany().HasForeignKey(p => p.ApprovalHistoryID);
            HasRequired(p => p.Annual).WithMany().HasForeignKey(p => p.PerformAnnualID);
            HasRequired(p => p.Airlines).WithMany().HasForeignKey(p => p.AirlinesID);
        }
    }


    /// <summary>
    /// 运营计划
    /// </summary>
    [Serializable]
    public class OperationPlan : PlanHistory
    {
        public Guid? OperationHistoryID { get; set; }

        [Include]
        public virtual OperationHistory OperationHistory { get; set; }
    }
    public class OperationPlanConfiguration : EntityTypeConfiguration<OperationPlan>
    {
        public OperationPlanConfiguration()
        {
            ToTable("OperationPlan", DB.Schema);

            HasOptional(o => o.OperationHistory).WithMany().HasForeignKey(o => o.OperationHistoryID);
        }
    }


    /// <summary>
    /// 变更计划
    /// </summary>
    [Serializable]
    public class ChangePlan : PlanHistory
    {
        public Guid? AircraftBusinessID { get; set; }

        [Include]
        public virtual AircraftBusiness AircraftBusiness { get; set; }
    }
    public class ChangePlanConfiguration : EntityTypeConfiguration<ChangePlan>
    {
        public ChangePlanConfiguration()
        {
            ToTable("ChangePlan", DB.Schema);

            HasOptional(c => c.AircraftBusiness).WithMany().HasForeignKey(c => c.AircraftBusinessID);
        }
    }


    /// <summary>
    /// 审批历史
    /// 批文申请的列表
    /// </summary>
    [Serializable]
    public class ApprovalHistory
    {
        public Guid ApprovalHistoryID { get; set; }
        public Guid PlanAircraftID { get; set; }
        public Guid RequestID { get; set; }
        public Guid ImportCategoryID { get; set; } // 申请引进方式
        public Guid AirlinesID { get; set; }
        [Display(Name = "座位数")]
        public int SeatingCapacity { get; set; } // 座位数
        [Display(Name = "商载(吨)")]
        public decimal CarryingCapacity { get; set; } // 商载，单位为吨
        [Display(Name = "交付年度")]
        public Guid RequestDeliverAnnualID { get; set; } // 申请交付年度
        [Display(Name = "交付月份")]
        public int RequestDeliverMonth { get; set; } // 申请交付月份
        [Display(Name = "是否批准")]
        public bool IsApproved { get; set; } // 是否批准
        [StringLength(200)]
        public string Note { get; set; }

        [Include]
        public virtual Request Request { get; set; }
        [Include]
        public virtual ManaApprovalHistory ManaApprovalHistory { get; set; }
        [Include]
        public virtual OperationHistory OperationHistory { get; set; }
        [Include]
        public virtual PlanAircraft PlanAircraft { get; set; }
        [Include]
        public virtual Annual Annual { get; set; }
        [Include]
        public virtual ActionCategory ImportCategory { get; set; }
        [Include]
        public virtual Airlines Airlines { get; set; }
    }
    public class ApprovalHistoryConfiguration : EntityTypeConfiguration<ApprovalHistory>
    {
        public ApprovalHistoryConfiguration()
        {
            ToTable("ApprovalHistory", DB.Schema);
            HasKey(k => k.ApprovalHistoryID);
            Property(p => p.ApprovalHistoryID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            HasRequired(a => a.PlanAircraft).WithMany(p => p.ApprovalHistories).HasForeignKey(a => a.PlanAircraftID);
            HasRequired(a => a.Request).WithMany(r => r.ApprovalHistories).HasForeignKey(a => a.RequestID);
            HasRequired(a => a.Annual).WithMany().HasForeignKey(a => a.RequestDeliverAnnualID);
            HasRequired(a => a.ImportCategory).WithMany().HasForeignKey(a => a.ImportCategoryID);
            HasRequired(p => p.Airlines).WithMany().HasForeignKey(p => p.AirlinesID);
        }
    }


    /// <summary>
    /// 管理批文历史
    /// 仅由民航局使用，由民航局业务人员录入
    /// </summary>
    [Serializable]
    public class ManaApprovalHistory
    {
        public Guid ApprovalHistoryID { get; set; }
        public bool IsApproved { get; set; } // 是否批准

        [Include]
        public virtual ApprovalHistory ApprovalHistory { get; set; }
    }
    public class ManaApprovalHistoryConfiguration : EntityTypeConfiguration<ManaApprovalHistory>
    {
        public ManaApprovalHistoryConfiguration()
        {
            ToTable("ManaApprovalHistory", DB.Schema);
            HasKey(k => k.ApprovalHistoryID);
            Property(p => p.ApprovalHistoryID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            HasRequired(m => m.ApprovalHistory).WithRequiredDependent(a => a.ManaApprovalHistory);
        }
    }


    /// <summary>
    /// 协议汇总
    /// </summary>
    [Serializable]
    public class Agreement
    {
        public Agreement()
        {
            this.AgreementDetails = new HashSet<AgreementDetail>();
        }
        public Guid AgreementID { get; set; }
        public Guid SupplierID { get; set; }
        public Guid AirlinesID { get; set; }
        public Guid? PlanAgreeAnnualID { get; set; } // 谈判计划启动年度
        public int PlanAgreeMonth { get; set; } // 谈判计划启动月份
        public Guid? ActualAgreeAnnualID { get; set; } // 谈判实际启动年度
        public int ActualAgreeMonth { get; set; } // 谈判实际启动月份
        public DateTime? SignedDate { get; set; } // 签约日期
        [StringLength(200)]
        public string Note { get; set; } // 备注

        public int Type { get; set; } // 协议类型：包括意向、合同
        public AgreementType AgreementType
        {
            get { return (AgreementType)Type; }
            set { Type = (int)value; }
        }

        public int Phase { get; set; } // 协议阶段：包括计划、谈判、签约
        public AgreementPhase AgreementPhase
        {
            get { return (AgreementPhase)Phase; }
            set { Phase = (int)value; }
        }

        public int Status { get; set; } // 处理状态：包括草稿、待审核、已审核、已提交。
        public OpStatus OpStatus
        {
            get { return (OpStatus)Status; }
            set { Status = (int)value; }
        }

        [Include]
        public virtual Owner Owner { get; set; }
        [Include]
        public virtual Annual PlanAnnual { get; set; }
        [Include]
        public virtual Annual ActualAnnual { get; set; }
        [Include]
        public virtual Airlines Airlines { get; set; }
        [Include]
        public virtual ICollection<AgreementDetail> AgreementDetails { get; set; }
    }
    public class AgreementConfiguration : EntityTypeConfiguration<Agreement>
    {
        public AgreementConfiguration()
        {
            ToTable("Agreement", DB.Schema);
            HasKey(k => k.AgreementID);
            Property(p => p.AgreementID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            Property(p => p.SignedDate).HasColumnType("datetime2");

            HasRequired(a => a.Owner).WithMany().HasForeignKey(o => o.SupplierID);
            HasOptional(a => a.PlanAnnual).WithMany().HasForeignKey(a => a.PlanAgreeAnnualID);
            HasOptional(a => a.ActualAnnual).WithMany().HasForeignKey(a => a.ActualAgreeAnnualID);
            HasRequired(a => a.Airlines).WithMany().HasForeignKey(a => a.AirlinesID);
        }
    }


    /// <summary>
    /// 交付风险
    /// </summary>
    [Serializable]
    public class DeliveryRisk
    {
        public DeliveryRisk()
        {
            this.AgreementDetails = new HashSet<AgreementDetail>();
        }
        public Guid DeliveryRiskID { get; set; }
        [StringLength(30)]
        public string Name { get; set; }
        [StringLength(200)]
        public string Description { get; set; }

        [Include]
        public virtual ICollection<AgreementDetail> AgreementDetails { get; set; }
    }
    public class DeliveryRiskConfiguration : EntityTypeConfiguration<DeliveryRisk>
    {
        public DeliveryRiskConfiguration()
        {
            ToTable("DeliveryRisk", DB.Schema);
            HasKey(k => k.DeliveryRiskID);
            Property(p => p.DeliveryRiskID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
        }
    }


    /// <summary>
    /// 协议明细
    /// </summary>
    [Serializable]
    public class AgreementDetail
    {
        public Guid AgreementDetailID { get; set; }
        public Guid AgreementID { get; set; }
        public Guid PlanAircraftID { get; set; }
        public Guid DeliveryRiskID { get; set; }
        public Guid ImportCategoryID { get; set; } // 协议引进方式
        public int SeatingCapacity { get; set; } // 座位数
        public decimal CarryingCapacity { get; set; } // 商载，单位为吨
        public Guid? PlanDeliverAnnualID { get; set; } // 协议交付年度
        public int PlanDeliverMonth { get; set; } // 协议交付月份

        [Include]
        public virtual Agreement Agreement { get; set; }
        [Include]
        public virtual PlanAircraft PlanAircraft { get; set; }
        [Include]
        public virtual DeliveryRisk DeliveryRisk { get; set; }
        [Include]
        public virtual Annual Annual { get; set; }
        [Include]
        public virtual ActionCategory ImportCategory { get; set; }
    }
    public class AgreementDetailConfiguration : EntityTypeConfiguration<AgreementDetail>
    {
        public AgreementDetailConfiguration()
        {
            ToTable("AgreementDetail", DB.Schema);
            HasKey(k => k.AgreementDetailID);
            Property(p => p.AgreementDetailID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            HasRequired(a => a.PlanAircraft).WithMany(p => p.AgreementDetails).HasForeignKey(a => a.PlanAircraftID);
            HasRequired(a => a.Agreement).WithMany(a => a.AgreementDetails).HasForeignKey(a => a.AgreementID);
            HasRequired(a => a.DeliveryRisk).WithMany(d => d.AgreementDetails).HasForeignKey(a => a.DeliveryRiskID);
            HasOptional(a => a.Annual).WithMany().HasForeignKey(a => a.PlanDeliverAnnualID);
            HasRequired(a => a.ImportCategory).WithMany().HasForeignKey(a => a.ImportCategoryID);
        }
    }


    /// <summary>
    /// 运营权历史
    /// 由飞机在某家航空公司的运营期间构成的飞机在中国的运营历史
    /// </summary>
    [Serializable]
    public class OperationHistory
    {
        public OperationHistory()
        {
            this.SubOperationHistories = new HashSet<SubOperationHistory>();
        }
        public Guid OperationHistoryID { get; set; }
        public Guid AirlinesID { get; set; }
        public Guid AircraftID { get; set; }
        public Guid ImportCategoryID { get; set; } // 实际引进方式
        public Guid? ExportCategoryID { get; set; } // 实际退出方式
        [StringLength(10), Display(Name = "机号")]
        //[RegularExpression(@"^[B,C]-\d{4}$", ErrorMessage = "格式有误，正确应类似B-9999")]
        public string RegNumber { get; set; } // 飞机注册号
        [Display(Name = "技术接收日期")]
        public DateTime? TechReceiptDate { get; set; } // 技术接收日期
        [Display(Name = "接收日期")]
        public DateTime? ReceiptDate { get; set; } // 接收日期
        [Display(Name = "运营日期")]
        public DateTime? StartDate { get; set; } // 运营日期
        [Display(Name = "退出停场日期")]
        public DateTime? StopDate { get; set; } // 退出停场日期
        [Display(Name = "技术交付日期")]
        public DateTime? TechDeliveryDate { get; set; } // 技术交付日期
        [Display(Name = "退出日期")]
        public DateTime? EndDate { get; set; } // 退出日期
        [Display(Name = "起租日期")]
        public DateTime? OnHireDate { get; set; } // 起租日期，适用于租赁
        [StringLength(200), Display(Name = "说明")]
        public string Note { get; set; }

        [Display(Name = "处理状态")]
        public int Status { get; set; } // 处理状态：包括草稿、待审核、已审核、已提交。
        [Display(Name = "处理状态")]
        public OpStatus OpStatus
        {
            get { return (OpStatus)Status; }
            set { Status = (int)value; }
        }

        [Include]
        public virtual ApprovalHistory ApprovalHistory { get; set; }
        [Include]
        public virtual Airlines Airlines { get; set; }
        [Include]
        public virtual Aircraft Aircraft { get; set; }
        [Include]
        public virtual ActionCategory ImportCategory { get; set; }
        [Include]
        public virtual ActionCategory ExportCategory { get; set; }
        [Include]
        public virtual ICollection<SubOperationHistory> SubOperationHistories { get; set; }
    }
    public class OperationHistoryConfiguration : EntityTypeConfiguration<OperationHistory>
    {
        public OperationHistoryConfiguration()
        {
            ToTable("OperationHistory", DB.Schema);
            HasKey(k => k.OperationHistoryID);
            Property(p => p.OperationHistoryID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            Property(p => p.StartDate).HasColumnType("datetime2");
            Property(p => p.EndDate).HasColumnType("datetime2");
            Property(p => p.TechReceiptDate).HasColumnType("datetime2");
            Property(p => p.ReceiptDate).HasColumnType("datetime2");
            Property(p => p.OnHireDate).HasColumnType("datetime2");
            Property(p => p.TechDeliveryDate).HasColumnType("datetime2");
            Property(p => p.StopDate).HasColumnType("datetime2");

            HasRequired(o => o.ApprovalHistory).WithOptional(a => a.OperationHistory);
            HasRequired(o => o.Airlines).WithMany(a => a.OperationHistories).HasForeignKey(o => o.AirlinesID);
            HasRequired(o => o.Aircraft).WithMany(a => a.OperationHistories).HasForeignKey(o => o.AircraftID);
            HasRequired(o => o.ImportCategory).WithMany().HasForeignKey(o => o.ImportCategoryID);
            HasOptional(o => o.ExportCategory).WithMany().HasForeignKey(o => o.ExportCategoryID);
        }
    }


    /// <summary>
    /// 运营权历史(分公司）
    /// </summary>
    [Serializable]
    public class SubOperationHistory
    {
        public Guid SubOperationHistoryID { get; set; }
        public Guid OperationHistoryID { get; set; }
        public Guid SubAirlinesID { get; set; }  //分公司
        [Display(Name = "运营日期")]
        public DateTime? StartDate { get; set; } // 运营日期
        [Display(Name = "退出日期")]
        public DateTime? EndDate { get; set; } // 退出日期

        [Display(Name = "处理状态")]
        public int Status { get; set; } // 处理状态：包括草稿、待审核、已审核。
        [Display(Name = "处理状态")]
        public OpStatus OpStatus
        {
            get { return (OpStatus)Status; }
            set { Status = (int)value; }
        }

        [Include]
        public virtual OperationHistory OperationHistory { get; set; }
        [Include]
        public virtual Airlines Airlines { get; set; }
    }
    public class SubOperationHistoryConfiguration : EntityTypeConfiguration<SubOperationHistory>
    {
        public SubOperationHistoryConfiguration()
        {
            ToTable("SubOperationHistory", DB.Schema);
            HasKey(k => k.SubOperationHistoryID);
            Property(p => p.SubOperationHistoryID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            Property(p => p.StartDate).HasColumnType("datetime2");
            Property(p => p.EndDate).HasColumnType("datetime2");

            HasRequired(p => p.OperationHistory).WithMany(p => p.SubOperationHistories).HasForeignKey(p => p.OperationHistoryID);
            HasRequired(p => p.Airlines).WithMany(p => p.SubOperationHistories).HasForeignKey(p => p.SubAirlinesID);
        }
    }


    /// <summary>
    /// 飞机商业数据历史
    /// 记录一个运营期内飞机商业数据的变化，包括机型、座位、商载、引进方式等
    /// </summary>
    [Serializable]
    public class AircraftBusiness
    {
        public Guid AircraftBusinessID { get; set; }
        public Guid AircraftID { get; set; }
        public Guid AircraftTypeID { get; set; }
        public Guid ImportCategoryID { get; set; } // 引进方式
        [Display(Name = "座位数")]
        public int SeatingCapacity { get; set; } // 座位数
        [Display(Name = "商载(吨)")]
        public decimal CarryingCapacity { get; set; } // 商载，单位吨
        [Display(Name = "开始日期")]
        public DateTime? StartDate { get; set; } // 开始日期
        [Display(Name = "结束日期")]
        public DateTime? EndDate { get; set; } // 结束日期

        [Display(Name = "处理状态")]
        public int Status { get; set; } // 处理状态：包括草稿、待审核、已审核、已提交。
        [Display(Name = "处理状态")]
        public OpStatus OpStatus
        {
            get { return (OpStatus)Status; }
            set { Status = (int)value; }
        }

        [Include]
        public virtual Aircraft Aircraft { get; set; }
        [Include]
        public virtual AircraftType AircraftType { get; set; }
        [Include]
        public virtual ActionCategory ImportCategory { get; set; }
    }
    public class AircraftBusinessConfiguration : EntityTypeConfiguration<AircraftBusiness>
    {
        public AircraftBusinessConfiguration()
        {
            ToTable("AircraftBusiness", DB.Schema);
            HasKey(k => k.AircraftBusinessID);
            Property(p => p.AircraftBusinessID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            Property(p => p.StartDate).HasColumnType("datetime2");
            Property(p => p.EndDate).HasColumnType("datetime2");

            HasRequired(a => a.Aircraft).WithMany(a => a.AircraftBusinesses).HasForeignKey(a => a.AircraftID);
            HasRequired(a => a.ImportCategory).WithMany().HasForeignKey(a => a.ImportCategoryID);
        }
    }

    #endregion

    #region 机队管理

    /// <summary>
    /// 操作类别
    /// </summary>
    [Serializable]
    public class ActionCategory
    {
        public Guid ActionCategoryID { get; set; }
        [StringLength(6), Display(Name = "活动类型")]
        public string ActionType { get; set; } // 活动类型：包括引进、退出、变更
        [StringLength(16), Display(Name = "活动名称")]
        public string ActionName { get; set; } // 活动名称
        [Display(Name = "需要审批")]
        public bool NeedRequest { get; set; }
    }
    public class ActionCategoryConfiguration : EntityTypeConfiguration<ActionCategory>
    {
        public ActionCategoryConfiguration()
        {
            ToTable("ActionCategory", DB.Schema);
            HasKey(k => k.ActionCategoryID);
            Property(p => p.ActionCategoryID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
        }
    }


    /// <summary>
    /// 规划
    /// </summary>
    [Serializable]
    public class Programming
    {
        public Programming()
        {
            this.PlanAnnuals = new HashSet<Annual>();
        }

        public Guid ProgrammingID { get; set; }
        [StringLength(20), Display(Name = "规划期间")]
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Include]
        public virtual ICollection<Annual> PlanAnnuals { get; set; }
    }
    public class ProgrammingConfiguration : EntityTypeConfiguration<Programming>
    {
        public ProgrammingConfiguration()
        {
            ToTable("Programming", DB.Schema);
            HasKey(k => k.ProgrammingID);
            Property(p => p.ProgrammingID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            Property(p => p.StartDate).HasColumnType("datetime2");
            Property(p => p.EndDate).HasColumnType("datetime2");
        }
    }


    /// <summary>
    /// 年度
    /// </summary>
    [Serializable]
    public class Annual
    {
        public Annual()
        {
            this.Plans = new HashSet<Plan>();
        }
        public Guid AnnualID { get; set; }
        public Guid ProgrammingID { get; set; }
        [Display(Name = "年度")]
        public int Year { get; set; }
        [Display(Name = "打开/关闭")]
        public bool? IsOpen { get; set; }

        [Include]
        public virtual Programming Programming { get; set; }
        [Include]
        public virtual ICollection<Plan> Plans { get; set; }
    }
    public class AnnualConfiguration : EntityTypeConfiguration<Annual>
    {
        public AnnualConfiguration()
        {
            ToTable("Annual", DB.Schema);
            HasKey(k => k.AnnualID);
            Property(p => p.AnnualID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            HasRequired(p => p.Programming).WithMany(p => p.PlanAnnuals).HasForeignKey(p => p.ProgrammingID);
        }
    }


    /// <summary>
    /// 计划
    /// 通过版本管理有效性，通过状态（草稿、待审核、已审核、已提交、退回）管理计划的处理流程
    /// </summary>
    [Serializable]
    public class Plan
    {
        public Plan()
        {
            this.PlanHistories = new HashSet<PlanHistory>();
        }

        public Guid PlanID { get; set; }
        public Guid AirlinesID { get; set; }
        public Guid AnnualID { get; set; }
        [StringLength(200), Display(Name = "标题")]
        public string Title { get; set; }
        [Display(Name = "版本")]
        public int VersionNumber { get; set; }
        public bool IsCurrentVersion { get; set; } // 是否当前版本
        [Display(Name = "是否有效")]
        public bool IsValid { get; set; } // 计划是否有效，通过审核的计划均为有效。
        [Display(Name = "创建日期")]
        public DateTime? CreateDate { get; set; }
        [Display(Name = "提交日期")]
        public DateTime? SubmitDate { get; set; }
        [Display(Name = "是否完成")]
        public bool IsFinished { get; set; } // 计划是否完成评审流程，计划发送后设为完成。
        [Display(Name = "评审标记")]
        public bool? ManageFlagPnr { get; set; } // 客机评审标记，仅民航局评审时可用。缺省Null为待评审；False为需要重报；True为符合。
        [Display(Name = "评审标记")]
        public bool? ManageFlagCargo { get; set; } // 货机评审标记，仅民航局评审时可用。缺省Null为待评审；False为需要重报；True为符合。
        [StringLength(255), Display(Name = "评审备注")]
        public string ManageNote { get; set; }
        [StringLength(100), Display(Name = "计划文号")]
        public string DocNumber { get; set; }
        [StringLength(100), Display(Name = "计划文件名")]
        public string AttachDocFileName { get; set; }
        [Display(Name = "计划文档")]
        public byte[] AttachDoc { get; set; }

        [Display(Name = "计划编辑处理状态")]
        public int Status { get; set; } // 计划编辑处理状态：包括草稿、待审核、已审核、已提交、退回
        [Display(Name = "计划编辑处理状态")]
        public PlanStatus PlanStatus
        {
            get { return (PlanStatus)Status; }
            set { Status = (int)value; }
        }

        [Display(Name = "发布计划处理状态")]
        public int PublishStatus { get; set; } // 计划发布的处理状态：包括草稿、待审核、已审核、已提交
        [Display(Name = "发布计划处理状态")]
        public PlanPublishStatus PlanPublishStatus
        {
            get { return (PlanPublishStatus)PublishStatus; }
            set { PublishStatus = (int)value; }
        }

        [Include]
        public virtual Airlines Airlines { get; set; }
        [Include]
        public virtual Annual Annual { get; set; }
        [Include]
        public virtual ICollection<PlanHistory> PlanHistories { get; set; }
    }
    public class PlanConfiguration : EntityTypeConfiguration<Plan>
    {
        public PlanConfiguration()
        {
            ToTable("AircraftPlan", DB.Schema);
            HasKey(k => k.PlanID);
            Property(p => p.PlanID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            Property(p => p.CreateDate).HasColumnType("datetime2");
            Property(p => p.SubmitDate).HasColumnType("datetime2");

            HasRequired(a => a.Airlines).WithMany(a => a.Plans).HasForeignKey(a => a.AirlinesID);
            HasRequired(a => a.Annual).WithMany(a => a.Plans).HasForeignKey(a => a.AnnualID);
        }
    }


    /// <summary>
    /// 申请
    /// </summary>
    [Serializable]
    public class Request
    {
        public Request()
        {
            this.ApprovalHistories = new HashSet<ApprovalHistory>();
        }

        public Guid RequestID { get; set; }
        public Guid AirlinesID { get; set; }
        public Guid? ApprovalDocID { get; set; }
        [StringLength(100), Display(Name = "标题")]
        public string Title { get; set; } // 申请的标题
        [Display(Name = "创建日期")]
        public DateTime? CreateDate { get; set; } // 创建日期
        [Display(Name = "提交日期")]
        public DateTime? SubmitDate { get; set; } // 提交日期
        [Display(Name = "是否完成")]
        public bool IsFinished { get; set; } // 是否完成，申请中批准的项都完成后申请完成
        [Display(Name = "评审标记")]
        public bool? ManageFlag { get; set; } // 仅民航局评审时可用。缺省Null为待评审；False为需要重报；True为符合。
        [StringLength(100), Display(Name = "申请文号")]
        public string DocNumber { get; set; } // 航空公司申请的文号
        [StringLength(100), Display(Name = "申请文件名")]
        public string AttachDocFileName { get; set; } // 申请文档文件名
        [Display(Name = "申请文档")]
        public byte[] AttachDoc { get; set; } // 申请文档

        [Display(Name = "处理状态")]
        public int Status { get; set; } // 申请状态：包括草稿、待审核、已审核、已提交、已审批。
        [Display(Name = "处理状态")]
        public ReqStatus ReqStatus
        {
            get { return (ReqStatus)Status; }
            set { Status = (int)value; }
        }

        [Include]
        public virtual Airlines Airlines { get; set; }
        [Include]
        public virtual ApprovalDoc ApprovalDoc { get; set; }
        [Include]
        public virtual ICollection<ApprovalHistory> ApprovalHistories { get; set; }
    }
    public class RequestConfiguration : EntityTypeConfiguration<Request>
    {
        public RequestConfiguration()
        {
            ToTable("Request", DB.Schema);
            HasKey(k => k.RequestID);
            Property(p => p.RequestID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            Property(p => p.CreateDate).HasColumnType("datetime2");
            Property(p => p.SubmitDate).HasColumnType("datetime2");

            HasRequired(r => r.Airlines).WithMany(a => a.Requests).HasForeignKey(r => r.AirlinesID);
            HasOptional(r => r.ApprovalDoc).WithMany(a => a.Requests).HasForeignKey(r => r.ApprovalDocID);
        }
    }


    /// <summary>
    /// 批文
    /// </summary>
    [Serializable]
    public class ApprovalDoc
    {
        public ApprovalDoc()
        {
            this.Requests = new HashSet<Request>();
        }

        public Guid ApprovalDocID { get; set; }
        public Guid DispatchUnitID { get; set; }
        [Display(Name = "审批日期")]
        public DateTime? ExamineDate { get; set; } // 审批日期
        [StringLength(100), Display(Name = "批文文号")]
        public string ApprovalNumber { get; set; } // 批文文号
        [StringLength(100), Display(Name = "批文文件名")]
        public string ApprovalDocFileName { get; set; } // 批文文档文件名
        [Display(Name = "批文文档")]
        public byte[] AttachDoc { get; set; } // 批文文档

        [Display(Name = "处理状态")]
        public int Status { get; set; } // 处理状态：包括草稿、待审核、已审核、已提交。
        [Display(Name = "处理状态")]
        public OpStatus OpStatus
        {
            get { return (OpStatus)Status; }
            set { Status = (int)value; }
        }

        [Include]
        public virtual Manager Manager { get; set; }
        [Include]
        public virtual ICollection<Request> Requests { get; set; }
    }
    public class ApprovalDocConfiguration : EntityTypeConfiguration<ApprovalDoc>
    {
        public ApprovalDocConfiguration()
        {
            ToTable("ApprovalDoc", DB.Schema);
            HasKey(k => k.ApprovalDocID);
            Property(p => p.ApprovalDocID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            Property(p => p.ExamineDate).HasColumnType("datetime2");

            HasRequired(a => a.Manager).WithMany().HasForeignKey(a => a.DispatchUnitID);
        }
    }

    #endregion

}
