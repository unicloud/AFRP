using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Windows.Data.DomainServices;
using UniCloud.AFRP.Operation;
using UniCloud.Fleet.Models;
using UniCloud.Fleet.Services;
using System.Linq;
using Telerik.Windows.Controls;
using System.Windows;
using System.IO;

namespace UniCloud.AFRP.ViewModels
{
    [Export(typeof(AfrpApprovalQueryViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public class AfrpApprovalQueryViewModel : NotificationObject, IPartImportsSatisfiedNotification, IConfirmNavigationRequest
    {
        public AfrpApprovalQueryViewModel()
        {
            //加载批文，查询申请需要关联到批文
            this._service.LoadApprovalDoc(new QueryBuilder<ApprovalDoc>(), lo => { }, null);

            //弹出窗口的设置
            radWindow.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            radWindow.Height = 300;
            radWindow.Width = 600;
            radWindow.ResizeMode = ResizeMode.NoResize;
            radWindow.Content = CreateGridView();
            //RadWindow 标题
            radWindow.Header = "运营历史";
        }

        #region Local

        [Import]
        public IRegionManager regionManager;
        private readonly IFleetService _service = ServiceLocator.Current.GetInstance<IFleetService>();
        private readonly UniCloud.AFRP.Views.AfrpApprovalQueryView _view = ServiceLocator.Current.GetInstance<UniCloud.AFRP.Views.AfrpApprovalQueryView>();
        //初始化RadWindow 需添加引用集 Telerik.Windows.Controls.Navigation
        private RadWindow radWindow = new RadWindow();
        private RadGridView request, requestDetail;
        private int i; //导出数据源格式判断

        #region Property



        #endregion

        #region Method

        #region Local 方法 CreateRadWindows -- 创建子窗体

        /// <summary>
        /// 初始化RadWindow控件的Content的值
        /// </summary>
        /// <returns></returns>
        public RadGridView CreateGridView()
        {

            var gridview = new RadGridView();
            gridview.ShowGroupPanel = false;
            gridview.AutoGenerateColumns = false;
            gridview.CanUserFreezeColumns = false;
            gridview.CanUserResizeRows = false;
            gridview.IsReadOnly = true;
            gridview.RowIndicatorVisibility = Visibility.Collapsed;
            gridview.IsFilteringAllowed = true;
            var gv1 = new GridViewDataColumn
            {
                Header = "航空公司",
                DataMemberBinding =
                    new System.Windows.Data.Binding("Airlines.ShortName")
            };

            var gv2 = new GridViewDataColumn
            {
                Header = "飞机号",
                DataMemberBinding =
                    new System.Windows.Data.Binding("Aircraft.RegNumber")
            };

            var gv3 = new GridViewDataColumn
            {
                Header = "机型",
                DataMemberBinding =
                    new System.Windows.Data.Binding("Aircraft.AircraftType.Name")
            };

            var gv4 = new GridViewDataColumn { Header = "技术接收日期" };
            var bing4 = new System.Windows.Data.Binding("TechReceiptDate")
            {
                StringFormat
                    = "yyyy/M/d"
            };
            gv4.DataMemberBinding = bing4;

            var gv5 = new GridViewDataColumn { Header = "接收日期" };
            var bing5 = new System.Windows.Data.Binding("ReceiptDate") { StringFormat = "yyyy/M/d" };
            gv5.DataMemberBinding = bing5;

            var gv6 = new GridViewDataColumn { Header = "运营日期" };
            var bing6 = new System.Windows.Data.Binding("StartDate") { StringFormat = "yyyy/M/d" };
            gv6.DataMemberBinding = bing6;

            var gv7 = new GridViewDataColumn { Header = "退出停场日期" };
            var bing7 = new System.Windows.Data.Binding("StopDate") { StringFormat = "yyyy/M/d" };
            gv7.DataMemberBinding = bing7;

            var gv8 = new GridViewDataColumn { Header = "技术交付日期" };
            var bing8 = new System.Windows.Data.Binding("TechDeliveryDate") { StringFormat = "yyyy/M/d" };
            gv8.DataMemberBinding = bing8;

            var gv9 = new GridViewDataColumn { Header = "退出日期" };
            var bing9 = new System.Windows.Data.Binding("EndDate") { StringFormat = "yyyy/M/d" };
            gv9.DataMemberBinding = bing9;

            var gv10 = new GridViewDataColumn { Header = "起租日期" };
            var bing10 = new System.Windows.Data.Binding("OnHireDate") { StringFormat = "yyyy/M/d" };
            gv10.DataMemberBinding = bing10;

            var gv11 = new GridViewDataColumn
            {
                Header = "引进方式",
                DataMemberBinding =
                    new System.Windows.Data.Binding("ImportCategory.ActionName")
            };

            var gv12 = new GridViewDataColumn
            {
                Header = "退出方式",
                DataMemberBinding =
                    new System.Windows.Data.Binding("ExportCategory.ActionName")
            };

            var gv13 = new GridViewDataColumn
            {
                Header = "备注",
                DataMemberBinding = new System.Windows.Data.Binding("Note")
            };


            gridview.Columns.Add(gv1);
            gridview.Columns.Add(gv2);
            gridview.Columns.Add(gv3);
            gridview.Columns.Add(gv4);
            gridview.Columns.Add(gv5);
            gridview.Columns.Add(gv6);
            gridview.Columns.Add(gv7);
            gridview.Columns.Add(gv8);
            gridview.Columns.Add(gv9);
            gridview.Columns.Add(gv10);
            gridview.Columns.Add(gv11);
            gridview.Columns.Add(gv12);
            gridview.Columns.Add(gv13);
            return gridview;

        }
        #endregion

        /// <summary>
        /// 以View的实例初始化ViewModel相关字段、属性
        /// </summary>
        private void ViewModelInitializer()
        {
            this.request = this._view.request as RadGridView;
            this.requestDetail = this._view.requestDetail as RadGridView;
        }

        #endregion

        #endregion

        #region ViewModel

        #region 加载实体集合

        /// <summary>
        /// 用于绑定的集合，根据实际需要修改
        /// </summary>
        public IEnumerable<Request> ViewRequest
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<Request>().Where(p => p.ApprovalDoc != null);
            }
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
        /// 加载实体集合的方法
        /// </summary>
        private void LoadRequest()
        {
            this.IsBusy = true;
            this._service.LoadRequest(new QueryBuilder<Request>(), lo =>
            {
                this.IsBusyRequest = false;
                this.IsBusy = this.IsBusyRequest;
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
            var req = sender as Request;
            if (req != null)
            {
                AttachmentOperation.OpenAttachment<Request>(req);
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

        public bool CanViewRequstOperation(object obj)
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
            if (menu != null && menu.Name == "RequestDetailExport" && requestDetail != null)
            {
                requestDetail.ElementExporting -= this.ElementExporting;
                requestDetail.ElementExporting += new EventHandler<GridViewElementExportingEventArgs>(ElementExporting);
                using (Stream stream = ImageAndGridOperation.DownloadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        requestDetail.Export(stream, ImageAndGridOperation.SetGridViewExportOptions());
                    }
                }
            }
            else if (menu != null && menu.Name == "RequestExport" && request != null)
            {
                using (Stream stream = ImageAndGridOperation.DownloadDialogStream("文档文件(*.xls)|*.xls|文档文件(*.doc)|*.doc"))
                {
                    if (stream != null)
                    {
                        request.Export(stream, ImageAndGridOperation.SetGridViewExportOptions());
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
