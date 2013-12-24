using MessageOperationLibrary.EventAggregatorRepository;
using MessageOperationLibrary.Events;
using Microsoft.Windows.Data.DomainServices;
using Ria.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ServiceModel.DomainServices.Client;
using UniCloud.Security.Models;
using UniCloud.Security.Services.Web;
using System.Linq;
using System.Text.RegularExpressions;
namespace UniCloud.Security.Services
{
    public interface IAuthServices
    {

        #region 加载完成属性

        bool AllApplicationsLoaded { get; }
        bool AllFunctionItemLoaded { get; }
        bool AllUsersLoaded { get; }
        bool AllRolesLoaded { get; }
        bool AllUserInRoleLoaded { get; }
        bool AllFunctionsInRolesLoaded { get; }

        #endregion

        #region 加载方法

        //是否存在当前角色
        bool IsExitRole(Roles roles);
        //是否存在当前用户
        bool IsExitUsers(Users users);

        #endregion

        #region 公共

        UniAuthDomainContext Context { get; }
        EntityContainer EntityContainer { get; }
        void SubmitChanges();
        void SubmitChanges(Action<ServiceSubmitChangesResult> callback, object state);
        void SubmitChanges(string success, string fail, Action<ServiceSubmitChangesResult> callback, object state);
        void RejectChanges();

        #endregion

        #region 应用架构

        void LoadApplications(QueryBuilder<Applications> query, Action<ServiceLoadResult<Applications>> callback, object state);//加载应用
        void LoadFunctionItem(QueryBuilder<FunctionItem> query, Action<ServiceLoadResult<FunctionItem>> callback, object state);//加载功能
        void LoadUsers(QueryBuilder<Users> query, Action<ServiceLoadResult<Users>> callback, object state);//加载所有用户
        void LoadRoles(QueryBuilder<Roles> query, Action<ServiceLoadResult<Roles>> callback, object state);//加载所有角色
        void LoadUserInRole(QueryBuilder<UserInRole> query, Action<ServiceLoadResult<UserInRole>> callback, object state);//加载所有用户角色
        void LoadFunctionsInRoles(QueryBuilder<FunctionsInRoles> query, Action<ServiceLoadResult<FunctionsInRoles>> callback, object state);//加载所有角色功能

        void GetUserEncodePassword(List<string> passwords, Action<InvokeCompletedResult> callback, object state); //获取用户密码

        #endregion

        #region 业务逻辑

        IDictionary<string, bool> GetViewButtonValidity(string viewType);

        #endregion

    }

    [Export(typeof(IAuthServices))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AuthServices : IAuthServices, IPartImportsSatisfiedNotification
    {
        private readonly UniAuthDomainContext _context = new UniAuthDomainContext();

        #region 属性绑定
        //是否存在当前角色
        public bool IsExitRole(Roles roles)
        {
            if (!this.EntityContainer.HasChanges)
            {
                return this.EntityContainer.GetEntitySet<Roles>().Any(p => p == roles);
            }
            var isChange = true;
            this.EntityContainer.GetChanges().ToList().ForEach(p =>
                {
                    if (p.EntityState == EntityState.New && p is Roles && ((p as Roles) == roles))
                    {
                        isChange = false;
                    }
                    if (p.EntityState == EntityState.Modified && p is Roles && ((p as Roles) == roles))
                    {
                        isChange = true;
                    }
                });
            return isChange;
        }

        //是否存在当前用户
        public bool IsExitUsers(Users users)
        {
            if (!this.EntityContainer.HasChanges)
            {
                return this.EntityContainer.GetEntitySet<Users>().Any(p => p.UserId == users.UserId);
            }
            var isChange = true;
            this.EntityContainer.GetChanges().ToList().ForEach(p =>
                {
                    if (p.EntityState == EntityState.New && p is Users && ((p as Users).UserId == users.UserId))
                    {
                        isChange = false;
                    }
                    if (p.EntityState == EntityState.Modified && p is Users && ((p as Users).UserId == users.UserId))
                    {
                        isChange = true;
                    }
                });
            return isChange;
        }

        #endregion

        #region 加载完成属性

        private bool _allApplicationsLoaded, _allFunctionItemLoaded, _allUsersLoaded, _allRolesLoaded, _allUserInRoleLoaded, _allFunctionsInRolesLoaded;

        public bool AllApplicationsLoaded { get { return _allApplicationsLoaded; } }
        public bool AllFunctionItemLoaded { get { return _allFunctionItemLoaded; } }
        public bool AllUsersLoaded { get { return _allUsersLoaded; } }
        public bool AllRolesLoaded { get { return _allRolesLoaded; } }
        public bool AllUserInRoleLoaded { get { return _allUserInRoleLoaded; } }
        public bool AllFunctionsInRolesLoaded { get { return _allFunctionsInRolesLoaded; } }

        #endregion

        #region  公共

        public UniAuthDomainContext Context
        {
            get { return this._context; }
        }

        public EntityContainer EntityContainer
        {
            get { return this._context.EntityContainer; }
        }

        /// <summary>
        /// 保存实体变化
        /// </summary>
        public void SubmitChanges()
        {
            this._context.SubmitChanges();
        }

        /// <summary>
        /// 保存实体变化
        /// </summary>
        /// <param name="callback">回调方法</param>
        /// <param name="state">状态</param>
        public void SubmitChanges(Action<ServiceSubmitChangesResult> callback, object state)
        {
            this._context.SubmitChanges(sm => callback(SubmitResult(sm)), state);
        }

        /// <summary>
        /// 保存实体变化
        /// </summary>
        /// <param name="success">保存成功所显示消息</param>
        /// <param name="fail">保存失败所显示消息</param>
        /// <param name="callback">回调方法</param>
        /// <param name="state">状态</param>
        public void SubmitChanges(string success, string fail, Action<ServiceSubmitChangesResult> callback, object state)
        {

            this._context.SubmitChanges(sm =>
            {
                if (sm.Error != null)
                {
                    MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(new MessageEvent { Message = fail, MessageType = MessageType.Fail });
                }
                else if (!sm.IsCanceled)
                {
                    MessageEventAggregatorRepository.EventAggregator.Publish<MessageEvent>(new MessageEvent { Message = success, MessageType = MessageType.Success });
                }
                callback(SubmitResult(sm));
            }, state);
        }

        public void RejectChanges()
        {
            this._context.RejectChanges();
        }


        private void Load<T>(EntityQuery<T> query, Action<LoadOperation<T>> callback, object state) where T : Entity
        {
            this._context.Load(query, callback, state);
        }

        private ServiceLoadResult<T> CreateResult<T>(LoadOperation<T> op, bool returnEditableCollection = false) where T : Entity
        {
            if (op.HasError)
            {
                op.MarkErrorAsHandled();
            }
            return new ServiceLoadResult<T>(
                returnEditableCollection ? new EntityList<T>(this.EntityContainer.GetEntitySet<T>(), op.Entities) : op.Entities,
                op.TotalEntityCount,
                op.ValidationErrors,
                op.Error,
                op.IsCanceled,
                op.UserState);
        }

        private static ServiceLoadResult<T> CreateResult<T>(IEnumerable<T> entities) where T : Entity
        {
            return new ServiceLoadResult<T>(
                entities,
                null);
        }

        private static ServiceSubmitChangesResult SubmitResult(SubmitOperation sm)
        {
            if (sm.HasError)
            {
                sm.MarkErrorAsHandled();
            }
            return new ServiceSubmitChangesResult(sm.ChangeSet, sm.EntitiesInError, sm.Error, sm.CanCancel, sm.UserState);
        }

        private static InvokeCompletedResult InvokeResult(InvokeOperation io)
        {
            if (io.HasError)
            {
                io.MarkErrorAsHandled();
            }
            return new InvokeCompletedResult(io.Value, io.ValidationErrors);
        }

        #endregion

        #region 应用架构

        /// <summary>
        /// 加载应用
        /// </summary>
        /// <param name="query"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public void LoadApplications(QueryBuilder<Applications> query, Action<ServiceLoadResult<Applications>> callback, object state)
        {
            if (!this._allApplicationsLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetApplicationsQuery()), lo =>
                {
                    this._allApplicationsLoaded = true;
                    callback(this.CreateResult(lo));
                }, state);
            }
            else
            {
                callback(CreateResult(this.EntityContainer.GetEntitySet<Applications>()));
            }
        }

        /// <summary>
        /// 加载功能
        /// </summary>
        /// <param name="query"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public void LoadFunctionItem(QueryBuilder<FunctionItem> query, Action<ServiceLoadResult<FunctionItem>> callback, object state)
        {
            if (!this._allFunctionItemLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetFunctionItemQuery()), lo =>
                {
                    this._allFunctionItemLoaded = true;
                    callback(this.CreateResult(lo));
                }, state);
            }
            else
            {
                callback(CreateResult(this.EntityContainer.GetEntitySet<FunctionItem>()));
            }
        }

        /// <summary>
        /// 加载用户
        /// </summary>
        /// <param name="query">查询集合</param>
        /// <param name="callback">回调函数</param>
        /// <param name="state">状态</param>
        public void LoadUsers(QueryBuilder<Users> query, Action<ServiceLoadResult<Users>> callback, object state)
        {
            if (!this._allUsersLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetUsersQuery()), lo =>
                {
                    this._allUsersLoaded = true;

                    callback(this.CreateResult(lo));
                }, state);
            }
            else
            {
                callback(CreateResult(this.EntityContainer.GetEntitySet<Users>()));
            }
        }

        /// <summary>
        /// 加载角色
        /// </summary>
        /// <param name="query"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public void LoadRoles(QueryBuilder<Roles> query, Action<ServiceLoadResult<Roles>> callback, object state)
        {
            if (!this._allRolesLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetRolesQuery()), lo =>
                {
                    this._allRolesLoaded = true;
                    callback(this.CreateResult(lo));
                }, state);
            }
            else
            {
                callback(CreateResult(this.EntityContainer.GetEntitySet<Roles>()));
            }
        }

        /// <summary>
        /// 加载用户角色
        /// </summary>
        /// <param name="query"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public void LoadFunctionsInRoles(QueryBuilder<FunctionsInRoles> query, Action<ServiceLoadResult<FunctionsInRoles>> callback, object state)
        {
            if (!this._allFunctionsInRolesLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetFunctionsInRolesQuery()), lo =>
                {
                    this._allFunctionsInRolesLoaded = true;
                    callback(this.CreateResult(lo));
                }, state);
            }
            else
            {
                callback(CreateResult(this.EntityContainer.GetEntitySet<FunctionsInRoles>()));
            }
        }

        /// <summary>
        /// 加载用户角色
        /// </summary>
        /// <param name="query"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public void LoadUserInRole(QueryBuilder<UserInRole> query, Action<ServiceLoadResult<UserInRole>> callback, object state)
        {
            if (!this._allUserInRoleLoaded)
            {
                this.Load(query.ApplyTo(this._context.GetUserInRoleQuery()), lo =>
                {
                    this._allUserInRoleLoaded = true;
                    callback(this.CreateResult(lo));
                }, state);
            }
            else
            {
                callback(CreateResult(this.EntityContainer.GetEntitySet<UserInRole>()));
            }
        }


        /// <summary>
        /// 获取用户加密密码
        /// </summary>
        /// <param name="passwords"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public void GetUserEncodePassword(List<string> passwords, Action<InvokeCompletedResult> callback, object state)
        {
            this._context.GetUserEncodePassword(passwords, io => callback(InvokeResult(io)), state);
        }
        #endregion

        #region 业务逻辑

        public IDictionary<string, bool> GetViewButtonValidity(string viewType)
        {
            var viewName = Regex.Replace(viewType, @".*\.(\w*)", "$1");
            return EntityContainer.GetEntitySet<FunctionItem>()
                                  .Where(f => f.ViewName == viewName)
                                  .SelectMany(f => f.SubItems)
                                  .Where(sf => sf.IsButton)
                                  .ToDictionary(k => k.Name,
                                                v =>
                                                StatusData.curUser.UserInRoles.SelectMany(u => u.Roles.FunctionsInRoles)
                                                          .Any(uf => uf.FunctionItemID == v.FunctionItemID && uf.IsValid));
        }

        #endregion

        #region IPartImportsSatisfiedNotification 成员

        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            LoadApplications(new QueryBuilder<Applications>(), lo => { }, null);
            LoadFunctionItem(new QueryBuilder<FunctionItem>(), lo => { }, null);
            LoadUsers(new QueryBuilder<Users>(), lo => { }, null);
            LoadRoles(new QueryBuilder<Roles>(), lo => { }, null);
            LoadUserInRole(new QueryBuilder<UserInRole>(), lo => { }, null);
            LoadFunctionsInRoles(new QueryBuilder<FunctionsInRoles>(), lo => { }, null);
        }

        #endregion

    }
}
