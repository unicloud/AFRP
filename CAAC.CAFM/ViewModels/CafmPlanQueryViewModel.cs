using CAAC.Fleet.Services;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Windows.Data.DomainServices;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Telerik.Windows.Controls;
using UniCloud.Fleet.Models;

namespace CAAC.CAFM.ViewModels
{
    [Export(typeof(CafmPlanQueryViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public class CafmPlanQueryViewModel : NotificationObject, IPartImportsSatisfiedNotification, IConfirmNavigationRequest
    {
        public CafmPlanQueryViewModel()
        {

        }

        #region Local

        [Import]
        public IRegionManager regionManager;
        private readonly IFleetService _service = ServiceLocator.Current.GetInstance<IFleetService>();
        private readonly CAAC.CAFM.Views.CafmPlanQueryView _view = ServiceLocator.Current.GetInstance<CAAC.CAFM.Views.CafmPlanQueryView>();
        private RadGridView originalPlan, contrastPlan, originalPlanHistory, contrastPlanHistory;

        #region Property



        #endregion

        #region Method


        #region Local 方法 GetPlanHistories --获取计划历史

        /// <summary>
        /// 获取计划历史
        /// </summary>
        public void GetPlanHistories()
        {
            List<PlanHistory> allOriginalPlanHistories = new List<PlanHistory>();
            List<PlanHistory> allContrastPlanHistories = new List<PlanHistory>();

            //原始计划和比较计划相同申报年份、申报单位
            if ((SelOriginalPlan != null && SelContrastPlan != null && SelOriginalPlan.AirlinesID == SelContrastPlan.AirlinesID && SelOriginalPlan.AnnualID == SelContrastPlan.AnnualID && SelOriginalPlan.PlanID != SelContrastPlan.PlanID))
            {
                if (SelOriginalPlan.PlanHistories != null && SelContrastPlan.PlanHistories != null && SelOriginalPlan.PlanHistories.Count > 0 && SelContrastPlan.PlanHistories.Count > 0)
                {
                    //获取相同的原始计划历史
                    var equalOriginalPlanHistories = from t in SelOriginalPlan.PlanHistories
                                                     from r in SelContrastPlan.PlanHistories
                                                     where t.PlanAircraftID == r.PlanAircraftID
                                                     select t;
                    //获取相同的比较计划历史
                    var equalContrastPlanHisotries = from t in SelOriginalPlan.PlanHistories
                                                     from r in SelContrastPlan.PlanHistories
                                                     where t.PlanAircraftID == r.PlanAircraftID
                                                     select r;
                    var diffientOriginalPlan = this.SelOriginalPlan.PlanHistories.Except(equalOriginalPlanHistories);
                    var diffientContrastPlan = this.SelContrastPlan.PlanHistories.Except(equalContrastPlanHisotries);

                    //相同记录的匹配
                    equalOriginalPlanHistories.ToList()
                        .ForEach
                        (
                             ep =>
                             {

                                 PlanHistory ph = equalContrastPlanHisotries.FirstOrDefault(p => p.PlanHistoryID == ep.PlanHistoryID);
                                 //相同项判断是否相同，如果不同设置属性 IsDifferent为true
                                 bool IsMatching = (ep.Note != ph.Note || ep.ActionCategoryID != ph.ActionCategoryID || ph.SeatingCapacity != ph.SeatingCapacity || ph.CarryingCapacity != ep.CarryingCapacity || ph.PerformAnnualID != ep.PerformAnnualID || ph.PerformMonth != ep.PerformMonth || ph.AircraftTypeID != ep.AircraftTypeID);
                                 if (IsMatching)
                                 {
                                     ep.IsDifferent = true;
                                     ph.IsDifferent = true;
                                 }
                                 allOriginalPlanHistories.Add(ep);
                                 allContrastPlanHistories.Add(ph);


                             }
                        );

                }
            }
            else
            {
                OriginalPlanHistories = SelOriginalPlan.PlanHistories.ToList();
                ContrastPlanHistories = SelContrastPlan.PlanHistories.ToList();

            }

        }
        #endregion

        /// <summary>
        /// 以View的实例初始化ViewModel相关字段、属性
        /// </summary>
        private void ViewModelInitializer()
        {
            this.originalPlan = this._view.originalPlan as RadGridView;
            this.contrastPlan = this._view.contrastPlan as RadGridView;
            this.originalPlanHistory = this._view.originalPlanHistory as RadGridView;
            this.contrastPlanHistory = this._view.contrastPlanHistory as RadGridView;
        }

        #endregion

        #endregion

        #region ViewModel


        #region  对比计划
        /// <summary>
        /// Plan集合 ,原始计划
        /// </summary>
        public IEnumerable<Plan> ContrastPlans
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<Plan>();
            }
        }

        private bool _isBusyPlan;
        public bool IsBusyPlan
        {
            get { return this._isBusyPlan; }
            private set
            {
                if (this._isBusyPlan != value)
                {
                    this._isBusyPlan = value;
                }
            }
        }

        /// <summary>
        /// 加载实体集合的方法
        /// </summary>
        private void LoadPlan()
        {
            this.IsBusy = true;
            this._service.LoadPlan(new QueryBuilder<Plan>(), lo =>
            {
                this.IsBusyPlan = false;
                this.IsBusy = this.IsBusyPlan;
                if (lo.Error != null)
                {
                    // 处理加载失败

                }
                else
                {

                    this.RaisePropertyChanged(() => this.OriginalPans);
                    this.RaisePropertyChanged(() => this.ContrastPlans);
                }
            }, null);
        }

        #endregion

        #region 原始计划

        /// <summary>
        /// Plan集合 ,原始计划
        /// </summary>
        public IEnumerable<Plan> OriginalPans
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<Plan>();
            }
        }



        #endregion

        #region Property

        #region ViewModel 属性 忙闲状态

        private bool _isBusy;
        /// <summary>
        /// 忙闲状态
        /// </summary>
        public bool IsBusy
        {
            get { return this._isBusy; }
            private set
            {
                if (this._isBusy != value)
                {
                    this._isBusy = value;
                    this.RaisePropertyChanged(() => this.IsBusy);
                }
            }
        }

        #endregion

        #region ViewModel 属性 SelOriginalPlan --选中原始计划

        private Plan _selOriginalPlan;
        /// <summary>
        /// 选中原始计划
        /// </summary>
        public Plan SelOriginalPlan
        {
            get { return _selOriginalPlan; }
            set
            {
                _selOriginalPlan = value;
                if (value != null)
                {
                    OriginalGridViewDetailHeader = value.Airlines.ShortName + " " + value.Annual.Year + "年度，" + value.VersionNumber + "版本计划明细";
                }
                this.RaisePropertyChanged(() => this.SelOriginalPlan);


            }
        }
        #endregion

        #region ViewModel 属性 SelContrastPlan --选中比较的计划

        private Plan _selContrastPlan;
        /// <summary>
        /// 选中比较的计划
        /// </summary>
        public Plan SelContrastPlan
        {
            get { return _selContrastPlan; }
            set
            {
                _selContrastPlan = value;
                if (value != null)
                {
                    ContrastGridViewDetailHeader = value.Airlines.ShortName + " " + value.Annual.Year + "年度，" + value.VersionNumber + "版本计划明细";

                }

                this.RaisePropertyChanged(() => this.SelContrastPlan);
            }
        }
        #endregion

        #region ViewModel 属性 OriginalPlanHistories --原始计划历史

        private List<PlanHistory> _originalPlanHistories;
        /// <summary>
        /// 原始计划历史
        /// </summary>
        public List<PlanHistory> OriginalPlanHistories
        {
            get { return _originalPlanHistories; }
            set
            {

                if (OriginalPlanHistories != value)
                {
                    _originalPlanHistories = value;
                    this.RaisePropertyChanged(() => this.OriginalPlanHistories);

                }
            }
        }
        #endregion

        #region ViewModel 属性 ContrastPlanHistories --比较计划历史

        private List<PlanHistory> _contrastPlanHistories;
        /// <summary>
        /// 比较计划历史
        /// </summary>
        public List<PlanHistory> ContrastPlanHistories
        {
            get { return _contrastPlanHistories; }
            set
            {

                if (ContrastPlanHistories != value)
                {
                    _contrastPlanHistories = value;
                    this.RaisePropertyChanged(() => this.ContrastPlanHistories);

                }
            }
        }
        #endregion

        #region ViewModel 属性 OriginalGridViewDetailHeader --原始计划DockingPanelGroup 标题

        private string _originalGridViewDetailHeader = "计划明细";
        /// <summary>
        /// 原始计划DockingPanelGroup 标题
        /// </summary>
        public string OriginalGridViewDetailHeader
        {
            get { return _originalGridViewDetailHeader; }
            set
            {

                if (OriginalGridViewDetailHeader != value)
                {
                    _originalGridViewDetailHeader = value;
                    this.RaisePropertyChanged(() => this.OriginalGridViewDetailHeader);

                }
            }
        }
        #endregion

        #region ViewModel 属性 ContrastGridViewDetailHeader --对比计划DockingPanelGroup 标题

        private string _contrastGridViewDetailHeader = "计划明细";
        /// <summary>
        /// 对比计划DockingPanelGroup 标题
        /// </summary>
        public string ContrastGridViewDetailHeader
        {
            get { return _contrastGridViewDetailHeader; }
            set
            {

                if (ContrastGridViewDetailHeader != value)
                {
                    _contrastGridViewDetailHeader = value;
                    this.RaisePropertyChanged(() => this.ContrastGridViewDetailHeader);

                }
            }
        }
        #endregion

        #region ViewModel 属性 IsContextMenuOpen --控制右键菜单的打开

        private bool _isContextMenuOpen = true;
        /// <summary>
        /// 控制右键菜单的打开
        /// </summary>
        public bool IsContextMenuOpen
        {
            get { return this._isContextMenuOpen; }
            set
            {

                if (this._isContextMenuOpen != value)
                {
                    _isContextMenuOpen = value;
                    this.RaisePropertyChanged(() => this.IsContextMenuOpen);

                }
            }
        }
        #endregion

        #endregion

        #region Command

        #region ViewModel 命令 ViewAttachmentCommand --查看附件

        // 查看附件
        public DelegateCommand<object> ViewAttachmentCommand { get; private set; }
        private void OnViewAttachment(object obj)
        {
            Plan plan = obj as Plan;
            if (plan != null)
            {
                AttachmentOperation.OpenAttachment<Plan>(plan);
            }
        }
        private bool _canViewAttachment = true;
        public bool CanViewAttachment(object obj)
        {
            return _canViewAttachment;
        }
        #endregion

        #region ViewModel 命令 --导出数据

        public DelegateCommand<object> ExportGridViewCommand { get; private set; }

        private void OnExportGridView(object sender)
        {
            RadMenuItem menu = sender as RadMenuItem;
            IsContextMenuOpen = false;
            if (menu != null && menu.Name == "OriginalPlanExport" && originalPlan != null)
            {
                originalPlan.ElementExporting -= this.ElementExporting;
                originalPlan.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGirdOperation.DowmLoadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        originalPlan.Export(stream, ImageAndGirdOperation.SetGridViewExportOptions());
                    }
                }
            }
            else if (menu != null && menu.Name == "ContrastPlanExport" && contrastPlan != null)
            {
                contrastPlan.ElementExporting -= this.ElementExporting;
                contrastPlan.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGirdOperation.DowmLoadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        contrastPlan.Export(stream, ImageAndGirdOperation.SetGridViewExportOptions());
                    }
                }
            }
            else if (menu != null && menu.Name == "OriginalPlanHistoryExport" && originalPlanHistory != null)
            {
                originalPlanHistory.ElementExporting -= this.ElementExporting;
                originalPlanHistory.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGirdOperation.DowmLoadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        originalPlanHistory.Export(stream, ImageAndGirdOperation.SetGridViewExportOptions());
                    }
                }
            }
            else if (menu != null && menu.Name == "ContrastPlanHistoryExport" && contrastPlanHistory != null)
            {
                contrastPlanHistory.ElementExporting -= this.ElementExporting;
                contrastPlanHistory.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGirdOperation.DowmLoadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        contrastPlanHistory.Export(stream, ImageAndGirdOperation.SetGridViewExportOptions());
                    }
                }
            }
        }
        /// <summary>
        /// 设置导出样式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ElementExporting(object sender, GridViewElementExportingEventArgs e)
        {
            e.Width = 120;
        }
        private bool _canExportGridView = true;
        bool CanExportGridView(object sender)
        {
            return this._canExportGridView;
        }

        #endregion

        #endregion

        #region Method

        public void ContextMenuOpened(object sender, System.Windows.RoutedEventArgs e)
        {
            IsContextMenuOpen = true;
        }
        #endregion

        #endregion

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
            SubscribeEvent();
            ViewModelInitializer();
            ViewAttachmentCommand = new DelegateCommand<object>(this.OnViewAttachment, this.CanViewAttachment);
            this.ExportGridViewCommand = new DelegateCommand<object>(this.OnExportGridView, this.CanExportGridView);//导出GridView数据
        }

        #endregion


        #region 订阅事件处理

        private void SubscribeEvent()
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

        #region IConfirmNavigationRequest Members

        public void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            continuationCallback(true);
        }

        #endregion
    }

}
