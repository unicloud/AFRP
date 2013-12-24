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
    [Export(typeof(AfrpPlanSendViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AfrpPlanSendViewModel : EditViewModelBase, IPartImportsSatisfiedNotification, INavigationAware
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
            this.SendCommand.RaiseCanExecuteChanged();
            this.AttachCommand.RaiseCanExecuteChanged();
        }

        #endregion

        #region 管理界面操作

        private void RefreshView()
        {
            this.CurrentPlan = this.service.CurrentPlan;
            this._needReFreshViewPlan = true;
            this.RaiseViewPlan();
            this._needReFreshViewCurrentPlan = true;
            this.RaiseViewCurrentPlan();
            this._needReFreshViewPlanHistory = true;
            this.RaiseViewPlanHistory();
            this.RefreshButtonState();
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
        public IEnumerable<Plan> ViewCurrentPlan
        {
            get { return this.service.EntityContainer.GetEntitySet<Plan>().Where(p => p == this.CurrentPlan); }
        }

        private bool _needReFreshViewCurrentPlan = true;

        private void RaiseViewCurrentPlan()
        {
            if (this._needReFreshViewCurrentPlan)
            {
                RaisePropertyChanged(() => this.ViewCurrentPlan);
                this._needReFreshViewCurrentPlan = false;
            }
        }

        private Plan _selCurrentPlan;

        /// <summary>
        /// 选中的计划
        /// </summary>
        public Plan SelCurrentPlan
        {
            get { return this._selCurrentPlan; }
            set
            {
                if (this._selCurrentPlan != value)
                {
                    _selCurrentPlan = value;
                    this.RaisePropertyChanged(() => this.SelCurrentPlan);
                }
            }
        }

        #endregion

        #region 计划

        /// <summary>
        /// 计划集合
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
        /// 选中的计划
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

        #region 当前计划

        private Plan _currentPlan;

        /// <summary>
        /// 当前计划
        /// </summary>
        public Plan CurrentPlan
        {
            get { return this._currentPlan; }
            private set
            {
                if (this._currentPlan != value)
                {
                    _currentPlan = value;
                    this.RaisePropertyChanged(() => this.CurrentPlan);
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

        #region 保存

        #endregion

        #region 放弃更改

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
            // 当前计划处于审核状态时，按钮可用
            return this.CurrentPlan != null && this.CurrentPlan.Status == (int)PlanStatus.Checked;
        }

        #endregion

        #endregion

        #region 发送

        /// <summary>
        /// 发送
        /// </summary>
        public DelegateCommand<object> SendCommand { get; private set; }

        private void OnSend(object obj)
        {
            var content = "计划选择了" + this.CurrentPlan.PlanHistories.Count(p => p.IsSubmit) + "项报送的明细项，是否向【民航局】报送该计划？";
            RadWindow.Confirm(this.SetConfirm("确认报送计划", "确认", "取消", content, 13, 250, (o, e) =>
                {
                    if (e.DialogResult == true)
                    {
                        // 审核、已提交状态下可以发送。如果已处于提交状态，需要重新发送的，不必改变状态。
                        if (this.CurrentPlan != null && this.CurrentPlan.Status != (int)PlanStatus.Submited)
                        {
                            this.CurrentPlan.Status = (int)PlanStatus.Submited;
                            this.CurrentPlan.IsFinished = true;
                        }
                        this.CurrentPlan.SubmitDate = DateTime.Now;
                        this.service.SubmitChanges(sc =>
                        {
                            if (sc.Error == null)
                            {
                                this.AttachCommand.RaiseCanExecuteChanged();
                                // 发送不成功的，也认为是已经做了发送操作，不回滚状态。始终可以重新发送。
                                this.service.TransferPlan(this.CurrentPlan.PlanID, tp => { }, null);
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
            // 当前计划的文档非空，当前计划处于审核、已发送状态，且当前没有未保存内容时，按钮可用
            if (this.CurrentPlan == null)
            {
                return false;
            }
            //if (string.IsNullOrWhiteSpace(this.CurrentPlan.DocNumber) || this.CurrentPlan.AttachDoc == null)
            if (this.CurrentPlan.AttachDoc == null)
            {
                return false;
            }
            if (this.CurrentPlan.Status != (int)PlanStatus.Checked && this.CurrentPlan.Status != (int)PlanStatus.Submited)
            {
                return false;
            }
            return !this.service.EntityContainer.HasChanges;
        }

        #endregion

        #endregion

        #region Methods


        #endregion

        #endregion

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
            this.SendCommand = new DelegateCommand<object>(this.OnSend, this.CanSend);

            this._viewButtonValidity = authService.GetViewButtonValidity(typeof(AfrpPlanSendView).ToString());
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
