using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Windows.Data.DomainServices;
using Telerik.Windows.Controls;
using UniCloud.AFRP.Views;
using UniCloud.Fleet.Models;
using UniCloud.Fleet.Services;
using System.Windows;

namespace UniCloud.AFRP.ViewModels
{
    [Export(typeof(AfrpDeliverViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AfrpDeliverViewModel : EditViewModelBase, IPartImportsSatisfiedNotification, INavigationAware
    {

        #region Local

        [Import]
        public IRegionManager regionManager;
        [Import]
        public PlanDeliverEditDialog EditDialog;
        private IDictionary<string, bool> _viewButtonValidity;
        private Plan CurrentPlan { get; set; }


        #region Property

        #endregion

        #region Method

        #region 加载数据

        private void LoadCompleted()
        {
            this._loadedPlan = false;
            this._loadedAircraft = false;
            this._loadedOperationHistory = false;
            this._loadedAircraftBusiness = false;
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
                        if (this._loadedPlan && this._loadedRequest && this._loadedAircraft && this._loadedOperationHistory && this._loadedAircraftBusiness)
                        {
                            this.LoadCompleted();
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
                        if (this._loadedPlan && this._loadedRequest && this._loadedAircraft && this._loadedOperationHistory && this._loadedAircraftBusiness)
                        {
                            this.LoadCompleted();
                        }
                    }
                }, null);
        }

        private bool _loadedAircraft;

        private void LoadAircraft()
        {
            this.service.LoadAircraft(new QueryBuilder<Aircraft>(), lo =>
                {
                    if (lo.Error == null)
                    {
                        this._loadedAircraft = true;
                        // 判断是否所有加载操作都完成
                        if (this._loadedPlan && this._loadedRequest && this._loadedAircraft && this._loadedOperationHistory && this._loadedAircraftBusiness)
                        {
                            this.LoadCompleted();
                        }
                    }
                }, null);
        }

        private bool _loadedOperationHistory;

        private void LoadOperationHistory()
        {
            this.service.LoadOperationHistory(new QueryBuilder<OperationHistory>(), lo =>
                {
                    if (lo.Error == null)
                    {
                        this._loadedOperationHistory = true;
                        // 判断是否所有加载操作都完成
                        if (this._loadedPlan && this._loadedRequest && this._loadedAircraft && this._loadedOperationHistory && this._loadedAircraftBusiness)
                        {
                            this.LoadCompleted();
                        }
                    }
                }, null);
        }

        private bool _loadedAircraftBusiness;

        private void LoadAircraftBusiness()
        {
            this.service.LoadAircraftBusiness(new QueryBuilder<AircraftBusiness>(), lo =>
                {
                    if (lo.Error == null)
                    {
                        this._loadedAircraftBusiness = true;
                        // 判断是否所有加载操作都完成
                        if (this._loadedPlan && this._loadedRequest && this._loadedAircraft && this._loadedOperationHistory && this._loadedAircraftBusiness)
                        {
                            this.LoadCompleted();
                        }
                    }
                }, null);
        }

        #endregion

        #region 管理按钮

        protected override void RefreshButtonState()
        {
            this.CompleteCommand.RaiseCanExecuteChanged();
            this.CommitCommand.RaiseCanExecuteChanged();
            this.ExamineCommand.RaiseCanExecuteChanged();
            this.SendCommand.RaiseCanExecuteChanged();
            this.RepealCommand.RaiseCanExecuteChanged();
        }

        #endregion

        #region 管理界面操作

        private void RefreshView()
        {
            this.CurrentPlan = this.service.CurrentPlan;
            RaisePropertyChanged(() => this.PlanTitle);
            var selHistory = this.SelPlanHistory;
            this._needReFreshViewAircraft = true;
            this.RaiseViewAircraft();
            this._needReFreshViewPlanHistory = true;
            this.RaiseViewPlanHistory();
            this.SelPlanHistory = selHistory ??
                                  ViewPlanHistory.FirstOrDefault(ph => ph.PlanAircraft.Status != (int)ManageStatus.Operation);
            this.RefreshButtonState();
        }

        /// <summary>
        /// 打开子窗体之前先设置好子窗体中的编辑属性
        /// </summary>
        /// <param name="planHistory">计划明细</param>
        internal void OpenEditDialog(PlanHistory planHistory)
        {
            this.ShowEditDialog();

        }


        private void ShowEditDialog()
        {
            this.IsAircraft = true;
            this.IsOperationHistory = false;
            this.IsAircraftBusiness = false;
            this.EditDialog.ShowDialog();
        }

        #endregion

        #endregion

        #endregion

        #region ViewModel

        #region 数据集合

        #region 当前计划

        /// <summary>
        /// 当前计划集合
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
        /// 选中的当前计划
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
                    if (this._selPlanHistory != null)
                    {
                        this.SelAircraft =
                            this.ViewAircraft.FirstOrDefault(
                                a =>
                                a.PlanAircrafts.FirstOrDefault(pa => pa.IsOwn) == this._selPlanHistory.PlanAircraft);
                    }
                    RefreshButtonState();
                }
            }
        }

        #endregion

        #region 运营飞机

        /// <summary>
        /// 运营飞机集合(包含已退出的飞机)
        /// </summary>
        public IEnumerable<Aircraft> ViewAircraft
        {
            get { return this.service.EntityContainer.GetEntitySet<Aircraft>().Where(p => p.IsOperation==true); }
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
                    this._selAircraft = value;
                    this.RaisePropertyChanged(() => this.SelAircraft);
                    this._needReFreshViewOperationHistory = true;
                    this.RaiseViewOperationHistory();
                    this._needReFreshViewAircraftBusiness = true;
                    this.RaiseViewAircraftBusiness();
                    if (this.SelAircraft != null)
                    {
                        var planHistory =
                            this.ViewPlanHistory.FirstOrDefault(ph => ph.PlanAircraft.Aircraft == this._selAircraft);
                        if (planHistory != null) this.SelPlanHistory = planHistory;

                        this.SelOperationHistory = this.ViewOperationHistory.LastOrDefault(p => p.Status < (int)OpStatus.Submited);
                        //如果为变更计划，则这个时候取到的SelOperationHistory为空，重新取集合中的最后一条来展示在子窗体中
                        if (SelOperationHistory == null)
                            SelOperationHistory = this.ViewOperationHistory.LastOrDefault();
                        this.SelAircraftBusiness = this.ViewAircraftBusiness.LastOrDefault(p => p.Status < (int)OpStatus.Submited);
                        //如果为退出计划，则这个时候取到的SelAircraftBusiness为空，重新取集合中的最后一条来展示在子窗体中
                        if (SelAircraftBusiness == null)
                            SelAircraftBusiness = this.ViewAircraftBusiness.LastOrDefault();
                    }
                    RaisePropertyChanged(() => this.OperationTitle);
                    RaisePropertyChanged(() => this.AircraftBusinessTitle);
                }
            }
        }

        #endregion

        #region 飞机运营历史

        /// <summary>
        /// 飞机运营历史集合
        /// </summary>
        public IEnumerable<OperationHistory> ViewOperationHistory
        {
            get
            {
                return
                    this.service.EntityContainer.GetEntitySet<OperationHistory>()
                        .OrderBy(oh => oh.StartDate)
                        .Where(oh => oh.Aircraft == this.SelAircraft);
            }
        }

        private bool _needReFreshViewOperationHistory = true;

        private void RaiseViewOperationHistory()
        {
            if (this._needReFreshViewOperationHistory)
            {
                RaisePropertyChanged(() => this.ViewOperationHistory);
                this._needReFreshViewOperationHistory = false;
            }
        }

        private OperationHistory _selOperationHistory;

        /// <summary>
        /// 选中的飞机运营历史
        /// </summary>
        public OperationHistory SelOperationHistory
        {
            get { return this._selOperationHistory; }
            set
            {
                if (this._selOperationHistory != value)
                {
                    this._selOperationHistory = value;
                    this.RaisePropertyChanged(() => this.SelOperationHistory);
                }
            }
        }

        #endregion

        #region 飞机商业数据历史

        /// <summary>
        /// 飞机商业数据历史集合
        /// </summary>
        public IEnumerable<AircraftBusiness> ViewAircraftBusiness
        {
            get
            {
                return
                    this.service.EntityContainer.GetEntitySet<AircraftBusiness>()
                        .OrderBy(ab => ab.StartDate)
                        .Where(ab => ab.Aircraft == this.SelAircraft);
            }
        }

        private bool _needReFreshViewAircraftBusiness = true;

        private void RaiseViewAircraftBusiness()
        {
            if (this._needReFreshViewAircraftBusiness)
            {
                RaisePropertyChanged(() => this.ViewAircraftBusiness);
                this._needReFreshViewAircraftBusiness = false;
            }
        }

        private AircraftBusiness _selAircraftBusiness;

        /// <summary>
        /// 选中的飞机商业数据历史
        /// </summary>
        public AircraftBusiness SelAircraftBusiness
        {
            get { return this._selAircraftBusiness; }
            set
            {
                if (this._selAircraftBusiness != value)
                {
                    this._selAircraftBusiness = value;
                    this.RaisePropertyChanged(() => this.SelAircraftBusiness);
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

        #region 运营历史

        /// <summary>
        /// 运营历史
        /// </summary>
        public string OperationTitle
        {
            get
            {
                if (this.SelAircraft != null)
                {
                    return this.SelAircraft.RegNumber + "运营历史";
                }
                return "飞机运营历史";
            }
        }

        #endregion

        #region 商业数据历史

        public string AircraftBusinessTitle
        {
            get
            {
                if (this.SelAircraft != null)
                {
                    return this.SelAircraft.RegNumber + "商业数据历史";
                }
                return "飞机商业数据历史";
            }
        }

        #endregion

        #region 子窗体选择运营飞机

        private bool _isAircraft;

        /// <summary>
        /// 选中变更计划
        /// </summary>
        public bool IsAircraft
        {
            get { return this._isAircraft; }
            private set
            {
                if (this._isAircraft != value)
                {
                    this._isAircraft = value;
                    if (value == true)
                    {
                        this.IsAircraftVisibility = Visibility.Visible;
                    }
                    else
                    {
                        this.IsAircraftVisibility = Visibility.Collapsed;
                    }
                    this.RaisePropertyChanged(() => this.IsAircraft);
                }
            }
        }

        #endregion

        #region 子窗体选择飞机运营历史

        private bool _isOperationHistory;

        /// <summary>
        /// 选中变更计划
        /// </summary>
        public bool IsOperationHistory
        {
            get { return this._isOperationHistory; }
            private set
            {
                if (this._isOperationHistory != value)
                {
                    this._isOperationHistory = value;
                    if (value == true)
                    {
                        this.IsOperationHistoryVisibility = Visibility.Visible;
                    }
                    else
                    {
                        this.IsOperationHistoryVisibility = Visibility.Collapsed;
                    }
                    this.RaisePropertyChanged(() => this.IsOperationHistory);
                }
            }
        }

        #endregion

        #region 子窗体选择飞机商业数据历史

        private bool _isAircraftBusiness;

        /// <summary>
        /// 选中变更计划
        /// </summary>
        public bool IsAircraftBusiness
        {
            get { return this._isAircraftBusiness; }
            private set
            {
                if (this._isAircraftBusiness != value)
                {
                    this._isAircraftBusiness = value;
                    if (value == true)
                    {
                        this.IsAircraftBusinessVisibility = Visibility.Visible;
                    }
                    else
                    {
                        this.IsAircraftBusinessVisibility = Visibility.Collapsed;
                    }
                    this.RaisePropertyChanged(() => this.IsAircraftBusiness);
                }
            }
        }

        #endregion

        #region 子窗体选择运营飞机是否显示

        private Visibility _isAircraftVisibility = Visibility.Visible;

        /// <summary>
        /// 选中变更计划
        /// </summary>
        public Visibility IsAircraftVisibility
        {
            get { return this._isAircraftVisibility; }
            private set
            {
                if (this._isAircraftVisibility != value)
                {
                    this._isAircraftVisibility = value;
                    this.RaisePropertyChanged(() => this.IsAircraftVisibility);
                }
            }
        }

        #endregion

        #region 子窗体选择飞机运营历史是否显示

        private Visibility _isOperationHistoryVisibility = Visibility.Collapsed;

        /// <summary>
        /// 选中变更计划
        /// </summary>
        public Visibility IsOperationHistoryVisibility
        {
            get { return this._isOperationHistoryVisibility; }
            private set
            {
                if (this._isOperationHistoryVisibility != value)
                {
                    this._isOperationHistoryVisibility = value;
                    this.RaisePropertyChanged(() => this.IsOperationHistoryVisibility);
                }
            }
        }

        #endregion

        #region 子窗体选择飞机商业数据历史是否显示

        private Visibility _isAircraftBusinessVisibility = Visibility.Collapsed;

        /// <summary>
        /// 选中变更计划
        /// </summary>
        public Visibility IsAircraftBusinessVisibility
        {
            get { return this._isAircraftBusinessVisibility; }
            private set
            {
                if (this._isAircraftBusinessVisibility != value)
                {
                    this._isAircraftBusinessVisibility = value;
                    this.RaisePropertyChanged(() => this.IsAircraftBusinessVisibility);
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
            this.RefreshButtonState();
        }

        #endregion

        #region 保存

        #endregion

        #region 放弃更改

        protected override void OnAbortExecuted(object sender)
        {
            this.RefreshView();
        }

        #endregion

        #region 确认

        protected override void OnOkExecute(object sender)
        {
            var aircraft = sender as Aircraft;
            this.EditDialog.Close();
            var viewPlanHistory = this.ViewPlanHistory;
            if (viewPlanHistory != null)
                this.SelPlanHistory = viewPlanHistory.FirstOrDefault(p => p.PlanAircraft.Aircraft == aircraft);
        }

        protected override bool CanOk(object obj)
        {
            return obj != null;
        }

        #endregion

        #region 取消


        #endregion

        #region 添加附件

        #endregion

        #endregion

        #region 完成计划

        /// <summary>
        /// 完成计划
        /// </summary>
        public DelegateCommand<object> CompleteCommand { get; private set; }

        private void OnComplete(object obj)
        {
            // 调用完成计划的操作，返回相关飞机
            var aircraft = this.service.CompletePlan(this.SelPlanHistory);

            if (aircraft == null) return;
            this._needReFreshViewAircraft = true;
            this.RaiseViewAircraft();
            // 定位选中的飞机，并确保运营历史、商业数据历史刷新
            if (this.SelAircraft != aircraft)
            {
                this.SelAircraft = aircraft;
            }
            else
            {
                this._needReFreshViewOperationHistory = true;
                this.RaiseViewOperationHistory();
                this._needReFreshViewAircraftBusiness = true;
                this.RaiseViewAircraftBusiness();
            }
            this._needReFreshViewPlanHistory = true;
            this.RaiseViewPlanHistory();
            this.OpenEditDialog(this.SelPlanHistory);
        }

        private bool CanComplete(object obj)
        {
            if (!this._viewButtonValidity["完成计划"])
            {
                return false;
            }
            // 有未保存内容时，按钮不可用
            if (this.service.EntityContainer.HasChanges)
            {
                return false;
            }
            // 没有选中的计划明细时，按钮不可用
            if (this.SelPlanHistory == null)
            {
                return false;
            }
            // 选中计划明细的完成状态为无，且计划明细为可交付时，按钮可用
            return this.SelPlanHistory.CompleteStatus == -1 && this.SelPlanHistory.CanDeliver == "1：可交付";
        }

        #endregion

        #region 提交审核

        /// <summary>
        /// 提交审核
        /// </summary>
        public DelegateCommand<object> CommitCommand { get; private set; }

        private void OnCommit(object obj)
        {
            //如果是引进计划，则需要同时修改其商业数据历史的状态
            if (SelPlanHistory.ActionCategory.ActionType == "引进")
            {
                if (SelPlanHistory.PlanAircraft.Aircraft.AircraftBusinesses != null)
                {
                    var aircraftBusiness =
                        SelPlanHistory.PlanAircraft.Aircraft.AircraftBusinesses.LastOrDefault(p => p.Status < (int)OpStatus.Submited);
                    if (aircraftBusiness != null)
                        aircraftBusiness.Status = (int)OpStatus.Checking;
                }
            }
            if (this.SelPlanHistory is OperationPlan)
            {
                var planDetail = this.SelPlanHistory as OperationPlan;
                planDetail.OperationHistory.Status = (int)OpStatus.Checking;

            }
            if (this.SelPlanHistory is ChangePlan)
            {
                var planDetail = this.SelPlanHistory as ChangePlan;
                planDetail.AircraftBusiness.Status = (int)OpStatus.Checking;
            }
            RefreshButtonState();
        }

        private bool CanCommit(object obj)
        {
            if (!this._viewButtonValidity["提交审核"])
            {
                return false;
            }
            // 有未保存内容时，按钮不可用
            if (this.service.EntityContainer.HasChanges)
            {
                return false;
            }
            // 没有选中的计划明细时，按钮不可用
            if (this.SelPlanHistory == null)
            {
                return false;
            }
            // 选中计划明细的完成状态为草稿时，按钮可用
            return this.SelPlanHistory.CompleteStatus == 0;
        }

        #endregion

        #region 审核

        /// <summary>
        /// 审核
        /// </summary>
        public DelegateCommand<object> ExamineCommand { get; private set; }

        private void OnExamine(object obj)
        {
            //如果是引进计划，则需要同时修改其商业数据历史的状态
            if (SelPlanHistory.ActionCategory.ActionType == "引进")
            {
                if (SelPlanHistory.PlanAircraft.Aircraft.AircraftBusinesses != null)
                {
                    var aircraftBusiness =
                        SelPlanHistory.PlanAircraft.Aircraft.AircraftBusinesses.LastOrDefault(p => p.Status < (int)OpStatus.Submited);
                    if (aircraftBusiness != null)
                        aircraftBusiness.Status = (int)OpStatus.Checked;
                }
            }
            if (this.SelPlanHistory is OperationPlan)
            {
                var planDetail = this.SelPlanHistory as OperationPlan;
                planDetail.OperationHistory.Status = (int)OpStatus.Checked;
            }
            if (this.SelPlanHistory is ChangePlan)
            {
                var planDetail = this.SelPlanHistory as ChangePlan;
                planDetail.AircraftBusiness.Status = (int)OpStatus.Checked;
            }
            RefreshView();
        }

        private bool CanExamine(object obj)
        {
            if (!this._viewButtonValidity["审核"])
            {
                return false;
            }
            // 有未保存内容时，按钮不可用
            if (this.service.EntityContainer.HasChanges)
            {
                return false;
            }
            // 没有选中的计划明细时，按钮不可用
            if (this.SelPlanHistory == null)
            {
                return false;
            }
            // 选中计划明细的完成状态为审核时，按钮可用
            return this.SelPlanHistory.CompleteStatus == 1;
        }

        #endregion

        #region 发送

        /// <summary>
        /// 发送
        /// </summary>
        public DelegateCommand<object> SendCommand { get; private set; }

        private void OnSend(object obj)
        {
            var content = "是否向【民航局】报送" + this.SelPlanHistory.PlanAircraft.Aircraft.RegNumber + "计划完成情况？";
            RadWindow.Confirm(this.SetConfirm("确认报送计划完成情况", "确认", "取消", content, 13, 250, (o, e) =>
            {
                if (e.DialogResult == true)
                {
                    //如果是引进计划，则需要同时修改其商业数据历史的状态
                    if (SelPlanHistory.ActionCategory.ActionType == "引进")
                    {
                        if (SelPlanHistory.PlanAircraft.Aircraft.AircraftBusinesses != null)
                        {
                            var aircraftBusiness =
                                SelPlanHistory.PlanAircraft.Aircraft.AircraftBusinesses.LastOrDefault(p => p.Status < (int)OpStatus.Submited);
                            if (aircraftBusiness != null)
                                aircraftBusiness.Status = (int)OpStatus.Submited;
                        }
                    }
                    // 审核、已提交状态下可以发送。如果已处于提交状态，需要重新发送的，不必改变状态。
                    if (this.SelPlanHistory is OperationPlan)
                    {
                        var planDetail = this.SelPlanHistory as OperationPlan;
                        planDetail.OperationHistory.Status = (int)OpStatus.Submited;
                    }
                    if (this.SelPlanHistory is ChangePlan)
                    {
                        var planDetail = this.SelPlanHistory as ChangePlan;
                        planDetail.AircraftBusiness.Status = (int)OpStatus.Submited;
                    }
                    this.service.SubmitChanges(sc =>
                    {
                        if (sc.Error == null)
                        {
                            // 发送不成功的，也认为是已经做了发送操作，不回滚状态。始终可以重新发送。
                            this.service.TransferPlanHistory(this.SelPlanHistory.PlanHistoryID, tp => { }, null);
                            RefreshView();
                        }
                    }, null);
                }
            }));
        }

        private bool CanSend(object obj)
        {
            if (!this._viewButtonValidity["发送"])
            {
                return false;
            }
            // 有未保存内容时，按钮不可用
            if (this.service.EntityContainer.HasChanges) return false;
            // 没有选中的计划明细时，按钮不可用
            if (this.SelPlanHistory == null) return false;
            // 选中计划明细的完成状态为已审核或已提交时，按钮可用
            return this.SelPlanHistory.CompleteStatus > 1;
        }

        #endregion

        #region 修改完成

        /// <summary>
        /// 修改完成
        /// </summary>
        public DelegateCommand<object> RepealCommand { get; private set; }

        private void OnRepeal(object obj)
        {
            const string content = "确认后计划完成状态将改为草稿并允许编辑，是否要对该计划明细进行修改？";
            RadWindow.Confirm(this.SetConfirm("确认修改计划完成情况", "确认", "取消", content, 13, 250, (o, e) =>
            {
                if (e.DialogResult == true)
                {
                    if (this.SelPlanHistory is OperationPlan)
                    {
                        var planDetail = this.SelPlanHistory as OperationPlan;
                        planDetail.OperationHistory.Status = (int)OpStatus.Draft;
                    }
                    if (this.SelPlanHistory is ChangePlan)
                    {
                        var planDetail = this.SelPlanHistory as ChangePlan;
                        planDetail.AircraftBusiness.Status = (int)OpStatus.Draft;
                    }
                    this.service.SubmitChanges(sc => { }, null);
                    RefreshView();
                }
            }));
        }

        private bool CanRepeal(object obj)
        {
            if (!this._viewButtonValidity["修改完成"])
            {
                return false;
            }
            // 有未保存内容时，按钮不可用
            if (this.service.EntityContainer.HasChanges) return false;
            // 没有选中的计划明细时，按钮不可用
            if (this.SelPlanHistory == null) return false;
            // 选中计划明细的完成状态不是无和草稿时，按钮可用
            return this.SelPlanHistory.CompleteStatus != -1 && this.SelPlanHistory.CompleteStatus != 0;
        }

        #endregion

        #endregion

        #region Methods

        #endregion

        #endregion

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
            this.CompleteCommand = new DelegateCommand<object>(this.OnComplete, this.CanComplete);
            this.CommitCommand = new DelegateCommand<object>(this.OnCommit, this.CanCommit);
            this.ExamineCommand = new DelegateCommand<object>(this.OnExamine, this.CanExamine);
            this.SendCommand = new DelegateCommand<object>(this.OnSend, this.CanSend);
            this.RepealCommand = new DelegateCommand<object>(this.OnRepeal, this.CanRepeal);

            this._viewButtonValidity = authService.GetViewButtonValidity(typeof(AfrpDeliverView).ToString());
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
            LoadAircraft();
            LoadOperationHistory();
            LoadAircraftBusiness();
        }

        #endregion

    }
}
