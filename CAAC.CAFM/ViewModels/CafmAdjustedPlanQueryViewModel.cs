using CAAC.Fleet.Services;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Windows.Data.DomainServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Net;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;
using UniCloud.Fleet.Models;
using Microsoft.Practices.Prism.Commands;
using System.IO;

namespace CAAC.CAFM.ViewModels
{
    [Export(typeof(CafmAdjustedPlanQueryViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public class CafmAdjustedPlanQueryViewModel : NotificationObject, IPartImportsSatisfiedNotification, IConfirmNavigationRequest
    {
        public CafmAdjustedPlanQueryViewModel()
        {
        }


        #region Local

        [Import]
        public IRegionManager regionManager;
        private readonly IFleetService _service = ServiceLocator.Current.GetInstance<IFleetService>();
        private readonly CAAC.CAFM.Views.CafmAdjustedPlanQueryView _view = ServiceLocator.Current.GetInstance<CAAC.CAFM.Views.CafmAdjustedPlanQueryView>();
        private RadGridView  adjustedPlans, originalPublishedPlanHistory, adjustedPlanHistory;
        private int i; //用于导出数据的格式控制
        #region Property

        #endregion

        #region Method


        /// <summary>
        /// 以View的实例初始化ViewModel相关字段、属性
        /// </summary>
        private void ViewModelInitializer()
        {
            this.adjustedPlans = this._view.adjustedPlans as RadGridView;
            this.originalPublishedPlanHistory = this._view.originalPublishedPlanHistory as RadGridView;
            this.adjustedPlanHistory = this._view.adjustedPlanHistory as RadGridView;
        }


        
        #region ViewModel 方法 GetAllPlanHistroy --获取计划历史的集合

        /// <summary>
        /// 获取计划历史的集合
        /// </summary>
        public void GetAllPlanHistroy()
        {
            List<PlanHistory> allOriginalPublishedPlanHistories = new List<PlanHistory>();
            List<PlanHistory> allAdjustedPlanHistories = new List<PlanHistory>();
            //List<PlanHistory> allDeletedPlanHistorise = new List<PlanHistory>();

            if (CurrentAdjustedPlan != null && OriginalPublishedPlan != null)
            {
                allOriginalPublishedPlanHistories = OriginalPublishedPlan.PlanHistories.ToList();
                allAdjustedPlanHistories = CurrentAdjustedPlan.PlanHistories.ToList();
                //foreach (PlanHistory planHistory in allOriginalPublishedPlanHistories)
                //{
                //    if(!(CurrentAdjustedPlan.PlanHistories.Any(p=>p.PlanAircraftID==planHistory.PlanAircraftID)))
                //        allDeletedPlanHistorise.Add(planHistory);
                //}
            }
            OriginalPublishedPlanHistories = allOriginalPublishedPlanHistories;
            AdjustedPlanHistories = allAdjustedPlanHistories;
            //DeletedPlanHistories = allDeletedPlanHistorise;
            //this.originalPublishedPlanHistory.GroupDescriptors.Reset();
        }
        #endregion
         


        #endregion

        #endregion


        #region ViewModel


        #region  调整计划
        /// <summary>
        /// Plan集合 ,调整计划
        /// </summary>
        public IEnumerable<Plan> AdjustedPlans
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<Plan>().Where(p => p.Title.Contains("年度机队资源调整"));
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

                    this.RaisePropertyChanged(() => this.AllPlans);
                    this.RaisePropertyChanged(() => this.AdjustedPlans);
                    //SelAdjustedPlan = AdjustedPlans.FirstOrDefault();
                    //this.adjustedPlanHistory.GroupDescriptors.Reset();
                }
            }, null);
        }

        #endregion

        #region 所有计划集合

        /// <summary>
        /// Plan集合 ,所有计划集合
        /// </summary>
        public IEnumerable<Plan> AllPlans
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<Plan>();
            }
        }



        #endregion

        #region Property

        #region ViewModel 属性 IsBusy

        private bool _isBusy;
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

        #region ViewModel 属性 OriginalPublishedPlan --选中的调整计划对应的原发布计划

        private Plan _originalPublishedPlan;
        /// <summary>
        /// 选中的调整计划对应的原发布计划
        /// </summary>
        public Plan OriginalPublishedPlan
        {
            get { return _originalPublishedPlan; }
            set
            {
                _originalPublishedPlan = value;
                if (value != null)
                {
                }
                this.RaisePropertyChanged(() => this.OriginalPublishedPlan);


            }
        }
        #endregion



        #region ViewModel 属性 CurrentAdjustedPlan --当前的调整计划

        private Plan _currentAdjustedPlan;
        /// <summary>
        /// 当前的调整计划
        /// </summary>
        public Plan CurrentAdjustedPlan
        {
            get { return _currentAdjustedPlan; }
            set
            {
                _currentAdjustedPlan = value;
                if (value != null)
                {
                }
                this.RaisePropertyChanged(() => this.CurrentAdjustedPlan);


            }
        }
        #endregion

        #region ViewModel 属性 SelAdjustedPlan --选中调整计划

        private Plan _selAdjustedPlan;
        /// <summary>
        /// 选中比较的计划
        /// </summary>
        public Plan SelAdjustedPlan
        {
            get { return _selAdjustedPlan; }
            set
            {
                _selAdjustedPlan = value;
                if (value != null)
                {
                    CurrentAdjustedPlan = AdjustedPlans.Where(p => p.PlanID == value.PlanID).FirstOrDefault();
                    OriginalPublishedPlan =
                        AllPlans.Where(
                            p =>
                            p.Airlines == SelAdjustedPlan.Airlines && p.AnnualID == SelAdjustedPlan.AnnualID &&
                            p.VersionNumber == SelAdjustedPlan.VersionNumber - 1).FirstOrDefault();
                    OriginalGridViewDetailHeader = OriginalPublishedPlan.Airlines.ShortName + " " +
                                                   OriginalPublishedPlan.Annual.Year + "年度，" +
                                                   OriginalPublishedPlan.VersionNumber + "版本发布计划明细";
                    AdjustedGridViewDetailHeader = CurrentAdjustedPlan.Airlines.ShortName + " " +
                                                   CurrentAdjustedPlan.Annual.Year + "年度，" +
                                                   CurrentAdjustedPlan.VersionNumber + "版本调整计划明细（碧蓝色为修改项，雄黄色为新增项） ";
                    //DeletedGridViewDetailHeader = OriginalPublishedPlan.Airlines.ShortName + " " +
                    //                              OriginalPublishedPlan.Annual.Year + "年度，" +
                    //                              OriginalPublishedPlan.VersionNumber + "版本已删除计划明细";
                    GetAllPlanHistroy();
                }
                this.RaisePropertyChanged(() => this.SelAdjustedPlan);
            }
        }
        #endregion

        #region ViewModel 属性 OriginalPublishedPlanHistories --原发布计划历史

        private List<PlanHistory> _originalPublishedPlanHistories;
        /// <summary>
        /// 原始计划历史
        /// </summary>
        public List<PlanHistory> OriginalPublishedPlanHistories
        {
            get { return _originalPublishedPlanHistories; }
            set
            {

                if (OriginalPublishedPlanHistories != value)
                {
                    _originalPublishedPlanHistories = value;
                    this.RaisePropertyChanged(() => this.OriginalPublishedPlanHistories);

                }
            }
        }
        #endregion

        #region ViewModel 属性 AdjustedPlanHistories --调整计划历史

        private List<PlanHistory> _adjustedPlanHistories;
        /// <summary>
        /// 调整计划历史
        /// </summary>
        public List<PlanHistory> AdjustedPlanHistories
        {
            get { return _adjustedPlanHistories; }
            set
            {

                if (AdjustedPlanHistories != value)
                {
                    _adjustedPlanHistories = value;
                    this.RaisePropertyChanged(() => this.AdjustedPlanHistories);

                }
            }
        }
        #endregion

        

        //#region ViewModel 属性 DeletedPlanHistories --已删除计划历史

        //private List<PlanHistory> _deletedPlanHistories;
        ///// <summary>
        ///// 已删除计划历史
        ///// </summary>
        //public List<PlanHistory> DeletedPlanHistories
        //{
        //    get { return _deletedPlanHistories; }
        //    set
        //    {

        //        if (DeletedPlanHistories != value)
        //        {
        //            _deletedPlanHistories = value;
        //            this.RaisePropertyChanged(() => this.DeletedPlanHistories);

        //        }
        //    }
        //}
        //#endregion

        #region ViewModel 属性 OriginalGridViewDetailHeader --原发布计划DockingPanelGroup 标题

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

        #region ViewModel 属性 AdjustedGridViewDetailHeader --调整计划DockingPanelGroup 标题

        private string _adjustedGridViewDetailHeader = "计划明细";
        /// <summary>
        /// 对比计划DockingPanelGroup 标题
        /// </summary>
        public string AdjustedGridViewDetailHeader
        {
            get { return _adjustedGridViewDetailHeader; }
            set
            {

                if (AdjustedGridViewDetailHeader != value)
                {
                    _adjustedGridViewDetailHeader = value;
                    this.RaisePropertyChanged(() => this.AdjustedGridViewDetailHeader);

                }
            }
        }
        #endregion

        //#region ViewModel 属性 DeletedGridViewDetailHeader --删除的计划项标题

        //private string _deletedGridViewDetailHeader = "已删除的计划项";
        ///// <summary>
        ///// 对比计划DockingPanelGroup 标题
        ///// </summary>
        //public string DeletedGridViewDetailHeader
        //{
        //    get { return _deletedGridViewDetailHeader; }
        //    set
        //    {

        //        if (DeletedGridViewDetailHeader != value)
        //        {
        //            _deletedGridViewDetailHeader = value;
        //            this.RaisePropertyChanged(() => this.DeletedGridViewDetailHeader);

        //        }
        //    }
        //}
        //#endregion

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

        #region ViewModel 命令 --导出数据

        public DelegateCommand<object> ExportGridViewCommand { get; private set; }

        private void OnExportGridView(object sender)
        {
            RadMenuItem menu = sender as RadMenuItem;
            IsContextMenuOpen = false;
            if (menu != null && menu.Name == "AdjustedPlansExport" && adjustedPlans != null)
            {
                adjustedPlans.ElementExporting -= this.ElementExporting;
                adjustedPlans.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGirdOperation.DowmLoadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        adjustedPlans.Export(stream, ImageAndGirdOperation.SetGridViewExportOptions());
                    }
                }
            }
            else if (menu != null && menu.Name == "OriginalPlanHistoryExport" && originalPublishedPlanHistory != null)
            {
                originalPublishedPlanHistory.ElementExporting -= this.ElementExporting;
                originalPublishedPlanHistory.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGirdOperation.DowmLoadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        originalPublishedPlanHistory.Export(stream, ImageAndGirdOperation.SetGridViewExportOptions());
                    }
                }
            }
            else if (menu != null && menu.Name == "AdjustedPlanHistoryExport" && adjustedPlanHistory != null)
            {
                adjustedPlanHistory.ElementExporting -= this.ElementExporting;
                adjustedPlanHistory.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGirdOperation.DowmLoadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        adjustedPlanHistory.Export(stream, ImageAndGirdOperation.SetGridViewExportOptions());
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
            if (e.Element == ExportElement.Row)
            { i = 1; }
            else if (e.Element == ExportElement.Cell &&
                e.Value != null)
            {
                var gridViewColumn = e.Context as GridViewColumn;
                if (gridViewColumn != null && gridViewColumn.UniqueName == "performTime")
                {
                    if (!string.IsNullOrEmpty(e.Value.ToString()))
                        e.Value = DateTime.Parse(e.Value.ToString()).ToString("yyyy年M月");
                }
                i++;
            }

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
