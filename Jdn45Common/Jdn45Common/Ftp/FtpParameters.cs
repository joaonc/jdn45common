using System;
using System.Collections.Generic;
using System.Text;

namespace Jdn45Common.Ftp
{
    /// <summary>
    /// Helper class to hold FTP parameters.
    /// XML serializable.
    /// </summary>
    [Serializable]
    public class FtpParameters
    {
        private string host;
        private int port;
        private string user;
        private string password;

        public FtpParameters()
        {
            Port = 21;  // Default FTP port
        }

        public string Host
        {
            get { return host; }
            set { host = value; }
        }

        public int Port
        {
            get { return port; }
            set { port = value; }
        }

        public string User
        {
            get { return user; }
            set { user = value; }
        }

        public string Password
        {
            get { return password; }
            set { password = value; }
        }
    }
}
