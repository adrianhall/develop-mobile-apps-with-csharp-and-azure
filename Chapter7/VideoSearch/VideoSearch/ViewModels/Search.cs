using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace VideoSearch.ViewModels
{
    public class Search : ViewModels.Base
    {
        private SearchService _service = new SearchService();

        public Search()
        {
            Title = "Video Search";
            SearchResults = new List<Movie>();
        }

        #region SearchString Property
        private string _pSearchString;
        public string SearchString
        {
            get { return _pSearchString; }
            set { SetProperty(ref _pSearchString, value, "SearchString"); }
        }
        #endregion

        #region Movies Propertu
        private List<Movie> _pSearchResults;
        public List<Movie> SearchResults
        {
            get { return _pSearchResults; }
            set { SetProperty(ref _pSearchResults, value, "Movies")}
        }

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
                var results = await _service.SearchMoviesAsync(SearchString);
                //
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
