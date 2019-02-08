using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VideoApp.Abstractions;
using VideoApp.UWP.Services;
using Windows.Security.Credentials;

[assembly: Xamarin.Forms.Dependency(typeof(UWPLoginProvider))]
namespace VideoApp.UWP.Services
{
    public class UWPLoginProvider : ILoginProvider
    {
        private readonly string PasswordVaultId = "videos";

        public PasswordVault PasswordVault { get; }

        public UWPLoginProvider()
        {
            PasswordVault = new PasswordVault();
        }

        #region ILoginProvider Interface
        public MobileServiceUser RetrieveTokenFromSecureStore()
        {
            try
            {
                // Check if the token is available within the password vault
                var acct = PasswordVault.FindAllByResource(PasswordVaultId).FirstOrDefault();
                if (acct != null)
                {
                    var token = PasswordVault.Retrieve(PasswordVaultId, acct.UserName).Password;
                    if (!string.IsNullOrEmpty(token))
                    {
                        return new MobileServiceUser(acct.UserName)
                        {
                            MobileServiceAuthenticationToken = token
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving existing token: {ex.Message}");
            }
            return null;
        }

        public void StoreTokenInSecureStore(MobileServiceUser user)
        {
            PasswordVault.Add(new PasswordCredential(PasswordVaultId, user.UserId, user.MobileServiceAuthenticationToken));
        }

        public void RemoveTokenFromSecureStore()
        {
            try
            {
                // Check if the token is available within the password vault
                var acct = PasswordVault.FindAllByResource(PasswordVaultId).FirstOrDefault();
                if (acct != null)
                {
                    PasswordVault.Remove(acct);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving existing token: {ex.Message}");
            }
        }

        public Task<MobileServiceUser> LoginAsync(MobileServiceClient client)
        {
            return client.LoginAsync("aad");
        }
        #endregion
    }
}
