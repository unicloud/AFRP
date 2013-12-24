using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Diagnostics;
using System.Linq;
using System.Web.Profile;
using System.Web.Security;
using UniCloud.Security.Models;

namespace UniCloud.Security.Services.Web
{
    public sealed class UniProfileProvider : ProfileProvider
    {
        //
        // Global connection string, generic exception message, event log info.
        //

        private string eventSource = "UniProfileProvider";
        private string eventLog = "Application";
        private string exceptionMessage = "An exception occurred. Please check the event log.";


        //
        // If false, exceptions are thrown to the caller. If true,
        // exceptions are written to the event log.
        //

        private bool pWriteExceptionsToEventLog;

        public bool WriteExceptionsToEventLog
        {
            get { return pWriteExceptionsToEventLog; }
            set { pWriteExceptionsToEventLog = value; }
        }



        //
        // System.Configuration.Provider.ProviderBase.Initialize Method
        //

        public override void Initialize(string name, NameValueCollection config)
        {

            //
            // Initialize values from web.config.
            //

            if (config == null)
                throw new ArgumentNullException("config");

            if (name == null || name.Length == 0)
                name = "UniProfileProvider";

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "UniCloud Profile provider");
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);


            if (config["applicationName"] == null || config["applicationName"].Trim() == "")
            {
                pApplicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
            }
            else
            {
                pApplicationName = config["applicationName"];
            }

        }


        //
        // System.Configuration.SettingsProvider.ApplicationName
        //

        private string pApplicationName;

        public override string ApplicationName
        {
            get { return pApplicationName; }
            set { pApplicationName = value; }
        }

        #region 重写SettingsProvider方法

        //
        // SettingsProvider.GetPropertyValues
        //

        public override SettingsPropertyValueCollection
              GetPropertyValues(SettingsContext context,
                    SettingsPropertyCollection ppc)
        {
            string username = (string)context["UserName"];
            bool isAuthenticated = (bool)context["IsAuthenticated"];

            // The serializeAs attribute is ignored in this provider implementation.

            SettingsPropertyValueCollection svc = new SettingsPropertyValueCollection();

            foreach (SettingsProperty prop in ppc)
            {
                SettingsPropertyValue pv = new SettingsPropertyValue(prop);

                switch (prop.Name)
                {
                    case "FriendlyName":
                        pv.PropertyValue = GetFriendlyName(username, isAuthenticated);
                        break;
                    default:
                        throw new ProviderException("Unsupported property.");
                }

                svc.Add(pv);
            }

            UpdateActivityDates(username, isAuthenticated, true);

            return svc;
        }



        //
        // SettingsProvider.SetPropertyValues
        //

        public override void SetPropertyValues(SettingsContext context,
                       SettingsPropertyValueCollection ppvc)
        {
            // The serializeAs attribute is ignored in this provider implementation.

            string username = (string)context["UserName"];
            bool isAuthenticated = (bool)context["IsAuthenticated"];
            int uniqueID = GetUniqueID(username, isAuthenticated, false);
            if (uniqueID == 0)
                uniqueID = CreateProfileForUser(username, isAuthenticated);

            foreach (SettingsPropertyValue pv in ppvc)
            {
                switch (pv.Property.Name)
                {
                    case "FriendlyName":
                        SetFriendlyName(uniqueID, (string)pv.PropertyValue);
                        break;
                    default:
                        throw new ProviderException("Unsupported property.");
                }
            }

            UpdateActivityDates(username, isAuthenticated, false);
        }


        //
        // UpdateActivityDates
        // Updates the LastActivityDate and LastUpdatedDate values 
        // when profile properties are accessed by the
        // GetPropertyValues and SetPropertyValues methods. 
        // Passing true as the activityOnly parameter will update
        // only the LastActivityDate.
        //

        private void UpdateActivityDates(string username, bool isAuthenticated, bool activityOnly)
        {
            using (var db = new SecurityEntities())
            {
                var profiles = db.Profiles.Where(p =>
                    p.User.Application.ApplicationName == Membership.ApplicationName &&
                    p.User.UserName == username &&
                    p.IsAnonymous == (isAuthenticated ? 0 : 1))
                    .ToList();
                if (activityOnly)
                {
                    profiles.ForEach(p => p.LastActivityDate = DateTime.Now);
                }
                else
                {
                    profiles.ForEach(p =>
                    {
                        p.LastActivityDate = DateTime.Now;
                        p.LastUpdatedDate = DateTime.Now;
                    });
                }
                db.SaveChanges();
            }
        }


        //
        // GetFriendlyName
        //   Retrieves FriendlyName from the database during the call to GetPropertyValues.
        //

        private string GetFriendlyName(string username, bool isAuthenticated)
        {
            using (var db = new SecurityEntities())
            {
                return db.ProfileValues.OfType<FriendlyName>().Where(p =>
                    p.Profiles.User.Application.ApplicationName == Membership.ApplicationName &&
                    p.Profiles.User.UserName == username &&
                    p.Profiles.IsAnonymous == (isAuthenticated ? 0 : 1))
                    .Select(p => p.PropertyValue).FirstOrDefault();
            }
        }


        //
        // SetFriendlyName
        // Inserts FriendlyName values into the database during 
        // the call to SetPropertyValues.
        //

        private void SetFriendlyName(int uniqueID, string friendlyName)
        {
            using (var db = new SecurityEntities())
            {
                db.ProfileValues.Where(p => p.UserId == uniqueID).ToList().ForEach(p => db.ProfileValues.Remove(p));
                var profile = db.Profiles.SingleOrDefault(p => p.UserId == uniqueID);
                db.ProfileValues.Add(new FriendlyName { Profiles = profile, PropertyValue = friendlyName });
                db.SaveChanges();
            }
        }


        //
        // GetUniqueID
        //   Retrieves the uniqueID from the database for the current user and application.
        //

        private int GetUniqueID(string username, bool isAuthenticated, bool ignoreAuthenticationType)
        {
            int uniqueID = 0;
            using (var db = new SecurityEntities())
            {
                if (ignoreAuthenticationType)
                    uniqueID = db.Profiles.Where(p =>
                        p.User.Application.ApplicationName == Membership.ApplicationName &&
                        p.User.UserName == username)
                        .Select(p => p.UserId)
                        .FirstOrDefault();
                else
                    uniqueID = db.Profiles.Where(p =>
                        p.User.Application.ApplicationName == Membership.ApplicationName &&
                        p.User.UserName == username &&
                        p.IsAnonymous == (isAuthenticated ? 0 : 1))
                        .Select(p => p.UserId)
                        .FirstOrDefault();

                return uniqueID;
            }
        }


        //
        // CreateProfileForUser
        // If no user currently exists in the database, 
        // a user record is created during
        // the call to the GetUniqueID private method.
        //

        private int CreateProfileForUser(string username, bool isAuthenticated)
        {
            // Check for valid user name.

            if (username == null)
                throw new ArgumentNullException("User name cannot be null.");
            if (username.Length > 255)
                throw new ArgumentException("User name exceeds 255 characters.");
            if (username.Contains(","))
                throw new ArgumentException("User name cannot contain a comma (,).");

            int uniqueID = 0;
            using (var db = new SecurityEntities())
            {
                var user = db.Users.SingleOrDefault(u =>
                    u.Application.ApplicationName == Membership.ApplicationName &&
                    u.UserName == username);
                user.Profiles = new Profiles
                {
                    LastActivityDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now,
                    IsAnonymous = isAuthenticated ? 0 : 1
                };
                if (db.SaveChanges() > 0)
                    uniqueID = user.UserId;
            }
            return uniqueID;
        }


        //
        // ProfileProvider.DeleteProfiles(ProfileInfoCollection)
        //

        public override int DeleteProfiles(ProfileInfoCollection profiles)
        {
            int deleteCount = 0;
            foreach (ProfileInfo p in profiles)
            {
                if (DeleteProfile(p.UserName))
                    deleteCount++;
            }
            return deleteCount;
        }


        //
        // ProfileProvider.DeleteProfiles(string[])
        //

        public override int DeleteProfiles(string[] usernames)
        {
            int deleteCount = 0;
            foreach (string user in usernames)
            {
                if (DeleteProfile(user))
                    deleteCount++;
            }
            return deleteCount;
        }



        //
        // ProfileProvider.DeleteInactiveProfiles
        //

        public override int DeleteInactiveProfiles(
          ProfileAuthenticationOption authenticationOption,
          DateTime userInactiveSinceDate)
        {
            using (var db = new SecurityEntities())
            {
                IEnumerable<Profiles> profiles;
                profiles = db.Profiles.Where(p =>
                    p.User.Application.ApplicationName == Membership.ApplicationName &&
                    p.LastActivityDate <= userInactiveSinceDate);
                switch (authenticationOption)
                {
                    case ProfileAuthenticationOption.Anonymous:
                        profiles = profiles.Where(p => p.IsAnonymous != 0);
                        break;
                    case ProfileAuthenticationOption.Authenticated:
                        profiles = profiles.Where(p => p.IsAnonymous == 0);
                        break;
                    default:
                        break;
                }
                return DeleteProfiles(profiles.Select(p => p.User.UserName).ToArray());
            }
        }


        //
        // DeleteProfile
        // Deletes profile data from the database for the 
        // specified user name.
        //

        private bool DeleteProfile(string username)
        {
            // Check for valid user name.
            if (username == null)
                throw new ArgumentNullException("User name cannot be null.");
            if (username.Length > 255)
                throw new ArgumentException("User name exceeds 255 characters.");
            if (username.Contains(","))
                throw new ArgumentException("User name cannot contain a comma (,).");

            int uniqueID = GetUniqueID(username, false, true);
            using (var db = new SecurityEntities())
            {
                var prof = db.Profiles.SingleOrDefault(p => p.UserId == uniqueID);
                db.Profiles.Remove(prof);
                prof.ProfileValues.ToList().ForEach(p => db.ProfileValues.Remove(p));
                if (db.SaveChanges() > 0)
                    return true;
                return false;
            }
        }


        //
        // ProfileProvider.FindProfilesByUserName
        //

        public override ProfileInfoCollection FindProfilesByUserName(
          ProfileAuthenticationOption authenticationOption,
          string usernameToMatch,
          int pageIndex,
          int pageSize,
          out int totalRecords)
        {
            CheckParameters(pageIndex, pageSize);

            return GetProfileInfo(authenticationOption, usernameToMatch,
                null, pageIndex, pageSize, out totalRecords);
        }


        //
        // ProfileProvider.FindInactiveProfilesByUserName
        //

        public override ProfileInfoCollection FindInactiveProfilesByUserName(
          ProfileAuthenticationOption authenticationOption,
          string usernameToMatch,
          DateTime userInactiveSinceDate,
          int pageIndex,
          int pageSize,
          out int totalRecords)
        {
            CheckParameters(pageIndex, pageSize);

            return GetProfileInfo(authenticationOption, usernameToMatch, userInactiveSinceDate,
                  pageIndex, pageSize, out totalRecords);
        }


        //
        // ProfileProvider.GetAllProfiles
        //

        public override ProfileInfoCollection GetAllProfiles(
          ProfileAuthenticationOption authenticationOption,
          int pageIndex,
          int pageSize,
          out int totalRecords)
        {
            CheckParameters(pageIndex, pageSize);

            return GetProfileInfo(authenticationOption, null, null,
                  pageIndex, pageSize, out totalRecords);
        }


        //
        // ProfileProvider.GetAllInactiveProfiles
        //

        public override ProfileInfoCollection GetAllInactiveProfiles(
          ProfileAuthenticationOption authenticationOption,
          DateTime userInactiveSinceDate,
          int pageIndex,
          int pageSize,
          out int totalRecords)
        {
            CheckParameters(pageIndex, pageSize);

            return GetProfileInfo(authenticationOption, null, userInactiveSinceDate,
                  pageIndex, pageSize, out totalRecords);
        }



        //
        // ProfileProvider.GetNumberOfInactiveProfiles
        //

        public override int GetNumberOfInactiveProfiles(
          ProfileAuthenticationOption authenticationOption,
          DateTime userInactiveSinceDate)
        {
            int inactiveProfiles = 0;

            ProfileInfoCollection profiles =
              GetProfileInfo(authenticationOption, null, userInactiveSinceDate,
                  0, 0, out inactiveProfiles);

            return inactiveProfiles;
        }



        //
        // CheckParameters
        // Verifies input parameters for page size and page index. 
        // Called by GetAllProfiles, GetAllInactiveProfiles, 
        // FindProfilesByUserName, and FindInactiveProfilesByUserName.
        //

        private void CheckParameters(int pageIndex, int pageSize)
        {
            if (pageIndex < 0)
                throw new ArgumentException("Page index must 0 or greater.");
            if (pageSize < 1)
                throw new ArgumentException("Page size must be greater than 0.");
        }


        //
        // GetProfileInfo
        // Retrieves a count of profiles and creates a 
        // ProfileInfoCollection from the profile data in the 
        // database. Called by GetAllProfiles, GetAllInactiveProfiles,
        // FindProfilesByUserName, FindInactiveProfilesByUserName, 
        // and GetNumberOfInactiveProfiles.
        // Specifying a pageIndex of 0 retrieves a count of the results only.
        //

        private ProfileInfoCollection GetProfileInfo(
          ProfileAuthenticationOption authenticationOption,
          string usernameToMatch,
          object userInactiveSinceDate,
          int pageIndex,
          int pageSize,
          out int totalRecords)
        {
            using (var db = new SecurityEntities())
            {
                IEnumerable<Profiles> profileList;
                profileList = db.Profiles.Where(p =>
                    p.User.Application.ApplicationName == Membership.ApplicationName &&
                    usernameToMatch != null ? p.User.UserName == usernameToMatch : true &&
                    userInactiveSinceDate != null ? p.LastActivityDate <= (DateTime)userInactiveSinceDate : true);
                switch (authenticationOption)
                {
                    case ProfileAuthenticationOption.Anonymous:
                        profileList = profileList.Where(p => p.IsAnonymous != 0);
                        break;
                    case ProfileAuthenticationOption.Authenticated:
                        profileList = profileList.Where(p => p.IsAnonymous == 0);
                        break;
                    default:
                        break;
                }

                ProfileInfoCollection profiles = new ProfileInfoCollection();
                totalRecords = profileList.Count();
                // No profiles found.
                if (totalRecords <= 0) { return profiles; }
                // Count profiles only.
                if (pageSize == 0) { return profiles; }

                int startIndex = pageSize * pageIndex;
                profileList.Skip(startIndex).Take(pageSize).Select(p => new ProfileInfo(
                    p.User.UserName,
                    p.IsAnonymous != 0,
                    p.LastActivityDate,
                    p.LastUpdatedDate,
                    0)).ToList().ForEach(p => profiles.Add(p));

                return profiles;
            }
        }

        #endregion

        //
        // WriteToEventLog
        // A helper function that writes exception detail to the event 
        // log. Exceptions are written to the event log as a security 
        // measure to prevent private database details from being 
        // returned to the browser. If a method does not return a 
        // status or Boolean value indicating whether the action succeeded 
        // or failed, the caller also throws a generic exception.
        //

        private void WriteToEventLog(Exception e, string action)
        {
            EventLog log = new EventLog();
            log.Source = eventSource;
            log.Log = eventLog;

            string message = "An exception occurred while communicating with the data source.\n\n";
            message += "Action: " + action + "\n\n";
            message += "Exception: " + e.ToString();

            log.WriteEntry(message);
        }
    }

}