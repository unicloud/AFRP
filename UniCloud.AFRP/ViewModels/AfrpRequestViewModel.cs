using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Windows.Data.DomainServices;
using Telerik.Windows.Controls;
using UniCloud.AFRP.Views;
using UniCloud.Fleet.Models;
using UniCloud.Fleet.Services;

namespace UniCloud.AFRP.ViewModels
{
    [Export(typeof(AfrpRequestViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AfrpRequestViewModel : EditViewModelBase, IPartImportsSatisfiedNotification, INavigationAware
    {

        #region Local

        [Import]
        public IRegionManager regionManager;

        private IDictionary<string, bool> _viewButtonValidity;
        internal Plan CurrentPlan { get; set; }

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
                        if (this._loadedRequest && this._loadedPlan)
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
            this.NewCommand.RaiseCanExecuteChanged();
            this.AttachCommand.RaiseCanExecuteChanged();
            this.CommitCommand.RaiseCanExecuteChanged();
            this.ExamineCommand.RaiseCanExecuteChanged();
            this.SendCommand.RaiseCanExecuteChanged();
            this.EditCommand.RaiseCanExecuteChanged();
        }

        #endregion

        #region 管理界面操作

        private void RefreshView()
        {
            this.CurrentPlan = this.service.CurrentPlan;
            this._needReFreshViewPlanHistory = true;
            RaiseViewPlanHistory();
            this._needReFreshViewRequest = true;
            RaiseViewRequest();
            if (this.SelRequest == null)
            {
                this.SelRequest = ViewRequest.LastOrDefault();
                this.RefreshButtonState();
            }
        }

        internal void AddNewRequestDetail(PlanHistory planHistory)
        {
            this.service.CreateNewRequestDetail(this.SelRequest, planHistory);
            this._needReFreshViewApprovalHistory = true;
            RaiseViewApprovalHistory();
            this._needReFreshViewPlanHistory = true;
            RaiseViewPlanHistory();
        }

        internal void RemoveRequestDetail(ApprovalHistory requestDetail)
        {
            this.service.RemoveRequestDetail(requestDetail);
            this._needReFreshViewApprovalHistory = true;
            RaiseViewApprovalHistory();
            this._needReFreshViewPlanHistory = true;
            RaiseViewPlanHistory();
        }

        #endregion

        #endregion

        #endregion

        #region ViewModel

        #region 数据集合

        #region 未完成申请

        /// <summary>
        /// 未完成申请集合
        /// </summary>
        public IEnumerable<Request> ViewRequest
        {
            get { return this.service.EntityContainer.GetEntitySet<Request>().Where(r => r.Status < (int)ReqStatus.Examined); }
        }

        private bool _needReFreshViewRequest = true;

        private void RaiseViewRequest()
        {
            if (this._needReFreshViewRequest)
            {
                RaisePropertyChanged(() => this.ViewRequest);
                this._needReFreshViewRequest = false;
            }
        }

        private Request _selRequest;

        /// <summary>
        /// 选中的未完成申请
        /// </summary>
        public Request SelRequest
        {
            get { return this._selRequest; }
            set
            {
                if (this._selRequest != value)
                {
                    _selRequest = value;
                    this.RaisePropertyChanged(() => this.SelRequest);
                    this._needReFreshViewApprovalHistory = true;
                    this.RaiseViewApprovalHistory();
                    this.RefreshButtonState();
                }
            }
        }

        #endregion

        #region 申请明细

        /// <summary>
        /// 申请明细集合
        /// </summary>
        public IEnumerable<ApprovalHistory> ViewApprovalHistory
        {
            get { return this.service.EntityContainer.GetEntitySet<ApprovalHistory>().Where(ah => ah.Request == this.SelRequest); }
        }

        private bool _needReFreshViewApprovalHistory = true;

        private void RaiseViewApprovalHistory()
        {
            if (this._needReFreshViewApprovalHistory)
            {
                RaisePropertyChanged(() => this.ViewApprovalHistory);
                this._needReFreshViewApprovalHistory = false;
            }
        }

        private ApprovalHistory _selApprovalHistory;

        /// <summary>
        /// 选中的申请明细
        /// </summary>
        public ApprovalHistory SelApprovalHistory
        {
            get { return this._selApprovalHistory; }
            set
            {
                if (this._selApprovalHistory != value)
                {
                    _selApprovalHistory = value;
                    this.RaisePropertyChanged(() => this.SelApprovalHistory);
                }
            }
        }

        #endregion

        #region 当前计划明细

        /// <summary>
        /// 当前计划明细集合
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
        /// 选中的当前计划明细
        /// </summary>
        public PlanHistory SelPlanHistory
        {
            get { return this._selPlanHistory; }
            set
            {
                if (this._selPlanHistory != value)
                {
                    _selPlanHistory = value;
                    this.RaisePropertyChanged(() => this.SelPlanHistory);
                }
            }
        }

        #endregion

        #endregion

        #region Property

        #endregion

        #region Command

        #region 重载命令

        #region 实体变更

        protected override void OnEntityHasChanges()
        {
            RefreshButtonState();
        }

        #endregion

        #region 保存

        #endregion

        #region 放弃更改

        protected override void OnAbortExecuted(object sender)
        {
            this._needReFreshViewPlanHistory = true;
            RaiseViewPlanHistory();
            this.RefreshView();
        }

        #endregion

        #region 确认

        #endregion

        #region 取消

        #endregion

        #region 添加附件

        protected override bool CanAttach(object sender)
        {
            if (!this._viewButtonValidity["添加附件"])
            {
                return false;
            }
            // 选中申请的状态还没到已审核时，按钮可用
            return this.SelRequest != null && this.SelRequest.Status < (int)ReqStatus.Checked;
        }

        #endregion

        #endregion

        #region 创建新申请

        /// <summary>
        /// 创建新申请
        /// </summary>
        public DelegateCommand<object> NewCommand { get; private set; }

        private void OnNew(object obj)
        {
            var request = this.service.CreateNewRequest();
            this._needReFreshViewRequest = true;
            RaiseViewRequest();
            this.SelRequest = request;
            RefreshButtonState();
        }

        private bool CanNew(object obj)
        {
            if (!this._viewButtonValidity["创建新申请"])
            {
                return false;
            }
            // 有未保存内容时，按钮不可用
            if (this.service.EntityContainer.HasChanges)
            {
                return false;
            }
            // 存在可申请的计划明细时，按钮可用
            return this.ViewPlanHistory != null &&
                   this.ViewPlanHistory.Any(ph => ph.CanRequest == "1：可申请");
        }

        #endregion

        #region 提交审核

        /// <summary>
        /// 提交审核
        /// </summary>
        public DelegateCommand<object> CommitCommand { get; private set; }

        private void OnCommit(object obj)
        {
            this.SelRequest.Status = (int)ReqStatus.Checking;
            RefreshButtonState();
        }

        private bool CanCommit(object obj)
        {
            if (!this._viewButtonValidity["提交审核"])
            {
                return false;
            }
            // 选中申请为空时，按钮不可用
            if (this.SelRequest == null)
            {
                return false;
            }
            // 选中申请的标题、文号、文档为空时，按钮不可用
            if (string.IsNullOrWhiteSpace(this.SelRequest.Title) ||
                string.IsNullOrWhiteSpace(this.SelRequest.DocNumber) ||
                this.SelRequest.AttachDoc == null)
            {
                return false;
            }
            // 有未保存内容时，按钮不可用
            if (this.service.EntityContainer.HasChanges)
            {
                return false;
            }
            // 选中申请的状态处于草稿，且申请明细不为空时，按钮可用
            return this.SelRequest.Status == (int)ReqStatus.Draft && this.SelRequest.ApprovalHistories.Any();
        }

        #endregion

        #region 审核

        /// <summary>
        /// 审核
        /// </summary>
        public DelegateCommand<object> ExamineCommand { get; private set; }

        private void OnExamine(object obj)
        {
            this.SelRequest.Status = (int)ReqStatus.Checked;
            RefreshButtonState();
        }

        private bool CanExamine(object obj)
        {
            if (!this._viewButtonValidity["审核"])
            {
                return false;
            }
            // 选中申请为空时，按钮不可用
            if (this.SelRequest == null)
            {
                return false;
            }
            // 选中申请的标题、文号、文档为空时，按钮不可用
            if (string.IsNullOrWhiteSpace(this.SelRequest.Title) ||
                string.IsNullOrWhiteSpace(this.SelRequest.DocNumber) ||
                this.SelRequest.AttachDoc == null)
            {
                return false;
            }
            // 有未保存内容时，按钮不可用
            if (this.service.EntityContainer.HasChanges)
            {
                return false;
            }
            // 选中申请的状态处于审核，且申请明细不为空时，按钮可用
            return this.SelRequest.Status == (int)ReqStatus.Checking && this.SelRequest.ApprovalHistories.Any();
        }

        #endregion

        #region 发送

        /// <summary>
        /// 发送
        /// </summary>
        public DelegateCommand<object> SendCommand { get; private set; }

        private void OnSend(object obj)
        {
            var content = "是否向【民航局】报送" + this.SelRequest.DocNumber + "，" + this.SelRequest.Title + "？";
            RadWindow.Confirm(this.SetConfirm("确认报送申请", "确认", "取消", content, 13, 250, (o, e) =>
                {
                    if (e.DialogResult == true)
                    {
                        // 审核、已提交状态下可以发送。如果已处于提交状态，需要重新发送的，不必改变状态。
                        if (this.SelRequest != null && this.SelRequest.Status != (int)ReqStatus.Submited)
                        {
                            this.SelRequest.Status = (int)ReqStatus.Submited;
                        }
                        this.SelRequest.SubmitDate = DateTime.Now;
                        this.service.SubmitChanges(sc =>
                        {
                            if (sc.Error == null)
                            {
                                // 发送不成功的，也认为是已经做了发送操作，不回滚状态。始终可以重新发送。
                                this.service.TransferPlanAndRequest(this.CurrentPlan.PlanID, this.SelRequest.RequestID, tp => { }, null);
                                RefreshButtonState();
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
            // 选中申请为空时，按钮不可用
            if (this.SelRequest == null) return false;
            // 有未保存内容时，按钮不可用
            if (this.service.EntityContainer.HasChanges) return false;
            // 选中申请的状态处于已审核或已提交时，按钮可用
            return this.SelRequest.Status == (int)ReqStatus.Checked ||
                   this.SelRequest.Status == (int)ReqStatus.Submited;
        }

        #endregion

        #region 修改申请

        /// <summary>
        /// 修改申请
        /// </summary>
        public DelegateCommand<object> EditCommand { get; private set; }

        private void OnEdit(object obj)
        {
            const string content = "确认后申请状态将改为草稿并允许编辑，是否要对该申请进行修改？";
            RadWindow.Confirm(this.SetConfirm("确认修改申请", "确认", "取消", content, 13, 250, (o, e) =>
            {
                if (e.DialogResult == true)
                {
                    this.SelRequest.Status = (int)ReqStatus.Draft;
                    this.service.SubmitChanges(sc => { }, null);
                    RefreshButtonState();
                }
            }));
        }

        private bool CanEdit(object obj)
        {
            if (!this._viewButtonValidity["修改申请"])
            {
                return false;
            }
            // 选中申请为空时，按钮不可用
            if (this.SelRequest == null)
            {
                return false;
            }
            // 有未保存内容时，按钮不可用
            if (this.service.EntityContainer.HasChanges)
            {
                return false;
            }
            // 选中申请的状态不是草稿时，按钮可用
            return this.SelRequest.Status != (int)ReqStatus.Draft;
        }

        #endregion

        #region Methods


        #endregion

        #endregion

        #endregion

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
            this.NewCommand = new DelegateCommand<object>(this.OnNew, this.CanNew);
            this.CommitCommand = new DelegateCommand<object>(this.OnCommit, this.CanCommit);
            this.ExamineCommand = new DelegateCommand<object>(this.OnExamine, this.CanExamine);
            this.SendCommand = new DelegateCommand<object>(this.OnSend, this.CanSend);
            this.EditCommand = new DelegateCommand<object>(this.OnEdit, this.CanEdit);

            this._viewButtonValidity = authService.GetViewButtonValidity(typeof(AfrpRequestView).ToString());
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
