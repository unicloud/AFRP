using CAAC.Fleet.Services;
using CAAC.Infrastructure;
using MessageOperationLibrary.EventAggregatorRepository;
using MessageOperationLibrary.Events;
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
    [Export(typeof(CafmApprovalCheckViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public class CafmApprovalCheckViewModel : NotificationObject, IPartImportsSatisfiedNotification, IConfirmNavigationRequest
    {
        public CafmApprovalCheckViewModel()
        {
            this._service.EntityContainer.PropertyChanged += new PropertyChangedEventHandler(EntityContainer_PropertyChanged);
        }

        #region Local

        [Import]
        public IRegionManager regionManager;
        private readonly IFleetService _service = ServiceLocator.Current.GetInstance<IFleetService>();
        private readonly CAAC.CAFM.Views.CafmApprovalCheckView _view = ServiceLocator.Current.GetInstance<CAAC.CAFM.Views.CafmApprovalCheckView>();
        private bool _isAnnotation = true;//是否批准
        private RadGridView request, requestDetail;

        #region Property


        #region 加载实体集合

        /// <summary>
        /// 用于绑定的集合，根据实际需要修改
        /// </summary>
        public IEnumerable<Request> ViewRequest
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<Request>().Where(p => p.IsFinished != true);
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
        /// 加载实体集合的方法 申请
        /// </summary>
        private void LoadRequest()
        {
            this.IsBusy = true;
            this._service.LoadRequest(new QueryBuilder<Request>(), lo =>
            {
                LoadApprovalDoc();
                LoadManaApprovalHistory();
            }, null);
        }

        /// <summary>
        /// 加载实体集合的方法 批文
        /// </summary>
        private void LoadApprovalDoc()
        {
            this._service.LoadApprovalDoc(new QueryBuilder<ApprovalDoc>(), lo =>
            {
                ;
            }, null);
        }

        //加载管理的批文
        private void LoadManaApprovalHistory()
        {
            this._service.LoadManaApprovalHistory(new QueryBuilder<ManaApprovalHistory>(), lo =>
            {
                this.IsBusyRequest = false;
                this.IsBusy = this.IsBusyRequest;
                if (lo.Error != null)
                {
                    // 处理加载失败

                }
                else
                {
                    // this.NewCommand.RaiseCanExecuteChanged();
                    this.RaisePropertyChanged(() => this.ViewRequest);
                }
            }, null);
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
            this.request = this._view.request as RadGridView;
            this.requestDetail = this._view.requestDetail as RadGridView;
        }

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

        #region ViewModel 命令 ManageFlagPlanCommand --标记是否批准

        // 标记是否批准
        public DelegateCommand<object> ManageApprovedCommand { get; private set; }
        private void OnManageApproved(object obj)
        {
            if (_isAnnotation)
            {
                _isAnnotation = false;
                this._service.SubmitChanges(sm =>
                {
                    if (sm.Error != null)
                    {
                        MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(new MessageEvent { Message = "批准失败,请检查！", MessageType = MessageType.Fail });
                    }
                    else if (!sm.Cancelled)
                    {
                        MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(new MessageEvent { Message = "批准成功！", MessageType = MessageType.Success });
                        //刷新批准状态数据
                      //  RefreshApprovalStateView();

                    }
                    _isAnnotation = true;
                }, null);
            }
        }

        //刷新批准状态数据
        private void RefreshApprovalStateView()
        {
            CAAC.CAFM.Views.CafmApprovalCheckView _view = ServiceLocator.Current.GetInstance<CAAC.CAFM.Views.CafmApprovalCheckView>();
            //RadGridView request = _view.request as RadGridView;
            Request lastSelect = this._selRequest;
            this.RaisePropertyChanged(() => this.ViewRequest);
            this.SelRequest = lastSelect;
        }

        private bool _canManageApproved = true;
        public bool CanManageApproved(object obj)
        {
            return _canManageApproved;
        }
        #endregion

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

        #region ViewModel 命令 --导出数据

        public DelegateCommand<object> ExportGridViewCommand { get; private set; }

        private void OnExportGridView(object sender)
        {
            RadMenuItem menu = sender as RadMenuItem;
            IsContextMenuOpen = false;
            if (menu != null && menu.Name == "RequestExport" && request != null)
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
            this.ManageApprovedCommand = new DelegateCommand<object>(this.OnManageApproved, this.CanManageApproved);
            this.ViewAttachCommand = new DelegateCommand<object>(this.OnViewAttach);
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