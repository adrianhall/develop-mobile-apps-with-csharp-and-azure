using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TaskList.Abstractions
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Interface
        /// <summary>
        /// Event Handler for INotifyPropertyChanged
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Event initiator for INotifyPropertyChanged
        /// </summary>
        /// <param name="propName">The property that changed</param>
        public void OnPropertyChanged(string propName)
        {
            if (PropertyChanged == null)
            {
                return;
            }
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        /// <summary>
        /// Set a property, initiating the INotifyPropertyChanged handler if appropriate
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="store">The backing store variable for the property</param>
        /// <param name="value">The new value of the property</param>
        /// <param name="propName">The property name</param>
        /// <param name="onChanged">An action to call if the property changes</param>
        protected void SetProperty<T>(ref T store, T value, string propName, Action onChanged = null)
        {
            // Determine if the property actually changed
            if (EqualityComparer<T>.Default.Equals(store, value)) return;

            // Change the value
            store = value;

            // Handle Property Changed events & callbacks
            onChanged?.Invoke();
            OnPropertyChanged(propName);
        }
        #endregion

        #region Standard Properties
        /// <summary>
        /// Backing store for the Title property
        /// </summary>
        private string _propTitle = string.Empty;

        /// <summary>
        /// The Title of the page
        /// </summary>
        public string Title
        {
            get { return _propTitle; }
            set { SetProperty(ref _propTitle, value, "Title");  }
        }

        /// <summary>
        /// Backing store for the IsBusy property
        /// </summary>
        private bool _propIsBusy;

        /// <summary>
        /// true if the network connection is busy
        /// </summary>
        public bool IsBusy
        {
            get { return _propIsBusy; }
            set { SetProperty(ref _propIsBusy, value, "IsBusy"); }
        }
        #endregion
    }
}
