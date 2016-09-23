namespace CupcakeMvvM.Framework
{
    public class ViewModelBase : PropertyChangedBase
    {
        protected static IEventAggregator Event;
        private string _DisplayName;

        /// <summary>
        /// Creates an instance of the screen.
        /// </summary>
        public ViewModelBase()
        {
            _DisplayName = GetType().FullName;
        }

        /// <summary>
        /// Gets or Sets the Display Name
        /// </summary>
        public virtual string DisplayName
        {
            get { return _DisplayName; }
            set
            {
                _DisplayName = value;
                NotifyOfPropertyChange();
            }
        }
    }
}