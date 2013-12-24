using System.ComponentModel.Composition;
using MessageOperationLibrary.EventAggregatorRepository;
using MessageOperationLibrary.Events;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.ViewModel;
using CAAC.Fleet.Services;
using UniCloud.Fleet.Models;
using System.ComponentModel;
using System.Collections.Generic;
using System;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Windows.Data.DomainServices;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using CAAC.Infrastructure;

namespace CAAC.CAFM.ViewModels
{
    [Export(typeof(CafmMailSettingViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public class CafmMailSettingViewModel : NotificationObject, IPartImportsSatisfiedNotification, IConfirmNavigationRequest
    {
        public CafmMailSettingViewModel()
        {
            this._service.EntityContainer.PropertyChanged += new PropertyChangedEventHandler(EntityContainer_PropertyChanged);
        }

        #region Local

        [Import]
        public IRegionManager regionManager;
        private readonly IFleetService _service = ServiceLocator.Current.GetInstance<IFleetService>();
        private bool _canSave = false;
        private bool _canAbort = false;

        #region Property

        #region ViewModel Account Name 属性

        public string AccountName
        {
            get
            {
                return _CurrentMailAddress == null ? "" : _CurrentMailAddress.LoginUser;
            }
            private set
            {
                if (_CurrentMailAddress != null && this._CurrentMailAddress.LoginUser != value)
                {
                    this._CurrentMailAddress.LoginUser = value;
                    this.RaisePropertyChanged(() => this.AccountName);
                }
            }
        }
        #endregion

        #region ViewModel Account Password 属性

        public string AccountPassword
        {
            get { return this._CurrentMailAddress == null ? "" : _CurrentMailAddress.OriginPassword; }
            private set
            {
                if (_CurrentMailAddress != null && this._CurrentMailAddress.OriginPassword != value)
                {
                    this._CurrentMailAddress.OriginPassword = value;
                    this.RaisePropertyChanged(() => this.AccountPassword);
                }
            }
        }


        #endregion

        #region ViewModel SmtpServer 属性

        public string SmtpHost
        {
            get { return _CurrentMailAddress == null ? "" : _CurrentMailAddress.SmtpHost; }
            private set
            {
                if (_CurrentMailAddress != null && this._CurrentMailAddress.SmtpHost != value)
                {
                    this._CurrentMailAddress.SmtpHost = value;
                    this.RaisePropertyChanged(() => this.SmtpHost);
                }
            }
        }

        #endregion

        #region ViewModel Smtp Port 属性

        public int SmtpPort
        {
            get { return _CurrentMailAddress == null ? 25 : _CurrentMailAddress.SendPort; }
            private set
            {
                if (_CurrentMailAddress != null && this._CurrentMailAddress.SendPort != value)
                {
                    this._CurrentMailAddress.SendPort = value;
                    this.RaisePropertyChanged(() => this.SmtpPort);
                }
            }
        }

        #endregion

        #region ViewModel Pop3 Server 属性

        public string Pop3Host
        {
            get { return _CurrentMailAddress == null ? "" : _CurrentMailAddress.Pop3Host; }
            private set
            {
                if (_CurrentMailAddress != null && this._CurrentMailAddress.Pop3Host != value)
                {
                    this._CurrentMailAddress.Pop3Host = value;
                    this.RaisePropertyChanged(() => this.Pop3Host);
                }
            }
        }

        #endregion

        #region ViewModel Pop3 Port 属性

        public int Pop3Port
        {
            get { return _CurrentMailAddress == null ? 110 : _CurrentMailAddress.ReceivePort; }
            private set
            {
                if (_CurrentMailAddress != null && this._CurrentMailAddress.ReceivePort != value)
                {
                    this._CurrentMailAddress.ReceivePort = value;
                    this.RaisePropertyChanged(() => this.Pop3Port);
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
            this.SaveCommand.RaiseCanExecuteChanged();
            this.AbortCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// 以View的实例初始化ViewModel相关字段、属性
        /// </summary>
        private void ViewModelInitializer()
        {

        }

        #endregion

        #endregion

        #region ViewModel

        #region 加载实体集合

        /// <summary>
        /// 用于绑定的集合，根据实际需要修改
        /// </summary>
        public IEnumerable<MailAddress> ViewMailAddress
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<MailAddress>();
            }
        }

        private MailAddress _CurrentMailAddress = null;

        private bool _isBusyMailAddress;
        public bool IsBusyMailAddress
        {
            get { return this._isBusyMailAddress; }
            private set
            {
                if (this._isBusyMailAddress != value)
                {
                    this._isBusyMailAddress = value;
                }
            }
        }


        private void NotifyPropertyChanged()
        {
            string[] AllProperty = new string[] { "AccountName", "AccountPassword", "SmtpHost", "SmtpPort", "Pop3Host", "Pop3Port" };

            this.RaisePropertyChanged(AllProperty);
        }
        /// <summary>
        /// 加载实体集合的方法
        /// </summary>
        private void LoadMailAddress()
        {
            this.IsBusy = true;
            this._service.LoadMailAddress(new QueryBuilder<MailAddress>(), lo =>
            {
                this.IsBusyMailAddress = false;
                this.IsBusy = this.IsBusyMailAddress;
                if (lo.Error != null)
                {
                    // 处理加载失败

                }
                else
                {

                    //民航局邮件地址
                    _CurrentMailAddress = this._service.EntityContainer.GetEntitySet<MailAddress>().Where(q => q.OwnerID == Guid.Parse(this._service.CAACID)).FirstOrDefault();
                    if (_CurrentMailAddress == null)
                    {
                        _CurrentMailAddress = new MailAddress();
                        _CurrentMailAddress.OwnerID = Guid.Parse(this._service.CAACID);
                        _CurrentMailAddress.MailAddressID = Guid.NewGuid();
                        _CurrentMailAddress.SendPort = 25;
                        _CurrentMailAddress.ReceivePort = 110;
                        this._service.EntityContainer.GetEntitySet<MailAddress>().Add(_CurrentMailAddress);
                    }

                    NotifyPropertyChanged();
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


        #endregion

        #region Command

        #region ViewModel 命令 -- 保存

        public DelegateCommand<object> SaveCommand { get; private set; }
        private bool StringEmpty(string str)
        {
            return str == null || str.Trim() == "";
        }
        private void OnSave(object sender)
        {
            if (this._CurrentMailAddress == null)
            {
                return;
            }
            if (StringEmpty(this.AccountName))
            {
                MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(new MessageEvent { Message = "邮件账号不能为空！", MessageType = MessageType.Info });
                return;
            }
            if (StringEmpty(this.AccountPassword))
            {
                MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(new MessageEvent { Message = "邮件密码不能为空！", MessageType = MessageType.Info });
                return;
            }
            if (StringEmpty(this.SmtpHost))
            {
                MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(new MessageEvent { Message = "发送服务器不能为空！", MessageType = MessageType.Info });
                return;
            }
            if (this.SmtpPort == 0)
            {
                MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(new MessageEvent { Message = "发送端口不能为零！", MessageType = MessageType.Info });
                return;
            }

            if (StringEmpty(this.Pop3Host))
            {
                MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(new MessageEvent { Message = "接收服务器不能为空！", MessageType = MessageType.Info });
                return;
            }

            if (this.Pop3Port == 0)
            {
                MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(new MessageEvent { Message = "接收端口不能为零！", MessageType = MessageType.Info });
                return;
            }

            this._service.SubmitChanges(CAFMStrings.SaveSuccess, CAFMStrings.SaveFail, sm => { }, null);
        }

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
            RefreshButtonState();
        }


        bool CanAbort(object sender)
        {
            return this._canAbort;
        }

        #endregion

        #endregion

        #region Methods

        #endregion

        #endregion

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
            SubscribeEvent();
            ViewModelInitializer();

            this.SaveCommand = new DelegateCommand<object>(this.OnSave, this.CanSave);
            this.AbortCommand = new DelegateCommand<object>(this.OnAbort, this.CanAbort);

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
            this.LoadMailAddress();
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
