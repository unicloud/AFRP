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
    [Export(typeof(AfrpApprovalViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AfrpApprovalViewModel : EditViewModelBase, IPartImportsSatisfiedNotification, INavigationAware
    {

        #region Local

        [Import]
        public IRegionManager regionManager;

        private IDictionary<string, bool> _viewButtonValidity;

        #region Property

        #endregion

        #region Method

        #region 加载数据

        private void LoadCompleted()
        {
            this._loadedApprovalDoc = false;
            this._loadedRequest = false;
            this.RefreshView();
        }

        private bool _loadedApprovalDoc;
        private void LoadApprovalDoc()
        {
            this.service.LoadApprovalDoc(new QueryBuilder<ApprovalDoc>(), lo =>
            {
                if (lo.Error == null)
                {
                    this._loadedApprovalDoc = true;
                    // 判断是否所有加载操作都完成
                    if (this._loadedApprovalDoc && this._loadedRequest)
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
                        if (this._loadedRequest && this._loadedApprovalDoc)
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
            this._needReFreshViewApprovalDoc = true;
            this.RaiseViewApprovalDoc();
            this._needReFreshViewEnRouteRequest = true;
            this.RaiseViewEnRouteRequest();
            if (this.SelApprovalDoc == null)
            {
                this.SelApprovalDoc = this.ViewApprovalDoc.LastOrDefault();
                this.RefreshButtonState();
            }
            else
            {
                this._needReFreshViewRequest = true;
                this.RaiseViewRequest();
                if (this.SelRequest == null)
                {
                    this.SelRequest = ViewRequest.FirstOrDefault();
                }
                this.RefreshButtonState();
            }
        }

        internal void AddRequestToApprovalDoc(Request request)
        {
            var req = this.service.AddRequestToApprovalDoc(this.SelApprovalDoc, request);
            this._needReFreshViewEnRouteRequest = true;
            this.RaiseViewEnRouteRequest();
            this._needReFreshViewRequest = true;
            this.RaiseViewRequest();
            this.SelRequest = req;
        }

        internal void RemoveRequest(Request request)
        {
            this.service.RemoveRequest(request);
            this._needReFreshViewEnRouteRequest = true;
            this.RaiseViewEnRouteRequest();
            this._needReFreshViewRequest = true;
            this.RaiseViewRequest();
            this.SelRequest = ViewRequest.LastOrDefault();
        }

        #endregion

        #endregion

        #endregion

        #region ViewModel

        #region 数据集合

        #region 未使用批文

        /// <summary>
        /// 未使用的批文集合（批文中的飞机未完成计划，或未提交的批文）
        /// </summary>
        public IEnumerable<ApprovalDoc> ViewApprovalDoc
        {
            get
            {
                return
                    this.service.EntityContainer.GetEntitySet<ApprovalDoc>()
                        .Where(a =>a.Status <3 || !a.Requests.Any() || a.Requests.Any(r => !r.IsFinished));
            }
        }

        private bool _needReFreshViewApprovalDoc = true;

        private void RaiseViewApprovalDoc()
        {
            if (this._needReFreshViewApprovalDoc)
            {
                RaisePropertyChanged(() => this.ViewApprovalDoc);
                this._needReFreshViewApprovalDoc = false;
            }
        }

        private ApprovalDoc _selApprovalDoc;

        /// <summary>
        /// 选中的未使用批文
        /// </summary>
        public ApprovalDoc SelApprovalDoc
        {
            get { return this._selApprovalDoc; }
            set
            {
                if (this._selApprovalDoc != value)
                {
                    this._selApprovalDoc = value;
                    this.RaisePropertyChanged(() => this.SelApprovalDoc);
                    this._needReFreshViewRequest = true;
                    this.RaiseViewRequest();
                    this.RefreshButtonState();
                    this.SelRequest = ViewRequest.FirstOrDefault();
                }
            }
        }

        #endregion

        #region 批文的申请

        /// <summary>
        /// 批文的申请集合
        /// </summary>
        public IEnumerable<Request> ViewRequest
        {
            get { return this.service.EntityContainer.GetEntitySet<Request>().Where(r => r.ApprovalDoc == this.SelApprovalDoc); }
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
        /// 选中的批文的申请
        /// </summary>
        public Request SelRequest
        {
            get { return this._selRequest; }
            set
            {
                if (this._selRequest != value)
                {
                    this._selRequest = value;
                    this.RaisePropertyChanged(() => this.SelRequest);
                    this._needReFreshViewApprovalHistory = true;
                    this.RaiseViewApprovalHistory();
                }
            }
        }

        #endregion

        #region 批文的申请明细

        /// <summary>
        /// 批文的申请明细集合
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
        /// 选中的批文的申请明细
        /// </summary>
        public ApprovalHistory SelApprovalHistory
        {
            get { return this._selApprovalHistory; }
            set
            {
                if (this._selApprovalHistory != value)
                {
                    this._selApprovalHistory = value;
                    this.RaisePropertyChanged(() => this.SelApprovalHistory);
                }
            }
        }

        #endregion

        #region 在途申请

        /// <summary>
        /// 在途申请集合
        /// </summary>
        public IEnumerable<Request> ViewEnRouteRequest
        {
            get
            {
                return
                    this.service.EntityContainer.GetEntitySet<Request>()
                        .OrderBy(r => r.CreateDate)
                        .Where(r => r.Status == (int)ReqStatus.Submited || r.Status == (int)ReqStatus.Examined);
            }
        }

        private bool _needReFreshViewEnRouteRequest = true;

        private void RaiseViewEnRouteRequest()
        {
            if (this._needReFreshViewEnRouteRequest)
            {
                RaisePropertyChanged(() => this.ViewEnRouteRequest);
                this._needReFreshViewEnRouteRequest = false;
            }
        }

        private Request _selEnRouteRequest;

        /// <summary>
        /// 选中的在途申请
        /// </summary>
        public Request SelEnRouteRequest
        {
            get { return this._selEnRouteRequest; }
            set
            {
                if (this._selEnRouteRequest != value)
                {
                    this._selEnRouteRequest = value;
                    this.RaisePropertyChanged(() => this.SelEnRouteRequest);
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
            this.RefreshButtonState();
        }

        #endregion

        #region 保存

        protected override void OnSaveSuccess(object sender)
        {
            base.OnSaveSuccess(sender);
        }

        protected override void OnSaveFail(object sender)
        {
            base.OnSaveFail(sender);
        }

        #endregion

        #region 放弃更改

        protected override void OnAbortExecuted(object sender)
        {
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
            // 选中批文的状态还没到已审核时，按钮可用
            return this.SelApprovalDoc != null && this.SelApprovalDoc.Status < (int)OpStatus.Checked;
        }

        #endregion

        #endregion

        #region 创建新批文

        /// <summary>
        /// 创建新批文
        /// </summary>
        public DelegateCommand<object> NewCommand { get; private set; }

        private void OnNew(object obj)
        {
            var approvalDoc = this.service.CreateNewApprovalDoc();
            this._needReFreshViewApprovalDoc = true;
            this.RaiseViewApprovalDoc();
            this.SelApprovalDoc = approvalDoc;
            RefreshButtonState();
        }

        private bool CanNew(object obj)
        {
            if (!this._viewButtonValidity["创建新批文"])
            {
                return false;
            }
            // 存在已提交的申请，且没有未保存内容时，按钮可用
            return this.ViewEnRouteRequest.Any(r => r.Status == (int)ReqStatus.Submited) &&
                   !this.service.EntityContainer.HasChanges;
        }

        #endregion

        #region 提交审核

        /// <summary>
        /// 提交审核
        /// </summary>
        public DelegateCommand<object> CommitCommand { get; private set; }

        private void OnCommit(object obj)
        {
            this.SelApprovalDoc.Status = (int)OpStatus.Checking;
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
            // 选中批文为空时，按钮不可用
            if (this.SelApprovalDoc == null)
            {
                return false;
            }
            // 批文号、审批单位、批文文档、审批日期为空时，按钮不可用
            if (string.IsNullOrWhiteSpace(this.SelApprovalDoc.ApprovalNumber) ||
                this.SelApprovalDoc.Manager == null ||
                this.SelApprovalDoc.AttachDoc == null ||
                this.SelApprovalDoc.ExamineDate == null)
            {
                return false;
            }
            // 选中批文的状态处于草稿，且批文申请明细不为空时，按钮可用
            return this.SelApprovalDoc.Status == (int)OpStatus.Draft && this.SelApprovalDoc.Requests.Any();
        }

        #endregion

        #region 审核

        /// <summary>
        /// 审核
        /// </summary>
        public DelegateCommand<object> ExamineCommand { get; private set; }

        private void OnExamine(object obj)
        {
            this.SelApprovalDoc.Status = (int)OpStatus.Checked;
            RefreshButtonState();
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
            // 选中批文为空时，按钮不可用
            if (this.SelApprovalDoc == null)
            {
                return false;
            }
            // 批文号、审批单位、批文文档、审批日期为空时，按钮不可用
            if (string.IsNullOrWhiteSpace(this.SelApprovalDoc.ApprovalNumber) ||
                this.SelApprovalDoc.Manager == null ||
                this.SelApprovalDoc.AttachDoc == null ||
                this.SelApprovalDoc.ExamineDate == null)
            {
                return false;
            }
            // 选中批文的状态处于审核，且批文申请明细不为空时，按钮可用
            return this.SelApprovalDoc.Status == (int)OpStatus.Checking && this.SelApprovalDoc.Requests.Any();
        }

        #endregion

        #region 发送

        /// <summary>
        /// 发送
        /// </summary>
        public DelegateCommand<object> SendCommand { get; private set; }

        private void OnSend(object obj)
        {
            var content = "是否向【民航局】报送批文：" + this.SelApprovalDoc.ApprovalNumber + "？";
            RadWindow.Confirm(this.SetConfirm("确认报送批文", "确认", "取消", content, 13, 250, (o, e) =>
            {
                if (e.DialogResult == true)
                {
                    // 审核、已提交状态下可以发送。如果已处于提交状态，需要重新发送的，不必改变状态。
                    if (this.SelApprovalDoc != null && this.SelApprovalDoc.Status != (int)OpStatus.Submited)
                    {
                        this.SelApprovalDoc.Status = (int)OpStatus.Submited;
                    }
                    this.service.SubmitChanges(sc =>
                    {
                        if (sc.Error == null)
                        {
                            // 发送不成功的，也认为是已经做了发送操作，不回滚状态。始终可以重新发送。
                            this.service.TransferApprovalDoc(this.SelApprovalDoc.ApprovalDocID, tp => { }, null);
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
            // 有未保存内容时，按钮不可用
            if (this.service.EntityContainer.HasChanges) return false;
            // 选中批文为空时，按钮不可用
            if (this.SelApprovalDoc == null) return false;
            // 选中批文的状态处于已审核或已提交时，按钮可用
            return this.SelApprovalDoc.Status == (int)OpStatus.Checked ||
                   this.SelApprovalDoc.Status == (int)OpStatus.Submited;
        }

        #endregion

        #region 修改批文

        /// <summary>
        /// 修改批文
        /// </summary>
        public DelegateCommand<object> EditCommand { get; private set; }

        private void OnEdit(object obj)
        {
            const string content = "确认后批文状态将改为草稿并允许编辑，是否要对该批文进行修改？";
            RadWindow.Confirm(this.SetConfirm("确认修改批文", "确认", "取消", content, 13, 250, (o, e) =>
            {
                if (e.DialogResult == true)
                {
                    this.SelApprovalDoc.Status = (int)OpStatus.Draft;
                    this.service.SubmitChanges(sc => { }, null);
                    RefreshButtonState();
                }
            }));
        }

        private bool CanEdit(object obj)
        {
            if (!this._viewButtonValidity["修改批文"])
            {
                return false;
            }
            // 有未保存内容时，按钮不可用
            if (this.service.EntityContainer.HasChanges) return false;
            // 选中批文为空时，按钮不可用
            if (this.SelApprovalDoc == null) return false;
            // 选中批文的状态不是草稿时，按钮可用
            return this.SelApprovalDoc.Status != (int)OpStatus.Draft;
        }

        #endregion

        #endregion

        #region Methods


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

            this._viewButtonValidity = authService.GetViewButtonValidity(typeof(AfrpApprovalView).ToString());
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
            this.LoadApprovalDoc();
            this.LoadRequest();
        }

        #endregion

    }
}
