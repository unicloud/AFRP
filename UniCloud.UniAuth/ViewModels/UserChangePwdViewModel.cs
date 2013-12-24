
using MessageOperationLibrary.EventAggregatorRepository;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Windows.Data.DomainServices;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Telerik.Windows.Controls.Data.DataForm;
using UniCloud.Security.Models;
using UniCloud.Security.Services;
using UniCloud.UniAuth.Views;

namespace UniCloud.UniAuth.ViewModels
{
    [Export(typeof(UserChangePwdViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public class UserChangePwdViewModel : NotificationObject, IPartImportsSatisfiedNotification, IConfirmNavigationRequest
    {
        public UserChangePwdViewModel()
        {
            this._service.EntityContainer.PropertyChanged += new PropertyChangedEventHandler(EntityContainer_PropertyChanged);
            this.confirmExitInteractionRequest = new InteractionRequest<ConfirmViewModel>();
        }

        #region Local

        [Import]
        public IRegionManager regionManager;
        private readonly IAuthServices _service = ServiceLocator.Current.GetInstance<IAuthServices>();
        private readonly UserChangePwdView _view = ServiceLocator.Current.GetInstance<UserChangePwdView>();
        private PasswordBox NewPasswordBox, ConfirmPasswordBox; //密码控件

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
            this.NewPasswordBox = this._view.NewPassword;
            this.ConfirmPasswordBox = this._view.ConfirmPassword;
            this._view.PasswordGrid.DataContext = this.PasswordClass;
        }

        #endregion

        #endregion

        #region ViewModel

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

        //#region ViewModel 属性 NewPassword -- 新密码

        //private string _newPassword = string.Empty;
        ///// <summary>
        ///// 新密码
        ///// </summary>
        //public string NewPassword
        //{
        //    get { return this._newPassword; }
        //    set
        //    {

        //        if (this._newPassword != value)
        //        {

        //            _newPassword = value;
        //            this.RaisePropertyChanged(() => this.NewPassword);

        //        }
        //    }
        //}
        //#endregion

        //#region ViewModel 属性 ConfirmPassword --确认密码

        //private string _confirmPassword = string.Empty;
        ///// <summary>
        ///// 确认密码
        ///// </summary>

        //public string ConfirmPassword
        //{
        //    get { return this._confirmPassword; }
        //    set
        //    {

        //        if (this._confirmPassword != value)
        //        {
        //            if (_newPassword != value)
        //            {
        //                throw new Exception("重复密码必须与新密码输入一致!");
        //            }
        //            _confirmPassword = value;
        //            this.RaisePropertyChanged(() => this.ConfirmPassword);

        //        }
        //    }
        //}
        //#endregion

        #region ViewModel 属性 NewPassword -- 密码类绑定

        private Password _passwordClass = new Password() { NewPassword = string.Empty, ConfirmPassword = string.Empty };
        /// <summary>
        /// 新密码
        /// </summary>
        public Password PasswordClass
        {
            get { return this._passwordClass; }
            set
            {

                if (this._passwordClass != value)
                {
                    _passwordClass = value;
                    this.RaisePropertyChanged(() => this.PasswordClass);
                }
            }
        }
        #endregion

        #region ViewModel 方法 LoadCurrentUser --加载当前用户信息

        #region ViewModel 属性 CurrentUser --当前用户

        public Users CurrentUser
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<Users>().Where(p => p.UserName == WebContext.Current.User.Name).FirstOrDefault();
            }
        }

        #endregion

        private bool _isBusyCurrentUser = true;
        public bool IsBusyCurrentUser
        {
            get { return this._isBusyCurrentUser; }
            private set
            {
                if (this._isBusyCurrentUser != value)
                {
                    this._isBusyCurrentUser = value;
                }
            }
        }

        /// <summary>
        /// 方法的说明
        /// </summary>
        public void LoadCurrentUser()
        {
            this.IsBusy = true;
            this._service.LoadUsers(new QueryBuilder<Users>(), lo =>
            {
                this.IsBusyCurrentUser = false;
                this.IsBusy = this.IsBusyCurrentUser;
                if (lo.Error != null)
                { }
                else
                {
                    this.RaisePropertyChanged(() => this.CurrentUser);
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

        #region ViewModel 属性 当页面发生修改，提醒用户是否保存

        private readonly InteractionRequest<ConfirmViewModel> confirmExitInteractionRequest;

        public IInteractionRequest ConfirmExitInteractionRequest
        {
            get { return this.confirmExitInteractionRequest; }
        }

        #endregion

        #endregion

        #region Command


        #region ViewModel 命令 -- 保存

        public DelegateCommand<object> SaveCommand { get; private set; }
        private void OnSave(object sender)
        {
            NewPasswordBox.Password = NewPasswordBox.Password + "1";
            NewPasswordBox.Password = NewPasswordBox.Password.Substring(0, NewPasswordBox.Password.Length - 1);
            ConfirmPasswordBox.Password = ConfirmPasswordBox.Password + "1";
            ConfirmPasswordBox.Password = ConfirmPasswordBox.Password.Substring(0, ConfirmPasswordBox.Password.Length - 1);
            if (this.PasswordClass.NewPassword == string.Empty)
            {
                this.NewPasswordBox.Focus();
                return;
            }
            else if (this.PasswordClass.NewPassword != this.PasswordClass.ConfirmPassword)
            {
                this.ConfirmPasswordBox.Focus();
                return;
            }
            this.CurrentUser.Password = this.PasswordClass.NewPassword;
            this._service.SubmitChanges(UniCloud.Infrastructure.CAFMStrings.SaveSuccess, UniCloud.Infrastructure.CAFMStrings.SaveFail, sm => {
                if (sm.Error == null)
                {
                    var navigation = new MessageOperationLibrary.Events.NavigationEvent();
                    MessageEventAggregatorRepository.EventAggregator.Publish<MessageOperationLibrary.Events.NavigationEvent>(navigation);
                }
            }, null);


        }
        private bool _canSave = true;
        bool CanSave(object sender)
        {
            return this._canSave;
        }
        #endregion

        #region ViewModel 命令 -- 放弃更改

        public DelegateCommand<object> AbortCommand { get; private set; }
        private void OnAbort(object sender)
        {
            var navigation = new MessageOperationLibrary.Events.NavigationEvent();
            MessageEventAggregatorRepository.EventAggregator.Publish<MessageOperationLibrary.Events.NavigationEvent>(navigation);
        }
        private bool _canAbort = true;
        bool CanAbort(object sender)
        {
            return this._canAbort;
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// 自动生成登录框DataForm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UpdateForm_AutoGeneratingField(object sender, AutoGeneratingFieldEventArgs e)
        {
            //if (e.PropertyName == "UserName")
            //{
            //    this.userNameTextBox = (TextBox)e.Field.Content;
            //}
            //else if (e.PropertyName == "Password")
            //{
            //    PasswordBox passwordBox = new PasswordBox();
            //    e.Field.ReplaceTextBox(passwordBox, PasswordBox.PasswordProperty);
            //    this.LogInfo.PasswordAccessor = () => passwordBox.Password;
            //}
        }

        /// <summary>
        /// 把用户输入框设置为焦点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UpdateForm_ContentLoaded(object sender, RoutedEventArgs e)
        {
            //if (!isDfFocused)
            //{
            //    this.userNameTextBox.Focus();
            //    this.isDfFocused = true;
            //}
        }

        /// <summary>
        /// 将 Esc 映射到取消按钮，将 Enter 映射到确定按钮。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UpdateForm_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //if (e.Key == Key.Escape)
            //{
            //    this.OnLoginCancel(sender);
            //}
            //else if (e.Key == Key.Enter && !this.LogInfo.IsLoggingIn)
            //{
            //    this.OnLoginOK(sender);
            //}
        }

        #endregion

        #region Class

        public class Password : INotifyPropertyChanged
        {
            private bool _isValidate = true;
            /// <summary>
            /// 控制是否启动验证
            /// </summary>
            public bool IsValidate
            {
                get { return this._isValidate; }
                set { _isValidate = value; }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            /// <summary>
            /// 属性改变时触发事件
            /// </summary>
            /// <param name="propertyName">Property that changed.</param>
            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (null != handler)
                {
                    handler.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }
            }

            private string _newPassword = string.Empty;
            /// <summary>
            /// 新密码
            /// </summary>
            public string NewPassword
            {
                get { return this._newPassword; }
                set
                {
                    if (this._newPassword != value)
                    {
                        _newPassword = value;
                        OnPropertyChanged("NewPassword");
                        if (value == string.Empty && _isValidate)
                        {
                            throw new Exception("新密码不能为空!");
                        }
                    }
                }
            }

            private string _confirmPassword = string.Empty;
            /// <summary>
            /// 确认密码
            /// </summary>
            public string ConfirmPassword
            {
                get { return this._confirmPassword; }
                set
                {
                    _confirmPassword = value;
                    OnPropertyChanged("ConfirmPassword");
                    if (NewPassword != value)
                    {
                        throw new Exception("重复密码与新密码输入不一致!");
                    }
                }
            }
        }

        #endregion

        #endregion

        #region IPartImportsSatisfiedNotification Members

        public void OnImportsSatisfied()
        {
            SubscribeEvent();
            ViewModelInitializer();

            this.SaveCommand = new DelegateCommand<object>(OnSave, CanSave);
            this.AbortCommand = new DelegateCommand<object>(OnAbort, CanAbort);
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
            LoadCurrentUser();

            //取消第二次进入此界面时的为空验证
            if (this.PasswordClass != null)
            {
                this.NewPasswordBox.Password = "1";
                this.ConfirmPasswordBox.Password = "1";
                this.PasswordClass.IsValidate = false;
                this.NewPasswordBox.Password = string.Empty;
                this.ConfirmPasswordBox.Password = string.Empty;
                this.PasswordClass.IsValidate = true;
            }
        }

        #endregion

        #region IConfirmNavigationRequest Members

        public void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            //if (this._service.EntityContainer.HasChanges)
            //{
            //    this.confirmExitInteractionRequest.Raise(
            //        new ConfirmViewModel
            //        {
            //            ConfirmContent = UniCloud.Infrastructure.CAFMStrings.ConfirmNavigateAwayFromRequestDetail,
            //            ConfirmTitle = UniCloud.Infrastructure.CAFMStrings.ConfirmNavigateAwayFromRequestDetailTitle
            //        },
            //        c =>
            //        {
            //            if (c.Result)
            //            {
            //                this._service.RejectChanges();
            //                continuationCallback(true);
            //            }
            //            else continuationCallback(false);
            //        }
            //        );
            //}
            //else continuationCallback(true);
            continuationCallback(true);
        }

        #endregion

    }

}
