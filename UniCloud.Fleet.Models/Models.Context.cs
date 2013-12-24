using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Configuration;

namespace UniCloud.Fleet.Models
{
    public partial class FleetEntities : DbContext
    {
        
        static string connectStr = ConnectionStringCryptography.DecryptConnectionString(System.Configuration.ConfigurationManager.ConnectionStrings[Conn.Default].ToString());
        public FleetEntities()
            : base(connectStr)
        {
            Database.SetInitializer<FleetEntities>(null);
           
        }

        public FleetEntities(string conn)
            : base(conn)
        {
            Database.SetInitializer<FleetEntities>(null);
        }

        #region 映射到数据库的实体集

        // 基础配置
        public DbSet<XmlConfig> XmlConfigs { get; set; }
        public DbSet<MailAddress> MailAddresses { get; set; }
        // 参与者
        public DbSet<Owner> Owners { get; set; }
        // 飞机
        public DbSet<AircraftCategory> AircraftCategories { get; set; }
        public DbSet<AircraftType> AircraftTypes { get; set; }
        public DbSet<Aircraft> Aircrafts { get; set; }
        public DbSet<PlanAircraft> PlanAircrafts { get; set; }
        public DbSet<OwnershipHistory> OwnershipHistories { get; set; }
        public DbSet<PlanHistory> PlanHistories { get; set; }
        public DbSet<ApprovalHistory> ApprovalHistories { get; set; }
        public DbSet<ManaApprovalHistory> ManaApprovalHistories { get; set; }
        public DbSet<Agreement> Agreements { get; set; }
        public DbSet<DeliveryRisk> DeleveryRisks { get; set; }
        public DbSet<AgreementDetail> AgreementDetails { get; set; }
        public DbSet<OperationHistory> OperationHistories { get; set; }
        public DbSet<SubOperationHistory> SubOperationHistories { get; set; }
        public DbSet<AircraftBusiness> AircraftBusinesses { get; set; }
        // 机队管理
        public DbSet<ActionCategory> ActionCategories { get; set; }
        public DbSet<Programming> Programmings { get; set; }
        public DbSet<Annual> Annuals { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<ApprovalDoc> ApprovalDocs { get; set; }
        public DbSet<OperationPlan> OperationPlans { get; set; }

        #endregion

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // 移除EF的表名公约
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            //移除多对多级联删除公约
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            //移除一对多级联删除公约
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            modelBuilder.Configurations
                .Add(new XmlConfigConfiguration())
                .Add(new XmlSettingConfiguration())
                .Add(new MailAddressConfiguration())
                .Add(new OwnerConfiguration())
                .Add(new ManagerConfiguration())
                .Add(new AirlinesConfiguration())
                .Add(new ManufacturerConfiguration())
                .Add(new AircraftCategoryConfiguration())
                .Add(new AircraftTypeConfiguration())
                .Add(new AircraftConfiguration())
                .Add(new PlanAircraftConfiguration())
                .Add(new OwnershipHistoryConfiguration())
                .Add(new PlanHistoryConfiguration())
                .Add(new OperationPlanConfiguration())
                .Add(new ChangePlanConfiguration())
                .Add(new ApprovalHistoryConfiguration())
                .Add(new ManaApprovalHistoryConfiguration())
                .Add(new AgreementConfiguration())
                .Add(new DeliveryRiskConfiguration())
                .Add(new AgreementDetailConfiguration())
                .Add(new OperationHistoryConfiguration())
                .Add(new SubOperationHistoryConfiguration())
                .Add(new DataSynchronousConfiguration())
                .Add(new AircraftBusinessConfiguration())
                .Add(new ActionCategoryConfiguration())
                .Add(new ProgrammingConfiguration())
                .Add(new AnnualConfiguration())
                .Add(new PlanConfiguration())
                .Add(new RequestConfiguration())
                .Add(new ApprovalDocConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}