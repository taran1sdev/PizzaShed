using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Collections.Specialized;
using Microsoft.Xaml.Behaviors;
using System.Windows.Media;

namespace PizzaShed.Behaviours
{
    public  class AutoScrollBehaviour : Behavior<ListView>
    {
        private ScrollViewer? _scrollViewer;

        // Called after the behaviour is attached to an associated object
        protected override void OnAttached()
        {
            base.OnAttached();

            // Wait until the object is loaded before we try and find it's scroll viewer
            AssociatedObject.Loaded += ListView_Loaded;
        }

        protected override void OnDetaching()
        {
            // Remove event subscriptions to avoid memory leaks
            AssociatedObject.Loaded -= ListView_Loaded;

            if (AssociatedObject.ItemsSource is INotifyCollectionChanged notifyCollection)
            {
                notifyCollection.CollectionChanged -= ItemsSource_CollectionChanged;
            }
            base.OnDetaching();
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            // Find the scrollviewer
            _scrollViewer = AssociatedObject.FindDescendant<ScrollViewer>();

            if (_scrollViewer != null)
            {
                // If the associated object supports INotifyCollectionChanged then subscribe to it
                if (AssociatedObject.ItemsSource is INotifyCollectionChanged notifyCollection)
                {
                    notifyCollection.CollectionChanged += ItemsSource_CollectionChanged;
                }
            }
        }

        private void ItemsSource_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Check if an item has been added or removed
            if (
                e.Action == NotifyCollectionChangedAction.Add
                || e.Action == NotifyCollectionChangedAction.Remove
            )
            {

                // We can safely ignore the _scrollViewer may be null warning as we only subscribe after a not null check
                // Using Async dispatcher ensures we are on the UI thread and the UI has been updated
                _scrollViewer.Dispatcher.InvokeAsync(() =>
                {
                    _scrollViewer.ScrollToBottom();
                }, DispatcherPriority.Loaded);
            }
        }
    }

    // This helper class simplifies finding elements in the visual tree
    public static class VisualTreeHelperExtensions
    {
        // recursive function returns elements
        public static T? FindDescendant<T>(this DependencyObject d) where T : DependencyObject
        {
            if (d == null) return null;

            int childCount = VisualTreeHelper.GetChildrenCount(d);

            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(d, i);
                var result = child as T ?? FindDescendant<T>(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}
