using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace VideoSearch.ViewModels
{
    public class Search : ViewModels.Base
    {
        public Search()
        {
            Title = "Video Search";
        }

        #region SearchString Property
        private string _pSearchString;
        public string SearchString
        {
            get { return _pSearchString; }
            set { SetProperty(ref _pSearchString, value, "SearchString"); }
        }
        #endregion

        #region Search Command
        Command _cmdSearch;

        public Command SearchCommand => _cmdSearch ?? (_cmdSearch = new Command(async () => await ExecuteSearchCommand()));

        private async Task ExecuteSearchCommand()
        {
            if (IsBusy)
                return;
            IsBusy = true;

            try
            {
                // Do Work here
            }
            catch (Exception ex)
            {
                // Do exception handling here
            }
            finally
            {
                IsBusy = false;
            }
        }
        #endregion
    }
}
