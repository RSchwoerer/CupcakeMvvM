using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace CupcakeMvvM.Framework
{
    /// <summary>
    /// Provides data for the <see cref="ObservableCollectionEx{T}.ItemPropertyChanged"/> event.
    /// </summary>
    public class ItemPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemPropertyChangedEventArgs"/> class.
        /// </summary>
        /// <param name="index">The index in the collection of changed item.</param>
        /// <param name="name">The name of the property that changed.</param>
        public ItemPropertyChangedEventArgs(int index, string name) : base(name)
        {
            CollectionIndex = index;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemPropertyChangedEventArgs"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="args">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        public ItemPropertyChangedEventArgs(int index, PropertyChangedEventArgs args) : this(index, args.PropertyName)
        { }

        /// <summary>
        /// Gets the index in the collection for which the property change has occurred.
        /// </summary>
        /// <value>
        /// Index in parent collection.
        /// </value>
        public int CollectionIndex { get; }
    }

    /// <summary>
    /// Implements an ObservableCollection that provides item changed notifications
    /// </summary>
    /// <remarks>
    /// source: http://stackoverflow.com/a/32013610/504398
    /// </remarks>
    public class ObservableCollectionEx<T> : ObservableCollection<T>
          where T : INotifyPropertyChanged
    {
        private bool _suppressNotification;

        public ObservableCollectionEx() : base()
        { }

        public ObservableCollectionEx(List<T> list) : base(list)
        {
            ObserveAll();
        }

        public ObservableCollectionEx(IEnumerable<T> enumerable) : base(enumerable)
        {
            ObserveAll();
        }

        /// <summary>
        /// Occurs when a property is changed within an item.
        /// </summary>
        public event EventHandler<ItemPropertyChangedEventArgs> ItemPropertyChanged;

        /// <summary>
        /// Adds item to the list without raising any CollectionChanged events.
        /// Call ForceReset to force the CollectionChaged event.
        /// </summary>
        public void AddSilent(T item)
        {
            try
            {
                _suppressNotification = true;
                Add(item);
            }
            finally
            {
                _suppressNotification = false;
            }
        }


        // TODO [rs]: this is not right. something got lost here in converstion.
        //              should re-fire property changed on this collection, looks like it is re-subscribing.
        //              probably should call OnItemPropertyChanged...
        ///// <summary>
        ///// Forces the CollectionChanged.Reset event to be fired.
        ///// Typically used after numerous calls to AddSilent.
        ///// </summary>
        //public void ForceReset()
        //{
        //    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        //}

        protected override void ClearItems()
        {
            foreach (T item in Items)
                item.PropertyChanged -= ChildPropertyChanged;

            base.ClearItems();
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove ||
                  e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (T item in e.OldItems)
                    item.PropertyChanged -= ChildPropertyChanged;
            }

            if (e.Action == NotifyCollectionChangedAction.Add ||
                e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (T item in e.NewItems)
                    item.PropertyChanged += ChildPropertyChanged;
            }

            base.OnCollectionChanged(e);
        }

        protected void OnItemPropertyChanged(ItemPropertyChangedEventArgs e)
        {
            if (_suppressNotification) return;

            // fire specialized event
            ItemPropertyChanged?.Invoke(this, e);

            // fire generic event
             base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected void OnItemPropertyChanged(int index, PropertyChangedEventArgs e)
        {
            OnItemPropertyChanged(new ItemPropertyChangedEventArgs(index, e));
        }

        private void ChildPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            T typedSender = (T)sender;
            int i = Items.IndexOf(typedSender);

            if (i < 0)
                throw new ArgumentException("Received property notification from item not in collection");

            OnItemPropertyChanged(i, e);
        }

        private void ObserveAll()
        {
            foreach (T item in Items)
                item.PropertyChanged += ChildPropertyChanged;
        }
    }
}