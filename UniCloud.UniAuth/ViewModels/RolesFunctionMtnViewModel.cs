
using MessageOperationLibrary.EventAggregatorRepository;
using MessageOperationLibrary.Events;
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
using Telerik.Windows.Controls;
using UniCloud.Security.Models;
using UniCloud.Security.Services;

namespace UniCloud.UniAuth.ViewModels
{


    [Export(typeof(RolesFunctionMtnViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public class RolesFunctionMtnViewModel : NotificationObject, IPartImportsSatisfiedNotification, IConfirmNavigationRequest
    {

        public RolesFunctionMtnViewModel()
        {
            this._service.EntityContainer.PropertyChanged += new PropertyChangedEventHandler(EntityContainer_PropertyChanged);
            this._confirmExitInteractionRequest = new InteractionRequest<ConfirmViewModel>();
        }

        #region Local

        [Import]
        public IRegionManager regionManager;
        private readonly IAuthServices _service = ServiceLocator.Current.GetInstance<IAuthServices>();
        private readonly UniCloud.UniAuth.Views.RolesFunctionMtnView _view = ServiceLocator.Current.GetInstance<UniCloud.UniAuth.Views.RolesFunctionMtnView>();
        private RadGridView roles;
        private RadTreeView functionRole;
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
            this.roles = this._view.roles as RadGridView;

        }

        /// <summary>
        /// 设置应用或者应用下面的页面按钮是否可用
        /// </summary>
        /// <param name="currentRoles"></param>
        private void SetFunctionIsCheck(Roles currentRoles)
        {
            if (currentRoles != null && currentRoles.FunctionsInRoles != null)
            {
                StatusData.roles = currentRoles;
                var roleApplications = currentRoles.FunctionsInRoles;
                if (roleApplications != null)
                {
                    SetButtonFunctionIsCheck(currentRoles);
                    SetViewFunctionIsCheck(currentRoles);
                    SetMenuFunctionIsCheck(currentRoles);
                    SetApplicationIsCheck(currentRoles);
                }
                RoleApplications = roleApplications.Select(p => p.FunctionItem).Where(p => p.LevelCode.Length == 2);
            }
            else
            {
                RoleApplications = null;
            }
        }
        /// <summary>
        /// 按钮师傅被选中
        /// </summary>
        /// <param name="currentRoles"></param>
        private void SetButtonFunctionIsCheck(Roles currentRoles)
        {
            if (currentRoles.FunctionsInRoles.Any(p => p.FunctionItem.IsButton == true))
            {
                currentRoles.FunctionsInRoles.Where(p => p.FunctionItem.IsButton == true).ToList().ForEach(r =>
                {
                    if (r.IsValid)
                    {
                        //获取功能项，设置自定义IsChecked为true
                        var fItem = r.FunctionItem;
                        fItem.IsClickStatus = false;
                        fItem.IsChecked = true;

                    }
                    else
                    {
                        //获取功能项，设置自定义IsChecked为false
                        var fItem = r.FunctionItem;
                        fItem.IsClickStatus = false;
                        fItem.IsChecked = false;

                    }
                });
            }
        }

        /// <summary>
        /// 设置页面是否被选
        /// </summary>
        /// <param name="currentRoles"></param>
        private void SetViewFunctionIsCheck(Roles currentRoles)
        {
            if (currentRoles.FunctionsInRoles.Any(p => p.FunctionItem.IsLeaf == true))
            {
                currentRoles.FunctionsInRoles.Where(p => p.FunctionItem.IsLeaf == true).ToList().ForEach(r =>
                {
                    if (r.IsValid)
                    {
                        //获取功能项，设置自定义IsChecked为true
                        var fItem = r.FunctionItem;
                        fItem.IsClickStatus = false;
                        bool allChildrenChecked = fItem.SubItems.All(x => x.IsChecked == true);

                        if (allChildrenChecked)
                        {
                            fItem.IsChecked = true;
                        }
                        else
                        {
                            fItem.IsChecked = null;
                        }
                    }
                    else
                    {
                        //获取功能项，设置自定义IsChecked为false
                        var fItem = r.FunctionItem;
                        fItem.IsClickStatus = false;
                        fItem.IsChecked = false;

                    }
                });
            }
        }
        /// <summary>
        /// 设置菜单是否被选
        /// </summary>
        /// <param name="currentRoles"></param>
        private void SetMenuFunctionIsCheck(Roles currentRoles)
        {
            if (currentRoles.FunctionsInRoles.Any(p => p.FunctionItem.IsLeaf == false && p.FunctionItem.IsButton == false && p.FunctionItem.LevelCode.Length != 2))
            {
                currentRoles.FunctionsInRoles.Where(p => p.FunctionItem.IsLeaf == false && p.FunctionItem.IsButton == false && p.FunctionItem.LevelCode.Length != 2).ToList().ForEach(r =>
                {
                    if (r.IsValid)
                    {
                        //获取功能项，设置自定义IsChecked为true
                        var fItem = r.FunctionItem;
                        fItem.IsClickStatus = false;
                        bool allChildrenChecked = fItem.SubItems.All(a => a.IsChecked == true);
                        if (allChildrenChecked)
                        {
                            fItem.IsChecked = true;
                        }
                        else
                        {
                            fItem.IsChecked = null;
                        }
                    }
                    else
                    {
                        //获取功能项，设置自定义IsChecked为false
                        var fItem = r.FunctionItem;
                        fItem.IsClickStatus = false;
                        fItem.IsChecked = false;
                    }
                });
            }
        }

        /// <summary>
        /// 设置应用是否被选
        /// </summary>
        /// <param name="currentRoles"></param>
        private void SetApplicationIsCheck(Roles currentRoles)
        {
            if (currentRoles.FunctionsInRoles.Any(p => p.FunctionItem.LevelCode.Length == 2))
            {
                currentRoles.FunctionsInRoles.Where(p => p.FunctionItem.LevelCode.Length == 2).ToList().ForEach(r =>
                {
                    if (r.IsValid)
                    {
                        //获取功能项，设置自定义IsChecked为true
                        var fItem = r.FunctionItem;
                        fItem.IsClickStatus = false;
                        bool allChildrenChecked = fItem.SubItems.All(a => a.IsChecked == true);
                        if (allChildrenChecked)
                        {
                            fItem.IsChecked = true;
                        }
                        else
                        {
                            fItem.IsChecked = null;
                        }
                    }
                    else
                    {
                        //获取功能项，设置自定义IsChecked为false
                        var fItem = r.FunctionItem;
                        fItem.IsClickStatus = false;
                        fItem.IsChecked = false;
                    }
                });
            }
        }

        /// <summary>
        /// 设置当前角色是否可编辑
        /// </summary>
        private void SetIsRolesEnabled(Roles currentRoles)
        {
            if (currentRoles == null || currentRoles.RoleName != "系统管理员")
            {
                IsRoleEnabled = true;
            }
            else
            {
                IsRoleEnabled = false;
            }
        }



        #endregion

        #endregion

        #region ViewModel

        #region 加载实体集合 AllRoles

        /// <summary>
        /// 用于绑定的集合，根据实际需要修改
        /// </summary>
        public IEnumerable<Roles> AllRoles
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<Roles>();
            }
        }

        private bool _isBusyRoles = true;
        public bool IsBusyRoles
        {
            get { return this._isBusyRoles; }
            private set
            {
                if (this._isBusyRoles != value)
                {
                    this._isBusyRoles = value;
                }
            }
        }

        /// <summary>
        /// 加载实体集合的方法
        /// </summary>
        private void LoadRoles()
        {
            this.IsBusy = true;
            this._service.LoadRoles(new QueryBuilder<Roles>(), lo =>
            {
                this.IsBusyRoles = false;
                this.IsBusy = this.IsBusyRoles || this.IsBusyApplications || this.IsBusyUserInRole;
                if (lo.Error != null)
                {
                    // 处理加载失败

                }
                else
                {
                    this.RaisePropertyChanged(() => this.AllRoles);

                }
            }, null);
        }

        #endregion

        #region 加载实体集合  ViewApplications

        /// <summary>
        /// 用于绑定的集合，根据实际需要修改
        /// </summary>
        public IEnumerable<Applications> ViewApplications
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<Applications>().Where(a => a.LoweredApplicationName != "unicloud");
            }
        }

        private bool _isBusyApplications = true;
        public bool IsBusyApplications
        {
            get { return this._isBusyApplications; }
            private set
            {
                if (this._isBusyApplications != value)
                {
                    this._isBusyApplications = value;
                }
            }
        }

        /// <summary>
        /// 加载实体集合的方法
        /// </summary>
        private void LoadApplications()
        {
            this.IsBusy = true;
            this._service.LoadApplications(new QueryBuilder<Applications>(), lo =>
            {
                this.IsBusyApplications = false;
                this.IsBusy = this.IsBusyRoles || this.IsBusyApplications || this.IsBusyUserInRole;
                if (lo.Error != null)
                {
                    // 处理加载失败

                }
                else
                {
                    this.RaisePropertyChanged(() => this.ViewApplications);

                }
            }, null);
        }
        #endregion

        #region 加载实体集合 UserInRole


        public IEnumerable<UserInRole> ViewUserInRole
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<UserInRole>();
            }
        }

        private bool _isBusyUserInRole = true;
        public bool IsBusyUserInRole
        {
            get { return this._isBusyUserInRole; }
            private set
            {
                if (this._isBusyUserInRole != value)
                {
                    this._isBusyUserInRole = value;
                }
            }
        }

        /// <summary>
        /// 加载实体集合UserInRole的方法
        /// </summary>
        private void LoadUserInRole()
        {
            this.IsBusy = true;
            this._service.LoadUserInRole(new QueryBuilder<UserInRole>(), lo =>
            {
                this.IsBusyUserInRole = false;
                this.IsBusy = this.IsBusyRoles || this.IsBusyApplications || this.IsBusyUserInRole;
                if (lo.Error != null)
                {
                    // 处理加载失败

                }
                else
                {

                    this.RaisePropertyChanged(() => this.ViewUserInRole);
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

        #region ViewModel 属性 SelCurrentRoles --选中当前角色

        private Roles _selCurrentRoles;
        /// <summary>
        /// 选中当前角色
        /// </summary>
        public Roles SelCurrentRoles
        {
            get { return this._selCurrentRoles; }
            set
            {
                _selCurrentRoles = value;
                SetFunctionIsCheck(value);
                SetIsRolesEnabled(value);
                RefreshButtonState();
                this.RaisePropertyChanged(() => this.SelCurrentRoles);


            }
        }
        #endregion

        #region ViewModel 属性 SelCurrentApplication --当前应用

        private Applications _selCurrentApplication;
        /// <summary>
        /// 当前应用
        /// </summary>
        public Applications SelCurrentApplication
        {
            get { return this._selCurrentApplication; }
            set
            {
                _selCurrentApplication = value;
                this.RaisePropertyChanged(() => this.SelCurrentApplication);
            }
        }
        #endregion

        #region ViewModel 属性 RoleApplications --角色的应用

        private IEnumerable<FunctionItem> _roleApplications;
        /// <summary>
        /// 角色的应用
        /// </summary>
        public IEnumerable<FunctionItem> RoleApplications
        {
            get { return this._roleApplications; }
            set
            {

                if (this._roleApplications != value)
                {
                    _roleApplications = value;
                    this.RaisePropertyChanged(() => this.RoleApplications);

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

        #region ViewModel 属性 IsRoleEnabled --是否角色允许编辑

        private bool _isRoleEnabled;
        /// <summary>
        /// 是否角色允许编辑
        /// </summary>
        public bool IsRoleEnabled
        {
            get { return this._isRoleEnabled; }
            set
            {

                if (this._isRoleEnabled != value)
                {
                    _isRoleEnabled = value;
                    this.RaisePropertyChanged(() => this.IsRoleEnabled);

                }
            }
        }
        #endregion

        #endregion

        #region Command

        #region ViewModel 命令 -- 添加新角色

        public DelegateCommand<object> AddCommand { get; private set; }
        private void OnAdd(object sender)
        {
            if (SelCurrentApplication == null)
            {
                MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(new MessageEvent { Message = "请先选择应用！", MessageType = MessageType.Info });
                return;
            }
            Roles role = new Roles();
            //role.RoleId = this._service.EntityContainer.GetEntitySet<Roles>().Count == 0 ? 1 :
            //    this._service.EntityContainer.GetEntitySet<Roles>().Max(p => p.RoleId) + 1;
            role.Application = SelCurrentApplication;
            //增加角色的同时需要增加FunctionsInRoles
            SelCurrentApplication.FunctionItems.ToList().ForEach(f =>
                {
                    FunctionsInRoles fr = new FunctionsInRoles();
                    fr.Role = role;
                    fr.FunctionItem = f;
                });

            this.roles.Items.MoveCurrentToLast();
            this._service.EntityContainer.GetEntitySet<Roles>().Add(role);
            RefreshButtonState();
        }
        private bool _canAdd = true;
        bool CanAdd(object sender)
        {
            return this._canAdd;
        }

        #endregion

        #region ViewModel 命令 -- 删除选中角色

        public DelegateCommand<object> RemoveCommand { get; private set; }
        private void OnRemove(object sender)
        {
            if (this.SelCurrentRoles != null && this.SelCurrentRoles.RoleName == "系统管理员")
            {
                MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(new MessageEvent { Message = "系统管理账户不能删除！", MessageType = MessageType.Info });
                return;
            }
            if (this.SelCurrentRoles != null)
            {
                //删除用户角色
                SelCurrentRoles.UserInRoles.ToList().ForEach(f =>
                    {
                        this._service.EntityContainer.GetEntitySet<UserInRole>().Remove(f);
                    });
                ////删除功能角色
                //SelCurrentRoles.FunctionsInRoles.ToList().ForEach(f =>
                //    {
                //        this._service.EntityContainer.GetEntitySet<FunctionsInRoles>().Remove(f);
                //    });
                this._service.EntityContainer.GetEntitySet<Roles>().Remove(SelCurrentRoles);

            }
            RefreshButtonState();
        }
        private bool _canRemove = true;
        bool CanRemove(object sender)
        {
            if (this.SelCurrentRoles != null)
            {
                this._canRemove = true;
            }
            else
            {
                this._canRemove = false;
            }
            return this._canRemove;
        }

        #endregion

        #region ViewModel 命令 -- 保存

        public DelegateCommand<object> SaveCommand { get; private set; }
        private void OnSave(object sender)
        {
            if (AllRoles.Any(p => string.IsNullOrEmpty(p.RoleName)))
            {
                MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(new MessageEvent { Message = "角色名称不能为空！", MessageType = MessageType.Info });
                return;
            }
            if (SelCurrentRoles != null && AllRoles.Where(p => p != SelCurrentRoles && p.ApplicationId == SelCurrentRoles.ApplicationId).Any(p => p.RoleName == SelCurrentRoles.RoleName))
            {
                MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(new MessageEvent { Message = "该角色已存在！", MessageType = MessageType.Info });
                return;
            }
            //假设还有存在未编辑的项目，则在保存前取消编辑
            if (this.roles.SelectedItems != null)
            {
                this.roles.CancelEdit();
            }
            this._service.SubmitChanges(UniCloud.Infrastructure.CAFMStrings.SaveSuccess, UniCloud.Infrastructure.CAFMStrings.SaveFail, sm => { }, null);
        }
        private bool _canSave = false;
        bool CanSave(object sender)
        {
            return this._canSave;
        }

        #endregion

        #region ViewModel 命令 -- 取消更改

        public DelegateCommand<object> AbortCommand { get; private set; }
        private void OnAbort(object sender)
        {
            this._service.RejectChanges();
            SetViewFunctionIsCheck(SelCurrentRoles);
        }
        private bool _canAbort = false;
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
            this.AddCommand = new DelegateCommand<object>(this.OnAdd, this.CanAdd);
            this.RemoveCommand = new DelegateCommand<object>(this.OnRemove, this.CanRemove);
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
            LoadRoles();
            LoadApplications();
            LoadUserInRole();
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
