using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using TaskList.Abstractions;
using TaskList.UWP.Services;
using Windows.Security.Credentials;

[assembly: Xamarin.Forms.Dependency(typeof(UWPPlatform))]
namespace TaskList.UWP.Services
{
    public class UWPPlatform : IPlatform
    {
        private const string ServiceIdentifier = "chapter3-tasklist";

        public PasswordVault PasswordVault { get; private set; }

        public UWPPlatform()
        {
            PasswordVault = new PasswordVault();
        }

        #region ILoginProvider Interface
        public MobileServiceUser RetrieveTokenFromSecureStore()
        {
            try
            {
                // Check if the token is available within the password vault
                var acct = PasswordVault.FindAllByResource(ServiceIdentifier).FirstOrDefault();
                if (acct != null)
                {
                    var token = PasswordVault.Retrieve(ServiceIdentifier, acct.UserName).Password;
                    if (token != null && token.Length > 0)
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
            PasswordVault.Add(new PasswordCredential(ServiceIdentifier, user.UserId, user.MobileServiceAuthenticationToken));
        }

        public void RemoveTokenFromSecureStore()
        {
            try
            {
                // Check if the token is available within the password vault
                var acct = PasswordVault.FindAllByResource(ServiceIdentifier).FirstOrDefault();
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

        public async Task<MobileServiceUser> LoginAsync(MobileServiceClient client)
        {
            var user = await client.LoginAsync("aad");
            return user;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task LogoutAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // Do nothing
        }

        public string GetSyncStore()
        {
            return "syncstore.db";
        }
        #endregion
    }
}

