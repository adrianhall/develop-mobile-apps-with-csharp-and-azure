#pragma warning disable IDE1006 // Naming Styles

using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using UIKit;
using VideoApp.Abstractions;
using VideoApp.iOS.Services;
using Xamarin.Auth;

[assembly: Xamarin.Forms.Dependency(typeof(iOSLoginProvider))]
namespace VideoApp.iOS.Services
{
    public class iOSLoginProvider : ILoginProvider
    {
        private readonly string AccountStoreId = "videos";

        public UIViewController RootView => UIApplication.SharedApplication.KeyWindow.RootViewController;

        public AccountStore AccountStore { get; }

        public iOSLoginProvider()
        {
            AccountStore = AccountStore.Create();
        }

        public MobileServiceUser RetrieveTokenFromSecureStore()
        {
            var accounts = AccountStore.FindAccountsForService(AccountStoreId);
            if (accounts != null)
            {
                foreach (var acct in accounts)
                {
                    if (acct.Properties.TryGetValue("token", out string token))
                    {
                        return new MobileServiceUser(acct.Username)
                        {
                            MobileServiceAuthenticationToken = token
                        };
                    }
                }
            }
            return null;
        }

        public void StoreTokenInSecureStore(MobileServiceUser user)
        {
            var account = new Account(user.UserId);
            account.Properties.Add("token", user.MobileServiceAuthenticationToken);
            AccountStore.Save(account, AccountStoreId);
        }

        public void RemoveTokenFromSecureStore()
        {
            var accounts = AccountStore.FindAccountsForService(AccountStoreId);
            if (accounts != null)
            {
                foreach (var acct in accounts)
                {
                    AccountStore.Delete(acct, AccountStoreId);
                }
            }
        }

        public Task<MobileServiceUser> LoginAsync(MobileServiceClient client)
        {
            return client.LoginAsync(RootView, "aad");
        }
    }
}