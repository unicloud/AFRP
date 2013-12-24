
using MessageOperationLibrary.EventAggregatorRepository;
using MessageOperationLibrary.Events;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Windows.Data.DomainServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.DragDrop;
using UniCloud.Security.Models;
using UniCloud.Security.Services;
using System.Linq;
using Telerik.Windows.Controls.TreeView;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;


namespace UniCloud.UniAuth.ViewModels
{
    [Export(typeof(UserRoleAllotViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public class UserRoleAllotViewModel : NotificationObject, IPartImportsSatisfiedNotification, IConfirmNavigationRequest
    {

        public UserRoleAllotViewModel()
        {
            this._service.EntityContainer.PropertyChanged += new PropertyChangedEventHandler(EntityContainer_PropertyChanged);
            this._confirmExitInteractionRequest = new InteractionRequest<ConfirmViewModel>();
        }

        #region Local

        [Import]
        public IRegionManager regionManager;
        private string dropImpossibleReason;
        private readonly IAuthServices _service = ServiceLocator.Current.GetInstance<IAuthServices>();
        private readonly UniCloud.UniAuth.Views.UserRoleAllotView _view = ServiceLocator.Current.GetInstance<UniCloud.UniAuth.Views.UserRoleAllotView>();
        private RadGridView users, userInRoles, roles;
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
        }

        /// <summary>
        /// 以View的实例初始化ViewModel相关字段、属性
        /// </summary>
        private void ViewModelInitializer()
        {
            this.users = _view.users as RadGridView;

            this.roles = _view.roles as RadGridView;
            // 拖拽事件处理程序设置
            RadDragAndDropManager.AddDragQueryHandler(roles, this.Roles_OnDragQuery);
            RadDragAndDropManager.AddDragInfoHandler(roles, this.Roles_OnDragInfo);
            RadDragAndDropManager.AddDropQueryHandler(roles, this.Roles_OnDropQuery);
            RadDragAndDropManager.AddDropInfoHandler(roles, this.Roles_OnDropInfo);
            // 鼠标双击事件处理程序设置
            roles.AddHandler(RadGridView.MouseLeftButtonUpEvent, new MouseButtonEventHandler(Roles_MouseLeftButtonUp), true);

            this.userInRoles = _view.userInRoles as RadGridView;
            // 拖拽事件处理程序设置
            RadDragAndDropManager.AddDragQueryHandler(userInRoles, this.UserInRoles_OnDragQuery);
            RadDragAndDropManager.AddDragInfoHandler(userInRoles, this.UserInRoles_OnDragInfo);
            RadDragAndDropManager.AddDropQueryHandler(userInRoles, this.UserInRoles_OnDropQuery);
            RadDragAndDropManager.AddDropInfoHandler(userInRoles, this.UserInRoles_OnDropInfo);
            // 鼠标双击事件处理程序设置
            userInRoles.AddHandler(RadGridView.MouseLeftButtonUpEvent, new MouseButtonEventHandler(UserInRoles_MouseLeftButtonUp), true);

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
        /// 处理通过拖拽和双击往角色列表中为用户添加角色
        /// </summary>
        /// <param name="sender"></param>
        private void AddRoleToUserInRoles(object sender)
        {
            var selrole = sender as Roles;
            UserInRole userRole = new UserInRole();
            userRole.Roles = selrole;
            //判断拖动的角色是否和角色列表中的角色重复,若重复不能添加
            SelCurrentUser.UserInRoles.Add(userRole);
            this.userInRoles.Items.MoveCurrentTo(userRole);

        }

        /// <summary>
        /// 处理通过拖拽和双击从当前角色列表中移除角色
        /// </summary>
        /// <param name="sender"></param>
        private void RemoveRoleFromUserInRoles(object sender)
        {
            //若刚新增的,直接从角色列表中移除;若是已经存入UsersInRoles表中的，要删除数据表中的记录
            UserInRole selUserInRole = sender as UserInRole;
            if (selUserInRole != null)
            {
                this._service.EntityContainer.GetEntitySet<UserInRole>().Remove(selUserInRole);
            }

        }

        /// <summary>
        /// 处理拖拽的拖动操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Roles_OnDragQuery(object sender, DragDropQueryEventArgs e)
        {
            RadGridView gridView = sender as RadGridView;
            if (gridView != null)
            {
                IList selectedItems = gridView.SelectedItems.ToList();
                e.QueryResult = selectedItems.Count > 0;
                e.Options.Payload = selectedItems;
            }
            else
                e.QueryResult = false;
            e.Handled = true;
        }
        /// <summary>
        /// 处理拖拽的拖动操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Roles_OnDragInfo(object sender, DragDropEventArgs e)
        {
            RadGridView gridView = sender as RadGridView;
            IEnumerable draggedItems = e.Options.Payload as IEnumerable;
            if (e.Options.Status == DragStatus.DragInProgress)
            {
                TreeViewDragCue cue = new TreeViewDragCue();
                // 设置拖放窗口显示内容的模板。
                cue.ItemTemplate = ServiceLocator.Current.GetInstance<UniCloud.UniAuth.Views.UserRoleAllotView>().Resources["RoleTemplate"] as DataTemplate;
                cue.Background = new SolidColorBrush(Colors.Transparent);
                cue.ItemsSource = draggedItems;
                e.Options.DragCue = cue;

            }
        }
        /// <summary>
        /// 处理拖拽的释放操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Roles_OnDropQuery(object sender, DragDropQueryEventArgs e)
        {
            bool result = true;
            var draggedItems = e.Options.Payload as List<object>;
            if (draggedItems != null && draggedItems[0].GetType() != typeof(UserInRole))
            {
                result = false;
                this.dropImpossibleReason = string.Empty;
            }
            e.QueryResult = result;
            e.Handled = true;
        }
        /// <summary>
        /// 处理拖拽的释放操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Roles_OnDropInfo(object sender, DragDropEventArgs e)
        {
            var gridView = sender as RadGridView;
            var draggedItems = e.Options.Payload as List<object>;

            TreeViewDragCue cue = e.Options.DragCue as TreeViewDragCue;

            if (e.Options.Status == DragStatus.DropPossible)
            {
                if (draggedItems != null && cue != null)
                {
                    cue.DragActionContent = String.Format("从当前用户移除 {0} 个角色", draggedItems.Count());
                    cue.IsDropPossible = true;
                }
            }
            else if (e.Options.Status == DragStatus.DropImpossible)
            {
                if (cue != null)
                {
                    cue.DragActionContent = dropImpossibleReason;
                    cue.IsDropPossible = false;
                }
            }
            else if (e.Options.Status == DragStatus.DropComplete)
            {

                if (draggedItems != null && draggedItems.Any())
                {
                    draggedItems.ForEach(d => RemoveRoleFromUserInRoles(d));
                }
            }
        }

        /// <summary>
        /// 处理拖拽的拖动操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserInRoles_OnDragQuery(object sender, DragDropQueryEventArgs e)
        {
            RadGridView gridView = sender as RadGridView;
            if (gridView != null)
            {
                IList selectedItems = gridView.SelectedItems.ToList();
                e.QueryResult = selectedItems.Count > 0 && (selectedItems[0] as UserInRole).Users.UserName != "admin";
                e.Options.Payload = selectedItems;
                if (selectedItems.Count > 0 && (selectedItems[0] as UserInRole).Users.UserName == "admin")
                {
                    MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(new MessageEvent { Message = "admin用户不能删除用户角色！", MessageType = MessageType.Info });
                }

            }
            else
                e.QueryResult = false;
            e.Handled = true;
        }
        /// <summary>
        /// 处理拖拽的拖动操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserInRoles_OnDragInfo(object sender, DragDropEventArgs e)
        {
            RadGridView gridView = sender as RadGridView;
            IEnumerable draggedItems = e.Options.Payload as IEnumerable;
            if (e.Options.Status == DragStatus.DragInProgress)
            {
                TreeViewDragCue cue = new TreeViewDragCue();
                // 设置拖放窗口显示内容的模板。
                cue.ItemTemplate = ServiceLocator.Current.GetInstance<UniCloud.UniAuth.Views.UserRoleAllotView>().Resources["UserInRolesTemplate"] as DataTemplate;
                cue.Background = new SolidColorBrush(Colors.Transparent);
                cue.ItemsSource = draggedItems;
                e.Options.DragCue = cue;
            }
        }
        /// <summary>
        /// 处理拖拽的释放操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserInRoles_OnDropQuery(object sender, DragDropQueryEventArgs e)
        {
            bool result = true;
            var draggedItems = e.Options.Payload as List<object>;
            if (draggedItems != null)
            {
                Roles selrole = draggedItems[0] as Roles;
                // 控制是否可以释放，添加不可释放的原因。
                if (SelCurrentUser == null)
                {
                    result = false;
                    this.dropImpossibleReason = "没有选中的用户";
                }
                if (selrole != null && (SelCurrentUser.UserInRoles.Any(p => p.Roles == selrole)))
                {
                    result = false;
                    this.dropImpossibleReason = "当前用户此角色已存在！";
                }
                else if (draggedItems != null && draggedItems[0].GetType() == typeof(UserInRole))
                {
                    result = false;
                    this.dropImpossibleReason = string.Empty;
                }
            }
            e.QueryResult = result;
            e.Handled = true;
        }
        /// <summary>
        /// 处理拖拽的释放操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserInRoles_OnDropInfo(object sender, DragDropEventArgs e)
        {
            var gridView = sender as RadGridView;
            var draggedItems = e.Options.Payload as List<object>;

            TreeViewDragCue cue = e.Options.DragCue as TreeViewDragCue;
            if (e.Options.Status == DragStatus.DropPossible)
            {
                if (cue != null)
                {
                    cue.DragActionContent = String.Format("添加 {0} 个角色到当前用户", draggedItems.Count());
                    cue.IsDropPossible = true;
                }
            }
            else if (e.Options.Status == DragStatus.DropImpossible)
            {
                if (cue != null)
                {
                    cue.DragActionContent = dropImpossibleReason;
                    cue.IsDropPossible = false;
                }
            }
            else if (e.Options.Status == DragStatus.DropComplete)
            {
                if (draggedItems != null && draggedItems.Any())
                {
                    draggedItems.ForEach(d => AddRoleToUserInRoles(d));
                }
            }
        }

        /// <summary>
        /// 处理鼠标双击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private DateTime _lastClickTime;
        private WeakReference _lastSender;
        private void Roles_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RadGridView gridView = sender as RadGridView;
            var now = DateTime.Now;
            if ((now - _lastClickTime).TotalMilliseconds < 250 && _lastSender != null && _lastSender.IsAlive && _lastSender.Target == sender)
            {

                if (SelCurrentRole != null)
                {

                    if (SelCurrentUser != null)
                    {
                        // 当前申请存、当前申请未提交且当前计划项未申请时才能通过双击添加计划项
                        if (SelCurrentUser != null && !SelCurrentUser.UserInRoles.Any(p => p.Roles == SelCurrentRole))
                        {
                            AddRoleToUserInRoles(SelCurrentRole);
                        }
                        else
                        {
                            MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(new MessageEvent { Message = "角色已存在！", MessageType = MessageType.Info });
                        }
                    }
                    else
                    {
                        MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(new MessageEvent { Message = "请先选择用户！", MessageType = MessageType.Info });
                    }
                }
                else
                {
                    MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(new MessageEvent { Message = "请先选中角色！", MessageType = MessageType.Info });
                }
            }
            _lastClickTime = now;
            _lastSender = new WeakReference(sender);
        }
        private void UserInRoles_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RadGridView gridView = sender as RadGridView;
            var now = DateTime.Now;
            if ((now - _lastClickTime).TotalMilliseconds < 250 && _lastSender != null && _lastSender.IsAlive && _lastSender.Target == sender)
            {

                if (SelUserInRoles != null)
                {
                    if (SelUserInRoles.Users.UserName != "admin")
                    {
                        RemoveRoleFromUserInRoles(SelUserInRoles);
                    }
                    else
                    {
                        MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(new MessageEvent { Message = "admin用户不能删除用户角色！", MessageType = MessageType.Info });
                        return;
                    }
                }
            }
            _lastClickTime = now;
            _lastSender = new WeakReference(sender);
        }

        #endregion

        #endregion

        #region ViewModel

        #region 加载实体集合 AllUsers

        /// <summary>
        /// 用于绑定的集合，根据实际需要修改
        /// </summary>
        public IEnumerable<Users> AllUsers
        {
            get
            {
                return this._service.EntityContainer.GetEntitySet<Users>();
            }
        }

        private bool _isBusyUsers = true;
        public bool IsBusyUsers
        {
            get { return this._isBusyUsers; }
            private set
            {
                if (this._isBusyUsers != value)
                {
                    this._isBusyUsers = value;
                }
            }
        }

        /// <summary>
        /// 加载实体集合的方法
        /// </summary>
        private void LoadUsers()
        {
            this.IsBusy = true;
            this._service.LoadUsers(new QueryBuilder<Users>(), lo =>
            {
                this.IsBusyUsers = false;
                this.IsBusy = this.IsBusyUserInRole || this.IsBusyRoles || this.IsBusyUsers;
                if (lo.Error != null)
                {
                    // 处理加载失败

                }
                else
                {
                    this.RaisePropertyChanged(() => this.AllUsers);
                    this.userInRoles.Items.MoveCurrentToFirst();
                }
            }, null);
        }

        #endregion

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
                this.IsBusy = this.IsBusyUserInRole || this.IsBusyRoles || this.IsBusyUsers;
                if (lo.Error != null)
                {
                    // 处理加载失败

                }
                else
                {

                    this.RaisePropertyChanged(() => this.AllRoles);
                    this.RaisePropertyChanged(() => this.SelCurrentRole);
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
                this.IsBusy = this.IsBusyUserInRole || this.IsBusyRoles || this.IsBusyUsers;
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

        #region ViewModel 属性 SelCurrentUser 当前选中的用户

        private Users _selCurrentUser;
        /// <summary>
        /// 当前选中的用户
        /// </summary>
        public Users SelCurrentUser
        {
            get { return _selCurrentUser; }
            set
            {
                _selCurrentUser = value;
                this.RaisePropertyChanged(() => this.SelCurrentUser);
            }
        }
        #endregion

        #region ViewModel 属性 SelCurrentRole --选中当前角色

        private Roles _selCurrentRole;
        /// <summary>
        /// 选中当前角色
        /// </summary>
        public Roles SelCurrentRole
        {
            get { return this._selCurrentRole; }
            set
            {
                _selCurrentRole = value;
                SetFunctionIsCheck(value);
                this.RaisePropertyChanged(() => this.SelCurrentRole);
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

        #region ViewModel 属性 SelUserInRoles --选中的用户角色

        private UserInRole _selUserInRoles;
        /// <summary>
        /// 选中的用户角色
        /// </summary>
        public UserInRole SelUserInRoles
        {
            get { return this._selUserInRoles; }
            set
            {

                _selUserInRoles = value;
                this.RaisePropertyChanged(() => this.SelUserInRoles);


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

        #region ViewModel 命令 -- 保存

        public DelegateCommand<object> SaveCommand { get; private set; }
        private void OnSave(object sender)
        {
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
            //设置保存、放弃修改按钮的可用性
            this._service.RejectChanges();
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
            LoadRoles();
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
