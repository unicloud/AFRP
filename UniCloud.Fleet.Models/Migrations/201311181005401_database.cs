namespace UniCloud.Fleet.Models.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class database : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Fleet.XmlConfig",
                c => new
                    {
                        XmlConfigID = c.Guid(nullable: false),
                        ConfigType = c.String(),
                        ConfigContent = c.String(storeType: "xml"),
                        VersionNumber = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.XmlConfigID);
            
            CreateTable(
                "Fleet.MailAddress",
                c => new
                    {
                        MailAddressID = c.Guid(nullable: false),
                        OwnerID = c.Guid(nullable: false),
                        SmtpHost = c.String(maxLength: 30),
                        Pop3Host = c.String(maxLength: 30),
                        SendPort = c.Int(nullable: false),
                        ReceivePort = c.Int(nullable: false),
                        LoginUser = c.String(maxLength: 100),
                        LoginPassword = c.String(maxLength: 50),
                        Address = c.String(maxLength: 100),
                        DisplayName = c.String(maxLength: 100),
                        SendSSL = c.Boolean(nullable: false),
                        StartTLS = c.Boolean(nullable: false),
                        ReceiveSSL = c.Boolean(nullable: false),
                        ServerType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MailAddressID)
                .ForeignKey("Fleet.Owner", t => t.OwnerID)
                .Index(t => t.OwnerID);
            
            CreateTable(
                "Fleet.Owner",
                c => new
                    {
                        OwnerID = c.Guid(nullable: false),
                        Name = c.String(maxLength: 200),
                        ShortName = c.String(maxLength: 100),
                        Description = c.String(maxLength: 200),
                        IsValid = c.Boolean(nullable: false),
                        SupplierType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.OwnerID);
            
            CreateTable(
                "Fleet.OwnershipHistory",
                c => new
                    {
                        OwnershipHistoryID = c.Guid(nullable: false),
                        AircraftID = c.Guid(nullable: false),
                        OwnerID = c.Guid(nullable: false),
                        StartDate = c.DateTime(storeType: "datetime2"),
                        EndDate = c.DateTime(storeType: "datetime2"),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.OwnershipHistoryID)
                .ForeignKey("Fleet.Aircraft", t => t.AircraftID)
                .ForeignKey("Fleet.Owner", t => t.OwnerID)
                .Index(t => t.AircraftID)
                .Index(t => t.OwnerID);
            
            CreateTable(
                "Fleet.Aircraft",
                c => new
                    {
                        AircraftID = c.Guid(nullable: false),
                        AircraftTypeID = c.Guid(nullable: false),
                        OwnerID = c.Guid(),
                        AirlinesID = c.Guid(nullable: false),
                        ImportCategoryID = c.Guid(nullable: false),
                        RegNumber = c.String(maxLength: 10),
                        SerialNumber = c.String(nullable: false, maxLength: 20),
                        CreateDate = c.DateTime(storeType: "datetime2"),
                        FactoryDate = c.DateTime(storeType: "datetime2"),
                        ImportDate = c.DateTime(storeType: "datetime2"),
                        ExportDate = c.DateTime(storeType: "datetime2"),
                        IsOperation = c.Boolean(nullable: false),
                        SeatingCapacity = c.Int(nullable: false),
                        CarryingCapacity = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.AircraftID)
                .ForeignKey("Fleet.AircraftType", t => t.AircraftTypeID)
                .ForeignKey("Fleet.Owner", t => t.OwnerID)
                .ForeignKey("Fleet.Airlines", t => t.AirlinesID)
                .ForeignKey("Fleet.ActionCategory", t => t.ImportCategoryID)
                .Index(t => t.AircraftTypeID)
                .Index(t => t.OwnerID)
                .Index(t => t.AirlinesID)
                .Index(t => t.ImportCategoryID);
            
            CreateTable(
                "Fleet.AircraftType",
                c => new
                    {
                        AircraftTypeID = c.Guid(nullable: false),
                        ManufacturerID = c.Guid(nullable: false),
                        AircraftCategoryID = c.Guid(nullable: false),
                        Name = c.String(maxLength: 16),
                        Description = c.String(maxLength: 200),
                    })
                .PrimaryKey(t => t.AircraftTypeID)
                .ForeignKey("Fleet.Manufacturer", t => t.ManufacturerID)
                .ForeignKey("Fleet.AircraftCategory", t => t.AircraftCategoryID)
                .Index(t => t.ManufacturerID)
                .Index(t => t.AircraftCategoryID);
            
            CreateTable(
                "Fleet.AircraftCategory",
                c => new
                    {
                        AircraftCategoryID = c.Guid(nullable: false),
                        Category = c.String(maxLength: 6),
                        Regional = c.String(maxLength: 30),
                    })
                .PrimaryKey(t => t.AircraftCategoryID);
            
            CreateTable(
                "Fleet.OperationHistory",
                c => new
                    {
                        OperationHistoryID = c.Guid(nullable: false),
                        AirlinesID = c.Guid(nullable: false),
                        AircraftID = c.Guid(nullable: false),
                        ImportCategoryID = c.Guid(nullable: false),
                        ExportCategoryID = c.Guid(),
                        RegNumber = c.String(maxLength: 10),
                        TechReceiptDate = c.DateTime(storeType: "datetime2"),
                        ReceiptDate = c.DateTime(storeType: "datetime2"),
                        StartDate = c.DateTime(storeType: "datetime2"),
                        StopDate = c.DateTime(storeType: "datetime2"),
                        TechDeliveryDate = c.DateTime(storeType: "datetime2"),
                        EndDate = c.DateTime(storeType: "datetime2"),
                        OnHireDate = c.DateTime(storeType: "datetime2"),
                        Note = c.String(maxLength: 200),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.OperationHistoryID)
                .ForeignKey("Fleet.ApprovalHistory", t => t.OperationHistoryID)
                .ForeignKey("Fleet.Airlines", t => t.AirlinesID)
                .ForeignKey("Fleet.Aircraft", t => t.AircraftID)
                .ForeignKey("Fleet.ActionCategory", t => t.ImportCategoryID)
                .ForeignKey("Fleet.ActionCategory", t => t.ExportCategoryID)
                .Index(t => t.OperationHistoryID)
                .Index(t => t.AirlinesID)
                .Index(t => t.AircraftID)
                .Index(t => t.ImportCategoryID)
                .Index(t => t.ExportCategoryID);
            
            CreateTable(
                "Fleet.ApprovalHistory",
                c => new
                    {
                        ApprovalHistoryID = c.Guid(nullable: false),
                        PlanAircraftID = c.Guid(nullable: false),
                        RequestID = c.Guid(nullable: false),
                        ImportCategoryID = c.Guid(nullable: false),
                        AirlinesID = c.Guid(nullable: false),
                        SeatingCapacity = c.Int(nullable: false),
                        CarryingCapacity = c.Decimal(nullable: false, precision: 18, scale: 2),
                        RequestDeliverAnnualID = c.Guid(nullable: false),
                        RequestDeliverMonth = c.Int(nullable: false),
                        IsApproved = c.Boolean(nullable: false),
                        Note = c.String(maxLength: 200),
                    })
                .PrimaryKey(t => t.ApprovalHistoryID)
                .ForeignKey("Fleet.Request", t => t.RequestID)
                .ForeignKey("Fleet.PlanAircraft", t => t.PlanAircraftID)
                .ForeignKey("Fleet.Annual", t => t.RequestDeliverAnnualID)
                .ForeignKey("Fleet.ActionCategory", t => t.ImportCategoryID)
                .ForeignKey("Fleet.Airlines", t => t.AirlinesID)
                .Index(t => t.RequestID)
                .Index(t => t.PlanAircraftID)
                .Index(t => t.RequestDeliverAnnualID)
                .Index(t => t.ImportCategoryID)
                .Index(t => t.AirlinesID);
            
            CreateTable(
                "Fleet.Request",
                c => new
                    {
                        RequestID = c.Guid(nullable: false),
                        AirlinesID = c.Guid(nullable: false),
                        ApprovalDocID = c.Guid(),
                        Title = c.String(maxLength: 100),
                        CreateDate = c.DateTime(storeType: "datetime2"),
                        SubmitDate = c.DateTime(storeType: "datetime2"),
                        IsFinished = c.Boolean(nullable: false),
                        ManageFlag = c.Boolean(),
                        DocNumber = c.String(maxLength: 100),
                        AttachDocFileName = c.String(maxLength: 100),
                        AttachDoc = c.Binary(),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.RequestID)
                .ForeignKey("Fleet.Airlines", t => t.AirlinesID)
                .ForeignKey("Fleet.ApprovalDoc", t => t.ApprovalDocID)
                .Index(t => t.AirlinesID)
                .Index(t => t.ApprovalDocID);
            
            CreateTable(
                "Fleet.ApprovalDoc",
                c => new
                    {
                        ApprovalDocID = c.Guid(nullable: false),
                        DispatchUnitID = c.Guid(nullable: false),
                        ExamineDate = c.DateTime(storeType: "datetime2"),
                        ApprovalNumber = c.String(maxLength: 100),
                        ApprovalDocFileName = c.String(maxLength: 100),
                        AttachDoc = c.Binary(),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ApprovalDocID)
                .ForeignKey("Fleet.Manager", t => t.DispatchUnitID)
                .Index(t => t.DispatchUnitID);
            
            CreateTable(
                "Fleet.ManaApprovalHistory",
                c => new
                    {
                        ApprovalHistoryID = c.Guid(nullable: false),
                        IsApproved = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ApprovalHistoryID)
                .ForeignKey("Fleet.ApprovalHistory", t => t.ApprovalHistoryID)
                .Index(t => t.ApprovalHistoryID);
            
            CreateTable(
                "Fleet.PlanAircraft",
                c => new
                    {
                        PlanAircraftID = c.Guid(nullable: false),
                        AircraftID = c.Guid(),
                        AircraftTypeID = c.Guid(nullable: false),
                        AirlinesID = c.Guid(nullable: false),
                        IsLock = c.Boolean(nullable: false),
                        IsOwn = c.Boolean(nullable: false),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PlanAircraftID)
                .ForeignKey("Fleet.Aircraft", t => t.AircraftID)
                .ForeignKey("Fleet.Airlines", t => t.AirlinesID)
                .ForeignKey("Fleet.AircraftType", t => t.AircraftTypeID)
                .Index(t => t.AircraftID)
                .Index(t => t.AirlinesID)
                .Index(t => t.AircraftTypeID);
            
            CreateTable(
                "Fleet.AircraftPlan",
                c => new
                    {
                        PlanID = c.Guid(nullable: false),
                        AirlinesID = c.Guid(nullable: false),
                        AnnualID = c.Guid(nullable: false),
                        Title = c.String(maxLength: 200),
                        VersionNumber = c.Int(nullable: false),
                        IsCurrentVersion = c.Boolean(nullable: false),
                        IsValid = c.Boolean(nullable: false),
                        CreateDate = c.DateTime(storeType: "datetime2"),
                        SubmitDate = c.DateTime(storeType: "datetime2"),
                        IsFinished = c.Boolean(nullable: false),
                        ManageFlagPnr = c.Boolean(),
                        ManageFlagCargo = c.Boolean(),
                        ManageNote = c.String(maxLength: 255),
                        DocNumber = c.String(maxLength: 100),
                        AttachDocFileName = c.String(maxLength: 100),
                        AttachDoc = c.Binary(),
                        Status = c.Int(nullable: false),
                        PublishStatus = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PlanID)
                .ForeignKey("Fleet.Airlines", t => t.AirlinesID)
                .ForeignKey("Fleet.Annual", t => t.AnnualID)
                .Index(t => t.AirlinesID)
                .Index(t => t.AnnualID);
            
            CreateTable(
                "Fleet.Annual",
                c => new
                    {
                        AnnualID = c.Guid(nullable: false),
                        ProgrammingID = c.Guid(nullable: false),
                        Year = c.Int(nullable: false),
                        IsOpen = c.Boolean(),
                    })
                .PrimaryKey(t => t.AnnualID)
                .ForeignKey("Fleet.Programming", t => t.ProgrammingID)
                .Index(t => t.ProgrammingID);
            
            CreateTable(
                "Fleet.Programming",
                c => new
                    {
                        ProgrammingID = c.Guid(nullable: false),
                        Name = c.String(maxLength: 20),
                        StartDate = c.DateTime(storeType: "datetime2"),
                        EndDate = c.DateTime(storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.ProgrammingID);
            
            CreateTable(
                "Fleet.ActionCategory",
                c => new
                    {
                        ActionCategoryID = c.Guid(nullable: false),
                        ActionType = c.String(maxLength: 6),
                        ActionName = c.String(maxLength: 16),
                        NeedRequest = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ActionCategoryID);
            
            CreateTable(
                "Fleet.PlanHistory",
                c => new
                    {
                        PlanHistoryID = c.Guid(nullable: false),
                        PlanAircraftID = c.Guid(),
                        PlanID = c.Guid(nullable: false),
                        ApprovalHistoryID = c.Guid(),
                        ActionCategoryID = c.Guid(nullable: false),
                        TargetCategoryID = c.Guid(nullable: false),
                        AircraftTypeID = c.Guid(nullable: false),
                        AirlinesID = c.Guid(nullable: false),
                        PerformAnnualID = c.Guid(nullable: false),
                        PerformMonth = c.Int(nullable: false),
                        SeatingCapacity = c.Int(nullable: false),
                        CarryingCapacity = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsValid = c.Boolean(nullable: false),
                        IsAdjust = c.Boolean(nullable: false),
                        Note = c.String(maxLength: 200),
                        IsSubmit = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.PlanHistoryID)
                .ForeignKey("Fleet.PlanAircraft", t => t.PlanAircraftID)
                .ForeignKey("Fleet.AircraftPlan", t => t.PlanID)
                .ForeignKey("Fleet.ActionCategory", t => t.ActionCategoryID)
                .ForeignKey("Fleet.ActionCategory", t => t.TargetCategoryID)
                .ForeignKey("Fleet.AircraftType", t => t.AircraftTypeID)
                .ForeignKey("Fleet.ApprovalHistory", t => t.ApprovalHistoryID)
                .ForeignKey("Fleet.Annual", t => t.PerformAnnualID)
                .ForeignKey("Fleet.Airlines", t => t.AirlinesID)
                .Index(t => t.PlanAircraftID)
                .Index(t => t.PlanID)
                .Index(t => t.ActionCategoryID)
                .Index(t => t.TargetCategoryID)
                .Index(t => t.AircraftTypeID)
                .Index(t => t.ApprovalHistoryID)
                .Index(t => t.PerformAnnualID)
                .Index(t => t.AirlinesID);
            
            CreateTable(
                "Fleet.AircraftBusiness",
                c => new
                    {
                        AircraftBusinessID = c.Guid(nullable: false),
                        AircraftID = c.Guid(nullable: false),
                        AircraftTypeID = c.Guid(nullable: false),
                        ImportCategoryID = c.Guid(nullable: false),
                        SeatingCapacity = c.Int(nullable: false),
                        CarryingCapacity = c.Decimal(nullable: false, precision: 18, scale: 2),
                        StartDate = c.DateTime(storeType: "datetime2"),
                        EndDate = c.DateTime(storeType: "datetime2"),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.AircraftBusinessID)
                .ForeignKey("Fleet.Aircraft", t => t.AircraftID)
                .ForeignKey("Fleet.AircraftType", t => t.AircraftTypeID)
                .ForeignKey("Fleet.ActionCategory", t => t.ImportCategoryID)
                .Index(t => t.AircraftID)
                .Index(t => t.AircraftTypeID)
                .Index(t => t.ImportCategoryID);
            
            CreateTable(
                "Fleet.AgreementDetail",
                c => new
                    {
                        AgreementDetailID = c.Guid(nullable: false),
                        AgreementID = c.Guid(nullable: false),
                        PlanAircraftID = c.Guid(nullable: false),
                        DeliveryRiskID = c.Guid(nullable: false),
                        ImportCategoryID = c.Guid(nullable: false),
                        SeatingCapacity = c.Int(nullable: false),
                        CarryingCapacity = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PlanDeliverAnnualID = c.Guid(),
                        PlanDeliverMonth = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.AgreementDetailID)
                .ForeignKey("Fleet.Agreement", t => t.AgreementID)
                .ForeignKey("Fleet.PlanAircraft", t => t.PlanAircraftID)
                .ForeignKey("Fleet.DeliveryRisk", t => t.DeliveryRiskID)
                .ForeignKey("Fleet.Annual", t => t.PlanDeliverAnnualID)
                .ForeignKey("Fleet.ActionCategory", t => t.ImportCategoryID)
                .Index(t => t.AgreementID)
                .Index(t => t.PlanAircraftID)
                .Index(t => t.DeliveryRiskID)
                .Index(t => t.PlanDeliverAnnualID)
                .Index(t => t.ImportCategoryID);
            
            CreateTable(
                "Fleet.Agreement",
                c => new
                    {
                        AgreementID = c.Guid(nullable: false),
                        SupplierID = c.Guid(nullable: false),
                        AirlinesID = c.Guid(nullable: false),
                        PlanAgreeAnnualID = c.Guid(),
                        PlanAgreeMonth = c.Int(nullable: false),
                        ActualAgreeAnnualID = c.Guid(),
                        ActualAgreeMonth = c.Int(nullable: false),
                        SignedDate = c.DateTime(storeType: "datetime2"),
                        Note = c.String(maxLength: 200),
                        Type = c.Int(nullable: false),
                        Phase = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.AgreementID)
                .ForeignKey("Fleet.Owner", t => t.SupplierID)
                .ForeignKey("Fleet.Annual", t => t.PlanAgreeAnnualID)
                .ForeignKey("Fleet.Annual", t => t.ActualAgreeAnnualID)
                .ForeignKey("Fleet.Airlines", t => t.AirlinesID)
                .Index(t => t.SupplierID)
                .Index(t => t.PlanAgreeAnnualID)
                .Index(t => t.ActualAgreeAnnualID)
                .Index(t => t.AirlinesID);
            
            CreateTable(
                "Fleet.DeliveryRisk",
                c => new
                    {
                        DeliveryRiskID = c.Guid(nullable: false),
                        Name = c.String(maxLength: 30),
                        Description = c.String(maxLength: 200),
                    })
                .PrimaryKey(t => t.DeliveryRiskID);
            
            CreateTable(
                "Fleet.SubOperationHistory",
                c => new
                    {
                        SubOperationHistoryID = c.Guid(nullable: false),
                        OperationHistoryID = c.Guid(nullable: false),
                        SubAirlinesID = c.Guid(nullable: false),
                        StartDate = c.DateTime(storeType: "datetime2"),
                        EndDate = c.DateTime(storeType: "datetime2"),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.SubOperationHistoryID)
                .ForeignKey("Fleet.OperationHistory", t => t.OperationHistoryID)
                .ForeignKey("Fleet.Airlines", t => t.SubAirlinesID)
                .Index(t => t.OperationHistoryID)
                .Index(t => t.SubAirlinesID);
            
            CreateTable(
                "Fleet.XmlSetting",
                c => new
                    {
                        XmlSettingID = c.Guid(nullable: false),
                        SettingType = c.String(),
                        SettingContent = c.String(storeType: "xml"),
                    })
                .PrimaryKey(t => t.XmlSettingID);
            
            CreateTable(
                "Fleet.DataSynchronous",
                c => new
                    {
                        DataSynchronousID = c.Guid(nullable: false),
                        OperationCode = c.String(),
                        Version = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.DataSynchronousID);
            
            CreateTable(
                "Fleet.OperationPlan",
                c => new
                    {
                        PlanHistoryID = c.Guid(nullable: false),
                        OperationHistoryID = c.Guid(),
                    })
                .PrimaryKey(t => t.PlanHistoryID)
                .ForeignKey("Fleet.PlanHistory", t => t.PlanHistoryID)
                .ForeignKey("Fleet.OperationHistory", t => t.OperationHistoryID)
                .Index(t => t.PlanHistoryID)
                .Index(t => t.OperationHistoryID);
            
            CreateTable(
                "Fleet.Manager",
                c => new
                    {
                        OwnerID = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.OwnerID)
                .ForeignKey("Fleet.Owner", t => t.OwnerID)
                .Index(t => t.OwnerID);
            
            CreateTable(
                "Fleet.Airlines",
                c => new
                    {
                        OwnerID = c.Guid(nullable: false),
                        MasterID = c.Guid(),
                        ICAOCode = c.String(maxLength: 3),
                        IATACode = c.String(maxLength: 2),
                        LevelCode = c.String(maxLength: 30),
                        IsShareData = c.Boolean(nullable: false),
                        IsCurrent = c.Boolean(nullable: false),
                        CreateDate = c.DateTime(storeType: "datetime2"),
                        LogoutDate = c.DateTime(storeType: "datetime2"),
                        OperationDate = c.DateTime(storeType: "datetime2"),
                        ExportDate = c.DateTime(storeType: "datetime2"),
                        SubType = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.OwnerID)
                .ForeignKey("Fleet.Owner", t => t.OwnerID)
                .ForeignKey("Fleet.Airlines", t => t.MasterID)
                .Index(t => t.OwnerID)
                .Index(t => t.MasterID);
            
            CreateTable(
                "Fleet.Manufacturer",
                c => new
                    {
                        OwnerID = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.OwnerID)
                .ForeignKey("Fleet.Owner", t => t.OwnerID)
                .Index(t => t.OwnerID);
            
            CreateTable(
                "Fleet.ChangePlan",
                c => new
                    {
                        PlanHistoryID = c.Guid(nullable: false),
                        AircraftBusinessID = c.Guid(),
                    })
                .PrimaryKey(t => t.PlanHistoryID)
                .ForeignKey("Fleet.PlanHistory", t => t.PlanHistoryID)
                .ForeignKey("Fleet.AircraftBusiness", t => t.AircraftBusinessID)
                .Index(t => t.PlanHistoryID)
                .Index(t => t.AircraftBusinessID);
            
        }
        
        public override void Down()
        {
            DropIndex("Fleet.ChangePlan", new[] { "AircraftBusinessID" });
            DropIndex("Fleet.ChangePlan", new[] { "PlanHistoryID" });
            DropIndex("Fleet.Manufacturer", new[] { "OwnerID" });
            DropIndex("Fleet.Airlines", new[] { "MasterID" });
            DropIndex("Fleet.Airlines", new[] { "OwnerID" });
            DropIndex("Fleet.Manager", new[] { "OwnerID" });
            DropIndex("Fleet.OperationPlan", new[] { "OperationHistoryID" });
            DropIndex("Fleet.OperationPlan", new[] { "PlanHistoryID" });
            DropIndex("Fleet.SubOperationHistory", new[] { "SubAirlinesID" });
            DropIndex("Fleet.SubOperationHistory", new[] { "OperationHistoryID" });
            DropIndex("Fleet.Agreement", new[] { "AirlinesID" });
            DropIndex("Fleet.Agreement", new[] { "ActualAgreeAnnualID" });
            DropIndex("Fleet.Agreement", new[] { "PlanAgreeAnnualID" });
            DropIndex("Fleet.Agreement", new[] { "SupplierID" });
            DropIndex("Fleet.AgreementDetail", new[] { "ImportCategoryID" });
            DropIndex("Fleet.AgreementDetail", new[] { "PlanDeliverAnnualID" });
            DropIndex("Fleet.AgreementDetail", new[] { "DeliveryRiskID" });
            DropIndex("Fleet.AgreementDetail", new[] { "PlanAircraftID" });
            DropIndex("Fleet.AgreementDetail", new[] { "AgreementID" });
            DropIndex("Fleet.AircraftBusiness", new[] { "ImportCategoryID" });
            DropIndex("Fleet.AircraftBusiness", new[] { "AircraftTypeID" });
            DropIndex("Fleet.AircraftBusiness", new[] { "AircraftID" });
            DropIndex("Fleet.PlanHistory", new[] { "AirlinesID" });
            DropIndex("Fleet.PlanHistory", new[] { "PerformAnnualID" });
            DropIndex("Fleet.PlanHistory", new[] { "ApprovalHistoryID" });
            DropIndex("Fleet.PlanHistory", new[] { "AircraftTypeID" });
            DropIndex("Fleet.PlanHistory", new[] { "TargetCategoryID" });
            DropIndex("Fleet.PlanHistory", new[] { "ActionCategoryID" });
            DropIndex("Fleet.PlanHistory", new[] { "PlanID" });
            DropIndex("Fleet.PlanHistory", new[] { "PlanAircraftID" });
            DropIndex("Fleet.Annual", new[] { "ProgrammingID" });
            DropIndex("Fleet.AircraftPlan", new[] { "AnnualID" });
            DropIndex("Fleet.AircraftPlan", new[] { "AirlinesID" });
            DropIndex("Fleet.PlanAircraft", new[] { "AircraftTypeID" });
            DropIndex("Fleet.PlanAircraft", new[] { "AirlinesID" });
            DropIndex("Fleet.PlanAircraft", new[] { "AircraftID" });
            DropIndex("Fleet.ManaApprovalHistory", new[] { "ApprovalHistoryID" });
            DropIndex("Fleet.ApprovalDoc", new[] { "DispatchUnitID" });
            DropIndex("Fleet.Request", new[] { "ApprovalDocID" });
            DropIndex("Fleet.Request", new[] { "AirlinesID" });
            DropIndex("Fleet.ApprovalHistory", new[] { "AirlinesID" });
            DropIndex("Fleet.ApprovalHistory", new[] { "ImportCategoryID" });
            DropIndex("Fleet.ApprovalHistory", new[] { "RequestDeliverAnnualID" });
            DropIndex("Fleet.ApprovalHistory", new[] { "PlanAircraftID" });
            DropIndex("Fleet.ApprovalHistory", new[] { "RequestID" });
            DropIndex("Fleet.OperationHistory", new[] { "ExportCategoryID" });
            DropIndex("Fleet.OperationHistory", new[] { "ImportCategoryID" });
            DropIndex("Fleet.OperationHistory", new[] { "AircraftID" });
            DropIndex("Fleet.OperationHistory", new[] { "AirlinesID" });
            DropIndex("Fleet.OperationHistory", new[] { "OperationHistoryID" });
            DropIndex("Fleet.AircraftType", new[] { "AircraftCategoryID" });
            DropIndex("Fleet.AircraftType", new[] { "ManufacturerID" });
            DropIndex("Fleet.Aircraft", new[] { "ImportCategoryID" });
            DropIndex("Fleet.Aircraft", new[] { "AirlinesID" });
            DropIndex("Fleet.Aircraft", new[] { "OwnerID" });
            DropIndex("Fleet.Aircraft", new[] { "AircraftTypeID" });
            DropIndex("Fleet.OwnershipHistory", new[] { "OwnerID" });
            DropIndex("Fleet.OwnershipHistory", new[] { "AircraftID" });
            DropIndex("Fleet.MailAddress", new[] { "OwnerID" });
            DropForeignKey("Fleet.ChangePlan", "AircraftBusinessID", "Fleet.AircraftBusiness");
            DropForeignKey("Fleet.ChangePlan", "PlanHistoryID", "Fleet.PlanHistory");
            DropForeignKey("Fleet.Manufacturer", "OwnerID", "Fleet.Owner");
            DropForeignKey("Fleet.Airlines", "MasterID", "Fleet.Airlines");
            DropForeignKey("Fleet.Airlines", "OwnerID", "Fleet.Owner");
            DropForeignKey("Fleet.Manager", "OwnerID", "Fleet.Owner");
            DropForeignKey("Fleet.OperationPlan", "OperationHistoryID", "Fleet.OperationHistory");
            DropForeignKey("Fleet.OperationPlan", "PlanHistoryID", "Fleet.PlanHistory");
            DropForeignKey("Fleet.SubOperationHistory", "SubAirlinesID", "Fleet.Airlines");
            DropForeignKey("Fleet.SubOperationHistory", "OperationHistoryID", "Fleet.OperationHistory");
            DropForeignKey("Fleet.Agreement", "AirlinesID", "Fleet.Airlines");
            DropForeignKey("Fleet.Agreement", "ActualAgreeAnnualID", "Fleet.Annual");
            DropForeignKey("Fleet.Agreement", "PlanAgreeAnnualID", "Fleet.Annual");
            DropForeignKey("Fleet.Agreement", "SupplierID", "Fleet.Owner");
            DropForeignKey("Fleet.AgreementDetail", "ImportCategoryID", "Fleet.ActionCategory");
            DropForeignKey("Fleet.AgreementDetail", "PlanDeliverAnnualID", "Fleet.Annual");
            DropForeignKey("Fleet.AgreementDetail", "DeliveryRiskID", "Fleet.DeliveryRisk");
            DropForeignKey("Fleet.AgreementDetail", "PlanAircraftID", "Fleet.PlanAircraft");
            DropForeignKey("Fleet.AgreementDetail", "AgreementID", "Fleet.Agreement");
            DropForeignKey("Fleet.AircraftBusiness", "ImportCategoryID", "Fleet.ActionCategory");
            DropForeignKey("Fleet.AircraftBusiness", "AircraftTypeID", "Fleet.AircraftType");
            DropForeignKey("Fleet.AircraftBusiness", "AircraftID", "Fleet.Aircraft");
            DropForeignKey("Fleet.PlanHistory", "AirlinesID", "Fleet.Airlines");
            DropForeignKey("Fleet.PlanHistory", "PerformAnnualID", "Fleet.Annual");
            DropForeignKey("Fleet.PlanHistory", "ApprovalHistoryID", "Fleet.ApprovalHistory");
            DropForeignKey("Fleet.PlanHistory", "AircraftTypeID", "Fleet.AircraftType");
            DropForeignKey("Fleet.PlanHistory", "TargetCategoryID", "Fleet.ActionCategory");
            DropForeignKey("Fleet.PlanHistory", "ActionCategoryID", "Fleet.ActionCategory");
            DropForeignKey("Fleet.PlanHistory", "PlanID", "Fleet.AircraftPlan");
            DropForeignKey("Fleet.PlanHistory", "PlanAircraftID", "Fleet.PlanAircraft");
            DropForeignKey("Fleet.Annual", "ProgrammingID", "Fleet.Programming");
            DropForeignKey("Fleet.AircraftPlan", "AnnualID", "Fleet.Annual");
            DropForeignKey("Fleet.AircraftPlan", "AirlinesID", "Fleet.Airlines");
            DropForeignKey("Fleet.PlanAircraft", "AircraftTypeID", "Fleet.AircraftType");
            DropForeignKey("Fleet.PlanAircraft", "AirlinesID", "Fleet.Airlines");
            DropForeignKey("Fleet.PlanAircraft", "AircraftID", "Fleet.Aircraft");
            DropForeignKey("Fleet.ManaApprovalHistory", "ApprovalHistoryID", "Fleet.ApprovalHistory");
            DropForeignKey("Fleet.ApprovalDoc", "DispatchUnitID", "Fleet.Manager");
            DropForeignKey("Fleet.Request", "ApprovalDocID", "Fleet.ApprovalDoc");
            DropForeignKey("Fleet.Request", "AirlinesID", "Fleet.Airlines");
            DropForeignKey("Fleet.ApprovalHistory", "AirlinesID", "Fleet.Airlines");
            DropForeignKey("Fleet.ApprovalHistory", "ImportCategoryID", "Fleet.ActionCategory");
            DropForeignKey("Fleet.ApprovalHistory", "RequestDeliverAnnualID", "Fleet.Annual");
            DropForeignKey("Fleet.ApprovalHistory", "PlanAircraftID", "Fleet.PlanAircraft");
            DropForeignKey("Fleet.ApprovalHistory", "RequestID", "Fleet.Request");
            DropForeignKey("Fleet.OperationHistory", "ExportCategoryID", "Fleet.ActionCategory");
            DropForeignKey("Fleet.OperationHistory", "ImportCategoryID", "Fleet.ActionCategory");
            DropForeignKey("Fleet.OperationHistory", "AircraftID", "Fleet.Aircraft");
            DropForeignKey("Fleet.OperationHistory", "AirlinesID", "Fleet.Airlines");
            DropForeignKey("Fleet.OperationHistory", "OperationHistoryID", "Fleet.ApprovalHistory");
            DropForeignKey("Fleet.AircraftType", "AircraftCategoryID", "Fleet.AircraftCategory");
            DropForeignKey("Fleet.AircraftType", "ManufacturerID", "Fleet.Manufacturer");
            DropForeignKey("Fleet.Aircraft", "ImportCategoryID", "Fleet.ActionCategory");
            DropForeignKey("Fleet.Aircraft", "AirlinesID", "Fleet.Airlines");
            DropForeignKey("Fleet.Aircraft", "OwnerID", "Fleet.Owner");
            DropForeignKey("Fleet.Aircraft", "AircraftTypeID", "Fleet.AircraftType");
            DropForeignKey("Fleet.OwnershipHistory", "OwnerID", "Fleet.Owner");
            DropForeignKey("Fleet.OwnershipHistory", "AircraftID", "Fleet.Aircraft");
            DropForeignKey("Fleet.MailAddress", "OwnerID", "Fleet.Owner");
            DropTable("Fleet.ChangePlan");
            DropTable("Fleet.Manufacturer");
            DropTable("Fleet.Airlines");
            DropTable("Fleet.Manager");
            DropTable("Fleet.OperationPlan");
            DropTable("Fleet.DataSynchronous");
            DropTable("Fleet.XmlSetting");
            DropTable("Fleet.SubOperationHistory");
            DropTable("Fleet.DeliveryRisk");
            DropTable("Fleet.Agreement");
            DropTable("Fleet.AgreementDetail");
            DropTable("Fleet.AircraftBusiness");
            DropTable("Fleet.PlanHistory");
            DropTable("Fleet.ActionCategory");
            DropTable("Fleet.Programming");
            DropTable("Fleet.Annual");
            DropTable("Fleet.AircraftPlan");
            DropTable("Fleet.PlanAircraft");
            DropTable("Fleet.ManaApprovalHistory");
            DropTable("Fleet.ApprovalDoc");
            DropTable("Fleet.Request");
            DropTable("Fleet.ApprovalHistory");
            DropTable("Fleet.OperationHistory");
            DropTable("Fleet.AircraftCategory");
            DropTable("Fleet.AircraftType");
            DropTable("Fleet.Aircraft");
            DropTable("Fleet.OwnershipHistory");
            DropTable("Fleet.Owner");
            DropTable("Fleet.MailAddress");
            DropTable("Fleet.XmlConfig");
        }
    }
}
