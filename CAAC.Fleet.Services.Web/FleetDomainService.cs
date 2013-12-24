using System;
using System.Data.Entity;
using System.Linq;
using System.ServiceModel.DomainServices.EntityFramework;
using System.ServiceModel.DomainServices.Hosting;
using System.ServiceModel.DomainServices.Server;
using UniCloud.Fleet.Models;
using UniCloud.Fleet.XmlConfigs;
//using UniCloud.DatabaseHelper;

namespace CAAC.Fleet.Services.Web
{
    [EnableClientAccess]
    public class FleetDomainService : DbDomainService<FleetEntities>
    {

        #region 基础配置

        /// 应用基础配置
        public IQueryable<XmlConfig> GetXmlConfig()
        {
            return this.DbContext.XmlConfigs;
        }

        [Insert]
        public void InsertXmlConfig(XmlConfig currentXmlConfig)
        {
            this.DbContext.Insert<XmlConfig>(currentXmlConfig);
        }
        [Update]
        public void UpdateXmlConfig(XmlConfig currentXmlConfig)
        {
            this.DbContext.Update<XmlConfig>(currentXmlConfig);
        }
        [Delete]
        public void DeleteXmlConfig(XmlConfig currentXmlConfig)
        {
            this.DbContext.Delete<XmlConfig>(currentXmlConfig);
        }

        /// 邮箱地址
        public IQueryable<MailAddress> GetMailAddress()
        {
            return this.DbContext.MailAddresses.OrderBy(m => m.MailAddressID);
        }

        [Insert]
        public void InsertMailAddress(MailAddress currentMailAddress)
        {
            this.DbContext.Insert<MailAddress>(currentMailAddress);
        }
        [Update]
        public void UpdateMailAddress(MailAddress currentMailAddress)
        {
            this.DbContext.Update<MailAddress>(currentMailAddress);
        }
        [Delete]
        public void DeleteMailAddress(MailAddress currentMailAddress)
        {
            this.DbContext.Delete<MailAddress>(currentMailAddress);
        }

        #endregion

        #region 参与者

        /// 所有权人
        public IQueryable<Owner> GetOwner()
        {
            return this.DbContext.Owners.OrderBy(o => o.OwnerID);
        }

        [Insert]
        public void InsertOwer(Owner currentOwner)
        {
            this.DbContext.Insert<Owner>(currentOwner);
        }
        [Update]
        public void UpdateOwner(Owner currentOwner)
        {
            this.DbContext.Update<Owner>(currentOwner);
        }
        [Delete]
        public void DeleteOwner(Owner currentOwner)
        {
            this.DbContext.Delete<Owner>(currentOwner);
        }

        /// 管理者
        /// 包括民航局、发改委等
        public IQueryable<Manager> GetManager()
        {
            return this.DbContext.Owners.OfType<Manager>().OrderBy(r => r.OwnerID);
        }

        /// 航空公司
        /// 即是运营人，也通常是所有权人
        public IQueryable<Airlines> GetAirLines()
        {
            return this.DbContext.Owners.OfType<Airlines>().OrderBy(r => r.OwnerID);
        }

        [Insert]
        public void InsertAirline(Airlines currentAirline)
        {
            this.DbContext.Insert<Airlines>(currentAirline);
        }
        [Update]
        public void UpdateAirline(Airlines currentAirline)
        {
            this.DbContext.Update<Airlines>(currentAirline);
        }
        [Delete]
        public void DeleteAirline(Airlines currentAirline)
        {
            this.DbContext.Delete<Airlines>(currentAirline);
        }

        /// 制造商
        public IQueryable<Manufacturer> GetManufacturer()
        {
            return this.DbContext.Owners.OfType<Manufacturer>().OrderBy(r => r.OwnerID);
        }

        #endregion

        #region 飞机

        /// 飞机类别
        public IQueryable<AircraftCategory> GetAircraftCategory()
        {
            return this.DbContext.AircraftCategories.OrderByDescending(a => a.Category).ThenBy(a => a.Regional);
        }

        [Insert]
        public void InsertAircraftCategory(AircraftCategory currentAircraftCategory)
        {
            this.DbContext.Insert<AircraftCategory>(currentAircraftCategory);
        }
        [Update]
        public void UpdateAircraftCategory(AircraftCategory currentAircraftCategory)
        {
            this.DbContext.Update<AircraftCategory>(currentAircraftCategory);
        }
        [Delete]
        public void DeleteAircraftCategory(AircraftCategory currentAircraftCategory)
        {
            this.DbContext.Delete<AircraftCategory>(currentAircraftCategory);
        }

        /// 机型
        public IQueryable<AircraftType> GetAircraftType()
        {
            return this.DbContext.AircraftTypes.OrderBy(a => a.Name);
        }

        [Insert]
        public void InsertAircraftType(AircraftType currentAircraftType)
        {
            this.DbContext.Insert<AircraftType>(currentAircraftType);
        }
        [Update]
        public void UpdateAircraftType(AircraftType currentAircraftType)
        {
            this.DbContext.Update<AircraftType>(currentAircraftType);
        }
        [Delete]
        public void DeleteAircraftType(AircraftType currentAircraftType)
        {
            this.DbContext.Delete<AircraftType>(currentAircraftType);
        }

        /// 飞机
        public IQueryable<Aircraft> GetAircraft()
        {
            return this.DbContext.Aircrafts.Include(p => p.OperationHistories).Include(p => p.AircraftBusinesses).Include(p => p.OwnershipHistorys).OrderBy(o => o.AircraftID);
        }

        [Insert]
        public void InsertAircraft(Aircraft currentAircraft)
        {
            currentAircraft.CreateDate = DateTime.Now;
            this.DbContext.Insert<Aircraft>(currentAircraft);
        }
        [Update]
        public void UpdateAircraft(Aircraft currentAircraft)
        {
            this.DbContext.Update<Aircraft>(currentAircraft);
        }
        [Delete]
        public void DeleteAircraft(Aircraft currentAircraft)
        {
            this.DbContext.Delete<Aircraft>(currentAircraft);
        }

        /// 计划飞机
        public IQueryable<PlanAircraft> GetPlanAircraft()
        {
            return this.DbContext.PlanAircrafts.OrderBy(p => p.PlanAircraftID);
        }

        [Insert]
        public void InsertPlanAircraft(PlanAircraft currentPlanAircraft)
        {
            this.DbContext.Insert<PlanAircraft>(currentPlanAircraft);
        }
        [Update]
        public void UpdatePlanAircraft(PlanAircraft currentPlanAircraft)
        {
            this.DbContext.Update<PlanAircraft>(currentPlanAircraft);
        }
        [Delete]
        public void DeletePlanAircraft(PlanAircraft currentPlanAircraft)
        {
            this.DbContext.Delete<PlanAircraft>(currentPlanAircraft);
        }

        /// 所有权历史
        [Insert]
        public void InsertOwnershipHistory(OwnershipHistory currentOwnershipHistory)
        {
            this.DbContext.Insert<OwnershipHistory>(currentOwnershipHistory);
        }
        [Update]
        public void UpdateOwnershipHistory(OwnershipHistory currentOwnershipHistory)
        {
            this.DbContext.Update<OwnershipHistory>(currentOwnershipHistory);
        }
        [Delete]
        public void DeleteOwnershipHistory(OwnershipHistory currentOwnershipHistory)
        {
            this.DbContext.Delete<OwnershipHistory>(currentOwnershipHistory);
        }

        /// 计划历史
        public IQueryable<PlanHistory> GetPlanHistory()
        {
            return this.DbContext.PlanHistories.Include(p => p.Plan.Annual).OrderBy(ob => ob.Annual.Year);
        }

        [Insert]
        public void InsertPlanHistory(PlanHistory currentPlanHistory)
        {
            this.DbContext.Insert<PlanHistory>(currentPlanHistory);
        }
        [Update]
        public void UpdatePlanHistory(PlanHistory currentPlanHistory)
        {
            this.DbContext.Update<PlanHistory>(currentPlanHistory);
        }
        [Delete]
        public void DeletePlanHistory(PlanHistory currentPlanHistory)
        {
            this.DbContext.Delete<PlanHistory>(currentPlanHistory);
        }

        [Insert]
        public void InsertOperationPlan(OperationPlan currentOperationPlan)
        {
            this.DbContext.Insert<OperationPlan>(currentOperationPlan);
        }
        [Update]
        public void UpdateOperationPlan(OperationPlan currentOperationPlan)
        {
            this.DbContext.Update<OperationPlan>(currentOperationPlan);
        }
        [Delete]
        public void DeleteOperationPlan(OperationPlan currentOperationPlan)
        {
            this.DbContext.Delete<OperationPlan>(currentOperationPlan);
        }

        [Insert]
        public void InsertChangePlan(ChangePlan currentChangePlan)
        {
            this.DbContext.Insert<ChangePlan>(currentChangePlan);
        }
        [Update]
        public void UpdateChangePlan(ChangePlan currentChangePlan)
        {
            this.DbContext.Update<ChangePlan>(currentChangePlan);
        }
        [Delete]
        public void DeleteChangePlan(ChangePlan currentChangePlan)
        {
            this.DbContext.Delete<ChangePlan>(currentChangePlan);
        }

        /// 审批历史
        public IQueryable<ApprovalHistory> GetApprovalHistory()
        {
            return this.DbContext.ApprovalHistories.Include(p => p.ManaApprovalHistory).OrderBy(o => o.PlanAircraftID);
        }

        [Insert]
        public void InsertApprovalHistory(ApprovalHistory currentApprovalHistory)
        {
            this.DbContext.Insert<ApprovalHistory>(currentApprovalHistory);
        }
        [Update]
        public void UpdateApprovalHistory(ApprovalHistory currentApprovalHistory)
        {
            this.DbContext.Update<ApprovalHistory>(currentApprovalHistory);
        }
        [Delete]
        public void DeleteApprovalHistory(ApprovalHistory currentApprovalHistory)
        {
            this.DbContext.Delete<ApprovalHistory>(currentApprovalHistory);
        }

        /// 管理批文历史
        /// 仅由民航局使用，由民航局业务人员录入


        /// 协议汇总


        /// 交付风险


        /// 协议明细


        /// 所有权历史
        public IQueryable<OwnershipHistory> GetOwnershipHistory()
        {
            return this.DbContext.OwnershipHistories.OrderBy(o => o.OwnershipHistoryID);
        }

        /// 运营权历史
        public IQueryable<OperationHistory> GetOperationHistory()
        {
            return this.DbContext.OperationHistories.OrderBy(o => o.OperationHistoryID);
        }

        [Insert]
        public void InsertOperationHistory(OperationHistory currentOperationHistory)
        {
            this.DbContext.Insert<OperationHistory>(currentOperationHistory);
        }
        [Update]
        public void UpdateOperationHistory(OperationHistory currentOperationHistory)
        {
            this.DbContext.Update<OperationHistory>(currentOperationHistory);
        }
        [Delete]
        public void DeleteOperationHistory(OperationHistory currentOperationHistory)
        {
            this.DbContext.Delete<OperationHistory>(currentOperationHistory);
        }

        /// 飞机商业数据历史
        [Query]
        public IQueryable<AircraftBusiness> GetAircraftBusiness()
        {
            return this.DbContext.AircraftBusinesses.OrderBy(o => o.StartDate);
        }

        [Insert]
        public void InsertAircraftBusiness(AircraftBusiness currentAircraftBusiness)
        {
            this.DbContext.Insert<AircraftBusiness>(currentAircraftBusiness);
        }
        [Update]
        public void UpdateAircraftBusiness(AircraftBusiness currentAircraftBusiness)
        {
            this.DbContext.Update<AircraftBusiness>(currentAircraftBusiness);
        }
        [Delete]
        public void DeleteAircraftBusiness(AircraftBusiness currentAircraftBusiness)
        {
            this.DbContext.Delete<AircraftBusiness>(currentAircraftBusiness);
        }

        #endregion

        #region 机队管理

        /// 操作类别
        public IQueryable<ActionCategory> GetActionCategory()
        {
            return this.DbContext.ActionCategories.OrderByDescending(a => a.ActionType).ThenBy(a => a.ActionName);
        }

        [Insert]
        public void InsertActionCategory(ActionCategory currentActionCategory)
        {
            this.DbContext.Insert<ActionCategory>(currentActionCategory);
        }
        [Update]
        public void UpdateActionCategory(ActionCategory currentActionCategory)
        {
            this.DbContext.Update<ActionCategory>(currentActionCategory);
        }
        [Delete]
        public void DeleteActionCategory(ActionCategory currentActionCategory)
        {
            this.DbContext.Delete<ActionCategory>(currentActionCategory);
        }

        /// 规划


        #region 计划

        /// 年度
        public IQueryable<Annual> GetAnnual()
        {
            return this.DbContext.Annuals.Include(a => a.Programming).OrderByDescending(a => a.Year);
        }

        [Insert]
        public void InserAnnual(Annual currentAnnual)
        {
            this.DbContext.Insert<Annual>(currentAnnual);
        }
        [Update]
        public void UpdateAnnual(Annual currentAnnual)
        {
            this.DbContext.Update<Annual>(currentAnnual);
        }
        [Delete]
        public void DeleteAnnual(Annual currentAnnual)
        {
            this.DbContext.Delete<Annual>(currentAnnual);
        }


        /// 计划
        /// 通过版本管理有效性，通过状态（草稿、待审核、已审核、已提交、退回）管理计划的处理流程
        public IQueryable<Plan> GetPlan()
        {
            return this.DbContext.Plans.Include(p => p.PlanHistories).OrderByDescending(p => p.Annual.Year);
        }

        [Insert]
        public void InserPlan(Plan currentPlan)
        {
            currentPlan.CreateDate = DateTime.Now;
            this.DbContext.Insert<Plan>(currentPlan);
        }
        [Update]
        public void UpdatePlan(Plan currentPlan)
        {
            this.DbContext.Update<Plan>(currentPlan);
        }
        [Delete]
        public void DeletePlan(Plan currentPlan)
        {
            this.DbContext.Delete<Plan>(currentPlan);
        }

        #endregion

        #region 申请

        /// 申请
        public IQueryable<Request> GetRequest()
        {
            return this.DbContext.Requests
                .Include(r => r.ApprovalHistories)
                .OrderBy(r => r.RequestID);
        }

        [Insert]
        public void InsertRequest(Request currentRequest)
        {
            currentRequest.CreateDate = DateTime.Now;
            this.DbContext.Insert<Request>(currentRequest);
        }
        [Update]
        public void UpdateRequest(Request currentRequest)
        {
            this.DbContext.Update<Request>(currentRequest);
        }
        [Delete]
        public void DeleteRequest(Request currentRequest)
        {
            this.DbContext.Delete<Request>(currentRequest);
        }

        /// 批文
        public IQueryable<ApprovalDoc> GetApprovalDoc()
        {
            return this.DbContext.ApprovalDocs.Include(a => a.Requests).OrderBy(a => a.ApprovalDocID);
        }

        [Insert]
        public void InsertApprovalDoc(ApprovalDoc currentApprovalDoc)
        {
            this.DbContext.Insert<ApprovalDoc>(currentApprovalDoc);
        }
        [Update]
        public void UpdateApprovalDoc(ApprovalDoc currentApprovalDoc)
        {
            this.DbContext.Update<ApprovalDoc>(currentApprovalDoc);
        }
        [Delete]
        public void DeleteApprovalDoc(ApprovalDoc currentApprovalDoc)
        {
            this.DbContext.Delete<ApprovalDoc>(currentApprovalDoc);
        }

        /// 管理的批文
        public IQueryable<ManaApprovalHistory> GetManaApprovalHistory()
        {
            return this.DbContext.ManaApprovalHistories;
        }

        [Insert]
        public void InsertManaApprovalHistory(ManaApprovalHistory currentApprovalHistory)
        {
            this.DbContext.Insert<ManaApprovalHistory>(currentApprovalHistory);
        }
        [Update]
        public void UpdateManaApprovalHistory(ManaApprovalHistory currentApprovalHistory)
        {
            this.DbContext.Update<ManaApprovalHistory>(currentApprovalHistory);
        }
        [Delete]
        public void DeleteManaApprovalHistory(ManaApprovalHistory currentApprovalHistory)
        {
            this.DbContext.Delete<ManaApprovalHistory>(currentApprovalHistory);
        }

        /// 民航局批文


        /// 发改委批文



        #endregion


        #region "查询XmlConfig"


        [Invoke]
        public int UpdateXmlConfigContent(string XmlConfigType)
        {
            XmlConfigService _XmlService = new XmlConfigService(this.DbContext, false);
            _XmlService.UpdateXmlConfigContent(XmlConfigType);
            return 1;
        }

        [Invoke]
        public int UpdateAllXmlConfigFlag()
        {
            XmlConfigService _XmlService = new XmlConfigService(this.DbContext, false);
            _XmlService.UpdateAllXmlConfigFlag();
            return 1;
        }

        #endregion


        #region "数据库备份与恢复"
        [Invoke]
        public bool BackupDataBase(string DatabaseName, string FilePath, string FileName)
        {
            //Databasehelper dbHelper = new Databasehelper();
            return false;//dbHelper.BackupDataBase(DatabaseName, FilePath, FileName);
        }
        [Invoke]
        public bool RestoreDataBase(string DatabaseName, string FilePath, string FileName)
        {
            //Databasehelper dbHelper = new Databasehelper();
            return false;//dbHelper.RestoreDataBase(DatabaseName, FilePath, FileName);
        }
        #endregion

        #endregion

    }
}