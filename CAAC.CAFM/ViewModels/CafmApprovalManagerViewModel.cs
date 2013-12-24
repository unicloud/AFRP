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
using System.Windows;
using Telerik.Windows.Controls;
using UniCloud.Fleet.Models;



namespace CAAC.CAFM.ViewModels
{
    [Export(typeof(CafmApprovalManagerViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public class CafmApprovalManagerViewModel : NotificationObject, IPartImportsSatisfiedNotification, IConfirmNavigationRequest
    {
        public CafmApprovalManagerViewModel()
        {


            this._service.EntityContainer.PropertyChanged += new PropertyChangedEventHandler(EntityContainer_PropertyChanged);
            //加载批文，查询申请需要关联到批文
            this._service.LoadApprovalDoc(new QueryBuilder<ApprovalDoc>(), lo => { }, null);

            //弹出窗口的设置
            radWindow.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            radWindow.Height = 200;
            radWindow.Width = 551;
            radWindow.ResizeMode = ResizeMode.NoResize;
            radWindow.Content = CreateGridView();
            //RadWindow 标题
            radWindow.Header = "运营历史";
        }

        #region Local

        [Import]
        public IRegionManager regionManager;
        private readonly IFleetService _service = ServiceLocator.Current.GetInstance<IFleetService>();
        private readonly CAAC.CAFM.Views.CafmApprovalManageView _view = ServiceLocator.Current.GetInstance<CAAC.CAFM.Views.CafmApprovalManageView>();
        //初始化RadWindow 需添加引用集 Telerik.Windows.Controls.Navigation
        private RadWindow radWindow = new RadWindow();
        private RadGridView approval, request, requestDetail;

        #region Property

        #region 加载实体集合 ----批文

        /// <summary>
        /// 用于绑定的集合，根据实际需要修改
        /// </summary>
        public IEnumerable<ApprovalDoc> ViewApprovalDoc
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<ApprovalDoc>();
            }
        }

        private bool _isBusyApprovalDoc;

        public bool IsBusyApprovalDoc
        {
            get { return this._isBusyApprovalDoc; }
            private set
            {
                if (this._isBusyApprovalDoc != value)
                {
                    this._isBusyApprovalDoc = value;
                }
            }
        }

        /// <summary>
        /// 加载批文实体集合的方法
        /// </summary>
        private void LoadApprovalDoc()
        {
            this.IsBusy = true;
            this._service.LoadApprovalDoc(new QueryBuilder<ApprovalDoc>(), lo =>
            {
                this.IsBusyApprovalDoc = false;
                this.IsBusy = this.IsBusyApprovalDoc || this.IsBusyRequest;
                if (lo.Error != null)
                {
                    // 处理加载失败

                }
                else
                {

                    this.RaisePropertyChanged(() => this.ViewApprovalDoc);
                }
            }, null);
        }

        private bool _isBusyRequest;
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
        /// 加载申请实体集合的方法
        /// </summary>
        private void LoadRequest()
        {
            this.IsBusy = true;
            this._service.LoadRequest(new QueryBuilder<Request>(), lo =>
            {
                this.IsBusyRequest = false;
                this.IsBusy = this.IsBusyRequest || this.IsBusyApprovalDoc;
                if (lo.Error != null)
                {
                    // 处理加载失败
                }
                else
                {
                    this.RaisePropertyChanged(() => this.ViewApprovalDoc);
                }
            }, null);
        }

        #endregion

        #region ViewModel 属性 ---选中的申请

        private Request _selRequest;
        public Request SelRequest
        {
            get { return this._selRequest; }
            private set
            {
                if (this._selRequest != value)
                {
                    this._selRequest = value;
                    this.RaisePropertyChanged(() => this.SelRequest);
                }
            }
        }


        #endregion

        #region ViewModel 属性  --- 选中的批文

        private ApprovalDoc _selApproval;
        public ApprovalDoc SelApproval
        {
            get { return this._selApproval; }
            private set
            {
                if (this._selApproval != value)
                {
                    this._selApproval = value;
                    this.RaisePropertyChanged(() => this.SelApproval);
                }
            }
        }

        #endregion

        #endregion

        #region Method

        /// <summary>
        /// 跟踪实体变化，控制保存、放弃按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EntityContainer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //if (e.PropertyName == "HasChanges")
            //{
            //    this._canSave = this._service.EntityContainer.HasChanges;
            //    this._canAbort = this._service.EntityContainer.HasChanges;
            //    this.SaveCommand.RaiseCanExecuteChanged();
            //    this.AbortCommand.RaiseCanExecuteChanged();
            //}
        }


        /// <summary>
        /// 以View的实例初始化ViewModel相关字段、属性
        /// </summary>
        private void ViewModelInitializer()
        {
            this.approval = this._view.approval as RadGridView;
            this.request = this._view.request as RadGridView;
            this.requestDetail = this._view.requestDetail as RadGridView;
        }

        #region Local 方法 CreateRadWindows -- 创建子窗体

        /// <summary>
        /// 初始化RadWindow控件的Content的值
        /// </summary>
        /// <returns></returns>
        public RadGridView CreateGridView()
        {

            RadGridView gridview = new RadGridView();
            gridview.ShowGroupPanel = false;
            gridview.AutoGenerateColumns = false;
            gridview.CanUserFreezeColumns = false;
            gridview.CanUserResizeRows = false;
            gridview.IsReadOnly = true;
            gridview.IsFilteringAllowed = false;
            gridview.RowIndicatorVisibility = Visibility.Collapsed;

            GridViewDataColumn gv1 = new GridViewDataColumn();
            gv1.Header = "运营开始";
            System.Windows.Data.Binding bing1 = new System.Windows.Data.Binding("StartDate");
            bing1.StringFormat = "yyyy/M/d";
            gv1.DataMemberBinding = bing1;

            GridViewDataColumn gv2 = new GridViewDataColumn();
            gv2.Header = "运营结束";
            System.Windows.Data.Binding bing2 = new System.Windows.Data.Binding("StartDate");
            bing2.StringFormat = "yyyy/M/d";
            gv2.DataMemberBinding = bing2;

            GridViewDataColumn gv3 = new GridViewDataColumn();
            gv3.Header = "航空公司";
            gv3.DataMemberBinding = new System.Windows.Data.Binding("Airlines.ShortName");

            GridViewDataColumn gv4 = new GridViewDataColumn();
            gv4.Header = "飞机号";
            gv4.DataMemberBinding = new System.Windows.Data.Binding("Aircraft.RegNumber");

            GridViewDataColumn gv5 = new GridViewDataColumn();
            gv5.Header = "机型";
            gv5.DataMemberBinding = new System.Windows.Data.Binding("Aircraft.AircraftType.Name");

            GridViewDataColumn gv6 = new GridViewDataColumn();
            gv6.Header = "引进方式";
            gv6.DataMemberBinding = new System.Windows.Data.Binding("ImportCategory.ActionName");

            GridViewDataColumn gv7 = new GridViewDataColumn();
            gv7.Header = "退出方式";
            gv7.DataMemberBinding = new System.Windows.Data.Binding("ExportCategory.ActionName");

            GridViewDataColumn gv8 = new GridViewDataColumn();
            gv8.Header = "备注";
            gv8.DataMemberBinding = new System.Windows.Data.Binding("Note");

            gridview.Columns.Add(gv1);
            gridview.Columns.Add(gv2);
            gridview.Columns.Add(gv3);
            gridview.Columns.Add(gv4);
            gridview.Columns.Add(gv5);
            gridview.Columns.Add(gv6);
            gridview.Columns.Add(gv7);
            gridview.Columns.Add(gv8);
            return gridview;

        }
        #endregion


        #endregion

        #endregion

        #region ViewModel


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

        #region ViewModel 命令 -- 查看附件

        public DelegateCommand<object> ViewAttachCommand { get; private set; }
        private void OnViewAttach(object sender)
        {

            if (sender.GetType() == typeof(Request))
            {
                AttachmentOperation.OpenAttachment<Request>((sender as Request));
            }
            else
            {
                AttachmentOperation.OpenAttachment<ApprovalDoc>((sender as ApprovalDoc));

            }
        }

        #endregion

        #region ViewModel 命令 ViewRequstOperationCommand --飞机运营情况

        // 飞机运营情况
        public DelegateCommand<object> ViewRequstOperationCommand { get; private set; }
        private void OnViewRequstOperation(object obj)
        {
            if (obj != null)
            {
                var approvalHistory = obj as ApprovalHistory;
                //获取子窗体的GridView的集合
                List<OperationHistory> OpertationHistories = new List<OperationHistory>();
                if (approvalHistory.OperationHistory != null)
                {
                    OpertationHistories.Add(approvalHistory.OperationHistory);
                }
                (radWindow.Content as RadGridView).ItemsSource = OpertationHistories;
                if (!radWindow.IsOpen)
                {
                    new CommonMethod().ShowRadWindow(radWindow);
                }

            }
        }
        private bool _canViewRequstOperation = true;
        public bool CanViewRequstOperation(object obj)
        {
            return _canViewRequstOperation;
        }
        #endregion

        #region ViewModel 命令 --导出数据

        public DelegateCommand<object> ExportGridViewCommand { get; private set; }

        private void OnExportGridView(object sender)
        {
            RadMenuItem menu = sender as RadMenuItem;
            IsContextMenuOpen = false;
            if (menu != null && menu.Name == "ApprovalExport" && approval != null)
            {
                approval.ElementExporting -= this.ElementExporting;
                approval.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGirdOperation.DowmLoadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        approval.Export(stream, ImageAndGirdOperation.SetGridViewExportOptions());
                    }
                }
            }
            else if (menu != null && menu.Name == "RequestExport" && request != null)
            {
                request.ElementExporting -= this.ElementExporting;
                request.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGirdOperation.DowmLoadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        request.Export(stream, ImageAndGirdOperation.SetGridViewExportOptions());
                    }
                }
            }
            else if (menu != null && menu.Name == "RequestDetailExport" && requestDetail != null)
            {
                requestDetail.ElementExporting -= this.ElementExporting;
                requestDetail.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGirdOperation.DowmLoadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        requestDetail.Export(stream, ImageAndGirdOperation.SetGridViewExportOptions());
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
            this.ViewAttachCommand = new DelegateCommand<object>(this.OnViewAttach);
            this.ViewRequstOperationCommand = new DelegateCommand<object>(this.OnViewRequstOperation, this.CanViewRequstOperation);
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
            this.LoadApprovalDoc();
            this.LoadRequest();
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




//[Export(typeof(CafmApprovalManagerViewModel))]
//[PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.NonShared)]
//public class CafmApprovalManagerViewModel : NotificationObject, IPartImportsSatisfiedNotification, IConfirmNavigationRequest
//{
//    public CafmApprovalManagerViewModel()
//    {
//        #region QueryableDomainServiceCollectionView 构造

//        EntityQuery<ApprovalDoc> queryApprovalDoc = this._service.Context.GetApprovalDocQuery();
//        this._viewApprovalDoc = new QueryableDomainServiceCollectionView<ApprovalDoc>(this._service.Context, queryApprovalDoc);
//        this._viewApprovalDoc.LoadingData += new EventHandler<Telerik.Windows.Controls.DomainServices.LoadingDataEventArgs>(LoadingApprovalDoc);
//        this._viewApprovalDoc.LoadedData += new EventHandler<Telerik.Windows.Controls.DomainServices.LoadedDataEventArgs>(LoadedApprovalDoc);

//        #endregion
//    }

//    #region Local

//    [Import]
//    public IRegionManager regionManager;
//    private readonly IFleetService _service = ServiceLocator.Current.GetInstance<IFleetService>();
//    private bool _needRefresh;

//    #region Property



//    #endregion

//    #region Method



//    #endregion

//    #endregion

//    #region ViewModel

//    #region Property

//    #region QueryableDomainServiceCollectionView 属性

//    private readonly QueryableDomainServiceCollectionView<ApprovalDoc> _viewApprovalDoc;
//    private bool _loadApprovalDoc;

//    private bool _isBusyApprovalDoc;
//    public bool IsBusyApprovalDoc
//    {
//        get { return this._isBusyApprovalDoc; }
//        private set
//        {
//            if (this._isBusyApprovalDoc != value)
//            {
//                this._isBusyApprovalDoc = value;
//                this.RaisePropertyChanged(() => this.IsBusyApprovalDoc);
//            }
//        }
//    }

//    public QueryableDomainServiceCollectionView<ApprovalDoc> ViewApprovalDoc
//    {
//        get { return this._viewApprovalDoc; }
//    }

//    private void LoadingApprovalDoc(object sender, Telerik.Windows.Controls.DomainServices.LoadingDataEventArgs e)
//    {
//        this.IsBusyApprovalDoc = true;

//    }

//    private void LoadedApprovalDoc(object sender, Telerik.Windows.Controls.DomainServices.LoadedDataEventArgs e)
//    {
//        this.IsBusyApprovalDoc = false;

//        if (e.HasError)
//        {
//            e.MarkErrorAsHandled();
//        }
//        else
//        {
//            if (!this._loadApprovalDoc)
//            {

//                this._loadApprovalDoc = true;
//            }
//        }
//    }

//    #endregion

//    #region ViewModel 属性 ---选中的申请

//    private Request _selRequest;
//    public Request SelRequest
//    {
//        get { return this._selRequest; }
//        private set
//        {
//            if (this._selRequest != value)
//            {
//                this._selRequest = value;
//                this.RaisePropertyChanged(() => this.SelRequest);
//            }
//        }
//    }


//    #endregion

//    #region ViewModel 属性  --- 选中的批文

//    private ApprovalDoc _selApproval;
//    public ApprovalDoc SelApproval
//    {
//        get { return this._selApproval; }
//        private set
//        {
//            if (this._selApproval != value)
//            {
//                this._selApproval = value;
//                this.RaisePropertyChanged(() => this.SelApproval);
//            }
//        }
//    }

//    #endregion

//    #endregion

//    #region Command



//    #endregion

//    #region Methods


//    #endregion

//    #endregion

//    #region IPartImportsSatisfiedNotification Members

//    public void OnImportsSatisfied()
//    {
//        SubscribeEvent();

//    }

//    #endregion

//    #region 订阅事件处理

//    private void SubscribeEvent()
//    {
//        EventAggregatorRepository.EventAggregator.GetEvent<SetRefreshEvent>().Subscribe(this.OnRefresh, ThreadOption.UIThread);
//    }

//    private void OnRefresh(bool needRefresh)
//    {
//        this._needRefresh = needRefresh;
//    }

//    #endregion

//    #region INavigationAware Members

//    public bool IsNavigationTarget(NavigationContext navigationContext)
//    {
//        return true;
//    }

//    public void OnNavigatedFrom(NavigationContext navigationContext)
//    {

//    }

//    public void OnNavigatedTo(NavigationContext navigationContext)
//    {
//        if (!_loadApprovalDoc)
//        {
//            this._viewApprovalDoc.AutoLoad = true;
//        }
//        else if (this._needRefresh)
//        {
//            this._viewApprovalDoc.Refresh();
//            this._needRefresh = false;
//        }
//    }

//    #endregion

//    #region IConfirmNavigationRequest Members

//    public void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
//    {
//        continuationCallback(true);
//    }

//    #endregion
//}