using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using Windows.UI.Xaml;

namespace BiliLite.Controls
{
    partial class CarouselPanel
    {
        private List<object> _items = new List<object>();

        #region Index
        public int Index
        {
            get { return (int)GetValue(IndexProperty); }
            set { SetValue(IndexProperty, value); }
        }

        private static void IndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CarouselPanel;
            control.InvalidateMeasure();
        }

        public static readonly DependencyProperty IndexProperty = DependencyProperty.Register("Index", typeof(int), typeof(CarouselPanel), new PropertyMetadata(0, IndexChanged));
        #endregion

        #region ItemsSource
        public object ItemsSource
        {
            get { return (object)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private static void ItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(e.NewValue is IEnumerable))
            {
                return;
            }

            var control = d as CarouselPanel;

            control.DetachNotificationEvents(e.OldValue as INotifyCollectionChanged);
            control.AttachNotificationEvents(e.NewValue as INotifyCollectionChanged);

            control.ItemsSourceChanged(e.NewValue as IEnumerable);
        }

        private void AttachNotificationEvents(INotifyCollectionChanged notifyCollection)
        {
            if (notifyCollection != null)
            {
                notifyCollection.CollectionChanged += OnCollectionChanged;
            }
        }

        private void DetachNotificationEvents(INotifyCollectionChanged notifyCollection)
        {
            if (notifyCollection != null)
            {
                notifyCollection.CollectionChanged -= OnCollectionChanged;
            }
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(object), typeof(CarouselPanel), new PropertyMetadata(null, ItemsSourceChanged));
        #endregion

        internal List<object> Items
        {
            get { return _items; }
        }

        internal int GetIndexOf(object content)
        {
            return _items.IndexOf(content);
        }

        private void ItemsSourceChanged(IEnumerable items)
        {
            _items.Clear();

            if (items != null)
            {
                foreach (var item in items)
                {
                    _items.Add(item);
                }
            }

            base.Children.Clear();
            this.InvalidateMeasure();
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    ClearChildren();
                    break;
                case NotifyCollectionChangedAction.Add:
                    int index = e.NewStartingIndex;
                    foreach (var item in e.NewItems)
                    {
                        AddItem(item, index++);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        RemoveItem(item);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                default:
                    break;
            }
            this.InvalidateMeasure();
        }

        private void ClearChildren()
        {
            _items.Clear();
            base.Children.Clear();
        }

        private void AddItem(object item, int index = -1)
        {
            index = index < 0 ? _items.Count : index;
            _items.Insert(index, item);
        }

        private void RemoveItem(object item)
        {
            int index = _items.IndexOf(item);
            _items.RemoveAt(index);
        }
    }
}
