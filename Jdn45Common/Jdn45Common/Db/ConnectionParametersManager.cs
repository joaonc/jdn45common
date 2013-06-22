using System;
using System.Collections.Generic;
using System.Text;
using Jdn45Common;

namespace Jdn45Common.Db
{
    /// <summary>
    /// Manages Connection Parameters to make it easy to add/remove/choose which ones to use.
    /// Make sure the names of the ConnectionParameters are unique.
    /// </summary>
    public class ConnectionParametersManager
    {
        /// <summary>
        /// The default Connection Parameters' name.
        /// </summary>
        public static readonly string DefaultConnectionParametersName = "Default";

        private List<ConnectionParameters> connectionParametersList;
        private string activeConnectionParametersName;

        public ConnectionParametersManager()
        {
            connectionParametersList = new List<ConnectionParameters>();
            activeConnectionParametersName = "";
        }

        /// <summary>
        /// Gets or sets the list of ConnectionParameters that can be used.
        /// </summary>
        public List<ConnectionParameters> ConnectionParametersList
        {
            get { return connectionParametersList; }
            set { connectionParametersList = value; }
        }

        /// <summary>
        /// Gets or sets the active connection parameters' name.
        /// </summary>
        public string ActiveConnectionParametersName
        {
            get { return activeConnectionParametersName; }
            set
            {
                if (value != null)
                {
                    if (GetByName(value) == null)
                    {
                        throw new Exception("ConnectionParameters not found: " + value);
                    }

                    activeConnectionParametersName = value;
                }
            }
        }

        public ConnectionParameters GetByName(string name)
        {
            return ConnectionParametersList.Find(delegate(ConnectionParameters connectionParameters)
            {
                return connectionParameters.Name.Equals(name);
            });
        }

        /// <summary>
        /// Gets the active ConnectionParameters.
        /// </summary>
        /// <returns></returns>
        public ConnectionParameters GetActive()
        {
            return GetByName(ActiveConnectionParametersName);
        }

        public void SetActive(ConnectionParameters connectionParameters)
        {
            ActiveConnectionParametersName = connectionParameters.Name;
        }
    }
}
