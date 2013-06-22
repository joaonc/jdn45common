using System;
using System.Collections.Generic;
using System.Text;

namespace Jdn45Common.Email
{
    /// <summary>
    /// Helper class to hold the Email parameters.
    /// XML serializable.
    /// </summary>
    public class EmailParameters
    {
        private string user;
        private string email;
        private string password;
        private string displayName;
        private string host;
        private int port;
        private bool useSSL;
        private bool skipEmailOnDebug;

        public EmailParameters()
        {
            SkipEmailOnDebug = true;  // Avoid accidentally sending emails
            Port = -1;  // Mark to use the default port later.
        }

        public bool UseSSL
        {
            get { return useSSL; }
            set { useSSL = value; }
        }

        /// <summary>
        /// Port to use in the SMTP server.
        /// 0 or less will assume default port.
        /// </summary>
        public int Port
        {
            get { return port; }
            set { port = value; }
        }

        /// <summary>
        /// SMTP server name or IP address.
        /// </summary>
        public string Host
        {
            get { return host; }
            set { host = value; }
        }

        public string DisplayName
        {
            get { return displayName; }
            set { displayName = value; }
        }

        public string Email
        {
            get { return email; }
            set { email = value; }
        }

        /// <summary>
        /// User name. Can be set to null, in which case it's the same as the email.
        /// </summary>
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

        [System.Xml.Serialization.XmlIgnore]
        public bool UsingDefaultPort
        {
            get { return Port <= 0; }
            set
            {
                if (value)
                {
                    Port = -1;
                }
                // else, no changes in Port number
            }
        }

        /// <summary>
        /// If true, doesn't send emails if the code is running in Debug mode.
        /// </summary>
        public bool SkipEmailOnDebug
        {
            get { return skipEmailOnDebug; }
            set { skipEmailOnDebug = value; }
        }
    }
}
