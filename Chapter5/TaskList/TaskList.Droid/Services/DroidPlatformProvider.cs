using Android.Content;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TaskList.Abstractions;
using TaskList.Droid.Services;
using Xamarin.Auth;

[assembly: Xamarin.Forms.Dependency(typeof(DroidPlatformProvider))]
namespace TaskList.Droid.Services
{
    public class DroidPlatformProvider : IPlatformProvider
    {
        #region ILoginProvider Interface
        public MobileServiceUser RetrieveTokenFromSecureStore()
        {
            var accounts = AccountStore.FindAccountsForService("tasklist");
            if (accounts != null)
            {
                foreach (var acct in accounts)
                {
                    string token;

                    if (acct.Properties.TryGetValue("token", out token))
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
            AccountStore.Save(account, "tasklist");
        }

        public void RemoveTokenFromSecureStore()
        {
            var accounts = AccountStore.FindAccountsForService("tasklist");
            if (accounts != null)
            {
                foreach (var acct in accounts)
                {
                    AccountStore.Delete(acct, "tasklist");
                }
            }
        }

        public async Task<MobileServiceUser> LoginAsync(MobileServiceClient client)
        {
            var mobileServiceUser = await client.LoginAsync(RootView, MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory);
            Debug.WriteLine($"ZUMO Token = {mobileServiceUser.MobileServiceAuthenticationToken}");
            return mobileServiceUser;
        }

        public Task RegisterForPushNotifications(MobileServiceClient client)
        {
            throw new NotImplementedException();
        }
        #endregion

        public Context RootView { get; private set; }

        public AccountStore AccountStore { get; private set; }

        public void Init(Context context)
        {
            RootView = context;
            AccountStore = AccountStore.Create(context);
        }
    }
}
