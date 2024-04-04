using Get.Data.XACL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Get.Data.ModelLinker
{
//    class ItemsProccessor<T> : IDisposable
//    {

//        public readonly ObservableCollection<T> SourceObservableCollection;
//        public event Action<T> ItemsAdded;
//        public event Action<T> ItemsRemoved;
//        readonly List<T> TemporaryList;
//        public ItemsProccessor(ObservableCollection<T> source)
//        {
//#pragma warning disable IDE0028 // Simplify collection initialization, syntax sugar does not work
//            this.SourceObservableCollection = source;
//            source.CollectionChanged += CollectionChanged;
//            // We add the collection changed first before we bind to temporary list
//            // so that we still have access to old items once the collection
//            // resets
//            TemporaryList = new() { CollectionItemsBinding.Create(source) };
//#pragma warning restore IDE0028 // Simplify collection initialization
//        }

//        private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
//        {
//            var newindex = e.NewStartingIndex;
//            var oldindex = e.OldStartingIndex;
//            switch (e.Action)
//            {
//                case NotifyCollectionChangedAction.Add:
//                    var lp = SourceObservableCollection[newindex];
//                    ItemsAdded?.Invoke(lp);
//                    break;
//                case NotifyCollectionChangedAction.Remove:
//                    ItemsRemoved?.Invoke(SourceObservableCollection[oldindex]);
//                    break;
//                case NotifyCollectionChangedAction.Replace:
//                    {
//                        var oldItem = SourceObservableCollection[oldindex];
//                        var newItem = SourceObservableCollection[oldindex];
//                        ItemsAdded?.Invoke(newItem);
//                        ItemsRemoved?.Invoke(oldItem);
//                    }
//                    break;
//                case NotifyCollectionChangedAction.Reset:
//                    Reset();
//                    break;
//            }
//        }
//        public void Reset()
//        {

//            foreach (var item in TemporaryList)
//                ItemsRemoved?.Invoke(item);

//            foreach (var item in SourceObservableCollection)
//                ItemsAdded?.Invoke(item);
//        }
//        public void Dispose()
//        {
//            SourceObservableCollection.CollectionChanged -= CollectionChanged;
//            GC.SuppressFinalize(this);
//        }
//    }
}
