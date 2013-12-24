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
    [Export(typeof(CafmApprovalViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public class CafmApprovalViewModel : NotificationObject, IPartImportsSatisfiedNotification, IConfirmNavigationRequest
    {
        public CafmApprovalViewModel()
        {


            this._service.EntityContainer.PropertyChanged += new PropertyChangedEventHandler(EntityContainer_PropertyChanged);
        }

        #region Local

        [Import]
        public IRegionManager regionManager;
        private readonly IFleetService _service = ServiceLocator.Current.GetInstance<IFleetService>();
        private readonly CAAC.CAFM.Views.CafmApprovalView _view = ServiceLocator.Current.GetInstance<CAAC.CAFM.Views.CafmApprovalView>();
        private RadGridView gvRequestDetail, gvApprovedRequest, request, requestDetail;
        private string dropImpossibleReason;
        private bool _isAnnotation = true;//是否批准

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
        /// 加载实体集合的方法 --- 申请 Request
        /// </summary>
        private void LoadRequest()
        {
            this.IsBusy = true;
            this._service.LoadRequest(new QueryBuilder<Request>(), lo =>
            {
                LoadManaApprovalHistory();

            }, null);
        }


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
                    this.NewCommand.RaiseCanExecuteChanged();
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

        #region Method

        public void ContextMenuOpened(object sender, System.Windows.RoutedEventArgs e)
        {
            IsContextMenuOpen = true;
        }
        #endregion


        #endregion

        #region Command

        #region ViewModel 命令 -- 创建新批文

        public DelegateCommand<object> NewCommand { get; private set; }
        private void OnNew(object sender)
        {
            //foreach (var item in this._selRequest.ApprovalHistories)
            //{
            //    if (! (this._service.EntityContainer.GetEntitySet<ManaApprovalHistory>().Any(p =>
            //            p.ApprovalHistoryID == item.ApprovalHistoryID)))
            //    {
            //        ManaApprovalHistory doc = new ManaApprovalHistory();
            //        doc.ApprovalHistoryID = item.ApprovalHistoryID;
            //        doc.IsApproved = false;
            //        this._service.EntityContainer.GetEntitySet<ManaApprovalHistory>().Add(doc);
            //        this._canSave = true;
            //        this._canAbort = true;
            //    }
            //}

        }

        private bool _canNew = true;
        bool CanNew(object sender)
        {
            var q = this._service.EntityContainer.GetEntitySet<Request>().Where(r => r.Status == (int)ReqStatus.Submited);

            if (this._selRequest != null)
            {
                this._canNew = true;
            }
            else
            {
                this._canNew = false;
            }
            return this._canNew;
        }

        #endregion

        #region ViewModel 命令 -- 保存

        public DelegateCommand<object> SaveCommand { get; private set; }
        private void OnSave(object sender)
        {
            this._service.SubmitChanges(CAFMStrings.SaveSuccess, CAFMStrings.SaveFail, sm => { }, null);
        }
        private bool _canSave = false;
        bool CanSave(object sender)
        {
            return this._canSave;
        }

        #endregion

        #region ViewModel 命令 -- 放弃更改

        public DelegateCommand<object> AbortCommand { get; private set; }
        private void OnAbort(object sender)
        {
            this._service.RejectChanges();
            this.gvRequestDetail.GroupDescriptors.Reset();
            this.gvApprovedRequest.GroupDescriptors.Reset();
            RefreshButtonState();
        }
        private bool _canAbort = false;
        bool CanAbort(object sender)
        {
            return this._canAbort;
        }

        #endregion

        #region ViewModel 命令 -- 查看附件

        public DelegateCommand<object> ViewAttachCommand { get; private set; }
        private void OnViewAttach(object sender)
        {

            if (sender.GetType() == typeof(Request) && (sender as Request) != null)
            {
                AttachmentOperation.OpenAttachment<Request>((sender as Request));
            }
        }

        #endregion

        #region ViewModel 命令 ManageApprovedCommand --标记是否批准

        // 标记是否批准
        public DelegateCommand<object> ManageApprovedCommand { get; private set; }
        private void OnManageApproved(object obj)
        {
            if (_isAnnotation)
            {
                _isAnnotation = false;
                //this._service.UpdateXmlConfigContent(p => { }, null);
                this._service.SubmitChanges(sm =>
                {
                    if (sm.Error != null)
                    {
                        MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(new MessageEvent { Message = "批准失败,请检查！", MessageType = MessageType.Fail });
                    }
                    else if (!sm.Cancelled)
                    {
                        MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(new MessageEvent { Message = "批准成功！", MessageType = MessageType.Success });
                    }
                    _isAnnotation = true;
                }, null);
            }
        }
        private bool _canManageApproved = true;
        public bool CanManageApproved(object obj)
        {
            return _canManageApproved;
        }
        #endregion

        #region ViewModel 命令 ManageFlagCommand --审核标志

        // 标记是否批准
        public DelegateCommand<object> ManageFlagCommand { get; private set; }
        private void OnManageFlag(object obj)
        {
            if (_isAnnotation)
            {
                _isAnnotation = false;
                this._service.SubmitChanges(sm =>
                {
                    if (sm.Error != null)
                    {
                        MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(new MessageEvent { Message = "审核失败,请检查！", MessageType = MessageType.Fail });
                    }
                    else if (!sm.Cancelled)
                    {
                        MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(new MessageEvent { Message = "审核成功！", MessageType = MessageType.Success });
                    }
                    _isAnnotation = true;
                }, null);
            }
        }
        private bool _canManageFlag = true;
        public bool CanManageFlag(object obj)
        {
            return _canManageFlag;
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

        /// <summary>
        /// 跟踪实体变化，控制保存、放弃按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EntityContainer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "HasChanges")
            {
                this._canSave = this._service.EntityContainer.HasChanges;
                this._canAbort = this._service.EntityContainer.HasChanges;
                this.SaveCommand.RaiseCanExecuteChanged();
                this.AbortCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// 刷新按钮状态
        /// </summary>
        private void RefreshButtonState()
        {
        }



        #endregion

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
            SubscribeEvent();
            ViewModelInitializer();
            this.SaveCommand = new DelegateCommand<object>(this.OnSave, this.CanSave);
            this.AbortCommand = new DelegateCommand<object>(this.OnAbort, this.CanAbort);
            this.NewCommand = new DelegateCommand<object>(this.OnNew, this.CanNew);
            this.ViewAttachCommand = new DelegateCommand<object>(this.OnViewAttach);
            this.ManageApprovedCommand = new DelegateCommand<object>(this.OnManageApproved, this.CanManageApproved);
            this.ManageFlagCommand = new DelegateCommand<object>(this.OnManageFlag, this.CanManageFlag);
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
