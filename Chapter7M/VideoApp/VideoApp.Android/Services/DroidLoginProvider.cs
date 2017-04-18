using Android.Content;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;
using VideoApp.Abstractions;
using VideoApp.Droid.Services;
//using Xamarin.Auth;

[assembly: Xamarin.Forms.Dependency(typeof(DroidLoginProvider))]
namespace VideoApp.Droid.Services
{
    public class DroidLoginProvider : ILoginProvider
    {
        private readonly string AccountStoreId = "videos";

        public Task<MobileServiceUser> LoginAsync(MobileServiceClient client)
        {
            return client.LoginAsync(RootView, "aad");
        }

        public void RemoveTokenFromSecureStore()
        {
            //var accounts = AccountStore.FindAccountsForService(AccountStoreId);
            //if (accounts != null)
            //{
            //    foreach (var acct in accounts)
            //    {
            //        AccountStore.Delete(acct, AccountStoreId);
            //    }
            //}
        }

        public MobileServiceUser RetrieveTokenFromSecureStore()
        {
            //var accounts = AccountStore.FindAccountsForService(AccountStoreId);
            //if (accounts != null)
            //{
            //    foreach (var acct in accounts)
            //    {
            //        if (acct.Properties.TryGetValue("token", out string token))
            //        {
            //            return new MobileServiceUser(acct.Username)
            //            {
            //                MobileServiceAuthenticationToken = token
            //            };
            //        }
            //    }
            //}
            return null;
        }

        public void StoreTokenInSecureStore(MobileServiceUser user)
        {
            //var account = new Account(user.UserId);
            //account.Properties.Add("token", user.MobileServiceAuthenticationToken);
            //AccountStore.Save(account, AccountStoreId);
        }

        public Context RootView { get; private set; }
        //public AccountStore AccountStore { get; private set; }

        public void Init(Context context)
        {
            RootView = context;
            //AccountStore = AccountStore.Create(context);
        }
    }
}