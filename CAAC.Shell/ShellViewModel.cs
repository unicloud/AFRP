
using CAAC.Infrastructure;
using MessageOperationLibrary.ApplicationModel.Eventing;
using MessageOperationLibrary.EventAggregatorRepository;
using MessageOperationLibrary.Events;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.ServiceModel.DomainServices.Client.ApplicationServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UniCloud.Security.Models;
using UniCloud.Security.Services;
using UniCloud.Security.Services.Web;
using WebContext = UniCloud.Security.Services.WebContext;

namespace CAAC.Shell
{
    [Export(typeof(ShellViewModel))]
    public class ShellViewModel : NotificationObject, IPartImportsSatisfiedNotification
    {
        public ShellViewModel()
        {
            if (Application.Current.IsRunningOutOfBrowser)
            {
                maximized = false;
                this.IsMaximize = false;
            }
            else
                this.IsOOB = false;
        }

        #region Local

        [Import]
        public IRegionManager regionManager;
        [Import]
        public IModuleManager moduleManager;
        private IRegionNavigationJournal navigationJournal;
        private readonly IAuthServices _service = ServiceLocator.Current.GetInstance<IAuthServices>();

        private TextBox userNameTextBox;
        private Applications curApp;
        private bool isDfFocused;
        private string lastFuncName = "应用功能";
        #region Property

        #endregion

        #region Method

        /// <summary>
        /// <see cref="LoginOperation"/> 的完成处理程序。
        /// 如果操作成功，则关闭窗口。
        /// 如果发生错误，则显示 <see cref="ErrorWindow"/> 并将错误标记为已处理。
        /// 如果未取消操作但是登录失败，则一定是因为凭据不正确，因此添加验证错误以通知用户。
        /// </summary>
        private void LoginOperation_Completed(LoginOperation loginOperation)
        {
            if (loginOperation.LoginSuccess)
            {
                // 设置当前用户
                var user = this._service.EntityContainer.GetEntitySet<Users>().SingleOrDefault(u => u.UserName.ToLower() == WebContext.Current.User.Name.ToLower());
                // 获取应用列表
                this.AppList = this._service.EntityContainer.GetEntitySet<Applications>()
                    .Where(a => a.ApplicationName != "UniCloud" && a.ApplicationName != "AFRP").ToList();
                // 设置当前用户应用与功能的可用性
                this.AppList.ForEach(a =>
                {
                    var userAppRoles = user.UserInRoles.Select(ur => ur.Roles).Where(r => r.Application == a);
                    a.IsValid = userAppRoles.Any();
                    a.FunctionItems.Where(fi => fi.IsLeaf).ToList().ForEach(f =>
                        f.IsValid = userAppRoles.SelectMany(r => r.FunctionsInRoles)
                        .Where(fr => fr.FunctionItemID == f.FunctionItemID && fr.IsValid).Any());
                });

                this.IsLogined = true;
            }
            else if (loginOperation.HasError)
            {
                ErrorWindow.CreateNew(loginOperation.Error);
                loginOperation.MarkErrorAsHandled();
            }
            else if (!loginOperation.IsCanceled)
            {
                this.LogInfo.ValidationErrors.Add(new ValidationResult(ErrorResources.ErrorBadUserNameOrPassword, new string[] { "UserName", "Password" }));
            }
        }

        /// <summary>
        /// 应用模块加载完成后的操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void moduleManager_LoadModuleCompleted(object sender, LoadModuleCompletedEventArgs e)
        {
            this.IsFuncEnabled = true;
        }

        /// <summary>
        /// 应用模块加载进程处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void moduleManager_ModuleDownloadProgressChanged(object sender, ModuleDownloadProgressChangedEventArgs e)
        {
            this.IsFuncEnabled = false;
        }

        /// <summary>
        /// 设置应用
        /// </summary>
        /// <param name="app">选择的应用</param>
        private void SetApp(Applications app)
        {
            if (this.curApp != app)
            {
                this.curApp = app;
                moduleManager.LoadModule(app.ModuleName);
                this.AppName = app.Description;
                this.OnHome(null);
                SetFunc(app);

                // 设置当前应用
                this._service.Context.SetAppName(AppConfig.unicloud, app.ApplicationName,
                    invokeOp =>
                    {
                        if (invokeOp.HasError)
                        {
                            //处理调用错误
                            ErrorWindow.CreateNew(invokeOp.Error);
                            invokeOp.MarkErrorAsHandled();
                        }
                        else
                        {
                            WebContext.Current.Authentication.LoadUser(lo =>
                            {
                                if (lo.HasError)
                                {
                                    ErrorWindow.CreateNew(lo.Error);
                                    lo.MarkErrorAsHandled();
                                }
                            }, null);
                        }
                    }, null);
            }
        }

        /// <summary>
        /// 设置当前功能列表
        /// </summary>
        /// <param name="role">当前角色</param>
        private void SetFunc(Applications app)
        {
            this.FuncList = app.FunctionItems.SingleOrDefault(f => f.ParentItemID == null).SubItems.ToList();
        }

        #endregion

        #endregion

        #region ViewModel

        #region Property

        #region 应用架构

        #region ViewModel 属性 -- 应用列表

        private List<Applications> _appList;
        /// <summary>
        /// 应用列表
        /// </summary>
        public List<Applications> AppList
        {
            get { return this._appList; }
            set
            {
                if (this._appList != value)
                {
                    _appList = value;
                    this.RaisePropertyChanged(() => this.AppList);
                }
            }
        }

        #endregion

        #region ViewModel 属性 -- 当前应用功能列表

        private List<FunctionItem> _funcList;
        /// <summary>
        /// 当前应用功能列表
        /// </summary>
        public List<FunctionItem> FuncList
        {
            get { return this._funcList; }
            set
            {
                if (this._funcList != value)
                {
                    _funcList = value;
                    this.RaisePropertyChanged(() => this.FuncList);
                }
            }
        }

        #endregion

        #region ViewModel 属性 -- 当前应用名称

        private string _appName = "应用角色";
        /// <summary>
        /// 当前应用名称
        /// </summary>
        public string AppName
        {
            get { return this._appName; }
            set
            {
                if (this._appName != value)
                {
                    _appName = value;
                    this.RaisePropertyChanged(() => this.AppName);
                }
            }
        }

        #endregion

        #region ViewModel 属性 -- 当前功能名称

        private string _funcName = "应用功能";
        /// <summary>
        /// 当前功能名称
        /// </summary>
        public string FuncName
        {
            get { return this._funcName; }
            set
            {
                if (this._funcName != value)
                {
                    _funcName = value;
                    this.RaisePropertyChanged(() => this.FuncName);
                }
            }
        }

        #endregion

        #region ViewModel 属性 --登录实体

        private LoginInfo _logInfo = new LoginInfo();
        /// <summary>
        /// 登录实体
        /// </summary>
        public LoginInfo LogInfo
        {
            get { return this._logInfo; }
            set
            {
                if (this._logInfo != value)
                {
                    _logInfo = value;
                    this.RaisePropertyChanged(() => this.LogInfo);
                }
            }
        }

        #endregion

        #region ViewModel 属性 -- 是否已登录

        private bool _isLogined;
        /// <summary>
        /// 是否已登录
        /// </summary>
        public bool IsLogined
        {
            get { return this._isLogined; }
            set
            {
                if (this._isLogined != value)
                {
                    _isLogined = value;
                    this.RaisePropertyChanged(() => this.IsLogined);
                }
            }
        }

        #endregion

        #endregion

        #region ViewModel 属性 -- 应用窗口是否打开

        private bool _isAppDrop;
        /// <summary>
        /// 应用窗口是否打开
        /// </summary>
        public bool IsAppDrop
        {
            get { return this._isAppDrop; }
            set
            {
                if (this._isAppDrop != value)
                {
                    _isAppDrop = value;
                    this.RaisePropertyChanged(() => this.IsAppDrop);
                }
            }
        }

        #endregion

        #region ViewModel 属性 -- 功能窗口是否打开

        private bool _isFuncDrop;
        /// <summary>
        /// 功能窗口是否打开
        /// </summary>
        public bool IsFuncDrop
        {
            get { return this._isFuncDrop; }
            set
            {
                if (this._isFuncDrop != value)
                {
                    _isFuncDrop = value;
                    this.RaisePropertyChanged(() => this.IsFuncDrop);
                }
            }
        }

        #endregion

        #region ViewModel 属性 -- 功能菜单是否可用

        private bool _isFuncEnabled;
        /// <summary>
        /// 功能菜单是否可用
        /// </summary>
        public bool IsFuncEnabled
        {
            get { return this._isFuncEnabled; }
            set
            {
                if (this._isFuncEnabled != value)
                {
                    _isFuncEnabled = value;
                    this.RaisePropertyChanged(() => this.IsFuncEnabled);
                }
            }
        }

        #endregion

        #region ViewModel 属性 IsMenuEnabled

        private bool _isMenuenabled = false;
        public bool IsMenuEnabled
        {
            get { return _isMenuenabled; }
            set
            {
                if (this._isMenuenabled != value)
                {
                    this._isMenuenabled = value;
                    RaisePropertyChanged(() => this.IsMenuEnabled);
                }
            }
        }

        #endregion

        #region ViewModel 属性 IsOOB

        private bool _isOOB;
        public bool IsOOB
        {
            get { return this._isOOB; }
            private set
            {
                if (this._isOOB != value)
                {
                    this._isOOB = value;
                    this.RaisePropertyChanged(() => this.IsOOB);
                }
            }
        }

        #endregion

        #region ViewModel 属性 IsMaximize

        private bool _isMaximize;
        public bool IsMaximize
        {
            get { return this._isMaximize; }
            private set
            {
                if (this._isMaximize != value)
                {
                    this._isMaximize = value;
                    this.RaisePropertyChanged(() => this.IsMaximize);
                }
            }
        }

        #endregion

        #region ViewModel 属性 OprationMsg

        private string _oprationMsg;
        public string OprationMsg
        {
            get { return this._oprationMsg; }
            private set
            {
                if (this._oprationMsg != value)
                {
                    this._oprationMsg = value;
                    this.RaisePropertyChanged(() => this.OprationMsg);
                }
            }
        }

        #endregion

        #region ViewModel 属性 MessageType

        private int _messageType;
        public int MessageType
        {
            get { return this._messageType; }
            private set
            {
                this._messageType = value;
                this.RaisePropertyChanged(() => this.MessageType);
            }
        }

        #endregion

        #region ViewModel 属性 VersionUpdateMessage

        private string _versionUpdateMessage;
        public string VersionUpdateMessage
        {
            get {    return this._versionUpdateMessage; }
            private set
            {
                this._versionUpdateMessage = value;
                this.RaisePropertyChanged(() => this.VersionUpdateMessage);
            }
        }

        #endregion


        #endregion

        #region Command

        #region 导航操作

        #region ViewModel 命令 HomeCommand -- 主页

        public DelegateCommand<object> HomeCommand { get; private set; }
        private void OnHome(object obj)
        {
            this.FuncName = "应用功能";
            Uri uri = new Uri("HomeView", UriKind.Relative);
            this.regionManager.RequestNavigate(RegionNames.MainRegion, uri);
        }

        #endregion

        #region ViewModel 命令 NavigateCommand -- 导航

        public DelegateCommand<object> NavigateCommand { get; private set; }
        private void OnNavigate(object obj)
        {
            this.IsFuncDrop = false;
            var curFunc = obj as FunctionItem;
            this.FuncName = this._service.EntityContainer.GetEntitySet<FunctionItem>()
                .Single(f => f.FunctionItemID == curFunc.ParentItemID).Name + "：" + curFunc.Name;

            var targetView = curApp.ViewNameSpace + curFunc.ViewName;
            Uri uri = new Uri(targetView, UriKind.Relative);
            if (this.regionManager.Regions[RegionNames.MainRegion].ActiveViews.FirstOrDefault().ToString() != targetView)
            {
                this.regionManager.RequestNavigate(RegionNames.MainRegion, uri);
            }
        }

        #endregion

        #endregion

        #region 窗口操作

        private bool maximized;

        #region ViewModel 命令 MinimizeCommand -- 最小化窗口

        public DelegateCommand<object> MinimizeCommand { get; private set; }
        private void OnMinimize(object obj)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        #endregion

        #region ViewModel 命令 MaxmizeCommand -- 最大化窗口

        public DelegateCommand<object> MaxmizeCommand { get; private set; }
        private void OnMaxmize(object obj)
        {
            if (!maximized)
            {
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
                maximized = true;
                this.IsMaximize = true;
            }
            else
            {
                Application.Current.MainWindow.WindowState = WindowState.Normal;
                maximized = false;
                this.IsMaximize = false;
            }
        }

        #endregion

        #region ViewModel 命令 CloseWndCommand -- 关闭窗口

        public DelegateCommand<object> CloseWndCommand { get; private set; }
        private void OnCloseWnd(object obj)
        {
            Application.Current.MainWindow.Close();
        }

        #endregion

        #region ViewModel 命令 MoveWndCommand -- 移动窗口

        public DelegateCommand<object> MoveWndCommand { get; private set; }
        private void OnMoveWnd(object obj)
        {
            Application.Current.MainWindow.DragMove();
        }

        #endregion

        #endregion

        #region 登录操作

        #region ViewModel 命令 LoginOKCommand -- 确认登录

        public DelegateCommand<object> LoginOKCommand { get; private set; }
        private void OnLoginOK(object obj)
        {
            var loginForm = obj as DataForm;

            // 由于未使用 DataForm 中的标准“确定”按钮，因此需要强制进行验证。
            // 如果未确保窗体有效，则在实体无效时调用该操作会导致异常。
            if (loginForm.ValidateItem())
            {
                this.LogInfo.CurrentLoginOperation = WebContext.Current.Authentication.Login(this.LogInfo.ToLoginParameters(), this.LoginOperation_Completed, null);
            }
            else
            {
                this.userNameTextBox.Focus();
            }
        }

        #endregion

        #region ViewModel 命令 LoginCancelCommand -- 取消登录

        public DelegateCommand<object> LoginCancelCommand { get; private set; }
        private void OnLoginCancel(object obj)
        {
            Application.Current.MainWindow.Close();
        }

        #endregion

        #endregion

        #region ViewModel 命令 SetAppCommand -- 选择应用

        public DelegateCommand<object> SetAppCommand { get; private set; }
        private void OnSetApp(object obj)
        {
            this.IsAppDrop = false;
            var app = obj as Applications;
            SetApp(app);
        }

        #endregion

        #endregion

        #region Method

        /// <summary>
        /// 移动窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MoveWindow(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Application.Current.MainWindow.DragMove();
        }

        /// <summary>
        /// 去掉右击选择默认菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// 自动生成登录框DataForm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void LoginForm_AutoGeneratingField(object sender, DataFormAutoGeneratingFieldEventArgs e)
        {
            if (e.PropertyName == "UserName")
            {
                this.userNameTextBox = (TextBox)e.Field.Content;
            }
            else if (e.PropertyName == "Password")
            {
                PasswordBox passwordBox = new PasswordBox();
                e.Field.ReplaceTextBox(passwordBox, PasswordBox.PasswordProperty);
                this.LogInfo.PasswordAccessor = () => passwordBox.Password;
            }
        }

        /// <summary>
        /// 把用户输入框设置为焦点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void LoginForm_ContentLoaded(object sender, DataFormContentLoadEventArgs e)
        {
            if (!isDfFocused)
            {
                this.userNameTextBox.Focus();
                this.isDfFocused = true;
            }
        }

        /// <summary>
        /// 将 Esc 映射到取消按钮，将 Enter 映射到确定按钮。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void LoginForm_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.OnLoginCancel(sender);
            }
            else if (e.Key == Key.Enter && !this.LogInfo.IsLoggingIn)
            {
                this.OnLoginOK(sender);
            }
        }

        #endregion

        #endregion

        #region IPartImportsSatisfiedNotification 成员

        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            SubscribeEvent();

            this.LoginOKCommand = new DelegateCommand<object>(this.OnLoginOK);
            this.LoginCancelCommand = new DelegateCommand<object>(this.OnLoginCancel);
            this.HomeCommand = new DelegateCommand<object>(this.OnHome);
            this.NavigateCommand = new DelegateCommand<object>(this.OnNavigate);
            this.SetAppCommand = new DelegateCommand<object>(this.OnSetApp);

            if (Application.Current.IsRunningOutOfBrowser)
            {
                this.MinimizeCommand = new DelegateCommand<object>(this.OnMinimize);
                this.MaxmizeCommand = new DelegateCommand<object>(this.OnMaxmize);
                this.CloseWndCommand = new DelegateCommand<object>(this.OnCloseWnd);
                this.MoveWndCommand = new DelegateCommand<object>(this.OnMoveWnd);
            }

            moduleManager.ModuleDownloadProgressChanged += moduleManager_ModuleDownloadProgressChanged;
            moduleManager.LoadModuleCompleted += moduleManager_LoadModuleCompleted;


        }

        #endregion

        #region 订阅事件处理

        private void SubscribeEvent()
        {
            MessageEventAggregatorRepository.EventAggregator.Subscribe<MessageEvent>(OnSendOprationMsg, ReceiveType.Single);
            MessageEventAggregatorRepository.EventAggregator.Subscribe<NavigationEvent>(OnSendOprationNavigation, ReceiveType.Single);
            MessageEventAggregatorRepository.EventAggregator.Subscribe<VersionUpdateEvent>(OnVersionUpdate, ReceiveType.Single);
        }

        private void OnSendOprationMsg(MessageEvent e)
        {
            this.OprationMsg = e.Message;
            switch (e.MessageType)
            {
                case MessageOperationLibrary.Events.MessageType.Success:
                    this.MessageType = 1;
                    break;
                case MessageOperationLibrary.Events.MessageType.Fail:
                    this.MessageType = 2;
                    break;
                case MessageOperationLibrary.Events.MessageType.Info:
                    this.MessageType = 3;
                    break;
                default:
                    break;
            }
        }

        private void OnSendOprationNavigation(NavigationEvent e)
        {
            if (e.IsSkip == true)
            {
                if (navigationJournal == null)
                    navigationJournal = regionManager.Regions[RegionNames.MainRegion].NavigationService.Journal;
                if (navigationJournal != null)
                {
                    if (navigationJournal.CanGoBack)
                    {
                        navigationJournal.GoBack();
                    }
                    else
                    {
                        Uri uri = new Uri("HomeView", UriKind.Relative);
                        this.regionManager.RequestNavigate(RegionNames.MainRegion, uri);
                    }
                    this.FuncName = lastFuncName;
                }
            }
            else
            {
                this.FuncName = lastFuncName;
            }
        }

        private void OnVersionUpdate(VersionUpdateEvent e)
        {
            VersionUpdateMessage = e.Message;
        }


        #endregion
    }
}