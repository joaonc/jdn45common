using System;
using System.Collections.Generic;
using System.Text;

namespace Jdn45Common.Permissions
{
    public class Role
    {
        private int level;
        private string name;
        private string description;

        public int Level
        {
            get { return level; }
            set { level = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }
    }
}
