using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Windows.Data.DomainServices;
using Telerik.Windows.Controls;
using UniCloud.AFRP.Views;
using UniCloud.Fleet.Models;
using UniCloud.Fleet.Services;
using UniCloud.Infrastructure;

namespace UniCloud.AFRP.ViewModels
{
    [Export(typeof(AfrpPlanLayViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AfrpPlanLayViewModel : EditViewModelBase, IPartImportsSatisfiedNotification, INavigationAware
    {

        #region Local

        [Import]
        public IRegionManager regionManager;
        [Import]
        public PlanDetailEditDialog editDialog;

        private IDictionary<string, bool> _viewButtonValidity;
        internal Plan CurrentPlan { get; private set; }
        private PlanAircraft _planAircraft;
        private OperationPlan _operationPlan;
        private ChangePlan _changePlan;

        #region Property

        #endregion

        #region Method

        #region 加载数据

        private void LoadCompleted()
        {
            this._loadedPlan = false;
            this._loadedRequest = false;
            this.RefreshView();
        }

        private bool _loadedPlan;

        private void LoadPlan()
        {
            this.service.LoadPlan(new QueryBuilder<Plan>(), lo =>
            {
                if (lo.Error == null)
                {
                    this._loadedPlan = true;
                    // 判断是否所有加载操作都完成
                    if (this._loadedPlan && this._loadedRequest)
                    {
                        LoadCompleted();
                    }
                }
            }, null);
        }

        private bool _loadedRequest;

        private void LoadRequest()
        {
            this.service.LoadRequest(new QueryBuilder<Request>(), lo =>
                {
                    if (lo.Error == null)
                    {
                        this._loadedRequest = true;
                        // 判断是否所有加载操作都完成
                        if (this._loadedRequest && this._loadedPlan)
                        {
                            LoadCompleted();
                        }
                    }
                }, null);
        }

        #endregion

        #region 管理按钮状态

        /// <summary>
        /// 刷新按钮状态
        /// </summary>
        protected override void RefreshButtonState()
        {
            this.NewPlanCommand.RaiseCanExecuteChanged();
            this.AddCommand.RaiseCanExecuteChanged();
            this.RemoveCommand.RaiseCanExecuteChanged();
            this.CommitCommand.RaiseCanExecuteChanged();
            this.AttachCommand.RaiseCanExecuteChanged();
            this.ExamineCommand.RaiseCanExecuteChanged();
        }

        #endregion

        #region 管理界面操作

        private void RefreshView()
        {
            this.CurrentPlan = this.service.CurrentPlan;
            RaisePropertyChanged(() => this.PlanTitle);
            this._needReFreshViewPlanHistory = true;
            this.RaiseViewPlanHistory();
            this._needReFreshViewPlanAircraft = true;
            this.RaiseViewPlanAircraft();

            this.RefreshButtonState();
        }

        /// <summary>
        /// 打开子窗体之前判断是否要打开
        /// </summary>
        /// <param name="planAircraft">计划飞机</param>
        /// <param name="source">调用的来源</param>
        internal void OpenEditDialog(PlanAircraft planAircraft, PlanDetailCreateSource source)
        {
            if (this.ViewPlanHistory == null || (!this.ViewPlanHistory.Any(p => p.HasValidationErrors)))
            {
                this._planAircraft = planAircraft;
                this._operationPlan = null;
                this._changePlan = null;
                // 获取计划飞机在当前计划中的明细项集合
                var planDetails = new List<PlanHistory>();
                if(planAircraft!=null)
                planDetails = this.ViewPlanHistory.Where(ph => ph.PlanAircraft == planAircraft).ToList();

                // 1、计划飞机在当前计划中没有明细项
                if (!planDetails.Any())
                    this.ShowEditDialog(null, source);
                // 2、计划飞机在当前计划中已有明细项
                else
                {
                    if (planDetails.Count == 1)
                    {
                        string content;
                        switch (source)
                        {
                            case PlanDetailCreateSource.New:
                                break;
                            case PlanDetailCreateSource.PlanAircraft:
                                content = "计划飞机在当前计划中已有引进计划明细项，是否要针对该计划飞机添加退出计划明细项？";
                                RadWindow.Confirm(this.SetConfirm("确认添加计划明细", "确认", "取消", content, 13, 250, (o, e) =>
                                    {
                                        if (e.DialogResult == true)
                                            this.ShowEditDialog(planDetails[0], source);
                                    }));
                                break;
                            case PlanDetailCreateSource.Aircraft:
                                content = planDetails[0].ActionType == "变更"
                                              ? "飞机在当前计划中已有变更计划明细项，是否要针对该飞机添加退出计划明细项？"
                                              : "飞机在当前计划中已有退出计划明细项，是否要针对该飞机添加变更计划明细项？";
                                RadWindow.Confirm(this.SetConfirm("确认添加计划明细", "确认", "取消", content, 13, 250, (o, e) =>
                                    {
                                        if (e.DialogResult == true)
                                            this.ShowEditDialog(planDetails[0], source);
                                    }));
                                break;
                            default:
                                throw new ArgumentOutOfRangeException("source");
                        }
                    }
                    else
                    {
                        RadWindow.Alert(this.SetAlter("提醒", "确认", "该计划飞机已有两条明细项，不能再添加新计划明细项！", 13, 250, () => { }));
                    }
                }
            }
        }

        private void ShowEditDialog(PlanHistory existDetail, PlanDetailCreateSource source)
        {
            switch (source)
            {
                case PlanDetailCreateSource.New:
                    this.IsPlanTypeVisible = Visibility.Collapsed;
                    this._operationPlan = this.service.CreateOperationPlan(this.CurrentPlan, this._planAircraft, "引进");
                    this.PlanDetail = this._operationPlan;
                    this.IsChangeable = true;
                    break;
                case PlanDetailCreateSource.PlanAircraft:
                    this.IsPlanTypeVisible = Visibility.Collapsed;
                    // 计划飞机已有的明细项肯定是引进计划，只能添加退出计划
                    this._operationPlan = this.service.CreateOperationPlan(this.CurrentPlan, this._planAircraft, existDetail != null ? "退出" : "引进");
                    this.PlanDetail = this._operationPlan;
                    //这时不能修改机型和座机
                    this.IsChangeable = false;
                    break;
                case PlanDetailCreateSource.Aircraft:
                    this.IsPlanTypeVisible = Visibility.Visible;
                    // 1、计划飞机已有明细项
                    if (existDetail != null)
                    {
                        // 已有的是变更计划，只能添加退出计划
                        if (existDetail.ActionType == "变更")
                        {
                            this.IsChangeEnabled = false;
                            this.IsOperation = true;
                            this.IsChange = false;
                            this.OnOperation();
                        }
                        // 已有的是退出计划，只能添加变更计划
                        else
                        {
                            this.IsOperationEnabled = false;
                            this.IsOperation = false;
                            this.IsChange = true;
                            this.OnOperation();//生成之后，不让用户编辑，起到保存原计划历史的机型的作用，在取消时，能够用来恢复计划飞机数据
                            this.OnChange();
                        }
                    }
                    // 2、计划飞机没有明细项
                    else
                    {
                        this.IsChangeEnabled = true;
                        this.IsOperationEnabled = true;
                        if (!this.IsOperation) this.IsOperation = true;
                        this.OnOperation();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("source");
            }

            this.editDialog.ShowDialog();
        }

        #endregion

        #region 调用业务逻辑处理

        private void OnOperation()
        {
            if (this._operationPlan == null)
            {
                // 针对运营飞机的运营计划只能是退出
                this._operationPlan = this.service.CreateOperationPlan(this.CurrentPlan, this._planAircraft, "退出");
            }
            this.PlanDetail = this._operationPlan;
            this.IsChangeable = false;
        }

        private void OnChange()
        {
            if (this._changePlan == null)
                this._changePlan = this.service.CreateChangePlans(this.CurrentPlan, this._planAircraft, "变更");
            this.PlanDetail = this._changePlan;
            this.IsChangeable = true;
        }

        #endregion

        #endregion

        #endregion

        #region ViewModel

        #region 数据集合

        #region 当前计划的计划历史明细

        /// <summary>
        /// 当前计划的计划历史明细
        /// </summary>
        public IEnumerable<PlanHistory> ViewPlanHistory
        {
            get
            {
                return
                    this.service.EntityContainer.GetEntitySet<PlanHistory>()
                        .OrderBy(ph => ph.PerformTime)
                        .Where(ph => ph.Plan == this.CurrentPlan);
            }
        }

        private bool _needReFreshViewPlanHistory = true;

        private void RaiseViewPlanHistory()
        {
            if (this._needReFreshViewPlanHistory)
            {
                RaisePropertyChanged(() => this.ViewPlanHistory);
                this._needReFreshViewPlanHistory = false;
            }
        }

        private PlanHistory _selPlanHistory;
        /// <summary>
        /// 选中的当前计划的计划历史明细
        /// </summary>
        public PlanHistory SelPlanHistory
        {
            get { return this._selPlanHistory; }
            set
            {
                if (this._selPlanHistory != value)
                {
                    this._selPlanHistory = value;
                    this.RaisePropertyChanged(() => this.SelPlanHistory);
                    this.RemoveCommand.RaiseCanExecuteChanged();
                    if (this._selPlanHistory == null) return;
                    if (this.ViewPlanAircraft.Any(pa => pa == this._selPlanHistory.PlanAircraft))
                        this.SelPlanAircraft = this._selPlanHistory.PlanAircraft;
                    var aircraft = this._selPlanHistory.PlanAircraft.Aircraft;
                    if (aircraft != null && this.ViewAircraft.Any(a => a == aircraft))
                        this.SelAircraft = aircraft;
                }
            }
        }

        #endregion

        #region 运营飞机

        /// <summary>
        /// 运营飞机
        /// </summary>
        public IEnumerable<Aircraft> ViewAircraft
        {
            get { return this.service.EntityContainer.GetEntitySet<Aircraft>().Where(a => a.IsOperation); }
        }

        private bool _needReFreshViewAircraft = true;

        private void RaiseViewAircraft()
        {
            if (this._needReFreshViewAircraft)
            {
                RaisePropertyChanged(() => this.ViewAircraft);
                this._needReFreshViewAircraft = false;
            }
        }

        private Aircraft _selAircraft;

        /// <summary>
        /// 选中的运营飞机
        /// </summary>
        public Aircraft SelAircraft
        {
            get { return this._selAircraft; }
            set
            {
                if (this._selAircraft != value)
                {
                    _selAircraft = value;
                    this.RaisePropertyChanged(() => this.SelAircraft);
                }
            }
        }

        #endregion

        #region 计划飞机

        /// <summary>
        /// 计划飞机集合
        /// </summary>
        public IEnumerable<PlanAircraft> ViewPlanAircraft
        {
            get
            {
                return
                  this.service.EntityContainer.GetEntitySet<PlanAircraft>().OrderBy(pa => pa.Status)
                  .Where(pa => pa.IsOwn && pa.Aircraft == null);
            }
        }

        private bool _needReFreshViewPlanAircraft = true;

        private void RaiseViewPlanAircraft()
        {
            if (this._needReFreshViewPlanAircraft)
            {
                RaisePropertyChanged(() => this.ViewPlanAircraft);
                this._needReFreshViewPlanAircraft = false;
            }
        }

        private PlanAircraft _selPlanAircraft;

        /// <summary>
        /// 选中的计划飞机
        /// </summary>
        public PlanAircraft SelPlanAircraft
        {
            get { return this._selPlanAircraft; }
            set
            {
                if (this._selPlanAircraft != value)
                {
                    _selPlanAircraft = value;
                    this.RaisePropertyChanged(() => this.SelPlanAircraft);
                }
            }
        }

        #endregion

        #endregion

        #region Property

        #region 计划标题

        /// <summary>
        /// 计划标题
        /// </summary>
        public string PlanTitle
        {
            get
            {
                if (this.CurrentPlan != null)
                {
                    var sb = new StringBuilder();
                    sb.Append(this.CurrentPlan.Title);
                    sb.Append("【版本");
                    sb.Append(this.CurrentPlan.VersionNumber);
                    sb.Append("】");
                    return sb.ToString();
                }
                return null;
            }
        }

        #endregion

        #region 当前编辑的计划明细项

        private PlanHistory _planDetail;

        /// <summary>
        /// 当前编辑的计划明细项
        /// </summary>
        public PlanHistory PlanDetail
        {
            get { return this._planDetail; }
            private set
            {
                if (this._planDetail != value)
                {
                    this._planDetail = value;
                    this.RaisePropertyChanged(() => this.PlanDetail);
                }
            }
        }

        #endregion

        #region 选中运营计划

        private bool _isOperation;

        /// <summary>
        /// 选中运营计划
        /// </summary>
        public bool IsOperation
        {
            get { return this._isOperation; }
            private set
            {
                if (this._isOperation != value)
                {
                    this._isOperation = value;
                    this.RaisePropertyChanged(() => this.IsOperation);
                    this.OnOperation();
                }
            }
        }

        #endregion

        #region 选中变更计划

        private bool _isChange;

        /// <summary>
        /// 选中变更计划
        /// </summary>
        public bool IsChange
        {
            get { return this._isChange; }
            private set
            {
                if (this._isChange != value)
                {
                    this._isChange = value;
                    this.RaisePropertyChanged(() => this.IsChange);
                    this.OnChange();
                }
            }
        }

        #endregion

        #region 运营计划按钮是否可用

        private bool _isOperationEnabled;

        /// <summary>
        /// 运营计划按钮是否可用
        /// </summary>
        public bool IsOperationEnabled
        {
            get { return this._isOperationEnabled; }
            private set
            {
                if (this._isOperationEnabled != value)
                {
                    this._isOperationEnabled = value;
                    this.RaisePropertyChanged(() => this.IsOperationEnabled);
                }
            }
        }

        #endregion

        #region 变更计划按钮是否可用

        private bool _isChangeEnabled;

        /// <summary>
        /// 变更计划按钮是否可用
        /// </summary>
        public bool IsChangeEnabled
        {
            get { return this._isChangeEnabled; }
            private set
            {
                if (this._isChangeEnabled != value)
                {
                    this._isChangeEnabled = value;
                    this.RaisePropertyChanged(() => this.IsChangeEnabled);
                }
            }
        }

        #endregion

        #region 是否隐藏Radio按钮

        private Visibility _isPlanTypeVisible = Visibility.Collapsed;

        /// <summary>
        /// 是否隐藏Radio按钮
        /// </summary>
        public Visibility IsPlanTypeVisible
        {
            get { return this._isPlanTypeVisible; }
            private set
            {
                if (this._isPlanTypeVisible != value)
                {
                    this._isPlanTypeVisible = value;
                    this.RaisePropertyChanged(() => this.IsPlanTypeVisible);
                }
            }
        }

        #endregion

        #region 控制编辑计划飞机的退出计划时，控制座机、机型是否可编辑

        private bool _isAicraftTypeAndRegionalChangeable = true;

        /// <summary>
        /// 座级、机型是否可改
        /// </summary>
        public bool IsAicraftTypeAndRegionalChangeable
        {
            get { return this._isAicraftTypeAndRegionalChangeable; }
            private set
            {
                if (this._isAicraftTypeAndRegionalChangeable != value)
                {
                    this._isAicraftTypeAndRegionalChangeable = value;
                    this.RaisePropertyChanged(() => this.IsAicraftTypeAndRegionalChangeable);
                }
            }
        }

        private bool _isChangeable = true;

        /// <summary>
        /// 座级、机型是否可改
        /// </summary>
        public bool IsChangeable
        {
            get { return _isChangeable; }
            private set
            {
                if (this._isChangeable != value)
                {
                    this._isChangeable = value;
                    this.RaisePropertyChanged(() => this.IsChangeable);
                    this.IsAicraftTypeAndRegionalChangeable = (this.PlanDetail.IsNotOperationOrChangePlan && this._isChangeable);
                }
            }
        }

        #endregion

        #endregion

        #region Command

        #region 重载命令

        #region 实体变更

        protected override void OnEntityHasChanges()
        {
            RefreshButtonState();
        }

        #endregion

        #region 放弃更改

        protected override void OnAbortExecuted(object sender)
        {
            this._needReFreshViewPlanAircraft = true;
            RaiseViewPlanAircraft();
            this.service.SetCurrentAnnual();
            this.service.SetCurrentPlan();
            this.RefreshView();
        }

        #endregion

        #region 确认

        protected override void OnOkExecute(object sender)
        {
            var planHistory = sender as PlanHistory;
            if (planHistory != null && planHistory.HasValidationErrors) return;

            // 已经创建了两个实例时，需要移除其中不用的一个
            if (this._operationPlan != null && this._changePlan != null)
            {
                if (planHistory is OperationPlan)
                {
                    this.PlanDetail.PlanAircraft.AircraftType = this._operationPlan.AircraftType;
                    this.service.EntityContainer.GetEntitySet<PlanHistory>().Remove(this._changePlan);
                }
                else
                    this.service.EntityContainer.GetEntitySet<PlanHistory>().Remove(this._operationPlan);
            }

            this.editDialog.Close();

            this._needReFreshViewPlanHistory = true;
            RaiseViewPlanHistory();
            RaiseViewPlanAircraft();

            this.SelPlanHistory = planHistory;
        }

        protected override bool CanOk(object obj)
        {
            return obj != null;
        }

        #endregion

        #region 取消

        protected override void OnCancelExecute(object sender)
        {
            var planHistory = sender as PlanHistory;
            if (planHistory != null && planHistory is OperationPlan && planHistory.PlanAircraft != null)
            {
                if(planHistory.PlanAircraft.PlanHistories.Count()==1)
                    this.service.EntityContainer.GetEntitySet<PlanAircraft>().Remove(planHistory.PlanAircraft);
            }
            if (this._changePlan != null && this._operationPlan != null)
            {
                this.PlanDetail.PlanAircraft.AircraftType = this._operationPlan.AircraftType;
                this.service.EntityContainer.GetEntitySet<PlanHistory>().Remove(this._changePlan);
            }
            if (this._operationPlan != null)
                this.service.EntityContainer.GetEntitySet<PlanHistory>().Remove(this._operationPlan);

            this.editDialog.Close();
            this._needReFreshViewPlanAircraft = false;
        }

        #endregion

        #endregion

        #region 创建新版本计划

        /// <summary>
        /// 创建新版本计划
        /// </summary>
        public DelegateCommand<object> NewPlanCommand { get; private set; }

        private void OnNewPlan(object obj)
        {
            var title = this.service.CurrentAnnual.Year + AppConfig.planTitle;
            service.CreateNewVersionPlan(title);


            this.RefreshView();
        }

        private bool CanNewPlan(object obj)
        {
            if (!this._viewButtonValidity["创建新版本计划"]) return false;
            // 有未保存内容时，按钮不可用
            if (this.service.EntityContainer.HasChanges) return false;
            // 操作台当前计划已到已审核状态时，按钮可用
            return this.CurrentPlan != null
                   && this.CurrentPlan.Status > (int)PlanStatus.Checking;
        }

        #endregion

        #region 添加计划项

        /// <summary>
        /// 添加计划项
        /// </summary>
        public DelegateCommand<object> AddCommand { get; private set; }

        private void OnAdd(object obj)
        {
            if (this.ViewPlanHistory == null || (!this.ViewPlanHistory.Any(p => p.HasValidationErrors)))
            {
                this._needReFreshViewPlanAircraft = true;
                this.OpenEditDialog(null, PlanDetailCreateSource.New);
            }
        }

        private bool CanAdd(object obj)
        {
            if (!this._viewButtonValidity["添加计划项"]) return false;
            // 操作台当前计划的状态还没有到已审核的，按钮可用
            return this.CurrentPlan != null && this.CurrentPlan.Status < (int)PlanStatus.Checked;
        }

        #endregion

        #region 移除计划项

        /// <summary>
        /// 移除计划项
        /// </summary>
        public DelegateCommand<object> RemoveCommand { get; private set; }

        private void OnRemove(object obj)
        {
            this.service.RemovePlanDetail(this.SelPlanHistory);
            this.RefreshView();
        }

        private bool CanRemove(object obj)
        {
            if (!this._viewButtonValidity["移除计划项"]) return false;
            // 当前计划为空时，按钮不可用
            if (this.CurrentPlan == null) return false;
            // 选中计划明细为空时，按钮不可用
            if (this.SelPlanHistory == null) return false;
            // 选中计划明细没有对应的计划飞机时，按钮不可用
            if (this.SelPlanHistory.PlanAircraft == null) return false;
            // 选中计划明细无需申请的，按钮可用
            if (this.SelPlanHistory.ActionCategory != null && !this.SelPlanHistory.ActionCategory.NeedRequest) return true;

            // 计算选中计划明细对应的计划飞机在当前计划中的明细项集合
            var planDetails = this.SelPlanHistory.PlanAircraft.PlanHistories.Where(ph => ph.Plan == this.CurrentPlan).ToList();
            // 1、选中计划明细对应的计划飞机在当前计划中只有一条明细项
            if (planDetails.Count == 1)
            {
                // 当前计划的状态还没有到已审核，且计划飞机的管理状态还没到申请时，按钮可用
                return this.CurrentPlan.Status < (int)PlanStatus.Checked &&
                       this.SelPlanHistory.PlanAircraft.Status < (int)ManageStatus.Request;
            }
            // 2、选中计划明细对应的计划飞机在当前计划中超过一条明细项
            // 选中计划明细的操作类型为引进时，按钮不可用
            else {return this.SelPlanHistory.ActionType != "引进";}
            // 3、选中计划明细对应的计划飞机在当前计划中没有明细项（业务逻辑控制上不会出现这种情况）
        }

        #endregion

        #region 提交审核

        /// <summary>
        /// 提交审核
        /// </summary>
        public DelegateCommand<object> CommitCommand { get; private set; }

        private void OnCommit(object obj)
        {
            if (!this.ViewPlanHistory.Any())
            {
                RadWindow.Alert(this.SetAlter("提醒", "确认", "计划明细不能为空！", 13, 250, () => { }));
                return;
            }
            if (this.CurrentPlan != null)
            {
                this.CurrentPlan.Status = (int)PlanStatus.Checking;
                RefreshButtonState();
            }
        }

        private bool CanCommit(object obj)
        {
            if (!this._viewButtonValidity["提交审核"]) return false;
            // 有未保存内容时，按钮不可用
            if (this.service.EntityContainer.HasChanges) return false;
            // 操作台当前计划处于草稿状态的，按钮可用
            return this.CurrentPlan != null && this.CurrentPlan.Status == (int)PlanStatus.Draft;
        }

        #endregion

        #region 审核

        /// <summary>
        /// 审核
        /// </summary>
        public DelegateCommand<object> ExamineCommand { get; private set; }

        private void OnExamine(object obj)
        {
            if (this.CurrentPlan != null)
            {
                this.CurrentPlan.Status = (int)PlanStatus.Checked;
                this.CurrentPlan.IsValid = true;
                RefreshButtonState();
            }
        }

        private bool CanExamine(object obj)
        {
            if (!this._viewButtonValidity["审核"]) return false;
            // 有未保存内容时，按钮不可用
            if (this.service.EntityContainer.HasChanges) return false;
            // 操作台当前计划处于审核状态的，按钮可用
            return this.CurrentPlan != null && this.CurrentPlan.Status == (int)PlanStatus.Checking;
        }

        #endregion

        #endregion

        #region Methods

        #endregion

        #endregion

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
            this.NewPlanCommand = new DelegateCommand<object>(this.OnNewPlan, this.CanNewPlan);
            this.AddCommand = new DelegateCommand<object>(this.OnAdd, this.CanAdd);
            this.RemoveCommand = new DelegateCommand<object>(this.OnRemove, this.CanRemove);
            this.CommitCommand = new DelegateCommand<object>(this.OnCommit, this.CanCommit);
            this.ExamineCommand = new DelegateCommand<object>(this.OnExamine, this.CanExamine);

            this._viewButtonValidity = authService.GetViewButtonValidity(typeof(AfrpPlanLayView).ToString());
        }

        #endregion

        #region INavigationAware Members

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            LoadPlan();
            LoadRequest();
        }

        #endregion

    }

}
