using CAAC.Fleet.Services;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Windows.Data.DomainServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Telerik.Windows.Controls;
using UniCloud.Fleet.Models;

namespace CAAC.CAFM.ViewModels
{
    [Export(typeof(CafmFleetQueryViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public class CafmFleetQueryViewModel : NotificationObject, IPartImportsSatisfiedNotification, IConfirmNavigationRequest
    {
        public CafmFleetQueryViewModel()
        {
            this._service.EntityContainer.PropertyChanged += new PropertyChangedEventHandler(EntityContainer_PropertyChanged);
        }

        #region Local

        [Import]
        public IRegionManager regionManager;
        private readonly IFleetService _service = ServiceLocator.Current.GetInstance<IFleetService>();
        private readonly CAAC.CAFM.Views.CafmFleetQueryView _view = ServiceLocator.Current.GetInstance<CAAC.CAFM.Views.CafmFleetQueryView>();
        private RadGridView aircraft, planHistory, requestHistory, ownershipHistory, operationHistory, aircraftBusiness;
        private int i; //导出数据源格式判断

        #region Property



        #endregion

        #region Method

        /// <summary>
        /// 跟踪实体变化，控制保存、放弃按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EntityContainer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "HasChanges")
            {
                //this._canSave = this._service.EntityContainer.HasChanges;
                //this._canAbort = this._service.EntityContainer.HasChanges;
                //this.SaveCommand.RaiseCanExecuteChanged();
                //this.AbortCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// 刷新按钮状态
        /// </summary>
        private void RefreshButtonState()
        {
        }

        /// <summary>
        /// 以View的实例初始化ViewModel相关字段、属性
        /// </summary>
        private void ViewModelInitializer()
        {
            this.aircraft = this._view.aircraft as RadGridView;
            this.planHistory = this._view.planHistory as RadGridView;
            this.requestHistory = this._view.requestHistory as RadGridView;
            this.ownershipHistory = this._view.ownershipHistory as RadGridView;
            this.operationHistory = this._view.operationHistory as RadGridView;
            this.aircraftBusiness = this._view.aircraftBusiness as RadGridView;
        }

        #endregion

        #endregion

        #region ViewModel

        #region 加载实体集合 Aircraft

        /// <summary>
        /// 用于绑定的集合，根据实际需要修改
        /// </summary>
        public IEnumerable<Aircraft> AircraftCollection
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<Aircraft>();
            }
        }

        private bool _isBusyAircraft = true;
        public bool IsBusyAircraft
        {
            get { return this._isBusyAircraft; }
            private set
            {
                if (this._isBusyAircraft != value)
                {
                    this._isBusyAircraft = value;
                }
            }
        }

        /// <summary>
        /// 加载实体集合的方法
        /// </summary>
        private void LoadAircraft()
        {
            this.IsBusy = true;
            this._service.LoadAircraft(new QueryBuilder<Aircraft>(), lo =>
            {
                this.IsBusyAircraft = false;
                this.IsBusy = this.IsBusyAircraft;
                if (lo.Error != null)
                {
                    // 处理加载失败

                }
                else
                {
                    this.RaisePropertyChanged(() => this.AircraftCollection);
                }
            }, null);
        }

        #endregion

        #region 加载实体集合 Plan

        /// <summary>
        /// 用于绑定的集合，根据实际需要修改
        /// </summary>
        public IEnumerable<Plan> ViewPlan
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<Plan>().Where(p => p.Annual.IsOpen == true);
            }
        }

        private bool _isBusyPlan = true;
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
                this.IsBusy = this.IsBusyPlan || this.IsBusyOperationHistory || this.IsBusyAircraftBusiness || this.IsBusyApprovalHistory || this.IsBusyAircraft || this.IsBusyRequest;
                if (lo.Error != null)
                {
                    // 处理加载失败

                }
                else
                {
                    this.RaisePropertyChanged(() => this.ViewPlan);
                }
            }, null);
        }

        #endregion

        #region 加载实体集合 Request

        /// <summary>
        /// 用于绑定的集合，根据实际需要修改
        /// </summary>
        public IEnumerable<Request> ViewRequest
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<Request>().Where(r => !r.IsFinished);
            }
        }

        private bool _isBusyRequest = true;
        public bool IsBusyRequest
        {
            get { return this._isBusyRequest; }
            private set
            {
                if (this._isBusyRequest != value)
                {
                    this._isBusyRequest = value;
                }
            }
        }

        /// <summary>
        /// 加载实体集合的方法
        /// </summary>
        private void LoadRequest()
        {
            this.IsBusy = true;
            this._service.LoadRequest(new QueryBuilder<Request>(), lo =>
            {
                this.IsBusyRequest = false;
                this.IsBusy = this.IsBusyPlan || this.IsBusyOperationHistory || this.IsBusyAircraftBusiness || this.IsBusyApprovalHistory || this.IsBusyAircraft || this.IsBusyRequest;
                if (lo.Error != null)
                {
                    // 处理加载失败

                }
                else
                {
                    this.RaisePropertyChanged(() => this.ViewRequest);
                }
            }, null);
        }

        #endregion

        #region 加载实体OperationHistory集合

        /// <summary>
        /// 获取计划历史，由于加载计划历史服务端Includ不到OperationHistory，故需要在前台加载OperationHistory，程序会自动完成匹配
        /// </summary>
        public IEnumerable<OperationHistory> ViewOperationHistory
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<OperationHistory>();
            }
        }

        private bool _isBusyOperationHistory = true;
        public bool IsBusyOperationHistory
        {
            get { return this._isBusyOperationHistory; }
            private set
            {
                if (this._isBusyOperationHistory != value)
                {
                    this._isBusyOperationHistory = value;
                }
            }
        }

        /// <summary>
        /// 加载实体集合的方法
        /// </summary>
        private void LoadOperationHistory()
        {
            this.IsBusy = true;
            this._service.LoadOperationHistory(new QueryBuilder<OperationHistory>(), lo =>
            {
                this.IsBusyOperationHistory = false;
                this.IsBusy = this.IsBusyPlan || this.IsBusyOperationHistory || this.IsBusyAircraftBusiness || this.IsBusyApprovalHistory || this.IsBusyAircraft || this.IsBusyRequest;
                if (lo.Error != null)
                {
                    // 处理加载失败

                }
                else
                {

                    this.RaisePropertyChanged(() => this.ViewOperationHistory);
                }
            }, null);
        }

        #endregion

        #region 加载实体AircraftBusiness集合

        /// <summary>
        /// 获取商业数据，由于加载计划历史服务端Includ不到AircraftBusiness，故需要在前台加载AircraftBusiness，程序会自动完成匹配
        /// </summary>
        public IEnumerable<AircraftBusiness> ViewAircraftBusiness
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<AircraftBusiness>();
            }
        }

        private bool _isBusyAircraftBusiness = true;
        public bool IsBusyAircraftBusiness
        {
            get { return this._isBusyAircraftBusiness; }
            private set
            {
                if (this._isBusyAircraftBusiness != value)
                {
                    this._isBusyAircraftBusiness = value;
                }
            }
        }

        /// <summary>
        /// 加载实体集合的方法
        /// </summary>
        private void LoadAircraftBusiness()
        {
            this.IsBusy = true;
            this._service.LoadAircraftBusiness(new QueryBuilder<AircraftBusiness>(), lo =>
            {
                this.IsBusyAircraftBusiness = false;
                this.IsBusy = this.IsBusyPlan || this.IsBusyOperationHistory || this.IsBusyAircraftBusiness || this.IsBusyApprovalHistory || this.IsBusyAircraft || this.IsBusyRequest;
                if (lo.Error != null)
                {
                    // 处理加载失败

                }
                else
                {

                    this.RaisePropertyChanged(() => this.ViewAircraftBusiness);
                }
            }, null);
        }

        #endregion

        #region 加载实体ApprovalHistory集合

        /// <summary>
        /// 获取批文历史，由于加载计划历史服务端Includ不到ApprovalHistory，故需要在前台加载ApprovalHistory，程序会自动完成匹配
        /// </summary>
        public IEnumerable<ApprovalHistory> ViewApprovalHistory
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<ApprovalHistory>();
            }
        }

        private bool _isBusyApprovalHistory = true;
        public bool IsBusyApprovalHistory
        {
            get { return this._isBusyApprovalHistory; }
            private set
            {
                if (this._isBusyApprovalHistory != value)
                {
                    this._isBusyApprovalHistory = value;
                }
            }
        }

        /// <summary>
        /// 加载实体集合的方法
        /// </summary>
        private void LoadApprovalHistory()
        {
            this.IsBusy = true;
            this._service.LoadApprovalHistory(new QueryBuilder<ApprovalHistory>(), lo =>
            {
                this.IsBusyApprovalHistory = false;
                this.IsBusy = this.IsBusyPlan || this.IsBusyOperationHistory || this.IsBusyAircraftBusiness || this.IsBusyApprovalHistory || this.IsBusyAircraft || this.IsBusyRequest;
                if (lo.Error != null)
                {
                    // 处理加载失败

                }
                else
                {

                    this.RaisePropertyChanged(() => this.ViewApprovalHistory);
                }
            }, null);
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

        #region ViewModel 属性 CurrentAircraft --选中的飞机

        private Aircraft _currentAircraft;
        /// <summary>
        /// 选中的飞机
        /// </summary>
        public Aircraft CurrentAircraft
        {
            get { return this._currentAircraft; }
            private set
            {
                if (this._currentAircraft != value)
                {
                    this._currentAircraft = value;
                    this.RaisePropertyChanged(() => this.CurrentAircraft);
                    this.RaisePropertyChanged(() => this.AllPlanHistories);
                }
            }
        }

        #endregion

        #region ViewModel 属性 AllPlanHistories --计划历史


        public IEnumerable<PlanHistory> AllPlanHistories
        {
            get
            {
                if (CurrentAircraft != null && CurrentAircraft.PlanAircrafts.Count > 0)
                {
                    var planAircraft = CurrentAircraft.PlanAircrafts;
                    if (planAircraft != null)
                        return planAircraft.SelectMany(p=>p.PlanHistories);
                }
                return null;
              
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
            if ((obj is Request) && (obj != null))
            {
                Request request = obj as Request;
                AttachmentOperation.OpenAttachment<Request>(request);
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
            if (menu != null && menu.Name == "AircraftExport" && aircraft != null)
            {
                aircraft.ElementExporting -= this.ElementExporting;
                aircraft.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGirdOperation.DowmLoadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        aircraft.Export(stream, ImageAndGirdOperation.SetGridViewExportOptions());
                    }
                }
            }
            else if (menu != null && menu.Name == "PlanHistoryExport" && planHistory != null)
            {
                planHistory.ElementExporting -= this.ElementExporting;
                planHistory.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGirdOperation.DowmLoadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        planHistory.Export(stream, ImageAndGirdOperation.SetGridViewExportOptions());
                    }
                }
            }
            else if (menu != null && menu.Name == "RequestHistoryExport" && requestHistory != null)
            {
                requestHistory.ElementExporting -= this.ElementExporting;
                requestHistory.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGirdOperation.DowmLoadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        requestHistory.Export(stream, ImageAndGirdOperation.SetGridViewExportOptions());
                    }
                }
            }
            else if (menu != null && menu.Name == "OwnershipHistoryExport" && ownershipHistory != null)
            {
                ownershipHistory.ElementExporting -= this.ElementExporting;
                ownershipHistory.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGirdOperation.DowmLoadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        ownershipHistory.Export(stream, ImageAndGirdOperation.SetGridViewExportOptions());
                    }
                }
            }
            else if (menu != null && menu.Name == "OperationHistoryExport" && operationHistory != null)
            {
                operationHistory.ElementExporting -= this.ElementExporting;
                operationHistory.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGirdOperation.DowmLoadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        operationHistory.Export(stream, ImageAndGirdOperation.SetGridViewExportOptions());
                    }
                }
            }
            else if (menu != null && menu.Name == "AircraftBusinessExport" && aircraftBusiness != null)
            {
                aircraftBusiness.ElementExporting -= this.ElementExporting;
                aircraftBusiness.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGirdOperation.DowmLoadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        aircraftBusiness.Export(stream, ImageAndGirdOperation.SetGridViewExportOptions());
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
        //    if (e.Element == ExportElement.HeaderCell || e.Element == ExportElement.GroupHeaderCell)
        //    {

        //        e.Width = 135;
        //        e.Height = 35;
        //        e.FontSize = 20;
        //        e.FontWeight = FontWeights.Bold;
        //    }
        //    else if (e.Element == ExportElement.Row)
        //    { i = 1; }
        //    else if (e.Element == ExportElement.Cell &&
        //        e.Value != null)
        //    {
        //        if (i == 5)
        //        {
        //            e.Value = DateTime.Parse(e.Value.ToString()).ToString("yyyy年M月");
        //        }
        //        e.Width = 135;
        //        e.Height = 30;
        //        i++;
        //    }

        //}
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
            //查看附件
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
            LoadAircraft();
            LoadAircraftBusiness();
            LoadApprovalHistory();
            LoadOperationHistory();
            LoadPlan();
            LoadRequest();

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
