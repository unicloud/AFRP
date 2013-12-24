using System;
using System.Data.Entity;
using System.Linq;
using System.ServiceModel.DomainServices.EntityFramework;
using System.ServiceModel.DomainServices.Hosting;
using System.ServiceModel.DomainServices.Server;
using UniCloud.Fleet.Models;
using UniCloud.Fleet.XmlConfigs;
using UniCloud.Mail;

namespace UniCloud.Fleet.Services.Web
{
    [EnableClientAccess]
    public class FleetDomainService : DbDomainService<FleetEntities>
    {
        #region 基础配置

        /// 应用基础配置
        public IQueryable<XmlConfig> GetXmlConfig()
        {
            return this.DbContext.XmlConfigs.OrderBy(x => x.VersionNumber);
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
            return this.DbContext.MailAddresses.OrderBy(m => m.Address);
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
            return this.DbContext.Owners.OrderBy(o => o.Name);
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
            return this.DbContext.Owners.OfType<Manager>().OrderBy(r => r.Name);
        }

        /// 航空公司
        /// 即是运营人，也通常是所有权人
        public IQueryable<Airlines> GetAirLines()
        {
            return this.DbContext.Owners.OfType<Airlines>().OrderBy(r => r.Name);
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
            return this.DbContext.Owners.OfType<Manufacturer>().OrderBy(r => r.Name);
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
            return
                this.DbContext.Aircrafts.OrderByDescending(a => a.FactoryDate)
                    .Include(p => p.OperationHistories.Select(s => s.SubOperationHistories))
                    .Include(p => p.AircraftBusinesses)
                    .Include(p => p.OwnershipHistorys);
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
            return this.DbContext.PlanAircrafts.OrderBy(p => p.Status);
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
        public IQueryable<OwnershipHistory> GetOwnershipHistory()
        {
            return this.DbContext.OwnershipHistories.OrderBy(o => o.StartDate);
        }

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
            return this.DbContext.PlanHistories.OrderBy(ob => ob.Annual.Year).Include(p => p.Plan.Annual);
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
            return this.DbContext.ApprovalHistories.OrderBy(o => o.Request.CreateDate);
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


        /// 运营权历史
        public IQueryable<OperationHistory> GetOperationHistory()
        {
            return this.DbContext.OperationHistories.Include(o => o.SubOperationHistories).OrderBy(o => o.StartDate);
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

        [Insert]
        public void InsertSubOperationHistory(SubOperationHistory currentSubOperationHistory)
        {
            this.DbContext.Insert<SubOperationHistory>(currentSubOperationHistory);
        }
        [Update]
        public void UpdateSubOperationHistory(SubOperationHistory currentSubOperationHistory)
        {
            this.DbContext.Update<SubOperationHistory>(currentSubOperationHistory);
        }
        [Delete]
        public void DeleteSubOperationHistory(SubOperationHistory currentSubOperationHistory)
        {
            this.DbContext.Delete<SubOperationHistory>(currentSubOperationHistory);
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
            return this.DbContext.Annuals.OrderByDescending(a => a.Year).Include(a => a.Programming);
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
            return
                this.DbContext.Plans.OrderBy(p => p.Annual.Year)
                    .ThenBy(p => p.VersionNumber)
                    .Include(p => p.PlanHistories);
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
            return this.DbContext.Requests.OrderBy(r => r.CreateDate).Include(r => r.ApprovalHistories);
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
            return this.DbContext.ApprovalDocs.OrderBy(a => a.ExamineDate).Include(a => a.Requests);
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


        #endregion

        #endregion

        #region 数据传输

        /// 民航局批文
        /// 发改委批文
        /// <summary>
        /// 通过邮件发送数据文件的方法
        /// </summary>
        /// <param name="currentAirlines"></param>
        /// <param name="obj">发送的对象</param>
        /// <param name="mailSubject">发送邮件的主题</param>
        /// <param name="attName">附件的名称</param>
        [Ignore]
        private bool TransferMail(Guid currentAirlines, object obj, string mailSubject, string attName)
        {

            var sender = MailAccountHelper.GetMailAccount(currentAirlines, this.DbContext);
            var receiver = MailAccountHelper.GetMailAccount(Guid.Parse(GlobalConst.CAACID), this.DbContext);
            if (sender == null)
            {
                return false;
            }
            if (receiver == null)
            {
                return false;
            }
            //邮件中增加航空公司信息
            var airlines = this.DbContext.Owners.FirstOrDefault(q => q.OwnerID == currentAirlines);
            if (airlines != null)
            {
                mailSubject = airlines.Name + "发送" + mailSubject;
            }
            //发送
            var sm = new SendMail();
            var blSend = sm.SendEntity(sender, receiver, obj, mailSubject, attName);
            if (blSend == -1)
            {
                return sm.SendEntity(sender, receiver, obj, mailSubject, attName) == 0;
            }
            return blSend == 0;
        }

        /// <summary>
        /// 传输申请
        /// </summary>
        /// <param name="currentAirlines"></param>
        /// <param name="currentRequest"></param>
        [Invoke]
        public bool TransferRequest(Guid currentAirlines, Guid currentRequest)
        {
            // 获取需要发送的对象
            var obj = this.DbContext.Requests.Where(r => r.RequestID == currentRequest).
                Include(r => r.ApprovalHistories).
                Include(r => r.ApprovalHistories.Select(q => q.PlanAircraft)).
                Include(r => r.ApprovalHistories.Select(q => q.PlanAircraft.Aircraft)).FirstOrDefault();
            return obj != null && TransferMail(currentAirlines, obj, obj.Title, "Request");
        }

        /// <summary>
        /// 传输计划
        /// </summary>
        /// <param name="currentAirlines"></param>
        /// <param name="currentPlan"></param>
        [Invoke]
        public bool TransferPlan(Guid currentAirlines, Guid currentPlan)
        {
            // 获取需要发送的对象
            var obj = this.DbContext.Plans.Where(p => p.PlanID == currentPlan).
                Include(p => p.PlanHistories).
                Include(p => p.PlanHistories.Select(q => q.PlanAircraft)).
                Include(p => p.PlanHistories.Select(q => q.PlanAircraft.Aircraft)).
                FirstOrDefault();
            return obj != null && TransferMail(currentAirlines, obj, obj.Title, "Plan");
        }
        /// <summary>
        /// 传输计划申请
        /// </summary>
        /// <param name="currentAirlines"></param>
        /// <param name="currentPlan"></param>
        /// <param name="currentRequest"></param>
        /// <returns></returns>
        [Invoke]
        public bool TransferPlanAndRequest(Guid currentAirlines, Guid currentPlan, Guid currentRequest)
        {
            // 获取需要发送的对象
            return TransferPlan(currentAirlines, currentPlan) && TransferRequest(currentAirlines, currentRequest);
        }

        /// <summary>
        /// 传输批文
        /// </summary>
        /// <param name="currentAirlines"></param>
        /// <param name="currentApprovalDoc"></param>
        [Invoke]
        public bool TransferApprovalDoc(Guid currentAirlines, Guid currentApprovalDoc)
        {
            // 获取需要发送的对象
            var obj = this.DbContext.ApprovalDocs.Where(a => a.ApprovalDocID == currentApprovalDoc).
                Include(a => a.Requests).Include(a => a.Requests.Select(q => q.ApprovalHistories)).
                Include(a => a.Requests.Select(r => r.ApprovalHistories.Select(p => p.PlanAircraft))).
                Include(a => a.Requests.Select(r => r.ApprovalHistories.Select(p => p.PlanAircraft.Aircraft)))
                .FirstOrDefault();
            return TransferMail(currentAirlines, obj, "批文", "ApprovalDoc");
        }

        /// <summary>
        /// 传输运营历史
        /// </summary>
        /// <param name="currentAirlines"></param>
        /// <param name="currentOperationHistory"></param>
        /// <returns></returns>
        [Invoke]
        public bool TransferOperationHistory(Guid currentAirlines, Guid currentOperationHistory)
        {
            // 获取需要发送的对象
            var obj = this.DbContext.OperationHistories.Where(a => a.OperationHistoryID == currentOperationHistory).
                Include(a => a.Aircraft).
                Include(a => a.Aircraft.PlanAircrafts).FirstOrDefault();
            return TransferMail(currentAirlines, obj, "运营历史", "OperationHistory");
        }


        /// <summary>
        /// 传输商业数据
        /// </summary>
        /// <param name="currentAirlines"></param>
        /// <param name="currentAircraftBusiness"></param>
        [Invoke]
        public bool TransferAircraftBusiness(Guid currentAirlines, Guid currentAircraftBusiness)
        {
            // 获取需要发送的对象
            var obj = this.DbContext.AircraftBusinesses.Where(a => a.AircraftBusinessID == currentAircraftBusiness).
                Include(a => a.Aircraft).
                Include(a => a.Aircraft.PlanAircrafts).FirstOrDefault();
            return TransferMail(currentAirlines, obj, "商业数据", "AircraftBusiness");
        }

        /// <summary>
        /// 传输所有权历史
        /// </summary>
        /// <param name="currentAirlines"></param>
        /// <param name="currentOwnershipHistory"></param>
        [Invoke]
        public bool TransferOwnershipHistory(Guid currentAirlines, Guid currentOwnershipHistory)
        {
            // 获取需要发送的对象
            var obj = this.DbContext.OwnershipHistories.Where(a => a.OwnershipHistoryID == currentOwnershipHistory).
                Include(a => a.Aircraft).
                Include(a => a.Aircraft.PlanAircrafts).FirstOrDefault();
            return TransferMail(currentAirlines, obj, "所有权历史", "OwnershipHistory");
        }

        [Invoke]
        public bool TransferPlanHistory(Guid currentAirlines, Guid currentPlanHistory)
        {
            //获取计划历史
            var planHistory =
                this.DbContext.PlanHistories.FirstOrDefault(p => p.PlanHistoryID == currentPlanHistory);
            if (planHistory != null && planHistory.GetType() != typeof(OperationPlan))
            {
                //获取商业数据Id
                var changePlan = planHistory as ChangePlan;
                if (changePlan != null)
                {
                    var aircraftBusinessID = changePlan.AircraftBusinessID;
                    //获取商业数据
                    var airBusiness = this.DbContext.AircraftBusinesses.
                                           Where(
                                               a =>
                                               a.AircraftBusinessID == aircraftBusinessID
                        ).Include(a => a.Aircraft).
                                           Include(a => a.Aircraft.PlanAircrafts).FirstOrDefault();
                    //发送数据
                    return TransferMail(currentAirlines, planHistory, "计划历史", "PlanHistory")
                           && TransferMail(currentAirlines, airBusiness, "商业数据", "AircraftBusinesses");
                }
            }
            else
            {
                //新建飞机时，在新建运营计划的同时新建商业数据，发送的同时发送商业数据
                //获取运营计划ID
                var operationPlan = planHistory as OperationPlan;
                if (operationPlan != null)
                {
                    var operationPlanID = operationPlan.OperationHistoryID;
                    //获取运营历史
                    var currentOperationHistory = this.DbContext.OperationHistories
                                                      .Where(p => p.OperationHistoryID == operationPlanID)
                                                      .Include(a => a.Aircraft)
                                                      .Include(a => a.Aircraft.PlanAircrafts).FirstOrDefault();
                    //获取运营历史Include商业数据
                    var allOperationHistory = this.DbContext.OperationHistories
                                                  .Where(p => p.OperationHistoryID == operationPlanID)
                                                  .Include(a => a.Aircraft)
                                                  .Include(a => a.Aircraft.PlanAircrafts)
                                                  .Include(a => a.Aircraft.AircraftBusinesses).FirstOrDefault();

                    if (allOperationHistory != null && allOperationHistory.Aircraft.AircraftBusinesses.Count == 1)
                    {
                        if (currentOperationHistory != null)
                        {
                            var aircraftBusiness = currentOperationHistory.Aircraft.AircraftBusinesses.FirstOrDefault();
                            if (aircraftBusiness != null)
                            {
                                var aircraftBusinessID =
                                    aircraftBusiness.AircraftBusinessID;
                                var airBusiness = this.DbContext.AircraftBusinesses.
                                                       Where(
                                                           a =>
                                                           a.AircraftBusinessID == aircraftBusinessID
                                    )
                                                      .Include(a => a.Aircraft)
                                                      .Include(a => a.Aircraft.PlanAircrafts).FirstOrDefault();
                                return TransferMail(currentAirlines, planHistory, "计划历史", "PlanHistory")
                                       && TransferMail(currentAirlines, currentOperationHistory, "运营历史", "OperationHistory")
                                       && TransferMail(currentAirlines, airBusiness, "商业数据", "AircraftBusinesses");
                            }
                        }
                    }
                    else
                    {
                        return TransferMail(currentAirlines, planHistory, "计划历史", "PlanHistory")
                               && TransferMail(currentAirlines, currentOperationHistory, "运营历史", "OperationHistory");
                    }
                }
            }
            return false;
        }

        #endregion

        #region "查询XmlConfig"

        [Invoke]
        public int UpdateXmlConfigContent(string xmlConfigType)
        {
            var xmlService = new XmlConfigService(this.DbContext, true);
            xmlService.UpdateXmlConfigContent(xmlConfigType);
            return 1;
        }

        [Invoke]
        public int UpdateAllXmlConfigFlag()
        {
            var xmlService = new XmlConfigService(this.DbContext, true);
            xmlService.UpdateAllXmlConfigFlag();
            return 1;
        }

        #endregion
    }

}
