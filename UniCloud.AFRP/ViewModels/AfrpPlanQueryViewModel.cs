using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Windows.Data.DomainServices;
using UniCloud.Fleet.Models;
using ViewModelBase = UniCloud.Fleet.Services.ViewModelBase;

namespace UniCloud.AFRP.ViewModels
{
    [Export(typeof(AfrpPlanQueryViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AfrpPlanQueryViewModel : ViewModelBase, IPartImportsSatisfiedNotification, INavigationAware
    {

        #region Local

        [Import]
        public IRegionManager regionManager;

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
                            this.LoadCompleted();
                        }
                    }
                }, null);
        }

        #endregion

        #region 管理界面操作

        private void RefreshView()
        {
            var selOriginalPlan = this.SelOriginalPlan;
            this._needReFreshViewOriginalPlan = true;
            this.RaiseViewOriginalPlan();
            this.SelOriginalPlan = selOriginalPlan ?? this.ViewOriginalPlan.FirstOrDefault();
            var selCompare = this.SelComparePlan;
            this._needReFreshViewComparePlan = true;
            this.RaiseViewComparePlan();
            this.SelComparePlan = selCompare ?? this.ViewComparePlan.FirstOrDefault();
        }

        #endregion

        #endregion

        #endregion

        #region ViewModel

        #region 数据集合

        #region 原始计划

        /// <summary>
        /// 原始计划集合
        /// </summary>
        public IEnumerable<Plan> ViewOriginalPlan
        {
            get { return this.service.EntityContainer.GetEntitySet<Plan>(); }
        }

        private bool _needReFreshViewOriginalPlan = true;

        private void RaiseViewOriginalPlan()
        {
            if (this._needReFreshViewOriginalPlan)
            {
                RaisePropertyChanged(() => this.ViewOriginalPlan);
                this._needReFreshViewOriginalPlan = false;
            }
        }

        private Plan _selOriginalPlan;

        /// <summary>
        /// 选中的原始计划
        /// </summary>
        public Plan SelOriginalPlan
        {
            get { return this._selOriginalPlan; }
            set
            {
                if (this._selOriginalPlan != value)
                {
                    this._selOriginalPlan = value;
                    this.RaisePropertyChanged(() => this.SelOriginalPlan);
                    this._needReFreshViewOriginalPlanHistory = true;
                    this.RaiseViewOriginalPlanHistory();
                }
            }
        }

        #endregion

        #region 对比计划

        /// <summary>
        /// 对比计划集合
        /// </summary>
        public IEnumerable<Plan> ViewComparePlan
        {
            get { return this.service.EntityContainer.GetEntitySet<Plan>(); }
        }

        private bool _needReFreshViewComparePlan = true;

        private void RaiseViewComparePlan()
        {
            if (this._needReFreshViewComparePlan)
            {
                RaisePropertyChanged(() => this.ViewComparePlan);
                this._needReFreshViewComparePlan = false;
            }
        }

        private Plan _selComparePlan;

        /// <summary>
        /// 选中的对比计划
        /// </summary>
        public Plan SelComparePlan
        {
            get { return this._selComparePlan; }
            set
            {
                if (this._selComparePlan != value)
                {
                    this._selComparePlan = value;
                    this.RaisePropertyChanged(() => this.SelComparePlan);
                    this._needReFreshViewComparePlanHistory = true;
                    this.RaiseViewComparePlanHistory();
                }
            }
        }

        #endregion

        #region 原始计划明细

        /// <summary>
        /// 原始计划明细集合
        /// </summary>
        public IEnumerable<PlanHistory> ViewOriginalPlanHistory
        {
            get
            {
                return
                    this.service.EntityContainer.GetEntitySet<PlanHistory>()
                        .OrderBy(ph => ph.PerformTime)
                        .Where(ph => ph.Plan == this.SelOriginalPlan);
            }
        }

        private bool _needReFreshViewOriginalPlanHistory = true;

        private void RaiseViewOriginalPlanHistory()
        {
            if (this._needReFreshViewOriginalPlanHistory)
            {
                RaisePropertyChanged(() => this.ViewOriginalPlanHistory);
                this._needReFreshViewOriginalPlanHistory = false;
            }
        }

        private PlanHistory _selOriginalPlanHistory;

        /// <summary>
        /// 选中的原始计划明细
        /// </summary>
        public PlanHistory SelOriginalPlanHistory
        {
            get { return this._selOriginalPlanHistory; }
            set
            {
                if (this._selOriginalPlanHistory != value)
                {
                    this._selOriginalPlanHistory = value;
                    this.RaisePropertyChanged(() => this.SelOriginalPlanHistory);
                }
            }
        }

        #endregion

        #region 对比计划明细

        /// <summary>
        /// 对比计划明细集合
        /// </summary>
        public IEnumerable<PlanHistory> ViewComparePlanHistory
        {
            get
            {
                return
                    this.service.EntityContainer.GetEntitySet<PlanHistory>()
                        .OrderBy(ph => ph.PerformTime)
                        .Where(ph => ph.Plan == this.SelComparePlan);
            }
        }

        private bool _needReFreshViewComparePlanHistory = true;

        private void RaiseViewComparePlanHistory()
        {
            if (this._needReFreshViewComparePlanHistory)
            {
                RaisePropertyChanged(() => this.ViewComparePlanHistory);
                this._needReFreshViewComparePlanHistory = false;
            }
        }

        private PlanHistory _selComparePlanHistory;

        /// <summary>
        /// 选中的对比计划明细
        /// </summary>
        public PlanHistory SelComparePlanHistory
        {
            get { return this._selComparePlanHistory; }
            set
            {
                if (this._selComparePlanHistory != value)
                {
                    this._selComparePlanHistory = value;
                    this.RaisePropertyChanged(() => this.SelComparePlanHistory);
                }
            }
        }

        #endregion

        #endregion

        #region Property

        #endregion

        #region Command

        #endregion

        #region Methods

        #endregion

        #endregion

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
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
