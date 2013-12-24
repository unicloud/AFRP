using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.ServiceModel.DomainServices.Client;
using MessageOperationLibrary.EventAggregatorRepository;
using MessageOperationLibrary.Events;
using Microsoft.Windows.Data.DomainServices;
using Ria.Common;
using UniCloud.Fleet.Models;
using UniCloud.Fleet.Services.Web;
using UniCloud.Infrastructure;

namespace UniCloud.Fleet.Services
{
    public interface IFleetService
    {

        #region 公共属性

        /// <summary>
        /// 民航局ID
        /// </summary>
        string Caacid { get; }

        /// <summary>
        /// 当前航空公司
        /// </summary>
        Airlines CurrentAirlines { get; }

        /// <summary>
        /// 当前年度
        /// </summary>
        Annual CurrentAnnual { get; }

        /// <summary>
        /// 当前计划
        /// </summary>
        Plan CurrentPlan { get; }

        #endregion

        #region 属性绑定

        /// <summary>
        /// 所有供应商，不包含管理单位和制造商
        /// </summary>
        IEnumerable<Owner> AllOwners { get; }

        /// <summary>
        /// 所有管理单位
        /// </summary>
        IEnumerable<Manager> AllManagers { get; }

        /// <summary>
        /// 飞机类别集合，区分客货机及座级
        /// </summary>
        IEnumerable<AircraftCategory> AllAircraftCategories { get; }

        /// <summary>
        /// 机型集合
        /// </summary>
        IEnumerable<AircraftType> AllAircraftTypes { get; }

        /// <summary>
        /// 操作类别集合
        /// </summary>
        IEnumerable<ActionCategory> AllActionCategories { get; }

        /// <summary>
        /// 执行年份集合
        /// </summary>
        IEnumerable<Annual> PerformAnnuals { get; }

        /// <summary>
        /// 月份集合
        /// </summary>
        List<int> AllMonths { get; }

        /// <summary>
        /// 获取飞机类别集合
        /// </summary>
        /// <param name="ph">计划明细</param>
        /// <returns>飞机类别集合</returns>
        IEnumerable<AircraftCategory> GetAircraftCategories(PlanHistory ph);

        /// <summary>
        /// 获取机型集合
        /// </summary>
        /// <param name="ph">计划明细</param>
        /// <returns>机型集合</returns>
        IEnumerable<AircraftType> GetAircraftTypes(PlanHistory ph);

        /// <summary>
        /// 获取操作类别
        /// </summary>
        /// <param name="ph">计划历史</param>
        /// <returns>操作类别</returns>
        IEnumerable<ActionCategory> GetActionCategories(PlanHistory ph);

        /// <summary>
        /// 获取目标操作类别
        /// </summary>
        /// <param name="ph">计划历史</param>
        /// <returns>目标操作类别</returns>
        IEnumerable<ActionCategory> GetTargetCategories(PlanHistory ph);

        #endregion

        #region 公共

        /// <summary>
        /// 领域上下文
        /// </summary>
        FleetDomainContext Context { get; }

        /// <summary>
        /// 实体容器
        /// </summary>
        EntityContainer EntityContainer { get; }

        /// <summary>
        /// 保存实体变化
        /// </summary>
        void SubmitChanges();

        /// <summary>
        /// 保存实体变化
        /// </summary>
        /// <param name="callback">回调方法</param>
        /// <param name="state">状态</param>
        void SubmitChanges(Action<ServiceSubmitChangesResult> callback, object state);

        /// <summary>
        /// 保存实体变化
        /// </summary>
        /// <param name="success">保存成功所显示消息</param>
        /// <param name="fail">保存失败所显示消息</param>
        /// <param name="callback">回调方法</param>
        /// <param name="state">状态</param>
        void SubmitChanges(string success, string fail, Action<ServiceSubmitChangesResult> callback, object state);

        /// <summary>
        /// 撤销改变
        /// </summary>
        void RejectChanges();

        #endregion

        #region 数据传输

        /// <summary>
        /// 传输申请
        /// </summary>
        /// <param name="currentRequest">当前申请</param>
        /// <param name="callback">回调方法</param>
        /// <param name="state">状态</param>
        void TransferRequest(Guid currentRequest, Action<InvokeCompletedResult> callback, object state);

        /// <summary>
        /// 传输计划
        /// </summary>
        /// <param name="currentPlan">当前计划</param>
        /// <param name="callback">回调方法</param>
        /// <param name="state">状态</param>
        void TransferPlan(Guid currentPlan, Action<InvokeCompletedResult> callback, object state);

        /// <summary>
        /// 传输所有权历史
        /// </summary>
        /// <param name="currentOwnershipHistory">当前所有权历史</param>
        /// <param name="callback">回调方法</param>
        /// <param name="state">状态</param>
        void TransferOwnershipHistory(Guid currentOwnershipHistory, Action<InvokeCompletedResult> callback, object state);

        /// <summary>
        /// 传输计划历史
        /// </summary>
        /// <param name="currentPlanHistory">当前计划明细</param>
        /// <param name="callback">回调方法</param>
        /// <param name="state">状态</param>
        void TransferPlanHistory(Guid currentPlanHistory, Action<InvokeCompletedResult> callback, object state);

        /// <summary>
        /// 传输批文
        /// </summary>
        /// <param name="currentApprovalDoc">当前批文</param>
        /// <param name="callback">回调方法</param>
        /// <param name="state">状态</param>
        void TransferApprovalDoc(Guid currentApprovalDoc, Action<InvokeCompletedResult> callback, object state);

        /// <summary>
        /// 传输计划申请
        /// </summary>
        /// <param name="currentPlan">当前计划</param>
        /// <param name="currentRequest">当前申请</param>
        /// <param name="callback">回调方法</param>
        /// <param name="state">状态</param>
        void TransferPlanAndRequest(Guid currentPlan, Guid currentRequest, Action<InvokeCompletedResult> callback, object state);

        #endregion

        #region 加载数据

        /// <summary>
        /// 加载基础配置
        /// </summary>
        /// <param name="query">查询表达式</param>
        /// <param name="callback">回调方法</param>
        /// <param name="state">状态</param>
        void LoadXmlConfig(QueryBuilder<XmlConfig> query, Action<ServiceLoadResult<XmlConfig>> callback, object state);

        #region 参与者

        /// <summary>
        /// 加载所有权人
        /// </summary>
        /// <param name="query">查询表达式</param>
        /// <param name="callback">回调方法</param>
        /// <param name="state">状态</param>
        void LoadOwner(QueryBuilder<Owner> query, Action<ServiceLoadResult<Owner>> callback, object state);

        #endregion

        #region 飞机

        /// <summary>
        /// 加载飞机类别
        /// </summary>
        /// <param name="query">查询表达式</param>
        /// <param name="callback">回调方法</param>
        /// <param name="state">状态</param>
        void LoadAircraftCategory(QueryBuilder<AircraftCategory> query, Action<ServiceLoadResult<AircraftCategory>> callback, object state);

        /// <summary>
        /// 加载飞机机型
        /// </summary>
        /// <param name="query">查询表达式</param>
        /// <param name="callback">回调方法</param>
        /// <param name="state">状态</param>
        void LoadAircraftType(QueryBuilder<AircraftType> query, Action<ServiceLoadResult<AircraftType>> callback, object state);

        /// <summary>
        /// 加载飞机
        /// </summary>
        /// <param name="query">查询表达式</param>
        /// <param name="callback">回调方法</param>
        /// <param name="state">状态</param>
        void LoadAircraft(QueryBuilder<Aircraft> query, Action<ServiceLoadResult<Aircraft>> callback, object state);

        /// <summary>
        /// 加载计划飞机
        /// </summary>
        /// <param name="query">查询表达式</param>
        /// <param name="callback">回调方法</param>
        /// <param name="state">状态</param>
        void LoadPlanAircraft(QueryBuilder<PlanAircraft> query, Action<ServiceLoadResult<PlanAircraft>> callback, object state);

        /// <summary>
        /// 加载运营权历史
        /// </summary>
        /// <param name="query">查询表达式</param>
        /// <param name="callback">回调方法</param>
        /// <param name="state">状态</param>
        void LoadOperationHistory(QueryBuilder<OperationHistory> query, Action<ServiceLoadResult<OperationHistory>> callback, object state);

        /// <summary>
        /// 加载飞机商业数据
        /// </summary>
        /// <param name="query">查询表达式</param>
        /// <param name="callback">回调方法</param>
        /// <param name="state">状态</param>
        void LoadAircraftBusiness(QueryBuilder<AircraftBusiness> query, Action<ServiceLoadResult<AircraftBusiness>> callback, object state);

        /// <summary>
        /// 加载所有权历史
        /// </summary>
        /// <param name="query">查询表达式</param>
        /// <param name="callback">回调方法</param>
        /// <param name="state">状态</param>
        void LoadOwnershipHistory(QueryBuilder<OwnershipHistory> query, Action<ServiceLoadResult<OwnershipHistory>> callback, object state);

        #endregion

        #region 机队管理

        /// <summary>
        /// 加载操作类别
        /// </summary>
        /// <param name="query">查询表达式</param>
        /// <param name="callback">回调方法</param>
        /// <param name="state">状态</param>
        void LoadActionCategory(QueryBuilder<ActionCategory> query, Action<ServiceLoadResult<ActionCategory>> callback, object state);

        #region 年度

        /// <summary>
        /// 加载年度
        /// </summary>
        /// <param name="query">查询表达式</param>
        /// <param name="callback">回调方法</param>
        /// <param name="state">状态</param>
        void LoadAnnual(QueryBuilder<Annual> query, Action<ServiceLoadResult<Annual>> callback, object state);

        #endregion

        #region 计划

        /// <summary>
        /// 加载计划
        /// </summary>
        /// <param name="query">查询表达式</param>
        /// <param name="callback">回调方法</param>
        /// <param name="state">状态</param>
        void LoadPlan(QueryBuilder<Plan> query, Action<ServiceLoadResult<Plan>> callback, object state);

        #endregion

        #region 申请

        /// <summary>
        /// 加载申请，包括明细及引进方式
        /// </summary>
        /// <param name="query">查询表达式</param>
        /// <param name="callback">回调方法</param>
        /// <param name="state">状态</param>
        void LoadRequest(QueryBuilder<Request> query, Action<ServiceLoadResult<Request>> callback, object state);

        /// <summary>
        /// 加载批文内容
        /// </summary>
        /// <param name="query">查询表达式</param>
        /// <param name="callback">回调方法</param>
        /// <param name="state">状态</param>
        void LoadApprovalDoc(QueryBuilder<ApprovalDoc> query, Action<ServiceLoadResult<ApprovalDoc>> callback, object state);

        #endregion

        #endregion

        #region 邮件

        /// <summary>
        /// 加载基础配置
        /// </summary>
        /// <param name="query">查询表达式</param>
        /// <param name="callback">回调方法</param>
        /// <param name="state">状态</param>
        void LoadMailAddress(QueryBuilder<MailAddress> query, Action<ServiceLoadResult<MailAddress>> callback, object state);

        #endregion

        #endregion

        #region 业务逻辑

        #region 计划

        /// <summary>
        /// 设置当前年度
        /// </summary>
        /// <returns>当前年度</returns>
        Annual SetCurrentAnnual();

        /// <summary>
        /// 设置当前计划
        /// </summary>
        /// <returns>当前计划</returns>
        Plan SetCurrentPlan();

        /// <summary>
        /// 创建新年度计划
        /// </summary>
        /// <param name="title">计划标题</param>
        /// <returns>创建的计划</returns>
        Plan CreateNewYearPlan(string title);

        /// <summary>
        /// 创建当前年度新版本计划
        /// </summary>
        /// <param name="title">计划标题</param>
        /// <returns>创建的计划</returns>
        Plan CreateNewVersionPlan(string title);

        /// <summary>
        /// 创建运营计划明细
        /// </summary>
        /// <param name="plan">需要添加明细的计划</param>
        /// <param name="planAircraft">相关的计划飞机</param>
        /// <param name="actionType">操作类型</param>
        /// <returns>添加的运营计划明细</returns>
        OperationPlan CreateOperationPlan(Plan plan, PlanAircraft planAircraft, string actionType);

        /// <summary>
        /// 创建变更计划明细
        /// </summary>
        /// <param name="plan">需要添加明细的计划</param>
        /// <param name="planAircraft">相关的计划飞机</param>
        /// <param name="actionType">操作类型</param>
        /// <returns>添加的变更计划明细</returns>
        ChangePlan CreateChangePlans(Plan plan, PlanAircraft planAircraft, string actionType);

        /// <summary>
        /// 移除计划明细项
        /// </summary>
        /// <param name="planDetail">需要移除的计划明细项</param>
        void RemovePlanDetail(PlanHistory planDetail);

        /// <summary>
        /// 完成计划项
        /// </summary>
        /// <param name="planDetail">需要完成的计划项</param>
        /// <returns>计划项关联的飞机</returns>
        Aircraft CompletePlan(PlanHistory planDetail);

        /// <summary>
        /// 获取当前打开年度发布中计划
        /// </summary>
        /// <returns>当年发布中的计划</returns>
        IEnumerable<Plan> GetLatestPublishingPlan();

        /// <summary>
        /// 获取当前打开年度最后一份已发布的计划
        /// </summary>
        /// <returns>当年最后一份已发布的计划</returns>
        Plan GetLatestPublishedPlan();

        /// <summary>
        /// 获取所有有效的计划
        /// </summary>
        /// <returns>所有有效的计划</returns>
        IEnumerable<Plan> GetAllValidPlan();

        /// <summary>
        /// 判断是否存在变更计划
        /// </summary>
        /// <param name="af">飞机商业数据</param>
        /// <returns></returns>
        bool IsExistChangePlan(AircraftBusiness af);

        #endregion

        #region 批文管理

        /// <summary>
        /// 创建新申请
        /// </summary>
        /// <returns>新建的申请</returns>
        Request CreateNewRequest();

        /// <summary>
        /// 创建新申请明细
        /// </summary>
        /// <param name="request">申请</param>
        /// <param name="planHistory">申请明细关联的计划明细</param>
        /// <returns>新建的申请明细</returns>
        ApprovalHistory CreateNewRequestDetail(Request request, PlanHistory planHistory);

        /// <summary>
        /// 移除申请明细
        /// </summary>
        /// <param name="requestDetail">需要移除的申请明细</param>
        /// <returns>移除的申请明细</returns>
        void RemoveRequestDetail(ApprovalHistory requestDetail);

        /// <summary>
        /// 创建新批文
        /// </summary>
        /// <returns>新建的批文</returns>
        ApprovalDoc CreateNewApprovalDoc();

        ///// <summary>
        ///// 往批文添加申请
        ///// </summary>
        ///// <param name="approvalDoc">批文</param>
        ///// <param name="request">需要添加的申请</param>

        /// <summary>
        /// 往批文添加申请
        /// </summary>
        /// <param name="approvalDoc">批文</param>
        /// <param name="request">需要添加的申请</param>
        /// <returns>添加的申请</returns>
        Request AddRequestToApprovalDoc(ApprovalDoc approvalDoc, Request request);

        /// <summary>
        /// 移除申请
        /// </summary>
        /// <param name="request">移除的申请</param>
        void RemoveRequest(Request request);

        /// <summary>
        /// 批准申请
        /// </summary>
        /// <param name="requestDetail">批准的申请明细</param>
        void ApproveRequest(ApprovalHistory requestDetail);

        /// <summary>
        /// 拒绝申请
        /// </summary>
        /// <param name="requestDetail">被拒绝的申请明细</param>
        void RejectRequest(ApprovalHistory requestDetail);

        #endregion

        #region 运营管理

        /// <summary>
        /// 创建新的所有权历史记录
        /// </summary>
        /// <param name="aircraft">需要创建所有权的飞机</param>
        /// <returns>新建的所有权</returns>
        OwnershipHistory CreateNewOwnership(Aircraft aircraft);

        /// <summary>
        /// 移除所有权历史记录
        /// </summary>
        /// <param name="ownership">需要移除的所有权</param>
        void RemoveOwnership(OwnershipHistory ownership);

        /// <summary>
        /// 创建新的运力分配
        /// </summary>
        /// <param name="operation">需要分配的运营历史</param>
        /// <returns>新建的运力分配</returns>
        SubOperationHistory CreateNewSubOperation(OperationHistory operation);

        /// <summary>
        /// 移除运力分配记录
        /// </summary>
        /// <param name="subOperation">需要移除的运力分配</param>
        void RemoveSubOperation(SubOperationHistory subOperation);

        /// <summary>
        /// 创建分公司
        /// </summary>
        /// <param name="airlines">需要创建分公司的航空公司</param>
        /// <returns>创建的分公司</returns>
        Airlines CreateFilialeAirlines(Airlines airlines);

        /// <summary>
        /// 移除分公司
        /// </summary>
        /// <param name="airlines">需要移除的分公司</param>
        void RemoveFilialeAirlines(Airlines airlines);

        /// <summary>
        /// 创建子公司
        /// </summary>
        /// <param name="airlines">需要设为子公司的航空公司</param>
        void CreateSubAirlines(Airlines airlines);

        /// <summary>
        /// 移除子公司
        /// </summary>
        /// <param name="airlines">需要移除的子公司</param>
        void RemoveSubAirlines(Airlines airlines);

        /// <summary>
        /// 创建新供应商
        /// </summary>
        /// <returns>创建的供应商</returns>
        Owner CreateSupplier();

        /// <summary>
        /// 移除供应商
        /// </summary>
        /// <param name="supplier">需要移除的供应商</param>
        void RemoveSupplier(Owner supplier);

        #endregion

        #endregion

    }

    [Export(typeof(IFleetService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class FleetService : IFleetService
    {

        #region 私有字段

        private bool _allXmlConfigLoaded, _allOwnerLoaded, _allAircraftCategoryLoaded, _allAircraftTypeLoaded, _allAircraftLoaded,
    _allPlanAircraftLoaded, _allOperationHistoryLoaded, _allAircraftBusinessLoaded, _allOwnershipHistoryLoaded, _allActionCategoryLoaded,
    _allAnnualLoaded, _allPlanLoaded, _allRequestLoaded, _allApprovalDocLoaded, _allMailAddressLoaded;
        private int _loadTaskCounter;

        private readonly MessageEvent _messageEvent = new MessageEvent();
        private readonly FleetDomainContext _context = new FleetDomainContext();
        private readonly List<int> _allMonths = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

        #endregion

        #region 私有方法

        private void Load<T>(EntityQuery<T> query, Action<LoadOperation<T>> callback, object state) where T : Entity
        {
            this._context.Load(query, callback, state);
        }

        private ServiceLoadResult<T> CreateResult<T>(LoadOperation<T> op, bool returnEditableCollection = false) where T : Entity
        {
            if (op.HasError)
            {
                op.MarkErrorAsHandled();
            }
            return new ServiceLoadResult<T>(
                returnEditableCollection ? new EntityList<T>(this.EntityContainer.GetEntitySet<T>(), op.Entities) : op.Entities,
                op.TotalEntityCount,
                op.ValidationErrors,
                op.Error,
                op.IsCanceled,
                op.UserState);
        }

        private static ServiceLoadResult<T> CreateResult<T>(IEnumerable<T> entities) where T : Entity
        {
            return new ServiceLoadResult<T>(
                entities,
                null);
        }

        private static ServiceSubmitChangesResult SubmitResult(SubmitOperation sm)
        {
            if (sm.HasError)
            {
                sm.MarkErrorAsHandled();
            }
            return new ServiceSubmitChangesResult(sm.ChangeSet, sm.EntitiesInError, sm.Error, sm.CanCancel, sm.UserState);
        }

        private static InvokeCompletedResult InvokeResult(InvokeOperation io)
        {
            if (io.HasError)
            {
                io.MarkErrorAsHandled();
            }
            return new InvokeCompletedResult(io.Value, io.ValidationErrors);
        }

        #endregion

        #region 公共属性

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        public string Caacid
        {
            get
            {
                return "31A9DE51-C207-4A73-919C-21521F17FEF9";
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        public Airlines CurrentAirlines { get; private set; }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        public Annual CurrentAnnual { get; private set; }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        public Plan CurrentPlan { get; private set; }

        #endregion

        #region 属性绑定

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        public IEnumerable<Owner> AllOwners { get; private set; }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        public IEnumerable<Manager> AllManagers { get; private set; }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        public IEnumerable<AircraftCategory> AllAircraftCategories { get; private set; }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        public IEnumerable<AircraftType> AllAircraftTypes { get; private set; }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        public IEnumerable<ActionCategory> AllActionCategories { get; private set; }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        public IEnumerable<Annual> PerformAnnuals { get; private set; }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        public List<int> AllMonths
        {
            get { return this._allMonths; }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="ph"><see cref="IFleetService"/></param>
        /// <returns><see cref="IFleetService"/></returns>
        public IEnumerable<AircraftCategory> GetAircraftCategories(PlanHistory ph)
        {
            if (ph.ActionCategory == null) return this.AllAircraftCategories;
            var actionName = ph.ActionCategory.ActionName;
            if (ph.AircraftType == null)
            {
                if (actionName == "客改货")
                {
                    ph.Regional = null;
                    return this.AllAircraftCategories.Where(ac => ac.Category == "货机");
                }
                else if (actionName == "货改客")
                {
                    ph.Regional = null;
                    return this.AllAircraftCategories.Where(ac => ac.Category == "客机");
                }
                else return this.AllAircraftCategories;
            }
            var actionType = ph.ActionCategory.ActionType;

            var aircraftCategory = ph.AircraftType.AircraftCategory;
            if (actionType == "变更")
            {
                if (actionName == "客改货")
                {
                    ph.Regional = null;
                    return this.AllAircraftCategories.Where(ac => ac.Category == "货机");
                }
                if (actionName == "货改客")
                {
                    ph.Regional = null;
                    return this.AllAircraftCategories.Where(ac => ac.Category == "客机");
                }
                return this.AllAircraftCategories.Where(ac => ac == aircraftCategory);
            }
            return this.AllAircraftCategories;
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="ph"><see cref="IFleetService"/></param>
        /// <returns><see cref="IFleetService"/></returns>
        public IEnumerable<AircraftType> GetAircraftTypes(PlanHistory ph)
        {
            var regional = ph.Regional;
            if (string.IsNullOrWhiteSpace(regional)) return null;
            if (ph.ActionCategory == null || ph.AircraftType == null)
                return this.AllAircraftTypes.Where(p => p.AircraftCategory.Regional == regional);
            var actionType = ph.ActionCategory.ActionType;
            var actionName = ph.ActionCategory.ActionName;
            if (actionType == "变更" && actionName != "客改货" && actionName != "货改客")
                return this.AllAircraftTypes.Where(at => at == ph.AircraftType);
            return this.AllAircraftTypes.Where(p => p.AircraftCategory.Regional == regional);
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="ph"><see cref="IFleetService"/></param>
        /// <returns><see cref="IFleetService"/></returns>
        public IEnumerable<ActionCategory> GetActionCategories(PlanHistory ph)
        {
            return this.AllActionCategories.Where(a => a.ActionType == ph.ActionType);
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="ph"><see cref="IFleetService"/></param>
        /// <returns><see cref="IFleetService"/></returns>
        public IEnumerable<ActionCategory> GetTargetCategories(PlanHistory ph)
        {
            return AllActionCategories.Where(a => a.ActionType != "变更");
        }

        #endregion

        #region 公共

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        public FleetDomainContext Context
        {
            get { return this._context; }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        public EntityContainer EntityContainer
        {
            get { return this._context.EntityContainer; }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        public void SubmitChanges()
        {
            this._context.SubmitChanges();

            this._context.UpdateAllXmlConfigFlag();
            this._allXmlConfigLoaded = false;
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="callback"><see cref="IFleetService"/></param>
        /// <param name="state"><see cref="IFleetService"/></param>
        public void SubmitChanges(Action<ServiceSubmitChangesResult> callback, object state)
        {
            this._context.SubmitChanges(sm =>
            {
                if (sm.Error != null)
                {
                    this._messageEvent.Message = CAFMStrings.SaveFail;
                    this._messageEvent.MessageType = MessageType.Fail;
                    MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(this._messageEvent);
                }
                else if (!sm.IsCanceled)
                {
                    this._messageEvent.Message = CAFMStrings.SaveSuccess;
                    this._messageEvent.MessageType = MessageType.Success;
                    MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(this._messageEvent);
                }
                callback(SubmitResult(sm));
                this._context.UpdateAllXmlConfigFlag();
                this._allXmlConfigLoaded = false;
            }, state);
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="success"><see cref="IFleetService"/></param>
        /// <param name="fail"><see cref="IFleetService"/></param>
        /// <param name="callback"><see cref="IFleetService"/></param>
        /// <param name="state"><see cref="IFleetService"/></param>
        public void SubmitChanges(string success, string fail, Action<ServiceSubmitChangesResult> callback, object state)
        {
            this._context.SubmitChanges(sm =>
            {
                if (sm.Error != null)
                {
                    this._messageEvent.Message = fail;
                    this._messageEvent.MessageType = MessageType.Success;
                    MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(this._messageEvent);
                }
                else if (!sm.IsCanceled)
                {
                    this._messageEvent.Message = success;
                    this._messageEvent.MessageType = MessageType.Success;
                    MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(this._messageEvent);
                }
                callback(SubmitResult(sm));
                this._context.UpdateAllXmlConfigFlag();
                this._allXmlConfigLoaded = false;
            }, state);
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        public void RejectChanges()
        {
            this._context.RejectChanges();
        }

        #endregion

        #region 数据传输

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="currentRequest"><see cref="IFleetService"/></param>
        /// <param name="callback"><see cref="IFleetService"/></param>
        /// <param name="state"><see cref="IFleetService"/></param>
        public void TransferRequest(Guid currentRequest, Action<InvokeCompletedResult> callback, object state)
        {
            this._context.TransferRequest(CurrentAirlines.OwnerID, currentRequest, io =>
                {
                    if (!io.Value)
                    {
                        this._messageEvent.Message = CAFMStrings.SendFail;
                        this._messageEvent.MessageType = MessageType.Fail;
                        MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(this._messageEvent);
                    }
                    else
                    {
                        this._messageEvent.Message = CAFMStrings.SendSuccess;
                        this._messageEvent.MessageType = MessageType.Success;
                        MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(this._messageEvent);
                    }
                    callback(InvokeResult(io));
                }, state);
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="currentPlan"><see cref="IFleetService"/></param>
        /// <param name="callback"><see cref="IFleetService"/></param>
        /// <param name="state"><see cref="IFleetService"/></param>
        public void TransferPlan(Guid currentPlan, Action<InvokeCompletedResult> callback, object state)
        {
            this._context.TransferPlan(CurrentAirlines.OwnerID, currentPlan, io =>
                {
                    if (!io.Value)
                    {
                        this._messageEvent.Message = CAFMStrings.SendFail;
                        this._messageEvent.MessageType = MessageType.Fail;
                        MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(this._messageEvent);
                    }
                    else
                    {
                        this._messageEvent.Message = CAFMStrings.SendSuccess;
                        this._messageEvent.MessageType = MessageType.Success;
                        MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(this._messageEvent);
                    }
                    callback(InvokeResult(io));
                }, state);
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="currentApprovalDoc"><see cref="IFleetService"/></param>
        /// <param name="callback"><see cref="IFleetService"/></param>
        /// <param name="state"><see cref="IFleetService"/></param>
        public void TransferApprovalDoc(Guid currentApprovalDoc, Action<InvokeCompletedResult> callback, object state)
        {
            this._context.TransferApprovalDoc(CurrentAirlines.OwnerID, currentApprovalDoc, io =>
            {
                if (!io.Value)
                {
                    this._messageEvent.Message = CAFMStrings.SendFail;
                    this._messageEvent.MessageType = MessageType.Fail;
                    MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(this._messageEvent);
                }
                else
                {
                    this._messageEvent.Message = CAFMStrings.SendSuccess;
                    this._messageEvent.MessageType = MessageType.Success;
                    MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(this._messageEvent);
                }
                callback(InvokeResult(io));
            }, state);
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="currentPlanHistory"><see cref="IFleetService"/></param>
        /// <param name="callback"><see cref="IFleetService"/></param>
        /// <param name="state"><see cref="IFleetService"/></param>
        public void TransferPlanHistory(Guid currentPlanHistory, Action<InvokeCompletedResult> callback, object state)
        {
            this._context.TransferPlanHistory(CurrentAirlines.OwnerID, currentPlanHistory, io =>
            {
                if (!io.Value)
                {
                    this._messageEvent.Message = CAFMStrings.SendFail;
                    this._messageEvent.MessageType = MessageType.Fail;
                    MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(this._messageEvent);
                }
                else
                {
                    this._messageEvent.Message = CAFMStrings.SendSuccess;
                    this._messageEvent.MessageType = MessageType.Success;
                    MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(this._messageEvent);
                }
                callback(InvokeResult(io));
            }, state);
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="currentOwnershipHistory"><see cref="IFleetService"/></param>
        /// <param name="callback"><see cref="IFleetService"/></param>
        /// <param name="state"><see cref="IFleetService"/></param>
        public void TransferOwnershipHistory(Guid currentOwnershipHistory, Action<InvokeCompletedResult> callback, object state)
        {
            this._context.TransferOwnershipHistory(CurrentAirlines.OwnerID, currentOwnershipHistory, io =>
            {
                if (!io.Value)
                {
                    this._messageEvent.Message = CAFMStrings.SendFail;
                    this._messageEvent.MessageType = MessageType.Fail;
                    MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(this._messageEvent);
                }
                else
                {
                    this._messageEvent.Message = CAFMStrings.SendSuccess;
                    this._messageEvent.MessageType = MessageType.Success;
                    MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(this._messageEvent);
                }
                callback(InvokeResult(io));
            }, state);

        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="currentPlan"><see cref="IFleetService"/></param>
        /// <param name="currentRequest"><see cref="IFleetService"/></param>
        /// <param name="callback"><see cref="IFleetService"/></param>
        /// <param name="state"><see cref="IFleetService"/></param>
        public void TransferPlanAndRequest(Guid currentPlan, Guid currentRequest, Action<InvokeCompletedResult> callback, object state)
        {
            this._context.TransferPlanAndRequest(CurrentAirlines.OwnerID, currentPlan, currentRequest, io =>
            {
                if (!io.Value)
                {
                    this._messageEvent.Message = CAFMStrings.SendFail;
                    this._messageEvent.MessageType = MessageType.Fail;
                    MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(this._messageEvent);
                }
                else
                {
                    this._messageEvent.Message = CAFMStrings.SendSuccess;
                    this._messageEvent.MessageType = MessageType.Success;
                    MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(this._messageEvent);
                }
                callback(InvokeResult(io));
            }, state);
        }

        #endregion

        #region 加载数据

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="query"><see cref="IFleetService"/></param>
        /// <param name="callback"><see cref="IFleetService"/></param>
        /// <param name="state"><see cref="IFleetService"/></param>
        public void LoadXmlConfig(QueryBuilder<XmlConfig> query, Action<ServiceLoadResult<XmlConfig>> callback, object state)
        {
            if (!this._allXmlConfigLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetXmlConfigQuery()), lo =>
                    {
                        this._allXmlConfigLoaded = true;
                        callback(this.CreateResult(lo));
                    }, state);
            }
            else
            {
                callback(CreateResult(this.EntityContainer.GetEntitySet<XmlConfig>()));
            }
        }

        #region 参与者

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="query"><see cref="IFleetService"/></param>
        /// <param name="callback"><see cref="IFleetService"/></param>
        /// <param name="state"><see cref="IFleetService"/></param>
        public void LoadOwner(QueryBuilder<Owner> query, Action<ServiceLoadResult<Owner>> callback, object state)
        {
            if (!this._allOwnerLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetOwnerQuery()), lo =>
                    {
                        this._allOwnerLoaded = true;
                        AllOwners =
                            this._context.Owners.Where(
                                o => !(o is Manager) && !(o is Manufacturer))
                                .OrderBy(o => o.GetType().ToString())
                                .ThenBy(o => o.Name);
                        AllManagers = this._context.Owners.OfType<Manager>().OrderBy(m => m.Name);
                        this.CurrentAirlines = _context.Owners.OfType<Airlines>().FirstOrDefault(r => r.IsCurrent);
                        callback(this.CreateResult(lo));
                    }, state);
            }
            else
            {
                callback(CreateResult(this.EntityContainer.GetEntitySet<Owner>()));
            }
        }

        #endregion

        #region 飞机

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="query"><see cref="IFleetService"/></param>
        /// <param name="callback"><see cref="IFleetService"/></param>
        /// <param name="state"><see cref="IFleetService"/></param>
        public void LoadAircraftCategory(QueryBuilder<AircraftCategory> query, Action<ServiceLoadResult<AircraftCategory>> callback, object state)
        {
            if (!this._allAircraftCategoryLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetAircraftCategoryQuery()), lo =>
                    {
                        this._allAircraftCategoryLoaded = true;
                        AllAircraftCategories = this._context.AircraftCategories.OrderBy(a => a.Category).ThenBy(a => a.Regional);
                        callback(this.CreateResult(lo));
                    }, state);
            }
            else
            {
                callback(CreateResult(this.EntityContainer.GetEntitySet<AircraftCategory>()));
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="query"><see cref="IFleetService"/></param>
        /// <param name="callback"><see cref="IFleetService"/></param>
        /// <param name="state"><see cref="IFleetService"/></param>
        public void LoadAircraftType(QueryBuilder<AircraftType> query, Action<ServiceLoadResult<AircraftType>> callback, object state)
        {
            if (!this._allAircraftTypeLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetAircraftTypeQuery()), lo =>
                    {
                        this._allAircraftTypeLoaded = true;
                        AllAircraftTypes = this._context.AircraftTypes.OrderBy(a => a.Name);
                        callback(this.CreateResult(lo));
                    }, state);
            }
            else
            {
                callback(CreateResult(this.EntityContainer.GetEntitySet<AircraftType>()));
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="query"><see cref="IFleetService"/></param>
        /// <param name="callback"><see cref="IFleetService"/></param>
        /// <param name="state"><see cref="IFleetService"/></param>
        public void LoadAircraft(QueryBuilder<Aircraft> query, Action<ServiceLoadResult<Aircraft>> callback, object state)
        {
            if (!this._allAircraftLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetAircraftQuery()), lo =>
                    {
                        this._allAircraftLoaded = true;
                        callback(this.CreateResult(lo));
                    }, state);
            }
            else
            {
                callback(CreateResult(this.EntityContainer.GetEntitySet<Aircraft>()));
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="query"><see cref="IFleetService"/></param>
        /// <param name="callback"><see cref="IFleetService"/></param>
        /// <param name="state"><see cref="IFleetService"/></param>
        public void LoadPlanAircraft(QueryBuilder<PlanAircraft> query, Action<ServiceLoadResult<PlanAircraft>> callback, object state)
        {
            if (!this._allPlanAircraftLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetPlanAircraftQuery()), lo =>
                    {
                        this._allPlanAircraftLoaded = true;
                        callback(this.CreateResult(lo));
                    }, state);
            }
            else
            {
                callback(CreateResult(this.EntityContainer.GetEntitySet<PlanAircraft>()));
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="query"><see cref="IFleetService"/></param>
        /// <param name="callback"><see cref="IFleetService"/></param>
        /// <param name="state"><see cref="IFleetService"/></param>
        public void LoadOperationHistory(QueryBuilder<OperationHistory> query, Action<ServiceLoadResult<OperationHistory>> callback, object state)
        {
            if (!this._allOperationHistoryLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetOperationHistoryQuery()), lo =>
                    {
                        this._allOperationHistoryLoaded = true;
                        callback(this.CreateResult(lo));
                    }, state);
            }
            else
            {
                callback(CreateResult(this.EntityContainer.GetEntitySet<OperationHistory>()));
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="query"><see cref="IFleetService"/></param>
        /// <param name="callback"><see cref="IFleetService"/></param>
        /// <param name="state"><see cref="IFleetService"/></param>
        public void LoadAircraftBusiness(QueryBuilder<AircraftBusiness> query, Action<ServiceLoadResult<AircraftBusiness>> callback, object state)
        {
            if (!this._allAircraftBusinessLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetAircraftBusinessQuery()), lo =>
                    {
                        this._allAircraftBusinessLoaded = true;
                        callback(this.CreateResult(lo));
                    }, state);
            }
            else
            {
                callback(CreateResult(this.EntityContainer.GetEntitySet<AircraftBusiness>()));
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="query"><see cref="IFleetService"/></param>
        /// <param name="callback"><see cref="IFleetService"/></param>
        /// <param name="state"><see cref="IFleetService"/></param>
        public void LoadOwnershipHistory(QueryBuilder<OwnershipHistory> query, Action<ServiceLoadResult<OwnershipHistory>> callback, object state)
        {
            if (!this._allOwnershipHistoryLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetOwnershipHistoryQuery()), lo =>
                {
                    this._allOwnershipHistoryLoaded = true;
                    callback(this.CreateResult(lo));
                }, state);
            }
            else
            {
                callback(CreateResult(this.EntityContainer.GetEntitySet<OwnershipHistory>()));
            }
        }

        #endregion

        #region 机队管理

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="query"><see cref="IFleetService"/></param>
        /// <param name="callback"><see cref="IFleetService"/></param>
        /// <param name="state"><see cref="IFleetService"/></param>
        public void LoadActionCategory(QueryBuilder<ActionCategory> query, Action<ServiceLoadResult<ActionCategory>> callback, object state)
        {
            if (!this._allActionCategoryLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetActionCategoryQuery()), lo =>
                    {
                        this._allActionCategoryLoaded = true;
                        AllActionCategories = this._context.ActionCategories.OrderByDescending(a => a.ActionType).ThenBy(a => a.ActionName);
                        callback(this.CreateResult(lo));
                    }, state);
            }
            else
            {
                callback(CreateResult(this.EntityContainer.GetEntitySet<ActionCategory>()));
            }
        }

        #region 年度

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="query"><see cref="IFleetService"/></param>
        /// <param name="callback"><see cref="IFleetService"/></param>
        /// <param name="state"><see cref="IFleetService"/></param>
        public void LoadAnnual(QueryBuilder<Annual> query, Action<ServiceLoadResult<Annual>> callback, object state)
        {
            if (!this._allAnnualLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetAnnualQuery()), lo =>
                    {
                        this._allAnnualLoaded = true;
                        this.SetCurrentAnnual();
                        this.PerformAnnuals =
                            _context.Annuals.OrderBy(a => a.Year)
                                    .SkipWhile(a => a.Year < this.CurrentAnnual.Year - 1)
                                    .Take(8);
                        callback(this.CreateResult(lo));
                    }, state);
            }
            else
            {
                callback(CreateResult(this.EntityContainer.GetEntitySet<Annual>()));
            }
        }

        #endregion

        #region 计划

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="query"><see cref="IFleetService"/></param>
        /// <param name="callback"><see cref="IFleetService"/></param>
        /// <param name="state"><see cref="IFleetService"/></param>
        public void LoadPlan(QueryBuilder<Plan> query, Action<ServiceLoadResult<Plan>> callback, object state)
        {
            if (!this._allPlanLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetPlanQuery()), lo =>
                    {
                        this._allPlanLoaded = true;
                        this.SetCurrentPlan();
                        callback(this.CreateResult(lo));
                    }, state);
            }
            else
            {
                callback(CreateResult(this.EntityContainer.GetEntitySet<Plan>()));
            }
        }

        #endregion

        #region 申请

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="query"><see cref="IFleetService"/></param>
        /// <param name="callback"><see cref="IFleetService"/></param>
        /// <param name="state"><see cref="IFleetService"/></param>
        public void LoadRequest(QueryBuilder<Request> query, Action<ServiceLoadResult<Request>> callback, object state)
        {
            if (!this._allRequestLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetRequestQuery()), lo =>
                    {
                        this._allRequestLoaded = true;
                        callback(this.CreateResult(lo));
                    }, state);
            }
            else
            {
                callback(CreateResult(this.EntityContainer.GetEntitySet<Request>()));
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="query"><see cref="IFleetService"/></param>
        /// <param name="callback"><see cref="IFleetService"/></param>
        /// <param name="state"><see cref="IFleetService"/></param>
        public void LoadApprovalDoc(QueryBuilder<ApprovalDoc> query, Action<ServiceLoadResult<ApprovalDoc>> callback, object state)
        {
            if (!this._allApprovalDocLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetApprovalDocQuery()), lo =>
                   {
                       this._allApprovalDocLoaded = true;
                       callback(this.CreateResult(lo));
                   }, state);
            }
            else
            {
                callback(CreateResult(this.EntityContainer.GetEntitySet<ApprovalDoc>()));
            }
        }

        #endregion

        #endregion

        #region 邮件

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="query"><see cref="IFleetService"/></param>
        /// <param name="callback"><see cref="IFleetService"/></param>
        /// <param name="state"><see cref="IFleetService"/></param>
        public void LoadMailAddress(QueryBuilder<MailAddress> query, Action<ServiceLoadResult<MailAddress>> callback, object state)
        {
            if (!this._allMailAddressLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetMailAddressQuery()), lo =>
                {
                    this._allMailAddressLoaded = true;
                    callback(this.CreateResult(lo));
                }, state);
            }
            else
            {
                callback(CreateResult(this.EntityContainer.GetEntitySet<MailAddress>()));
            }
        }

        #endregion

        #endregion

        #region 业务逻辑

        #region 计划

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <returns><see cref="IFleetService"/></returns>
        public Annual SetCurrentAnnual()
        {
            this.CurrentAnnual = _context.Annuals.FirstOrDefault(a => a.IsOpen == true);
            return this.CurrentAnnual;
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <returns><see cref="IFleetService"/></returns>
        public Plan SetCurrentPlan()
        {
            this.CurrentPlan =
                _context.Plans.Where(p => p.Annual.IsOpen == true).OrderBy(p => p.VersionNumber).LastOrDefault();
            return this.CurrentPlan;
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="title"><see cref="IFleetService"/></param>
        /// <returns><see cref="IFleetService"/></returns>
        public Plan CreateNewYearPlan(string title)
        {
            using (var pb = new FleetServiceHelper())
            {
                return pb.CreateNewYearPlan(title, this);
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="title"><see cref="IFleetService"/></param>
        /// <returns><see cref="IFleetService"/></returns>
        public Plan CreateNewVersionPlan(string title)
        {
            using (var pb = new FleetServiceHelper())
            {
                return pb.CreateNewVersionPlan(title, this);
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="plan"><see cref="IFleetService"/></param>
        /// <param name="planAircraft"><see cref="IFleetService"/></param>
        /// <param name="actionType"><see cref="IFleetService"/></param>
        /// <returns><see cref="IFleetService"/></returns>
        public OperationPlan CreateOperationPlan(Plan plan, PlanAircraft planAircraft, string actionType)
        {
            using (var pb = new FleetServiceHelper())
            {
                return pb.CreateOperationPlan(plan, planAircraft, actionType, this);
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="plan"><see cref="IFleetService"/></param>
        /// <param name="planAircraft"><see cref="IFleetService"/></param>
        /// <param name="actionType"><see cref="IFleetService"/></param>
        /// <returns><see cref="IFleetService"/></returns>
        public ChangePlan CreateChangePlans(Plan plan, PlanAircraft planAircraft, string actionType)
        {
            using (var pb = new FleetServiceHelper())
            {
                return pb.CreateChangePlan(plan, planAircraft, actionType, this);
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="planDetail"><see cref="IFleetService"/></param>
        public void RemovePlanDetail(PlanHistory planDetail)
        {
            using (var pb = new FleetServiceHelper())
            {
                pb.RemovePlanDetail(planDetail, this);
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="planDetail"><see cref="IFleetService"/></param>
        /// <returns><see cref="IFleetService"/></returns>
        public Aircraft CompletePlan(PlanHistory planDetail)
        {
            using (var pb = new FleetServiceHelper())
            {
                return pb.CompletePlan(planDetail, this);
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <returns><see cref="IFleetService"/></returns>
        public IEnumerable<Plan> GetLatestPublishingPlan()
        {
            using (var pb = new FleetServiceHelper())
            {
                return pb.GetLatestPublishingPlan(this);
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <returns><see cref="IFleetService"/></returns>
        public Plan GetLatestPublishedPlan()
        {
            using (var pb = new FleetServiceHelper())
            {
                return pb.GetLatestPublishedPlan(this);
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <returns><see cref="IFleetService"/></returns>
        public IEnumerable<Plan> GetAllValidPlan()
        {
            using (var pb = new FleetServiceHelper())
            {
                return pb.GetAllValidPlan(this);
            }
        }


        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="af"><see cref="IFleetService"/></param>
        /// <returns><see cref="IFleetService"/></returns>
        public bool IsExistChangePlan(AircraftBusiness af)
        {
            return this.EntityContainer.GetEntitySet<PlanHistory>().OfType<ChangePlan>().Any(
                p => p.AircraftBusinessID == af.AircraftBusinessID);
        }

        #endregion

        #region 批文管理

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <returns><see cref="IFleetService"/></returns>
        public Request CreateNewRequest()
        {
            using (var pb = new FleetServiceHelper())
            {
                return pb.CreateNewRequest(this);
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="request"><see cref="IFleetService"/></param>
        /// <param name="planHistory"><see cref="IFleetService"/></param>
        /// <returns><see cref="IFleetService"/></returns>
        public ApprovalHistory CreateNewRequestDetail(Request request, PlanHistory planHistory)
        {
            using (var pb = new FleetServiceHelper())
            {
                return pb.CreateNewRequestDetail(request, planHistory, this);
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="requestDetail"><see cref="IFleetService"/></param>
        /// <returns><see cref="IFleetService"/></returns>
        public void RemoveRequestDetail(ApprovalHistory requestDetail)
        {
            using (var pb = new FleetServiceHelper())
            {
                pb.RemoveRequestDetail(requestDetail, this);
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <returns><see cref="IFleetService"/></returns>
        public ApprovalDoc CreateNewApprovalDoc()
        {
            using (var pb = new FleetServiceHelper())
            {
                return pb.CreateNewApprovalDoc(this);
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="approvalDoc"><see cref="IFleetService"/></param>
        /// <param name="request"><see cref="IFleetService"/></param>
        /// <returns><see cref="IFleetService"/></returns>
        public Request AddRequestToApprovalDoc(ApprovalDoc approvalDoc, Request request)
        {
            using (var pb = new FleetServiceHelper())
            {
                return pb.AddRequestToApprovalDoc(approvalDoc, request, this);
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="request"><see cref="IFleetService"/></param>
        public void RemoveRequest(Request request)
        {
            using (var pb = new FleetServiceHelper())
            {
                pb.RemoveRequest(request, this);
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="requestDetail"><see cref="IFleetService"/></param>
        public void ApproveRequest(ApprovalHistory requestDetail)
        {
            using (var pb = new FleetServiceHelper())
            {
                pb.ApproveRequest(requestDetail, this);
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="requestDetail"><see cref="IFleetService"/></param>
        public void RejectRequest(ApprovalHistory requestDetail)
        {
            using (var pb = new FleetServiceHelper())
            {
                pb.RejectRequest(requestDetail, this);
            }
        }

        #endregion

        #region 运营管理

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="aircraft"><see cref="IFleetService"/></param>
        /// <returns><see cref="IFleetService"/></returns>
        public OwnershipHistory CreateNewOwnership(Aircraft aircraft)
        {
            using (var pb = new FleetServiceHelper())
            {
                return pb.CreateNewOwnership(aircraft, this);
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="ownership"><see cref="IFleetService"/></param>
        public void RemoveOwnership(OwnershipHistory ownership)
        {
            using (var pb = new FleetServiceHelper())
            {
                pb.RemoveOwnership(ownership, this);
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="operation"><see cref="IFleetService"/></param>
        /// <returns><see cref="IFleetService"/></returns>
        public SubOperationHistory CreateNewSubOperation(OperationHistory operation)
        {
            using (var pb = new FleetServiceHelper())
            {
                return pb.CreateNewSubOperation(operation, this);
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="subOperation"><see cref="IFleetService"/></param>
        public void RemoveSubOperation(SubOperationHistory subOperation)
        {
            using (var pb = new FleetServiceHelper())
            {
                pb.RemoveSubOperation(subOperation, this);
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="airlines"><see cref="IFleetService"/></param>
        /// <returns><see cref="IFleetService"/></returns>
        public Airlines CreateFilialeAirlines(Airlines airlines)
        {
            using (var pb = new FleetServiceHelper())
            {
                return pb.CreateFilialeAirlines(airlines, this);
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="airlines"><see cref="IFleetService"/></param>
        public void RemoveFilialeAirlines(Airlines airlines)
        {
            using (var pb = new FleetServiceHelper())
            {
                pb.RemoveFilialeAirlines(airlines, this);
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="airlines"><see cref="IFleetService"/></param>
        public void CreateSubAirlines(Airlines airlines)
        {
            using (var pb = new FleetServiceHelper())
            {
                pb.CreateSubAirlines(airlines, this);
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="airlines"><see cref="IFleetService"/></param>
        public void RemoveSubAirlines(Airlines airlines)
        {
            using (var pb = new FleetServiceHelper())
            {
                pb.RemoveSubAirlines(airlines, this);
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <returns><see cref="IFleetService"/></returns>
        public Owner CreateSupplier()
        {
            using (var pb = new FleetServiceHelper())
            {
                return pb.CreateSupplier(this);
            }
        }

        /// <summary>
        /// <see cref="IFleetService"/>
        /// </summary>
        /// <param name="supplier"><see cref="IFleetService"/></param>
        public void RemoveSupplier(Owner supplier)
        {
            using (var pb = new FleetServiceHelper())
            {
                pb.RemoveSupplier(supplier, this);
            }
        }

        #endregion

        #endregion

    }

}
