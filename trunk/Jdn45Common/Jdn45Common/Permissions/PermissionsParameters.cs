using System;
using System.Text;
using System.Collections.Generic;
using System.Xml.Serialization;
using Jdn45Common;

namespace Jdn45Common.Permissions
{
    public class PermissionsParameters
    {
        private List<User> userList;
        private List<Role> roleList;
        private KeyValuePairCollection<string, string> userRolesCollection;  // Not an optimal collection to be used for this,
                                                                             // but had problems in XML serialization with other objects

        public PermissionsParameters()
        {
            userList = new List<User>();
            roleList = new List<Role>();
            userRolesCollection = new KeyValuePairCollection<string, string>();
        }

        public List<User> Users
        {
            get { return userList; }
            set { userList = value; }
        }

        public List<Role> Roles
        {
            get { return roleList; }
            set { roleList = value; }
        }

        public KeyValuePairCollection<string, string> UserRoles
        {
            get { return userRolesCollection; }
            set { userRolesCollection = value; }
        }

        /// <summary>
        /// Returns a user by its login name or null if not found.
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public User GetUserByLogin(string login)
        {
            return Users.Find(delegate(User user)
                {
                    return user.Login.Equals(login);
                });
        }

        /// <summary>
        /// Returns a role by its name or null if not found.
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public Role GetRoleByName(string roleName)
        {
            return Roles.Find(delegate(Role role)
                {
                    return role.Name.Equals(roleName);
                });
        }

        /// <summary>
        /// Gets the roles associated with the given user.
        /// Returns an empty list if the user is not found.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public List<string> GetUserRoles(string userLogin)
        {
            return userRolesCollection.GetValuesByKey(userLogin);
        }

        /// <summary>
        /// Gets all users with the given role.
        /// Returns an empty list if no users have that role.
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public List<string> GetUsersWithRole(string roleName)
        {
            return userRolesCollection.GetKeysWithValue(roleName);
        }

        public bool UserHasRole(string userLogin, string roleName)
        {
            List<string> userRoles = GetUserRoles(userLogin);

            return (userRoles != null) && (userRoles.Contains(roleName));
        }

        /// <summary>
        /// Adds the role to the user.
        /// Returns true if the role was indeed added or false if the user already had the role.
        /// </summary>
        /// <param name="userLogin"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public bool AddRoleToUser(string userLogin, string roleName)
        {
            // Check user and role exist
            if (GetUserByLogin(userLogin) == null)
            {
                throw new Exception("User with login not found: " + userLogin);
            }

            if (GetRoleByName(roleName) == null)
            {
                throw new Exception("Role with name not found: " + roleName);
            }

            // Add role to user only if user doesn't yet have it
            bool roleAdded = false;
            List<string> userRoles = GetUserRoles(userLogin);

            if (!userRoles.Contains(roleName))
            {
                userRolesCollection.Add(userLogin, roleName);
                roleAdded = true;
            }

            return roleAdded;
        }
    }
}
