using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SeatFlow.Helpers
{
    public class FullyObservableCollection<T> : ObservableCollection<T>
        where T : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler ItemPropertyChanged;

        public FullyObservableCollection() : base()
        {
            CollectionChanged += new NotifyCollectionChangedEventHandler(FullyObservableCollectionOnCollectionChanged);
        }

        private void FullyObservableCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (object item in e.NewItems)
                {
                    (item as INotifyPropertyChanged).PropertyChanged += new PropertyChangedEventHandler(ItemOnPropertyChanged);
                }
            }
            if (e.OldItems != null)
            {
                foreach (object item in e.OldItems)
                {
                    (item as INotifyPropertyChanged).PropertyChanged -= new PropertyChangedEventHandler(ItemOnPropertyChanged);
                }
            }
        }

        private void ItemOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyCollectionChangedEventArgs eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            OnCollectionChanged(eventArgs);

            ItemPropertyChanged?.Invoke(sender, e);
        }
    }

}
