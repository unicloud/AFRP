using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.InteropServices.Automation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Linq;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Windows.Data.DomainServices;
using Ria.Common;
using Telerik.Windows.Controls.Charting;
using Telerik.Windows.Data;
using System.ServiceModel.DomainServices.Client;
using System.Collections;
using Microsoft.Practices.Prism.ViewModel;
using UniCloud.AFRP.Operation;
using UniCloud.Fleet.Services;
using UniCloud.Fleet.Models;
using Telerik.Windows.Controls;

namespace UniCloud.AFRP.ViewModels
{
    [Export(typeof(AfrpFleetQueryViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public class AfrpFleetQueryViewModel : NotificationObject, IPartImportsSatisfiedNotification, IConfirmNavigationRequest
    {
        public AfrpFleetQueryViewModel()
        {
            //this._service.EntityContainer.PropertyChanged += new PropertyChangedEventHandler(EntityContainer_PropertyChanged);
            //弹出窗口的设置
            radWindow.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            radWindow.Height = 250;
            radWindow.Width = 500;
            radWindow.ResizeMode = ResizeMode.NoResize;
        }

        #region Local

        [Import]
        public IRegionManager regionManager;
        private readonly IFleetService _service = ServiceLocator.Current.GetInstance<IFleetService>();
        private readonly UniCloud.AFRP.Views.AfrpFleetQueryView _view = ServiceLocator.Current.GetInstance<UniCloud.AFRP.Views.AfrpFleetQueryView>();
        private RadGridView aircraft, planHistory, requestHistory, ownershipHistory, operationHistory, aircraftBusiness;
        private int i; //导出数据源格式判断
        private RadWindow radWindow = new RadWindow(); //子窗体，用来显示分公司运营权历史
     
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

        #region Local 方法 CreateRadWindows -- 创建子窗体

        /// <summary>
        /// 初始化RadWindow控件的Content的值
        /// </summary>
        /// <param name="obj">RadGridView控件的数据集合</param>
        /// <returns></returns>
        public Grid CreateRadWindows(IEnumerable<SubOperationHistory> obj)
        {
            Grid grid = new Grid();
            RadGridView gridview = new RadGridView();
            gridview.ShowGroupPanel = true;
            gridview.AutoGenerateColumns = false;
            gridview.CanUserFreezeColumns = false;
            gridview.CanUserResizeRows = false;
            gridview.IsReadOnly = true;
            gridview.IsFilteringAllowed = true;
            gridview.RowIndicatorVisibility = Visibility.Collapsed;

            gridview.ItemsSource = obj;
            GridViewDataColumn gv1 = new GridViewDataColumn();
            gv1.Header = "分公司名称";
            System.Windows.Data.Binding bing1 = new System.Windows.Data.Binding("Airlines.Name");
            gv1.DataMemberBinding = bing1;

            GridViewDataColumn gv2 = new GridViewDataColumn();
            gv2.Header = "运营日期";
            System.Windows.Data.Binding bing2 = new System.Windows.Data.Binding("StartDate");
            bing2.StringFormat = "yyyy/M/d";
            gv2.DataMemberBinding = bing2;

            GridViewDataColumn gv3 = new GridViewDataColumn();
            gv3.Header = "退出日期";
            System.Windows.Data.Binding bing3 = new System.Windows.Data.Binding("EndDate");
            bing3.StringFormat = "yyyy/M/d";
            gv3.DataMemberBinding = bing3;

            GridViewDataColumn gv4 = new GridViewDataColumn();
            gv4.Header = "创建日期";
            System.Windows.Data.Binding bing4 = new System.Windows.Data.Binding("Airlines.CreateDate");
            bing4.StringFormat = "yyyy/M/d";
            gv4.DataMemberBinding = bing4;

            GridViewDataColumn gv5 = new GridViewDataColumn();
            gv5.Header = "注销日期";
            System.Windows.Data.Binding bing5 = new System.Windows.Data.Binding("Airlines.LogoutDate");
            bing5.StringFormat = "yyyy/M/d";
            gv5.DataMemberBinding = bing5;


            gridview.Columns.Add(gv1);
            gridview.Columns.Add(gv2);
            gridview.Columns.Add(gv3);
            gridview.Columns.Add(gv4);
            gridview.Columns.Add(gv5);
            grid.Children.Add(gridview);
            return grid;

        }
        #endregion


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

        /// <summary>
        /// 加载实体集合的方法
        /// </summary>
        private void LoadAircraft()
        {
            this._service.LoadAircraft(new QueryBuilder<Aircraft>(), lo =>
            {
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

        /// <summary>
        /// 加载实体集合的方法
        /// </summary>
        private void LoadPlan()
        {
            this._service.LoadPlan(new QueryBuilder<Plan>(), lo =>
            {
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

        /// <summary>
        /// 加载实体集合的方法
        /// </summary>
        private void LoadRequest()
        {
            this._service.LoadRequest(new QueryBuilder<Request>(), lo =>
            {
                if (lo.Error != null)
                {
                    // 处理加载失败

                }
                else
                {
                    this.RaisePropertyChanged(() => this.ViewRequest);
                    this.RaisePropertyChanged(() => this.ViewApprovalHistory);
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

        /// <summary>
        /// 加载实体集合的方法
        /// </summary>
        private void LoadOperationHistory()
        {
            this._service.LoadOperationHistory(new QueryBuilder<OperationHistory>(), lo =>
            {
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

        /// <summary>
        /// 加载实体集合的方法
        /// </summary>
        private void LoadAircraftBusiness()
        {
            this._service.LoadAircraftBusiness(new QueryBuilder<AircraftBusiness>(), lo =>
            {
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

        #region 获取实体集合 ApprovalHistory

        /// <summary>
        /// 用于绑定的集合，根据实际需要修改
        /// </summary>
        public IEnumerable<ApprovalHistory> ViewApprovalHistory
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<ApprovalHistory>();
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
                    if (value != null && value.OperationHistories.Any(p => p.SubOperationHistories.Any()))
                    {
                        this.IsExistSubOperation = true;
                    }
                    else
                    {
                        this.IsExistSubOperation = false;
                    }
                    this.RaisePropertyChanged(() => this.CurrentAircraft);
                    this.RaisePropertyChanged(() => this.AllPlanHistories);
                    this.RaisePropertyChanged(() => this.AllApprovalHistories);
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

        #region ViewModel 属性 IsBusy

        private bool _isExistSubOperation = false;
        public bool IsExistSubOperation
        {
            get { return _isExistSubOperation; }
            private set
            {
                if (this._isExistSubOperation != value)
                {
                    this._isExistSubOperation = value;
                    this.RaisePropertyChanged(() => this.IsExistSubOperation);
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
                    var planAircraft = CurrentAircraft.PlanAircrafts.FirstOrDefault(p => p.IsOwn);
                    if (planAircraft != null)
                        return planAircraft.PlanHistories;
                }
                return null;


            }
        }
        #endregion

        #region ViewModel 属性 AllPlanHistories --批文历史


        public IEnumerable<ApprovalHistory> AllApprovalHistories
        {

            get
            {
                if (CurrentAircraft != null && CurrentAircraft.PlanAircrafts.Count > 0)
                {
                    var planAircraft = CurrentAircraft.PlanAircrafts.FirstOrDefault(p => p.IsOwn);
                    if (planAircraft != null)
                        return planAircraft.ApprovalHistories;
                }
                return null;


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
            if ((obj != null) && (obj is ApprovalHistory))
            {
                Request request = (obj as ApprovalHistory).Request;
                AttachmentOperation.OpenAttachment<Request>(request);
            }
            else if (obj != null && obj is PlanHistory)
            {
                Plan plan = (obj as PlanHistory).Plan;
                AttachmentOperation.OpenAttachment<Plan>(plan);
            }
        }

        public bool CanViewAttachment(object obj)
        {
            return true;
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
                using (Stream stream = ImageAndGridOperation.DownloadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        aircraft.Export(stream, ImageAndGridOperation.SetGridViewExportOptions());
                    }
                }
            }
            else if (menu != null && menu.Name == "PlanHistoryExport" && planHistory != null)
            {
                planHistory.ElementExporting -= this.ElementExporting;
                planHistory.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGridOperation.DownloadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        planHistory.Export(stream, ImageAndGridOperation.SetGridViewExportOptions());
                    }
                }
            }
            else if (menu != null && menu.Name == "RequestHistoryExport" && requestHistory != null)
            {
                requestHistory.ElementExporting -= this.ElementExporting;
                requestHistory.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGridOperation.DownloadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        requestHistory.Export(stream, ImageAndGridOperation.SetGridViewExportOptions());
                    }
                }
            }
            else if (menu != null && menu.Name == "OwnershipHistoryExport" && ownershipHistory != null)
            {
                ownershipHistory.ElementExporting -= this.ElementExporting;
                ownershipHistory.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGridOperation.DownloadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        ownershipHistory.Export(stream, ImageAndGridOperation.SetGridViewExportOptions());
                    }
                }
            }
            else if (menu != null && menu.Name == "OperationHistoryExport" && operationHistory != null)
            {
                operationHistory.ElementExporting -= this.ElementExporting;
                operationHistory.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGridOperation.DownloadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        operationHistory.Export(stream, ImageAndGridOperation.SetGridViewExportOptions());
                    }
                }
            }
            else if (menu != null && menu.Name == "AircraftBusinessExport" && aircraftBusiness != null)
            {
                aircraftBusiness.ElementExporting -= this.ElementExporting;
                aircraftBusiness.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGridOperation.DownloadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        aircraftBusiness.Export(stream, ImageAndGridOperation.SetGridViewExportOptions());
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

        bool CanExportGridView(object sender)
        {
            return true;
        }

        #endregion

        #region ViewModel 命令 --打开子窗体分公司运营历史

        public DelegateCommand<object> ViewSubOperationHistoryCommand { get; private set; }

        private void OnViewSubOperationHistory(object sender)
        {
            var operationHistory = sender as OperationHistory;
            if (operationHistory != null && operationHistory.SubOperationHistories.Any())
            {
                //RadWindow 标题
                radWindow.Header = "分公司运营历史";
                //RadWindow 内容
                radWindow.Content = CreateRadWindows(operationHistory.SubOperationHistories);
                //RadWindow单例
                if (!radWindow.IsOpen)
                {
                    new CommonMethod().ShowRadWindow(radWindow);
                }
            }
        }

        bool CanViewSubOperationHistory(object sender)
        {
            return true;
        }

        #endregion

        #endregion

        #region Methods

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
            this.ViewSubOperationHistoryCommand = new DelegateCommand<object>(this.OnViewSubOperationHistory, this.CanViewSubOperationHistory);//导出GridView数据

        }

        #endregion

        #region 订阅事件处理

        private void SubscribeEvent()
        {
            //EventAggregatorRepository.EventAggregator.GetEvent<IsLoadingEvent<CafmFleetQueryViewModel>>().Subscribe(this.OnLoad, ThreadOption.UIThread);
        }

        private void OnLoad(bool isLoading)
        {
            this.IsBusy = true;
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
            LoadOperationHistory();
            LoadPlan();
            LoadRequest();
        }

        #endregion

        #region IConfirmNavigationRequest Members

        public void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            if (radWindow.IsOpen)
            {
                radWindow.Close();
            }
            continuationCallback(true);
        }

        #endregion
    }

}
