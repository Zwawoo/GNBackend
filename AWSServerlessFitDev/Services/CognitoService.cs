using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using AWSServerlessFitDev.Util;
using System;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Services
{
    public class CognitoService
    {

        public static async Task<bool> IsUserAuthenticated(string userName, string accessToken)
        {
            var cognitoIDP = new AmazonCognitoIdentityProviderClient(Amazon.RegionEndpoint.EUCentral1);
            var getUserRequest = new GetUserRequest() { AccessToken = accessToken };
            try
            {
                GetUserResponse response = await cognitoIDP.GetUserAsync(getUserRequest);
                string authenticatedUser = response.Username;
                if (userName.ToLower().Equals(authenticatedUser.ToLower()))
                {
                    return true;
                }
                return false;
            }
            catch(Exception ex)
            {
                return false;
            }  
        }

        public static async Task<string> GetUserNameFromAccessToken(string accessToken)
        {
            var cognitoIDP = new AmazonCognitoIdentityProviderClient(Amazon.RegionEndpoint.EUCentral1);
            var getUserRequest = new GetUserRequest() { AccessToken = accessToken };
            
            try
            {
                GetUserResponse response = await cognitoIDP.GetUserAsync(getUserRequest);
                if(response != null)
                {
                    return response.Username;
                }
                else
                {
                    return null;
                }
                
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static async Task DisableUser(string userName)
        {
            var cognitoIDP = new AmazonCognitoIdentityProviderClient(Amazon.RegionEndpoint.EUCentral1);
            var req = new AdminDisableUserRequest() { Username = userName, UserPoolId = Constants.UserPoolId };
            var resp = await cognitoIDP.AdminDisableUserAsync(req);

            var signOutReq = new AdminUserGlobalSignOutRequest() { Username = userName, UserPoolId = Constants.UserPoolId };
            await cognitoIDP.AdminUserGlobalSignOutAsync(signOutReq);
        }

        public static async Task EnableUser(string userName)
        {
            var cognitoIDP = new AmazonCognitoIdentityProviderClient(Amazon.RegionEndpoint.EUCentral1);
            var req = new AdminEnableUserRequest() { Username = userName, UserPoolId = Constants.UserPoolId };

            var resp = await cognitoIDP.AdminEnableUserAsync(req);
        }

        public static async Task<bool> AdminCheckIfUserIsEnabled(string userName)
        {
            var cognitoIDP = new AmazonCognitoIdentityProviderClient(Amazon.RegionEndpoint.EUCentral1);
            var getUserRequest = new AdminGetUserRequest() {  Username = userName, UserPoolId = Constants.UserPoolId };

            try
            {
                AdminGetUserResponse response = await cognitoIDP.AdminGetUserAsync(getUserRequest);
                if (response != null)
                {
                    return response.Enabled;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static async Task AdminDeleteUser(string userName)
        {
            var cognitoIDP = new AmazonCognitoIdentityProviderClient(Amazon.RegionEndpoint.EUCentral1);
            var req = new AdminDeleteUserRequest() {  Username = userName, UserPoolId = Constants.UserPoolId };
            var resp = await cognitoIDP.AdminDeleteUserAsync(req);
        }


    }
}
