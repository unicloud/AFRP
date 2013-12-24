using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.ServiceModel.DomainServices.Client.ApplicationServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MessageOperationLibrary.ApplicationModel.Eventing;
using MessageOperationLibrary.EventAggregatorRepository;
using MessageOperationLibrary.Events;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.ServiceLocation;
using Telerik.Windows.Controls;
using UniCloud.Fleet.Services;
using UniCloud.Infrastructure;
using UniCloud.Security.Models;
using UniCloud.Security.Services;
using ViewModelBase = UniCloud.Fleet.Services.ViewModelBase;

namespace UniCloud.Shell
{
    [Export(typeof(ShellViewModel))]
    public class ShellViewModel : ViewModelBase, IPartImportsSatisfiedNotification
    {

        public ShellViewModel()
        {
            if (Application.Current.InstallState == InstallState.Installed)
            {
                Application.Current.CheckAndDownloadUpdateCompleted += Current_CheckAndDownloadUpdateCompleted;
                Application.Current.CheckAndDownloadUpdateAsync();
            }
        }

        /// <summary>
        /// 服务端发生更新，客户端也需更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Current_CheckAndDownloadUpdateCompleted(object sender, CheckAndDownloadUpdateCompletedEventArgs e)
        {
            if (e.UpdateAvailable && e.Error == null)
            {
                RadWindow.Alert(this.SetAlter("新版本更新提醒", "确定", "新版本已经更新成功。", 13, 250, () =>
                    {
                        this._userNameTextBox.Focus();
                        this._isDfFocused = true;
                    }));
            }
            else if (e.Error != null)
            {
                var text = "更新版本出现以下错误信息：" + Environment.NewLine + Environment.NewLine + e.Error.Message;
                RadWindow.Alert(this.SetAlter("新版本更新提醒", "确定", text, 13, 250, () => Application.Current.MainWindow.Close()));
            }
        }

        #region Local

        [Import]
        public IRegionManager regionManager;
        [Import]
        public IModuleManager moduleManager;

        private IRegionNavigationJournal _navigationJournal;
        private readonly IAuthServices _service = ServiceLocator.Current.GetInstance<IAuthServices>();
        private readonly IFleetService _fleetService = ServiceLocator.Current.GetInstance<IFleetService>();
        private TextBox _userNameTextBox;
        private Applications _curApp;
        private bool _isDfFocused;

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
                StatusData.curUser = user;
                // 获取应用列表
                this.AppList = this._service.EntityContainer.GetEntitySet<Applications>()
                    .Where(a => a.ApplicationName != "UniCloud" && a.ApplicationName != "CAFM").ToList();
                // 设置当前用户应用与功能的可用性
                this.AppList.ForEach(a =>
                    {
                        if (user != null)
                        {
                            var userAppRoles = user.UserInRoles.Select(ur => ur.Roles).Where(r => r.Application == a).ToList();
                            a.IsValid = userAppRoles.Any();
                            a.FunctionItems.Where(fi => fi.IsLeaf).ToList().ForEach(f =>
                                                                                    f.IsValid =
                                                                                    userAppRoles.SelectMany(
                                                                                        r => r.FunctionsInRoles)
                                                                                                .Any(
                                                                                                    fr =>
                                                                                                    fr.FunctionItemID ==
                                                                                                    f.FunctionItemID &&
                                                                                                    fr.IsValid));
                        }
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
                this.LogInfo.ValidationErrors.Add(new ValidationResult(ErrorResources.ErrorBadUserNameOrPassword,
                                                                       new[] { "UserName", "Password" }));
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
            if (this._curApp != app)
            {
                this._curApp = app;
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
        /// <param name="app">当前应用</param>
        private void SetFunc(Applications app)
        {
            var functionItem = app.FunctionItems.SingleOrDefault(f => f.ParentItemID == null);
            if (functionItem != null)
                this.FuncList = functionItem.SubItems.ToList();
        }

        /// <summary>
        /// 导航
        /// </summary>
        /// <param name="obj"></param>
        private void Navigating(object obj)
        {
            var curFunc = obj as FunctionItem;
            if (curFunc != null)
            {
                this.FuncName = this._service.EntityContainer.GetEntitySet<FunctionItem>()
                                    .Single(f => f.FunctionItemID == curFunc.ParentItemID).Name + "：" + curFunc.Name;

                var targetView = _curApp.ViewNameSpace + curFunc.ViewName;
                var uri = new Uri(targetView, UriKind.Relative);
                var activeView = this.regionManager.Regions[RegionNames.MainRegion].ActiveViews.FirstOrDefault();
                if (activeView != null && activeView.ToString() != targetView)
                {
                    this.regionManager.RequestNavigate(RegionNames.MainRegion, uri);
                }
            }
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

        private bool _isMenuenabled;
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
            get { return this._versionUpdateMessage; }
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
            var uri = new Uri("HomeView", UriKind.Relative);
            this.regionManager.RequestNavigate(RegionNames.MainRegion, uri);
        }

        #endregion

        #region ViewModel 命令 NavigateCommand -- 导航

        public DelegateCommand<object> NavigateCommand { get; private set; }
        private void OnNavigate(object obj)
        {
            this.IsFuncDrop = false;
            if (this._fleetService.EntityContainer.HasChanges)
            {
                RadWindow.Confirm(this.SetConfirm("确认是否继续导航", "继续", "返回", CAFMStrings.ConfirmNavigateAwayFromRequestDetail, 13, 250,
                                                  (o, e) =>
                                                  {
                                                      if (e.DialogResult == true)
                                                      {
                                                          this._fleetService.RejectChanges();
                                                          this._fleetService.SetCurrentAnnual();
                                                          this._fleetService.SetCurrentPlan();
                                                          this.Navigating(obj);
                                                      }
                                                  }));
            }
            else
            {
                Navigating(obj);
            }
        }

        #endregion

        #endregion

        #region 窗口操作

        private bool _maximized;

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
            if (!_maximized)
            {
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
                _maximized = true;
                this.IsMaximize = true;
            }
            else
            {
                Application.Current.MainWindow.WindowState = WindowState.Normal;
                _maximized = false;
                this.IsMaximize = false;
            }
        }

        #endregion

        #region ViewModel 命令 CloseWndCommand -- 关闭窗口

        public DelegateCommand<object> CloseWndCommand { get; private set; }
        private static void OnCloseWnd(object obj)
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

        #region ViewModel 命令 LoginOkCommand -- 确认登录

        public DelegateCommand<object> LoginOkCommand { get; private set; }
        private void OnLoginOk(object obj)
        {
            var loginForm = obj as DataForm;

            // 由于未使用 DataForm 中的标准“确定”按钮，因此需要强制进行验证。
            // 如果未确保窗体有效，则在实体无效时调用该操作会导致异常。
            if (loginForm != null && loginForm.ValidateItem())
            {
                this.LogInfo.CurrentLoginOperation = WebContext.Current.Authentication.Login(this.LogInfo.ToLoginParameters(), this.LoginOperation_Completed, null);
            }
            else
            {
                this._userNameTextBox.Focus();
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
            StatusData.curApplications = obj as Applications;
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
        public void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            Application.Current.MainWindow.DragMove();
        }

        /// <summary>
        /// 去掉右击选择默认菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MouseRightButtonDown(object sender, MouseButtonEventArgs e)
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
            switch (e.PropertyName)
            {
                case "UserName":
                    this._userNameTextBox = (TextBox)e.Field.Content;
                    break;
                case "Password":
                    {
                        var passwordBox = new PasswordBox();
                        e.Field.ReplaceTextBox(passwordBox, PasswordBox.PasswordProperty);
                        this.LogInfo.PasswordAccessor = () => passwordBox.Password;
                    }
                    break;
            }
        }

        /// <summary>
        /// 把用户输入框设置为焦点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void LoginForm_ContentLoaded(object sender, DataFormContentLoadEventArgs e)
        {
            if (!_isDfFocused)
            {
                this._userNameTextBox.Focus();
                this._isDfFocused = true;
            }
        }

        /// <summary>
        /// 将 Esc 映射到取消按钮，将 Enter 映射到确定按钮。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void LoginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.OnLoginCancel(sender);
            }
            else if (e.Key == Key.Enter && !this.LogInfo.IsLoggingIn)
            {
                this.OnLoginOk(sender);
            }
        }

        #endregion

        #endregion

        #region IPartImportsSatisfiedNotification 成员

        public void OnImportsSatisfied()
        {
            SubscribeEvent();

            this.LoginOkCommand = new DelegateCommand<object>(this.OnLoginOk);
            this.LoginCancelCommand = new DelegateCommand<object>(this.OnLoginCancel);
            this.HomeCommand = new DelegateCommand<object>(this.OnHome);
            this.NavigateCommand = new DelegateCommand<object>(this.OnNavigate);
            this.SetAppCommand = new DelegateCommand<object>(this.OnSetApp);

            if (Application.Current.IsRunningOutOfBrowser)
            {
                _maximized = false;
                this.IsMaximize = false;
                this.MinimizeCommand = new DelegateCommand<object>(this.OnMinimize);
                this.MaxmizeCommand = new DelegateCommand<object>(this.OnMaxmize);
                this.CloseWndCommand = new DelegateCommand<object>(OnCloseWnd);
                this.MoveWndCommand = new DelegateCommand<object>(this.OnMoveWnd);
            }
            else
                this.IsOOB = false;

            moduleManager.ModuleDownloadProgressChanged += moduleManager_ModuleDownloadProgressChanged;
            moduleManager.LoadModuleCompleted += moduleManager_LoadModuleCompleted;

            this._fleetService.Context.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == "IsLoading")
                {
                    this.IsBusy = this._fleetService.Context.IsLoading;
                }
                if (e.PropertyName == "IsSubmitting")
                {
                    this.IsBusy = this._fleetService.Context.IsSubmitting;
                }
            };
        }

        #endregion

        #region 订阅事件处理

        private void SubscribeEvent()
        {
            MessageEventAggregatorRepository.EventAggregator.Subscribe<MessageEvent>(OnReceiveMessage, ReceiveType.Single);
            MessageEventAggregatorRepository.EventAggregator.Subscribe<VersionUpdateEvent>(OnVersionUpdate, ReceiveType.Single);
        }

        private void OnSendOprationMsg(Tuple<OperationMessageType, string> msg)
        {
            this.OprationMsg = msg.Item2;
            switch (msg.Item1)
            {
                case OperationMessageType.Success:
                    this.MessageType = 1;
                    break;
                case OperationMessageType.Fail:
                    this.MessageType = 2;
                    break;
                case OperationMessageType.Info:
                    this.MessageType = 3;
                    break;
            }
        }

        private void OnReceiveMessage(MessageEvent e)
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
            }
        }

        private void OnVersionUpdate(VersionUpdateEvent e)
        {
            VersionUpdateMessage = e.Message;
        }

        #endregion

    }
}