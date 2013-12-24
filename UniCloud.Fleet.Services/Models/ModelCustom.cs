using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Practices.ServiceLocation;
using UniCloud.Fleet.Services;

namespace UniCloud.Fleet.Models
{

    #region 基础配置

    public sealed partial class XmlConfig
    {
        public XElement LocalXmlContent
        {
            get { return XElement.Parse(this.ConfigContent); }
            set { this.ConfigContent = value.ToString(); }
        }
    }

    public sealed partial class ActionCategory
    {
        public string ActionOperation
        {
            get
            {
                return this.ActionType + "：" + this.ActionName;
            }
        }
    }

    #endregion

    #region 参与者

    public partial class Owner
    {
        private List<SupplierType> _suppliers;
        public List<SupplierType> Suppliers
        {
            get
            {
                return _suppliers ?? (_suppliers = new List<SupplierType>
                    {
                        new SupplierType {Type = 1, Name = "国内供应商"},
                        new SupplierType {Type = 2, Name = "国外供应商"}
                    });
            }
        }
    }

    public class SupplierType
    {
        public int Type { get; set; }
        public string Name { get; set; }
    }

    #endregion

    #region 飞机

    public sealed partial class Aircraft
    {
        public string Title { get; set; }

        [Display(Name = "活动类型")]
        public string ActionCategoryOperation
        {
            get
            {
                if (this.ImportCategory != null)
                {
                    return this.ImportCategory.ActionType + "：" + this.ImportCategory.ActionName;
                }
                return null;
            }
        }

        /// <summary>
        /// 座位
        /// </summary>
        public decimal TheSeatingCapacity { get; set; }

        /// <summary>
        /// 商载
        /// </summary>
        public decimal TheCarryingCapacity { get; set; }

        /// <summary>
        /// 座级
        /// </summary>
        public string TheRegional { get; set; }

        /// <summary>
        /// 机型
        /// </summary>
        public string TheAircraftTypeName { get; set; }

        /// <summary>
        /// 引进方式
        /// </summary>
        public string TheActionName { get; set; }

        /// <summary>
        /// 运营权人
        /// </summary>
        public string TheAirlineName { get; set; }

        /// <summary>
        /// 供应商
        /// </summary>
        public string ThenOwnerName { get; set; }

        /// <summary>
        ///在飞机设置注销日期时，将这架飞机的“IsOperation”标记为false，说明飞机已退出运营
        /// </summary>
        partial void OnExportDateChanged()
        {
            if (this.ExportDate != null)
            {
                this.IsOperation = false;
            }
            else this.IsOperation = true;
        }
    }

    public abstract partial class PlanHistory
    {
        protected static IFleetService Service
        {
            get { return ServiceLocator.Current.GetInstance<IFleetService>(); }
        }

        #region 属性

        #region 控制只读属性

        #region 只读条件

        /// <summary>
        /// 已处于审核及之后的计划状态
        /// </summary>
        private bool PlanCheckedCondition
        {
            get { return this.Plan != null && this.Plan.Status > (int)PlanStatus.Checking; }
        }

        /// <summary>
        /// 已处于已提交及之后的计划状态
        /// </summary>
        private bool PlanSubmittedCondition
        {
            get { return this.Plan != null && this.Plan.Status > (int)PlanStatus.Checked; }
        }

        /// <summary>
        /// 已处于申请及之后的管理状态
        /// </summary>
        private bool ManageRequestCondition
        {
            get { return this.PlanAircraft != null && this.PlanAircraft.Status > (int)ManageStatus.Plan; }
        }

        /// <summary>
        /// 计划飞机处于锁定状态
        /// </summary>
        private bool LockCondition
        {
            get { return this.PlanAircraft != null && this.PlanAircraft.IsLock; }
        }

        /// <summary>
        /// 属于现役飞机
        /// </summary>
        private bool OperationCondition
        {
            get { return this.PlanAircraft != null && this.PlanAircraft.Aircraft != null; }
        }

        /// <summary>
        /// 没有下属航空公司
        /// </summary>
        private bool OnlyAirlinesCondition
        {
            get { return (AirlinesCollection.Count() <= 1); }
        }

        /// <summary>
        /// 是退出计划
        /// </summary>
        private bool ExportPlanCondition
        {
            get { return this.ActionType == "退出"; }
        }

        /// <summary>
        /// 是变更计划
        /// </summary>
        private bool ChangePlanCondition
        {
            get { return this is ChangePlan; }
        }

        #endregion

        #region 只读逻辑

        public bool IsPlanChecked
        {
            get { return this.PlanCheckedCondition; }
        }

        public bool IsPlanCheckedOrLock
        {
            get { return this.PlanCheckedCondition || this.LockCondition; }
        }

        public bool IsPlanCheckedOrOnlyAirlines
        {
            get { return this.PlanCheckedCondition || this.OnlyAirlinesCondition; }
        }

        public bool IsPlanCheckedOrOperation
        {
            get { return this.PlanCheckedCondition || this.OperationCondition; }
        }

        public bool IsManageRequestOrPlanSubmitted
        {
            get { return this.ManageRequestCondition; }
        }

        public bool IsOperationAndExportPlan
        {
            get { return this.OperationCondition && this.ExportPlanCondition; }
        }

        #endregion

        #region 可用逻辑

        public bool IsAirlineEnabled
        {
            get { return !this.IsPlanCheckedOrOperation; }
        }

        public bool IsNotOperationOrChangePlan
        {
            get { return !this.OperationCondition || ChangePlanCondition; }
        }

        #endregion

        #endregion

        private string _regional;
        /// <summary>
        /// 机型所对应的座级别（用来控制计划历史所对应的机型）
        /// </summary>
        public string Regional
        {
            get
            {
                if (string.IsNullOrEmpty(_regional))
                    _regional = this.AircraftType == null ? string.Empty : this.AircraftType.AircraftCategory.Regional;
                return _regional;
            }
            set
            {
                _regional = value;
                RaisePropertyChanged("AircraftTypes");
            }
        }

        #region 客户端计算属性

        private string _actionType;
        /// <summary>
        /// 计划明细所对应的操作类型，包括引进、退出、变更
        /// </summary>
        public string ActionType
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this._actionType))
                    this._actionType = this.ActionCategory == null ? string.Empty : this.ActionCategory.ActionType;
                return this._actionType;
            }
            set { this._actionType = value; }
        }

        [Display(Name = "执行时间")]
        public string PerformTime
        {
            get { return this.Annual != null ? this.Annual.Year + "/" + this.PerformMonth : null; }
        }

        [Display(Name = "净增客机")]
        public int DeltaPnr
        {
            get
            {
                if (this.AircraftType == null || this.ActionCategory == null)
                    return 0;
                if (this.AircraftType.AircraftCategory != null && this.AircraftType.AircraftCategory.Category == "客机")
                {
                    switch (this.ActionCategory.ActionName)
                    {
                        case "购买": return 1;
                        case "融资租赁": return 1;
                        case "经营租赁": return 1;
                        case "湿租": return 1;
                        case "经营租赁续租": return 0;
                        case "湿租续租": return 0;
                        case "退役": return -1;
                        case "出售": return -1;
                        case "退租": return -1;
                        case "出租": return -1;
                        case "一般改装": return 0;
                        case "货改客": return 1;
                        case "售后回租": return 0;
                        case "租转购": return 0;
                        default: return 0;
                    }
                }
                if (this.ActionCategory.ActionName == "客改货")
                {
                    return -1;
                }
                return 0;
            }
        }

        [Display(Name = "净增货机")]
        public int DeltaCargo
        {
            get
            {
                if (this.AircraftType == null || this.ActionCategory == null)
                    return 0;
                if (this.AircraftType.AircraftCategory != null && this.AircraftType.AircraftCategory.Category == "货机")
                {
                    switch (this.ActionCategory.ActionName)
                    {
                        case "购买": return 1;
                        case "融资租赁": return 1;
                        case "经营租赁": return 1;
                        case "湿租": return 1;
                        case "经营租赁续租": return 0;
                        case "湿租续租": return 0;
                        case "退役": return -1;
                        case "出售": return -1;
                        case "出租": return -1;
                        case "退租": return -1;
                        case "一般改装": return 0;
                        case "客改货": return 1;
                        case "售后回租": return 0;
                        case "租转购": return 0;
                        default: return 0;
                    }
                }
                if (this.ActionCategory.ActionName == "货改客")
                {
                    return -1;
                }
                return 0;
            }
        }

        /// <summary>
        /// 能否提出申请
        /// 1、可申请
        /// 2、未报计划
        /// 3、已申请
        /// 4、无需申请
        /// </summary>
        public string CanRequest
        {
            get
            {
                if (this.ActionCategory != null && this.ActionCategory.NeedRequest)
                {
                    if (this.PlanAircraft.Status > (int)ManageStatus.Plan) return "3：已申请";
                    return (this.IsSubmit && this.Plan.Status ==(int)OpStatus.Submited) ? "1：可申请" : "2：未报计划";
                }
                return "4：无需申请";
            }
        }

        [Display(Name = "活动类型")]
        public string ActionCategoryOperation
        {
            get
            {
                if (this.ActionCategory != null)
                {
                    return this.ActionCategory.ActionType + "：" + this.ActionCategory.ActionName;
                }
                return null;
            }
        }

        /// <summary>
        /// 能否执行交付操作
        /// 可交付存在两种情形，一种是无需申请的，一种是申请已批复且批准的
        /// 1、可交付
        /// 2、交付中
        /// 3、已交付
        /// 4、未申请
        /// 5、未批复
        /// 6、未批准
        /// </summary>
        public string CanDeliver
        {
            get
            {
                // 1、活动是需要申报的类型
                if (this.ActionCategory != null && this.ActionCategory.NeedRequest)
                {
                    if (this.ApprovalHistory == null) return "4：未申请";
                    if (this.PlanAircraft.Status == (int)ManageStatus.Request) return "5：未批复";
                    if (!this.ApprovalHistory.IsApproved) return "6：未批准";
                    if (this is OperationPlan)
                    {
                        var planDetail = this as OperationPlan;
                        if (planDetail == null) return "1：可交付";
                        if (planDetail.OperationHistory == null) return "1：可交付";
                        return planDetail.OperationHistory.Status == (int)OpStatus.Submited ? "3：已交付" : "2：交付中";
                    }
                    if (this is ChangePlan)
                    {
                        var planDetail = this as ChangePlan;
                        if (planDetail == null) return "1：可交付";
                        if (planDetail.AircraftBusiness == null) return "1：可交付";
                        return planDetail.AircraftBusiness.Status == (int)OpStatus.Submited ? "3：已交付" : "2：交付中";
                    }
                }
                // 2、活动是无需申报的类型
                else
                {
                    if (this is OperationPlan)
                    {
                        var planDetail = this as OperationPlan;
                        if (planDetail == null) return "1：可交付";
                        if (planDetail.OperationHistory == null) return "1：可交付";
                        return planDetail.OperationHistory.Status == (int)OpStatus.Submited ? "3：已交付" : "2：交付中";
                    }
                    if (this is ChangePlan)
                    {
                        var planDetail = this as ChangePlan;
                        if (planDetail == null) return "1：可交付";
                        if (planDetail.AircraftBusiness == null) return "1：可交付";
                        return planDetail.AircraftBusiness.Status == (int)OpStatus.Submited ? "3：已交付" : "2：交付中";
                    }
                }
                return "1：可交付";
            }
        }

        /// <summary>
        /// 计划完成状态
        /// 0：草稿
        /// 1：审核
        /// 2：已审核
        /// 3：已提交
        /// -1：无状态
        /// </summary>
        public int CompleteStatus
        {
            get
            {
                if (this is OperationPlan)
                {
                    var planDetail = this as OperationPlan;
                    if (planDetail == null || planDetail.OperationHistory == null)
                    {
                        return -1;
                    }
                    return planDetail.OperationHistory.Status;
                }
                if (this is ChangePlan)
                {
                    var planDetail = this as ChangePlan;
                    if (planDetail == null || planDetail.AircraftBusiness == null)
                    {
                        return -1;
                    }
                    return planDetail.AircraftBusiness.Status;
                }
                return -1;
            }
        }

        #endregion

        #region 属性绑定

        /// <summary>
        /// 座级集合，用于属性绑定
        /// </summary>
        public IEnumerable<AircraftCategory> AircraftCategores
        {
            get { return Service.GetAircraftCategories(this); }
        }

        /// <summary>
        /// 机型集合，用于属性绑定
        /// </summary>
        public IEnumerable<AircraftType> AircraftTypes
        {
            get { return Service.GetAircraftTypes(this); }
        }

        /// <summary>
        /// 操作集合，用于属性绑定
        /// </summary>
        public IEnumerable<ActionCategory> ActionCategories
        {
            get { return Service.GetActionCategories(this); }
        }

        /// <summary>
        /// 年度集合，用于属性绑定
        /// </summary>
        public IEnumerable<Annual> Annuals
        {
            get { return Service.PerformAnnuals; }
        }

        /// <summary>
        /// 月份集合，用于属性绑定
        /// </summary>
        public List<int> Months
        {
            get { return Service.AllMonths; }
        }

        /// <summary>
        /// 子航空公司集合（包括当前航空公司），用于属性绑定
        /// </summary>
        public IEnumerable<Airlines> AirlinesCollection
        {
            get
            {
                var currentAirlines = Service.CurrentAirlines;
                if (currentAirlines == null) return null;
                var subAirlines = new List<Airlines> { currentAirlines };
                var subs = currentAirlines.SubAirlines.Where(p => p.SubType == 1).ToList();
                if (subs.Any()) subAirlines.AddRange(subs);
                return subAirlines;
            }
        }

        #endregion

        #endregion

        #region 方法

        /// <summary>
        /// 执行年度发生变化时触发相关变化
        /// </summary>
        partial void OnPerformAnnualIDChanged()
        {
            this.RaisePropertyChanged("PerformTime");
        }

        /// <summary>
        /// 执行月份发生变化时触发相关变化
        /// </summary>
        partial void OnPerformMonthChanged()
        {
            this.RaisePropertyChanged("PerformTime");
        }

        /// <summary>
        /// 机型发生变化时触发相关变化
        /// </summary>
        partial void OnAircraftTypeIDChanged()
        {
            if (this.PlanAircraft != null)
            {
                this.PlanAircraft.AircraftTypeID = this.AircraftTypeID;
            }
            this.RaisePropertyChanged("DeltaPnr");
            this.RaisePropertyChanged("DeltaCargo");
        }

        /// <summary>
        /// 操作类别发生变化时触发相关变化
        /// </summary>
        partial void OnActionCategoryIDChanged()
        {
            var actionCategory =
                Service.AllActionCategories.FirstOrDefault(ac => ac.ActionCategoryID == this.ActionCategoryID);
            if (actionCategory != null)
            {
                if (actionCategory.ActionType == "退出")
                {
                    if (this.PlanAircraft != null && this.PlanAircraft.Aircraft != null)
                    {
                        this.SeatingCapacity = -this.PlanAircraft.Aircraft.SeatingCapacity;
                        this.CarryingCapacity = -this.PlanAircraft.Aircraft.CarryingCapacity;
                    }
                }

                if (actionCategory.ActionType == "引进" || actionCategory.ActionType == "退出")
                {
                    this.TargetCategory = actionCategory;
                }
                else
                {
                    RaisePropertyChanged("AircraftCategores");
                    RaisePropertyChanged("AircraftTypes");

                    // 改变目标引进方式
                    switch (actionCategory.ActionName)
                    {
                        case "一般改装":
                            if (this.PlanAircraft != null && this.PlanAircraft.Aircraft != null)
                                this.TargetCategory = this.PlanAircraft.Aircraft.ImportCategory;
                            break;
                        case "客改货":
                            if (this.PlanAircraft != null && this.PlanAircraft.Aircraft != null)
                            {
                                this.TargetCategory = this.PlanAircraft.Aircraft.ImportCategory;
                                this.AircraftType = null;
                            }
                            break;
                        case "货改客":
                            if (this.PlanAircraft != null && this.PlanAircraft.Aircraft != null)
                            {
                                this.TargetCategory = this.PlanAircraft.Aircraft.ImportCategory;
                                this.AircraftType = null;
                            }
                            break;
                        case "售后融资租赁":
                            this.TargetCategory = Service.AllActionCategories.FirstOrDefault(a => a.ActionName == "融资租赁");
                            break;
                        case "售后经营租赁":
                            this.TargetCategory = Service.AllActionCategories.FirstOrDefault(a => a.ActionName == "经营租赁");
                            break;
                        case "租转购":
                            this.TargetCategory = Service.AllActionCategories.FirstOrDefault(a => a.ActionName == "购买");
                            break;
                    }
                    // 修改计划飞机管理状态
                    if (this.PlanAircraft != null)
                    {
                        if (actionCategory.NeedRequest)
                            this.PlanAircraft.Status = (int)ManageStatus.Plan;
                        else
                            this.PlanAircraft.Status = (int)ManageStatus.Operation;
                    }
                }
                this.RaisePropertyChanged("ActionCategoryOperation");
            }
        }

        /// <summary>
        /// 申请明细发生变化时触发相关变化
        /// </summary>
        partial void OnApprovalHistoryIDChanged()
        {
            RaisePropertyChanged("CanRequest");
        }

        #endregion

    }

    public sealed partial class OperationPlan
    {

        #region 方法

        partial void OnOperationHistoryIDChanged()
        {
            RaisePropertyChanged("CompleteStatus");
        }

        #endregion

    }

    public sealed partial class ChangePlan
    {

        #region 方法

        partial void OnAircraftBusinessIDChanged()
        {
            RaisePropertyChanged("CompleteStatus");
        }

        #endregion
    }

    public sealed partial class OwnershipHistory
    {
        private static IFleetService Service
        {
            get { return ServiceLocator.Current.GetInstance<IFleetService>(); }
        }

        #region 属性

        #region 控制只读属性

        #region 只读条件

        /// <summary>
        /// 已处于审核及之后的所有权历史状态
        /// </summary>
        private bool OwnershipCheckedCondition
        {
            get { return this.Status > (int)OpStatus.Checking; }
        }

        #endregion

        #region 只读逻辑

        public bool IsOwnershipChecked
        {
            get { return this.OwnershipCheckedCondition; }
        }

        #endregion

        #endregion

        #region 属性绑定

        /// <summary>
        /// 所有权人集合，用于属性绑定
        /// </summary>
        public IEnumerable<Owner> Owners
        {
            get
            {
                // 获取分支的集合
                var subAirlines = Service.AllOwners.OfType<Airlines>().Where(a => a.MasterID != null);
                return Service.AllOwners.Except(subAirlines);
            }
        }

        #endregion

        #endregion

        #region 方法

        /// <summary>
        /// 新增所有权历史的开始日期变化时更新之前所有权历史的结束日期
        /// </summary>
        partial void OnStartDateChanged()
        {
            if (this.Aircraft != null && this.Aircraft.OwnershipHistorys.Count > 1)
            {
                var ownership = this.Aircraft.OwnershipHistorys.Where(os => os.Status == (int)OpStatus.Submited)
                    .OrderBy(os => os.StartDate)
                    .LastOrDefault();
                if (ownership != null)
                {
                    var startDate = this.StartDate;
                    if (startDate != null) ownership.EndDate = startDate.Value.AddDays(-1);
                }
            }
        }

        /// <summary>
        /// 所有权改变飞机当前所有也相应发生改变
        /// </summary>
        partial void OnOwnerIDChanged()
        {
            if (this.Aircraft != null && this.Owner != null)
            {
                this.Aircraft.Owner = this.Owner;
            }
        }

        #endregion

    }

    public sealed partial class ApprovalHistory
    {
        private static IFleetService Service
        {
            get { return ServiceLocator.Current.GetInstance<IFleetService>(); }
        }

        #region 属性

        #region 控制只读属性

        #region 只读条件

        /// <summary>
        /// 已处于审核及之后的申请状态
        /// </summary>
        private bool RequestCheckedCondition
        {
            get { return this.Request != null && this.Request.Status > (int)ReqStatus.Checking; }
        }

        /// <summary>
        /// 已处于审核及之后的批文状态
        /// </summary>
        private bool ApprovalCheckedCondition
        {
            get
            {
                return this.Request != null && this.Request.ApprovalDoc != null &&
                       this.Request.ApprovalDoc.Status > (int)OpStatus.Checking;
            }
        }

        /// <summary>
        /// 没有下属航空公司
        /// </summary>
        private bool OnlyAirlinesCondition
        {
            get { return (AirlinesCollection.Count() <= 1); }
        }

        #endregion

        #region 只读逻辑

        public bool IsRequestChecked
        {
            get { return this.RequestCheckedCondition; }
        }

        public bool IsRequestCheckedOrOnlyAirlines
        {
            get { return this.RequestCheckedCondition || this.OnlyAirlinesCondition; }
        }

        public bool IsApprovalChecked
        {
            get { return this.ApprovalCheckedCondition; }
        }

        #endregion

        #endregion

        #region 客户端计算属性

        [Display(Name = "申请交付时间")]
        public string RequestDeliver
        {
            get { return this.Annual != null ? this.Annual.Year + "/" + this.RequestDeliverMonth : null; }
        }

        [Display(Name = "飞机投入运营情况")]
        public string RequstOperation
        {
            get { return this.OperationHistory == null ? null : "飞机运营情况"; }
        }

        [Display(Name = "活动类型")]
        public string ActionCategoryOperation
        {
            get
            {
                if (this.ImportCategory != null)
                {
                    return this.ImportCategory.ActionType + "：" + this.ImportCategory.ActionName;
                }
                return null;
            }
        }

        #endregion

        #region 属性绑定

        /// <summary>
        /// 机型集合，用于属性绑定
        /// </summary>
        public IEnumerable<AircraftType> AircraftTypes
        {
            get
            {
                return
                    Service.AllAircraftTypes.Where(
                        p => p.AircraftCategory.Regional == this.PlanAircraft.AircraftType.AircraftCategory.Regional);
            }
        }

        /// <summary>
        /// 引进方式，用于属性绑定
        /// </summary>
        public IEnumerable<ActionCategory> ActionCategories
        {
            get
            {
                var planDetail =
                    Service.EntityContainer.GetEntitySet<PlanHistory>()
                           .FirstOrDefault(ph => ph.ApprovalHistoryID == this.ApprovalHistoryID);
                return planDetail != null ? Service.GetActionCategories(planDetail) : null;
            }
        }

        /// <summary>
        /// 年度集合，用于属性绑定
        /// </summary>
        public IEnumerable<Annual> Annuals
        {
            get { return Service.PerformAnnuals; }
        }

        /// <summary>
        /// 月份集合，用于属性绑定
        /// </summary>
        public List<int> Months
        {
            get { return Service.AllMonths; }
        }

        /// <summary>
        ///获取子航空公司（包括当前航空公司）
        /// </summary>
        public IEnumerable<Airlines> AirlinesCollection
        {
            get
            {
                var currentAirlines = Service.CurrentAirlines;
                if (currentAirlines == null) return null;
                var subAirlines = new List<Airlines> { currentAirlines };
                var subs = currentAirlines.SubAirlines.Where(p => p.SubType == 1).ToList();
                if (subs.Any()) subAirlines.AddRange(subs);
                return subAirlines;
            }
        }

        #endregion

        #endregion

        #region 方法

        partial void OnRequestDeliverAnnualIDChanged()
        {
            this.RaisePropertyChanged("RequestDeliver");
        }

        partial void OnRequestDeliverMonthChanged()
        {
            this.RaisePropertyChanged("RequestDeliver");
        }

        partial void OnAirlinesIDChanged()
        {
            if (this.PlanAircraft != null)
            {
                this.PlanAircraft.AirlinesID = this.Airlines.OwnerID;
            }
        }

        partial void OnIsApprovedChanged()
        {
            if (this.PlanAircraft != null)
            {
                if (this.IsApproved)
                {
                    Service.ApproveRequest(this);
                }
                else
                {
                    Service.RejectRequest(this);
                }
            }
        }

        #endregion

    }

    public sealed partial class Agreement
    {
        [Display(Name = "计划启动谈判")]
        public string PlanNegotiateStart
        {
            get { return this.PlanAnnual != null ? this.PlanAnnual.Year + "/" + this.PlanAgreeMonth : null; }
        }
        partial void OnPlanAgreeAnnualIDChanged()
        {
            this.RaisePropertyChanged("PlanNegotiateStart");
        }
        partial void OnPlanAgreeMonthChanged()
        {
            this.RaisePropertyChanged("PlanNegotiateStart");
        }

        [Display(Name = "实际启动谈判")]
        public string ActualnegotiateStart
        {
            get { return this.ActualAnnual != null ? this.ActualAnnual.Year + "/" + this.ActualAgreeMonth : null; }
        }
        partial void OnActualAgreeAnnualIDChanged()
        {
            this.RaisePropertyChanged("ActualnegotiateStart");
        }
        partial void OnActualAgreeMonthChanged()
        {
            this.RaisePropertyChanged("ActualnegotiateStart");
        }
    }

    public sealed partial class AgreementDetail
    {
        [Display(Name = "协议交付时间")]
        public string AgreementDeliver
        {
            get { return this.Annual != null ? this.Annual.Year + "/" + this.PlanDeliverMonth : null; }
        }
        partial void OnPlanDeliverAnnualIDChanged()
        {
            this.RaisePropertyChanged("AgreementDeliver");
        }
        partial void OnPlanDeliverMonthChanged()
        {
            this.RaisePropertyChanged("AgreementDeliver");
        }
    }

    public sealed partial class OperationHistory
    {
        private static IFleetService Service
        {
            get { return ServiceLocator.Current.GetInstance<IFleetService>(); }
        }

        #region 属性

        #region 控制只读属性

        #region 只读条件

        /// <summary>
        /// 运营历史处于已审核之后的状态
        /// </summary>
        private bool OperationHistoryCheckedCondition
        {
            get { return this.Status > (int)OpStatus.Checking; }
        }

        /// <summary>
        /// 引进类型
        /// 退出类型为空时，属于引进
        /// </summary>
        private bool ImportCondition
        {
            get { return this.ExportCategory == null; }
        }

        /// <summary>
        /// 退出类型
        /// 退出类型非空时，属于退出
        /// </summary>
        private bool ExportCondition
        {
            get { return this.ExportCategory != null; }
        }

        #endregion

        #region 只读逻辑

        public bool IsOperationHistoryChecked
        {
            get { return this.OperationHistoryCheckedCondition; }
        }

        public bool IsOperationHistoryCheckedOrExportCondition
        {
            get { return (this.OperationHistoryCheckedCondition || this.ExportCondition); }
        }

        public bool IsOperationHistoryCheckedOrImportCondition
        {
            get { return (this.OperationHistoryCheckedCondition || this.ImportCondition); }
        }

        public bool IsOperationHistoryNotCheckedAndExportCondition
        {
            get { return (!(this.OperationHistoryCheckedCondition) && this.ExportCondition); }
        }

        public bool IsOperationHistoryNotCheckedAndImportCondition
        {
            get { return (!(this.OperationHistoryCheckedCondition) && this.ImportCondition); }
        }

        #endregion

        #endregion

        #region 属性绑定

        /// <summary>
        /// 引进方式集合，用于属性绑定
        /// </summary>
        public IEnumerable<ActionCategory> ImportTypes
        {
            get { return Service.AllActionCategories.Where(a => a.ActionType == "引进"); }
        }

        /// <summary>
        /// 退出方式集合，用于属性绑定
        /// </summary>
        public IEnumerable<ActionCategory> ExportTypes
        {
            get { return Service.AllActionCategories.Where(a => a.ActionType == "退出"); }
        }

        /// <summary>
        ///获取子航空公司（包括当前航空公司）
        /// </summary>
        public IEnumerable<Airlines> AirlinesCollection
        {
            get
            {
                var currentAirlines = Service.CurrentAirlines;
                if (currentAirlines == null) return null;
                var subAirlines = new List<Airlines> { currentAirlines };
                var subs = currentAirlines.SubAirlines.Where(p => p.SubType == 1).ToList();
                if (subs.Any()) subAirlines.AddRange(subs);
                return subAirlines;
            }
        }

        #endregion

        #endregion

        #region 方法

        /// <summary>
        /// 新增运营历史的引进方式变化时更新相关飞机的引进方式
        /// </summary>
        partial void OnImportCategoryIDChanged()
        {
            if (this.Aircraft != null)
            {
                this.Aircraft.ImportCategory = this.ImportCategory;
            }
        }

        /// <summary>
        /// 新增运营历史，接收日期等于飞机引进日期
        /// </summary>
        partial void OnReceiptDateChanged()
        {
            if (this.Aircraft != null)
            {
                // 如果运营历史记录只有一条，更新飞机引进日期；否则更新前一条运营历史的结束日期。
                if (this.Aircraft.OperationHistories.Count == 1)
                {
                    this.Aircraft.ImportDate = this.ReceiptDate;
                }
            }
        }

        /// <summary>
        /// 新增运营历史的开始日期变化时更新相关飞机的引进日期或之前运营历史的结束日期
        /// </summary>
        partial void OnStartDateChanged()
        {
            if (this.Aircraft == null) return;
            var operationHistory =
                this.Aircraft.OperationHistories.Where(oh => oh.Status == (int)OpStatus.Submited)
                    .OrderBy(oh => oh.StartDate)
                    .LastOrDefault();
            if (operationHistory != null)
            {
                var startDate = this.StartDate;
                if (startDate != null) operationHistory.EndDate = startDate.Value.AddDays(-1);
            }
        }

        /// <summary>
        /// 结束运营时，更新相关飞机的结束日期
        /// 如果是退出，更新相关商业数据历史的结束日期
        /// 不是退出的不更新商业数据历史的结束日期，因为有可能商业数据还没有结束，例如续租
        /// </summary>
        partial void OnEndDateChanged()
        {
            if (this.Aircraft != null)
            {
                if (this.ExportCategory != null && this.ExportCategory.ActionName != "出租")
                {
                    this.Aircraft.ExportDate = this.EndDate;
                }
                // 如果退出方式不为空，证明是退出
                if (this.ExportCategory != null)
                {
                    var aircraftBusiness = this.Aircraft.AircraftBusinesses.LastOrDefault(ab => ab.EndDate == null);
                    if (aircraftBusiness != null) aircraftBusiness.EndDate = this.EndDate;
                }
                // 如果存在运力分配记录，修改最后一条运力分配记录的结束日期
                if (this.SubOperationHistories != null)
                {
                    var lastSubOperation = this.SubOperationHistories.OrderBy(soh => soh.StartDate).LastOrDefault();
                    lastSubOperation.EndDate = this.EndDate;
                }
            }
        }

        partial void OnRegNumberChanged()
        {
            if (this.Aircraft != null && !string.IsNullOrEmpty(this.RegNumber))
            {
                this.Aircraft.RegNumber = this.RegNumber;
            }
        }

        partial void OnAirlinesIDChanged()
        {
            if (this.Aircraft != null)
            {
                this.Aircraft.AirlinesID = this.Airlines.OwnerID;
                if (this.Aircraft.PlanAircrafts.Any())
                {
                    var planAircraft = this.Aircraft.PlanAircrafts.FirstOrDefault();
                    if (planAircraft != null) planAircraft.AirlinesID = this.Airlines.OwnerID;
                }
            }
        }

        #endregion

    }

    public sealed partial class SubOperationHistory
    {
        private static IFleetService Service
        {
            get { return ServiceLocator.Current.GetInstance<IFleetService>(); }
        }

        #region 属性

        #region 控制只读属性

        #region 只读条件

        /// <summary>
        /// 运力分配记录处于已审核状态
        /// </summary>
        private bool OperationCheckedCondition
        {
            get { return this.Status >= (int)OpStatus.Checked; }
        }

        #endregion

        #region 只读逻辑

        public bool IsOperationChecked
        {
            get { return this.OperationCheckedCondition; }
        }

        #endregion

        #endregion


        #region 属性绑定

        /// <summary>
        /// 分公司集合，用于属性绑定
        /// </summary>
        public IEnumerable<Airlines> AirlinesCollection
        {
            get
            {
                var currentAirlines = Service.CurrentAirlines;
                if (currentAirlines != null)
                {
                    var subAirlines = new List<Airlines> { currentAirlines };
                    var subs = currentAirlines.SubAirlines.Where(p => p.SubType == 0 && p.Status == 0).ToList();
                    if (subs.Any()) subAirlines.AddRange(subs);
                    return subAirlines;
                }
                return null;
            }
        }

        #endregion

        #endregion

        #region 方法

        partial void OnStartDateChanged()
        {
            if (this.OperationHistory == null) return;
            var subOperation =
                this.OperationHistory.SubOperationHistories.Where(soh => soh.Status == (int)OpStatus.Checked)
                    .OrderBy(soh => soh.StartDate)
                    .LastOrDefault();
            if (subOperation != null)
            {
                var startDate = this.StartDate;
                if (startDate != null) subOperation.EndDate = startDate.Value.AddDays(-1);
            }
        }

        #endregion

    }

    public sealed partial class AircraftBusiness
    {
        private static IFleetService Service
        {
            get { return ServiceLocator.Current.GetInstance<IFleetService>(); }
        }

        #region 属性

        #region 控制只读属性

        #region 只读条件

        /// <summary>
        /// 商业数据历史处于已审核之后的状态
        /// </summary>
        private bool AircraftBusinessCheckedCondition
        {
            get { return this.Status > (int)OpStatus.Checking; }
        }

        #endregion

        #region 只读逻辑

        public bool IsAircraftBusinessChecked
        {
            get { return this.AircraftBusinessCheckedCondition; }
        }

        public bool IsAircraftBusinessNotChecked
        {
            get { return !(this.AircraftBusinessCheckedCondition); }
        }

        #endregion

        #endregion

        private string _regional;
        /// <summary>
        /// 机型所对应的座级别（用来控制计划历史所对应的机型）
        /// </summary>
        public string Regional
        {
            get
            {
                if (string.IsNullOrEmpty(_regional))
                {
                    _regional = this.AircraftType == null ? string.Empty : this.AircraftType.AircraftCategory.Regional;
                }
                return _regional;
            }
            set
            {
                _regional = value;
                RaisePropertyChanged("AircraftTypes");
            }
        }

        #region 属性绑定

        /// <summary>
        /// 引进方式集合，用于属性绑定
        /// </summary>
        public IEnumerable<ActionCategory> ImportTypes
        {
            get { return Service.AllActionCategories.Where(a => a.ActionType == "引进"); }
        }

        /// <summary>
        /// 座级集合，用于属性绑定
        /// </summary>
        public IEnumerable<AircraftCategory> AircraftCategores
        {
            get
            {
                return Service.AllAircraftCategories;
            }
        }

        /// <summary>
        /// 机型集合，用于属性绑定
        /// </summary>
        public IEnumerable<AircraftType> AircraftTypes
        {
            get
            {
                return string.IsNullOrEmpty(Regional)
                           ? null
                           : Service.AllAircraftTypes.Where(p => p.AircraftCategory.Regional == Regional);
            }
        }

        #endregion

        #endregion

        #region 方法

        partial void OnAircraftTypeIDChanged()
        {
            if (this.Aircraft == null) return;
            this.Aircraft.AircraftTypeID = this.AircraftTypeID;
        }

        partial void OnStartDateChanged()
        {
            //新增的商业数据，在新的开始时间， 等于上一条已提交的结束时间
            if (this.Aircraft != null && this.Aircraft.AircraftBusinesses.Any())
            {
                var oh = this.Aircraft.AircraftBusinesses.Where(o => o.Status == (int)OpStatus.Submited).OrderBy(o => o.StartDate).LastOrDefault();
                if (oh != null && this.StartDate != null)
                {
                    oh.EndDate = this.StartDate.Value.AddDays(-1);
                }
            }
        }

        partial void OnSeatingCapacityChanged()
        {
            if (this.Aircraft != null)
            {
                this.Aircraft.SeatingCapacity = this.SeatingCapacity;
            }
        }

        partial void OnCarryingCapacityChanged()
        {
            if (this.Aircraft != null)
            {
                this.Aircraft.CarryingCapacity = this.CarryingCapacity;
            }
        }

        #endregion

    }

    #endregion

    #region 机队管理

    public sealed partial class Request
    {

        #region 属性

        #region 控制只读属性

        /// <summary>
        /// 申请审核后只读
        /// </summary>
        public bool IsRequestChecked
        {
            get { return this.Status > (int)ReqStatus.Checking; }
        }

        /// <summary>
        /// 控制申请在有批文的情况的只读状态
        /// </summary>
        public bool IsApprovalDocActive
        {
            get
            {
                if (this.ApprovalDoc != null)
                {
                    return this.ApprovalDoc.Status >= (int)ReqStatus.Checked;
                }
                return true;
            }
        }

        #endregion

        #region 客户端计算属性

        public string RequestState
        {
            get
            {
                switch (Status)
                {
                    case 0:
                        return "1、草稿";
                    case 1:
                        return "2、审核";
                    case 2:
                        return "3、已审核";
                    case 3:
                        return "4、已提交";
                    case 4:
                        return "5、已审批";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        #endregion

        #endregion

        #region 方法

        partial void OnStatusChanged()
        {
            this.RaisePropertyChanged("RequestState");
        }

        #endregion

    }

    public sealed partial class Plan
    {
        /// <summary>
        /// 非审核状态下计划的属性都是只读
        /// </summary>
        public bool IsActive
        {
            get
            {
                return this.Status != (int)PlanStatus.Checked;
            }
        }
    }

    public sealed partial class ApprovalDoc
    {
        private static IFleetService Service
        {
            get { return ServiceLocator.Current.GetInstance<IFleetService>(); }
        }

        /// <summary>
        /// 控制只读属性
        /// </summary>
        public bool IsApprovalChecked
        {
            get { return this.Status > (int)OpStatus.Checking; }
        }

        /// <summary>
        /// 管理者集合，用于属性绑定
        /// </summary>
        public IEnumerable<Manager> Managers
        {
            get
            {
                return Service.AllManagers;
            }
        }

    }

    #endregion

}
