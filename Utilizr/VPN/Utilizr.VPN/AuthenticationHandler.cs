using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace Utilizr.VPN
{
    public delegate UserPass UserPassHandler(ConnectionType connectionType);
    
    public class UserPass
    {
        public string Username { get; set; }
        public SecureString Password { get; set; }

        public UserPass(string username, SecureString password)
        {
            Username = username;
            Password = password;
        }
    }

    public delegate string L2TPSecretHandler();
}
