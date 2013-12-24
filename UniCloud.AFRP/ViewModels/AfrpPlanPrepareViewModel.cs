using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Windows.Data.DomainServices;
using UniCloud.AFRP.Views;
using UniCloud.Fleet.Models;
using UniCloud.Fleet.Services;
using UniCloud.Infrastructure;

namespace UniCloud.AFRP.ViewModels
{
    [Export(typeof(AfrpPlanPrepareViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AfrpPlanPrepareViewModel : EditViewModelBase, IPartImportsSatisfiedNotification, INavigationAware
    {

        #region Local

        [Import]
        public IRegionManager regionManager;

        private IDictionary<string, bool> _viewButtonValidity;
        private readonly int _leadMonth = Convert.ToInt32(AppConfig.leadmonth);

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
            this.UnlockCommand.RaiseCanExecuteChanged();
        }

        #endregion

        #region 管理界面操作

        private void RefreshView()
        {
            if (this.SelAnnual != this.service.CurrentAnnual)
            {
                this.SelAnnual = service.CurrentAnnual;
            }
            else
            {
                this._needReFreshViewPlan = true;
                this.RaiseViewPlan();
            }
            this.RefreshButtonState();
        }

        #endregion

        #endregion

        #endregion

        #region ViewModel

        #region 数据集合

        #region 年度

        /// <summary>
        /// 年度集合
        /// </summary>
        public IEnumerable<Annual> ViewAnnual
        {
            get { return this.service.EntityContainer.GetEntitySet<Annual>(); }
        }

        private bool _needReFreshViewAnnual = true;

        private void RaiseViewAnnual()
        {
            if (this._needReFreshViewAnnual)
            {
                RaisePropertyChanged(() => this.ViewAnnual);
                this._needReFreshViewAnnual = false;
            }
        }

        private Annual _selAnnual;

        /// <summary>
        /// 选中的年度
        /// </summary>
        public Annual SelAnnual
        {
            get { return this._selAnnual; }
            set
            {
                if (this._selAnnual != value)
                {
                    this._selAnnual = value;
                    this.RaisePropertyChanged(() => this.SelAnnual);
                    this._needReFreshViewPlan = true;
                    this.RaiseViewPlan();
                    this.SelPlan = this.ViewPlan.LastOrDefault();
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
            get { return this.service.EntityContainer.GetEntitySet<Plan>().Where(p => p.Annual == this.SelAnnual); }
        }

        private bool _needReFreshViewPlan = true;

        private void RaiseViewPlan()
        {
            if (this._needReFreshViewPlan)
            {
                RaisePropertyChanged(() => this.ViewPlan);
                this.SelPlan = ViewPlan.LastOrDefault();
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
                    this._needReFreshViewPlanHistory = true;
                    RaiseViewPlanHistory();
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

        #region 放弃更改

        protected override void OnAbortExecuted(object sender)
        {
            this.service.SetCurrentAnnual();
            this.service.SetCurrentPlan();
            this.SelAnnual = service.CurrentAnnual;
        }

        #endregion

        #endregion

        #region ViewModel 命令 -- 准备新年度计划

        public DelegateCommand<object> UnlockCommand { get; private set; }

        private void OnUnlock(object sender)
        {
            var title = this.service.CurrentAnnual.Year + 1 + AppConfig.planTitle;
            service.CreateNewYearPlan(title);
            this.RefreshView();
        }

        bool CanUnlock(object sender)
        {
            if (!this._viewButtonValidity["准备年度计划"])
            {
                return false;
            }
            // 有未保存内容时，按钮不可用
            if (this.service.EntityContainer.HasChanges)
            {
                return false;
            }
            // 当前月份在允许打开新计划年度的范围内且当前年度在当前打开年度，按钮可用。
            if (this.service.CurrentAnnual != null)
            {
                if (DateTime.Now.Month > 12 - this._leadMonth)
                    return this.service.CurrentAnnual.Year == DateTime.Now.Year;
                else return this.service.CurrentAnnual.Year == DateTime.Now.Year - 1;
            }
            return false;
        }

        #endregion

        #endregion

        #region Methods


        #endregion

        #endregion

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
            this.UnlockCommand = new DelegateCommand<object>(this.OnUnlock, this.CanUnlock);
            this._viewButtonValidity = authService.GetViewButtonValidity(typeof(AfrpPlanPrepareView).ToString());
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
