using SocialFORM.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace SocialFORM.Providers
{
    public class CustomRoleProvider : RoleProvider
    {
        public override string Name => base.Name;

        public override string Description => base.Description;

        public override string ApplicationName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string[] GetRolesForUser(string username)
        {
            string[] roles = new string[] {};

            using (ApplicationContext db = new ApplicationContext())
            {
                User user = db.SetUser.FirstOrDefault(u => u.Login == username);

                if (user != null)
                {
                    Role userRole = db.SetRoles.Find(user.RoleId);

                    if (userRole != null)
                        roles = new string[] { userRole.Name };

                }
            }

            return roles;

        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            bool outputResult = false;

            using (ApplicationContext db = new ApplicationContext())
            {
                User user = db.SetUser.FirstOrDefault(u => u.Login == username);

                if (user != null)
                {
                    Role userRole = db.SetRoles.Find(user.RoleId);

                    if (userRole != null && userRole.Name == roleName)
                        outputResult = true;

                }
            }

            return outputResult;

        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}