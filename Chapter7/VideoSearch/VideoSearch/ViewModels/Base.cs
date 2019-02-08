using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace VideoSearch.ViewModels
{
    public class Base : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Title Property
        private string _pTitle = string.Empty;

        public string Title
        {
            get { return _pTitle; }
            set { SetProperty(ref _pTitle, value, "Title"); }
        }
        #endregion

        #region IsBusy Property
        private bool _pIsBusy;

        public bool IsBusy
        {
            get { return _pIsBusy; }
            set { SetProperty(ref _pIsBusy, value, "IsBusy"); }
        }
        #endregion

        #region SetProperty
        /// <summary>
        /// Sets the defined property while still handling notifications
        /// </summary>
        /// <typeparam name="T">Property type</typeparam>
        /// <param name="store">The private store for the property</param>
        /// <param name="value">The new value of the property</param>
        /// <param name="propName">The property name</param>
        /// <param name="onChanged">An optional action to be called if the property changes</param>
        protected void SetProperty<T>(ref T store, T value, string propName, Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(store, value))
            {
                return;
            }

            store = value;

            if (onChanged != null)
            {
                onChanged();
            }
            OnPropertyChanged(propName);
        }
        #endregion

        #region OnPropertyChanged
        /// <summary>
        /// Handler called when the property value changes
        /// </summary>
        /// <param name="propName">The name of the property that changed</param>
        public void OnPropertyChanged(string propName)
        {
            if (PropertyChanged == null)
            {
                return;
            }
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        #endregion
    }
}
