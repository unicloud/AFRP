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
    [Export(typeof(AfrpPlanPublishViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AfrpPlanPublishViewModel : EditViewModelBase, IPartImportsSatisfiedNotification, INavigationAware
    {

        #region Local

        [Import]
        public IRegionManager regionManager;

        private IDictionary<string, bool> _viewButtonValidity;
        private List<Plan> PublishingPlan { get; set; }
        private Plan LastPublishedPlan { get; set; }

        #region Property

        #endregion

        #region Method

        #region 加载数据

        private void LoadCompleted()
        {
            this._loadedPlan = false;
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
                    if (this._loadedPlan)
                    {
                        LoadCompleted();
                    }
                }
            }, null);
        }

        #endregion

        #region 管理按钮

        protected override void RefreshButtonState()
        {
            this.CommitCommand.RaiseCanExecuteChanged();
            this.ExamineCommand.RaiseCanExecuteChanged();
            this.SendCommand.RaiseCanExecuteChanged();
            this.RepealCommand.RaiseCanExecuteChanged();
        }

        #endregion

        #region 管理界面操作

        private void RefreshView()
        {
            this.PublishingPlan = this.service.GetLatestPublishingPlan().ToList();
            this.LastPublishedPlan = this.service.GetLatestPublishedPlan();
            this._needReFreshViewPlan = true;
            this.RaiseViewPlan();
            this.SelPlan = this.PublishingPlan.Any() ? this.PublishingPlan[0] : this.service.CurrentPlan;
            this.RefreshButtonState();
        }

        #endregion

        #endregion

        #endregion

        #region ViewModel

        #region 数据集合

        #region 当年计划

        /// <summary>
        /// 当年计划集合
        /// </summary>
        public IEnumerable<Plan> ViewPlan
        {
            get { return this.service.EntityContainer.GetEntitySet<Plan>().Where(p => p.Annual.IsOpen == true); }
        }

        private bool _needReFreshViewPlan = true;

        private void RaiseViewPlan()
        {
            if (this._needReFreshViewPlan)
            {
                RaisePropertyChanged(() => this.ViewPlan);
                this._needReFreshViewPlan = false;
            }
        }

        private Plan _selPlan;

        /// <summary>
        /// 选中的当年计划
        /// </summary>
        public Plan SelPlan
        {
            get { return this._selPlan; }
            set
            {
                if (this._selPlan != value)
                {
                    _selPlan = value;
                    this.RaisePropertyChanged(() => this.SelPlan);
                    this._needReFreshViewPlanHistory = true;
                    RaiseViewPlanHistory();
                    RefreshButtonState();
                }
            }
        }

        #endregion

        #region 计划明细

        /// <summary>
        /// 计划明细集合
        /// </summary>
        public IEnumerable<PlanHistory> ViewPlanHistory
        {
            get
            {
                return
                    this.service.EntityContainer.GetEntitySet<PlanHistory>()
                        .OrderBy(ph => ph.PerformTime)
                        .Where(ph => ph.Plan == this.SelPlan);
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
        /// 选中的计划明细
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
            this.PublishingPlan = this.service.GetLatestPublishingPlan().ToList();
        }

        #endregion

        #region 确认

        #endregion

        #region 取消

        #endregion

        #region 添加附件

        #endregion

        #endregion

        #region 提交审核

        /// <summary>
        /// 提交审核
        /// </summary>
        public DelegateCommand<object> CommitCommand { get; private set; }

        private void OnCommit(object obj)
        {
            this.SelPlan.PublishStatus = (int)PlanPublishStatus.Checking;
            this.PublishingPlan.Add(this.SelPlan);
            RefreshButtonState();
        }

        private bool CanCommit(object obj)
        {
            if (!this._viewButtonValidity["提交审核"])
            {
                return false;
            }
            // 没有选中计划，或者当前已有发布中的计划时，按钮不可用
            if (this.SelPlan == null || this.PublishingPlan.Any())
            {
                return false;
            }
            // 有未保存内容时，按钮不可用
            if (this.service.EntityContainer.HasChanges)
            {
                return false;
            }
            // 1、不存在已发布的计划
            // 编制状态处于已提交，且发布状态为待发布时，按钮可用
            if (this.LastPublishedPlan == null)
            {
                return this.SelPlan.Status == (int)PlanStatus.Submited &&
                       this.SelPlan.PublishStatus == (int)PlanPublishStatus.Draft;
            }
            // 2、存在已发布的计划
            // 编制状态处于已提交，发布状态为待发布，且选中计划的版本高于最后一份已发布计划版本时，按钮可用
            return this.SelPlan.Status == (int)PlanStatus.Submited &&
                   this.SelPlan.PublishStatus == (int)PlanPublishStatus.Draft &&
                   this.SelPlan.VersionNumber > this.LastPublishedPlan.VersionNumber;
        }

        #endregion

        #region 审核

        /// <summary>
        /// 审核
        /// </summary>
        public DelegateCommand<object> ExamineCommand { get; private set; }

        private void OnExamine(object obj)
        {
            this.SelPlan.PublishStatus = (int)PlanPublishStatus.Checked;
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
            // 选中计划发布状态为待审核时，按钮可用
            return this.SelPlan != null && this.SelPlan.PublishStatus == (int)PlanPublishStatus.Checking;
        }

        #endregion

        #region 发送

        /// <summary>
        /// 发送
        /// </summary>
        public DelegateCommand<object> SendCommand { get; private set; }

        private void OnSend(object obj)
        {
            RadWindow.Confirm(this.SetConfirm("确认发布计划", "确认", "取消", "是否向【民航局】报送计划发布情况？", 13, 250, (o, e) =>
            {
                if (e.DialogResult == true)
                {
                    // 审核、已提交状态下可以发送。如果已处于提交状态，需要重新发送的，不必改变状态。
                    if (this.SelPlan.PublishStatus != (int)PlanPublishStatus.Submited)
                    {
                        this.SelPlan.PublishStatus = (int)PlanPublishStatus.Submited;
                        this.service.SubmitChanges(sc =>
                        {
                            if (sc.Error == null)
                            {
                                // 发送不成功的，也认为是已经做了发送操作，不回滚状态。始终可以重新发送。
                                this.service.TransferPlan(this.SelPlan.PlanID, tp => { }, null);
                            }
                        }, null);
                    }
                    else
                    {
                        this.service.TransferPlan(this.SelPlan.PlanID, tp => { }, null);
                    }
                }
            }));
            RefreshButtonState();
        }

        private bool CanSend(object obj)
        {
            if (!this._viewButtonValidity["发送"])
            {
                return false;
            }
            // 没有选中计划，按钮不可用
            if (this.SelPlan == null)
            {
                return false;
            }
            // 选中计划发布状态不是已审核与已提交时，按钮不可用。
            if (this.SelPlan.PublishStatus != (int)PlanPublishStatus.Checked && this.SelPlan.PublishStatus != (int)PlanPublishStatus.Submited)
            {
                return false;
            }
            // 当前没有未保存内容时，按钮可用
            return !this.service.EntityContainer.HasChanges;
        }

        #endregion

        #region 撤销发布

        /// <summary>
        /// 撤销发布
        /// </summary>
        public DelegateCommand<object> RepealCommand { get; private set; }

        private void OnRepeal(object obj)
        {
            var content = this.SelPlan.PublishStatus == (int)PlanPublishStatus.Submited
                           ? "是否要将发布状态撤回到待发布（同时向民航局报送）？"
                           : "是否要将发布状态撤回到待发布（不会向民航局报送）？";
            RadWindow.Confirm(this.SetConfirm("确认撤销发布", "确认", "取消", content, 13, 250, (o, e) =>
                {
                    if (e.DialogResult == true)
                    {
                        // 1、发布状态为已发布时，需要向民航局发送邮件
                        if (this.SelPlan.PublishStatus == (int)PlanPublishStatus.Submited)
                        {
                            this.SelPlan.PublishStatus = (int)PlanPublishStatus.Draft;
                            this.service.SubmitChanges(sc =>
                            {
                                if (sc.Error == null)
                                {
                                    this.service.TransferPlan(this.SelPlan.PlanID, tp => { }, null);
                                    RefreshButtonState();
                                }
                            }, null);
                        }
                        // 2、发布状态不是已发布时，无需向民航局发送邮件
                        else
                        {
                            this.SelPlan.PublishStatus = (int)PlanPublishStatus.Draft;
                            this.service.SubmitChanges(sc => { }, null);
                            RefreshButtonState();
                        }
                    }
                }));
            this.PublishingPlan.Remove(this.SelPlan);
            RefreshButtonState();
        }

        private bool CanRepeal(object obj)
        {
            if (!this._viewButtonValidity["撤销发布"])
            {
                return false;
            }
            // 有未保存内容时，按钮不可用
            if (this.service.EntityContainer.HasChanges)
            {
                return false;
            }
            // 选中计划的发布状态不是待发布，且没有未保存的内容时，按钮可用
            return this.SelPlan != null && this.SelPlan.PublishStatus > (int)PlanPublishStatus.Draft &&
                   !this.service.EntityContainer.HasChanges;
        }

        #endregion

        #region Methods


        #endregion

        #endregion

        #endregion

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
            this.CommitCommand = new DelegateCommand<object>(this.OnCommit, this.CanCommit);
            this.ExamineCommand = new DelegateCommand<object>(this.OnExamine, this.CanExamine);
            this.SendCommand = new DelegateCommand<object>(this.OnSend, this.CanSend);
            this.RepealCommand = new DelegateCommand<object>(this.OnRepeal, this.CanRepeal);

            this._viewButtonValidity = authService.GetViewButtonValidity(typeof(AfrpPlanPublishView).ToString());
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
        }

        #endregion

    }
}


