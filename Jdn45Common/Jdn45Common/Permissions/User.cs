using System;
using System.Collections.Generic;
using System.Text;
using Jdn45Common.Attributes;

namespace Jdn45Common.Permissions
{
    public class User
    {
        private string login;
        private string password;
        private string name;
        private string email;
        private int active;

        [Mandatory]
        public string Login
        {
            get { return login; }
            set { login = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Email
        {
            get { return email; }
            set { email = value; }
        }

        [Password(true)]
        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        public int Active
        {
            get { return active; }
            set { active = value; }
        }
    }
}
