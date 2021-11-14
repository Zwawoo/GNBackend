using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model
{
    public class AuthenticationResult
    {
        public bool IsAuthenticated { get; set; }
        public string AuthenticatedUser { get; set; }

        public bool AuthenticatedUserEquals(string userName)
        {
            if (IsAuthenticated == false)
                return false;
            if (String.IsNullOrWhiteSpace(AuthenticatedUser) || String.IsNullOrWhiteSpace(userName))
                return false;
            if (AuthenticatedUser.ToLower() == userName.ToLower())
                return true;
            else
                return false;
        }
    }
}
