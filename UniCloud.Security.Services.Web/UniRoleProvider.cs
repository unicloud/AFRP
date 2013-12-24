using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Data.Odbc;
using System.Diagnostics;
using System.Linq;
using System.Web.Security;
using UniCloud.Security.Models;

namespace UniCloud.Security.Services.Web
{
    public sealed class UniRoleProvider : RoleProvider
    {
        private string eventSource = "UniRoleProvider";
        private string eventLog = "Application";
        private string exceptionMessage = "An exception occurred. Please check the Event Log.";

        private bool pWriteExceptionsToEventLog = false;

        public bool WriteExceptionsToEventLog
        {
            get { return pWriteExceptionsToEventLog; }
            set { pWriteExceptionsToEventLog = value; }
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            // 从web.config文件初始化相关值

            if (config == null)
                throw new ArgumentNullException("config");

            if (name == null || name.Length == 0)
                name = "UniRoleProvider";

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "UniCloud Role provider");
            }

            base.Initialize(name, config);

            if (config["applicationName"] == null || config["applicationName"].Trim() == "")
            {
                pApplicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
            }
            else
            {
                pApplicationName = config["applicationName"];
            }

            if (config["writeExceptionsToEventLog"] != null)
            {
                if (config["writeExceptionsToEventLog"].ToUpper() == "TRUE")
                {
                    pWriteExceptionsToEventLog = true;
                }
            }
        }


        private string pApplicationName;

        public override string ApplicationName
        {
            get { return pApplicationName; }
            set { pApplicationName = value; }
        }

        public bool creatRoleStatus; //创建用户角色状态，true表示创建成功，false表示创建失败
        public bool addUserToRoleStatus;//创建添加用户到角色状态，true表示创建成功，false表示创建失败

        #region 重写RoleProvider方法

        // RoleProvider.AddUsersToRoles
        public override void AddUsersToRoles(string[] usernames, string[] rolenames)
        {
            foreach (string rolename in rolenames)
            {
                if (!RoleExists(rolename))
                {
                    throw new ProviderException("Role name not found.");
                }
            }

            foreach (string username in usernames)
            {
                if (username.Contains(","))
                {
                    throw new ArgumentException("User names cannot contain commas.");
                }

                foreach (string rolename in rolenames)
                {
                    if (IsUserInRole(username, rolename))
                    {
                        throw new ProviderException("User is already in role.");
                    }
                }
            }

            using (var db = new SecurityEntities())
            {
                var users = db.Users.Where(u =>
                    u.Application.ApplicationName == Membership.ApplicationName &&
                    usernames.Contains(u.UserName)).ToList();
                var roles = db.Roles.Where(r =>
                    r.Application.ApplicationName == ApplicationName &&
                    rolenames.Contains(r.RoleName)).ToList();
                roles.ForEach(r => users.ForEach(u => db.UserInRoles.Add(new UserInRole { Users = u, Roles = r })));
                if (db.SaveChanges() > 0)
                {
                    addUserToRoleStatus = true;
                }
                else
                {
                    addUserToRoleStatus = false;
                }
            }
        }

        // RoleProvider.CreateRole
        public override void CreateRole(string rolename)
        {
            if (rolename.Contains(","))
            {
                throw new ArgumentException("Role names cannot contain commas.");
            }

            if (RoleExists(rolename))
            {
                throw new ProviderException("Role name already exists.");
            }

            using (var db = new SecurityEntities())
            {
                var app = db.Applications.SingleOrDefault(a => a.ApplicationName == ApplicationName);
                var role = new UniCloud.Security.Models.Roles { RoleName = rolename, Application = app };
                db.Roles.Add(role);
                app.FunctionItems.ToList().ForEach(f =>
                    db.FunctionsInRoles.Add(new FunctionsInRoles { Role = role, FunctionItem = f }));
                if (db.SaveChanges() > 0)
                {
                    creatRoleStatus = true;
                }
                else
                {
                    creatRoleStatus = false;
                }
            }

        }

        // RoleProvider.DeleteRole
        public override bool DeleteRole(string rolename, bool throwOnPopulatedRole)
        {
            if (!RoleExists(rolename))
            {
                throw new ProviderException("Role does not exist.");
            }

            if (throwOnPopulatedRole && GetUsersInRole(rolename).Length > 0)
            {
                throw new ProviderException("Cannot delete a populated role.");
            }

            using (var db = new SecurityEntities())
            {
                var role = db.Roles.SingleOrDefault(r =>
                    r.Application.ApplicationName == ApplicationName &&
                    r.RoleName == rolename);
                db.Roles.Remove(role);
                if (db.SaveChanges() > 0)
                    return true;
            }

            return false;
        }

        // RoleProvider.GetAllRoles
        public override string[] GetAllRoles()
        {
            using (var db = new SecurityEntities())
            {
                string[] allRoles = db.Roles.Where(r =>
                    r.Application.ApplicationName == ApplicationName).Select(r => r.RoleName).ToArray();
                return allRoles;
            }
        }

        // RoleProvider.GetRolesForUser
        public override string[] GetRolesForUser(string username)
        {
            using (var db = new SecurityEntities())
            {
                string[] rolesForUser = db.Users.Single(u =>
                    u.Application.ApplicationName == Membership.ApplicationName &&
                    u.UserName == username).UserInRoles
                    .Select(ur => ur.Roles.RoleName).ToArray();
                return rolesForUser;
            }
        }

        //RoleProvider.GetUsersInRole
        public override string[] GetUsersInRole(string rolename)
        {
            using (var db = new SecurityEntities())
            {
                string[] usersInRole = db.Roles.Single(r =>
                    r.Application.ApplicationName == ApplicationName &&
                    r.RoleName == rolename).UserInRoles
                    .Select(ur => ur.Users.UserName)
                    .ToArray();
                return usersInRole;
            }
        }

        //RoleProvider.IsUserInRole
        public override bool IsUserInRole(string username, string rolename)
        {
            using (var db = new SecurityEntities())
            {
                var userInRole = db.UserInRoles.Where(ur =>
                    ur.Users.Application.ApplicationName == Membership.ApplicationName &&
                    ur.Users.UserName == username && ur.Roles.RoleName == rolename);
                if (userInRole.Any())
                    return true;
                return false;
            }
        }

        //RoleProvider.RemoveUsersFromRoles
        public override void RemoveUsersFromRoles(string[] usernames, string[] rolenames)
        {
            foreach (string rolename in rolenames)
            {
                if (!RoleExists(rolename))
                {
                    throw new ProviderException("Role name not found.");
                }
            }

            foreach (string username in usernames)
            {
                foreach (string rolename in rolenames)
                {
                    if (!IsUserInRole(username, rolename))
                    {
                        throw new ProviderException("User is not in role.");
                    }
                }
            }

            using (var db = new SecurityEntities())
            {
                db.UserInRoles.Where(ur => usernames.Contains(ur.Users.UserName) &&
                    rolenames.Contains(ur.Roles.RoleName)).ToList().ForEach(ur => db.UserInRoles.Remove(ur));
                db.SaveChanges();
            }

        }

        // RoleProvider.RoleExists
        public override bool RoleExists(string rolename)
        {
            using (var db = new SecurityEntities())
            {
                var role = db.Roles.Where(r =>
                    r.Application.ApplicationName == ApplicationName &&
                    r.RoleName == rolename);
                if (role.Any())
                    return true;
            }

            return false;
        }

        // RoleProvider.FindUsersInRole
        public override string[] FindUsersInRole(string rolename, string usernameToMatch)
        {
            using (var db = new SecurityEntities())
            {
                string[] usersInRole = db.Roles.Single(r =>
                    r.Application.ApplicationName == ApplicationName &&
                    r.RoleName == rolename).UserInRoles
                    .Select(ur => ur.Users).Where(u =>
                        u.UserName == usernameToMatch)
                        .Select(u => u.UserName)
                        .ToArray();
                return usersInRole;
            }
        }

        #endregion

        // WriteToEventLog
        //   A helper function that writes exception detail to the event log. Exceptions
        // are written to the event log as a security measure to avoid private database
        // details from being returned to the browser. If a method does not return a status
        // or boolean indicating the action succeeded or failed, a generic exception is also 
        // thrown by the caller.
        private void WriteToEventLog(OdbcException e, string action)
        {
            EventLog log = new EventLog();
            log.Source = eventSource;
            log.Log = eventLog;

            string message = exceptionMessage + "\n\n";
            message += "Action: " + action + "\n\n";
            message += "Exception: " + e.ToString();

            log.WriteEntry(message);
        }
    }
}