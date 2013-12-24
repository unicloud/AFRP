using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.ServiceModel.DomainServices.EntityFramework;
using System.ServiceModel.DomainServices.Hosting;
using System.ServiceModel.DomainServices.Server;
using System.Web.Security;
using UniCloud.Security.Models;
namespace UniCloud.Security.Services.Web
{
    [EnableClientAccess]
    public class UniAuthDomainService : DbDomainService<SecurityEntities>
    {
        public UniAuthDomainService()
        {
            app = this.DbContext.Applications.SingleOrDefault(a => a.ApplicationName == _uniMembershipProvider.ApplicationName);
            _uniMembershipProvider.pPasswordFormat = MembershipPasswordFormat.Hashed;
        }

        UniMembershipProvider _uniMembershipProvider = new UniMembershipProvider();
        UniRoleProvider _uniRoleProvider = new UniRoleProvider();
        Applications app;

        /// <summary>
        /// 获取应用
        /// </summary>
        /// <returns></returns>
        public IQueryable<Applications> GetApplications()
        {
            return this.DbContext.Applications.OrderBy(o => o.ApplicationId);
        }

        /// <summary>
        /// 获取功能
        /// </summary>
        /// <returns></returns>
        public IQueryable<FunctionItem> GetFunctionItem()
        {
            return this.DbContext.FunctionItems.OrderBy(f => f.LevelCode);
        }

        #region 应用架构

        #region 用户

        /// <summary>
        /// 获取所有用户
        /// </summary>
        /// <returns></returns>
        public IQueryable<Users> GetUsers()
        {
            return this.DbContext.Users.OrderBy(u => u.UserName);
        }

        /// <summary>
        /// 创建用户
        /// </summary>
        /// <param name="user">用户</param>
        [Insert]
        public void InsertUsers(Users currentUser)
        {
            DateTime createDate = DateTime.Now;
            int passwordFormat;
            switch (_uniMembershipProvider.PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    passwordFormat = 0;
                    break;
                case MembershipPasswordFormat.Encrypted:
                    passwordFormat = 2;
                    break;
                case MembershipPasswordFormat.Hashed:
                    passwordFormat = 1;
                    break;
                default:
                    passwordFormat = 0;
                    break;
            }
            currentUser.LoweredUserName = currentUser.UserName.ToLower();
            currentUser.Password = string.IsNullOrEmpty(currentUser.Password) ? null : _uniMembershipProvider.EncodePassword(currentUser.Password);
            currentUser.PasswordFormat = passwordFormat;
            currentUser.PasswordAnswer = string.IsNullOrEmpty(currentUser.PasswordAnswer) ? null : _uniMembershipProvider.EncodePassword(currentUser.PasswordAnswer);
            currentUser.CreateDate = createDate;
            currentUser.IsLockedOut = false;
            currentUser.LastLockoutDate = createDate;
            currentUser.FailedPasswordAttemptCount = 0;
            currentUser.FailedPasswordAttemptWindowStart = createDate;
            currentUser.FailedPasswordAnswerAttemptCount = 0;
            currentUser.FailedPasswordAnswerAttemptWindowStart = createDate;
            currentUser.LastActivityDate = System.DateTime.Now;
            currentUser.LastLoginDate = System.DateTime.Now;
            currentUser.LastPasswordChangedDate = System.DateTime.Now;
            currentUser.ApplicationId = app.ApplicationId;
            //增加用户需要增加密码历史
            PasswordHistory ph = new PasswordHistory();
            ph.Users = currentUser;
            ph.Password = currentUser.Password;
            ph.CreateDate = createDate;
            this.DbContext.Insert<PasswordHistory>(ph);
            this.DbContext.Insert<Users>(currentUser);

        }
        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="user">用户</param>
        /// <returns></returns>
        [Delete]
        public void DeleteUsers(Users currentUser)
        {

            DeletePasswordHistoryFromUsers(currentUser); //删除用户的密码历史
            this.DbContext.Delete<Users>(currentUser);
        }

        /// <summary>
        /// 更新用户
        /// </summary>
        /// <param name="user"></param>
        [Update]
        public void UpdateUsers(Users currentUser)
        {

            var passwordHistories = this.DbContext.PasswordHistories.Where(p => p.UserId == currentUser.UserId);
            if (passwordHistories != null)
            {
                //判断用户密码是否经过修改，如果经过修改，则需要重新加密
                var currentPasswordHistory = passwordHistories.OrderByDescending(t => t.CreateDate).FirstOrDefault();
                if (currentPasswordHistory != null && currentPasswordHistory.Password != currentUser.Password)
                {
                    string encodePassword = string.IsNullOrEmpty(currentUser.Password) ? null : _uniMembershipProvider.EncodePassword(currentUser.Password);
                    if (currentPasswordHistory.Password != encodePassword)
                    {
                        PasswordHistory ph = new PasswordHistory();
                        ph.Password = encodePassword;
                        ph.Users = currentUser;
                        ph.CreateDate = System.DateTime.Now;
                        InsertPasswordHistory(ph);
                        currentUser.Password = encodePassword;
                    }
                    else
                    {
                        currentUser.Password = currentPasswordHistory.Password;
                    }
                }
                else
                {

                }
            }
            this.DbContext.Update<Users>(currentUser);

        }

        /// <summary>
        /// 删除角色中的用户
        /// </summary>
        /// <param name="userInRoles"></param>
        [Ignore]
        public void DeleteUsersFromRoles(IEnumerable<UserInRole> userInRoles)
        {
            userInRoles.ToList().ForEach(f => this.DbContext.Delete<UserInRole>(f));
        }

        /// <summary>
        /// 删除用户密码历史
        /// </summary>
        /// <param name="user"></param>
        [Ignore]
        public void DeletePasswordHistoryFromUsers(Users currentUser)
        {
            if (currentUser.PasswordHistories.Any())
            {
                currentUser.PasswordHistories.ToList().ForEach(ph =>
                {
                    this.DbContext.Delete<PasswordHistory>(ph);
                });
            }
        }
        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="user">用户</param>
        /// <param name="newPwd">新密码</param>
        /// <returns></returns>
        [Invoke]
        public void ChangePassword(Users currentUser, string oldPassword, string newPassword)
        {
            currentUser.Password = string.IsNullOrEmpty(newPassword) ? null : _uniMembershipProvider.EncodePassword(newPassword);
            currentUser.LastPasswordChangedDate = DateTime.Now;

            //修改密码需要增加密码历史
            PasswordHistory ph = new PasswordHistory();
            ph.Users = currentUser;
            ph.Password = currentUser.Password;
            ph.CreateDate = System.DateTime.Now;
            this.DbContext.Insert<PasswordHistory>(ph);
            UpdateUsers(currentUser);
        }

        /// <summary>
        /// 获取用户加密密码
        /// </summary>
        /// <param name="passwords"></param>
        /// <returns></returns>
        [Invoke]
        public List<string> GetUserEncodePassword(List<string> passwords)
        {
            List<string> encodePasswords = new List<string>();
            passwords.ForEach(f =>
                {
                    string encodePassword = string.IsNullOrEmpty(f) ? null : _uniMembershipProvider.EncodePassword(f);
                    encodePasswords.Add(encodePassword);
                }
                );
            return encodePasswords;
        }

        #endregion

        #region 用户密码

        /// <summary>
        /// 获取用户密码
        /// </summary>
        /// <returns></returns>
        public IQueryable<PasswordHistory> GetPasswordHistory()
        {
            return this.DbContext.PasswordHistories;
        }

        /// <summary>
        /// 增加用户密码
        /// </summary>
        /// <param name="currentPasswordHistory"></param>
        [Insert]
        public void InsertPasswordHistory(PasswordHistory currentPasswordHistory)
        {
            this.DbContext.Insert<PasswordHistory>(currentPasswordHistory);
        }

        /// <summary>
        /// 删除用户密码
        /// </summary>
        /// <param name="currentPasswordHistory"></param>
        [Delete]
        public void DeletePasswordHistory(PasswordHistory currentPasswordHistory)
        {
            this.DbContext.Delete<PasswordHistory>(currentPasswordHistory);
        }

        /// <summary>
        /// 更新用户密码
        /// </summary>
        /// <param name="currentPasswordHistory"></param>
        [Update]
        public void UpdatePasswordHistory(PasswordHistory currentPasswordHistory)
        {
            this.DbContext.Update<PasswordHistory>(currentPasswordHistory);
        }

        #endregion

        #region 角色

        /// <summary>
        /// 获取角色
        /// </summary>
        /// <returns></returns>
        public IQueryable<UniCloud.Security.Models.Roles> GetRoles()
        {
            return this.DbContext.Roles.Include(p => p.FunctionsInRoles).OrderBy(o => o.RoleName);
        }
        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        [Insert]
        public void InsertRoles(UniCloud.Security.Models.Roles currentRole)
        {
            this.DbContext.Insert<UniCloud.Security.Models.Roles>(currentRole);
        }
        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="role">角色</param>
        /// <returns></returns>
        [Delete]
        public void DeleteRoles(UniCloud.Security.Models.Roles currentRole)
        {
            this.DbContext.Delete<UniCloud.Security.Models.Roles>(currentRole);
        }

        /// <summary>
        /// 更新用户
        /// </summary>
        /// <param name="role"></param>
        [Update]
        public void UpdateRoles(UniCloud.Security.Models.Roles currentRole)
        {
            this.DbContext.Update<UniCloud.Security.Models.Roles>(currentRole);
        }
        #endregion

        #region 用户角色

        /// <summary>
        /// 获取所有用户角色
        /// </summary>
        /// <returns></returns>
        public IQueryable<UserInRole> GetUserInRole()
        {
            return this.DbContext.UserInRoles;
        }

        /// <summary>
        ///  创建用户角色
        /// </summary>
        /// <param name="currentUserInRole"></param>
        [Insert]
        public void InsertUserInRole(UserInRole currentUserInRole)
        {
            this.DbContext.Insert<UserInRole>(currentUserInRole);
        }
        /// <summary>
        /// 删除用户角色
        /// </summary>
        /// <param name="currentUserInRole"></param>
        [Delete]
        public void DeleteUserInRole(UserInRole currentUserInRole)
        {
            this.DbContext.Delete<UserInRole>(currentUserInRole);
        }

        /// <summary>
        /// 更新用户角色
        /// </summary>
        /// <param name="currentUserInRole"></param>
        [Update]
        public void UpdateUserInRole(UserInRole currentUserInRole)
        {
            this.DbContext.Update<UserInRole>(currentUserInRole);
        }

        #endregion

        #region 角色功能

        /// <summary>
        /// 获取角色功能
        /// </summary>
        /// <returns></returns>
        public IQueryable<FunctionsInRoles> GetFunctionsInRoles()
        {
            return this.DbContext.FunctionsInRoles;
        }

        /// <summary>
        /// 增加角色功能
        /// </summary>
        /// <param name="currentFunctionsInRoles"></param>
        [Insert]
        public void InsertFunctionsInRoles(FunctionsInRoles currentFunctionsInRoles)
        {
            this.DbContext.Insert<FunctionsInRoles>(currentFunctionsInRoles);
        }

        /// <summary>
        /// 删除角色功能
        /// </summary>
        /// <param name="currentFunctionsInRoles"></param>
        [Delete]
        public void DeleteFunctionsInRoles(FunctionsInRoles currentFunctionsInRoles)
        {
            this.DbContext.Delete<FunctionsInRoles>(currentFunctionsInRoles);
        }

        /// <summary>
        /// 更新角色功能
        /// </summary>
        /// <param name="role"></param>
        [Update]
        public void UpdateRoles(FunctionsInRoles currentFunctionsInRoles)
        {
            this.DbContext.Update<FunctionsInRoles>(currentFunctionsInRoles);
        }


        #endregion

        #endregion


        [Invoke]
        public void SetAppName(string userApp, string currentApp)
        {
            System.Web.Security.Membership.ApplicationName = userApp;
            System.Web.Security.Roles.ApplicationName = currentApp;
        }
    }
}