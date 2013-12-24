using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Configuration;
using System.Web.Security;
using UniCloud.Security.Models;

namespace UniCloud.Security.Services.Web
{
    public sealed class UniMembershipProvider : MembershipProvider
    {

        public UniMembershipProvider()
        {
            pApplicationName = "unicloud";
            pName = "UniMembershipProvider";
            Configuration cfg = WebConfigurationManager.OpenWebConfiguration(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            machineKey = (MachineKeySection)cfg.GetSection("system.web/machineKey");
        }
        private int newPasswordLength = 8;
        private string eventSource = "UniMembershipProvider";
        private string eventLog = "Application";
        private string exceptionMessage = "An exception occurred. Please check the Event Log.";

        private MachineKeySection machineKey;

        private bool pWriteExceptionsToEventLog;

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
                name = "UniMembershipProvider";

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "UniCloud Membership provider");
            }

            base.Initialize(name, config);

            pApplicationName = GetConfigValue(config["applicationName"], System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            pMaxInvalidPasswordAttempts = Convert.ToInt32(GetConfigValue(config["maxInvalidPasswordAttempts"], "5"));
            pPasswordAttemptWindow = Convert.ToInt32(GetConfigValue(config["passwordAttemptWindow"], "10"));
            pMinRequiredNonAlphanumericCharacters = Convert.ToInt32(GetConfigValue(config["minRequiredNonAlphanumericCharacters"], "1"));
            pMinRequiredPasswordLength = Convert.ToInt32(GetConfigValue(config["minRequiredPasswordLength"], "7"));
            pPasswordStrengthRegularExpression = Convert.ToString(GetConfigValue(config["passwordStrengthRegularExpression"], ""));
            pEnablePasswordReset = Convert.ToBoolean(GetConfigValue(config["enablePasswordReset"], "true"));
            pEnablePasswordRetrieval = Convert.ToBoolean(GetConfigValue(config["enablePasswordRetrieval"], "true"));
            pRequiresQuestionAndAnswer = Convert.ToBoolean(GetConfigValue(config["requiresQuestionAndAnswer"], "false"));
            pRequiresUniqueEmail = Convert.ToBoolean(GetConfigValue(config["requiresUniqueEmail"], "true"));
            pWriteExceptionsToEventLog = Convert.ToBoolean(GetConfigValue(config["writeExceptionsToEventLog"], "true"));

            string temp_format = config["passwordFormat"];
            if (temp_format == null)
            {
                temp_format = "Hashed";
            }

            switch (temp_format)
            {
                case "Hashed":
                    pPasswordFormat = MembershipPasswordFormat.Hashed;
                    break;
                case "Encrypted":
                    pPasswordFormat = MembershipPasswordFormat.Encrypted;
                    break;
                case "Clear":
                    pPasswordFormat = MembershipPasswordFormat.Clear;
                    break;
                default:
                    throw new ProviderException("Password format not supported.");
            }

            Configuration cfg = WebConfigurationManager.OpenWebConfiguration(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            machineKey = (MachineKeySection)cfg.GetSection("system.web/machineKey");

            if (machineKey.ValidationKey.Contains("AutoGenerate"))
                if (PasswordFormat != MembershipPasswordFormat.Clear)
                    throw new ProviderException("Hashed or Encrypted passwords " +
                                                "are not supported with auto-generated keys.");
        }

        //从Config配置文件获取值的方法
        private string GetConfigValue(string configValue, string defaultValue)
        {
            if (String.IsNullOrEmpty(configValue))
                return defaultValue;

            return configValue;
        }

        private string pApplicationName;
        private bool pEnablePasswordReset;
        private bool pEnablePasswordRetrieval;
        private bool pRequiresQuestionAndAnswer;
        private bool pRequiresUniqueEmail;
        private int pMaxInvalidPasswordAttempts;
        private int pPasswordAttemptWindow;
        private string pName;
        public MembershipPasswordFormat pPasswordFormat;

        public override string ApplicationName
        {
            get { return pApplicationName; }
            set { pApplicationName = value; }
        }

        public override bool EnablePasswordReset
        {
            get { return pEnablePasswordReset; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return pEnablePasswordRetrieval; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return pRequiresQuestionAndAnswer; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return pRequiresUniqueEmail; }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return pMaxInvalidPasswordAttempts; }
        }

        public override int PasswordAttemptWindow
        {
            get { return pPasswordAttemptWindow; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { return pPasswordFormat; }
        }

        private int pMinRequiredNonAlphanumericCharacters;

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return pMinRequiredNonAlphanumericCharacters; }
        }

        private int pMinRequiredPasswordLength;

        public override int MinRequiredPasswordLength
        {
            get { return pMinRequiredPasswordLength; }
        }

        private string pPasswordStrengthRegularExpression;

        public override string PasswordStrengthRegularExpression
        {
            get { return pPasswordStrengthRegularExpression; }
        }

        public override string Name
        {
            get
            {
                return this.pName;
            }
        }

        #region 重写MembershipProvider方法

        // MembershipProvider.ChangePassword
        public override bool ChangePassword(string username, string oldPwd, string newPwd)
        {
            if (!ValidateUser(username, oldPwd))
                return false;

            ValidatePasswordEventArgs args =
              new ValidatePasswordEventArgs(username, newPwd, true);

            OnValidatingPassword(args);

            if (args.Cancel)
                if (args.FailureInformation != null)
                    throw args.FailureInformation;
                else
                    throw new MembershipPasswordException("Change password canceled due to new password validation failure.");

            using (var db = new SecurityEntities())
            {
                var user = db.Users.SingleOrDefault(u =>
                    u.Application.ApplicationName == pApplicationName && u.UserName == username);

                user.Password = EncodePassword(newPwd);
                user.LastPasswordChangedDate = DateTime.Now;
                if (db.SaveChanges() > 0)
                    return true;
            }

            return false;
        }

        // MembershipProvider.ChangePasswordQuestionAndAnswer
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPwdQuestion, string newPwdAnswer)
        {
            if (!ValidateUser(username, password))
                return false;

            using (var db = new SecurityEntities())
            {
                var user = db.Users.SingleOrDefault(u =>
                    u.Application.ApplicationName == pApplicationName && u.UserName == username);

                user.PasswordQuestion = newPwdQuestion;
                user.PasswordAnswer = EncodePassword(newPwdAnswer);
                if (db.SaveChanges() > 0)
                    return true;
            }

            return false;
        }

        // MembershipProvider.CreateUser
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, password, true);

            OnValidatingPassword(args);

            if (args.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if (RequiresUniqueEmail && GetUserNameByEmail(email) != "")
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            MembershipUser u = GetUser(username, false);

            if (u == null)
            {
                DateTime createDate = DateTime.Now;
                int passwordFormat;
                switch (pPasswordFormat)
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

                using (var db = new SecurityEntities())
                {
                    var app = db.Applications.SingleOrDefault(a => a.ApplicationName == pApplicationName);

                    var user = new Users
                    {
                        UserName = username,
                        LoweredUserName = username.ToLower(),
                        Password = EncodePassword(password),
                        Email = email,
                        PasswordFormat = passwordFormat,
                        PasswordQuestion = passwordQuestion,
                        PasswordAnswer = EncodePassword(passwordAnswer),
                        IsApproved = isApproved,
                        CreateDate = createDate,
                        IsLockedOut = false,
                        LastLockoutDate = createDate,
                        FailedPasswordAttemptCount = 0,
                        FailedPasswordAttemptWindowStart = createDate,
                        FailedPasswordAnswerAttemptCount = 0,
                        FailedPasswordAnswerAttemptWindowStart = createDate,
                        LastActivityDate = System.DateTime.Now,
                        LastLoginDate = System.DateTime.Now,
                        LastPasswordChangedDate = System.DateTime.Now,
                        ApplicationId = app.ApplicationId
                    };
                    db.Users.Add(user);
                    status = MembershipCreateStatus.Success;
                    if (db.SaveChanges() > 0)
                        status = MembershipCreateStatus.Success;
                    else
                        status = MembershipCreateStatus.UserRejected;

                }

                return GetUser(username, false);
            }
            else
            {
                status = MembershipCreateStatus.DuplicateUserName;
            }

            return null;
        }

        // MembershipProvider.DeleteUser
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            using (var db = new SecurityEntities())
            {
                var user = db.Users.SingleOrDefault(u =>
                    u.Application.ApplicationName == pApplicationName &&
                    u.UserName == username);

                db.Users.Remove(user);
                if (deleteAllRelatedData)
                {
                    // Process commands to delete all data for the user in the database.

                }
                db.SaveChanges();
                if (db.SaveChanges() > 0)
                    return true;
            }

            return false;
        }

        // MembershipProvider.GetAllUsers
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            totalRecords = 0;
            MembershipUserCollection users = new MembershipUserCollection();

            using (var db = new SecurityEntities())
            {
                var allUser = db.Users.Where(u => u.Application.ApplicationName == pApplicationName)
                    .OrderBy(u => u.UserName);
                totalRecords = allUser.Count();
                if (!allUser.Any())
                    return users;

                int startIndex = pageSize * pageIndex;

                allUser.Skip(startIndex).Take(pageSize).Select(u => new MembershipUser(
                    this.Name,
                    u.UserName,
                    u.UserId,
                    u.Email,
                    u.PasswordQuestion,
                    u.Comment,
                    u.IsApproved,
                    u.IsLockedOut,
                    u.CreateDate,
                    u.LastLoginDate,
                    u.LastActivityDate,
                    u.LastPasswordChangedDate,
                    u.LastLockoutDate)).ToList().ForEach(u => users.Add(u));
            }

            return users;
        }

        // MembershipProvider.GetNumberOfUsersOnline
        public override int GetNumberOfUsersOnline()
        {

            TimeSpan onlineSpan = new TimeSpan(0, System.Web.Security.Membership.UserIsOnlineTimeWindow, 0);
            DateTime compareTime = DateTime.Now.Subtract(onlineSpan);
            using (var db = new SecurityEntities())
            {
                return db.Users.Where(u =>
                    u.Application.ApplicationName == pApplicationName && u.LastActivityDate > compareTime)
                    .Count();
            }
        }

        // MembershipProvider.GetPassword
        public override string GetPassword(string username, string answer)
        {
            if (!EnablePasswordRetrieval)
            {
                throw new ProviderException("Password Retrieval Not Enabled.");
            }

            if (PasswordFormat == MembershipPasswordFormat.Hashed)
            {
                throw new ProviderException("Cannot retrieve Hashed passwords.");
            }

            string password = "";
            string passwordAnswer = "";

            using (var db = new SecurityEntities())
            {
                var user = db.Users.SingleOrDefault(u =>
                    u.Application.ApplicationName == pApplicationName &&
                    u.UserName == username);

                if (user != null)
                {
                    if (user.IsLockedOut)
                        throw new MembershipPasswordException("The supplied user is locked out.");
                    password = user.Password;
                    passwordAnswer = user.PasswordAnswer;
                }
                else
                    throw new MembershipPasswordException("The supplied user name is not found.");
            }

            if (RequiresQuestionAndAnswer && !CheckPassword(answer, passwordAnswer))
            {
                UpdateFailureCount(username, "passwordAnswer");

                throw new MembershipPasswordException("Incorrect password answer.");
            }


            if (PasswordFormat == MembershipPasswordFormat.Encrypted)
            {
                password = UnEncodePassword(password);
            }

            return password;
        }

        // MembershipProvider.GetUser(string, bool)
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            MembershipUser mu = null;

            using (var db = new SecurityEntities())
            {
                var user = db.Users.Where(u =>
                    u.Application.ApplicationName == pApplicationName && u.UserName == username)
                    .FirstOrDefault();

                if (user != null)
                {
                    mu = new MembershipUser(
                    this.Name,
                    user.UserName,
                    user.UserId,
                    user.Email,
                    user.PasswordQuestion,
                    user.Comment,
                    user.IsApproved,
                    user.IsLockedOut,
                    user.CreateDate,
                    user.LastLoginDate,
                    user.LastActivityDate,
                    user.LastPasswordChangedDate,
                    user.LastLockoutDate);
                }
                if (userIsOnline)
                {
                    user.LastActivityDate = DateTime.Now;
                    db.SaveChanges();
                }
            }

            return mu;
        }

        // MembershipProvider.GetUser(object, bool)
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            MembershipUser mu = null;

            using (var db = new SecurityEntities())
            {
                var user = db.Users.Where(u => u.UserId == (int)providerUserKey).FirstOrDefault();

                if (user != null)
                {
                    mu = new MembershipUser(
                    this.Name,
                    user.UserName,
                    user.UserId,
                    user.Email,
                    user.PasswordQuestion,
                    user.Comment,
                    user.IsApproved,
                    user.IsLockedOut,
                    user.CreateDate,
                    user.LastLoginDate,
                    user.LastActivityDate,
                    user.LastPasswordChangedDate,
                    user.LastLockoutDate);
                }
                if (userIsOnline)
                {
                    user.LastActivityDate = DateTime.Now;
                    db.SaveChanges();
                }
            }

            return mu;
        }

        // MembershipProvider.UnlockUser
        public override bool UnlockUser(string username)
        {
            using (var db = new SecurityEntities())
            {
                var user = db.Users.SingleOrDefault(u =>
                    u.Application.ApplicationName == pApplicationName &&
                    u.UserName == username);

                user.IsLockedOut = false;
                user.LastLockoutDate = DateTime.Now;
                if (db.SaveChanges() > 0)
                    return true;
            }

            return false;
        }

        // MembershipProvider.GetUserNameByEmail
        public override string GetUserNameByEmail(string email)
        {
            string username = "";

            using (var db = new SecurityEntities())
            {
                username = db.Users.Where(u =>
                    u.Application.ApplicationName == pApplicationName && u.Email == email)
                    .Select(u => u.UserName).FirstOrDefault();
            }

            if (username == null)
                username = "";

            return username;
        }

        // MembershipProvider.ResetPassword
        public override string ResetPassword(string username, string answer)
        {
            if (!EnablePasswordReset)
            {
                throw new NotSupportedException("Password reset is not enabled.");
            }

            if (answer == null && RequiresQuestionAndAnswer)
            {
                UpdateFailureCount(username, "passwordAnswer");

                throw new ProviderException("Password answer required for password reset.");
            }

            string newPassword =
              System.Web.Security.Membership.GeneratePassword(newPasswordLength, MinRequiredNonAlphanumericCharacters);

            ValidatePasswordEventArgs args =
              new ValidatePasswordEventArgs(username, newPassword, true);

            OnValidatingPassword(args);

            if (args.Cancel)
                if (args.FailureInformation != null)
                    throw args.FailureInformation;
                else
                    throw new MembershipPasswordException("Reset password canceled due to password validation failure.");

            string passwordAnswer = "";

            using (var db = new SecurityEntities())
            {
                var user = db.Users.SingleOrDefault(u =>
                    u.Application.ApplicationName == pApplicationName &&
                    u.UserName == username);

                passwordAnswer = user.PasswordAnswer;
                if (RequiresQuestionAndAnswer && !CheckPassword(answer, passwordAnswer))
                {
                    UpdateFailureCount(username, "passwordAnswer");

                    throw new MembershipPasswordException("Incorrect password answer.");
                }

                user.Password = EncodePassword(newPassword);
                user.LastPasswordChangedDate = DateTime.Now;
                if (db.SaveChanges() > 0)
                    return newPassword;
                else
                    throw new MembershipPasswordException("User not found, or user is locked out. Password not Reset.");
            }

        }

        // MembershipProvider.UpdateUser
        public override void UpdateUser(MembershipUser user)
        {
            using (var db = new SecurityEntities())
            {
                var mu = db.Users.SingleOrDefault(u =>
                    u.Application.ApplicationName == pApplicationName &&
                    u.UserName == user.UserName);

                mu.Email = user.Email;
                mu.Comment = user.Comment;
                mu.IsApproved = user.IsApproved;
                db.SaveChanges();
            }
        }

        // MembershipProvider.ValidateUser
        public override bool ValidateUser(string username, string password)
        {
            bool isValid = false;
            bool isApproved = false;
            string pwd = "";

            using (var db = new SecurityEntities())
            {
                var user = db.Users.SingleOrDefault(u =>
                    u.Application.ApplicationName == pApplicationName &&
                    u.UserName == username &&
                    !u.IsLockedOut);

                if (user != null)
                {
                    pwd = user.Password;
                    isApproved = user.IsApproved;
                }
                else
                    return false;

                if (CheckPassword(password, pwd))
                {
                    if (isApproved)
                    {
                        isValid = true;
                        user.LastLoginDate = DateTime.Now;
                        db.SaveChanges();
                    }
                }
                else
                    UpdateFailureCount(username, "password");

                return isValid;
            }

        }

        // UpdateFailureCount
        //   A helper method that performs the checks and updates associated with
        // password failure tracking.
        private void UpdateFailureCount(string username, string failureType)
        {
            DateTime windowStart = new DateTime();
            int failureCount = 0;

            using (var db = new SecurityEntities())
            {
                var user = db.Users.SingleOrDefault(u =>
                    u.Application.ApplicationName == pApplicationName &&
                    u.UserName == username);

                if (user != null)
                {
                    if (failureType == "password")
                    {
                        failureCount = user.FailedPasswordAttemptCount;
                        windowStart = user.FailedPasswordAttemptWindowStart;
                    }

                    if (failureType == "passwordAnswer")
                    {
                        failureCount = user.FailedPasswordAnswerAttemptCount;
                        windowStart = user.FailedPasswordAnswerAttemptWindowStart;
                    }
                }

                DateTime windowEnd = windowStart.AddMinutes(PasswordAttemptWindow);

                if (failureCount == 0 || DateTime.Now > windowEnd)
                {
                    if (failureType == "password")
                    {
                        user.FailedPasswordAttemptCount = 1;
                        user.FailedPasswordAttemptWindowStart = DateTime.Now;
                    }
                    if (failureType == "passwordAnswer")
                    {
                        user.FailedPasswordAnswerAttemptCount = 1;
                        user.FailedPasswordAnswerAttemptWindowStart = DateTime.Now;
                    }
                    db.SaveChanges();
                }
                else
                {
                    if (failureCount++ >= MaxInvalidPasswordAttempts)
                    {
                        user.IsLockedOut = true;
                        user.LastLockoutDate = DateTime.Now;
                        db.SaveChanges();
                    }
                    else
                    {
                        if (failureType == "password")
                            user.FailedPasswordAttemptCount = failureCount;
                        if (failureType == "passwordAnswer")
                            user.FailedPasswordAnswerAttemptCount = failureCount;
                        db.SaveChanges();
                    }
                }
            }

        }

        // CheckPassword
        //   Compares password values based on the MembershipPasswordFormat.
        private bool CheckPassword(string password, string dbpassword)
        {
            string pass1 = password;
            string pass2 = dbpassword;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Encrypted:
                    pass2 = UnEncodePassword(dbpassword);
                    break;
                case MembershipPasswordFormat.Hashed:
                    pass1 = EncodePassword(password);
                    break;
                default:
                    break;
            }

            if (pass1 == pass2)
            {
                return true;
            }

            return false;
        }

        // EncodePassword
        //   Encrypts, Hashes, or leaves the password clear based on the PasswordFormat.
        public string EncodePassword(string password)
        {
            string encodedPassword = password;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    encodedPassword =
                      Convert.ToBase64String(EncryptPassword(Encoding.Unicode.GetBytes(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    HMACSHA1 hash = new HMACSHA1();
                    hash.Key = HexToByte(machineKey.ValidationKey);
                    encodedPassword =
                      Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(password)));
                    break;
                default:
                    throw new ProviderException("Unsupported password format.");
            }

            return encodedPassword;
        }

        // UnEncodePassword
        //   Decrypts or leaves the password clear based on the PasswordFormat.
        private string UnEncodePassword(string encodedPassword)
        {
            string password = encodedPassword;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    password =
                      Encoding.Unicode.GetString(DecryptPassword(Convert.FromBase64String(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    throw new ProviderException("Cannot unencode a hashed password.");
                default:
                    throw new ProviderException("Unsupported password format.");
            }

            return password;
        }

        // HexToByte
        //   Converts a hexadecimal string to a byte array. Used to convert encryption
        // key values from the configuration.
        private byte[] HexToByte(string hexString)
        {
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        // MembershipProvider.FindUsersByName
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            totalRecords = 0;
            MembershipUserCollection users = new MembershipUserCollection();

            using (var db = new SecurityEntities())
            {
                var selUsers = db.Users.Where(u =>
                    u.Application.ApplicationName == pApplicationName &&
                    u.UserName == usernameToMatch)
                    .OrderBy(u => u.UserName);
                totalRecords = selUsers.Count();
                if (!selUsers.Any())
                    return users;

                int startIndex = pageSize * pageIndex;

                selUsers.Skip(startIndex).Take(pageSize).Select(u => new MembershipUser(
                    this.Name,
                    u.UserName,
                    u.UserId,
                    u.Email,
                    u.PasswordQuestion,
                    u.Comment,
                    u.IsApproved,
                    u.IsLockedOut,
                    u.CreateDate,
                    u.LastLoginDate,
                    u.LastActivityDate,
                    u.LastPasswordChangedDate,
                    u.LastLockoutDate)).ToList().ForEach(u => users.Add(u));
            }

            return users;
        }

        // MembershipProvider.FindUsersByEmail
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            totalRecords = 0;
            MembershipUserCollection users = new MembershipUserCollection();

            using (var db = new SecurityEntities())
            {
                var selUsers = db.Users.Where(u =>
                    u.Application.ApplicationName == pApplicationName &&
                    u.Email.ToLower() == emailToMatch.ToLower())
                    .OrderBy(u => u.UserName);
                totalRecords = selUsers.Count();
                if (!selUsers.Any())
                    return users;

                int startIndex = pageSize * pageIndex;

                selUsers.Skip(startIndex).Take(pageSize).Select(u => new MembershipUser(
                    this.Name,
                    u.UserName,
                    u.UserId,
                    u.Email,
                    u.PasswordQuestion,
                    u.Comment,
                    u.IsApproved,
                    u.IsLockedOut,
                    u.CreateDate,
                    u.LastLoginDate,
                    u.LastActivityDate,
                    u.LastPasswordChangedDate,
                    u.LastLockoutDate)).ToList().ForEach(u => users.Add(u));
            }

            return users;
        }

        #endregion

        // WriteToEventLog
        //   A helper function that writes exception detail to the event log. Exceptions
        // are written to the event log as a security measure to avoid private database
        // details from being returned to the browser. If a method does not return a status
        // or boolean indicating the action succeeded or failed, a generic exception is also 
        // thrown by the caller.
        private void WriteToEventLog(Exception e, string action)
        {
            EventLog log = new EventLog();
            log.Source = eventSource;
            log.Log = eventLog;

            string message = "An exception occurred communicating with the data source.\n\n";
            message += "Action: " + action + "\n\n";
            message += "Exception: " + e.ToString();

            log.WriteEntry(message);
        }

    }

}
