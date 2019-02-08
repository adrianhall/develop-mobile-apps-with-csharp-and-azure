using VideoSearch.Models;

namespace VideoSearch.ViewModels
{
    public class Details : Base
    {
        public Details(Movie item)
        {
            Movie = item;
            Title = item.Title;
        }

        #region Movie Property
        Movie _pItem;
        public Movie Movie
        {
            get { return _pItem; }
            set { SetProperty(ref _pItem, value, "Movie"); }
        }
        #endregion
    }
}
