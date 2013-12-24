
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Windows.Data.DomainServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using Telerik.Windows.Controls;
using UniCloud.Security.Models;
using UniCloud.Security.Services;
using UniCloud.Security.Services.Web;
using UniCloud.UniAuth.Views;
using Telerik.Windows.Controls.Navigation;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;
using MessageOperationLibrary.EventAggregatorRepository;
using MessageOperationLibrary.Events;

namespace UniCloud.UniAuth.ViewModels
{
    [Export(typeof(UserMaintainViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public class UserMaintainViewModel : NotificationObject, IPartImportsSatisfiedNotification, IConfirmNavigationRequest
    {
        public UserMaintainViewModel()
        {
            this._service.EntityContainer.PropertyChanged += new PropertyChangedEventHandler(EntityContainer_PropertyChanged);
            this._confirmExitInteractionRequest = new InteractionRequest<ConfirmViewModel>();
        }

        #region Local

        [Import]
        public IRegionManager regionManager;
        private readonly IAuthServices _service = ServiceLocator.Current.GetInstance<IAuthServices>();
        private readonly UserMaintainView _view = ServiceLocator.Current.GetInstance<UserMaintainView>();

        private readonly string defaultPassword = "123@abc";
        private RadGridView radUsersCollection;
        private Users usersToReset;
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
            this.AddCommand.RaiseCanExecuteChanged();
            this.RemoveCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// 以View的实例初始化ViewModel相关字段、属性
        /// </summary>
        private void ViewModelInitializer()
        {
            this.radUsersCollection = this._view.UsersCollection as RadGridView;
        }
        #endregion

        #endregion

        #region ViewModel

        #region ViewModel 加载实体集合 LoadUsers

        #region ViewModel 属性 UsersCollection -用户集合
        /// <summary>
        /// 用户集合
        /// </summary>
        public IEnumerable<Users> UsersCollection
        {
            get { return this._service.EntityContainer.GetEntitySet<Users>(); }
        }
        #endregion

        private bool isBusyUsers = true;
        public bool IsBusyUsers
        {
            get { return this.isBusyUsers; }
            private set
            {
                if (this.isBusyUsers != value)
                    this.isBusyUsers = value;
            }
        }

        #region ViewModel 方法 LoadUsers --加载所有用户

        public void LoadUsers()
        {
            this.IsBusy = true;
            this._service.LoadUsers(new QueryBuilder<Users>(), lo =>
                {
                    this.IsBusyUsers = false;
                    this.IsBusy = this.IsBusyUsers;
                    if (lo.Error != null)
                    {
                        //处理加载失败
                    }
                    else
                    {
                        this.SelectedUser = this.UsersCollection.OrderBy(n => n.UserId).FirstOrDefault();
                        this.RaisePropertyChanged(() => this.UsersCollection);
                        RefreshButtonState();
                    }
                }, null);
        }
        #endregion

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

        #region ViewModel 属性 SelectedUser --选中的用户

        private Users _selectedUser;
        public Users SelectedUser
        {
            get { return this._selectedUser; }
            set
            {

                if (this._selectedUser != value)
                {
                    _selectedUser = value;
                    this.RaisePropertyChanged(() => this.SelectedUser);
                    RefreshButtonState();
                }
            }
        }
        #endregion

        #region ViewModel 属性 ConfirmInteractionRequest --当页面发生修改，提醒用户是否保存

        private readonly InteractionRequest<ConfirmViewModel> _confirmExitInteractionRequest;
        /// <summary>
        /// 当页面发生修改，提醒用户是否保存
        /// </summary>
        public IInteractionRequest ConfirmExitInteractionRequest
        {
            get { return this._confirmExitInteractionRequest; }
        }
        #endregion
        #endregion

        #region Command

        #region ViewModel 命令 SaveCommand --保存

        public DelegateCommand<object> SaveCommand { get; private set; }
        private void OnSave(object obj)
        {
            foreach (var u in this.UsersCollection)
            {
                if (string.IsNullOrEmpty(u.UserName) || UsersCollection.Any(n => (n.UserId != u.UserId && n.UserName == u.UserName)))
                {
                    MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(new MessageEvent { Message = "用户名不能为空或者用户名已存在！", MessageType = MessageType.Info });
                    this.radUsersCollection.Items.MoveCurrentTo(u);
                    return;
                }
                else if (string.IsNullOrEmpty(u.Password))
                {
                    MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(new MessageEvent { Message = "用户密码不能为空！", MessageType = MessageType.Info });
                    this.radUsersCollection.Items.MoveCurrentTo(u);
                    return;
                }
            }
            //假设还有存在未编辑的项目，则在保存前取消编辑
            if (radUsersCollection.SelectedItems != null)
            {
                this.radUsersCollection.CancelEdit();
            }
            this._service.SubmitChanges(UniCloud.Infrastructure.CAFMStrings.SaveSuccess, UniCloud.Infrastructure.CAFMStrings.SaveFail, sm => { }, null);
        }
        private bool _canSave = false;
        public bool CanSave(object obj)
        {
            return _canSave;
        }
        #endregion

        #region ViewModel 命令 AbortCommand --放弃更改

        public DelegateCommand<object> AbortCommand { get; private set; }
        private void OnAbort(object obj)
        {
            this._service.RejectChanges();
        }
        private bool _canAbort = false;
        public bool CanAbort(object obj)
        {
            return _canAbort;
        }
        #endregion

        #region ViewModel 命令 AddCommand --增加新用户

        public DelegateCommand<object> AddCommand { get; private set; }
        private void OnAdd(object obj)
        {
            Users u = new Users();
            u.UserId = this._service.EntityContainer.GetEntitySet<Users>().Count == 0 ? 1 :
             this._service.EntityContainer.GetEntitySet<Users>().Max(p => p.UserId) + 1;
            u.Password = defaultPassword;
            u.IsAnonymous = false;
            u.IsApproved = true;
            u.IsLockedOut = true;
            u.CreateDate = DateTime.Now.Date;
            this._service.EntityContainer.GetEntitySet<Users>().Add(u);
            this.radUsersCollection.Items.MoveCurrentToLast();
            this.radUsersCollection.ScrollIntoView(this.SelectedUser);
            this.RaisePropertyChanged(() => this.UsersCollection);
            RefreshButtonState();
        }
        private bool _canAdd = true;
        public bool CanAdd(object obj)
        {
            return _canAdd;
        }
        #endregion

        #region ViewModel 命令 RemoveCommand --删除用户

        public DelegateCommand<object> RemoveCommand { get; private set; }
        private void OnRemove(object obj)
        {
            if (SelectedUser != null && SelectedUser.UserName == "admin")
            {
                MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(new MessageEvent { Message = "管理员账户不能删除！", MessageType = MessageType.Info });
                return;
            }
            if (SelectedUser != null)
            {
                this._service.EntityContainer.GetEntitySet<Users>().Remove(SelectedUser);
            }
            this.RaisePropertyChanged(() => this.UsersCollection);
            RefreshButtonState();
        }
        private bool _canRemove = true;
        public bool CanRemove(object obj)
        {
            if (this.SelectedUser == null)
            {
                return false;
            }
            else
            {
                return true;
            }

        }
        #endregion

        #region ViewModel 命令 ResetUserPasswordCommand --重置用户密码

        public DelegateCommand<object> ResetUserPasswordCommand { get; private set; }
        private void OnResetUserPassword(object obj)
        {
            usersToReset = obj as Users;
            OnConfirm(usersToReset.UserName);
        }
        private void OnConfirm(string text)
        {
            string header = "提示";
            string confirmtext;
            if (!string.IsNullOrEmpty(text))
            {
                confirmtext = "是否重置用户：\r\n" + text + "的密码？";
            }
            else
            {
                confirmtext = "是否重置用户密码？";
            }
            ConfirmRadWindow.Show(header, confirmtext, new EventHandler<WindowClosedEventArgs>(OnClosed));
            //ConfirmRadWindow.Show(header, confirmtext,source, new EventHandler<WindowClosedEventArgs>(OnClosed));

        }
        private void OnClosed(object sender, WindowClosedEventArgs e)
        {
            if (e.DialogResult == true)
            {
                usersToReset.Password = defaultPassword;
            }
        }
        private bool _canResetUserPassword = true;
        public bool CanResetUserPassword(object obj)
        {
            if (this.radUsersCollection.CurrentItem != null)
            {
                _canResetUserPassword = true;
            }
            else
            {
                _canResetUserPassword = false;
            }
            return _canResetUserPassword;
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
            this.AddCommand = new DelegateCommand<object>(this.OnAdd, this.CanAdd);
            this.RemoveCommand = new DelegateCommand<object>(this.OnRemove, this.CanRemove);
            this.ResetUserPasswordCommand = new DelegateCommand<object>(this.OnResetUserPassword, this.CanResetUserPassword);
        }

        #endregion

        #region 订阅事件处理

        private void SubscribeEvent()
        {

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
            LoadUsers();
        }

        #endregion

        #region IConfirmNavigationRequest Members

        public void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            if (this._service.EntityContainer.HasChanges)
            {
                this._confirmExitInteractionRequest.Raise(new ConfirmViewModel { ConfirmContent = UniCloud.Infrastructure.CAFMStrings.ConfirmNavigateAwayFromRequestDetail, ConfirmTitle = UniCloud.Infrastructure.CAFMStrings.ConfirmNavigateAwayFromRequestDetailTitle },
                    c =>
                    {
                        if (c.Result)
                        {
                            this._service.RejectChanges();
                            continuationCallback(true);
                        }
                        else
                        {
                            continuationCallback(false);
                            var navigation = new MessageOperationLibrary.Events.NavigationEvent(false);
                            MessageEventAggregatorRepository.EventAggregator.Publish<MessageOperationLibrary.Events.NavigationEvent>(navigation);
                        }
                    });
            }
            else
            {
                continuationCallback(true);
            }
        }

        #endregion
    }
}
