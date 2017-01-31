using System;
using System.Threading.Tasks;
using VideoSearch.Helpers;
using VideoSearch.Services;
using Xamarin.Forms;

namespace VideoSearch.ViewModels
{
    public class Search : ViewModels.Base
    {
        private SearchService _service = new SearchService();

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

        #region SearchResult Property
        private ObservableRangeCollection<Movie> _pSearchResults = new ObservableRangeCollection<Movie>();
        public List<Movie> SearchResults
        {
            get { return _pSearchResults; }
            set { SetProperty(ref _pSearchResults, value, "SearchResults"); }
        }
        #endregion

        #region SelectedItem Property
        private Movie _pSelectedItem = null;
        public Movie SelectedItem
        {
            get { return _pSelectedItem; }
            set { 
                SetProperty(ref _pSelectedItem, value, "SelectedItem");                
                if (selectedItem != null)
                {
                    Application.Current.MainPage.Navigation.PushAsync(new Views.Details(_pSelectedItem));
                    SelectedItem = null;
                }
            }
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
                var results = await _service.SearchMoviesAsync(SearchString);
                SearchResults.ReplaceRange(results);
            }
            catch (Exception ex)
            {
                SearchResults.Clear();
                await Application.Current.MainPage.DisplayAlert("Search Failed", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
        #endregion
    }
}
